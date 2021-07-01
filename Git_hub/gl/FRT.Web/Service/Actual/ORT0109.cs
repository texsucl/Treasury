using Dapper;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：跨系統資料庫勾稽銀存銷帳不比對帳號
/// 初版作者：20210517 Mark
/// 修改歷程：20210517 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{

    public class ORT0109 : Common , IORT0109
    {
        /// <summary>
        /// 查詢 跨系統勾稽作業_批次定義
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel<List<ORT0109ViewModel>> GetSearchData(string bank_acct_no = null)
        {
            MSGReturnModel<List<ORT0109ViewModel>> results = new MSGReturnModel<List<ORT0109ViewModel>>();
            List<ORT0109ViewModel> datas = new List<ORT0109ViewModel>();
            List<BANK_ACCT_NOs> _nos = new List<BANK_ACCT_NOs>();
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    datas = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.AsNoTracking()
                        .Where(x => x.bank_acct_no == bank_acct_no, !bank_acct_no.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select(x => new ORT0109ViewModel()
                        {
                            bank_acct_no = x.bank_acct_no, //帳號
                            data_status = x.data_status, //資料狀態代碼
                            update_time_compare = x.update_datetime, //最後異動時間
                            pk_id = x.pk_id, //跨系統勾稽作業 pk_id
                            create_id = x.create_id,
                            create_datetime = x.create_datetime.ToString("yyyy/MM/dd"),
                            update_id = x.update_id,
                            update_datetime = x.update_datetime.ToString("yyyy/MM/dd"),
                            appr_id = x.appr_id,
                            appr_datetime = x.appr_datetime.ToString("yyyy/MM/dd")
                        }).ToList();
                    List<SYS_CODE> sys_codes = new List<SYS_CODE>();
                    var emplys = new List<V_EMPLY2>();
                    if (datas.Any())
                    {
                        using (DB_INTRAEntities db_intra = new DB_INTRAEntities())
                        {
                            emplys = db_intra.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
                        }
                        sys_codes = GetSysCodes("RT", new List<string>() { "DATA_STATUS" });
                        _nos = getBANK_ACCT_NOs();
                    }
                    foreach (var item in datas)
                    {
                        //資料狀態(中文)
                        item.data_status_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "DATA_STATUS" && x.CODE == item.data_status)?.CODE_VALUE ?? item.data_status;
                        //建立人員(名子)
                        item.create_name = (emplys.FirstOrDefault(x => x.USR_ID == item.create_id)?.EMP_NAME?.Trim()) ?? item.create_id;
                        //修改人員(名子)
                        item.update_name = (emplys.FirstOrDefault(x => x.USR_ID == item.update_id)?.EMP_NAME?.Trim()) ?? item.update_id;
                        //覆核人員(名子)
                        item.appr_name = (emplys.FirstOrDefault(x => x.USR_ID == item.appr_id)?.EMP_NAME?.Trim()) ?? item.appr_id;
                        //銀行簡稱
                        item.bank_acct_make_out = (_nos.FirstOrDefault(x => x.BANK_ACCT_NO == item.bank_acct_no)?.BANK_ACCT_MAKE_OUT?.Trim()) ?? string.Empty;
                    }
                    results.RETURN_FLAG = true;
                    results.Datas = datas;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                results.DESCRIPTION = ex.Message;
            }
            return results;
        }

        /// <summary>
        /// 申請 跨系統勾稽作業_批次定義
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel ApplyDeptData(IEnumerable<ORT0109ViewModel> updateDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var updateStatus = new List<string>() { "A", "U", "D" };
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _ApplyDeptData = updateDatas.Where(x => updateStatus.Contains(x.exec_action)).ToList();
                    if (!_ApplyDeptData.Any())
                    {
                        resultModel.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                        return resultModel;
                    }
                    bool _sameFlag = false;
                    _ApplyDeptData.ForEach(x =>
                    {
                        var _CheckSameData = CheckData(x);
                        if (_CheckSameData.Item1)
                        {
                            resultModel.DESCRIPTION = _CheckSameData.Item2;
                            _sameFlag = true;
                        }

                    });
                    if (_sameFlag)
                        return resultModel;
                    var check_updates = _ApplyDeptData.Where(x => x.exec_action != "A").ToList();
                    if (check_updates.Any())
                    {
                        foreach (var check in check_updates)
                        {
                            var _check = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO
                                .AsNoTracking().First(x => x.pk_id == check.pk_id);
                            if ((_check.data_status == "2") || (_check.update_datetime > check.update_time_compare))
                            {
                                resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                                return resultModel;
                            }
                        }
                    }
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    String qPreCode = dtn.ToString("yyyyMMdd");
                    foreach (var item in _ApplyDeptData)
                    {
                        var _S0109 = sysSeqDao.qrySeqNo("RT", "S0109", qPreCode).ToString();
                        var _his_pk_id = $@"S0109{qPreCode}{_S0109.ToString().PadLeft(5, '0')}";

                        #region 主檔
                        var _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS = new FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS()
                        {
                            bank_acct_no = item.bank_acct_no, //帳號
                            apply_id = userId, //申請人員
                            apply_datetime = dtn, //申請時間
                            his_pk_id = _his_pk_id, //跨系統勾稽作業異動檔_pk_id
                            exec_action = item.exec_action, //A or D or U
                            apply_status = "1", //表單申請
                        };
                        if (item.exec_action == "D" || item.exec_action == "U")
                        {
                            _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.pk_id = item.pk_id;
                            var _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO
                                .First(x => x.pk_id == item.pk_id);
                            _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.data_status = "2"; //異動中
                        }
                        db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.Add(_FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS);
                        #endregion
                    }
                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (validateMessage.Any())
                    {
                        resultModel.DESCRIPTION = validateMessage;
                    }
                    else {
                        try
                        {
                            db.SaveChanges();
                            resultModel.RETURN_FLAG = true;
                            resultModel.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            resultModel.DESCRIPTION = MessageType.Apply_Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                resultModel.DESCRIPTION = ex.Message;
            }


            return resultModel;
        }

        /// <summary>
        /// 檢核重覆資料 & 是否有存在Wanpie帳號基本資料輸入(UUU050201017771QS)檔案
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Tuple<bool,string,string> CheckData(ORT0109ViewModel viewModel)
        {
            bool flag = false;
            string result = string.Empty;
            string _BANK_ACCT_MAKE_OUT = string.Empty;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                flag = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.AsNoTracking()
                    .Any(x => x.bank_acct_no == viewModel.bank_acct_no &&
                    x.apply_status == "1"); //有相同 原因代碼  的資料在申請中

                if (!flag && (viewModel.exec_action == "A"))
                    flag = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.AsNoTracking()
                        .Any(x => x.bank_acct_no == viewModel.bank_acct_no);  //現行資料已有相同的 原因代碼

                if (flag)
                {
                    result = $@"欲新增的資料,已有相同'帳號:{viewModel.bank_acct_no}'存在現行的資料或申請的資料!";
                }
                else
                {
                    var _no = getBANK_ACCT_NOs().FirstOrDefault(x => x.BANK_ACCT_NO == viewModel.bank_acct_no);
                    if (_no == null)
                    {
                        flag = true;
                        result = $@"該帳號:{viewModel.bank_acct_no} 不存在基本資料輸入(UUU050201017771QS)檔案 請重新輸入!";
                    }
                    else
                    {
                        _BANK_ACCT_MAKE_OUT = _no.BANK_ACCT_MAKE_OUT;
                    }
                }
            }
            return new Tuple<bool, string, string >(flag,result, _BANK_ACCT_MAKE_OUT);
        }

        public List<BANK_ACCT_NOs> getBANK_ACCT_NOs()
        {
            List<BANK_ACCT_NOs> result = new List<BANK_ACCT_NOs>();
            using (SqlConnection conn = new SqlConnection(CommonUtil.GetGLSIACTConn()))
            {
                conn.Open();
                string sql = $@"
select M.BANK_ACCT_NO,
(select top 1 Code7.BANK_ABBR 
from GLSIACT..UUU05010101 Code7 WITH(NOLOCK) 
where Code7.CORP_NO = M.CORP_NO 
and Code7.BANK_NO=M.BANK_NO ) AS BANK_ACCT_MAKE_OUT
from GLSIACT..UUU05020101 M with(nolock)
where M.corp_no in ('FUBONLIFE','OIU')
and M.STOP_USE_DATE is null
";
                result = conn.Query<BANK_ACCT_NOs>(sql, null, null, true, 3600).ToList();
            }
            return result;
        }

        public class BANK_ACCT_NOs
        {
            /// <summary>
            /// 帳號
            /// </summary>
            public string BANK_ACCT_NO { get; set; }

            /// <summary>
            /// 銀行代碼
            /// </summary>
            public string BANK_ACCT_MAKE_OUT { get; set; }
        }
    }
}