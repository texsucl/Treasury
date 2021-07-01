using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FAP.Web.ViewModels;
using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;
using FAP.Web.Daos;
using System.Data.Entity.Infrastructure;
using System.Data.EasycomClient;
using System.Transactions;

namespace FAP.Web.Service.Actual
{
    public class OAP0018A : Common, IOAP0018A
    {
        public Utility.MSGReturnModel<List<OAP0018AViewModel>> ApprovedData(OAP0018ASearchViewModel searchData, List<OAP0018AViewModel> viewModels)
        {
            var result = new MSGReturnModel<List<OAP0018AViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.Audit_Fail.GetDescription();
            if (!viewModels.Any())
            {
                return result;
            }

            DateTime now = DateTime.Now;
            string logStr = string.Empty;

            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (dbFGLEntities db = new dbFGLEntities())
                        {
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            var aplyNos = viewModels.Select(x => x.aply_no).ToList();
                            //using (EacCommand com = new EacCommand(conn))
                            //{
                                foreach (var aplyNo in aplyNos)
                                {
                                    var dbFCD_HIS = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking().Where(x => x.aply_no == aplyNo).ToList();
                                    if (dbFCD_HIS.Any())
                                    {
                                        foreach (var row in dbFCD_HIS)
                                        {
                                            string pk_id = string.Empty;

                                            //                                            if(row.exec_action == "U" || row.exec_action == "D")
                                            //                                            {
                                            //                                                var dbFCD_DELE = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);
                                            //                                                if (dbFCD_DELE != null)
                                            //                                                {
                                            //                                                    string sql_D = $@"
                                            //Delete LAPPYSU1
                                            //where FUN_ID = :FUN_ID_B and APPR_UNIT = :APPR_UNIT_B and USER_UNIT = :USER_UNIT_B
                                            //";
                                            //                                                    com.Transaction = transaction;
                                            //                                                    com.Parameters.Add("FUN_ID_B", dbFCD_DELE.fun_id);
                                            //                                                    com.Parameters.Add("APPR_UNIT_B", dbFCD_DELE.appr_unit);
                                            //                                                    com.Parameters.Add("USER_UNIT_B", dbFCD_DELE.user_unit);
                                            //                                                    //com.Parameters.Add("MEMO_B", dbFCD_DELE.memo);
                                            //                                                    com.CommandText = sql_D;
                                            //                                                    com.Prepare();
                                            //                                                    var DeleteNum = com.ExecuteNonQuery();
                                            //                                                    if (DeleteNum != 0)
                                            //                                                    {

                                            //                                                        if (row.exec_action == "U")
                                            //                                                        {
                                            //                                                            dbFCD_DELE.data_status = "1";
                                            //                                                            dbFCD_DELE.fun_id = row.fun_id_before;
                                            //                                                            dbFCD_DELE.appr_unit = row.appr_unit_before;
                                            //                                                            dbFCD_DELE.user_unit = row.user_unit_before;
                                            //                                                            dbFCD_DELE.memo = row.memo_before;

                                            //                                                            dbFCD_DELE.appr_id = searchData.current_uid;
                                            //                                                            dbFCD_DELE.appr_datetime = now;
                                            ////                                                         //logStr += dbRCR.modelToString(logStr);
                                            //                                                        }
                                            //                                                        else
                                            //                                                        {
                                            //                                                            db.FAP_CROSS_DEPARMENT.Remove(dbFCD_DELE);
                                            //                                                        }
                                            //                                                    }
                                            //                                                    else
                                            //                                                    {
                                            //                                                        transaction.Rollback();
                                            //                                                        result.RETURN_FLAG = false;
                                            //                                                        result.DESCRIPTION = "無更新資料!";
                                            //                                                    }
                                            //                                                }
                                            //                                            }
                                            //                                            else if(row.exec_action == "U" || row.exec_action == "A")
                                            //                                            {
                                            //                                                string sql = $@"
                                            //insert into LAPPYSU1 
                                            //(FUN_ID, APPR_UNIT, USER_UNIT, MEMO) 
                                            //VALUES(
                                            //:FUN_ID,
                                            //:APPR_UNIT,
                                            //:USER_UNIT,
                                            //:MEMO
                                            //)";
                                            //                                                com.Transaction = transaction;
                                            //                                                com.Parameters.Add("FUN_ID", row.fun_id);
                                            //                                                com.Parameters.Add("APPR_UNIT", row.appr_unit);
                                            //                                                com.Parameters.Add("USER_UNIT", row.user_unit);
                                            //                                                com.Parameters.Add("MEMO", row.memo);
                                            //                                                com.CommandText = sql;
                                            //                                                com.Prepare();
                                            //                                                var InsertNum = com.ExecuteNonQuery();
                                            //                                                if (InsertNum != 0)
                                            //                                                {
                                            //                                                    var twCalendar = new System.Globalization.TaiwanCalendar();
                                            //                                                    string qPreCode = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0'));
                                            //                                                    var cId = sysSeqDao.qrySeqNo("AP", "B1", qPreCode).ToString().PadLeft(3, '0');
                                            //                                                    pk_id = $@"B1{qPreCode}{cId}";//B1 + 系統日期YYYMMDD(民國年) + 3碼流水號

                                            //                                                    var FCD = new FAP_CROSS_DEPARMENT()
                                            //                                                    {
                                            //                                                        pk_id = pk_id,
                                            //                                                        fun_id = row.fun_id,
                                            //                                                        appr_unit = row.appr_unit,
                                            //                                                        user_unit = row.user_unit,
                                            //                                                        memo = row.memo,

                                            //                                                        data_status = "1", //可異動
                                            //                                                        create_id = row.apply_id,
                                            //                                                        create_datetime = now,
                                            //                                                        appr_id = searchData.current_uid,
                                            //                                                        appr_datetime = now,
                                            //                                                        update_id = row.apply_id,
                                            //                                                        update_datetime = now
                                            //                                                    };
                                            //                                                    //logStr += RCR.modelToString(logStr);
                                            //                                                    db.FAP_CROSS_DEPARMENT.Add(FCD);

                                            //                                                }
                                            //                                                else
                                            //                                                {
                                            //                                                    transaction.Rollback();
                                            //                                                    result.RETURN_FLAG = false;
                                            //                                                    result.DESCRIPTION = "無更新資料!";
                                            //                                                }
                                            //                                            }
                                            //                                            else
                                            //                                            {
                                            //                                                return result;
                                            //                                            }
                                            ///////////////////////////////////////////////////////////////////
                                            switch (row.exec_action)
                                            {
                                                case "A"://新增
                                                using (EacCommand com = new EacCommand(conn))
                                                {
                                                    string sql = $@"
insert into LAPPYSU1 
(FUN_ID, APPR_UNIT, USER_UNIT, MEMO) 
VALUES(
:FUN_ID,
:APPR_UNIT,
:USER_UNIT,
:MEMO
)";
                                                    com.Transaction = transaction;
                                                    com.Parameters.Add("FUN_ID", row.fun_id);
                                                    com.Parameters.Add("APPR_UNIT", row.appr_unit);
                                                    com.Parameters.Add("USER_UNIT", row.user_unit);
                                                    com.Parameters.Add("MEMO", row.memo.IsNullOrWhiteSpace() ? "" : row.memo);
                                                    com.CommandText = sql;
                                                    com.Prepare();
                                                    var InsertNum = com.ExecuteNonQuery();
                                                    if (InsertNum != 0)
                                                    {
                                                        var twCalendar = new System.Globalization.TaiwanCalendar();
                                                        string qPreCode = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0'));
                                                        var cId = sysSeqDao.qrySeqNo("AP", "B1", qPreCode).ToString().PadLeft(3, '0');
                                                        pk_id = $@"B1{qPreCode}{cId}";//B1 + 系統日期YYYMMDD(民國年) + 3碼流水號

                                                        var FCD = new FAP_CROSS_DEPARMENT()
                                                        {
                                                            pk_id = pk_id,
                                                            fun_id = row.fun_id,
                                                            appr_unit = row.appr_unit,
                                                            user_unit = row.user_unit,
                                                            memo = row.memo,

                                                            data_status = "1", //可異動
                                                            create_id = row.apply_id,
                                                            create_datetime = now,
                                                            appr_id = searchData.current_uid,
                                                            appr_datetime = now,
                                                            update_id = row.apply_id,
                                                            update_datetime = now
                                                        };
                                                        //logStr += RCR.modelToString(logStr);
                                                        db.FAP_CROSS_DEPARMENT.Add(FCD);

                                                    }
                                                    else
                                                    {
                                                        //transaction.Rollback();
                                                        result.RETURN_FLAG = false;
                                                        result.DESCRIPTION = "無更新資料!";
                                                    }
                                                    com.Dispose();
                                                }  
                                                    break;
                                                case "U": //修改
                                                using (EacCommand com = new EacCommand(conn))
                                                {
                                                    var dbFCD = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);
                                                    if (dbFCD != null)
                                                    {
//                                                        string sql_U = $@"
//update LAPPYSU1
//set FUN_ID = :FUN_ID, APPR_UNIT = :APPR_UNIT, USER_UNIT = :USER_UNIT, MEMO = :MEMO
//where FUN_ID = :FUN_ID_B and APPR_UNIT = :APPR_UNIT_B and USER_UNIT = :USER_UNIT_B and MEMO = :MEMO_B
//";
                                                        string sql_U = $@"
update LAPPYSU1
set MEMO = :MEMO
where FUN_ID = :FUN_ID_B and APPR_UNIT = :APPR_UNIT_B and USER_UNIT = :USER_UNIT_B
";
                                                        com.Transaction = transaction;
                                                        //com.Parameters.Add("FUN_ID", row.fun_id);
                                                        //com.Parameters.Add("APPR_UNIT", row.appr_unit);
                                                        //com.Parameters.Add("USER_UNIT", row.user_unit);
                                                        com.Parameters.Add("MEMO", row.memo.IsNullOrWhiteSpace() ? "" : row.memo);
                                                        com.Parameters.Add("FUN_ID_B", dbFCD.fun_id);
                                                        com.Parameters.Add("APPR_UNIT_B", dbFCD.appr_unit);
                                                        com.Parameters.Add("USER_UNIT_B", dbFCD.user_unit);
                                                        //com.Parameters.Add("MEMO_B", dbFCD.memo);
                                                        com.CommandText = sql_U;
                                                        com.Prepare();
                                                        var UpdateNum = com.ExecuteNonQuery();
                                                        if (UpdateNum != 0)
                                                        {
                                                            dbFCD.data_status = "1";
                                                            dbFCD.fun_id = row.fun_id;
                                                            dbFCD.appr_unit = row.appr_unit;
                                                            dbFCD.user_unit = row.user_unit;
                                                            dbFCD.memo = row.memo;

                                                            dbFCD.appr_id = searchData.current_uid;
                                                            dbFCD.appr_datetime = now;
                                                            //logStr += dbRCR.modelToString(logStr);
                                                        }
                                                        else
                                                        {
                                                            //transaction.Rollback();
                                                            result.RETURN_FLAG = false;
                                                            result.DESCRIPTION = "無更新資料!";
                                                        } 
                                                    }
                                                    com.Dispose();
                                                }
                                                    break;
                                                case "D"://刪除
                                                using (EacCommand com = new EacCommand(conn))
                                                {
                                                    var dbFCD_DELE = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);
                                                    if (dbFCD_DELE != null)
                                                    {
                                                        string sql_D = $@"
Delete LAPPYSU1
where FUN_ID = :FUN_ID_B and APPR_UNIT = :APPR_UNIT_B and USER_UNIT = :USER_UNIT_B
";
                                                        com.Transaction = transaction;
                                                        com.Parameters.Add("FUN_ID_B", dbFCD_DELE.fun_id);
                                                        com.Parameters.Add("APPR_UNIT_B", dbFCD_DELE.appr_unit);
                                                        com.Parameters.Add("USER_UNIT_B", dbFCD_DELE.user_unit);
                                                        //com.Parameters.Add("MEMO_B", dbFCD_DELE.memo);
                                                        com.CommandText = sql_D;
                                                        com.Prepare();
                                                        var DeleteNum = com.ExecuteNonQuery();
                                                        if (DeleteNum != 0)
                                                        {
                                                            db.FAP_CROSS_DEPARMENT.Remove(dbFCD_DELE);
                                                        }
                                                        else
                                                        {
                                                            //transaction.Rollback();
                                                            result.RETURN_FLAG = false;
                                                            result.DESCRIPTION = "無更新資料!";
                                                        }
                                                    }
                                                    com.Dispose();
                                                }
                                                    break;
                                            }
                                            var dbFCD_HIS_ROW = db.FAP_CROSS_DEPARMENT_HIS.FirstOrDefault(x => x.aply_no == row.aply_no && x.seq_id == row.seq_id);
                                            if (dbFCD_HIS_ROW != null)
                                            {
                                                if (row.pk_id.IsNullOrWhiteSpace())
                                                    dbFCD_HIS_ROW.pk_id = pk_id;

                                                dbFCD_HIS_ROW.apply_status = "2"; //覆核完成
                                                dbFCD_HIS_ROW.appr_id = searchData.current_uid;
                                                dbFCD_HIS_ROW.appr_datetime = now;

                                                //logStr = _TREA_ITEM_His.modelToString(logStr);
                                            }
                                            else
                                            {
                                                return result;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return result;
                                    }
                                }
                                if(result.DESCRIPTION == "無更新資料!")
                                {
                                transaction.Rollback();
                                conn.Dispose();
                                conn.Close();
                                return result;
                                }
                                var validateMessage = db.GetValidationErrors().getValidateString();

                                if (validateMessage.Any())
                                {
                                    result.DESCRIPTION = validateMessage;
                                }
                                else
                                {
                                    try
                                    {
                                        db.SaveChanges();
                                        scope.Complete();
                                        transaction.Commit();

                                        #region LOG
                                        //新增LOG
                                        //Log log = new Log();
                                        //log.CFUNCTION = "覆核";
                                        //log.CACTION = "U";
                                        //log.CCONTENT = logStr;
                                        //LogDao.Insert(log, searchData.vCreateUid);
                                        #endregion

                                        result.RETURN_FLAG = true;
                                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplyNos)} 覆核成功!";
                                        
                                    }
                                    catch (DbUpdateException ex)
                                    {
                                        transaction.Rollback();
                                        result.DESCRIPTION = ex.exceptionMessage();
                                    }
                                }
                            //}
                        }
                    }
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            }

            if(result.RETURN_FLAG == true)
                result.Datas = GetReviewSearchDetail(searchData);

            //using (dbFGLEntities db = new dbFGLEntities())
            //{
            //    SysSeqDao sysSeqDao = new SysSeqDao();
            //    var aplyNos = viewModels.Select(x => x.aply_no).ToList();

            //    foreach (var aplyNo in aplyNos)
            //    {
            //        var dbFCD_HIS = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking().Where(x => x.aply_no == aplyNo).ToList();
            //        if (dbFCD_HIS.Any())
            //        {
            //            foreach (var row in dbFCD_HIS)
            //            {
            //                string pk_id = string.Empty;
            //                switch (row.exec_action)
            //                {
            //                    case "A"://新增
            //                             //string qPreCode = DateUtil.getCurChtDateTime(3);
            //                        var twCalendar = new System.Globalization.TaiwanCalendar();
            //                        string qPreCode = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0'));
            //                        var cId = sysSeqDao.qrySeqNo("AP", "B1", qPreCode).ToString().PadLeft(3, '0');
            //                        pk_id = $@"B1{qPreCode}{cId}";//B1 + 系統日期YYYMMDD(民國年) + 3碼流水號

            //                        var FCD = new FAP_CROSS_DEPARMENT()
            //                        {
            //                            pk_id = pk_id,
            //                            fun_id = row.fun_id,
            //                            appr_unit = row.appr_unit,
            //                            user_unit = row.user_unit,
            //                            memo = row.memo,

            //                            data_status = "1", //可異動
            //                            create_id = row.apply_id,
            //                            create_datetime = now,
            //                            appr_id = searchData.current_uid,
            //                            appr_datetime = now,
            //                            update_id = row.apply_id,
            //                            update_datetime = now
            //                        };
            //                        //logStr += RCR.modelToString(logStr);
            //                        db.FAP_CROSS_DEPARMENT.Add(FCD);
            //                        break;
            //                    case "U": //修改
            //                        var dbFCD = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);
            //                        if (dbFCD != null)
            //                        {
            //                            dbFCD.data_status = "1";
            //                            dbFCD.fun_id = row.fun_id_before;
            //                            dbFCD.appr_unit = row.appr_unit_before;
            //                            dbFCD.user_unit = row.user_unit_before;
            //                            dbFCD.memo = row.memo_before;

            //                            dbFCD.appr_id = searchData.current_uid;
            //                            dbFCD.appr_datetime = now;
            //                            //logStr += dbRCR.modelToString(logStr);
            //                        }
            //                        break;
            //                    case "D"://刪除
            //                        var dbFCD_DELE = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);
            //                        db.FAP_CROSS_DEPARMENT.Remove(dbFCD_DELE);
            //                        break;
            //                }

            //                var dbFCD_HIS_ROW = db.FAP_CROSS_DEPARMENT_HIS.FirstOrDefault(x => x.aply_no == row.aply_no);
            //                if (dbFCD_HIS_ROW != null)
            //                {
            //                    if (row.pk_id.IsNullOrWhiteSpace())
            //                        dbFCD_HIS_ROW.pk_id = pk_id;

            //                    dbFCD_HIS_ROW.apply_status = "2"; //覆核完成
            //                    dbFCD_HIS_ROW.appr_id = searchData.current_uid;
            //                    dbFCD_HIS_ROW.appr_datetime = now;

            //                    //logStr = _TREA_ITEM_His.modelToString(logStr);
            //                }
            //                else
            //                {
            //                    return result;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            return result;
            //        }
            //    }
            //    var validateMessage = db.GetValidationErrors().getValidateString();

            //    if (validateMessage.Any())
            //    {
            //        result.DESCRIPTION = validateMessage;
            //    }
            //    else
            //    {
            //        try
            //        {
            //            db.SaveChanges();

            //            #region LOG
            //            //新增LOG
            //            //Log log = new Log();
            //            //log.CFUNCTION = "覆核";
            //            //log.CACTION = "U";
            //            //log.CCONTENT = logStr;
            //            //LogDao.Insert(log, searchData.vCreateUid);
            //            #endregion

            //            result.RETURN_FLAG = true;
            //            result.DESCRIPTION = $"申請單號 : {string.Join(",", aplyNos)} 覆核成功!";
            //            result.Datas = GetReviewSearchDetail(searchData);
            //        }
            //        catch (DbUpdateException ex)
            //        {
            //            result.DESCRIPTION = ex.exceptionMessage();
            //        }
            //    }

            //    return result;
            //}
            return result;
        }

        public List<OAP0018AHisViewModel> GetHisData(string aply_no)
        {
            List<OAP0018AHisViewModel> result = new List<OAP0018AHisViewModel>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var dbFun = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "FUN_ID").ToList();
                var dbAction = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var depts = GetDepts();
                var emps = GetEmps();
                string time = DateTime.Now.ToString();

                result = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking()
                    .Where(x => x.aply_no == aply_no)
                    .AsEnumerable()
                    .Select(x => new OAP0018AHisViewModel()
                    {
                        exec_action = x.exec_action,
                        exec_action_value = dbAction.FirstOrDefault(z => z.CODE == x.exec_action)?.CODE_VALUE,
                        fun_value = x.exec_action != "D" ? dbFun.Where(z => z.CODE == x.fun_id).Select(y => y.CODE_VALUE).FirstOrDefault() : dbFun.Where(z => z.CODE == x.fun_id_before).Select(y => y.CODE_VALUE).FirstOrDefault(),
                        //fun_value_before = dbFun.Where(z => z.CODE == x.fun_id_before).Select(y => y.CODE_VALUE).FirstOrDefault(),
                        //appr_unit_name = x.exec_action != "D" ? depts.Where(z => z.DEP_ID == x.appr_unit).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault() : depts.Where(z => z.DEP_ID == x.appr_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        appr_unit_name = x.exec_action != "D" ? getFullDepName(depts.Where(z => z.DEP_ID == x.appr_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2 : getFullDepName(depts.Where(z => z.DEP_ID == x.appr_unit_before)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //appr_unit_name_before = depts.Where(z => z.DEP_ID == x.appr_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        //user_unit_name = x.exec_action != "D" ? depts.Where(z => z.DEP_ID == x.user_unit).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault() : depts.Where(z => z.DEP_ID == x.user_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        user_unit_name = x.exec_action != "D" ? getFullDepName(depts.Where(z => z.DEP_ID == x.user_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2 : getFullDepName(depts.Where(z => z.DEP_ID == x.user_unit_before)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //user_unit_name_before = depts.Where(z => z.DEP_ID == x.user_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        memo = x.exec_action != "D" ? x.memo : x.memo_before,
                        //memo_before = x.memo_before,
                        apply_name = x.apply_id.IsNullOrWhiteSpace() ? "" : emps.Where(z => z.MEM_MEMO1 == x.apply_id)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault(),
                        apply_time = TypeTransfer.dateTimeNToStringNT(x.apply_datetime, time)
                    }).ToList();
            }
            return result;
        }

        public List<OAP0018AViewModel> GetReviewSearchDetail(OAP0018ASearchViewModel searchModel)
        {
            List<OAP0018AViewModel> result = new List<OAP0018AViewModel>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var emps = GetEmps();
                var cUser = GetUser();
                var update_time_s = TypeTransfer.stringToADDateTimeN(searchModel.update_time_start?.Replace("/", string.Empty));
                var update_time_e = TypeTransfer.stringToADDateTimeN(searchModel.update_time_end?.Replace("/", string.Empty)).DateToLatestTime();


                var dbFDH = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking();
                var _FDH = dbFDH
                    .Where(x => x.apply_datetime >= update_time_s, update_time_s != null)
                    .Where(x => x.apply_datetime <= update_time_e, update_time_e != null)
                    .Where(x => x.apply_status == "1")
                    .AsEnumerable()
                    .Select(x => new OAP0018AViewModel()
                    {
                        aply_date = x.apply_datetime?.ToString("yyyy/MM/dd"),
                        aply_no = x.aply_no,
                        aply_id = x.apply_id,
                        aply_name = emps.FirstOrDefault(y => y.MEM_MEMO1 == x.apply_id)?.MEM_NAME?.Trim(),
                        review_flag = x.apply_id != cUser
                    }).ToList();
                result.AddRange(_FDH);
            }

            result = result.Distinct(new OAP0018AViewModel_Comparer()).OrderBy(x => x.aply_no).ToList();
            return result;
        }

        public Utility.MSGReturnModel<List<OAP0018AViewModel>> RejectedData(OAP0018ASearchViewModel searchData, List<OAP0018AViewModel> viewModels, string apprDesc)
        {
            var result = new MSGReturnModel<List<OAP0018AViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.Audit_Fail.GetDescription();
            DateTime now = DateTime.Now;
            string logStr = string.Empty;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var aplyNos = viewModels.Select(x => x.aply_no).ToList();
                foreach (var aplyNo in aplyNos)
                {
                    var dbFCD_HIS = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking().Where(x => x.aply_no == aplyNo).ToList();
                    if (dbFCD_HIS.Any())
                    {
                        foreach (var row in dbFCD_HIS)
                        {
                            var dbFCD = db.FAP_CROSS_DEPARMENT.FirstOrDefault(x => x.pk_id == row.pk_id);

                            if (dbFCD != null)
                            {
                                dbFCD.data_status = "1";//可異動
                                dbFCD.appr_id = searchData.current_uid;
                                dbFCD.appr_datetime = now;
                            }

                            var dbFCD_HIS_ROW = db.FAP_CROSS_DEPARMENT_HIS.FirstOrDefault(x => x.aply_no == row.aply_no && x.seq_id == row.seq_id);

                            if (dbFCD_HIS_ROW != null)
                            {
                                dbFCD_HIS_ROW.apply_status = "3"; //退回
                                dbFCD_HIS_ROW.appr_datetime = now;
                                dbFCD_HIS_ROW.appr_id = searchData.current_uid;

                                //logStr += dbRTR_HIS_ROW.modelToString(logStr);
                            }
                            else
                            {
                                return result;
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                var validateMessage = db.GetValidationErrors().getValidateString();

                if (validateMessage.Any())
                {
                    result.DESCRIPTION = validateMessage;
                }
                else
                {
                    try
                    {
                        db.SaveChanges();

                        #region LOG
                        //新增LOG
                        //Log log = new Log();
                        //log.CFUNCTION = "覆核";
                        //log.CACTION = "U";
                        //log.CCONTENT = logStr;
                        //LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplyNos)} 已駁回!";
                        result.Datas = GetReviewSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }
    }
}