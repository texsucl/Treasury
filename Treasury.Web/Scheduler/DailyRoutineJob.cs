using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;


namespace Treasury.Web.Scheduler
{
    public class DailyRoutineJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Extension.NlogSet("[Execute]執行開始!!");

            MAIL_TIME _MT = new MAIL_TIME();
            MAIL_TIME _mt = new MAIL_TIME();
            var dtnstr = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dtn = DateTime.Now;
                dtnstr = $"{ dtn.Hour.ToString().PadLeft(2, '0')}:{dtn.Minute.ToString().PadLeft(2, '0')}";
                Extension.NlogSet($"SEND_TIME:{dtnstr}");

                #region 8.2 金庫確認作業提醒
                try
                {
                    //金庫開庫流程提醒通知 抓3
                    _mt = db.MAIL_TIME.AsEnumerable().FirstOrDefault(x => x.MAIL_TIME_ID == "3" && x.IS_DISABLED == "N" && DateTime.Parse(x.EXEC_TIME_B + ":00") <= DateTime.Parse(dtnstr + ":00") && DateTime.Parse(x.EXEC_TIME_E + ":00") >= DateTime.Parse(dtnstr + ":00"));
                    if (_mt != null)
                    {
                        var _INTERVAL_MIN = _mt.INTERVAL_MIN;
                        //if (dtn.Minute % _INTERVAL_MIN == 0)
                        if (int.Parse((DateTime.Parse(dtnstr + ":00") - DateTime.Parse(_mt.EXEC_TIME_B + ":00")).Minutes.ToString()) % _INTERVAL_MIN == 0)
                        {
                            _mt.SCHEDULER_STATUS = "Y";
                            _mt.SCHEDULER_UPDATE_DT = dtn;

                            try
                            {
                                db.SaveChanges();

                                Extension.NlogSet($"MAIL_TIME 通知檢核啟動");
                                Extension.NlogSet($"DateTime : {dtn}");

                                RemindClose(db, dtnstr, dtn, _INTERVAL_MIN);
                            }
                            catch (Exception ex)
                            {
                                Extension.NlogSet($"關庫確認作業錯誤,Exception:{ex.exceptionMessage()}!!");
                            }
                            finally
                            {
                                Extension.NlogSet($"執行 更新SCHEDULER_STATUS 為 N !!");
                                var _mt2 = db.MAIL_TIME
                                        .Where(x =>
                                        x.MAIL_TIME_ID == "3")
                                        .ToList();
                                if (_mt2.Any())
                                {
                                    Extension.NlogSet($"更新SCHEDULER_STATUS 為 N  !!");
                                    foreach (var item in _mt2)
                                    {
                                        item.SCHEDULER_STATUS = "N";
                                    }
                                    try
                                    {
                                        db.SaveChanges();
                                    }
                                    catch
                                    {

                                    }
                                }
                                else
                                {
                                    Extension.NlogSet($"無更新SCHEDULER_STATUS !!");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Extension.NlogSet($"8.2 關庫確認作業提醒,Exception:{ex.exceptionMessage()}!!");
                }
                #endregion

                #region 8.1 每日例行出入庫mail通知作業
                try
                {
                    //例行開庫通知 抓1,2
                    var _MAIL_TIME_ID = new List<string>() { "1", "2" };
                    _MT = db.MAIL_TIME.FirstOrDefault(
                    x => x.SEND_TIME != null &&
                    x.SEND_TIME == dtnstr &&
                    x.IS_DISABLED != "Y" &&
                    x.SCHEDULER_STATUS != "Y" &&
                    _MAIL_TIME_ID.Contains(x.MAIL_TIME_ID));
                    //var _Mail_Time_ID = _MT.MAIL_TIME_ID;
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking().AsEnumerable()
                        .FirstOrDefault(x =>
                        x.OPEN_TREA_TYPE != "1" &&
                        x.REGI_STATUS != "E01" &&
                        x.APPR_STATUS != "4");
                    //if (_MT != null && _checkFlag)
                    if (_MT != null && _TREA_OPEN_REC == null)
                    {
                        try
                        {
                            _MT.SCHEDULER_STATUS = "Y";
                            _MT.SCHEDULER_UPDATE_DT = dtn;
                            db.SaveChanges();
                            Extension.NlogSet($"MAIL_TIME 有找到例行性排程設定檔案");
                            Extension.NlogSet($"DateTime : {dtn}");
                            Routine(db, dtnstr, dtn, _MT.MAIL_TIME_ID);
                        }
                        catch (Exception ex)
                        {
                            Extension.NlogSet($"每日例行作業錯誤,Exception:{ex.exceptionMessage()}!!");
                        }
                        finally
                        {
                            Extension.NlogSet($"執行 更新SCHEDULER_STATUS 為 N !!");
                            var _MT2 = db.MAIL_TIME
                                    .Where(x =>
                                    x.SEND_TIME != null &&
                                    x.SEND_TIME == dtnstr &&
                                    _MAIL_TIME_ID.Contains(x.MAIL_TIME_ID)).ToList();
                            if (_MT2.Any())
                            {
                                Extension.NlogSet($"更新SCHEDULER_STATUS 為 N  !!");
                                foreach (var item in _MT2)
                                {
                                    item.SCHEDULER_STATUS = "N";
                                }
                                try
                                {
                                    db.SaveChanges();
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                Extension.NlogSet($"無更新SCHEDULER_STATUS !!");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Extension.NlogSet($"8.1 每日例行出入庫mail通知作業,Exception:{ex.exceptionMessage()}!!");
                }

                #endregion


            }

            Extension.NlogSet("[Execute]執行結束!!");
        }

        public void Routine(TreasuryDBEntities db, string dateTime, DateTime _dtn,string MAIL_TIME_ID)
        {
            var _split = dateTime.Split(':');
            var hh = _split[0];
            var mm = _split.Length > 1 ? _split[1] : string.Empty;
            var dt = new DateTime(_dtn.Year, _dtn.Month, _dtn.Day, Convert.ToInt32(hh), Convert.ToInt32(mm), 0);
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var _Mail_Time = db.MAIL_TIME.ToList()
                .FirstOrDefault(x =>
                x.MAIL_TIME_ID == MAIL_TIME_ID &&
                x.SEND_TIME != null &&
                x.SEND_TIME == dateTime &&
                x.IS_DISABLED != "Y" &&
                x.SCHEDULER_UPDATE_DT == _dtn);
            if (_Mail_Time != null)
            {
                string errorMsg = string.Empty;

                var MailTId = sysSeqDao.qrySeqNo("MailT", qPreCode).ToString().PadLeft(3, '0');
                var _JOB_ITEM_ID = $@"{qPreCode}W{MailTId}";
                try
                {
                    #region 新增排程工作紀錄檔
                    Extension.NlogSet($"新增排程工作紀錄檔");
                    db.SYS_JOB_REC.Add(new SYS_JOB_REC()
                    {
                        JOB_ITEM_ID = _JOB_ITEM_ID,
                        JOB_ID = _Mail_Time.MAIL_TIME_ID,
                        CREATE_TIME = dt,
                        STATUS = "2",
                        START_TIME = _dtn,
                    });

                    var test = db.GetValidationErrors().getValidateString();
                    if (!test.IsNullOrWhiteSpace())
                        Extension.NlogSet($"{test}");
                    db.SaveChanges();
                    #endregion

                    #region 新增 開庫紀錄檔
                    Extension.NlogSet($"新增開庫紀錄檔");
                    db.TREA_OPEN_REC.RemoveRange(
                    db.TREA_OPEN_REC.Where(x =>
                    x.OPEN_TREA_TYPE == "1" &&
                    x.OPEN_TYPE == "Y" &&
                    x.REGI_STATUS == "C02"));

                    var _TORId = sysSeqDao.qrySeqNo("W", qPreCode).ToString().PadLeft(2, '0');
                    var _TOR = new TREA_OPEN_REC()
                    {
                        TREA_REGISTER_ID = $"W{qPreCode}{_TORId}", //開庫工作單號(金庫登記簿單號)
                        OPEN_TREA_TYPE = "1", //開庫類型
                        OPEN_TREA_REASON = "例行性開庫", //開庫原因
                        OPEN_TREA_TIME = _Mail_Time.TREA_OPEN_TIME, //開庫時間
                        EXEC_TIME_B = _Mail_Time.EXEC_TIME_B, //系統區間(起)
                        EXEC_TIME_E = _Mail_Time.EXEC_TIME_E, //系統區間(迄)
                        OPEN_TREA_DATE = _dtn,
                        APPR_STATUS = "2",
                        REGI_STATUS = "C02",
                        CREATE_DT = _dtn,
                        OPEN_TYPE = "Y" //Y為例行性
                    };
                    db.TREA_OPEN_REC.Add(_TOR);

                    #endregion

                    #region 取得例行出入庫作業項目

                    #region 自【金庫存取作業設定檔】查詢例行出入庫作業項目
                    Extension.NlogSet($"自【金庫存取作業設定檔】查詢例行出入庫作業項目");
                    var _Trea_Item = db.TREA_ITEM.AsNoTracking()
                                .Where(x =>
                                x.DAILY_FLAG == "Y" &&
                                x.IS_DISABLED == "N").ToList();
                    #endregion

                    #region 清空【申請單紀錄暫存檔】
                    Extension.NlogSet($" 清空【申請單紀錄暫存檔】");
                    db.TREA_APLY_TEMP.RemoveRange(db.TREA_APLY_TEMP);
                    #endregion

                    var _MAIL_CONTENT = db.MAIL_CONTENT.AsNoTracking()
                        .First(x => x.MAIL_CONTENT_ID == _Mail_Time.MAIL_CONTENT_ID);
                    var _MAIL_RECEIVE = db.MAIL_RECEIVE.AsNoTracking();
                    var _CODE_ROLE_FUNC = db.CODE_ROLE_FUNC.AsNoTracking();
                    var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsEnumerable();
                    var _CODE_USER = db.CODE_USER.AsNoTracking();
                    List<string> _userIdList = new List<string>();
                    var emps = GetEmps();
                    List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") };
                    List<Tuple<string, string>> _ccTo = new List<Tuple<string, string>>();

                    var _FuncId = _MAIL_RECEIVE.Where(x => x.MAIL_CONTENT_ID == _MAIL_CONTENT.MAIL_CONTENT_ID).Select(x => x.FUNC_ID);
                    var _RoleId = _CODE_ROLE_FUNC.Where(x => _FuncId.Contains(x.FUNC_ID)).Select(x => x.ROLE_ID);
                    var _UserId = _CODE_USER_ROLE.Where(x => _RoleId.Contains(x.ROLE_ID)).Select(x => x.USER_ID).Distinct();
                    _userIdList.AddRange(_CODE_USER.Where(x => _UserId.Contains(x.USER_ID) && x.IS_MAIL == "Y").Select(x => x.USER_ID));

                    if (_userIdList.Any())
                    {
                        //人名 EMAIl
                        var _EMP = emps.Where(x => _userIdList.Contains(x.USR_ID)).ToList();
                        if (_EMP.Any())
                        {
                            _EMP.ForEach(x => {
                                _mailTo.Add(new Tuple<string, string>(x.EMAIL, x.EMP_NAME));
                            });
                        }
                    }

                    var str = _MAIL_CONTENT.MAIL_CONTENT1;

                    str = str.Replace("@_TREA_OPEN_TIME_", _Mail_Time.TREA_OPEN_TIME);
                    str = str.Replace("@_EXEC_TIME_E_", _Mail_Time.EXEC_TIME_E);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(str);
//                    sb.AppendLine(
//$@"您好,
//通知今日金庫開關庫時間為:{_Mail_Time.TREA_OPEN_TIME}，請準時至金庫門口集合。
//為配合金庫大門之啟閉，請有權人在:{_Mail_Time.EXEC_TIME_E} 前進入「金庫進出管理系統」完成入庫確認作業，謝謝。
//");
                    int num = 1;

                    #region 將例行作業項目寫入【申請單紀錄暫存檔】
                    Extension.NlogSet($" 將例行作業項目寫入【申請單紀錄暫存檔】");
                    _Trea_Item.ForEach(x =>
                    {
                        db.TREA_APLY_TEMP.Add(new TREA_APLY_TEMP()
                        {
                            ITEM_ID = x.ITEM_ID
                        });
                        //sb.AppendLine($"{num}. {x.ITEM_DESC}");
                        num += 1;
                    });
                    #endregion

                    #endregion

                    #region 寄送mail給保管人
                    Extension.NlogSet($" 寄送mail給保管人");
                    try
                    {
                        var sms = new SendMail.SendMailSelf();
                        sms.smtpPort = 25;
                        sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                        sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                        sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                        Extension.NlogSet($" 寄送mail內容 : {sb.ToString()}");
                        sms.Mail_Send(
                            new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"),
                            _mailTo,
                            _ccTo,
                            _MAIL_CONTENT?.MAIL_SUBJECT ?? "金庫每日例行開庫通知",
                            sb.ToString()
                            );

                    }
                    catch (Exception ex)
                    {
                        Extension.NlogSet($" 寄送mail給保管人 錯誤 : {ex.exceptionMessage()}");
                    }


                    #endregion
                    test = db.GetValidationErrors().getValidateString();
                    if (!test.IsNullOrWhiteSpace())
                        Extension.NlogSet($"{test}");
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorMsg = ex.exceptionMessage();
                    Extension.NlogSet($"錯誤 : {errorMsg}");
                }
                #region 異動【排程工作紀錄檔】資料(工作結束) 
                var _SJR = db.SYS_JOB_REC
                    .FirstOrDefault(x => x.JOB_ITEM_ID == _JOB_ITEM_ID);
                if (_SJR != null)
                {
                    if (!errorMsg.IsNullOrWhiteSpace())
                    {
                        _SJR.STATUS = "4"; //執行失敗
                        _SJR.MEMO = errorMsg;
                    }
                    else
                    {
                        _SJR.STATUS = "3"; //執行成功
                        _SJR.MEMO = null;
                    }
                    _SJR.END_TIME = DateTime.Now;
                    var test = db.GetValidationErrors().getValidateString();
                    if (!test.IsNullOrWhiteSpace())
                        Extension.NlogSet($"{test}");
                    db.SaveChanges();
                }
                #endregion                       
            }
            //db.MAIL_TIME          
        }

        public void RemindClose(TreasuryDBEntities db, string dateTime, DateTime _dtn, int? _INTERVAL_MIN)
        {
            //bool result = true;
            var _split = dateTime.Split(':');
            var hh = _split[0];
            var mm = _split.Length > 1 ? _split[1] : string.Empty;
            var dt = new DateTime(_dtn.Year, _dtn.Month, _dtn.Day, Convert.ToInt32(hh), Convert.ToInt32(mm), 0);
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

            //var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking()
            //    .Where(x => x.REGI_STATUS.Trim()[0] == 'D' && x.LAST_UPDATE_DT < _dtn.AddMinutes(Convert.ToDouble(-_INTERVAL_MIN)));


            var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking().AsEnumerable()
                .FirstOrDefault(x => x.REGI_STATUS != "E01" && x.APPR_STATUS != "4" && x.LAST_UPDATE_DT < _dtn.AddMinutes(Convert.ToDouble(-_INTERVAL_MIN)));

            if (_TREA_OPEN_REC != null)
            {
                //result = false;

                var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking().FirstOrDefault(x => x.TREA_REGISTER_ID == _TREA_OPEN_REC.TREA_REGISTER_ID);

                var _Mail_Time = db.MAIL_TIME.AsNoTracking().FirstOrDefault(x => x.MAIL_TIME_ID == "3" && x.IS_DISABLED == "N");
                if (_Mail_Time != null)
                {
                    var _MAIL_CONTENT = db.MAIL_CONTENT.AsNoTracking().FirstOrDefault(x => x.MAIL_CONTENT_ID == _Mail_Time.MAIL_CONTENT_ID && x.IS_DISABLED == "N");
                    string errorMsg = string.Empty;

                    Extension.NlogSet($"MAIL_TIME 有找到未完成關庫覆核的金庫登記簿");
                    Extension.NlogSet($"DateTime : {_dtn}");

                    var MailTId = sysSeqDao.qrySeqNo("MailT", qPreCode).ToString().PadLeft(3, '0');
                    var _JOB_ITEM_ID = $@"{qPreCode}W{MailTId}";

                    try
                    {
                        #region 新增排程工作紀錄檔
                        Extension.NlogSet($"新增排程工作紀錄檔");
                        db.SYS_JOB_REC.Add(new SYS_JOB_REC()
                        {
                            JOB_ITEM_ID = _JOB_ITEM_ID,
                            JOB_ID = _Mail_Time.MAIL_TIME_ID,
                            CREATE_TIME = dt,
                            STATUS = "2",
                            START_TIME = _dtn,
                        });
                        var test = db.GetValidationErrors().getValidateString();
                        if (!test.IsNullOrWhiteSpace())
                            Extension.NlogSet($"{test}");
                        db.SaveChanges();
                        #endregion

                        List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") };
                        List<Tuple<string, string>> _ccTo = new List<Tuple<string, string>>();
                        var _MAIL_RECEIVE = db.MAIL_RECEIVE.AsNoTracking();
                        var _CODE_ROLE_FUNC = db.CODE_ROLE_FUNC.AsNoTracking();
                        var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsEnumerable();
                        var _CODE_USER = db.CODE_USER.AsNoTracking();
                        List<string> _userIdList = new List<string>();
                        var emps = GetEmps();

                        var _FuncId = _MAIL_RECEIVE.Where(x => x.MAIL_CONTENT_ID == _MAIL_CONTENT.MAIL_CONTENT_ID).Select(x => x.FUNC_ID);
                        var _RoleId = _CODE_ROLE_FUNC.Where(x => _FuncId.Contains(x.FUNC_ID)).Select(x => x.ROLE_ID);
                        var _UserId = _CODE_USER_ROLE.Where(x => _RoleId.Contains(x.ROLE_ID)).Select(x => x.USER_ID).Distinct();
                        _userIdList.AddRange(_CODE_USER.Where(x => _UserId.Contains(x.USER_ID) && x.IS_MAIL == "Y").Select(x => x.USER_ID).ToList());
                        if (_userIdList.Any())
                        {
                            //人名 EMAIl
                            var _EMP = emps.Where(x => _userIdList.Contains(x.USR_ID)).ToList();
                            if (_EMP.Any())
                            {
                                _EMP.ForEach(x => {
                                    _mailTo.Add(new Tuple<string, string>(x.EMAIL, x.EMP_NAME));
                                });
                            }
                        }


                        StringBuilder sb = new StringBuilder();

                        string str = _MAIL_CONTENT.MAIL_CONTENT1;
                        str = str.Replace("@_TREA_REGISTER_ID_", _TREA_OPEN_REC.TREA_REGISTER_ID);
                        var status = string.Empty;

                        switch (_TREA_OPEN_REC.REGI_STATUS)
                        {
                            case "C02":
                                if (_TREA_OPEN_REC.APPR_STATUS == "3")
                                {
                                    status = "【指定開庫申請作業】";
                                }
                                else if (_TREA_OPEN_REC.APPR_STATUS == "1")
                                {
                                    status = "【指定開庫覆核作業】";
                                }
                                else if (_TREA_OPEN_REC.APPR_STATUS == "2")
                                {
                                    if (_TREA_APLY_REC == null)
                                    {
                                        status = "【入庫人員確認作業】";
                                    }
                                    else
                                    {
                                        status = "【金庫登記簿執行作業(開庫前)】";
                                    }
                                }
                                break;
                            case "D01":
                                status = "【金庫登記簿執行作業(開庫後)】";
                                break;
                            case "D02":
                            case "D04":
                                status = "【金庫登記簿覆核作業】";
                                break;
                        }

                        str = str.Replace("@_STATUS_", status);
                        sb.AppendLine(str);

                        #region 寄送mail給相關人員
                        Extension.NlogSet($" 寄送mail給相關人員");
                        try
                        {
                            var sms = new SendMail.SendMailSelf();
                            sms.smtpPort = 25;
                            sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                            sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                            sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                            Extension.NlogSet($" 寄送mail內容 : {sb.ToString()}");
                            sms.Mail_Send(
                                new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"),
                                _mailTo,
                                _ccTo,
                                 _MAIL_CONTENT?.MAIL_SUBJECT ?? "金庫登記簿開庫流程尚完成通知",
                                sb.ToString()
                                );
                        }
                        catch (Exception ex)
                        {
                            Extension.NlogSet($" 寄送mail給相關人員 錯誤 : {ex.exceptionMessage()}");
                        }
                        test = db.GetValidationErrors().getValidateString();
                        if (!test.IsNullOrWhiteSpace())
                            Extension.NlogSet($"{test}");
                        db.SaveChanges();
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.exceptionMessage();
                        Extension.NlogSet($"錯誤 : {errorMsg}");
                    }

                    #region 異動【排程工作紀錄檔】資料(工作結束) 
                    var _SJR = db.SYS_JOB_REC
                        .FirstOrDefault(x => x.JOB_ITEM_ID == _JOB_ITEM_ID);
                    if (_SJR != null)
                    {
                        if (!errorMsg.IsNullOrWhiteSpace())
                        {
                            _SJR.STATUS = "4"; //執行失敗
                            _SJR.MEMO = errorMsg;
                        }
                        else
                        {
                            _SJR.STATUS = "3"; //執行成功
                            _SJR.MEMO = null;
                        }
                        _SJR.END_TIME = DateTime.Now;
                        var test = db.GetValidationErrors().getValidateString();
                        if (!test.IsNullOrWhiteSpace())
                            Extension.NlogSet($"{test}");
                        db.SaveChanges();
                    }
                    #endregion
                }
            }
            //return result;
        }

        /// <summary>
        /// 獲取 員工資料
        /// </summary>
        /// <returns></returns>
        public List<V_EMPLY2> GetEmps()
        {
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                emps = dbINTRA.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
            }

            return emps;
        }
    }
}