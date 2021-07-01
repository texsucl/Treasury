using FRTXml.Model;
using FRTXml.obj;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using System.Transactions;
using System.ComponentModel.DataAnnotations;
using Topshelf;
using System.Timers;

namespace FRTXml
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

                x.SetServiceName("FRTXmlServiceName");
                x.SetDisplayName("FRTXmlDisplayName");
                x.SetDescription("FRTXmlDescription");
                x.RunAsLocalSystem();
                x.StartAutomatically();
            });
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
                //直接測試
                //new test().run("Q0710000A1");

                var _RcvPort = Properties.Settings.Default["RcvPort"]?.ToString();

                int port = 9201;
                int _port = 0;
                if (int.TryParse(_RcvPort, out _port))
                    port = _port;

                //宣告一個Socket通訊介面(使用IPv4協定,通訊類型,通訊協定)
                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //將IP位址和Port宣告為服務的連接點(所有網路介面卡 IP,1234 Port)
                    IPEndPoint ipont = new IPEndPoint(IPAddress.Any, port);
                    //IPEndPoint ipont = new IPEndPoint(IPAddress.Any, 9201);

                    newsock.Bind(ipont);

                    //偵測連接(最大連接數)
                    newsock.Listen(50);

                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");
                        WriteLog($@"Waiting for a connection...", Ref.Nlog.Trace);

                        //宣告一個Socket等於新建立的連線
                        Socket client = newsock.Accept();

                        //宣告一個連接點為socket端的連接點
                        IPEndPoint clientip = (IPEndPoint)client.RemoteEndPoint;

                        //印出遠端IP位址
                        System.Console.WriteLine("Client End Point = " + clientip);
                        WriteLog($@"Client End Point ={clientip}", Ref.Nlog.Trace);

                        //宣告一個監聽類別SocketListener監聽client訊息
                        SocketListener listener = new SocketListener(client);

                        //宣告一個執行序去跑SocketListener監聽事件
                        Thread thread = new Thread(new ThreadStart(listener.run));

                        thread.Start();
                    }
                }
                catch (Exception ex)
                {
                    CloseSocket(newsock);
                    var _msg = $"錯誤訊息_Socket : InnerException:{ex.InnerException},Message:{ex.Message}";
                    Console.WriteLine(_msg);
                    WriteLog($"{_msg}", Ref.Nlog.Error);
                    string Sys_Error = "SYS_ERROR";
                    sendMail(string.Empty, mailType.SOCKET, Sys_Error, _msg);
                    new CustomerSocket().Start();
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

        protected static string work_date; //日期_全域變數
        protected static string CORP_REST; //是否為工作日_全域變數
        protected static bool SendMailFlag = false; //寄送報表註記
        protected static string SendMailMsg; //寄送報表訊息

        public class test
        {
            public void run(string fastNo = null)
            {
                
                Program.work(fastNo);
                
            }
        }

        private static object lockitem2 = new object();

        private static List<string> fastNos = new List<string>(); //目前在處理的快速付款編號

        public class SocketListener
        {
            Encoding ascii = Encoding.ASCII;
            Encoding ebcdic = Encoding.GetEncoding("IBM037");
            private Socket socket;
            
            public SocketListener(Socket socket)
            {
                //建構元取得遠端socket連線
                this.socket = socket;
                
            }

            public void run()
            {
                try
                {
                    string fastNo = string.Empty;
                    while (true)
                    {

                        Console.WriteLine("Start...");
                        WriteLog($@"Start...", Ref.Nlog.Trace);

                        Thread.Sleep(1);
                        //定義一個資料緩衝區接收長度最大為(1024)
                        byte[] data = new byte[1024];

                        //接收資料至緩衝區中並回傳成功接收位元數
                        int len = socket.Receive(data);

                        ////若成功接收位元數為0則跳出迴圈
                        if (len == 0) break;

                        byte[] asciiBytes = Encoding.Convert(ebcdic, ascii, data);
                        //string fastNo = Encoding.ASCII.GetString(asciiBytes);
                        fastNo = Encoding.ASCII.GetString(asciiBytes);

                        //印出編碼後的資料(資料,起始位置,長度)
                        Console.WriteLine(fastNo);//DEBUG LINE
                        WriteLog($@"接收到 fastNo={fastNo}", Ref.Nlog.Trace);

                        //socket.Send(Encoding.ASCII.GetBytes("0"));
                        socket.Send(Encoding.GetEncoding("IBM037").GetBytes("0"));
                        CloseSocket(socket);
                        break;

                        //socket.Send(Encoding.UTF8.GetBytes("0"));
                    }
                    List<string> workStatus = new List<string>() { string.Empty, "Q" };
                    if (fastNo != null && fastNo.Length > 10)
                        fastNo = fastNo.Substring(0, 10);
                    fastNo = fastNo?.Trim();
                    string TEXT_TYPE = null;
                    using (EacConnection conn = new EacConnection(ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString))
                    {
                        conn.Open();
                        using (EacCommand ec = new EacCommand(conn))
                        {
                            ec.CommandText = $@"select TEXT_TYPE from LRTBARM1 where FAST_NO = :FAST_NO ; ";
                            ec.Parameters.Add("FAST_NO", fastNo);
                            ec.Prepare();
                            DbDataReader result = ec.ExecuteReader();
                            while (result.Read())
                            {
                                TEXT_TYPE = result["TEXT_TYPE"]?.ToString()?.Trim();
                            }
                            ec.Dispose();

                        }
                        conn.Dispose();
                        conn.Close();
                    }
                    if (fastNo != null && !fastNos.Any(x => x == fastNo) && workStatus.Contains(TEXT_TYPE))
                    //if (fastNo != null && !fastNos.Any(x => x == fastNo))
                    {
                        fastNos.Add(fastNo);
                        WriteLog($@"fastNo={fastNo} (加入處理)", Ref.Nlog.Trace);
                        Program.work(fastNo); //執行 主程式
                    }
                    else if (fastNo != null && workStatus.Contains(TEXT_TYPE) && fastNos.Any(x => x == fastNo) )
                    {
                        WriteLog($@"fastNo={fastNo} (已在處理中)", Ref.Nlog.Trace);
                    }
                    else
                    {
                        WriteLog($@"fastNo={fastNo}, text_type={TEXT_TYPE} (檢核失敗) ", Ref.Nlog.Trace);
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    WriteLog($@"SocketException : { se.ToString()}", Ref.Nlog.Error);
                }
                catch (Exception e)
                {
                    Console.WriteLine("外層-錯誤訊息 : {0}", e);
                    WriteLog($@"外層-錯誤訊息:{e}", Ref.Nlog.Error);
                }
                finally
                {
                    CloseSocket(socket);
                }
            }        
        }

        private static object lockitem = new object();

        public static void work(string fastNo)
        {
            lock (lockitem2)
            {
                EacConnection con = new EacConnection();
                try
                {
                    if (fastNo.Length > 10)
                        fastNo = fastNo.Substring(0, 10);

                    //S1
                    WriteStatus(fastNo, XML_STATUS.S1);
                    //if (con.State == System.Data.ConnectionState.Closed)
                    //    con.Open();
                    SendMailFlag = false;
                    SendMailMsg = string.Empty;

                    bool bool_alert1_cnt = true; //判斷第一段是否需要發送
                    bool bool_alert2_cnt = true; //判斷第二段是否需要發送

                    Guid guid = Guid.NewGuid();
                    
                    #region 預設參數
                    DateTime dt = DateTime.MinValue;
                    TimeSpan ts = TimeSpan.MinValue;
                    DateTime dtn = DateTime.Now;
                    ErrorModel model = new ErrorModel(); //要傳送給Api的model
                    int times = 3; //發送次數 預設3有設定就取代
                    decimal alert1 = 200M; //第一段水位警示 預設200有設定就取代
                    decimal alert2 = 300M; //第二段水位警示 預設300有設定就取代
                    dt = dtn.Date;
                    ts = dtn.TimeOfDay;
                    #endregion

                    #region 前置動作
                    using (dbGLEntities db = new dbGLEntities())
                    {
                        var WaterLevels = db.SYS_PARA.Where(x => x.GRP_ID == "WaterLevel").ToList();

                        #region 復原參數
                        var send_time = WaterLevels.FirstOrDefault(x => x.PARA_ID == "send_time");
                        if (dtn.ToString("yyyyMMdd") != send_time?.PARA_VALUE) //更新時間不等於今天 (表示為今天第一筆)
                        {
                            send_time.PARA_VALUE = dtn.ToString("yyyyMMdd"); //更新時間 設定為今天
                            var _day_amt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "day_amt");
                            var _yday_amt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "yday_amt");
                            _yday_amt.PARA_VALUE = _day_amt.PARA_VALUE; //前日總金額 設定為 今日總金額
                            _day_amt.PARA_VALUE = "0"; //今日總金額 歸0
                            var _alert1_cnt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert1_cnt");
                            var _alert2_cnt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert2_cnt");
                            _alert1_cnt.PARA_VALUE = "0";
                            _alert2_cnt.PARA_VALUE = "0";
                            db.SaveChanges();
                        }
                        else //非第一次 需做動作嗎?
                        {

                        }
                        #endregion

                        #region 取參數
                        var _times = WaterLevels.FirstOrDefault(x => x.PARA_ID == "times")?.PARA_VALUE?.stringToIntN();
                        if (_times != null)
                            times = _times.Value;
                        var _alert1_cnt_s = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert1_cnt")?.PARA_VALUE?.stringToIntN();
                        if (_alert1_cnt_s != null && _alert1_cnt_s.Value >= times)
                            bool_alert1_cnt = false;
                        var _alert2_cnt_s = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert2_cnt")?.PARA_VALUE?.stringToIntN();
                        if (_alert2_cnt_s != null && _alert2_cnt_s.Value >= times)
                            bool_alert2_cnt = false;
                        var _alert1 = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert1")?.PARA_VALUE?.stringToDecimalN();
                        if (_alert1 != null)
                            alert1 = _alert1.Value;
                        var _alert2 = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert2")?.PARA_VALUE?.stringToDecimalN();
                        if (_alert2 != null)
                            alert2 = _alert2.Value;
                        #endregion
                    }
                    #endregion


                    string Remit_group = "REMIT_ERR";
                    string _Remit_group = Properties.Settings.Default["Remit_Group"]?.ToString();
                    if (!_Remit_group.IsNullOrWhiteSpace())
                        Remit_group = _Remit_group;
                    var validatResult = new List<ValidationResult>();
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString;
                    con.Open();
                    DateTime _dtn = DateTime.Now;
                    string _dtnStr = _dtn.ToString("yyyy/MM/dd");

                    #region 查詢系統日是否為工作日
                    if (work_date.IsNullOrWhiteSpace() || work_date != _dtnStr)
                    {
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
                            work_date = _dtnStr;
                            Console.WriteLine(string.Empty);
                            ec.Dispose();
                        }
                    }
                    #endregion

                    //工作日做事
                    if (CORP_REST != "Y")
                    {
                        #region 預設參數
                        string BANK_CODE = ""; //銀行代碼
                        string SUB_BANK = ""; //分行代號
                        string BANK_TYPE = "C"; //銀行類型 
                        string TEXT_TYPE = ""; //電文類型 
                        string SETL_ID = "";
                        string SYS_TYPE = string.Empty; //系統別
                        string SRCE_FROM = string.Empty; //資料來源
                        string SRCE_KIND = string.Empty; //資料類別
                        GetData myFunc = XmlToData; //抓取資料事件     
                        TxBody_XM96 XM96 = new TxBody_XM96();
                        TxBody_EACH EACH = new TxBody_EACH();
                        FMPConnectionString FMP = new FMPConnectionString();
                        TxHead Head = new TxHead();
                        Head.HSTANO = dtn.ToString("HHmmssf");
                        string MACData_FAST_NO = "7154151ILI";
                        var _MACData_FAST_NO = Properties.Settings.Default["MACData"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(_MACData_FAST_NO))
                            MACData_FAST_NO = _MACData_FAST_NO;
                        var _REF_NO = Properties.Settings.Default["REF_NO"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(_MACData_FAST_NO))
                            XM96.REF_NO = _REF_NO;
                      
                
                        var workDatas = new List<FRT_WORD>();
                
                        using (dbGLEntities db = new dbGLEntities())
                        {
                            workDatas = db.FRT_WORD.AsNoTracking().ToList();
                        }
                        #endregion
               
                        //判斷銀行類型+電文類別及時間控制            
                
                        #region 先用快速付款編號查出銀行代碼BANK_CODE
                        using (EacCommand ec = new EacCommand(con))
                        {
                            Console.WriteLine(@"先用快速付款編號查出銀行代碼BANK_CODE");
                            Console.WriteLine(string.Empty);
                            //string sql = @"SELECT * FROM FRTBARM1 WHERE 1 = 1 ";
                            string sql = @"SELECT BANK_CODE, SUB_BANK, PAID_ID, FAST_NO, BANK_CODE, BANK_ACT, REMIT_AMT, RCV_NAME , SYS_TYPE , SRCE_FROM , SRCE_KIND FROM LRTBARM1 WHERE 1 = 1 ";
                            sql += @"AND FAST_NO = :FAST_NO ";
                            sql += @" ;";
                            ec.CommandText = sql;
                            ec.Parameters.Add("FAST_NO", fastNo);
                            Console.WriteLine($@"查出銀行代碼 CommandText:{sql}");
                            WriteLog($@"查出銀行代碼 CommandText:{sql}");
                            Console.WriteLine($@"FAST_NO:{fastNo}");
                            WriteLog($@"FAST_NO:{fastNo}");
                            Console.WriteLine(string.Empty);
                            //EASYCOM廠商建議新增此句
                            ec.Prepare();
                            DbDataReader result = ec.ExecuteReader();
                            while (result.Read())
                            {
                                BANK_CODE = result["BANK_CODE"]?.ToString();
                                SUB_BANK = result["SUB_BANK"]?.ToString();
                                Console.WriteLine($@"BANK_CODE:{BANK_CODE}");//DEBUG LINE
                                WriteLog($@"BANK_CODE:{BANK_CODE}");
                                Console.WriteLine($@"SUB_BANK:{SUB_BANK}");//DEBUG LINE
                                WriteLog($@"SUB_BANK:{SUB_BANK}");
                                //匯款日期
                                //var _REMIT_DATE = result["REMIT_DATE"]?.ToString();
                                var _REMIT_DATE = dtn.ToString("yyyyMMdd");
                                XM96.ACT_DATE = _REMIT_DATE;
                                Console.WriteLine($@"匯款日期:{_REMIT_DATE}");
                                WriteLog($@"匯款日期:{_REMIT_DATE}");
                                //受款人ID
                                var _PAID_ID = result["PAID_ID"]?.ToString();
                                //XM96.PAY_IDNO = _PAID_ID;
                                EACH.INIDNO = _PAID_ID;
                                Console.WriteLine($@"受款人ID:{_PAID_ID}");
                                WriteLog($@"受款人ID:{_PAID_ID}");
                                //銀行處理序號(快速付款編號)
                                var _SETL_ID = $@"{MACData_FAST_NO}{result["FAST_NO"]?.ToString()}";
                                SETL_ID = _SETL_ID;
                                //var _Hex_SETL_ID = (BitConverter.ToString(Encoding.Default.GetBytes(_SETL_ID))).Replace("-", "");
                                XM96.SETL_ID = _SETL_ID;
                                Console.WriteLine($@"銀行處理序號(快速付款編號):{_SETL_ID}");
                                WriteLog($@"銀行處理序號(快速付款編號):{_SETL_ID}");
                                //Console.WriteLine($@"銀行處理序號(快速付款編號轉Hex):{_Hex_SETL_ID}");
                                //收款行庫代號
                                var _RCV_BANK = result["BANK_CODE"]?.ToString();
                                //XM96.RCV_BANK = _RCV_BANK;
                                XM96.RCV_BANK = (BANK_CODE + SUB_BANK);
                                Console.WriteLine($@"收款行庫代號:{_RCV_BANK}");
                                WriteLog($@"收款行庫代號:{_RCV_BANK}");
                                //收款人帳號
                                var _ACT = result["BANK_ACT"]?.ToString();
                                XM96.RCV_ACNO = _ACT.PadLeft(16, '0');
                                EACH.TRANSFERINACTNO = _ACT.PadLeft(16, '0');
                                Console.WriteLine($@"收款人帳號:{_ACT}");
                                WriteLog($@"收款人帳號:{_ACT}");
                                //交易金額
                                var _AMT = result["REMIT_AMT"]?.ToString();
                                //XM96.TX_AMT = _AMT.PadLeft(13, '0').PadRight(15, '0');
                                XM96.TX_AMT = _AMT.PadLeft(13, '0').PadRight(15, '0');
                                EACH.TXNAMT = _AMT;
                                Console.WriteLine($@"交易金額:{_AMT}");
                                WriteLog($@"交易金額:{_AMT}");
                                //收款人姓名
                                var _RCV_NAME = result["RCV_NAME"]?.ToString()?.Trim();
                                _RCV_NAME = _RCV_NAME.Substring(0, (_RCV_NAME.Length > 36 ? 36 : _RCV_NAME.Length));
                                XM96.RCV_NAME = _RCV_NAME;
                                Console.WriteLine($@"收款人姓名:{_RCV_NAME}");
                                WriteLog($@"收款人姓名:{_RCV_NAME}");
                                //請求編號
                                var _REQUESTID = result["FAST_NO"]?.ToString();
                                EACH.REQUESTID = _REQUESTID;
                                Console.WriteLine($@"請求編號:{_REQUESTID}");
                                WriteLog($@"請求編號:{_REQUESTID}");

                                SYS_TYPE = result["SYS_TYPE"]?.ToString(); //系統別
                                Console.WriteLine($@"系統別:{SYS_TYPE}");
                                WriteLog($@"系統別:{SYS_TYPE}");
                                SRCE_FROM = result["SRCE_FROM"]?.ToString(); //資料來源
                                Console.WriteLine($@"資料來源:{SRCE_FROM}");
                                WriteLog($@"資料來源:{SRCE_FROM}");
                                SRCE_KIND = result["SRCE_KIND"]?.ToString(); //資料類別
                                Console.WriteLine($@"資料類別:{SRCE_KIND}");
                                WriteLog($@"資料類別:{SRCE_KIND}");
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
                                if (!result["BBKM0_BANK_TYPE"].ToString().IsNullOrWhiteSpace())
                                {
                                    BANK_TYPE = result["BBKM0_BANK_TYPE"]?.ToString();
                                }
                                Console.WriteLine($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                                WriteLog($@"BBKM0_BANK_TYPE:{BANK_TYPE}");
                            }
                            Console.WriteLine(string.Empty);
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
                            WriteStatus(fastNo, XML_STATUS.SW);
                            using (EacCommand ec = new EacCommand(con))
                            {
                                Console.WriteLine("超過傳送時間");
                                WriteLog("超過傳送時間");
                                string sql = $@"
                        update LRTBARM1
                        set TEXT_TYPE = 'W' 
                        where FAST_NO = :FAST_NO ; ";
                                Console.WriteLine($"update sql:{sql}");
                                WriteLog($"update sql:{sql}");
                                ec.CommandText = sql;
                                ec.Parameters.Add("FAST_NO", fastNo);
                                //EASYCOM廠商建議新增此句
                                ec.Prepare();
                                var updateNum = ec.ExecuteNonQuery();
                                Console.WriteLine(string.Empty);
                                Console.WriteLine($"資料異動筆數:{updateNum}");
                                WriteLog($"資料異動筆數:{updateNum}");
                                ec.Dispose();
                            }
                        }
                        #endregion

                        #region 傳送時間內
                        else
                        {
                            #region HSM押碼
                            Console.WriteLine("===HSM押碼開始===");
                            Console.WriteLine(string.Empty);
                
                            string WorkingKeyLabel = $"WorkingKeyforTest_{DateTime.Now.ToString("yyyyMMdd")}";
                            var _year = (dtn.Year - 1911).ToString();
                            string IV = $@"{_year.Substring(_year.Length - 2, 2)}{dtn.ToString("MMdd")}{"0000000000"}"; //民國年 2碼 + MMdd
                                                                                                                        //IV = (BitConverter.ToString(Encoding.ASCII.GetBytes(IV))).Replace("-", "");                                                                                                                        // yyyyMMdd 16碼 左邊補0 + (前面10碼固定加 '7154151ILI' pro) + fastNo                                                                                                                       //string Data = $@"{dtn.ToString("yyyyMMdd")}{(MACData_FAST_NO + fastNo).PadLeft(16, '0')}";
                            string Data = "";
                            if (TEXT_TYPE == "3")
                            {
                                Data = $@"{dtn.ToString("yyyyMMdd")}{XM96.ACNO_OUT}{SETL_ID}{"0000"}";
                            }
                            else
                            {
                                Data = $@"{dtn.ToString("yyyyMMdd")}{EACH.TRANSFEROUTACTNO}{fastNo}{"00000000000000"}";
                            }
                
                            var _Hex_Data = Data.CHREncoding();
                            //Extension.writeTxtLog(_Hex_Data, Path.Combine(Directory.GetCurrentDirectory(), "mytxt2.txt"));
                            //var _Hex_Data = (BitConverter.ToString(Encoding.ASCII.GetBytes(Data))).Replace("-", "");
                            //var _Hex_Data = (BitConverter.ToString(Encoding.Default.GetBytes(Data))).Replace("-", "");
                            //var _Hex_Data = Encoding.ASCII.GetBytes(Data);
                            IResultModel resultModel = null;
                            MACModel MACData = new MACModel();
                            try
                            {
                                var _IV = Properties.Settings.Default["IV"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(_IV))
                                    IV = _IV;
                                using (dbGLEntities db = new dbGLEntities())
                                {
                                    WorkingKeyLabel = db.FRT_WorkingKey.AsNoTracking().FirstOrDefault(x => x.IsActive == "Y")?.WorkingKeyLabel;
                                }
                                var _WorkingKeyLabel = Properties.Settings.Default["WorkingKeyLabel"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(_WorkingKeyLabel))
                                    WorkingKeyLabel = _WorkingKeyLabel;
                                MACData.Data = _Hex_Data;
                                MACData.IV = IV;
                                MACData.WorkingKeyLabel = WorkingKeyLabel;
                                resultModel = new MACResultModel();
                                resultModel = myFunc.Invoke(GetMAC_XML(MACData), resultModel, url.HSMUrl_MAC);
                                using (dbGLEntities db = new dbGLEntities())
                                {
                                    var _result = (MACResultModel)resultModel;
                                    var FM = new FRT_MAC()
                                    {
                                        CreateDate = dt,
                                        CreateTime = ts,
                                        WorkingKeyLabel = MACData.WorkingKeyLabel,
                                        Data = MACData.Data,
                                        IV = MACData.IV,
                                        ErrorCode = _result?.ErrorCode,
                                        ErrorMessage = _result?.ErrorMsg,
                                        MAC = _result?.MAC
                                    };
                                    db.FRT_MAC.Add(FM);
                                    try
                                    {
                                        XM96.MAC = FM.MAC;
                                        EACH.MAC = FM.MAC;
                                        //要存資料庫請拿掉下方註解
                                        db.SaveChanges();
                                        var _FM = db.FRT_MAC.AsNoTracking().FirstOrDefault(x =>
                                            x.CreateDate == dt &&
                                            x.CreateTime == ts);
                                        Console.WriteLine($"新增資料 : {FM.modelToString()}");
                                        WriteLog($"HSM押碼開始新增資料 : {FM.modelToString()}");
                                    }
                                    catch (DbUpdateException ex)
                                    {
                                        var _msg = $"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}";
                                        _result.ErrorMsg = _msg;
                                        Console.WriteLine(_msg);
                                        WriteLog($"{_msg}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var _msg = $"錯誤訊息 : InnerException:{ex.InnerException},Message:{ex.Message}";
                                Console.WriteLine(_msg);
                                WriteLog($"{_msg}");
                            }
                            finally
                            {
                                Console.WriteLine("===HSM押碼結束===");
                                WriteLog("===HSM押碼結束===");
                                Console.WriteLine(string.Empty);
                            }
                
                            #region SYNC
                            resultModel = new SYNCResultModel();
                            resultModel = myFunc.Invoke(GetSYNC_XML(MACData), resultModel, url.HSMUrl_SYNC);
                            var _SYNCresult = (SYNCResultModel)resultModel;
                            XM96.SYNC = _SYNCresult.SyncValue;
                            Console.WriteLine($"SYNC : {_SYNCresult.SyncValue}");
                            #endregion
                
                            #endregion
                
                            #region 電文傳送
                
                            var _BARM0_REMIT_DATE = $"{(dtn.Year - 1911)}{dtn.ToString("MMdd")}";
                            SRCE_KIND = SRCE_KIND.IsNullOrWhiteSpace() ? string.Empty : SRCE_KIND;
                            var _workData = workDatas.FirstOrDefault(z => z.frt_sys_type == SYS_TYPE && z.frt_srce_from == SRCE_FROM && z.frt_srce_kind == SRCE_KIND);
                            string _mailmsg = string.Empty;
                            using (dbGLEntities db = new dbGLEntities())
                            {
                                string _Head = string.Empty;
                                string _Error = string.Empty;
                                switch (TEXT_TYPE) //電文類型
                                {
                                    #region XM96
                                    case "3": //XM96   
                                                                                                                
                                        #region default value
                                        FMP.TxnId = "FPS622821";
                                        Head.HTLID = "7154151";
                                        Head.HTXTID = "FPS622821";
                                        XM96.APX = (string.IsNullOrWhiteSpace(_workData?.frt_memo_apx)) ? (string.Empty.PadRight(3,'　')) : (_workData.frt_memo_apx.Trim().PadRight(3, '　'));
                                        if (_workData == null || string.IsNullOrWhiteSpace(_workData.frt_memo_apx))
                                        {
                                            _mailmsg += $@"快速付款編號:{fastNo} 存摺顯示字樣無對應資料，請確認";
                                        }
                                        //XM96.APX = "借款　";
                                        //XM96.TRAN_ID = "XM96";
                                        //XM96.TX_RMK = "1";
                                        //XM96.ACT_DATE = "20181003";
                                        //XM96.PAY_IDNO = "27935073";
                                        //XM96.NAME_COD = "0001";
                                        //XM96.SETL_ID = "7154151ILIQ606130023";
                                        //XM96.PAY_BANK = "0127152";
                                        //XM96.ACNO_OUT = "0000737102001802";
                                        //XM96.RCV_BANK = "8220015";
                                        //XM96.RCV_ACNO = "0000012345678901";
                                        //XM96.TX_AMT = "10000";
                                        //XM96.FEE_COD = "13";
                                        //XM96.REF_NO = "00000000000000N                     ";
                                        //XM96.REF_NO = "";
                                        //XM96.PAY_NAME = "FubonLife";
                                        //XM96.RCV_NAME = "TestMan";

                                        XM96.ICV = $@"{_year.Substring(_year.Length - 2, 2)}{dtn.ToString("MMdd")}"; //民國年 2碼 + MMdd
                                        #endregion

                                        #region 檢核欄位

                                        var contextXM96 = new ValidationContext(XM96, null, null);
                                 
                                        //檢核失敗
                                        if (!Validator.TryValidateObject(XM96, contextXM96, validatResult, true))
                                        {
                                            var pros = XM96.GetType()
                                                .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                                            StringBuilder checkMsg = new StringBuilder();
                                            checkMsg.AppendLine("XM96欄位檢核失敗:");
                                            validatResult.ForEach(x =>
                                            {
                                                var m = x.MemberNames.FirstOrDefault()?.ToString();
                                                var p = pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                                var val = (p == null) ? null : (p.GetValue(XM96))?.ToString();
                                                if (!val.IsNullOrWhiteSpace())
                                                    checkMsg.AppendLine((m + " : " + val) + " Error : " + x.ErrorMessage);
                                                else
                                                    checkMsg.AppendLine(x.ErrorMessage);
                                            });
                                            WriteLog(checkMsg.ToString(), Ref.Nlog.Error);
                                            sendMail(fastNo, mailType.XML, Remit_group, checkMsg.ToString());
                                        }
                                        //檢核成功
                                        else
                                        {
                                            #region XM96 傳送資料
                                            _Head = GetXM96_XML(XM96, FMP, Head);
                                            FRT_XML_T_622821 _T_622821 = XM96.ModelConvert<TxBody_XM96, FRT_XML_T_622821>();
                                            _T_622821.TX_AMT = _T_622821.TX_AMT.stringToDecimalN().fixedAMT()?.ToString();
                                            _T_622821.HEADER = _Head;
                                            _T_622821.CRT_TIME = DateTime.Now;
                                            _T_622821.FAST_NO = fastNo;
                                            _T_622821.GUID = guid;
                                            db.FRT_XML_T_622821.Add(_T_622821);
                                            Console.WriteLine($"===新增622821(XM96)傳送資料===");
                                            Console.WriteLine(string.Empty);
                                            _Error = db.GetValidationErrors().getValidateString();
                                            if (_Error.IsNullOrWhiteSpace())
                                            {
                                                try
                                                {
                                                    db.SaveChanges();
                                                    Console.WriteLine($"新增622821(XM96)傳送資料 : {_T_622821.modelToString()}");
                                                    WriteLog($"新增622821(XM96)傳送資料 : {_T_622821.modelToString()}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"新增622821(XM96)傳送資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                    WriteLog($"新增622821(XM96)傳送資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"欄位驗證錯誤:{_Error}");
                                                WriteLog($"欄位驗證錯誤:{_Error}");
                                            }
                                            Console.WriteLine(string.Empty);

                                            #endregion

                                            #region 修改400狀態(已傳送) 並傳送給銀行
                                            resultModel = new FRT_XML_R_622821Model();
                                            save400_1(
                                                con,
                                                fastNo,
                                                _BARM0_REMIT_DATE,
                                                "2", //已匯款
                                                "Y", //已傳送
                                                _BARM0_REMIT_DATE,
                                                DateTime.Now.ToString("HHmmssff"),
                                                TEXT_TYPE
                                                );
                                            //S2
                                            WriteStatus(fastNo, XML_STATUS.S2);
                                            resultModel = myFunc.Invoke(_Head, resultModel, url.FubonBankUrl_XM96);
                                            //S3
                                            WriteStatus(fastNo, XML_STATUS.S3);
                                            #endregion

                                            #region XM96 接收資料
                                            var _resultXM96Model = (FRT_XML_R_622821Model)resultModel;
                                            _resultXM96Model.TX_AMT = _resultXM96Model.TX_AMT.stringToDecimalN().fixedAMT()?.ToString();
                                            FRT_XML_R_622821 _R_622821 = _resultXM96Model.ModelConvert<FRT_XML_R_622821Model, FRT_XML_R_622821>();
                                            _R_622821.HEADER = _resultXM96Model.TxHead;
                                            _R_622821.CRT_TIME = DateTime.Now;
                                            _R_622821.FAST_NO = fastNo;
                                            _R_622821.GUID = guid;
                                            db.FRT_XML_R_622821.Add(_R_622821);
                                            Console.WriteLine($"===新增622821(XM96)接收資料===");
                                            Console.WriteLine(string.Empty);
                                            _Error = db.GetValidationErrors().getValidateString();
                                            if (_Error.IsNullOrWhiteSpace())
                                            {
                                                try
                                                {
                                                    db.SaveChanges();
                                                    Console.WriteLine($"新增622821(XM96)接收資料 : {_R_622821.modelToString()}");
                                                    WriteLog($"新增622821(XM96)接收資料 : {_R_622821.modelToString()}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"新增622821(XM96)接收資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                    WriteLog($"新增622821(XM96)接收資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"欄位驗證錯誤:{_Error}");
                                                WriteLog($"欄位驗證錯誤:{_Error}");
                                            }
                                            #endregion

                                            #region 水位判斷
                                            overAMT(_R_622821.TX_AMT.stringToDecimal(), alert1, alert2, times, bool_alert1_cnt, bool_alert2_cnt);
                                            #endregion

                                            #region 修改400資料(已接收)
                                            save400_2(
                                                con,
                                                fastNo,
                                                "Y", //已接收
                                                _BARM0_REMIT_DATE,
                                                DateTime.Now.ToString("HHmmssff"));
                                            #endregion

                                            #region ErrorCode  呼叫 Web Api

                                            if (!(_resultXM96Model.RC_CODE == "0000"))
                                            {
                                                model = new ErrorModel()
                                                {
                                                    ExecType = "F",
                                                    Fast_No = fastNo,
                                                    ErrorCode = _R_622821.RC_CODE == null ? _R_622821.EMSGID : _R_622821.RC_CODE,
                                                    EMSGTXT = _R_622821.EMSGTXT,
                                                    TextType = TEXT_TYPE
                                                };
                                                callApi(model);
                                            }
                                            else
                                            {
                                                //SS
                                                WriteStatus(fastNo, XML_STATUS.SS);
                                                //model = new ErrorModel()
                                                //{
                                                //    ExecType = "S",
                                                //    Fast_No = fastNo,
                                                //    ErrorCode = _R_622821.RC_CODE,
                                                //    EMSGTXT = null,
                                                //    TextType = TEXT_TYPE
                                                //};
                                                //callApi(model);
                                            }
                                            #endregion
                                        }
                                        #endregion

                                        break;
                                    #endregion
                
                                    #region EACH
                                    case "1":
                                    case "2": //EACH

                                        #region  default value
                                        //if (TEXT_TYPE == "1")                                       

                                        if (_workData != null)
                                        {
                                            EACH.MEMO = XM96.APX = (string.IsNullOrWhiteSpace(_workData?.frt_memo_apx)) ? (string.Empty.PadRight(3, '　')) : (_workData.frt_memo_apx.Trim().PadRight(3, '　'));                                      
                                            if (!string.IsNullOrWhiteSpace(_workData.frt_achcode))
                                                EACH.ACHCODE = _workData?.frt_achcode?.Trim();
                                            else
                                                _mailmsg += $@"快速付款編號:{fastNo} eACH交易代號無對應資料，請確認";
                                            if (string.IsNullOrWhiteSpace(_workData.frt_memo_apx))
                                                _mailmsg += $@"快速付款編號:{fastNo} 存摺顯示字樣無對應資料，請確認";
                                        }
                                        else
                                        {
                                            _mailmsg += $@"快速付款編號:{fastNo} eACH交易代號無對應資料，請確認";
                                            _mailmsg += System.Environment.NewLine;
                                            _mailmsg += $@"快速付款編號:{fastNo} 存摺顯示字樣無對應資料，請確認";
                                        }
                                            
                                        FMP.TxnId = "FEPEACH001";
                                        Head.HTXTID = "FEPEACH001";
                                        //Head.HSTANO = "0000099"; //註解
                                        Head.HTLID = "7154151";
                                        //EACH.REQUESTID = "Q070800002"; //註解
                                        EACH.SEQNO = "0000";
                                        //EACH.TXNCODE = "TF05"; //註解
                                        //EACH.ACHCODE = "351"; 
                                        //EACH.TXNAMT = "3800"; //註解
                                        EACH.TXNINBANK = (BANK_CODE + SUB_BANK);
                                        //EACH.TXNINBANK = "0122209"; //註解
                                        //EACH.TXNOUTBANK = "0122209";
                                        //EACH.TRANSFERINACTNO = "0000200168022823";
                                        //EACH.TRANSFEROUTACTNO = "0000737102710668";
                                        //EACH.INIDNO = "F223369467"; //註解
                                        //EACH.OUTIDNO = "27935073";
                                        #endregion

                                        #region 檢核欄位

                                        var contextEACH = new ValidationContext(EACH, null, null);

                                        //檢核失敗
                                        if (!Validator.TryValidateObject(EACH, contextEACH, validatResult, true))
                                        {
                                            var pros = EACH.GetType()
                                                .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                                            StringBuilder checkMsg = new StringBuilder();
                                            checkMsg.AppendLine("EACH欄位檢核失敗:");
                                            validatResult.ForEach(x =>
                                            {
                                                var m = x.MemberNames.FirstOrDefault()?.ToString();
                                                var p = pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                                var val = (p == null) ? null : (p.GetValue(XM96))?.ToString();
                                                if (!val.IsNullOrWhiteSpace())
                                                    checkMsg.AppendLine((m + " : " + val) + " Error : " + x.ErrorMessage);
                                                else
                                                    checkMsg.AppendLine(x.ErrorMessage);
                                            });
                                            WriteLog(checkMsg.ToString(), Ref.Nlog.Error);
                                            sendMail(fastNo, mailType.XML, Remit_group, checkMsg.ToString());
                                        }
                                        //檢核成功
                                        else
                                        {
                                            #region EACH 傳送資料
                                            resultModel = new FRT_XML_R_eACHModel();
                                            _Head = GetEACH_XML(EACH, FMP, Head);
                                            FRT_XML_T_eACH _T_Eeah = EACH.ModelConvert<TxBody_EACH, FRT_XML_T_eACH>();
                                            _T_Eeah.HEADER = _Head;
                                            _T_Eeah.CRT_TIME = DateTime.Now;
                                            _T_Eeah.FAST_NO = fastNo;
                                            _T_Eeah.GUID = guid;
                                            db.FRT_XML_T_eACH.Add(_T_Eeah);
                                            Console.WriteLine($"===新增EACH傳送資料===");
                                            Console.WriteLine(string.Empty);
                                            _Error = db.GetValidationErrors().getValidateString();
                                            if (_Error.IsNullOrWhiteSpace())
                                            {
                                                try
                                                {
                                                    db.SaveChanges();
                                                    Console.WriteLine($"新增EACH傳送資料 : {_T_Eeah.modelToString()}");
                                                    WriteLog($"新增EACH傳送資料 : {_T_Eeah.modelToString()}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"新增EACH傳送資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                    WriteLog($"新增EACH傳送資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"欄位驗證錯誤:{_Error}");
                                                WriteLog($"欄位驗證錯誤:{_Error}");
                                            }
                                            #endregion

                                            #region 修改400狀態(已傳送) 並傳送給銀行
                                            save400_1(
                                                con,
                                                fastNo,
                                                _BARM0_REMIT_DATE,
                                                "2", //已匯款
                                                "Y", //已傳送
                                                _BARM0_REMIT_DATE,
                                                dtn.ToString("HHmmssff"),
                                                TEXT_TYPE);
                                            //S2
                                            WriteStatus(fastNo, XML_STATUS.S2);
                                            resultModel = myFunc.Invoke(_Head, resultModel, url.FubonBankUrl_EACH);
                                            //S3
                                            WriteStatus(fastNo, XML_STATUS.S3);
                                            #endregion

                                            #region EACH 接收資料
                                            var _resulEACHtModel = (FRT_XML_R_eACHModel)resultModel;
                                            FRT_XML_R_eACH _R_Eeah = _resulEACHtModel.ModelConvert<FRT_XML_R_eACHModel, FRT_XML_R_eACH>();
                                            _R_Eeah.HEADER = _resulEACHtModel.TxHead;
                                            _R_Eeah.CRT_TIME = DateTime.Now;
                                            _R_Eeah.INIDNO = _R_Eeah.INIDNO?.Substring(0, ((_R_Eeah.INIDNO.Length > 10) ? 10 : _R_Eeah.INIDNO.Length));
                                            _R_Eeah.OUTIDNO = _R_Eeah.OUTIDNO?.Substring(0, ((_R_Eeah.OUTIDNO.Length > 10) ? 10 : _R_Eeah.OUTIDNO.Length));
                                            _R_Eeah.FAST_NO = fastNo;
                                            _R_Eeah.GUID = guid;
                                            db.FRT_XML_R_eACH.Add(_R_Eeah);
                                            Console.WriteLine($"===新增EACH接收資料===");
                                            Console.WriteLine(string.Empty);

                                            _Error = db.GetValidationErrors().getValidateString();
                                            if (_Error.IsNullOrWhiteSpace())
                                            {
                                                try
                                                {
                                                    db.SaveChanges();
                                                    Console.WriteLine($"新增EACH接收資料 : {_R_Eeah.modelToString()}");
                                                    WriteLog($"新增EACH接收資料 : {_R_Eeah.modelToString()}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"新增EACH接收資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                    WriteLog($"新增EACH接收資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"欄位驗證錯誤:{_Error}");
                                                WriteLog($"欄位驗證錯誤:{_Error}");
                                            }
                                            #endregion

                                            #region 水位判斷
                                            overAMT(_R_Eeah.TXNAMT.stringToDecimal(), alert1, alert2, times, bool_alert1_cnt, bool_alert2_cnt);
                                            #endregion

                                            #region 判斷錯誤
                                            string errorFlag = "N";
                                            if (!(_resulEACHtModel.ERROR_CODE == "0000"))
                                            {
                                                errorFlag = "Y";
                                            }
                                            #endregion

                                            #region 修改400資料(已接收)
                                            save400_2(
                                                con,
                                                fastNo,
                                                "Y", //已接收
                                                _BARM0_REMIT_DATE,
                                                DateTime.Now.ToString("HHmmssff"),
                                                errorFlag);
                                            #endregion

                                            #region 呼叫 Web Api

                                            if (errorFlag == "Y")
                                            {
                                                model = new ErrorModel()
                                                {
                                                    ExecType = "F",
                                                    Fast_No = fastNo,
                                                    ErrorCode = _R_Eeah.ERROR_CODE == null ? _R_Eeah.EMSGID : _R_Eeah.ERROR_CODE,
                                                    EMSGTXT = _R_Eeah.EMSGTXT,
                                                    TextType = TEXT_TYPE
                                                };
                                                callApi(model);
                                            }
                                            else
                                            {
                                                model = new ErrorModel()
                                                {
                                                    ExecType = "S",
                                                    Fast_No = fastNo,
                                                    ErrorCode = _R_Eeah.ERROR_CODE,
                                                    EMSGTXT = null,
                                                    TextType = TEXT_TYPE
                                                };
                                                callApi(model);

                                            }
                                            #endregion
                                        }
                                        #endregion

                                        break;
                                        #endregion
                                }
                            }
                            if (!_mailmsg.IsNullOrWhiteSpace())
                            {
                                sendMail(fastNo, mailType.WORK, Remit_group, _mailmsg);
                                //sendMail(fastNo, mailType.WORK, "REMIT_ERR", $@"條件 SYS_TYPE:{SYS_TYPE} , SRCE_FROM:{SRCE_FROM} , SRCE_KIND:{SRCE_KIND} 查詢不到資料.");
                            }
                            #endregion
                        }
                            #endregion
                    }
                    else //非工作日
                    {
                        //SW
                        WriteStatus(fastNo, XML_STATUS.SW);
                        using (EacCommand ec = new EacCommand(con))
                        {
                            Console.WriteLine("非工作日處理");
                            WriteLog("非工作日處理");
                            string sql = $@"
                                     update LRTBARM1
                                     set TEXT_TYPE = 'W' 
                                     where FAST_NO = :FAST_NO ; ";
                            Console.WriteLine($"update sql:{sql}");
                            WriteLog($"update sql:{sql}");
                            ec.CommandText = sql;
                            ec.Parameters.Add("FAST_NO", fastNo);
                            //EASYCOM廠商建議新增此句
                            ec.Prepare();
                            var updateNum = ec.ExecuteNonQuery();
                            Console.WriteLine(string.Empty);
                            Console.WriteLine($"資料異動筆數:{updateNum}");
                            WriteLog($"資料異動筆數:{updateNum}");
                            ec.Dispose();
                        }
                    }
                }
                catch (Exception e)
                {                   
                    SendMailFlag = true;
                    var msg = $"錯誤訊息 : InnerException:{e.InnerException},Message{e.Message}";
                    SendMailMsg += System.Environment.NewLine;
                    SendMailMsg += msg;
                    WriteLog($@"錯誤訊息:{e}", Ref.Nlog.Error);
                }
                finally
                {
                    try
                    {
                        fastNos.Remove(fastNo);
                        WriteLog($@"fastNo={fastNo} (處理結束)", Ref.Nlog.Trace);
                        if (SendMailFlag)
                        {
                            string Sys_Error = "SYS_ERROR";
                            string _Sys_Error = Properties.Settings.Default["System_Group"]?.ToString();
                            if (!_Sys_Error.IsNullOrWhiteSpace())
                                Sys_Error = _Sys_Error;
                            sendMail(fastNo, mailType.XML, Sys_Error);
                        }
                        con.Dispose();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        WriteLog($@"寄信錯誤訊息:{ex}", Ref.Nlog.Error);
                    }
                    //con.Close();
                }
                               
                //將字串以UTF8編碼存入緩衝區
                //byte[] dataS = Encoding.UTF8.GetBytes(test);              
                //socket.Send(dataS);
                //con.Close();
                //con = null;
            }
        }

        #region Other

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
	        } 
	        catch (Exception e) 
	        {
                WriteLog($@"CloseSocket_Error : {e.ToString()}" , Ref.Nlog.Error);
	            //Console.WriteLine(e.Message); 
	        } 
	    } 

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="successFlag"></param>
        /// <returns></returns>
        protected static int sendMail(string fastNo , mailType type = mailType.XML, string groupCode = "SYS_ERROR", string msg = null)
        {
            int result = 0;
            DateTime dn = DateTime.Now;
            string dtn = $@"{(dn.Year - 1911)}/{dn.ToString("MM/dd")}";
            var emps = new List<V_EMPLY2>();
            var depts = new List<VW_OA_DEPT>();
            var sub = string.Empty;
            var body = string.Empty;
            switch (type)
            {
                case mailType.XML:
                    sub = $@"{dtn} 快速付款 FRT_XML fastNo:{fastNo} 失敗通知";
                    body = $@"錯誤訊息:{SendMailMsg}";
                    break;
                case mailType.WORK:
                    sub = $@"存摺顯示字樣& eACH交易代號無對應資料";
                    body = msg;
                    break;
                case mailType.SOCKET:
                    sub = $@"Socket (監聽器) 錯誤通知";
                    body = msg;
                    break;
            }
            var _msg = string.Empty;
            Dictionary<string, Tuple<string, string>> mailToList = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> ccToList = new Dictionary<string, Tuple<string, string>>();
            //mailToList.Add("SYS", new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"));
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
                    mailToList.Add("FRTXml", new Tuple<string, string>(Properties.Settings.Default["mailAccount"]?.ToString(), "FRTXml"));
                if (mailToList.Values.Any())
                {
                    var sms = new SendMail.SendMailSelf();
                    sms.smtpPort = 25;
                    sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                    sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                    sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                    _msg = sms.Mail_Send(
                        new Tuple<string, string>(sms.mailAccount, "FRTXml"),
                        mailToList.Values.AsEnumerable(),
                        ccToList.Any() ? ccToList.Values.AsEnumerable() : null,
                        sub,
                        body
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
                    using (dbGLEntities db = new dbGLEntities())
                    {
                        foreach (var mail in mailToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = mail.Key,
                                EMAIL = mail.Value.Item1,
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0,250) : _msg,
                                MAIL_SUB = sub
                            });
                        }
                        foreach (var cc in ccToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = cc.Key,
                                EMAIL = cc.Value.Item1,
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0, 250) : _msg,
                                MAIL_SUB = sub
                            });
                        }
                        db.SaveChanges();
                    }
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
                db.SaveChanges();
            }
            else
            {
                seqNo = $@"{date}{seq.CURR_VALUE.ToString().PadLeft(5, '0')}";
                seq.CURR_VALUE = seq.CURR_VALUE + 1;
            }
            return Convert.ToInt64(seqNo);
        }

        /// <summary>
        /// 錯誤時呼叫 Api 
        /// </summary>
        /// <param name="model">傳送的Data</param>
        protected static void callApi(ErrorModel model)
        {
            try {

                //S4
                WriteStatus(model.Fast_No, XML_STATUS.S4);
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"開始傳送API");
                    WriteLog($"開始傳送API");
                    // 指定 authorization header
                    client.DefaultRequestHeaders.Add("authorization", "token = 6D9310E55EB72CA5D7BBC8F98DD517BC");
                    client.DefaultRequestHeaders.Add("token", "6D9310E55EB72CA5D7BBC8F98DD517BC");
                    // 將 data 轉為 json
                    string json = JsonConvert.SerializeObject(model);
                    // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                    // 發出 post 並取得結果
                    var _callApiUrl = Properties.Settings.Default["callApiUrl"]?.ToString();
                    HttpResponseMessage response = client.PostAsync(_callApiUrl, contentPost).Result;
                    //HttpResponseMessage response = client.PostAsync("http://GL.fubonlife.com.tw:8096/FastError/RemittanceFailureNotice/", contentPost).Result;
                    // 將回應結果內容取出並轉為 string 再透過 linqpad 輸出
                    var result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                    WriteLog($"{result}");
                }
                //SS
                WriteStatus(model.Fast_No, XML_STATUS.SS);
            }
            catch (Exception ex)
            {
                SendMailFlag = true;
                var msg = $"傳送API失敗 : InnerException:{ex.InnerException},Message{ex.Message}";
                Console.WriteLine(msg);
                WriteLog(msg);
                SendMailMsg += System.Environment.NewLine;
                SendMailMsg += msg;
            }
        }

        /// <summary>
        /// 水位告警MAIL
        /// </summary>
        /// <param name="model"></param>
        protected static void callWaterNotifyApi(FastMailModel model)
        {
            model.groupCode = "REMIT_ERR";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"開始傳送WaterNotifyApi");
                    // 指定 authorization header
                    client.DefaultRequestHeaders.Add("authorization", "token = 6D9310E55EB72CA5D7BBC8F98DD517BC");
                    client.DefaultRequestHeaders.Add("token", "6D9310E55EB72CA5D7BBC8F98DD517BC");
                    // 將 data 轉為 json
                    string json = JsonConvert.SerializeObject(model);
                    // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                    // 發出 post 並取得結果
                    var _WaterNotifyUrl = Properties.Settings.Default["WaterNotifyUrl"]?.ToString();
                    HttpResponseMessage response = client.PostAsync(_WaterNotifyUrl, contentPost).Result;
                    // 將回應結果內容取出並轉為 string 再透過 linqpad 輸出
                    var result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                    WriteLog($"開始傳送WaterNotifyApi {result}");
                }

            }
            catch (Exception ex)
            {
                SendMailFlag = true;
                var msg = $"傳送WaterNotifyApi失敗 : InnerException:{ex.InnerException},Message{ex.Message}";
                Console.WriteLine(msg);
                WriteLog(msg);
                SendMailMsg += System.Environment.NewLine;
                SendMailMsg += msg;
            }
        }


        /// <summary>
        /// 判斷超過水位警示
        /// </summary>
        /// <param name="amt">匯款金額</param>
        /// <param name="alert1">第一段水位警示</param>
        /// <param name="alert2">第二段水位警示</param>
        /// <param name="time">發送次數</param>
        protected static void overAMT(decimal amt, decimal alert1, decimal alert2, int time, bool bool_alert1_cnt,bool bool_alert2_cnt)
        {
            lock (lockitem)
            {
                Console.WriteLine($"=====匯款警告水位判斷=====");
                Console.WriteLine(string.Empty);
                Console.WriteLine($"匯款金額:{amt}");
                Console.WriteLine($"第一段水位警示:{alert1}");
                Console.WriteLine($"第二段水位警示:{alert2}");
                Console.WriteLine($"最多發送次數:{time}");
                WriteLog($"=====匯款警告水位判斷=====");
                WriteLog($"匯款金額:{amt}");
                WriteLog($"第一段水位警示:{alert1}");
                WriteLog($"第二段水位警示:{alert2}");
                WriteLog($"最多發送次數:{time}");
                using (dbGLEntities db = new dbGLEntities())
                {
                    var WaterLevels = db.SYS_PARA.Where(x => x.GRP_ID == "WaterLevel").ToList();
                    var _alert1_cnt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert1_cnt");
                    var _alert2_cnt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "alert2_cnt");
                    var _day_amt = WaterLevels.FirstOrDefault(x => x.PARA_ID == "day_amt");
                    if (_day_amt != null)
                    {
                        decimal _day_amt_val = _day_amt.PARA_VALUE.stringToDecimal();
                        Console.WriteLine($"當日總金額:{_day_amt_val}");
                        WriteLog($"當日總金額:{_day_amt_val}");
                        _day_amt_val += amt;
                        _day_amt.PARA_VALUE = _day_amt_val.ToString(); //更新當日總金額
                        Console.WriteLine($"更新當日總金額:{_day_amt_val}");
                        WriteLog($"更新當日總金額:{_day_amt_val}");
                        Console.WriteLine(string.Empty);
                        if (bool_alert1_cnt)
                        {
                            var _alert1_cnt_val = _alert1_cnt.PARA_VALUE.stringToInt(); //目前第一段發送次數
                            bool alert1Flag = false;
                            //當今日總金額 大於等於 第一段水位警示 且 第一段發送次數 未達到 發送次數
                            if (_day_amt_val >= alert1 && _alert1_cnt != null && _alert1_cnt_val < time)
                                alert1Flag = true;
                            var alert1FlagStr = alert1Flag ? "是" : "否";
                            Console.WriteLine($"當今日總金額:{_day_amt_val} >= 第一段水位警示:{alert1} 且 第一段發送次數:{_alert1_cnt_val} < 發送次數:{time} => 判斷:{alert1FlagStr}");
                            WriteLog($"當今日總金額:{_day_amt_val} >= 第一段水位警示:{alert1} 且 第一段發送次數:{_alert1_cnt_val} < 發送次數:{time} => 判斷:{alert1FlagStr}");
                            Console.WriteLine(string.Empty);
                            if (alert1Flag)
                            {
                                Console.WriteLine($"第一段水位警示寄信");
                                Console.WriteLine(string.Empty);
                                //發送mail
                                FastMailModel model = new FastMailModel();
                                model.reserve1 = "1";
                                callWaterNotifyApi(model);

                                _alert1_cnt.PARA_VALUE = (_alert1_cnt_val += 1).ToString(); //更新 第一段發送次數 + 1
                            }
                        }
                        if (bool_alert2_cnt)
                        {
                            var _alert2_cnt_val = _alert2_cnt.PARA_VALUE.stringToInt(); //目前第二段發送次數
                            bool alert2Flag = false;
                            //當今日總金額 大於等於 第二段水位警示 且 第二段發送次數 未達到 發送次數
                            if (_day_amt_val >= alert2 && _alert2_cnt != null && _alert2_cnt_val < time)
                                alert2Flag = true;
                            var alert2FlagStr = alert2Flag ? "是" : "否";
                            Console.WriteLine($"當今日總金額:{_day_amt_val} >= 第二段水位警示:{alert2} 且 第二段發送次數:{_alert2_cnt_val} < 發送次數:{time} => 判斷:{alert2FlagStr}");
                            WriteLog($"當今日總金額:{_day_amt_val} >= 第二段水位警示:{alert2} 且 第二段發送次數:{_alert2_cnt_val} < 發送次數:{time} => 判斷:{alert2FlagStr}");
                            Console.WriteLine(string.Empty);
                            if (alert2Flag)
                            {
                                Console.WriteLine($"第二段水位警示寄信");
                                Console.WriteLine(string.Empty);
                                //發送mail
                                FastMailModel model = new FastMailModel();
                                model.reserve1 = "2";
                                callWaterNotifyApi(model);

                                _alert2_cnt.PARA_VALUE = (_alert2_cnt_val += 1).ToString(); //更新 第二段發送次數 + 1
                            }
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            SendMailFlag = true;
                            var msg = $"警告水更新資料失敗 : InnerException:{ex.InnerException},Message{ex.Message}";
                            Console.WriteLine(msg);
                            WriteLog(msg);
                            SendMailMsg += System.Environment.NewLine;
                            SendMailMsg += msg;
                        }
                    }
                }
                Console.WriteLine($"=====匯款警告水位判斷結束=====");
            }
        }

        /// <summary>
        /// 傳送前寫400
        /// </summary>
        /// <param name="BARM0_FAST_NO">快速付款編號</param>
        /// <param name="BARM0_REMIT_DATE">快速付款日期</param>
        /// <param name="BARM0_REMIT_STAT">快速付款狀態</param>
        /// <param name="BARM0_TEXT_SND">快速付款電文狀態</param>
        /// <param name="BARM0_TEXT_SNDDT">快速付款電文傳送日期</param>
        /// <param name="BARM0_TEXT_SNDTM">快速付款電文傳送時間</param>
        protected static void save400_1(
            EacConnection con,
            string BARM0_FAST_NO,
            string BARM0_REMIT_DATE,
            string BARM0_REMIT_STAT,
            string BARM0_TEXT_SND,
            string BARM0_TEXT_SNDDT,
            string BARM0_TEXT_SNDTM,
            string BARM0_TEXT_TYPE
            )
        {
            try
            {
                using (EacCommand ec = new EacCommand(con))
                {
                    Console.WriteLine($"===更新400資料(傳送前)===");
                    Console.WriteLine(string.Empty);

                    #region Parameters
                    //string sql = $@"
                    //update LRTBARM1
                    //set REMIT_DATE = :BARM0_REMIT_DATE ,
                    //    REMIT_STAT = 2 ,
                    //    TEXT_CNT =  (int(TEXT_CNT)+1) ,
                    //    TEXT_SND = :BARM0_TEXT_SND ,
                    //    TEXT_SNDDT = :BARM0_TEXT_SNDDT ,
                    //    TEXT_SNDTM = :BARM0_TEXT_SNDTM
                    //where FAST_NO = :BARM0_FAST_NO ; ";
                    #endregion

                    #region injection
                    string sql = $@"
                    update LRTBARM1
                    set REMIT_DATE = '{BARM0_REMIT_DATE}' ,
                        REMIT_STAT = '2' ,
                        FAIL_CODE = '',
                        TEXT_CNT =  (int(TEXT_CNT)+1) ,
                        TEXT_SND = '{BARM0_TEXT_SND}' ,
                        TEXT_SNDDT = '{BARM0_TEXT_SNDDT}' ,
                        TEXT_SNDTM = '{BARM0_TEXT_SNDTM}' ,
                        TEXT_TYPE = '{BARM0_TEXT_TYPE}'
                    where FAST_NO = '{BARM0_FAST_NO}' ; ";
                    #endregion


                    //Extension.writeTxtLog(sql, Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_FAST_NO:{BARM0_FAST_NO}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_REMIT_DATE:{BARM0_REMIT_DATE}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_REMIT_STAT:{BARM0_REMIT_STAT}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_SND:{BARM0_TEXT_SND}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_SNDDT:{BARM0_TEXT_SNDDT}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_SNDTM:{BARM0_TEXT_SNDTM}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_TYPE:{BARM0_TEXT_TYPE}", Path.Combine(Directory.GetCurrentDirectory(), "update1.txt"));
                    Console.WriteLine($"update sql:{sql}");
                    WriteLog($"更新400資料(傳送前)update sql:{sql}");
                    Console.WriteLine(string.Empty);
                    ec.CommandText = sql;
                    ec.Parameters.Add("BARM0_FAST_NO", BARM0_FAST_NO);
                    ec.Parameters.Add("BARM0_REMIT_DATE", BARM0_REMIT_DATE);
                    //ec.Parameters.Add("BARM0_REMIT_STAT", BARM0_REMIT_STAT);
                    ec.Parameters.Add("BARM0_TEXT_SND", BARM0_TEXT_SND);
                    ec.Parameters.Add("BARM0_TEXT_SNDDT", BARM0_TEXT_SNDDT);
                    ec.Parameters.Add("BARM0_TEXT_SNDTM", BARM0_TEXT_SNDTM);
                    ec.Parameters.Add("BARM0_TEXT_TYPE", BARM0_TEXT_TYPE);
                    Console.WriteLine($"Parameters : ");
                    Console.WriteLine($"BARM0_FAST_NO:{BARM0_FAST_NO}");
                    Console.WriteLine($"BARM0_REMIT_DATE:{BARM0_REMIT_DATE}");
                    //Console.WriteLine($"BARM0_REMIT_STAT:{BARM0_REMIT_STAT}");
                    Console.WriteLine($"BARM0_TEXT_SND:{BARM0_TEXT_SND}");
                    Console.WriteLine($"BARM0_TEXT_SNDDT:{BARM0_TEXT_SNDDT}");
                    Console.WriteLine($"BARM0_TEXT_SNDTM:{BARM0_TEXT_SNDTM}");
                    Console.WriteLine($"BARM0_TEXT_TYPE:{BARM0_TEXT_TYPE}");
                    //EASYCOM廠商建議新增此句
                    ec.Prepare();
                    var updateNum = ec.ExecuteNonQuery();
                    Console.WriteLine(string.Empty);
                    Console.WriteLine($"資料異動筆數:{updateNum}");
                    WriteLog($"資料異動筆數:{updateNum}");
                    ec.Dispose();
                }
            }
            catch (Exception ex)
            {
                SendMailFlag = true;
                var msg = $"Error : InnerException:{ex.InnerException},Message{ex.Message}";
                Console.WriteLine(msg);
                WriteLog(msg);
                SendMailMsg += System.Environment.NewLine;
                SendMailMsg += msg;
            }
        }

        /// <summary>
        /// 接收後寫400
        /// </summary>
        /// <param name="BARM0_FAST_NO">快速付款編號</param>
        /// <param name="BARM0_TEXT_RCV">快速付款電文回饋狀態</param>
        /// <param name="BARM0_TEXT_RCVDT">快速付款電文回饋日期</param>
        /// <param name="BARM0_TEXT_RCVTM">快速付款電文回饋時間</param>
        protected static void save400_2(
         EacConnection con,
         string BARM0_FAST_NO,
         string BARM0_TEXT_RCV,
         string BARM0_TEXT_RCVDT,
         string BARM0_TEXT_RCVTM,
         string errorFlag = null
         )
        {
            try
            {
                using (EacCommand ec = new EacCommand(con))
                {
                    Console.WriteLine($"===更新400資料(接收後)===");
                    Console.WriteLine(string.Empty);

                    #region Parameter

                    //string sql = "";
                    //sql = "UPDATE LRTBARM1 " +
                    //" SET TEXT_RCV = :BARM0_TEXT_RCV" +
                    //"    ,TEXT_RCVDT = :BARM0_TEXT_RCVDT" +
                    //"    ,TEXT_RCVTM = :BARM0_TEXT_RCVTM" +
                    //" WHERE FAST_NO = :BARM0_FAST_NO";

                    #endregion

                    #region injection

                    string sql = $@"
                    UPDATE LRTBARM1
                    SET TEXT_RCV = '{BARM0_TEXT_RCV}' ,
                        TEXT_RCVDT = '{BARM0_TEXT_RCVDT}' ,
                        TEXT_RCVTM = '{BARM0_TEXT_RCVTM}' ";
                    if (errorFlag == "N")
                        sql += @" , REMIT_STAT = '3' ";
                   sql += $@" WHERE FAST_NO = '{BARM0_FAST_NO}' ; ";

                    #endregion



                    //Extension.writeTxtLog(sql, Path.Combine(Directory.GetCurrentDirectory(), "update2.txt"));
                    //Extension.writeTxtLog($"BARM0_FAST_NO:{BARM0_FAST_NO}", Path.Combine(Directory.GetCurrentDirectory(), "update2.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_RCV:{BARM0_TEXT_RCV}", Path.Combine(Directory.GetCurrentDirectory(), "update2.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_RCVDT:{BARM0_TEXT_RCVDT}", Path.Combine(Directory.GetCurrentDirectory(), "update2.txt"));
                    //Extension.writeTxtLog($"BARM0_TEXT_RCVTM:{BARM0_TEXT_RCVTM}", Path.Combine(Directory.GetCurrentDirectory(), "update2.txt"));
                    Console.WriteLine($"更新400資料(接收後) update sql:{sql}");
                    WriteLog($"update sql:{sql}");
                    Console.WriteLine(string.Empty);
                    ec.Parameters.Clear();
                    ec.CommandText = sql;
                    var _BARM0_FAST_NO = new EacParameter();
                    _BARM0_FAST_NO.ParameterName = "BARM0_FAST_NO";
                    _BARM0_FAST_NO.DbType = System.Data.DbType.String;
                    _BARM0_FAST_NO.Size = BARM0_FAST_NO.Length;
                    _BARM0_FAST_NO.Direction = System.Data.ParameterDirection.InputOutput;
                    _BARM0_FAST_NO.Value = BARM0_FAST_NO;
                    
                    var _BARM0_TEXT_RCV = new EacParameter();
                    _BARM0_TEXT_RCV.ParameterName = "BARM0_TEXT_RCV";
                    _BARM0_TEXT_RCV.DbType = System.Data.DbType.String;
                    _BARM0_TEXT_RCV.Size = BARM0_TEXT_RCV.Length;
                    _BARM0_TEXT_RCV.Direction = System.Data.ParameterDirection.InputOutput;
                    _BARM0_TEXT_RCV.Value = BARM0_TEXT_RCV;
                  
                    var _BARM0_TEXT_RCVDT = new EacParameter();
                    _BARM0_TEXT_RCVDT.ParameterName = "BARM0_TEXT_RCVDT";
                    _BARM0_TEXT_RCVDT.DbType = System.Data.DbType.String;
                    _BARM0_TEXT_RCVDT.Size = BARM0_TEXT_RCVDT.Length;
                    _BARM0_TEXT_RCVDT.Direction = System.Data.ParameterDirection.InputOutput;
                    _BARM0_TEXT_RCVDT.Value = BARM0_TEXT_RCVDT;
                  
                    var _BARM0_TEXT_RCVTM = new EacParameter();
                    _BARM0_TEXT_RCVTM.ParameterName = "BARM0_TEXT_RCVTM";
                    _BARM0_TEXT_RCVTM.DbType = System.Data.DbType.String;
                    _BARM0_TEXT_RCVTM.Size = BARM0_TEXT_RCVTM.Length;
                    _BARM0_TEXT_RCVTM.Direction = System.Data.ParameterDirection.InputOutput;
                    _BARM0_TEXT_RCVTM.Value = BARM0_TEXT_RCVTM;

                    ec.Parameters.Add(_BARM0_FAST_NO);
                    ec.Parameters.Add(_BARM0_TEXT_RCV);
                    ec.Parameters.Add(_BARM0_TEXT_RCVDT);
                    ec.Parameters.Add(_BARM0_TEXT_RCVTM);

                    Console.WriteLine($"Parameters : ");
                    Console.WriteLine($"BARM0_FAST_NO:{BARM0_FAST_NO}");
                    Console.WriteLine($"BARM0_TEXT_RCV:{BARM0_TEXT_RCV}");
                    Console.WriteLine($"BARM0_TEXT_RCVDT:{BARM0_TEXT_RCVDT}");
                    Console.WriteLine($"BARM0_TEXT_RCVTM:{BARM0_TEXT_RCVTM}");

                    //EASYCOM廠商建議新增此句
                    ec.Prepare();
                    var updateNum = ec.ExecuteNonQuery();
                    Console.WriteLine(string.Empty);
                    Console.WriteLine($"資料異動筆數:{updateNum}");
                    WriteLog($"資料異動筆數:{updateNum}");
                    ec.Dispose();
                }
            }
            catch (Exception ex)
            {
                SendMailFlag = true;
                var msg = $"Error : InnerException:{ex.InnerException},Message{ex.Message}";
                Console.WriteLine(msg);
                WriteLog(msg);
                SendMailMsg += System.Environment.NewLine;
                SendMailMsg += msg;
            }

        }

        /// <summary>
        /// 委派抓資料事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">http request</param>
        /// <param name="xml">發送的xml資料</param>
        /// <param name="type">傳送類別</param>
        /// <returns></returns>
        internal delegate IResultModel GetData(string xml, IResultModel model, url type);

        /// <summary>
        /// 呼叫httpRequest 並把Xml資料轉入類別中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">發送的xml資料</param>
        /// <param name="model">回傳資料</param>
        /// <param name="type">傳送類別</param>
        /// <returns></returns>
        internal static IResultModel XmlToData(string xml, IResultModel model, url type)
        {

            HttpWebRequest request = CreateSOAPWebRequest(type);
            Console.WriteLine($@"傳送的xml:{xml}");
            WriteLog($@"傳送的xml:{xml}");
            Console.WriteLine(string.Empty);

                switch (type)
                {
                    case url.HSMUrl_MAC:
                    case url.HSMUrl_SYNC:
                    using (Stream stream = request.GetRequestStream())
                        {
                            XmlDocument SOAPReqBody = new XmlDocument();
                            SOAPReqBody.LoadXml(xml);
                            SOAPReqBody.Save(stream);
                        }
                        break;
                    case url.FubonBankUrl_EACH:
                    case url.FubonBankUrl_XM96:
                        using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                        {
                            XmlDocument SOAPReqBody2 = new XmlDocument();
                            SOAPReqBody2.LoadXml(xml);
                            sw.Write(SOAPReqBody2.InnerXml);
                        }
                        //byte[] byteArray = Encoding.Default.GetBytes(xml);
                        //stream.Write(byteArray, 0, byteArray.Length);
                        break;
                }

                //Geting response from request  
                using (WebResponse Serviceres = request.GetResponse())
                {
                    Console.WriteLine("接收的檔案");
                    Console.WriteLine(((HttpWebResponse)Serviceres).StatusDescription);
                    using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                    {
                        var ServiceResult = rd.ReadToEnd();
                        Console.WriteLine(ServiceResult);
                        WriteLog($"{ServiceResult}");
                        Console.WriteLine(string.Empty);
                        var str = string.Empty; //回傳資料
                        XmlDocument doc = new XmlDocument();
                        switch (type)
                        {
                            case url.HSMUrl_MAC:
                            case url.HSMUrl_SYNC:
                                doc.LoadXml(ServiceResult); //轉成 XML
                                str = doc?.SelectSingleNode("//return")?.FirstChild?.Value;
                                break;
                            case url.FubonBankUrl_EACH:
                            case url.FubonBankUrl_XM96:
                                str = ServiceResult;
                                //byte[] asciiBytes = Encoding.Convert(ebcdic, ascii, Encoding.ASCII.GetBytes(ServiceResult));
                                //ServiceResult = Encoding.ASCII.GetString(asciiBytes);
                                //Console.WriteLine(ServiceResult);
                                //Console.WriteLine(string.Empty);
                                break;
                        }
                        //Extension.writeTxtLog(str, Path.Combine(Directory.GetCurrentDirectory(), "mytxt2.txt"));
                        Console.WriteLine("===資收資料部分===");
                        Console.WriteLine(string.Empty);
                        string _name = null; //欄位名稱
                        string val = null; //欄位值
                        foreach (var item in model.GetType().GetProperties())
                        {
                            _name = item.Name;
                            val = null;
                            if (!string.IsNullOrWhiteSpace(str) && str.IndexOf($"<{_name}>") > -1)
                            {
                                var start = str.IndexOf($"<{_name}>") + $"<{_name}>".Length;
                                val = str.Substring(start, (str.IndexOf($"</{_name}>") - start));
                                //if (_name == "ACT_DATE" || _name == "PRE_REQSRL" || _name == "SETL_ID")
                                //val = Encoding.ASCII.GetString(Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(val)));
                                //val = Encoding.ASCII.GetString(StringToByteArray(val));
                                item.SetValue(model, val);
                            }
                            Console.WriteLine($"{_name}:{val}");
                            WriteLog($"{_name}:{val}");
                        }
                    }
                }
          
            return model;
        }

        /// <summary>
        /// 發送url
        /// </summary>           
        protected internal enum url
        {
            [Description("XM96")]
            FubonBankUrl_XM96,

            [Description("EACH")]
            FubonBankUrl_EACH,

            [Description("MAC")]
            HSMUrl_MAC,

            [Description("SYNC")]
            HSMUrl_SYNC,
        }

        protected internal enum mailType
        {
            [Description("XML程式有錯誤")]
            XML,

            [Description("查詢不到FRT_WORK")]
            WORK,

            [Description("Socket 錯誤")]
            SOCKET,
        }

        protected internal interface IResultModel
        {

        }

        protected internal interface IValidate
        {

        }

        /// <summary>
        /// 組合 Http Request 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static HttpWebRequest CreateSOAPWebRequest(url type)
        {
            var url = Properties.Settings.Default[type.ToString().Split('_')[0]]?.ToString();
            //Making Web Request  
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
            //SOAPAction  
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/Addition");
            //Content_type  
            Req.ContentType = "application/x-www-form-urlencoded";
            switch (type)
            {
                case Program.url.HSMUrl_MAC:
                case Program.url.HSMUrl_SYNC:
                    Req.ContentType = "text/xml;charset=\"utf-8\"";
                    break;
            }
            //if(type == Program.url.HSMUrl_MAC)
            //    Req.ContentType = "text/xml;charset=\"utf-8\"";
            //if (type == Program.test.url.FubonBankUrl_XM96)
            //{
            //    Req.ContentType = "text/xml;charset=\"IBM037\""; 
            //}
            Req.Accept = "text/xml";
            //HTTP method  
            Req.Method = "POST";
            //return HttpWebRequest  
            return Req;
        }

        /// <summary>
        /// MAC XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static string GetMAC_XML(MACModel data)
        {
            string result = string.Empty;
            result = $@"<?xml version=""1.0"" encoding=""utf-8""?>  
                          <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.hsm.twca.com.tw/"">
                            <soap:Header/>
                             <soap:Body>
                                 <ser:MAC>
                                     <WorkingKeyLabel>{data.WorkingKeyLabel}</WorkingKeyLabel>
                                     <Data>{data.Data}</Data>
                                     <IV>{data.IV}</IV>
                                 </ser:MAC>
                             </soap:Body>
                          </soap:Envelope>";
            return result;
        }

        /// <summary>
        /// MAC XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static string GetSYNC_XML(MACModel data)
        {
            string result = string.Empty;
            result = $@"<?xml version=""1.0"" encoding=""utf-8""?>  
                          <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.hsm.twca.com.tw/"">
                            <soap:Header/>
                             <soap:Body>
                                 <ser:GetSyncValue>
                                     <WorkingKeyLabel>{data.WorkingKeyLabel}</WorkingKeyLabel>
                                     <Input>7370000000000000</Input>
                                 </ser:GetSyncValue>
                             </soap:Body>
                          </soap:Envelope>";
            return result;
        }

        /// <summary>
        /// XM96 XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static string GetXM96_XML(TxBody_XM96 data, FMPConnectionString FMP = null, TxHead Head = null)
        {
            string result = string.Empty;
            result += @"<Tx>";
            result += GetFMPANDHead(FMP, Head);
            var _XM96 = data.GetType();
            result += $@"<{_XM96.Name.Split('_')[0]}>";
            foreach (var item in _XM96.GetProperties())
            {
                result += $@"<{item.Name}>{item.GetValue(data)}</{item.Name}>";
            }
            result += $@"</{_XM96.Name.Split('_')[0]}>";
            result += @"</Tx>";
            return result;
        }

        /// <summary>
        /// EACH XML
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static string GetEACH_XML(TxBody_EACH data, FMPConnectionString FMP = null, TxHead Head = null)
        {
            string result = string.Empty;
            result += @"<Tx>";
            result += GetFMPANDHead(FMP, Head);
            var _EACH = data.GetType();
            result += $@"<{_EACH.Name.Split('_')[0]}>";
            foreach (var item in _EACH.GetProperties())
            {
                result += $@"<{item.Name}>{item.GetValue(data)}</{item.Name}>";
            }
            result += $@"</{_EACH.Name.Split('_')[0]}>";
            result += @"</Tx>";
            return result;
        }

        /// <summary>
        /// XML 組合 FMPConnectionString & TxHead
        /// </summary>
        /// <returns></returns>
        protected static string GetFMPANDHead(FMPConnectionString FMP = null, TxHead Head = null)
        {
            string result = string.Empty;
            var _FMP = FMP != null ? FMP : new FMPConnectionString();
            var _FMP_Type = _FMP.GetType();
            result += $@"<{_FMP_Type.Name}>";
            foreach (var item in _FMP_Type.GetProperties())
            {
                result += $@"<{item.Name}>{item.GetValue(_FMP)}</{item.Name}>";
            }
            result += $@"</{_FMP_Type.Name}>";
            var _Head = Head != null ? Head : new TxHead();
            var _Head_Type = _Head.GetType();
            result += $@"<{_Head_Type.Name}>";
            foreach (var item in _Head_Type.GetProperties())
            {
                result += $@"<{item.Name}>{item.GetValue(_Head)}</{item.Name}>";
            }
            result += $@"</{_Head_Type.Name}>";
            return result;
        }

        protected static void WriteStatus(string fastNo, XML_STATUS status)
        {
            try
            {
                using (dbGLEntities db = new dbGLEntities())
                {
                    var dts1 = DateTime.Now;
                    if (status == XML_STATUS.S1)
                    {
                        var log = db.FRT_XML_LOG.AsNoTracking().FirstOrDefault(x => x.fast_no == fastNo);
                        if (log == null)
                        {
                            db.FRT_XML_LOG.Add(new FRT_XML_LOG()
                            {
                                fast_no = fastNo,
                                create_date = dts1,
                                create_time_hms = dts1.TimeOfDay
                            });
                        }
                    }
                    db.FRT_XML_LOG_DETAIL.Add(new FRT_XML_LOG_DETAIL()
                    {
                        fast_no = fastNo,
                        create_date = dts1,
                        create_time_hms = dts1.TimeOfDay,
                        operation_status = status.ToString()
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"WirteCheckPoint Error:{ex}",Ref.Nlog.Error);
            }
        } 

        /// <summary>
        /// XML FMPConnectionString 區塊
        /// </summary>
        protected class FMPConnectionString
        {
            public FMPConnectionString()
            {
                SPName = "FBL_FPS";
                LoginID = "FBL_FPS";
            }
            public string SPName { get; set; }
            public string LoginID { get; set; }
            public string Password { get; set; }
            public string TxnId { get; set; }
        }

        /// <summary>
        /// XML TxHead 區塊
        /// </summary>
        protected class TxHead
        {
            public TxHead()
            {
                HWSID = "FBL_FPS";
                HSTANO = "HHmmssf(千毫秒)";
            }
            public string HWSID { get; set; }
            public string HSTANO { get; set; }
            public string HTLID { get; set; }
            public string HTXTID { get; set; }
        }

        /// <summary> 
        /// XML XM96 TxBody 區塊
        /// </summary>
        protected class TxBody_XM96
        {
            public TxBody_XM96()
            {
                TRAN_ID = "XM96";
                TX_RMK = "1";
                NAME_COD = "0001";
                PAY_BANK = "0127370";
                PAY_IDNO = "27935073";
                ACNO_OUT = "0000737102710668";
                FEE_COD = "13";
                REF_NO = "000000000000000000000000000000000000"; //動態
                PAY_NAME = "富邦人壽股份有限公司";
            }

            public string TRAN_ID { get; set; }
            public string TX_RMK { get; set; }
            [Required]
            public string ACT_DATE { get; set; }
            public string PRE_REQDATE { get; set; }
            public string PRE_REQSRL { get; set; }
            [Required]
            public string PAY_IDNO { get; set; }
            public string NAME_COD { get; set; }
            [Required]
            public string SETL_ID { get; set; }
            [Required]
            public string PAY_BANK { get; set; }
            [Required]
            public string ACNO_OUT { get; set; }
            public string CERT_BKID { get; set; }
            public string CERT_NO { get; set; }
            [Required]
            public string RCV_BANK { get; set; }
            [Required]
            public string RCV_ACNO { get; set; }
            [Required]
            public string TX_AMT { get; set; }
            public string FEE_COD { get; set; }
            public string REF_NO { get; set; }
            [Required]
            public string PAY_NAME { get; set; }
            [Required]
            public string RCV_NAME { get; set; }
            public string APX { get; set; }
            public string AG_BKID { get; set; }
            public string MAC { get; set; }
            public string SYNC { get; set; }
            public string ICV { get; set; }
        }

        /// <summary>
        /// XML EACH TxBody 區塊
        /// </summary>
        protected class TxBody_EACH
        {
            public TxBody_EACH()
            {
                TXNCODE = "TF05";
                ACHCODE = "351";
                TXNOUTBANK = "0127370";
                OUTIDNO = "27935073";
                TRANSFEROUTACTNO = "0000737102710668";
                MEMO = "　　　";
            }

            [Required]
            public string REQUESTID { get; set; }
            public string SEQNO { get; set; }
            public string TXNCODE { get; set; }
            public string ACHCODE { get; set; }
            [Required]
            public string TXNAMT { get; set; }
            [Required]
            public string TXNINBANK { get; set; }
            [Required]
            public string TXNOUTBANK { get; set; }
            [Required]
            public string TRANSFERINACTNO { get; set; }
            [Required]
            public string TRANSFEROUTACTNO { get; set; }
            [Required]
            public string INIDNO { get; set; }
            [Required]
            public string OUTIDNO { get; set; }
            public string USERNO { get; set; }
            public string REFNO { get; set; }
            public string MERCHANTID { get; set; }
            public string ORDERNO { get; set; }
            public string TERMID { get; set; }
            public string MEMO { get; set; }
            public string MAC { get; set; }
        }

        /// <summary>
        /// XML MAC
        /// </summary>
        protected class MACModel
        {
            public string WorkingKeyLabel { get; set; }
            public string Data { get; set; }
            public string IV { get; set; }
        }

        /// <summary>
        /// MAC 回傳參數
        /// </summary>
        protected class MACResultModel : IResultModel , IValidate
        {
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string MAC { get; set; }
        }

        /// <summary>
        /// SYNC 回傳參數
        /// </summary>
        protected class SYNCResultModel : IResultModel , IValidate
        {
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string SyncValue { get; set; }
        }

        /// <summary>
        /// EACH 接收
        /// </summary>
        protected class FRT_XML_R_eACHModel : IResultModel 
        {
            public string TxHead { get; set; }
            public string REQUESTID { get; set; }
            public string SEQNO { get; set; }
            public string ACHSTAN { get; set; }
            public string TXNCODE { get; set; }
            public string ACHCODE { get; set; }
            public string TXNAMT { get; set; }
            public string TXNINBANK { get; set; }
            public string TXNOUTBANK { get; set; }
            public string TRANSFERINACTNO { get; set; }
            public string TRANSFEROUTACTNO { get; set; }
            public string INIDNO { get; set; }
            public string OUTIDNO { get; set; }
            public string USERNO { get; set; }
            public string MERCHANTID { get; set; }
            public string ORDERNO { get; set; }
            public string TERMID { get; set; }
            public string MEMO { get; set; }
            public string CHARGEFEE { get; set; }
            public string ACHBIZDATE { get; set; }
            public string O360TXNSEQNO { get; set; }
            public string STLFLAG { get; set; }
            public string ERROR_CODE { get; set; }
            public string EMSGID { get; set; }
            public string EMSGTXT { get; set; }
            public string SQL_STS { get; set; }
            public DateTime CRT_TIME { get; set; }
            public DateTime UPD_TIME { get; set; }
        }

        /// <summary>
        /// EACH 傳送
        /// </summary>
        protected class FRT_XML_T_eACHModel : IValidate
        {
            //[Required(ErrorMessage = "TRAD_PARTNERS(交易對象) 欄位是必要項。")]
            //[StringLength(30, ErrorMessage = "交易對象欄位,不得大於30個字元。")]
            //[RegularExpression(@"^([0-9]{1,18})?$", ErrorMessage = "金額不符合整數或超過18位")]
            //[RegularExpression(@"^([0-9]{1,})?$", ErrorMessage = "股票張數需為數字。")]
            public string HEADER { get; set; }
            public string REQUESTID { get; set; }
            public string SEQNO { get; set; }
            public string TXNCODE { get; set; }
            public string ACHCODE { get; set; }
            public Decimal TXNAMT { get; set; }
            public string TXNINBANK { get; set; }
            public string TXNOUTBANK { get; set; }
            public string TRANSFERINACTNO { get; set; }
            public string TRANSFEROUTACTNO { get; set; }
            public string INIDNO { get; set; }
            public string OUTIDNO { get; set; }
            public string USERNO { get; set; }
            public string REFNO { get; set; }
            public string MERCHANTID { get; set; }
            public string ORDERNO { get; set; }
            public string TERMID { get; set; }
            public string MEMO { get; set; }
            public string MAC { get; set; }
            public string SQL_STS { get; set; }
            public DateTime CRT_TIME { get; set; }
            public DateTime UPD_TIME { get; set; }
        }

        /// <summary>
        /// XM96 接收
        /// </summary>
        protected class FRT_XML_R_622821Model : IResultModel 
        {
            public string TxHead { get; set; }
            public string ACT_DATE { get; set; }
            public string PRE_REQSRL { get; set; }
            public string SETL_ID { get; set; }
            public string MSG_TYPE { get; set; }
            public string TX_AMT { get; set; }
            public string FEE_AMT { get; set; }
            public string SIGN_ACT { get; set; }
            public string ACT_BAL { get; set; }
            public string SIGN_AVAL { get; set; }
            public string AVAL_BAL { get; set; }
            public string PRE_REQDATE { get; set; }
            public string RC_CODE { get; set; }
            public string EMSGID { get; set; }
            public string EMSGTXT { get; set; }
            public string SQL_STS { get; set; }
            public DateTime CRT_TIME { get; set; }
            public DateTime UPD_TIME { get; set; }

        }

        /// <summary>
        /// XM96 傳送
        /// </summary>
        protected class FRT_XML_T_622821Model : IValidate
        {
            public string HEADER { get; set; }
            public string TRAN_ID { get; set; }
            public string TX_RMK { get; set; }
            public string ACT_DATE { get; set; }
            public string PRE_REQDATE { get; set; }
            public string PRE_REQSRL { get; set; }
            public string PAY_IDNO { get; set; }
            public string NAME_COD { get; set; }
            public string SETL_ID { get; set; }
            public string PAY_BANK { get; set; }
            public string ACNO_OUT { get; set; }
            public string CERT_BKID { get; set; }
            public string CERT_NO { get; set; }
            public string RCV_BANK { get; set; }
            public string RCV_ACNO { get; set; }
            public string TX_AMT { get; set; }
            public string FEE_COD { get; set; }
            public string REF_NO { get; set; }
            public string PAY_NAME { get; set; }
            public string RCV_NAME { get; set; }
            public string APX { get; set; }
            public string AG_BKID { get; set; }
            public string MAC { get; set; }
            public string SYNC { get; set; }
            public string ICV { get; set; }
            public string SQL_STS { get; set; }
            public string CRT_TIME { get; set; }
            public string UPD_TIME { get; set; }
        }

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        protected partial class ErrorModel
        {
            /// <summary>
            /// 回寫匯款狀態
            /// </summary>
            public string ExecType { get; set; }

            /// <summary>
            /// 快速付款編號
            /// </summary>
            public string Fast_No { get; set; }

            /// <summary>
            /// 錯誤代碼
            /// </summary>
            public string ErrorCode { get; set; }

            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string EMSGTXT { get; set; }

            /// <summary>
            /// 電文類型
            /// </summary>
            public string TextType { get; set; }
        }

        protected partial class FastMailModel
        {
            /// <summary>
            /// ” REMIT_ERR”
            /// </summary>
            public string groupCode { get; set; }

            /// <summary>
            /// 1,2(第N段水位)
            /// </summary>
            public string reserve1 { get; set; }

            /// <summary>
            /// 錯誤代碼
            /// </summary>
            public string rtnCode { get; set; }

            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string errorMsg { get; set; }

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

        public enum XML_STATUS
        {
            [Description("400 呼叫 FRTXml")]
            S1,
            [Description("非工作日 or 不在傳送時間內")]
            SW,
            [Description("FRTXml 丟給銀行前")]
            S2,
            [Description("銀行回覆 FRTXml")]
            S3,
            [Description("FRTXML 寫入OpenDb , 水位判斷 , 修改400Db")]
            S4,
            [Description("FRTXML 流程完成")]
            SS
        }

        #endregion
    }

    internal static class Extension
    {
        /// <summary>
        /// 寫入 寄信txt
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="folderPath"></param>
        public static void writeTxtLog(string msg, string folderPath)
        {
            try
            {
                string txtData = string.Empty;
                try //試著抓取舊資料
                {
                    txtData = File.ReadAllText(folderPath, System.Text.Encoding.Default);
                }
                catch { }
                writeTxt(folderPath, msg, txtData, true);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 寫入 txt
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="newtTxt"></param>
        /// <param name="oldTxt"></param>
        private static void writeTxt(string folderPath, string newtTxt, string oldTxt = null, bool flag = false)
        {
            if (!folderPath.IsNullOrWhiteSpace() && !newtTxt.IsNullOrWhiteSpace())
                using (FileStream fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write))
                {
                    var txtData = string.Empty;
                    if (!oldTxt.IsNullOrWhiteSpace())
                    {
                        txtData = oldTxt;
                        if (!flag)
                            txtData += string.Format("\r\n{0}", newtTxt);
                        else
                            txtData = (string.Format("{0}\r\n{1}", newtTxt, txtData));
                    }
                    else
                        txtData = newtTxt;
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.Write(txtData); //存檔
                    sw.Close();
                }
        }

        /// Model 和 Model 轉換
        /// <summary>
        /// Model 和 Model 轉換
        /// </summary>
        /// <typeparam name="T1">來源型別</typeparam>
        /// <typeparam name="T2">目的型別</typeparam>
        /// <param name="model">來源資料</param>
        /// <returns></returns>
        public static T2 ModelConvert<T1, T2>(this T1 model) where T2 : new()
        {
            T2 newModel = new T2();
            if (model != null)
            {
                foreach (PropertyInfo itemInfo in model.GetType().GetProperties())
                {
                    PropertyInfo propInfoT2 = typeof(T2).GetProperty(itemInfo.Name);
                    if (propInfoT2 != null)
                    {
                        // 型別相同才可轉換
                        if (propInfoT2.PropertyType == itemInfo.PropertyType)
                        {
                            var _value = itemInfo.GetValue(model, null);
                            //if (propInfoT2.PropertyType == typeof(string))
                            //    _value = _value?.ToString()?.Trim();
                            propInfoT2.SetValue(newModel, _value, null);
                        }
                    }
                }
            }
            return newModel;
        }

        public static string getValidateString
        (this IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
                result = string.Join(" ", errors.Select(y => string.Join(",", y.ValidationErrors.Select(z => z.ErrorMessage))));
            return result;
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static int stringToInt(this string value)
        {
            int i = 0;
            if (value.IsNullOrWhiteSpace())
                return i;
            Int32.TryParse(value, out i);
            return i;
        }

        public static Nullable<int> stringToIntN(this string value)
        {
            Nullable<int> i = null;
            int _i = 0;
            if (value.IsNullOrWhiteSpace())
                return i;
            if (Int32.TryParse(value,out _i))
            {
                i = _i;
            }
            return i;
        }

        public static decimal stringToDecimal(this string value)
        {
            decimal d = 0M;
            if (value.IsNullOrWhiteSpace())
                return d;
            decimal.TryParse(value, out d);
            return d;
        }

        public static Nullable<decimal> stringToDecimalN(this string value)
        {
            Nullable<decimal> d = null;
            if (value.IsNullOrWhiteSpace())
                return d;
            decimal _d = 0M;
            if (decimal.TryParse(value, out _d))
            {
                d = _d;
            }
            return d;
        }

        public static string modelToString<T>(this T model, string log = null)
        {
            var result = string.Empty;
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(log))
                    result += "|";
                StringBuilder sb = new StringBuilder();
                var Type = model.GetType();
                sb.Append($@"TableName:{Type.Name}|");
                var Pros = Type.GetProperties();
                Pros.ToList().ForEach(x =>
                {
                    sb.Append($@"{x.Name}:{x.GetValue(model)?.ToString()}|");
                });
                if (sb.Length > 0)
                {
                    result = sb.ToString();
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        public static string CHREncoding(this string value)
        {
            string str = string.Empty;
            char[] NumChar = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            foreach (char C in value)
            {
                if (NumChar.Contains(C))  //數字
                {
                    str += (BitConverter.ToString(Encoding.ASCII.GetBytes(C.ToString()))).Replace("-", "");
                }
                else  //文字
                {
                    str += (BitConverter.ToString(Encoding.GetEncoding(500).GetBytes(C.ToString()))).Replace("-", "");                   
                }
            }
            return str;
        }

        public static Nullable<decimal> fixedAMT(this Nullable<decimal> d)
        {
            if (d == null)
                return d;
            return d.Value / 100;
        }

        public static string validateString(this IEnumerable<Program.IValidate> models)
        {
            string resultStr = string.Empty;
            if (!models.Any())
                return resultStr;
            
            var result = new List<ValidationResult>();
            List<string> errors = new List<string>();
            var pros = models.First().GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            foreach (var item in models)
            {
                var context = new ValidationContext(item, null, null);
                if (!Validator.TryValidateObject(item, context, result, true))
                {
                    result.ForEach(x =>
                    {
                        var m = x.MemberNames.FirstOrDefault()?.ToString();
                        var p = pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                        var val = (p == null) ? null : (p.GetValue(item))?.ToString();
                        if (!val.IsNullOrWhiteSpace())
                            errors.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                        else
                            errors.Add(x.ErrorMessage);
                    });
                }
            }
            return resultStr;
        }
    }
}
