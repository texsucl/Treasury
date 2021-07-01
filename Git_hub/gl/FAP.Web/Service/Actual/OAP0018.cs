using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FAP.Web.ViewModels;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.BO;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;
using FAP.Web.Daos;
using System.Data.Entity.Infrastructure;

namespace FAP.Web.Service.Actual
{
    public class OAP0018 : Common, IOAP0018
    {
        public Utility.MSGReturnModel<string> ApplyDeptData(List<OAP0018ViewModel> saveData)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            DateTime now = DateTime.Now;
            string _aply_no = string.Empty;

            try
            {
                if (saveData.Any())
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        var twCalendar = new System.Globalization.TaiwanCalendar();
                        string qPreCode = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0'));
                        int _seq_id = 0;
                        //取得流水號
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        //string qPreCode = DateUtil.getCurChtDateTime(3);
                        var cId = sysSeqDao.qrySeqNo("AP", "S1", qPreCode).ToString().PadLeft(3, '0');

                        _aply_no = $@"S1{qPreCode}{cId}";//S1 + 系統日期YYYMMDD(民國年) + 3碼流水號
                        var user = GetUser();

                        foreach (var rowData in saveData)
                        {
                            var _pkId = string.Empty;
                            var _FCD = new FAP_CROSS_DEPARMENT();
                            _seq_id = _seq_id + 1;
                            #region 主檔
                            //判斷執行功能
                            switch (rowData.exec_action)
                            {
                                case "A": //新增
                                    _pkId = "";
                                    break;
                                case "U": //修改
                                case "D": //刪除
                                    _FCD = db.FAP_CROSS_DEPARMENT.FirstOrDefault(y => y.pk_id == rowData.pk_id);
                                    if (_FCD.update_datetime != null && _FCD.update_datetime > rowData.update_time_cpmpare)
                                    {
                                        result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                        return result;
                                    }
                                    _pkId = rowData.pk_id;

                                    _FCD.update_datetime = now;
                                    _FCD.update_id = user;
                                    _FCD.data_status = "2"; //凍結中
                                    //logStr += "|";
                                    //logStr += _RDR.modelToString();
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                            #region 客戶異動檔
                            switch (rowData.exec_action)
                            {

                                case "A"://新增
                                    var _FCDH = new FAP_CROSS_DEPARMENT_HIS()
                                    {
                                        aply_no = _aply_no,
                                        seq_id = _seq_id.ToString(),
                                        fun_id = rowData.fun_id,
                                        appr_unit = rowData.appr_unit,
                                        user_unit = rowData.user_unit,
                                        pk_id = _pkId,
                                        memo = rowData.memo,
                                        exec_action = rowData.exec_action,
                                        apply_status = "1", //表單申請
                                        apply_id = user,
                                        apply_datetime = now
                                    };
                                    db.FAP_CROSS_DEPARMENT_HIS.Add(_FCDH);
                                    break;
                                case "U": //修改
                                    var db_FCD = db.FAP_CROSS_DEPARMENT.AsNoTracking().FirstOrDefault(y => y.pk_id == rowData.pk_id);
                                    if (db_FCD != null)
                                    {
                                        var _FCDHU = new FAP_CROSS_DEPARMENT_HIS()
                                        {
                                            aply_no = _aply_no,
                                            seq_id = _seq_id.ToString(),
                                            pk_id = rowData.pk_id,
                                            exec_action = rowData.exec_action,
                                            fun_id_before = db_FCD.fun_id,
                                            appr_unit_before = db_FCD.appr_unit,
                                            user_unit_before = db_FCD.user_unit,
                                            memo_before = db_FCD.memo,
                                            fun_id = rowData.fun_id,
                                            appr_unit = rowData.appr_unit,
                                            user_unit = rowData.user_unit,
                                            memo = rowData.memo,
                                            
                                            apply_datetime = now,
                                            apply_id = user,
                                            apply_status = "1" //表單申請
                                        };
                                        db.FAP_CROSS_DEPARMENT_HIS.Add(_FCDHU);
                                        //logStr += "|";
                                        //logStr += _RCRHU.modelToString();
                                    }
                                    break;
                                case "D": //刪除
                                    var db_FCD_DELE = db.FAP_CROSS_DEPARMENT.AsNoTracking().FirstOrDefault(y => y.pk_id == rowData.pk_id);
                                    if (db_FCD_DELE != null)
                                    {
                                        var _FCDHD = new FAP_CROSS_DEPARMENT_HIS()
                                        {
                                            aply_no = _aply_no,
                                            seq_id = _seq_id.ToString(),
                                            pk_id = rowData.pk_id,
                                            exec_action = rowData.exec_action,
                                            fun_id_before = db_FCD_DELE.fun_id,
                                            appr_unit_before = db_FCD_DELE.appr_unit,
                                            user_unit_before = db_FCD_DELE.user_unit,
                                            memo_before = db_FCD_DELE.memo,
                                            fun_id = "",
                                            appr_unit = "",
                                            user_unit = "",
                                            memo = "",

                                            apply_datetime = now,
                                            apply_id = user,
                                            apply_status = "1" //表單申請
                                        };
                                        db.FAP_CROSS_DEPARMENT_HIS.Add(_FCDHD);
                                        //logStr += "|";
                                        //logStr += _RCRHU.modelToString();
                                    }
                                    break;
                            }
                            #endregion
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
                                ////申請覆核LOG
                                //Log log = new Log();
                                //log.CFUNCTION = "申請覆核-收據類別與公司別";
                                //log.CACTION = "A";
                                //log.CCONTENT = logStr;
                                //LogDao.Insert(log, searchModel.vCurrent_Uid);
                                #endregion

                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_aply_no}");

                            }
                            catch (DbUpdateException ex)
                            {
                                result.DESCRIPTION = ex.exceptionMessage();
                            }
                        }
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }
            return result;
        }

        public bool CheckSameData(OAP0018InsertViewModel model, string mod)
        {
            bool hasSameData = false;
            if (model != null)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //var dbUserUnit = db.FAP_CROSS_DEPARMENT.AsNoTracking()
                    //    .Where(x => x.user_unit == model.user_unit)
                    //    .ToList();

                    var dbCorss = db.FAP_CROSS_DEPARMENT.AsNoTracking()
                        .Where(x => x.fun_id == model.fun_id)
                        .Where(x => x.user_unit == model.user_unit)
                        .ToList();

                    var dbHis = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking()
                        .Where(x => x.exec_action == "A")
                        .Where(x => x.apply_status == "1")
                        .Where(x => x.fun_id == model.fun_id)
                        .Where(x => x.user_unit == model.user_unit)
                        .Any();

                    //var dbCorss = db.FAP_CROSS_DEPARMENT.AsNoTracking()
                    //    .Where(x => x.fun_id == model.fun_id)
                    //    .Where(x => x.appr_unit == model.appr_unit)
                    //    .Where(x => x.user_unit == model.user_unit)
                    //    .ToList();

                    if (mod == "U")
                        dbCorss.Remove(dbCorss.Where(x => x.pk_id == model.pk_id).FirstOrDefault());

                    //if (dbCorss.Any() || dbUserUnit.Any())
                    if (dbCorss.Any() || dbHis)
                        hasSameData = true;
                }
            }
            return hasSameData;
        }

        public List<OAP0018ViewModel> GetSearchData(OAP0018SearchViewModel searchModel)
        {
            List<OAP0018ViewModel> result = new List<OAP0018ViewModel>();
            DateTime now = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                var dbCROSS_HIS = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking();
                var dbAction = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var dbStatus = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();
                var dbFun = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "FUN_ID").ToList();
                string time = DateTime.Now.ToString();

                result = db.FAP_CROSS_DEPARMENT.AsNoTracking()
                    .Where(x => x.appr_unit == searchModel.appr_unit, !searchModel.appr_unit.IsNullOrWhiteSpace())
                    .Where(x => x.user_unit == searchModel.user_unit, !searchModel.user_unit.IsNullOrWhiteSpace())
                    .AsEnumerable()
                    .Select(x => new OAP0018ViewModel
                    {
                        fun_id = x.fun_id,
                        fun_value = dbFun.Where(z => z.CODE == x.fun_id)?.Select(y => y.CODE_VALUE).FirstOrDefault(),
                        exec_action = dbCROSS_HIS.FirstOrDefault(y => y.pk_id == x.pk_id && y.apply_status == "1")?.exec_action,
                        exec_action_value = dbAction.FirstOrDefault(z => z.CODE == dbCROSS_HIS.FirstOrDefault(y => y.pk_id == x.pk_id && y.apply_status == "1")?.exec_action)?.CODE_VALUE,
                        appr_unit = x.appr_unit,
                        //appr_unit_name = depts.Where(z => z.DEP_ID == x.appr_unit)?.Select(z => z.DEP_NAME).FirstOrDefault(),
                        appr_unit_name = getFullDepName(depts.Where(z => z.DEP_ID == x.appr_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        user_unit = x.user_unit,
                        //user_unit_name = depts.Where(z => z.DEP_ID == x.user_unit)?.Select(z => z.DEP_NAME).FirstOrDefault(),
                        user_unit_name = getFullDepName(depts.Where(z => z.DEP_ID == x.user_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        memo = x.memo,
                        data_status = x.data_status,
                        data_status_value = dbStatus.Where(z => z.CODE == x.data_status)?.Select(y => y.CODE_VALUE).FirstOrDefault(),
                        update_name = x.update_id.IsNullOrWhiteSpace() ? "" : emps.Where(z => z.MEM_MEMO1 == x.update_id)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault(),
                        update_time = TypeTransfer.dateTimeNToStringNT(x.update_datetime, time),
                        update_time_cpmpare = now,
                        pk_id = x.pk_id
                    }).ToList();
            }
            return result;
        }
    }
}