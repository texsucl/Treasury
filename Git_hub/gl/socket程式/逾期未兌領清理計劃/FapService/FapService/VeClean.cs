using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace FapService
{
    public class VeClean
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public VeClean()
        {
        }


        public static async Task VeCleanApi(Socket socket,string input)
        {
            PostDataVeClean model = new PostDataVeClean();
            Encoding ascii = Encoding.ASCII;
            Encoding ebcdic = Encoding.GetEncoding("IBM037");

            string rtnCode = "";

            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 authorization header
                client.DefaultRequestHeaders.Add("authorization", "token {'6D9310E55EB72CA5D7BBC8F98DD517BC'}");

                // 準備寫入的 data
                PostDataVeClean postData = new PostDataVeClean();
                string[] strInput = input.Split('|');

                if (strInput.Length != 5)
                {
                    rtnCode = "F";
                    postData.rtnCode = "F";
                    //postData.errorMsg = "輸入參數個數錯誤";
                    logger.Error("KEY:" + input + "==>輸入參數個數錯誤");

                }
                else {
                    postData.type = strInput[1].TrimEnd();
                    postData.exec_date = strInput[2].TrimEnd();
                    postData.upd_id = strInput[3].TrimEnd();
                    postData.upd_date = strInput[4].TrimEnd();
                    logger.Info("KEY:" + input + "==>" + "postData.type:" + postData.type);
                    logger.Info("KEY:" + input + "==>" + "postData.exec_date:" + postData.exec_date);
                    logger.Info("KEY:" + input + "==>" + "postData.upd_id:" + postData.upd_id);
                    logger.Info("KEY:" + input + "==>" + "postData.upd_date:" + postData.upd_date);

                    if ("".Equals(postData.exec_date) || "".Equals(postData.upd_id) || "".Equals(postData.upd_date))
                    {
                        logger.Error("KEY:" + input + "==>輸入參數輸入錯誤");
                        rtnCode = "F";
                    }


                    switch (postData.type.TrimEnd())
                    {
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                            break;
                        default:
                            rtnCode = "F";
                            logger.Error("KEY:" + input + "==>輸入參數輸入錯誤");
                            break;
                    }


                    if (!"F".Equals(rtnCode)) {

                        // 將 data 轉為 json
                        string json = JsonConvert.SerializeObject(postData);

                        // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                        HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                        try
                        {
                            //測試
                            var url = Properties.Settings.Default["ProcVeTraceUrl"]?.ToString();

                            //string url = "http://10.240.68.38:8198/VeClean/ProcVeTrace";

                            //正式
                            //string url = "http://GL.fubonlife.com.tw:8098/VeClean/ProcVeTrace";
                            // 發出 post 並取得結果
                            // HttpResponseMessage response = client.PostAsync("http://10.240.68.38:8098/VeClean/ProcVeTrace", contentPost).Result;
                            HttpResponseMessage response = client.PostAsync(url, contentPost).Result;

                            // 將回應結果內容取出
                            var customerJsonString = await response.Content.ReadAsStringAsync();



                            model = JsonConvert.DeserializeObject<PostDataVeClean>(customerJsonString);

                            //將字串以UTF8編碼存入緩衝區
                            rtnCode = model.rtnCode == null ? "S" : model.rtnCode;

                            byte[] dataRtnCode = Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(rtnCode));

                            /*-------回傳AS400訊息--------*/


                            logger.Info("KEY:" + input + "==>" + "rtnCode:" + rtnCode);

                            //socket.Send(Encoding.GetEncoding("IBM037").GetBytes(rtnCode));
                            //socket.Send(dataMailTo);

                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                    }


                    
                }

                socket.Send(Encoding.GetEncoding("IBM037").GetBytes(rtnCode));
                socket.Close();


            }
        }
        }
    }
  







