using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace FastRemit
{
    class Program
    {

        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new Service1()
            //};
            //ServiceBase.Run(ServicesToRun);

            HostFactory.Run(x =>
            {
                x.Service<CustomerSocket>(s =>
                {
                    s.ConstructUsing(name => new CustomerSocket());
                    s.WhenStarted(ms => ms.Start());
                    s.WhenStopped(ms => ms.Stop());
                });

                x.SetServiceName("FastRemitServiceName");
                x.SetDisplayName("FastRemitDisplayName");
                x.SetDescription("FastRemit 服務");
                x.RunAsLocalSystem();
                x.StartAutomatically();
            });

        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class CustomerSocket
        {
            private CancellationTokenSource _canceller = new CancellationTokenSource();
            private Task _serviceTask;
         

            //宣告一個Socket通訊介面(使用IPv4協定,通訊類型,通訊協定)
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            public CustomerSocket()
            {
                try
                {
                    //System.Configuration.ConfigurationManager.AppSettings.Get("maxTask");
                }
                catch
                {

                }
            }

            public async Task Init()
            {
                //直接測試
                //new test().run("Q0710000A1");

                var _RcvPort = System.Configuration.ConfigurationManager.AppSettings.Get("RcvPort");

                int port = 9202;
                int _port = 0;
                if (int.TryParse(_RcvPort, out _port))
                    port = _port;



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
                        //WriteLog($@"Waiting for a connection...", null, Ref.Nlog.Info);
                        logger.Info("Waiting for a connection...");
                        //Task.Delay(1).Wait();

                        //宣告一個Socket等於新建立的連線
                        Socket client = newsock.Accept();

                        //宣告一個連接點為socket端的連接點
                        IPEndPoint clientip = (IPEndPoint)client.RemoteEndPoint;

                        //印出遠端IP位址
                        //WriteLog($@"Client End Point = {clientip}", null, Ref.Nlog.Info);

                        logger.Info($@"Client End Point = {clientip}");

                        //Task.Delay(1).Wait();

                        //宣告一個監聽類別SocketListener監聽client訊息
                        SocketListener listener = new SocketListener(client);

                        Task.Run(() => { listener.run(); });

                        //宣告一個執行序去跑SocketListener監聽事件
                        //Thread thread = new Thread(new ThreadStart(listener.run));

                        //thread.Start();
                    }
                }
                catch (Exception ex)
                {
                    CloseSocket(newsock);
                    var _msg = $"錯誤訊息_Socket : InnerException:{ex.InnerException},Message:{ex.Message}";
                    logger.Info($@"{_msg}");
                    //WriteLog($"{_msg}", null, Ref.Nlog.Error);
                    new CustomerSocket().Start();
                }
            }

            public void Start()
            {
                _canceller = new CancellationTokenSource();
                Task.Run(() => { Init(); }, _canceller.Token);
                _serviceTask = ExecuteAsync();
            }

            public void Stop()
            {
                _canceller.Cancel();
                _serviceTask.Wait();

            }

            public async Task ExecuteAsync()
            {
                while (_canceller.Token.IsCancellationRequested)
                {
                    logger.Info($@"FastRemit Cancel !");
                    CloseSocket(newsock);
                }
            }

            //private System.Timers.Timer _timer;

            //public CustomerSocket()
            //{
            //    _timer = new System.Timers.Timer(1000) { AutoReset = false };
            //    _timer.Elapsed += new ElapsedEventHandler(this.MainTask);
            //}

            //private void MainTask(object sender, ElapsedEventArgs args)
            //{
            //    //直接測試
            //    //new test().run("Q0710000A1");

            //    var _RcvPort = Properties.Settings.Default["RcvPort"]?.ToString();

            //    int port = 9201;
            //    int _port = 0;
            //    if (int.TryParse(_RcvPort, out _port))
            //        port = _port;

            //    //宣告一個Socket通訊介面(使用IPv4協定,通訊類型,通訊協定)
            //    Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //    try
            //    {
            //        //將IP位址和Port宣告為服務的連接點(所有網路介面卡 IP,1234 Port)
            //        IPEndPoint ipont = new IPEndPoint(IPAddress.Any, port);
            //        //IPEndPoint ipont = new IPEndPoint(IPAddress.Any, 9201);

            //        newsock.Bind(ipont);

            //        //偵測連接(最大連接數)
            //        newsock.Listen(50);

            //        while (true)
            //        {
            //            WriteLog($@"Waiting for a connection...",null, Ref.Nlog.Trace);

            //            //宣告一個Socket等於新建立的連線
            //            Socket client = newsock.Accept();

            //            //宣告一個連接點為socket端的連接點
            //            IPEndPoint clientip = (IPEndPoint)client.RemoteEndPoint;

            //            //印出遠端IP位址
            //            WriteLog($@"Client End Point = {clientip}",null, Ref.Nlog.Trace);

            //            //宣告一個監聽類別SocketListener監聽client訊息
            //            SocketListener listener = new SocketListener(client);

            //            //宣告一個執行序去跑SocketListener監聽事件
            //            Thread thread = new Thread(new ThreadStart(listener.run));

            //            thread.Start();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        CloseSocket(newsock);
            //        var _msg = $"錯誤訊息_Socket : InnerException:{ex.InnerException},Message:{ex.Message}";
            //        WriteLog($"{_msg}",null, Ref.Nlog.Error);
            //        string Sys_Error = "SYS_ERROR";
            //        sendMail(string.Empty, mailType.SOCKET, Sys_Error, _msg);
            //        new CustomerSocket().Start();
            //    }
            //}

            //public async void Start()
            //{
            //    _timer.Start();
            //}

            //public async void Stop()
            //{
            //    _timer.Stop();
            //}
        }

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

            public async Task run()
            {
                try
                {
                    string input = string.Empty;
                    while (true)
                    {
                        logger.Info($@"Start...");
                        //WriteLog($@"Start...", null, Ref.Nlog.Trace);

                        //Task.Delay(1).Wait();

                        //定義一個資料緩衝區接收長度最大為(1024)
                        byte[] data = new byte[1024];

                        //接收資料至緩衝區中並回傳成功接收位元數
                        int len = socket.Receive(data);

                        ////若成功接收位元數為0則跳出迴圈
                        if (len == 0) break;

                        byte[] asciiBytes = Encoding.Convert(ebcdic, ascii, data);
                        //string fastNo = Encoding.ASCII.GetString(asciiBytes);
                        input = Encoding.ASCII.GetString(asciiBytes);

                        //印出編碼後的資料(資料,起始位置,長度)
                        //WriteLog($@"接收到 fastNo = {fastNo}", null, Ref.Nlog.Trace);
                        logger.Info($@"接收到 input = {input}");

                        //socket.Send(Encoding.ASCII.GetBytes("0"));
                        //Task.Run(() => { new Program().work(input); });
                        //socket.Send(Encoding.GetEncoding("IBM037").GetBytes("0"));
                        await RemitOms.FastRemitApi(socket, input);
                        CloseSocket(socket);
                        break;

                        //socket.Send(Encoding.UTF8.GetBytes("0"));
                    }

                }
                catch (SocketException se)
                {
                    logger.Info($@"SocketException : { se.ToString()}");
                }
                catch (Exception e)
                {
                    logger.Info($@"外層-錯誤訊息:{e}");
                }
                finally
                {
                    CloseSocket(socket);
                }
            }
        }

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
                logger.Info($@"CloseSocket_Error : {e.ToString()}");
            }
        }
    }
}
