using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

                string[] strInput = input.Replace(" ", "")?.Trim()?.Split('|');

                if (strInput.Length < 4)
                {
                    postData.rtnCode = "1";
                    postData.errorMsg = "輸入參數個數錯誤";
                    logger.Error("KEY:" + input + "==>輸入參數個數錯誤");

                }
                else {
                    postData.mailGroup = renameValue(strInput[0]?.Trim());
                    postData.sysType = strInput[1]?.Trim();
                    postData.srceFrom = strInput[2]?.Trim();
                    postData.srceKind = checkValue(strInput[3]?.Trim());
                    logger.Info("KEY:" + input + "==>" + "postData.mailGroup:" + postData.mailGroup);
                    logger.Info("KEY:" + input + "==>" + "postData.sysType:" + postData.sysType);
                    logger.Info("KEY:" + input + "==>" + "postData.srceFrom:" + postData.srceFrom);
                    logger.Info("KEY:" + input + "==>" + "postData.srceKind:" + postData.srceKind);

                    // 將 data 轉為 json
                    string json = JsonConvert.SerializeObject(postData);

                    // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                    try
                    {
                        string url = System.Configuration.ConfigurationManager.AppSettings.Get("callApiUrl");

                        //string url = "http://GL.fubonlife.com.tw:8096/FastRemit/RemitOms";
                        // 發出 post 並取得結果
                        // HttpResponseMessage response = client.PostAsync("http://10.240.68.38:8096/FastRemit/RemitOms", contentPost).Result;
                        logger.Info("url:" + url);
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

        public static string renameValue(string str)
        {
            if (str == "REMITOMS")
                return "REMIT_OMS";
            return str;
        }

        public static string checkValue(string str)
        {
            Regex rx = new Regex(@"^[a-zA-Z0-9]");
            string r = string.Empty;
            foreach (var v in str)
            {
                if (rx.IsMatch(v.ToString()))
                    r += v;
            }
            return r;
        }

    }

       
    }
  







