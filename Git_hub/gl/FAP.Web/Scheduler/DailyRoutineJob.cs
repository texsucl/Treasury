using FAP.Web.BO;
using FAP.Web.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Text;
using System.Web;

namespace FAP.WebScheduler
{
    public class DailyRoutineJob : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");
            var now = DateTime.Now;
            string nowTime = now.ToString("HH:mm");

            logger.Info("SendRemindMail:" + nowTime);
            SendRemindMail();
            //if (nowTime == "10:00")
            //{

            //}
        }

        public void SendRemindMail()
        {
            List<string> DateList = new List<string>();
            List<DailyViewModel> result = new List<DailyViewModel>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                string sql = string.Empty;
                System.Globalization.TaiwanCalendar twCalendar = new System.Globalization.TaiwanCalendar();
                var now = DateTime.Now;
                var TWTODAY = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0'));
                string ENTRY_DATE = string.Format("{0}{1}{2}", twCalendar.GetYear(now), now.Month.ToString().PadLeft(2, '0'), now.AddDays(-3).Day.ToString().PadLeft(2, '0'));
                conn.Open();

                try
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select YEAR, MONTH, DAY
from LGLCALE1
where ((LPAD(YEAR, 3, '0') || LPAD(MONTH, 2, '0') || LPAD(DAY, 2, '0')) <=  :TWTODAY)
and corp_rest <> 'Y'
order by YEAR desc, month desc, day desc
FETCH FIRST 4 ROWS ONLY
";



                        com.Parameters.Add("TWTODAY", TWTODAY);
                        sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbDate = com.ExecuteReader();
                        while (dbDate.Read())
                        {
                            var y = dbDate["YEAR"]?.ToString()?.Trim();
                            var m = dbDate["MONTH"]?.ToString()?.Trim()?.PadLeft(2, '0');
                            var d = dbDate["DAY"]?.ToString()?.Trim()?.PadLeft(2, '0');
                            string _date = y + m + d;
                            //string _date = $"{dbDate["YEAR"]?.ToString()?.Trim()}{dbDate["MONTH"]?.ToString()?.Trim()}{dbDate["DAY"]?.ToString()?.Trim()}";
                            DateList.Add(_date);
                        }
                        com.Dispose();
                    }

                    if (DateList.Any())
                        ENTRY_DATE = DateList.LastOrDefault();

                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                        select APPLY_ID, APPLY_NO
                        from FAPPYSN0
                        where ENTRY_DATE <= :ENTRY_DATE
                        AND FLAG != 'Y'
                        ";
                        com.Parameters.Add("ENTRY_DATE", ENTRY_DATE);
                        sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            //result.Add(dbresult["APPLY_ID"]?.ToString()?.Trim());
                            result.Add(new DailyViewModel()
                            {
                                APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(),
                                APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim()
                            });
                        }

                        com.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    logger.Info(ex.ToString());
                }
                conn.Dispose();
                conn.Close();
            }

            var data = result.GroupBy(x => x.APPLY_ID)
                .Select(x => new DailyViewModel() {
                    APPLY_ID = x.Key,
                    APPLY_NO = string.Join(",", x.Select(y => y.APPLY_NO))
                }).ToList();

            var remindList = new FAP.Web.Service.Actual.Common().GetMemoByUserId(data.Select(x => x.APPLY_ID));
            data.ForEach(x => {
                var _person = remindList.FirstOrDefault(y => y.Item1 == x.APPLY_ID);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($@"
親愛的同仁您好：
申請單號：{x.APPLY_NO}逾期未簽收，請儘速至AS400 系統PAP8005應付票據簽收功能中完成簽收作業。

");
                var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { };
                if (_UATmailAccount == "Y")
                {
                    _mailTo.Add(new Tuple<string, string>("Ex2016ap@fbt.com", "測試帳號-Ex2016ap"));
                }
                FAP_NOTES_PAYABLE_RECEIVED personDetail = new FAP_NOTES_PAYABLE_RECEIVED();
                if (string.IsNullOrWhiteSpace(_person.Item5))
                {
                    logger.Info("組織樹 & DB_INTRA 比對不到EMAIL: " + x.APPLY_ID);
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        personDetail = db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking().FirstOrDefault(y => y.apt_id == x.APPLY_ID);
                        if(personDetail != null)
                        {
                            if (string.IsNullOrWhiteSpace(personDetail.apt_name))
                            {
                                logger.Info("開始寄信:" + personDetail.apt_id);
                                _mailTo.Add(new Tuple<string, string>(personDetail.email, personDetail.apt_id));   //EMAIL, Id
                            }
                                
                            else
                            {
                                logger.Info("開始寄信:" + personDetail.apt_name);
                                _mailTo.Add(new Tuple<string, string>(personDetail.email, personDetail.apt_name));   //EMAIL, NAME
                            }
                        }
                        else
                        {
                            logger.Info("Table: FAP_NOTES_PAYABLE_RECEIVED 找不到資料: " + x.APPLY_ID);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_person.Item2))
                    {
                        logger.Info("開始寄信:" + _person.Item1);
                        _mailTo.Add(new Tuple<string, string>(_person.Item5, _person.Item1));   //EMAIL, Id
                    }
                    else
                    {
                        logger.Info("開始寄信:" + _person.Item2);
                        _mailTo.Add(new Tuple<string, string>(_person.Item5, _person.Item2));   //EMAIL, NAME
                    }   
                }

                if (_mailTo.Any())
                {
                    try
                    {
                        var sms = new SendMail.SendMailSelf();
                        sms.smtpPort = 25;
                        sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                        sms.mailAccount = ConfigurationManager.AppSettings["mailAccount"];
                        sms.mailPwd = ConfigurationManager.AppSettings["mailPwd"];
                        var message = sms.Mail_Send(
                            new Tuple<string, string>(sms.mailAccount, "應付票據 MAIL 通知"),
                            _mailTo,
                            null,
                            "應付票據變更逾期未簽收案件通知",
                            sb.ToString(),
                            false,
                            null
                            );
                        var ex = message;
                        if (!string.IsNullOrWhiteSpace(ex))
                            logger.Info("寄信異常訊息: " + ex);
                        else
                            logger.Info("寄信成功: " + _person.Item2);
                    }
                    catch (Exception ex)
                    {
                        logger.Info(_person.Item2 + " 發送失敗," + ex.ToString());
                    }
                }
                else
                {
                    logger.Info("無收件者,不寄信");
                }
                
            }); 
        }
    }

    public class DailyViewModel
    {
        public string APPLY_ID { get; set; }
        public string APPLY_NO { get; set; }
    }
}