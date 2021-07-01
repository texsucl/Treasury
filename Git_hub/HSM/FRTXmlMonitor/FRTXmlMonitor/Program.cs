using FRTXmlMonitor.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace FRTXmlMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<CustomerSocket>(s =>
                {
                    s.ConstructUsing(name => new CustomerSocket());
                    s.WhenStarted(ms => ms.Start());
                    s.WhenStopped(ms => ms.Stop());
                });

                x.SetServiceName("FRTXmlMonitorServiceName");
                x.SetDisplayName("FRTXmlMonitorDisplayName");
                x.SetDescription("快速付款FBO主電文監視程式");
                x.RunAsLocalSystem();
                x.StartAutomatically();
            });
        }

        public class CustomerSocket
        {
            private System.Timers.Timer _timer;

            public CustomerSocket()
            {
                int _time = getckeckNum(Properties.Settings.Default["compareSec"]?.ToString(), 300000); 
                _timer = new System.Timers.Timer(_time) { AutoReset = true,Enabled = true };
                _timer.Elapsed += new ElapsedEventHandler(this.MainTask);
            }

            public void MainTask(object sender, ElapsedEventArgs args)
            {
                try
                {
                    DateTime dtn = DateTime.Now;
                    #region 修正檢核Log by 20191007

                    List<string> fixs = new List<string>();
                    string fixStr = Properties.Settings.Default["fixFastNo"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(fixStr))
                    {
                        fixs = fixStr.Split(';').ToList();
                    }
                    if (fixs.Any())
                    {
                        using (dbGLEntities db = new dbGLEntities())
                        {
                            bool _saveFlag = false;
                            foreach (var item in fixs)
                            {
                                var _fastNo = item?.Trim();
                                if (db.FRT_XML_LOG.AsNoTracking().Any(x => x.fast_no == _fastNo))
                                {
                                    _saveFlag = true;
                                    db.FRT_XML_LOG_DETAIL.Add(new FRT_XML_LOG_DETAIL()
                                    {
                                        fast_no = _fastNo,
                                        operation_status = "SS",
                                        create_date = dtn,
                                        create_time_hms = dtn.TimeOfDay
                                    });
                                }
                            }
                            if (_saveFlag)
                            {
                                db.SaveChanges();
                            }
                        }
                    }
                    #endregion

                    List<string> hidden = new List<string>();
                    string hiddenStr = Properties.Settings.Default["hiddenFastNo"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(hiddenStr))
                    {
                        hidden = hiddenStr.Split(';').ToList();
                    }

                    using (dbGLEntities db = new dbGLEntities())
                    {
                        
                        WriteLog($@"執行時間 : {dtn.ToString("yyyy/MM/dd HH:mm:ss")}");
                        int checkDay = getckeckNum(Properties.Settings.Default["checkDay"]?.ToString(), -1);
                        int checkMinute = getckeckNum(Properties.Settings.Default["checkMinute"]?.ToString(), -5);
                        DateTime dtnd = dtn.AddDays(checkDay).Date;
                        DateTime dtnt = dtn.AddMinutes(checkMinute);
                        List<string> status = new List<string>() { "SS", "SW", "SB" };

                        var items = from item in db.FRT_XML_LOG_DETAIL.AsNoTracking()
                                    .Where(x => x.create_date >= dtnd)
                                    group item by item.fast_no into c
                                    select new
                                    {
                                        key = c.Key,
                                        value = c.OrderByDescending(x => x.create_date)
                                    .ThenByDescending(x => x.create_time_hms)
                                    .FirstOrDefault()
                                    };
                        List<FRT_XML_LOG_DETAIL> checks = new List<FRT_XML_LOG_DETAIL>();
                        foreach (var item in items)
                        {
                            var detail = item.value;
                            if (detail != null &&
                                !status.Contains(detail.operation_status) &&
                                !hidden.Contains(detail.fast_no))
                            {
                                var detaildt = detail.create_date;
                                detaildt = detaildt.Add(detail.create_time_hms);
                                if (dtnt >= detaildt)
                                    checks.Add(detail);
                            }
                        }
                        if (checks.Any())
                        {
                            string body = string.Empty;
                            body = string.Join(" ;\r\n", checks
                                .Select(x => $@"fastNo:{x.fast_no},status:{x.operation_status},createDate:{x.create_date.ToString("yyyy/MM/dd")},createTime{x.create_time_hms}"));
                            WriteLog($@"檢核異常 : {body}");
                            sendMail(body);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString(), Ref.Nlog.Error);
                }
            }

            public async void Start()
            {
                _timer.Start();
            }

            public async void Stop()
            {
                _timer.Stop();
            }
        }

        protected static int getckeckNum(string value, int defaultValue)
        {
            int val = 0;
            if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out val))
                return val;
            return defaultValue;
        }

        protected static void sendMail(string body)
        {
            var sms = new SendMail.SendMailSelf();
            sms.smtpPort = 25;
            sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
            sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
            sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
            string sendMailstr = Properties.Settings.Default["mailSend"]?.ToString();
            List<Tuple<string, string>> sendMails = new List<Tuple<string, string>>();
            foreach (var item in sendMailstr.Split(';'))
            {
                string mail = string.Empty;
                string name = string.Empty;
                var items = item.Split(':');
                mail = items[0];
                if (items.Length == 2)
                    name = items[1];
                else
                    name = items[0];
                sendMails.Add(new Tuple<string, string>(mail, name));
            }
            string _msg = sms.Mail_Send(
                new Tuple<string, string>(sms.mailAccount, "FRTXmlMonitor"),
                sendMails,
                null,
                "FRTXmlMonitor",
                body
                );
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected static void WriteLog(string log, Ref.Nlog type = Ref.Nlog.Info)
        {
            switch (type)
            {
                //追蹤
                case Ref.Nlog.Trace:
                    logger.Trace(log);
                    break;
                //開發
                case Ref.Nlog.Debug:
                    logger.Debug(log);
                    break;
                //訊息
                case Ref.Nlog.Info:
                    logger.Info(log);
                    break;
                //警告
                case Ref.Nlog.Warn:
                    logger.Warn(log);
                    break;
                //錯誤
                case Ref.Nlog.Error:
                    logger.Error(log);
                    break;
                //致命
                case Ref.Nlog.Fatal:
                    logger.Fatal(log);
                    break;
            }
        }

        public partial class Ref
        {
            public enum Nlog
            {
                [Description("追蹤")]
                Trace,
                [Description("開發")]
                Debug,
                [Description("訊息")]
                Info,
                [Description("警告")]
                Warn,
                [Description("錯誤")]
                Error,
                [Description("致命")]
                Fatal
            }
        }
    }
}
