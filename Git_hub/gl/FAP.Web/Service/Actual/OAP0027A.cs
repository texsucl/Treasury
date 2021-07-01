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
/// 功能說明：抽票原因覆核
/// 初版作者：20200121 Mark
/// 修改歷程：20200121 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0027A : Common, IOAP0027A
    {

        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<OAP0027ViewModel> GetSearchData(OAP0027SearchModel searchModel,string userid)
        {
            List<OAP0027ViewModel> resultModel = new List<OAP0027ViewModel>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _exex_actions = new SysCodeDao().qryByType("AP", "EXEC_ACTION");
                var update_time_s = TypeTransfer.stringToADDateTimeN(searchModel.update_time_start);
                var update_time_e = TypeTransfer.stringToADDateTimeN(searchModel.update_time_end).DateToLatestTime();
                resultModel = db.FAP_CODE_HIS.AsNoTracking().Where(x => x.apply_status == "1") //狀態為=>表單申請
                    .Where(x => x.apply_datetime >= update_time_s, update_time_s != null)
                    .Where(x => x.apply_datetime <= update_time_e, update_time_e != null)
                    .Where(x => x.apply_id == searchModel.apply_id, !searchModel.apply_id.IsNullOrWhiteSpace())
                    .Where(x => x.reason_code == searchModel.reason_code, !searchModel.reason_code.IsNullOrWhiteSpace())
                    .AsEnumerable().Select(x => new OAP0027ViewModel()
                    {
                        exec_action = x.exec_action,
                        exec_action_value = _exex_actions.FirstOrDefault(y => y.CODE == x.exec_action)?.CODE_VALUE,
                        reason = x.reason,
                        reason_code = x.reason_code,
                        referral_dep = x.referral_dep,
                        aply_no = x.aply_no,
                        pk_id = x.pk_id,
                        update_id = x.apply_id,
                        update_datetime = TypeTransfer.dateTimeNToStringNT(x.apply_datetime),
                        review_flag = !(x.apply_id == userid)
                    }).ToList();
                var common = new Service.Actual.Common();
                var _fullDepName = common.getFullDepName(resultModel.Select(x => x.referral_dep).Distinct());
                var userMemo = GetMemoByUserId(resultModel.Select(x => x.update_id).Distinct(), true);
                foreach (var item in resultModel)
                {
                    item.referral_dep_name = _fullDepName.First(x => x.Item1 == item.referral_dep).Item2;
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
        public MSGReturnModel ApprovedData(IEnumerable<OAP0027ViewModel> apprDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var _AP_RESN = "AP_RESN"; //原因中文
            var _AP_RESN_DP = "AP_RESN_DP"; //指定轉交部門
            var _yy = (dtn.Year - 1911).ToString();
            var _mm = (dtn.Month).ToString();
            var _dd = (dtn.Day).ToString();
            var _date = $@"{(dtn.Year - 1911)}{dtn.ToString("MMdd")}";
            var _time = $@"{dtn.ToString("HHmmssff")}";
            var updateStatus = new List<string>() { "A", "U", "D" };
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var aply_nos = apprDatas.Where(x=> updateStatus.Contains(x.exec_action))
                    .Select(x => x.aply_no).ToList();
                if (db.FAP_CODE_HIS.AsNoTracking()
                    .Where(x => aply_nos.Contains(x.aply_no))
                    .Any(x => x.apply_status != "1")) //有任一筆資料不為 表單申請 
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
                        try
                        {
                            #region MyRegion
                            foreach (var item in db.FAP_CODE_HIS.Where(x => aply_nos.Contains(x.aply_no)))
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
                                        var cId = sysSeqDao.qrySeqNo("AP", "B3", qPreCode).ToString();
                                        var _pk_id = $@"B3{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                        db.FAP_CODE.Add(new FAP_CODE()
                                        {
                                            pk_id = _pk_id,
                                            reason = item.reason,
                                            reason_code = item.reason_code,
                                            referral_dep = item.referral_dep,
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
INSERT INTO LRTCODE1 (GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM, ENTRY_YY, ENTRY_MM, ENTRY_DD, ENTRY_TIME, ENTRY_ID, UPD_YY, UPD_MM, UPD_DD, UPD_ID, FILLER_08N, APPR_STAT, APPR_ID, APPR_DATE, APPR_TIMEN, APPRV_FLG) 
VALUES (:GROUP_ID, :TEXT_LEN, :REF_NO, :TEXT, :SRCE_FROM, :ENTRY_YY, :ENTRY_MM, :ENTRY_DD, :ENTRY_TIME, :ENTRY_ID, :UPD_YY, :UPD_MM, :UPD_DD, :UPD_ID, :FILLER_08N, :APPR_STAT, :APPR_ID, :APPR_DATE, :APPR_TIMEN, :APPRV_FLG) 
";
                                            com.Parameters.Add($@"GROUP_ID", $@"AP_RESN");
                                            com.Parameters.Add($@"TEXT_LEN", $@"60");
                                            com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                            com.Parameters.Add($@"TEXT", item.reason.strto400DB()); //原因中文
                                            com.Parameters.Add($@"SRCE_FROM", $@"AP");
                                            com.Parameters.Add($@"ENTRY_YY", _yy);
                                            com.Parameters.Add($@"ENTRY_MM", _mm);
                                            com.Parameters.Add($@"ENTRY_DD", _dd);
                                            com.Parameters.Add($@"ENTRY_TIME", _time);
                                            com.Parameters.Add($@"ENTRY_ID", item.apply_id.strto400DB());
                                            com.Parameters.Add($@"UPD_YY", _yy);
                                            com.Parameters.Add($@"UPD_MM", _mm);
                                            com.Parameters.Add($@"UPD_DD", _dd);
                                            com.Parameters.Add($@"UPD_ID", item.apply_id.strto400DB());
                                            com.Parameters.Add($@"FILLER_08N", $@"0");
                                            com.Parameters.Add($@"APPR_STAT", $@"Y");
                                            com.Parameters.Add($@"APPR_ID", item.appr_id.strto400DB());
                                            com.Parameters.Add($@"APPR_DATE", _date);
                                            com.Parameters.Add($@"APPR_TIMEN", _time);
                                            com.Parameters.Add($@"APPRV_FLG", @"Y");
                                            com.Transaction = transaction;
                                            com.CommandText = sql;
                                            com.Prepare();
                                            var updateNum = com.ExecuteNonQuery();
                                            com.Dispose();
                                        }
                                        if (!item.referral_dep.IsNullOrWhiteSpace())
                                        {
                                            using (EacCommand com = new EacCommand(conn))
                                            {
                                                sql = $@"
INSERT INTO LRTCODE1 (GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM, ENTRY_YY, ENTRY_MM, ENTRY_DD, ENTRY_TIME, ENTRY_ID, UPD_YY, UPD_MM, UPD_DD, UPD_ID, FILLER_08N, APPR_STAT, APPR_ID, APPR_DATE, APPR_TIMEN, APPRV_FLG) 
VALUES (:GROUP_ID, :TEXT_LEN, :REF_NO, :TEXT, :SRCE_FROM, :ENTRY_YY, :ENTRY_MM, :ENTRY_DD, :ENTRY_TIME, :ENTRY_ID, :UPD_YY, :UPD_MM, :UPD_DD, :UPD_ID, :FILLER_08N, :APPR_STAT, :APPR_ID, :APPR_DATE, :APPR_TIMEN, :APPRV_FLG) 
";
                                                com.Parameters.Add($@"GROUP_ID", $@"AP_RESN_DP");
                                                com.Parameters.Add($@"TEXT_LEN", $@"10");
                                                com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                                com.Parameters.Add($@"TEXT", item.referral_dep.strto400DB()); //指定轉交部門
                                                com.Parameters.Add($@"SRCE_FROM", $@"AP");
                                                com.Parameters.Add($@"ENTRY_YY", _yy);
                                                com.Parameters.Add($@"ENTRY_MM", _mm);
                                                com.Parameters.Add($@"ENTRY_DD", _dd);
                                                com.Parameters.Add($@"ENTRY_TIME", _time);
                                                com.Parameters.Add($@"ENTRY_ID", item.apply_id.strto400DB());
                                                com.Parameters.Add($@"UPD_YY", _yy);
                                                com.Parameters.Add($@"UPD_MM", _mm);
                                                com.Parameters.Add($@"UPD_DD", _dd);
                                                com.Parameters.Add($@"UPD_ID", item.apply_id.strto400DB());
                                                com.Parameters.Add($@"FILLER_08N", $@"0");
                                                com.Parameters.Add($@"APPR_STAT", $@"Y");
                                                com.Parameters.Add($@"APPR_ID", item.appr_id.strto400DB());
                                                com.Parameters.Add($@"APPR_DATE", _date);
                                                com.Parameters.Add($@"APPR_TIMEN", _time);
                                                com.Parameters.Add($@"APPRV_FLG", @"Y");
                                                com.Transaction = transaction;
                                                com.CommandText = sql;
                                                com.Prepare();
                                                var updateNum = com.ExecuteNonQuery();
                                                com.Dispose();
                                            }
                                        }
                                        break;
                                    case "U":
                                        var _FAP_CODE_U = db.FAP_CODE.First(x => x.pk_id == item.pk_id);
                                        _FAP_CODE_U.data_status = "1"; //可異動
                                        _FAP_CODE_U.reason = item.reason;
                                        _FAP_CODE_U.referral_dep = item.referral_dep;
                                        _FAP_CODE_U.update_id = item.apply_id;
                                        _FAP_CODE_U.update_datetime = item.apply_datetime;
                                        _FAP_CODE_U.appr_id = item.appr_id;
                                        _FAP_CODE_U.appr_datetime = item.appr_datetime;
                                        bool _haveFlag = false; //現型是否有資料 (有 => 修改 or 刪除,沒有 => 新增)
                                        using (EacCommand com = new EacCommand(conn))
                                        {
                                            sql = $@"
select REF_NO,GROUP_ID,SRCE_FROM from LRTCODE1 
where GROUP_ID = 'AP_RESN_DP'
and REF_NO = :REF_NO
and SRCE_FROM = 'AP'
";

                                            com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                            com.CommandText = sql;
                                            com.Prepare();
                                            DbDataReader dbresult = com.ExecuteReader();
                                            var par = string.Empty;
                                            while (dbresult.Read())
                                            {
                                                _haveFlag = true;
                                            }
                                            com.Dispose();
                                        }
                                        //DB有資料 指定部門代碼資料為空值 => 刪除
                                        if (_haveFlag && _FAP_CODE_U.referral_dep.IsNullOrWhiteSpace())
                                        {
                                            using (EacCommand com = new EacCommand(conn))
                                            {
                                                sql = $@"
delete LRTCODE1 
where GROUP_ID = 'AP_RESN_DP'
and REF_NO = :REF_NO
and SRCE_FROM = 'AP'
";
                                                com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                                com.Transaction = transaction;
                                                com.CommandText = sql;
                                                com.Prepare();
                                                var updateNum = com.ExecuteNonQuery();
                                                com.Dispose();
                                            }
                                        }
                                        //DB有資料 指定部門代碼資料不為空值 => 修改
                                        else if (_haveFlag && !_FAP_CODE_U.referral_dep.IsNullOrWhiteSpace())
                                        {
                                            using (EacCommand com = new EacCommand(conn))
                                            {
                                                sql = $@"
update LRTCODE1 
set TEXT = :TEXT,
UPD_YY = :UPD_YY,
UPD_MM = :UPD_MM,
UPD_DD = :UPD_DD,
UPD_ID = :UPD_ID,
APPR_ID = :APPR_ID,
APPR_DATE = :APPR_DATE,
APPR_TIMEN = :APPR_TIMEN
where GROUP_ID = 'AP_RESN_DP'
and REF_NO = :REF_NO
and SRCE_FROM = 'AP'
";

                                                com.Parameters.Add($@"TEXT", item.referral_dep.strto400DB()); //指定轉交部門
                                                com.Parameters.Add($@"UPD_YY", _yy);
                                                com.Parameters.Add($@"UPD_MM", _mm);
                                                com.Parameters.Add($@"UPD_DD", _dd);
                                                com.Parameters.Add($@"UPD_ID", item.apply_id.strto400DB());
                                                com.Parameters.Add($@"APPR_ID", item.appr_id.strto400DB());
                                                com.Parameters.Add($@"APPR_DATE", _date);
                                                com.Parameters.Add($@"APPR_TIMEN", _time);
                                                com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                                com.Transaction = transaction;
                                                com.CommandText = sql;
                                                com.Prepare();
                                                var updateNum = com.ExecuteNonQuery();
                                                com.Dispose();
                                            }
                                        }
                                        //DB無資料 指定部門代碼資料不為空值 => 新增
                                        else if (!_haveFlag && !_FAP_CODE_U.referral_dep.IsNullOrWhiteSpace())
                                        {
                                            using (EacCommand com = new EacCommand(conn))
                                            {
                                                sql = $@"
INSERT INTO LRTCODE1 (GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM, ENTRY_YY, ENTRY_MM, ENTRY_DD, ENTRY_TIME, ENTRY_ID, UPD_YY, UPD_MM, UPD_DD, UPD_ID, FILLER_08N, APPR_STAT, APPR_ID, APPR_DATE, APPR_TIMEN, APPRV_FLG) 
VALUES (:GROUP_ID, :TEXT_LEN, :REF_NO, :TEXT, :SRCE_FROM, :ENTRY_YY, :ENTRY_MM, :ENTRY_DD, :ENTRY_TIME, :ENTRY_ID, :UPD_YY, :UPD_MM, :UPD_DD, :UPD_ID, :FILLER_08N, :APPR_STAT, :APPR_ID, :APPR_DATE, :APPR_TIMEN, :APPRV_FLG) 
";
                                                com.Parameters.Add($@"GROUP_ID", $@"AP_RESN_DP");
                                                com.Parameters.Add($@"TEXT_LEN", $@"10");
                                                com.Parameters.Add($@"REF_NO", $@"{item.reason_code.strto400DB()}");
                                                com.Parameters.Add($@"TEXT", item.referral_dep.strto400DB()); //指定轉交部門
                                                com.Parameters.Add($@"SRCE_FROM", $@"AP");
                                                com.Parameters.Add($@"ENTRY_YY", _yy);
                                                com.Parameters.Add($@"ENTRY_MM", _mm);
                                                com.Parameters.Add($@"ENTRY_DD", _dd);
                                                com.Parameters.Add($@"ENTRY_TIME", _time);
                                                com.Parameters.Add($@"ENTRY_ID", item.apply_id.strto400DB());
                                                com.Parameters.Add($@"UPD_YY", _yy);
                                                com.Parameters.Add($@"UPD_MM", _mm);
                                                com.Parameters.Add($@"UPD_DD", _dd);
                                                com.Parameters.Add($@"UPD_ID", item.apply_id.strto400DB());
                                                com.Parameters.Add($@"FILLER_08N", $@"0");
                                                com.Parameters.Add($@"APPR_STAT", $@"Y");
                                                com.Parameters.Add($@"APPR_ID", item.appr_id.strto400DB());
                                                com.Parameters.Add($@"APPR_DATE", _date);
                                                com.Parameters.Add($@"APPR_TIMEN", _time);
                                                com.Parameters.Add($@"APPRV_FLG", @"Y");
                                                com.Transaction = transaction;
                                                com.CommandText = sql;
                                                com.Prepare();
                                                var updateNum = com.ExecuteNonQuery();
                                                com.Dispose();
                                            }
                                        }
                                        break;
                                    case "D":
                                        var _FAP_CODE_D = db.FAP_CODE.First(x => x.pk_id == item.pk_id);
                                        db.FAP_CODE.Remove(_FAP_CODE_D);
                                        using (EacCommand com = new EacCommand(conn))
                                        {
                                            sql = $@"
Delete LRTCODE1
where GROUP_ID in ('AP_RESN','AP_RESN_DP')
and SRCE_FROM = 'AP'
and REF_NO = :REF_NO 
";
                                            com.Parameters.Add($@"REF_NO", item.reason_code.strto400DB()); //原因代碼
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
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
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
        public MSGReturnModel RejectedData(IEnumerable<OAP0027ViewModel> rejDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var updateStatus = new List<string>() { "A", "U", "D" };
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var aply_nos = rejDatas.Where(x=> updateStatus.Contains(x.exec_action))
                    .Select(x => x.aply_no).ToList();
                if (db.FAP_CODE_HIS.AsNoTracking()
                    .Where(x => aply_nos.Contains(x.aply_no))
                    .Any(x => x.apply_status != "1")) //有任一筆資料不為 表單申請 
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    foreach (var item in db.FAP_CODE_HIS.Where(x => aply_nos.Contains(x.aply_no)))
                    {
                        item.appr_id = userId;
                        item.appr_datetime = dtn;
                        item.apply_status = "3"; //退回/駁回
                        switch (item.exec_action)
                        {
                            case "D":
                            case "U":
                                var _FAP_CODE = db.FAP_CODE.First(x => x.pk_id == item.pk_id);
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