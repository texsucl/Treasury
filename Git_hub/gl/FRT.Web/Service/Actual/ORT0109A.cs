using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;
using static FRT.Web.Service.Actual.ORT0109;

/// <summary>
/// 功能說明：跨系統資料庫勾稽銀存銷帳不比對帳號覆核
/// 初版作者：20210518 Mark
/// 修改歷程：20210518 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0109A : Common , IORT0109A
    {
        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public MSGReturnModel<List<ORT0109ViewModel>> GetSearchData(string userid)
        {
            MSGReturnModel<List<ORT0109ViewModel>> resultModel = new MSGReturnModel<List<ORT0109ViewModel>>();
            List<ORT0109ViewModel> datas = new List<ORT0109ViewModel>();
            List<BANK_ACCT_NOs> _nos = new List<BANK_ACCT_NOs>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                datas = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.AsNoTracking()
                    .Where(x => x.apply_status == "1").AsEnumerable() // 狀態為=>表單申請
                    .Select(x => new ORT0109ViewModel()
                    {
                        exec_action = x.exec_action, //執行功能
                        bank_acct_no = x.bank_acct_no, //帳號
                        update_id = x.apply_id, 
                        update_datetime = x.apply_datetime.ToString("yyyy/MM/dd"),
                        his_pk_id = x.his_pk_id, //跨系統勾稽作業異動檔_pk_id
                        pk_id = x.pk_id, //跨系統勾稽作業 pk_id
                        review_flag = !(x.apply_id == userid)
                    }).ToList();
                List<SYS_CODE> sys_codes = new List<SYS_CODE>();
                var emplys = new List<V_EMPLY2>();
                if (datas.Any())
                {
                    using (DB_INTRAEntities db_intra = new DB_INTRAEntities())
                    {
                        emplys = db_intra.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
                    }
                    sys_codes = GetSysCodes("RT", new List<string>() { "STATUS", "DATA_STATUS" });
                    _nos = new ORT0109().getBANK_ACCT_NOs();
                }
                foreach (var item in datas)
                {
                    //資料狀態(中文)
                    item.data_status_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "DATA_STATUS" && x.CODE == item.data_status)?.CODE_VALUE ?? item.data_status;
                    //執行功能(中文)
                    item.exec_action_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == item.exec_action)?.CODE_VALUE ?? item.exec_action;
                    //修改人員(名子)
                    item.update_name = (emplys.FirstOrDefault(x => x.USR_ID == item.update_id)?.EMP_NAME?.Trim()) ?? item.update_id;
                    //銀行簡稱
                    item.bank_acct_make_out = (_nos.FirstOrDefault(x => x.BANK_ACCT_NO == item.bank_acct_no)?.BANK_ACCT_MAKE_OUT?.Trim()) ?? string.Empty;
                }
                resultModel.RETURN_FLAG = true;
                resultModel.Datas = datas;
            }
            return resultModel;
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="apprDatas">待核可資料</param>
        /// <param name="userId">核可ID</param>
        /// <returns></returns>
        public MSGReturnModel ApprovedData(IEnumerable<ORT0109ViewModel> apprDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;

            var updateStatus = new List<string>() { "A", "U", "D" };
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his_pk_ids = apprDatas.Select(x => x.his_pk_id).ToList();
                    if (db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.AsNoTracking()
                          .Where(x => his_pk_ids.Contains(x.his_pk_id))
                          .Any(x => x.apply_status != "1")) //有任一筆資料不為 表單申請 
                    {
                        resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                    }
                    else
                    {
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        String qPreCode = dtn.ToString("yyyyMMdd");
                        foreach (var item in db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS
                            .Where(x => his_pk_ids.Contains(x.his_pk_id)))
                        {
                            item.appr_id = userId;
                            item.appr_datetime = dtn;
                            item.apply_status = "2";
                            var _pk_id = string.Empty;

                            switch (item.exec_action)
                            {
                                case "A":
                                    var _B0105 = sysSeqDao.qrySeqNo("RT", "B0109", qPreCode).ToString();
                                    _pk_id = $@"B0109{qPreCode}{_B0105.ToString().PadLeft(5, '0')}";
                                    db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.Add(new FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO()
                                    {
                                        pk_id = _pk_id, //pk_id
                                        bank_acct_no = item.bank_acct_no, //帳號
                                        data_status = "1", //可異動
                                        create_id = item.apply_id,
                                        create_datetime = item.apply_datetime,
                                        update_id = item.apply_id,
                                        update_datetime = item.apply_datetime,
                                        appr_id = userId,
                                        appr_datetime = dtn,                                     
                                    });
                                    break;
                                case "U":
                                    var _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.Single(x => x.pk_id == item.pk_id);
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.bank_acct_no = item.bank_acct_no; //帳號
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.data_status = "1"; //可異動
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.appr_id = userId;
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.appr_datetime = dtn;
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.update_id = item.apply_id;
                                    _FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.update_datetime = item.apply_datetime;
                                    break;
                                case "D":
                                    db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.Remove(
                                        db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.Single(x => x.pk_id == item.pk_id));
                                    break;
                            }
                        }
                        try
                        {
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                resultModel.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                db.SaveChanges();
                                resultModel.RETURN_FLAG = true;
                                resultModel.DESCRIPTION = MessageType.Audit_Success.GetDescription();
                            }
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            resultModel.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
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
        /// 駁回
        /// </summary>
        /// <param name="rejDatas">待駁回資料</param>
        /// <param name="userId">駁回Id</param>
        /// <returns></returns>
        public MSGReturnModel RejectedData(IEnumerable<ORT0109ViewModel> rejDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var his_pk_ids = rejDatas.Select(x => x.his_pk_id).ToList();
                if (db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.AsNoTracking()
                    .Where(x => his_pk_ids.Contains(x.his_pk_id)).Any(x => x.apply_status != "1"))  //有任一筆資料不為 表單申請 
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    foreach (var item in db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO_HIS.Where(x => his_pk_ids.Contains(x.his_pk_id)))
                    {
                        item.appr_id = userId;
                        item.appr_datetime = dtn;
                        item.apply_status = "3"; //退回/駁回
                        switch (item.exec_action)
                        {
                            case "D":
                            case "U":
                                var _FAP_CODE = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.First(x => x.pk_id == item.pk_id);
                                _FAP_CODE.data_status = "1";
                                break;
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                        resultModel.RETURN_FLAG = true;
                        resultModel.DESCRIPTION = MessageType.Reject_Success.GetDescription();
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                        resultModel.DESCRIPTION = MessageType.Reject_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
            }
            return resultModel;
        }
    }
}