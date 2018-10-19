using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Scheduler
{
    public class DailyRoutineJob :IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Extension.NlogSet("[Execute]執行開始!!");

            MAIL_TIME _MT = new MAIL_TIME();
            var dtnstr = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dtn = DateTime.Now;
                dtnstr = $"{ dtn.Hour.ToString().PadLeft(2, '0')}:{dtn.Minute.ToString().PadLeft(2, '0')}";
                Extension.NlogSet($"SEND_TIME:{dtnstr}");            
                _MT = db.MAIL_TIME.FirstOrDefault(
                    x => x.SEND_TIME != null &&
                    x.SEND_TIME == dtnstr &&
                    x.IS_DISABLED != "Y" &&
                    x.SCHEDULER_STATUS != "Y");
                //var _Mail_Time_ID = _MT.MAIL_TIME_ID;
                if (_MT != null)
                {
                    try
                    {
                        _MT.SCHEDULER_STATUS = "Y";
                        _MT.SCHEDULER_UPDATE_DT = dtn;
                        db.SaveChanges();
                        Extension.NlogSet($"MAIL_TIME 有找到例行性排程設定檔案");
                        Extension.NlogSet($"DateTime : {dtn}");
                        Routine(db, dtnstr, dtn);
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
                                x.MAIL_CONTENT_ID == "01").ToList();
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


            Extension.NlogSet("[Execute]執行結束!!");
        }

        public void Routine(TreasuryDBEntities db,string dateTime,DateTime _dtn)
        {
            var _split = dateTime.Split(':');
            var hh = _split[0];
            var mm = _split.Length > 1 ?  _split[1] : string.Empty;
            var dt = new DateTime(_dtn.Year, _dtn.Month, _dtn.Day, Convert.ToInt32(hh),Convert.ToInt32(mm),0);
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var _Mail_Time = db.MAIL_TIME.ToList()
                .FirstOrDefault(x =>
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
                    if(!test.IsNullOrWhiteSpace())
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
                        APPR_STATUS = "1",
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

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(
$@"您好,
通知今日金庫開關庫時間為:{_Mail_Time.TREA_OPEN_TIME}，請準時至金庫門口集合。
為配合金庫大門之啟閉，請有權人在:{_Mail_Time.EXEC_TIME_E} 前進入「金庫進出管理系統」完成入庫確認作業，謝謝。
");
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
                            new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") },
                            null,
                            "金庫每日例行開庫通知",
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
    }
}