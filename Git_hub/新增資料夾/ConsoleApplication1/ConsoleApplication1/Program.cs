using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace ConsoleApplication1
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

            //    x.SetServiceName("test1ServiceName");
            //    x.SetDisplayName("test1DisplayName");
            //    x.SetDescription("test1Description");
            //    x.RunAsLocalSystem();
            //    x.StartAutomatically();
            //});
            //StartListening();
            new CustomerSocket().Start();
            Console.ReadLine();

        }

        //private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected static void WriteLog(string log, string name = null, Ref.Nlog type = Ref.Nlog.Info)
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

        public class CustomerSocket
        {

            private CancellationTokenSource _canceller = new CancellationTokenSource();
            private Task _serviceTask;

            public CustomerSocket()
            {

            }

            public void Start()
            {
                WriteLog("Start");
                Task.Run(() => { Init(); }, _canceller.Token);
                _serviceTask = ExecuteAsync();
            }

            public void Stop()
            {
                WriteLog("Stop");
                _canceller.Cancel();
                _serviceTask.Wait();

            }

            public async Task ExecuteAsync()
            {
                WriteLog("ExecuteAsync");
                while (_canceller.Token.IsCancellationRequested)
                {
                    WriteLog("Cancel");
                    CloseSocket(listener);
                }
            }

            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            // Create a TCP/IP socket.  
            // Socket listener = new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            Socket listener = new Socket(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            public async Task Init()
            {
                // Establish the local endpoint for the socket.  
                // Dns.GetHostName returns the name of the   
                // host running the application.  


                //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);



                // Bind the socket to the local endpoint and   
                // listen for incoming connections.  
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(3);
                    int i = 0;
                    // Start listening for connections.  
                    while (true)
                    {

                        Console.WriteLine("Waiting for a connection...");
                        WriteLog("Waiting for a connection...");
                        // Program is suspended while waiting for an incoming connection.  
                        Socket handler = listener.Accept();

                        //宣告一個連接點為socket端的連接點
                        IPEndPoint clientip = (IPEndPoint)handler.RemoteEndPoint;

                        //印出遠端IP位址
                        System.Console.WriteLine("Client End Point = " + clientip);
                        WriteLog("Client End Point = " + clientip);
                        //宣告一個監聽類別SocketListener監聽client訊息
                        SocketListener listener1 = new SocketListener(handler);
                        Task.Run(() => { listener1.run(); });
                        ////宣告一個執行序去跑SocketListener監聽事件
                        //Thread thread = new Thread(new ThreadStart(listener1.run));

                        //thread.Start();

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    WriteLog(e.ToString());
                    CloseSocket(listener);
                    new CustomerSocket().Start();
                    //listener.Dispose();
                    //listener.Close();
                    //StartListening();
                }

                Console.WriteLine("\nPress ENTER to continue...");
                Console.Read();
            }

            //private System.Timers.Timer _timer;

            //public CustomerSocket()
            //{
            //    _timer = new System.Timers.Timer(1000) { AutoReset = false };
            //    _timer.Elapsed += new ElapsedEventHandler(this.MainTask);
            //}

            //private void MainTask(object sender, ElapsedEventArgs args)
            //{

            //    // Establish the local endpoint for the socket.  
            //    // Dns.GetHostName returns the name of the   
            //    // host running the application.  

            //    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //    IPAddress ipAddress = ipHostInfo.AddressList[0];
            //    //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            //    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            //    // Create a TCP/IP socket.  
            //    Socket listener = new Socket(ipAddress.AddressFamily,
            //        SocketType.Stream, ProtocolType.Tcp);

            //    // Bind the socket to the local endpoint and   
            //    // listen for incoming connections.  
            //    try
            //    {
            //        listener.Bind(localEndPoint);
            //        listener.Listen(3);
            //        int i = 0;
            //        // Start listening for connections.  
            //        while (true)
            //        {

            //            Console.WriteLine("Waiting for a connection...");
            //            WriteLog("Waiting for a connection...");
            //            // Program is suspended while waiting for an incoming connection.  
            //            Socket handler = listener.Accept();

            //            //if (i > 3)
            //            //{
            //            //    string s = null;
            //            //    s = s.ToString();
            //            //}

            //            //i++;


            //            //宣告一個連接點為socket端的連接點
            //            IPEndPoint clientip = (IPEndPoint)handler.RemoteEndPoint;

            //            //印出遠端IP位址
            //            System.Console.WriteLine("Client End Point = " + clientip);
            //            WriteLog("Client End Point = " + clientip);
            //            //宣告一個監聽類別SocketListener監聽client訊息
            //            SocketListener listener1 = new SocketListener(handler);

            //            //宣告一個執行序去跑SocketListener監聽事件
            //            Thread thread = new Thread(new ThreadStart(listener1.run));

            //            thread.Start();



            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //        WriteLog(e.ToString());
            //        CloseSocket(listener);
            //        new CustomerSocket().Start();
            //        //listener.Dispose();
            //        //listener.Close();
            //        //StartListening();
            //    }

            //    Console.WriteLine("\nPress ENTER to continue...");
            //    Console.Read();
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
                WriteLog(e.ToString(),null, Ref.Nlog.Error);
                //Console.WriteLine(e.Message); 
            }
        }

        private static List<string> datas = new List<string>(); //目前在處理的快速付款編號

        public class SocketListener
        {
            public static string data = null;

            Encoding ascii = Encoding.ASCII;
            Encoding ebcdic = Encoding.GetEncoding("IBM037");
            private Socket socket;

            public SocketListener(Socket socket)
            {
                //建構元取得遠端socket連線
                this.socket = socket;

            }

            private object obje = new object();

            public async Task run()
            {
                // Data buffer for incoming data.  
                byte[] bytes = new Byte[1024];
                data = null;
            
                try
                {
                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        //lock(obje)
                        //{
                            int bytesRec = socket.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                            //if (data.IndexOf("<EOF>") > -1)
                            //{
                            //    break;
                            //}

                            // Show the data on the console.  
                            Console.WriteLine("Text received : {0}", data);
                            //WriteLog("Text received : " + data);
                            WriteLog("Text received : " + data, data);
                            byte[] msg = Encoding.ASCII.GetBytes(data);
                            Task.Run(() => { new Program().work(data); });
                            socket.Send(msg);
                            CloseSocket(socket);
                        //}

                        break;

                        // Echo the data back to the client.  

                        //socket.Shutdown(SocketShutdown.Both);
                        //socket.Close();

                     
                    }


                    //WriteLog("Done!", data);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    CloseSocket(socket);
                }

            }
        }

        public async Task work(string fastNo)
        {
            Task.Delay(100).Wait();
            WriteLog("compare : " + fastNo, fastNo);
            //WriteLog("compare : " + fastNo);
            if (!datas.Contains(fastNo) && !working.Contains(fastNo))
            {
                datas.Add(fastNo);
                WriteLog("Add : " + fastNo, fastNo);
                //WriteLog("Add : " + fastNo);
                TaskContinueDemo();
                //WriteLog("work start!", fastNo);
                //Task.Delay(6000).Wait();
                //WriteLog("work end!", fastNo);
                //datas.Remove(fastNo);
            }
            else
            {
                WriteLog("重複資料" + fastNo);
            }
        }

        static object lockObj = new object();
        static object lockObj2 = new object();
        static int maxTask = 5;
        static int currentCount = 0;
        static List<string> working = new List<string>();

        private static void TaskContinueDemo()
        {
            while (currentCount < maxTask && datas.Any())
            {
                lock (lockObj)
                {
                    if (currentCount < maxTask && datas.Any())
                    {
                        Interlocked.Increment(ref currentCount);

                        var task = Task.Factory.StartNew( () =>
                        {
                            if (datas.Any())
                            {
                                var number = string.Empty;
                                lock (lockObj2)
                                {
                                    number = datas.FirstOrDefault();
                                    working.Add(number);
                                    datas.Remove(number);
                                }
                                var sleepTime = Rand(5) + 1;
                                WriteLog($@"start fastNo:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now} ");
                                WriteLog($@"start fastNo:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now} ",number);
                                //Task.Delay(sleepTime * 1000).Wait();
                                works(sleepTime, number).Wait();
                                WriteLog($@"End fastNo:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now}");
                                WriteLog($@"End fastNo:{number} , sleepTime:{sleepTime}s , currentCount => {currentCount} , dtn:{DateTime.Now}",number);
                                working.Remove(number);
                            }
                        }, TaskCreationOptions.LongRunning).ContinueWith(t =>
                        {//在ContinueWith中恢复计数
                            Interlocked.Decrement(ref currentCount);
                            WriteLog($@"ContinueWith  currentCount => {currentCount} , dtn:{DateTime.Now}");
                            //Console.WriteLine("Continue Task id {0} Time{1} currentCount{2}", Task.CurrentId, DateTime.Now, currentCount);
                            TaskContinueDemo();
                        });
                    }
                }
            }
        }

        private static int Rand(int maxNumber = 5)
        {
            return Math.Abs(Guid.NewGuid().GetHashCode()) % maxNumber;
        }

        private static async Task works(int sleepTime,string fastNo)
        {
            WriteLog($@"workstart sleepTime:{sleepTime}s , fastNo => {fastNo}", fastNo);
            await sleeptime(sleepTime, fastNo);
            WriteLog($@"workend sleepTime:{sleepTime}s , fastNo => {fastNo}", fastNo);
            WriteLog($@"workEnd sleepTime:{sleepTime}s , fastNo => {fastNo}");
        }

        private static async Task sleeptime(int sleepTime, string fastNo)
        {
            WriteLog($@"sleeptimestart sleepTime:{sleepTime}s , fastNo => {fastNo}", fastNo);
            await Task.Delay(sleepTime * 1000);
            WriteLog($@"sleeptimeend sleepTime:{sleepTime}s , fastNo => {fastNo}", fastNo);
        }
    }

  

  
}
