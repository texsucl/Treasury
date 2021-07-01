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

namespace FastRemit
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
                TcpListener tcpListener = new TcpListener(IPAddress.Any, 9202);

                tcpListener.Start();


                while (true)
                {
                    Socket socket = tcpListener.AcceptSocket();


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
                    if (input.StartsWith("REMITOMS|"))
                        await RemitOms.FastRemitApi(socket, input);
                    else {
                        logger.Error("input error!!");
                        socket.Send(Encoding.GetEncoding("IBM037").GetBytes(""));
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





        //protected static void WriteLog(string log, Ref.Nlog type = Ref.Nlog.Info)
        //{
        //    switch (type)
        //    {
        //        //追蹤
        //        case Ref.Nlog.Trace:
        //            logger.Trace(log);
        //            break;
        //        //開發
        //        case Ref.Nlog.Debug:
        //            logger.Debug(log);
        //            break;
        //        //訊息
        //        case Ref.Nlog.Info:
        //            logger.Info(log);
        //            break;
        //        //警告
        //        case Ref.Nlog.Warn:
        //            logger.Warn(log);
        //            break;
        //        //錯誤
        //        case Ref.Nlog.Error:
        //            logger.Error(log);
        //            break;
        //        //致命
        //        case Ref.Nlog.Fatal:
        //            logger.Fatal(log);
        //            break;
        //    }
        //}

        //public partial class Ref
        //{
        //    public enum Nlog
        //    {
        //        [Description("追蹤")]
        //        Trace,
        //        [Description("開發")]
        //        Debug,
        //        [Description("訊息")]
        //        Info,
        //        [Description("警告")]
        //        Warn,
        //        [Description("錯誤")]
        //        Error,
        //        [Description("致命")]
        //        Fatal
        //    }
        //}


    }



    //internal static class Extension
    //{


    //    /// <summary>
    //    /// 寫入 txt
    //    /// </summary>
    //    /// <param name="folderPath"></param>
    //    /// <param name="newtTxt"></param>
    //    /// <param name="oldTxt"></param>
    //    private static void writeTxt(string folderPath, string newtTxt, string oldTxt = null, bool flag = false)
    //    {
    //        if (!folderPath.IsNullOrWhiteSpace() && !newtTxt.IsNullOrWhiteSpace())
    //            using (FileStream fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write))
    //            {
    //                var txtData = string.Empty;
    //                if (!oldTxt.IsNullOrWhiteSpace())
    //                {
    //                    txtData = oldTxt;
    //                    if (!flag)
    //                        txtData += string.Format("\r\n{0}", newtTxt);
    //                    else
    //                        txtData = (string.Format("{0}\r\n{1}", newtTxt, txtData));
    //                }
    //                else
    //                    txtData = newtTxt;
    //                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
    //                sw.Write(txtData); //存檔
    //                sw.Close();
    //            }
    //    }



    //    public static bool IsNullOrWhiteSpace(this string value)
    //    {
    //        return string.IsNullOrWhiteSpace(value);
    //    }



    //}
}
