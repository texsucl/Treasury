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
    public class VeTraceStatus
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public VeTraceStatus()
        {
        }


        public static async Task VeTraceStatusApi(Socket socket,string input)
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
                PostDataVeTraceStatus postData = new PostDataVeTraceStatus();
                string[] strInput = input.Split('|');

                if (strInput.Length != 6)
                {
                    rtnCode = "F";
                    postData.rtnCode = "F";
                    //postData.errorMsg = "輸入參數個數錯誤";
                    logger.Error("KEY:" + input + "==>輸入參數個數錯誤");

                }
                else {
                    postData.check_acct_short = strInput[1].TrimEnd();
                    postData.check_no = strInput[2].TrimEnd();
                    postData.re_paid_date = strInput[3].TrimEnd();
                    postData.re_paid_type = strInput[4].TrimEnd();
                    postData.upd_id = strInput[5].TrimEnd();
                    logger.Info("KEY:" + input + "==>" + "postData.check_acct_short:" + postData.check_acct_short);
                    logger.Info("KEY:" + input + "==>" + "postData.check_no:" + postData.check_no);
                    logger.Info("KEY:" + input + "==>" + "postData.re_paid_date:" + postData.re_paid_date);
                    logger.Info("KEY:" + input + "==>" + "postData.re_paid_type:" + postData.re_paid_type);
                    logger.Info("KEY:" + input + "==>" + "postData.upd_id:" + postData.upd_id);


                    //modify by diayu 20190906 再給付方式、異動人員  可不輸入
                    if ("".Equals(postData.check_acct_short) || "".Equals(postData.check_no) || "".Equals(postData.re_paid_date))
                    {
                        logger.Error("KEY:" + input + "==>輸入參數輸入錯誤");
                        rtnCode = "F";
                    }


                    if (!"F".Equals(rtnCode)) {

                        // 將 data 轉為 json
                        string json = JsonConvert.SerializeObject(postData);

                        // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                        HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                        try
                        {
                            var url = Properties.Settings.Default["ProcVeTraceStatusUrl"]?.ToString();
                            //正式
                            //string url = "http://GL.fubonlife.com.tw:8098/VeClean/ProcVeTraceStatus";

                            //測試
                            //string url = "http://10.240.68.38:8198/VeClean/ProcVeTraceStatus";
        
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
  







