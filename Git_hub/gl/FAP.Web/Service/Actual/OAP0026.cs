using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Utilitys;
using FAP.Web.Daos;
using FAP.Web.Models;
using System.Web.Mvc;

/// <summary>
/// 功能說明：抽票部門權限關聯維護
/// 初版作者：20200120 Mark
/// 修改歷程：20200120 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0026 : Common, IOAP0026
    {
        /// <summary>
        /// 查詢抽票部門權限關聯維護
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public List<OAP0026ViewModel> GetSearchData(OAP0026SearchModel searchModel)
        {
            List<OAP0026ViewModel> resultModel = new List<OAP0026ViewModel>();
            var _data_status = new SysCodeDao().qryByType("AP", "DATA_STATUS"); //資料狀態
            var datas = getData(); //查詢給付類型 & 中文
            using (dbFGLEntities db = new dbFGLEntities())
            {
                resultModel = db.FAP_PAID_DEPARMENT.AsNoTracking()
                    .Where(x => x.unit_code == searchModel.unit_code, !searchModel.unit_code.IsNullOrWhiteSpace())
                    .Where(x => x.ap_paid == searchModel.ap_paid, !searchModel.ap_paid.IsNullOrWhiteSpace() && searchModel.ap_paid != "All")
                    .AsEnumerable()
                    .Select(x => new OAP0026ViewModel()
                    {
                        pk_id = x.pk_id,
                        data_status = x.data_status,
                        data_status_value = _data_status.FirstOrDefault(y => y.CODE == x.data_status)?.CODE_VALUE,
                        ap_paid = x.ap_paid,
                        ap_paid_value = datas.FirstOrDefault(y => y.Item1 == x.ap_paid)?.Item2,
                        unit_code = x.unit_code,
                        create_id = x.create_id,
                        create_datetime = TypeTransfer.dateTimeNToStringNT(x.create_datetime),
                        update_id = x.update_id,
                        update_datetime = TypeTransfer.dateTimeNToStringNT(x.update_datetime),
                        appr_id = x.appr_id,
                        appr_datetime = TypeTransfer.dateTimeNToStringNT(x.appr_datetime)
                    }).ToList();
                var common = new Service.Actual.Common();
                var _fullDepName = common.getFullDepName(resultModel.Select(x => x.unit_code).Distinct());
                var _allUserId = resultModel.Select(x => x.create_id).ToList();
                _allUserId.AddRange(resultModel.Select(x => x.update_id));
                _allUserId.AddRange(resultModel.Select(x => x.appr_id));
                var userMemo = GetMemoByUserId(_allUserId.Distinct(), true);
                foreach (var item in resultModel)
                {
                    item.unit_code_value = _fullDepName.First(x => x.Item1 == item.unit_code).Item2;
                    item.create_id = $@"{item.create_id}({userMemo.FirstOrDefault(y => y.Item1 == item.create_id).Item2})";
                    item.update_id = $@"{item.update_id}({userMemo.FirstOrDefault(y => y.Item1 == item.update_id).Item2})";
                    item.appr_id = $@"{item.appr_id}({userMemo.FirstOrDefault(y => y.Item1 == item.appr_id).Item2})";
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="updateDatas">申請資料</param>
        /// <param name="userId">申請人員ID</param>
        /// <returns></returns>
        public MSGReturnModel ApplyDeptData(IEnumerable<OAP0026ViewModel> updateDatas,string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            bool changFlag = false;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var updateStatus = new List<string>() { "A", "D" };
                if (!updateDatas.Any(x => updateStatus.Contains(x.exec_action)))
                {
                    resultModel.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                    return resultModel;
                }
                var _pk_id = updateDatas.Where(x => updateStatus.Contains(x.exec_action)).Select(x => x.pk_id).ToList();
                if (db.FAP_PAID_DEPARMENT_HIS.AsNoTracking()
                    .Where(x => _pk_id.Contains(x.pk_id))
                    .Any(x => x.apply_status == "1")) //異動檔裡面有在申請中的
                {                 
                    changFlag = true;
                }
                if (changFlag)
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    DateTime dtn = DateTime.Now;
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                    String qPreCode = curDateTime[0];
                    foreach (var item in updateDatas)
                    {
                        var cId = sysSeqDao.qrySeqNo("AP", "S2", qPreCode).ToString();
                        var _aplyNo = $@"S2{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                        db.FAP_PAID_DEPARMENT_HIS.Add(new FAP_PAID_DEPARMENT_HIS()
                        {
                            aply_no = _aplyNo,
                            pk_id = item.exec_action == "D" ? item.pk_id : string.Empty,
                            exec_action = item.exec_action, //A or D
                            apply_status = "1", //表單申請
                            ap_paid = item.ap_paid,
                            unit_code = item.unit_code,
                            apply_id = userId,
                            apply_datetime = dtn
                        });
                        if (item.exec_action == "D")
                        {
                            var _FAP_PAID_DEPARMENT = db.FAP_PAID_DEPARMENT
                                .First(x => x.pk_id == item.pk_id);
                            _FAP_PAID_DEPARMENT.data_status = "2"; //異動中
                        }
                    }
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
            return resultModel;
        }

        /// <summary>
        /// 檢核重覆資料 (新增時檢查)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>true為有相同的資料,false為沒有相同的資料</returns>
        public bool CheckSameData(OAP0026ViewModel viewModel)
        {
            bool sameFlag = false;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                sameFlag = db.FAP_PAID_DEPARMENT_HIS.AsNoTracking()
                    .Any(x => x.ap_paid == viewModel.ap_paid &&
                    x.unit_code == viewModel.unit_code &&
                    x.apply_status == "1"); //有相同 部門代碼 & 給付類型 的資料在申請中

                if (!sameFlag && (viewModel.exec_action == "A"))
                    sameFlag = db.FAP_PAID_DEPARMENT.AsNoTracking()
                        .Any(x => x.ap_paid == viewModel.ap_paid &&
                        x.unit_code == viewModel.unit_code);  //現行資料已有相同的 部門代碼 & 給付類型
            }
            return sameFlag;
        }

        /// <summary>
        /// 查詢給付類型 & 中文
        /// </summary>
        /// <returns>1.給付類型 2.中文</returns>
        public List<Tuple<string, string>> getData(bool valueFlag = false)
        {
            List<Tuple<string, string>> ref_nos = new List<Tuple<string, string>>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select REF_NO,TEXT from LPMCODE1
where group_id = 'AP_PAID'
and srce_from = 'AP' 
order by REF_NO ; ";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    var par = string.Empty;
                    while (dbresult.Read())
                    {
                        par = valueFlag ? $@"{dbresult["REF_NO"]?.ToString()?.Trim()} : " : string.Empty;
                        ref_nos.Add(new Tuple<string, string>(
                            dbresult["REF_NO"]?.ToString()?.Trim(),
                            $@"{par}{dbresult["TEXT"]?.ToString()?.Trim()}"
                            ));
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }
            return ref_nos;
        }
    }
}