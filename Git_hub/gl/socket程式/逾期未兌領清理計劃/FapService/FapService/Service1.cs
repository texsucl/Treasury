using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// modify by daiyu 20191023 加逾期未兌領AML報表呼叫
/// </summary>
namespace FapService
{
    public partial class Service1 : ServiceBase
    {
        private TcpListener server;

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread thread = new Thread(StartListenning);
            thread.Start();
        }

        protected override void OnStop()
        {
        }

        public async void StartListenning()
        {
            try
            {
                // IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
                TcpListener tcpListener = new TcpListener(IPAddress.Any, 9203);

                tcpListener.Start();


                while (true)
                {
                    Socket socket = tcpListener.AcceptSocket();

                    try {

                        //定義一個資料緩衝區接收長度最大為(1024)
                        byte[] data = new byte[1024];

                        /*-------data-->即AS400訊息--------*/
                        int len = socket.Receive(data);


                        //若成功接收位元數為0則跳出迴圈
                        if (len == 0) break;

                        Encoding ascii = Encoding.ASCII;
                        Encoding ebcdic = Encoding.GetEncoding("IBM037");

                        int lastIndex = Array.FindLastIndex(data, b => b != 0);
                        logger.Info("lastIndex:" + lastIndex);

                        Array.Resize(ref data, lastIndex + 1);

                        byte[] asciiBytes = Encoding.Convert(ebcdic, ascii, data);
                        string input = Encoding.ASCII.GetString(asciiBytes);



                        logger.Info("input:" + input);
                        if (input.StartsWith("VECLEAN|"))
                            await VeClean.VeCleanApi(socket, input);
                        else if (input.StartsWith("VETRACESTATUS|"))
                            await VeTraceStatus.VeTraceStatusApi(socket, input);
                        else if (input.StartsWith("VEAML|"))
                            await VeAML.VeAMLRptApi(socket, input);
                        else
                        {
                            logger.Error("input error!!");
                            socket.Send(Encoding.GetEncoding("IBM037").GetBytes("F"));
                            socket.Close();
                        }
                    }
                    catch (Exception e) {
                        logger.Error(e.ToString);

                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }


                    /*--------主程式碼加在這裡  end--------*/

                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString);

            }
        }

    }


}
