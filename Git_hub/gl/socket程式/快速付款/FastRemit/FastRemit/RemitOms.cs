using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FastRemit
{
    public class RemitOms
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public RemitOms()
        {
        }


        public static async Task FastRemitApi(Socket socket,string input)
        {
            PostDataRemitOms model = new PostDataRemitOms();
            Encoding ascii = Encoding.ASCII;
            Encoding ebcdic = Encoding.GetEncoding("IBM037");

            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 authorization header
                client.DefaultRequestHeaders.Add("authorization", "token {'6D9310E55EB72CA5D7BBC8F98DD517BC'}");

                // 準備寫入的 data
                PostDataRemitOms postData = new PostDataRemitOms();
                string[] strInput = input.Split('|');

                if (strInput.Length != 4)
                {
                    postData.rtnCode = "1";
                    postData.errorMsg = "輸入參數個數錯誤";
                    logger.Error("KEY:" + input + "==>輸入參數個數錯誤");

                }
                else {
                    postData.sysType = strInput[1];
                    postData.srceFrom = strInput[2];
                    postData.srceKind = strInput[3];
                    logger.Info("KEY:" + input + "==>" + "postData.sysType:" + postData.sysType);
                    logger.Info("KEY:" + input + "==>" + "postData.srceFrom:" + postData.srceFrom);
                    logger.Info("KEY:" + input + "==>" + "postData.srceKind:" + postData.srceKind);

                    // 將 data 轉為 json
                    string json = JsonConvert.SerializeObject(postData);

                    // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                    try
                    {
                        string url = "http://10.240.68.38:8096/FastRemit/RemitOms";
                        //string url = "http://GL.fubonlife.com.tw:8096/FastRemit/RemitOms";
                        // 發出 post 並取得結果
                        // HttpResponseMessage response = client.PostAsync("http://10.240.68.38:8096/FastRemit/RemitOms", contentPost).Result;
                        HttpResponseMessage response = client.PostAsync(url, contentPost).Result;

                        // 將回應結果內容取出
                        var customerJsonString = await response.Content.ReadAsStringAsync();



                        model = JsonConvert.DeserializeObject<PostDataRemitOms>(customerJsonString);

                        //將字串以UTF8編碼存入緩衝區
                        string mailTo = model.mailTo == null ? "" : model.mailTo;
                        string rtnCode = model.rtnCode == null ? "0" : model.rtnCode ;
                        string errorMsg = model.errorMsg == null ? "" : model.errorMsg;

                        byte[] dataMailTo = Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(mailTo));
                        byte[] dataRtnCode = Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(rtnCode));
                        byte[] dataErrorMsg = Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(errorMsg));

                        /*-------回傳AS400訊息--------*/

                        mailTo = mailTo.PadRight(120, ' ');
                        logger.Info("KEY:" + input + "==>" + "mailTo:" + mailTo);
                        logger.Info("KEY:" + input + "==>" + "rtnCode:" + rtnCode);
                        logger.Info("KEY:" + input + "==>" + "errorMsg:" + errorMsg);
                        socket.Send(Encoding.GetEncoding("IBM037").GetBytes(mailTo));
                        //socket.Send(dataMailTo);

                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                    }
                }

                socket.Close();


            }
        }
        }
    }
  







