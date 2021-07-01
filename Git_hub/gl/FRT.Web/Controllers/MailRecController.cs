using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace FRT.Web.Controllers
{
    [RoutePrefix("api/MailRec")]
    public class MailRecController : ApiController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static List<WebSocket> _sockets = new List<WebSocket>();

        [Route]
        [HttpGet]
        public HttpResponseMessage Connect()
        {
           
            HttpContext.Current.AcceptWebSocketRequest(ProcessRequest); //在服务器端接受Web Socket请求，传入的函数作为Web Socket的处理函数，待Web Socket创建后该函数会被调用，在该函数中可以对Web Socket进行消息收发

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols); //构造同意切换至Web Socket的Response.
        }


        public async Task ProcessRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;//传入的context中有当前的web socket对象
            _sockets.Add(socket);//此处将web socket对象加入一个静态列表中

            //进入一个无限循环，当web socket close是循环结束
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);

                var receivedResult = await socket.ReceiveAsync(buffer, CancellationToken.None);//对web socket进行异步接收数据


                logger.Info("receivedResult.MessageType:" + receivedResult.MessageType);


                if (receivedResult.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);//如果client发起close请求，对client进行ack
                    _sockets.Remove(socket);
                    break;
                }

                if (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Encoding utf8 = Encoding.UTF8;
                    Encoding ebcdic = Encoding.GetEncoding("IBM037");

                    //byte[] data = Encoding.ASCII.GetBytes(buffer);
                    byte[] utf8Bytes = Encoding.Convert(ebcdic, utf8, buffer.Array);
                    string fastNo = Encoding.UTF8.GetString(utf8Bytes);

                    logger.Info("fastNo:" + fastNo);

                    //string recvMsg = Encoding.UTF8.GetString(buffer.Array, 0, receivedResult.Count);
                    string recvMsg = "";
                    recvMsg = fastNo.Trim() + " success!!";
                    logger.Info(recvMsg);
                    var recvBytes = Encoding.UTF8.GetBytes(recvMsg);



                    byte[] ebcdicBytes = Encoding.Convert(utf8, ebcdic, recvBytes);


                    byte[] dataS = Encoding.Convert(utf8, ebcdic, Encoding.UTF8.GetBytes("S"));
                    logger.Info("Encoding.UTF8.GetBytes:" + Encoding.UTF8.GetBytes("S").ToString());
                    logger.Info("dataS:" + dataS[0]);

     
                    var sendBuffer = new ArraySegment<byte>(dataS, 0, 1);

                    logger.Info("sendBuffer:" + sendBuffer.Array[0]);
                    await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);


                    //foreach (var innerSocket in _sockets)//当接收到文本消息时，对当前服务器上所有web socket连接进行广播
                    //{
                    //    if (innerSocket != socket)
                    //    {
                    //        logger.Info("send:" + recvMsg);
                    //        await innerSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    //    }
                    //}
                }
            }
        }

        //public class MessageSendController : ApiController
        //{
        //    static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();




        //    private async Task test(AspNetWebSocketContext arg)
        //    {

        //        var web = arg.WebSocket;
        //        while (true)
        //        {
        //            //ArraySegment数组的小抽屉，用于对该数组中元素的范围进行分隔
        //            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
        //            //开始接收
        //            WebSocketReceiveResult result = await web.ReceiveAsync(buffer, CancellationToken.None);
        //            //判断通信状态是否是打开的
        //            if (web.State == WebSocketState.Open)
        //            {
        //                string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        //                //这里可以自己自定义，我当时是用于前端可以做出停止指令来操作控制后台的任务，所以这样写
        //                if (message.Contains("停止"))
        //                {
        //                    //var id = message.Split('|').LastOrDefault()?.ObjTolong();
        //                    //var mesd = Listws.FirstOrDefault(p => p.userid == id);
        //                    //mesd.isstop = true;
        //                }
        //                //这里是接收前端发来的消息，然后做判断的,其中socketMod是自定义的一个实体类，用来存储与客户端连接的信息，比如后台消息要发送到前台时要通过唯一id来在此实体类数组里查找，然后进行指定发送到哪个客户端上，这个可以用来存放在CallContext（线程里唯一）里，这样可以做成简单的聊天器。
        //                //if (message.ObjTolong() > 0 && !Listws.Exists(p => p.userid == message.ObjTolong()))
        //                //{
        //                //    var md = new Models.socketMod();
        //                //    md.SecWebSocketKey = arg.SecWebSocketKey;
        //                //    md.userid = message.ObjTolong();
        //                //    md.webst = web;
        //                //    Listws.Add(md);
        //                //}
        //                //var mes = new BLL.mescontent();
        //                //mes.jdt = "0";
        //                //mes.mess = "成功连接 :" + DateTime.Now.ToLongTimeString();
        //                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("test123"));
        //                //发送消息到前台，这里可以通过调用Listws实体类数组来指定发送或群发
        //                await web.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        //            }
        //            else
        //            {
        //                //if (Listws.Exists(p => p.SecWebSocketKey == arg.SecWebSocketKey))
        //                //{
        //                //    Listws.Remove(Listws.FirstOrDefault(p => p.SecWebSocketKey.Equals(arg.SecWebSocketKey)));
        //                //    SessionHelper.SetCach("websockect", Listws);
        //                //}
        //                //break;
        //            }
        //        }
        //    }

    }
}