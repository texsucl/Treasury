using FRT_ReTrXml.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Transactions;
using Topshelf;

namespace FRT_ReTrXml
{
    class Program
    {
        static void Main(string[] args)
        {
            //HostFactory.Run(x =>
            //{
            //    x.Service<CustomerSocket>(s =>
            //    {
            //        s.ConstructUsing(name => new CustomerSocket());
            //        s.WhenStarted(ms => ms.Start());
            //        s.WhenStopped(ms => ms.Stop());
            //    });

            //    x.SetServiceName("FRTReXmlServiceName");
            //    x.SetDisplayName("FRTReXmlDisplayName");
            //    x.SetDescription("FRTReXmlDescription");
            //    x.RunAsLocalSystem();
            //    x.StartAutomatically();
            //});

            work();
        }

        private static void work()
        {
            //Console.WriteLine("程式開始!");l
            WriteLog("程式開始!");
            //IPEndPoint ipont = new IPEndPoint(IPAddress.Parse("10.240.68.193"), 1234);
            //IPEndPoint ipont = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
            var msg = string.Empty;
            string Sys_Error = "SYS_ERROR";
            StringBuilder sbMsg = new StringBuilder();
            int workNum = 0; //已判斷工作時間內資料數
            int unworkNum = 0; //已判斷非工作時間資料數
            int doworkNum = 0; //已傳送成功資料數
            int statNum = 0; //已變更狀態資料數
            string _Sys_Error = Properties.Settings.Default["System_Group"]?.ToString();
            if (!string.IsNullOrWhiteSpace(_Sys_Error))
                Sys_Error = _Sys_Error;

            //將IP位址和Port宣告為服務的連接點
            //Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var _fastNo = string.Empty;
            EacConnection con = new EacConnection(ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString);
            try
            {
                //宣告一個Socket通訊介面(使用IPv4協定,通訊類型,通訊協定)
                //server.Connect(ipont);

                string CORP_REST = "Y"; //是否為工作日
                con.Open();
                DateTime _dtn = DateTime.Now;
                using (EacCommand ec = new EacCommand(con))
                {
                    Console.WriteLine(@"查詢是否為工作日");
                    Console.WriteLine(string.Empty);
                    //string sql = @"SELECT * FROM LRTBARM1 WHERE 1 = 1 ";
                    string sql = @"select CALE1_CORP_REST from LGLCALE1 WHERE CALE1_YEAR = :CALE1_YEAR AND CALE1_MONTH = :CALE1_MONTH AND CALE1_DAY = :CALE1_DAY ;";
                    ec.CommandText = sql;
                    var _CALE1_YEAR = (_dtn.Year - 1911).ToString();
                    var _CALE1_MONTH = _dtn.Month.ToString();
                    var _CALE1_DAY = _dtn.Day.ToString();
                    ec.Parameters.Add("CALE1_YEAR", _CALE1_YEAR);
                    ec.Parameters.Add("CALE1_MONTH", _CALE1_MONTH);
                    ec.Parameters.Add("CALE1_DAY", _CALE1_DAY);
                    Console.WriteLine($@"查出工作日 CommandText:{sql}");
                    WriteLog($@"查出工作日 CommandText:{sql}");
                    Console.WriteLine($@"CALE1_YEAR:{_CALE1_YEAR}");
                    Console.WriteLine($@"CALE1_MONTH:{_CALE1_MONTH}");
                    Console.WriteLine($@"CALE1_DAY:{_CALE1_DAY}");
                    WriteLog($@"CALE1_YEAR:{_CALE1_YEAR}");
                    WriteLog($@"CALE1_MONTH:{_CALE1_MONTH}");
                    WriteLog($@"CALE1_DAY:{_CALE1_DAY}");
                    Console.WriteLine(string.Empty);
                    //EASYCOM廠商建議新增此句
                    ec.Prepare();
                    DbDataReader result = ec.ExecuteReader();
                    while (result.Read())
                    {
                        var CALE1_CORP_REST = result["CALE1_CORP_REST"]?.ToString();
                        Console.WriteLine($@"CALE1_CORP_REST:{CALE1_CORP_REST}");//CALE1_CORP_REST 
                        WriteLog($@"CALE1_CORP_REST:{CALE1_CORP_REST}");
                        CORP_REST = CALE1_CORP_REST == "Y" ? "Y" : "N";
                    }
                    Console.WriteLine(string.Empty);
                    ec.Dispose();
                }

                //工作日做事
                if (CORP_REST != "Y")
                {
                    List<string> fastnos = new List<string>();
                    using (EacCommand ec = new EacCommand(con))
                    {
                        //Console.WriteLine(@"");
                        //Console.WriteLine(string.Empty);
                        //WHERE REMIT_STAT = 'W'
                        string sql = @"SELECT FAST_NO FROM LRTBARM1 WHERE TEXT_TYPE = 'W' ";
                        ec.CommandText = sql;
                        ec.Prepare();
                        DbDataReader result = ec.ExecuteReader();
                        DataTable data = new DataTable();
                        while (result.Read())
                        {
                            //server.Connect(ipont);
                            string FAST_NO = result["FAST_NO"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(FAST_NO))
                            {
                                fastnos.Add(FAST_NO);
                            }
                            //server.Dispose();
                        }
                        ec.Dispose();
                    }
                    sbMsg.AppendLine($@"本次執行查詢W件有:{fastnos.Count}筆");
                    foreach (var FAST_NO in fastnos)
                    {
                        _fastNo = FAST_NO;
                        string BANK_CODE = string.Empty; //銀行代碼
                        string BANK_TYPE = string.Empty; //銀行類型
                        string TEXT_TYPE = string.Empty; //電文類型

                        WriteLog($@"查詢FAST_NO:{_fastNo} 是否可以執行");

                        #region 先用快速付款編號查出銀行代碼BANK_CODE
                        using (EacCommand ec = new EacCommand(con))
                        {
                            Console.WriteLine(@"先用快速付款編號查出銀行代碼BANK_CODE");
                            Console.WriteLine(string.Empty);
                            string sql = @"SELECT BANK_CODE, SUB_BANK, PAID_ID, FAST_NO, BANK_CODE, BANK_ACT, REMIT_AMT, RCV_NAME , SYS_TYPE , SRCE_FROM , SRCE_KIND FROM LRTBARM1 WHERE 1 = 1 ";
                            sql += @"AND FAST_NO = :FAST_NO ";
                            sql += @" ;";
                            ec.CommandText = sql;
                            ec.Parameters.Add("FAST_NO", _fastNo);
                            Console.WriteLine($@"查出銀行代碼 CommandText:{sql}");
                            WriteLog($@"查出銀行代碼 CommandText:{sql}");
                            Console.WriteLine($@"FAST_NO:{_fastNo}");           
                            Console.WriteLine(string.Empty);
                            //EASYCOM廠商建議新增此句
                            ec.Prepare();
                            DbDataReader result = ec.ExecuteReader();
                            while (result.Read())
                            {
                                BANK_CODE = result["BANK_CODE"]?.ToString();
                                Console.WriteLine($@"BANK_CODE:{BANK_CODE}");//DEBUG LINE
                            }
                            Console.WriteLine(string.Empty);
                            ec.Dispose();
                        }
                        #endregion

                        #region 再用銀行代碼查出銀行類型
                        using (EacCommand ec = new EacCommand(con))
                        {
                            Console.WriteLine(@"再用銀行代碼查出銀行類型");
                            Console.WriteLine(string.Empty);
                            //string sql = @"SELECT * FROM FRTBBKM0 WHERE 1 = 1 ";
                            string sql = @"SELECT BBKM0_BANK_TYPE FROM FRTBBKM0 WHERE 1 = 1 ";
                            sql += @"AND BBKM0_BANK_CODE = :BANK_CODE ";
                            sql += @" ;";
                            ec.CommandText = sql;
                            ec.Parameters.Add("BANK_CODE", BANK_CODE);
                            Console.WriteLine($@"查出銀行類型 CommandText:{sql}");
                            WriteLog($@"查出銀行類型 CommandText:{sql}");
                            Console.WriteLine($@"BANK_CODE:{BANK_CODE}");
                            WriteLog($@"BANK_CODE:{BANK_CODE}");
                            Console.WriteLine(string.Empty);
                            //EASYCOM廠商建議新增此句
                            ec.Prepare();
                            DbDataReader result = ec.ExecuteReader();
                            while (result.Read())
                            {
                                if (!string.IsNullOrWhiteSpace(result["BBKM0_BANK_TYPE"]?.ToString()))
                                {
                                    BANK_TYPE = result["BBKM0_BANK_TYPE"]?.ToString();
                                }
                                Console.WriteLine($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                                WriteLog($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                            }
                            ec.Dispose();
                        }
                        if (string.IsNullOrWhiteSpace(BANK_TYPE))
                        {
                            using (EacCommand ec = new EacCommand(con))
                            {
                                Console.WriteLine(@"用萬用銀行代碼查出銀行類型");
                                Console.WriteLine(string.Empty);
                                //string sql = @"SELECT * FROM FRTBBKM0 WHERE 1 = 1 ";
                                string sql = @"SELECT BBKM0_BANK_TYPE FROM FRTBBKM0 WHERE 1 = 1 ";
                                sql += @"AND BBKM0_BANK_CODE = :BANK_CODE ";
                                sql += @" ;";
                                ec.CommandText = sql;
                                string bc = string.Empty.PadLeft(BANK_CODE.Length, '*');
                                ec.Parameters.Add("BANK_CODE", bc);
                                Console.WriteLine($@"查出銀行類型 CommandText:{sql}");
                                WriteLog($@"查出銀行類型 CommandText:{sql}");
                                Console.WriteLine($@"BANK_CODE:{bc}");
                                WriteLog($@"BANK_CODE:{bc}");
                                Console.WriteLine(string.Empty);
                                //EASYCOM廠商建議新增此句
                                ec.Prepare();
                                DbDataReader result = ec.ExecuteReader();
                                while (result.Read())
                                {
                                    if (!string.IsNullOrWhiteSpace(result["BBKM0_BANK_TYPE"]?.ToString()))
                                    {
                                        BANK_TYPE = result["BBKM0_BANK_TYPE"]?.ToString();
                                    }
                                    Console.WriteLine($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                                    WriteLog($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                                }
                                ec.Dispose();
                            }
                        }
                        #endregion

                        #region 再用銀行類型查詢出電文類型
                        using (EacCommand ec = new EacCommand(con))
                        {
                            Console.WriteLine(@"再用銀行類型查詢出電文類型");
                            Console.WriteLine(string.Empty);
                            //string sql = @"SELECT * FROM FRTBTMM0 WHERE 1 = 1 ";
                            string sql = @"SELECT BTMM0_TEXT_TYPE FROM FRTBTMM0 WHERE 1 = 1 ";
                            sql += @"AND BTMM0_BANK_TYPE = :BBKM0_BANK_TYPE ";
                            //判斷是否在傳送區間內？是-->0，否-->W
                            sql += @"AND :NOW_TIME BETWEEN BTMM0_STR_TIME AND BTMM0_END_TIME ";
                            sql += @" ;";
                            ec.CommandText = sql;
                            ec.Parameters.Add("BBKM0_BANK_TYPE", BANK_TYPE);
                            Console.WriteLine($@"查出電文類型 CommandText:{sql}");
                            WriteLog($@"查出電文類型 CommandText:{sql}");
                            Console.WriteLine($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                            WriteLog($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                            string NOW_TIME = DateTime.Now.ToString("HHmm");
                            //NOW_TIME = "1000";
                            ec.Parameters.Add("NOW_TIME", NOW_TIME);
                            Console.WriteLine($@"NOW_TIME:{NOW_TIME}");
                            WriteLog($@"NOW_TIME:{NOW_TIME}");
                            Console.WriteLine(string.Empty);
                            //EASYCOM廠商建議新增此句
                            ec.Prepare();
                            DbDataReader result = ec.ExecuteReader();
                            while (result.Read())
                            {
                                TEXT_TYPE = result["BTMM0_TEXT_TYPE"].ToString();
                                Console.WriteLine($@"BTMM0_TEXT_TYPE:{TEXT_TYPE}");
                                WriteLog($@"BTMM0_TEXT_TYPE:{TEXT_TYPE}");
                            }
                            Console.WriteLine(string.Empty);
                            ec.Dispose();
                        }
                        #endregion

                        #region 超過傳送時間
                        if (string.IsNullOrWhiteSpace(TEXT_TYPE))
                        {
                            //SW
                            unworkNum += 1;
                            Console.WriteLine("超過傳送時間");
                            WriteLog("超過傳送時間");
                        }
                        #endregion
                        #region 傳送時間內
                        else
                        {
                            workNum += 1;
                            using (EacCommand ec = new EacCommand(con))
                            {
                                WriteLog($"FAST_NO:{FAST_NO}");
                                string sql = $@"
                        update LRTBARM1
                        set TEXT_TYPE = 'Q' 
                        where FAST_NO = :FAST_NO 
                        and TEXT_TYPE = 'W' ; ";
                                WriteLog($"update sql:{sql}");
                                ec.CommandText = sql;
                                ec.Parameters.Add("FAST_NO", FAST_NO);
                                //EASYCOM廠商建議新增此句
                                ec.Prepare();
                                var updateNum = ec.ExecuteNonQuery();
                                ec.Dispose();
                                WriteLog($"資料異動筆數:{updateNum}");
                                if (updateNum == 1)
                                {
                                    var _result = StartClient(Encoding.GetEncoding("IBM037").GetBytes(FAST_NO));
                                    WriteLog($"Result:{_result.Item1}");
                                    if (!string.IsNullOrWhiteSpace(_result.Item2))
                                    {
                                        WriteLog($"FAST_NO:{_fastNo},傳送失敗", Ref.Nlog.Trace);
                                        msg += System.Environment.NewLine;
                                        msg += $"FAST_NO:{_fastNo}";
                                        msg += System.Environment.NewLine;
                                        msg += _result.Item2;
                                        //sendMail(Sys_Error, msg);
                                        //msg = string.Empty;
                                        break;
                                    }
                                    else
                                    {
                                        doworkNum += 1;
                                        WriteLog($"FAST_NO:{_fastNo},傳送成功", Ref.Nlog.Trace);
                                    }
                                }
                                else
                                {
                                    statNum += 1;
                                    WriteLog($"FAST_NO:{_fastNo},電文狀態已不為'W'",Ref.Nlog.Trace);
                                }
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    Console.WriteLine("非工作日");
                    WriteLog("非工作日");
                }
            }
            catch (Exception ex)
            {
                msg += System.Environment.NewLine;
                msg += $"FAST_NO:{_fastNo}";
                msg += System.Environment.NewLine;
                msg += $"程式錯誤:{ex.Message}";
                WriteLog($"程式錯誤:{ex.Message}", Ref.Nlog.Error);
                //server.Dispose();
                //Console.WriteLine($"程式錯誤:{ex.Message}");
            }
            finally
            {
                con.Dispose();
                con.Close();
                sbMsg.AppendLine($@"已判斷工作時間內資料數:{workNum}筆");
                sbMsg.AppendLine($@"已判斷非工作時間資料數:{unworkNum}筆");
                sbMsg.AppendLine($@"已傳送成功資料數:{doworkNum}筆");
                if(statNum != 0)
                    sbMsg.AppendLine($@"已變更狀態資料數:{statNum}筆");
                if (workNum > 0)
                    sendMail(Sys_Error, sbMsg.ToString(), msg);
                //if (!string.IsNullOrWhiteSpace(msg))
                //{                
                //    sendMail(Sys_Error, msg);
                //}
                WriteLog("程式結束!");
                //Console.WriteLine("程式結束!");
                //Console.ReadLine();
                //server.Dispose();
            }
        }

        public class CustomerSocket
        {
            private System.Timers.Timer _timer;

            public CustomerSocket()
            {
                _timer = new System.Timers.Timer(1000) { AutoReset = false };
                _timer.Elapsed += new ElapsedEventHandler(this.MainTask);
            }

            private void MainTask(object sender, ElapsedEventArgs args)
            {
                work();
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

        public static Tuple<int,string> StartClient(byte[] msg)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];
            var result1 = 0;
            var result2 = string.Empty;

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];

                var IpPoint = "127.0.0.1";
                var _IpPoint = Properties.Settings.Default["IPPoint"]?.ToString();
                if (!string.IsNullOrWhiteSpace(_IpPoint))
                    IpPoint = _IpPoint;

                var _RcvPort = Properties.Settings.Default["RcvPort"]?.ToString();

                int port = 1234;
                int _port = 0;
                if (int.TryParse(_RcvPort, out _port))
                    port = _port;

                IPEndPoint ipont = new IPEndPoint(IPAddress.Parse(IpPoint), port);

                //將IP位址和Port宣告為服務的連接點
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.ReceiveTimeout = 5000;
                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    server.Connect(ipont);

                    //Console.WriteLine("Socket connected to {0}",
                    //    server.RemoteEndPoint.ToString());
                    WriteLog($@"Socket connected to : {server.RemoteEndPoint.ToString()}");
                    // Encode the data string into a byte array.  
                    //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.  
                    int bytesSent = server.Send(msg);

                    // Receive the response from the remote device.  
                    result1 = server.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}",
                    //    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    //server.Shutdown(SocketShutdown.Both);
                    //server.Close();

                }
                catch (ArgumentNullException ane)
                {
                    //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    result2 = $@"ArgumentNullException : {ane}";
                    WriteLog($@"ArgumentNullException : {ane}");
                }
                catch (SocketException se)
                {
                    //Console.WriteLine("SocketException : {0}", se.ToString());
                    result2 = $@"SocketException : {se}";
                    WriteLog($@"SocketException : {se}");
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    result2 = $@"Unexpected exception : {e}";
                    WriteLog($@"Unexpected exception : {e}");
                }
                finally
                {
                    CloseSocket(server);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                result2 = $@"Exception : {e}";
                WriteLog($@"Exception : {e}");
            }

            return new Tuple<int, string>(result1,result2);
        }


        /// <summary> 
        /// 關閉Socket 
        /// </summary> 
        /// <param name="s">需要關閉的Socket</param> 
        private static void CloseSocket(Socket s)
        {
            try
            {
                if (s.Connected)
                {
                    s.Shutdown(SocketShutdown.Both);

                    s.Disconnect(false);

                }

                s.Close();

                //Console.WriteLine("關閉連線"); 
            }
            catch (Exception e)
            {
                WriteLog(e.ToString(), Ref.Nlog.Error);
                //Console.WriteLine(e.Message); 
            }
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

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name=""></param>
        /// <returns></returns>
        protected static int sendMail(string groupCode = "SYS_ERROR", string sbMsg = null, string msg = null)
        {
            int result = 0;
            DateTime dn = DateTime.Now;
            string dtn = $@"{(dn.Year - 1911)}/{dn.ToString("MM/dd")}";
            var emps = new List<V_EMPLY2>();
            var depts = new List<VW_OA_DEPT>();
            var sub = string.Empty;
            var body = new StringBuilder();
            var _sub = (string.IsNullOrWhiteSpace(msg) ? "訊息" : "失敗");
            sub = $@"{dtn} 快速付款 FRT_ReTrXml {_sub}通知";
            body.AppendLine(sbMsg);
            if(!string.IsNullOrWhiteSpace(msg))
                body.AppendLine($@"錯誤訊息:{msg}");

            var _msg = string.Empty;
            Dictionary<string, Tuple<string, string>> mailToList = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> ccToList = new Dictionary<string, Tuple<string, string>>();
            bool Flag = false;
            try
            {
                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    emps = db.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null && x.EMP_NO != null && x.DPT_CD != null).ToList();
                    depts = db.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }
                using (dbGLEntities db = new dbGLEntities())
                {
                    List<FRT_MAIL_NOTIFY> _FMNs = db.FRT_MAIL_NOTIFY.AsNoTracking()
                         .Where(x => x.GROUP_CODE == groupCode).ToList();
                    _FMNs.ForEach(x =>
                    {
                        List<string> USRIDs = new List<string>();
                        if (x.EMP_TYPE == "U")
                        {
                            USRIDs.Add(x.RECEIVER_EMPNO?.Trim());
                        }
                        else if (x.EMP_TYPE == "R")
                        {
                            USRIDs.AddRange(db.CODE_ROLE.Where(y => y.ROLE_ID != null && y.ROLE_ID == x.RECEIVER_EMPNO && y.IS_DISABLED == "N")
                            .Join(db.CODE_USER_ROLE.AsNoTracking().Where(z => z.ROLE_ID != null && z.USER_ID != null),
                             i => i.ROLE_ID.Trim(),
                             j => j.ROLE_ID.Trim(),
                             (i, j) => j)
                             .Join(db.CODE_USER.AsNoTracking().Where(z => z.IS_DISABLED == "N" && z.USER_ID != null),
                             i => i.USER_ID.Trim(),
                             j => j.USER_ID.Trim(),
                             (i, j) => j).Where(y => y.USER_ID != null).Select(y => y.USER_ID.Trim()));
                        }
                        if (USRIDs.Any())
                        {
                            USRIDs = USRIDs.Distinct().ToList();
                            var mailDatas = getSendMail(USRIDs, x.IS_NOTIFY_MGR, x.IS_NOTIFY_DEPT_MGR, emps, depts);
                            foreach (var mail in mailDatas.Item1)
                            {
                                if (!mailToList.ContainsKey(mail.Key))
                                {
                                    mailToList.Add(mail.Key, mail.Value);
                                }
                            }
                            foreach (var cc in mailDatas.Item2)
                            {
                                //if (!ccToList.ContainsKey(cc.Key))
                                //{
                                //    ccToList.Add(cc.Key,cc.Value);
                                //}
                                if (!mailToList.ContainsKey(cc.Key))
                                {
                                    mailToList.Add(cc.Key, cc.Value);
                                }
                            }
                        }
                        else
                        {

                        }
                    });
                }
                var self = Properties.Settings.Default["sendMailTest"]?.ToString();
                if (self == "Y")
                    mailToList.Add("ReTrXml", new Tuple<string, string>(Properties.Settings.Default["mailAccount"]?.ToString(), "FRT_ReTrXml"));
                if (mailToList.Values.Any())
                {
                    var sms = new SendMail.SendMailSelf();
                    sms.smtpPort = 25;
                    sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                    sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                    sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                    _msg = sms.Mail_Send(
                        new Tuple<string, string>(sms.mailAccount, "FRT_ReTrXml"),
                        mailToList.Values.AsEnumerable(),
                        ccToList.Any() ? ccToList.Values.AsEnumerable() : null,
                        sub,
                        body.ToString()
                        );
                    if (string.IsNullOrWhiteSpace(_msg))
                    {
                        Flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"寄信: InnerException:{ex.InnerException},Message:{ex.Message}", Ref.Nlog.Error);
                Flag = false;
                _msg = $"InnerException:{ex.InnerException},Message:{ex.Message}";
            }
            finally
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    var saveFlog = true;
                    using (dbGLEntities db = new dbGLEntities())
                    {
                        foreach (var mail in mailToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = mail.Key ?? string.Empty,
                                EMAIL = mail.Value.Item1 ?? string.Empty,
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0, 250) : _msg,
                                MAIL_SUB = sub.Length > 100 ? sub.Substring(0,100) : sub
                            });
                        }
                        foreach (var cc in ccToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = cc.Key ?? string.Empty,
                                EMAIL = cc.Value.Item1 ?? string.Empty,
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0, 250) : _msg,
                                MAIL_SUB = sub.Length > 100 ? sub.Substring(0, 100) : sub
                            });
                        }
                        var err = db.GetValidationErrors();
                        if (err.Any())
                        {
                            WriteLog(getValidateString(err), Ref.Nlog.Error);
                            saveFlog = false;
                        }
                        else
                            db.SaveChanges();
                    }
                    if(saveFlog)
                        scope.Complete();
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢收信人
        /// </summary>
        /// <param name="USRIDs">收信人</param>
        /// <param name="IS_NOTIFY_MGR">IS_NOTIFY_MGR = Y時，除TABLE本身的人員要寄送，還要寄給科主管</param>
        /// <param name="IS_NOTIFY_DEPT_MGR">IS_NOTIFY_DEPT_MGR = Y時，除TABLE本身的人員要寄送，還要寄給部主管</param>
        /// <returns></returns>
        protected static Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>> getSendMail(
            List<string> USRIDs,
            string IS_NOTIFY_MGR,
            string IS_NOTIFY_DEPT_MGR,
            List<V_EMPLY2> emps,
            List<VW_OA_DEPT> depts)
        {
            var sendEmps = emps.Where(x => USRIDs.Contains(x.USR_ID.Trim())).ToList();
            Dictionary<string, Tuple<string, string>> mailTos = new Dictionary<string, Tuple<string, string>>();
            sendEmps.ForEach(x =>
            {
                mailTos.Add(x.USR_ID?.Trim(), new Tuple<string, string>(x.EMAIL?.Trim(), x.EMP_NAME?.Trim()));
            });
            Dictionary<string, Tuple<string, string>> ccs = new Dictionary<string, Tuple<string, string>>();
            if (IS_NOTIFY_MGR == "Y" || IS_NOTIFY_DEPT_MGR == "Y")
            {
                List<string> ccUSRIDs = new List<string>();
                var _sendEmps = sendEmps;
                if (IS_NOTIFY_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給科主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }
                    });
                }
                if (IS_NOTIFY_DEPT_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給部主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "03") //部
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            var dept2 = depts.FirstOrDefault(y => y.DPT_CD.Trim() == dept.UP_DPT_CD.Trim());
                            var emp2 = emps.FirstOrDefault(y => y.EMP_NO.Trim() == dept2?.DPT_HEAD.Trim());
                            if (emp2 != null)
                                ccUSRIDs.Add(emp2.EMP_NO.Trim());
                        }
                    });
                }
                if (ccUSRIDs.Any())
                {
                    ccUSRIDs = ccUSRIDs.Distinct().ToList();

                    foreach (var item in emps.Where(x => ccUSRIDs.Contains(x.EMP_NO.Trim())))
                    {
                        if (!ccs.ContainsKey(item.USR_ID?.Trim()))
                        {
                            ccs.Add(item.USR_ID?.Trim(), new Tuple<string, string>(item.EMAIL?.Trim(), item.EMP_NAME?.Trim()));
                        }
                    }
                }
            }
            return new Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>>(mailTos, ccs);
        }

        public static string getValidateString
            (IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
                result = string.Join(" ", errors.Select(y => string.Join(",", y.ValidationErrors.Select(z => z.ErrorMessage))));
            return result;
        }

        protected static long getMailSeq(dbGLEntities db)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var seq = db.SYS_SEQ.FirstOrDefault(x => x.SYS_CD == "RT" && x.SEQ_TYPE == "mail" && x.PRECODE == date);
            string seqNo = string.Empty;
            if (seq == null)
            {
                seqNo = $@"{date}{"1".PadLeft(5, '0')}";
                db.SYS_SEQ.Add(new SYS_SEQ()
                {
                    SYS_CD = "RT",
                    SEQ_TYPE = "mail",
                    PRECODE = date,
                    CURR_VALUE = 2
                });
                var err = db.GetValidationErrors();
                if (err.Any())
                    WriteLog(getValidateString(err),Ref.Nlog.Error);
                else
                    db.SaveChanges();
            }
            else
            {
                seqNo = $@"{date}{seq.CURR_VALUE.ToString().PadLeft(5, '0')}";
                seq.CURR_VALUE = seq.CURR_VALUE + 1;
            }
            return Convert.ToInt64(seqNo);
        }
    }
}
