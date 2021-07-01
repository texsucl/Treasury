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
using System.Transactions;

/// <summary>
/// 功能說明：抽票部門權限關聯覆核
/// 初版作者：20200120 Mark
/// 修改歷程：20200120 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0026A : Common, IOAP0026A
    {

        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<OAP0026ViewModel> GetSearchData(OAP0026SearchModel searchModel,string userid)
        {
            List<OAP0026ViewModel> resultModel = new List<OAP0026ViewModel>();
            var datas = new OAP0026().getData(); //查詢給付類型 & 中文
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _exex_actions = new SysCodeDao().qryByType("AP", "EXEC_ACTION");
                var update_time_s = TypeTransfer.stringToADDateTimeN(searchModel.update_time_start);
                var update_time_e = TypeTransfer.stringToADDateTimeN(searchModel.update_time_end).DateToLatestTime();
                resultModel = db.FAP_PAID_DEPARMENT_HIS.AsNoTracking().Where(x => x.apply_status == "1") //狀態為=>表單申請
                    .Where(x => x.apply_datetime >= update_time_s, update_time_s != null)
                    .Where(x => x.apply_datetime <= update_time_e, update_time_e != null)
                    .Where(x => x.apply_id == searchModel.apply_id, !searchModel.apply_id.IsNullOrWhiteSpace())
                    .Where(x => x.ap_paid == searchModel.ap_paid, !searchModel.ap_paid.IsNullOrWhiteSpace() && searchModel.ap_paid != "All")
                    .AsEnumerable().Select(x => new OAP0026ViewModel()
                    {
                        exec_action = x.exec_action,
                        exec_action_value = _exex_actions.FirstOrDefault(y => y.CODE == x.exec_action)?.CODE_VALUE,
                        unit_code = x.unit_code,
                        ap_paid = x.ap_paid,
                        ap_paid_value = datas.FirstOrDefault(y => y.Item1 == x.ap_paid)?.Item2,
                        aply_no = x.aply_no,
                        pk_id = x.pk_id,
                        update_id = x.apply_id,
                        update_datetime = TypeTransfer.dateTimeNToStringNT(x.apply_datetime),
                        review_flag = !(x.apply_id == userid)
                    }).ToList();
                var common = new Service.Actual.Common();
                var _fullDepName = common.getFullDepName(resultModel.Select(x => x.unit_code).Distinct());
                var userMemo = GetMemoByUserId(resultModel.Select(x => x.update_id).Distinct(), true);
                foreach (var item in resultModel)
                {
                    item.unit_code_value = _fullDepName.First(x => x.Item1 == item.unit_code).Item2;
                    item.update_id = $@"{item.update_id}({userMemo.FirstOrDefault(y => y.Item1 == item.update_id).Item2})";
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="apprDatas">待核可資料</param>
        /// <param name="userId">核可ID</param>
        /// <returns></returns>
        public MSGReturnModel ApprovedData(IEnumerable<OAP0026ViewModel> apprDatas,string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var updateStatus = new List<string>() { "A", "D" };
                var aply_nos = apprDatas.Where(x=> updateStatus.Contains(x.exec_action))
                    .Select(x => x.aply_no).ToList();
                if (db.FAP_PAID_DEPARMENT_HIS.AsNoTracking()
                    .Where(x => aply_nos.Contains(x.aply_no))
                    .Any(x=>x.apply_status != "1")) //有任一筆資料不為 表單申請 
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        EacTransaction transaction = conn.BeginTransaction();
                        string sql = string.Empty;
                        foreach (var item in db.FAP_PAID_DEPARMENT_HIS.Where(x => aply_nos.Contains(x.aply_no)))
                        {
                            item.appr_id = userId;
                            item.appr_datetime = dtn;
                            item.apply_status = "2"; //覆核完成
                            switch (item.exec_action)
                            {
                                case "A":
                                    SysSeqDao sysSeqDao = new SysSeqDao();
                                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                                    String qPreCode = curDateTime[0];
                                    var cId = sysSeqDao.qrySeqNo("AP", "B2", qPreCode).ToString();
                                    var _pk_id = $@"B2{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                    db.FAP_PAID_DEPARMENT.Add(new FAP_PAID_DEPARMENT()
                                    {
                                        pk_id = _pk_id,
                                        unit_code = item.unit_code,
                                        ap_paid = item.ap_paid,
                                        data_status = "1", //可異動
                                        create_id = item.apply_id,
                                        create_datetime = item.apply_datetime,
                                        update_id = item.apply_id,
                                        update_datetime = item.apply_datetime,
                                        appr_id = item.appr_id,
                                        appr_datetime = item.appr_datetime,
                                    });
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"
INSERT INTO LAPPYPU1 (UNIT_CODE, AP_PAID) 
VALUES (:UNIT_CODE, :AP_PAID) 
";
                                        com.Parameters.Add($@"UNIT_CODE", item.unit_code.strto400DB()); //部門代碼
                                        com.Parameters.Add($@"AP_PAID", item.ap_paid.strto400DB()); //給付類型
                                        com.Transaction = transaction;
                                        com.CommandText = sql;
                                        com.Prepare();
                                        var updateNum = com.ExecuteNonQuery();
                                        com.Dispose();
                                    }
                                    break;
                                case "D":
                                    var _FAP_PAID_DEPARMENT = db.FAP_PAID_DEPARMENT.First(x => x.pk_id == item.pk_id);
                                    db.FAP_PAID_DEPARMENT.Remove(_FAP_PAID_DEPARMENT);
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"
Delete LAPPYPU1
where UNIT_CODE = :UNIT_CODE
and AP_PAID = :AP_PAID 
";
                                        com.Parameters.Add($@"UNIT_CODE", item.unit_code.strto400DB()); //部門代碼
                                        com.Parameters.Add($@"AP_PAID", item.ap_paid.strto400DB()); //給付類型
                                        com.Transaction = transaction;
                                        com.CommandText = sql;
                                        com.Prepare();
                                        var updateNum = com.ExecuteNonQuery();
                                        com.Dispose();
                                    }
                                    break;
                            }
                        }
                        try
                        {
                            transaction.Commit();
                            db.SaveChanges();
                            resultModel.RETURN_FLAG = true;
                            resultModel.DESCRIPTION = MessageType.Audit_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            transaction.Rollback();
                            resultModel.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="rejDatas">待駁回資料</param>
        /// <param name="userId">駁回Id</param>
        /// <returns></returns>
        public MSGReturnModel RejectedData(IEnumerable<OAP0026ViewModel> rejDatas,string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var aply_nos = rejDatas.Select(x => x.aply_no).ToList();
                if (db.FAP_PAID_DEPARMENT_HIS.AsNoTracking()
                    .Where(x => aply_nos.Contains(x.aply_no))
                    .Any(x => x.apply_status != "1")) //有任一筆資料不為 表單申請 
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    foreach (var item in db.FAP_PAID_DEPARMENT_HIS.Where(x => aply_nos.Contains(x.aply_no)))
                    {
                        item.appr_id = userId;
                        item.appr_datetime = dtn;
                        item.apply_status = "3"; //退回/駁回
                        switch (item.exec_action)
                        {
                            case "D":
                                var _FAP_PAID_DEPARMENT = db.FAP_PAID_DEPARMENT.First(x => x.pk_id == item.pk_id);
                                _FAP_PAID_DEPARMENT.data_status = "1";
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