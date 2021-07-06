using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ConsoleApplication6
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rnd = new Random();
            //var lst = new MyTaskList();
            //for (var i = 0; i < 30; i++)
            //{
            //    var s = rnd.Next(10);
            //    var j = i;
            //    var act = new Action(() =>
            //    {
            //        Console.WriteLine(string.Format("第一次 第{0}個Action（用時{1}秒）Start", j, s));
            //        Thread.Sleep(s * 1000);
            //        Console.WriteLine(string.Format("第一次 第{0}個Action（用時{1}秒）End", j, s));
            //    });
            //    lst.Tasks.Add(act);
            //}
            //lst.Completed += () => Console.WriteLine("____________________Completed Action！");
            //lst.Start();
            //for (var i = 0; i < 30; i++)
            //{
            //    var s = rnd.Next(10);
            //    var j = i;
            //    var act = new Action(() =>
            //    {
            //        Console.WriteLine(string.Format("第二次 第{0}個Action（用時{1}秒）Start", j, s));
            //        Thread.Sleep(s * 1000);
            //        Console.WriteLine(string.Format("第二次 第{0}個Action（用時{1}秒）End", j, s));
            //    });
            //    lst.Tasks.Add(act);
            //}
            //lst.Start();

            //var a = "123";
            //foreach (var b in a.Split(new char[] { ':' }, 2))
            //{
            //    Console.WriteLine(b);
            //}
            //var a1 = getA1().GetAwaiter().GetResult();
            //Console.WriteLine(a1);
            //Console.WriteLine(("1234500".CompareTo("123451") >= 0));
            //var a = new List<string>()
            //{
            //    "Q12340001",
            //    "Q12340002",
            //    "Q12340003",
            //    "Q1234000A",
            //    "Q1234000B",
            //    "Q1234000C",
            //    "Q12340004",
            //    "Q12340005",
            //    "Q12340006"
            //};
            //foreach (var b in a.Where(z=> z.CompareTo("Q12340000") >= 0 & z.CompareTo("Q12350000") <= 0))
            //{
            //    Console.WriteLine(b);
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    new Program().getB1(i.ToString());
            //    //Task.Run(() => { new Program().getB1(i.ToString()); });
            //}
            //Console.WriteLine("E");


            //double d = 0d;
            //if (double.TryParse("123,324.34", out d))
            //    d = d;

            //var t = new Task[10];
            //for (var i = 0; i < 10; i++)
            //{
            //   t[i] = a(i);             
            //}
            // Task.WhenAll(t).Wait();

            //var sms = new SendMail.SendMailSelf();
            //sms.smtpPort = 25;
            //sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
            //sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
            //sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
            //try
            //{
            //    var _msg = sms.Mail_Send(
            //    new Tuple<string, string>(sms.mailAccount, "test"),
            //    new List<Tuple<string, string>>() {new Tuple<string, string>(sms.mailAccount, "test1") },
            //    null,
            //    "test",
            //    "test"
            //    );
            //    Console.WriteLine(_msg);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            //Console.ReadKey();
            Console.WriteLine("s");
            getC1("C1").Wait();
            Console.WriteLine("e");

            Console.ReadLine();
        }


        public class MyTaskList
        {
            public List<Action> Tasks = new List<Action>();

            public void Start()
            {
                for (var i = 0; i < 3; i++)
                    StartAsync();
            }

            public event Action Completed;

            public void StartAsync()
            {
                lock (Tasks)
                {
                    if (Tasks.Count > 0)
                    {
                        var t = Tasks[0];
                        Tasks.Remove(t);
                        ThreadPool.QueueUserWorkItem(h =>
                        {
                            t();
                            StartAsync();
                        });
                    }
                    else if (Completed != null)
                        Completed();
                }
            }
        }

        public static async Task getC1(string C1)
        {
            Console.WriteLine("start" + C1);
            await Task.Delay(2000);
            Console.WriteLine("end" + C1);
        }

        public async Task getB1(string B1)
        {
            Task.Delay(500).Wait();
            Task.Run(() => { getA1("A1_" + B1); });
            Console.WriteLine(B1);
        }


        public async Task getA1(string A1)
        {
            Task.Delay(500).Wait();
             Console.WriteLine(A1);
            //Console.WriteLine($@"END_{A1}");
        }

        private static object lockitem = new object();

        private static string getDt()
        {
            lock(lockitem)
            {
                Task.Delay(1000).Wait();
                return DateTime.Now.ToString("HHmmssf");
            }
        }

        private static async Task<string> a(int i)
        {
            WriteLog($@"start:{i}",$@"{i}");
            //await Task.Delay(1000);
            WriteLog($@"dt:{getDt()}", $@"{i}");
            WriteLog($@"end:{i}", $@"{i}");
            return null;
        }

        
        public static void WriteLog(string log, string name = null, Ref.Nlog type = Ref.Nlog.Info)
        {
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            if (!string.IsNullOrWhiteSpace(name))
            {
                logger = LogManager.GetLogger(name);
            }
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
