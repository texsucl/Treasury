using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using System.Threading.Tasks;
using NLog;

namespace eFlowData.Controllers
{
    public class eFlowCountController : ApiController
    {
        /// <summary>
        /// Get api/<controller>/id
        /// </summary>
        /// <param name="id">mail</param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string id)
        {
            var mail = id?.Split('@')[0];
            bool successFlag = false;
            string errMsg = string.Empty;
            string count = string.Empty;
            NlogSet($@"mailId : {mail}");
            StringBuilder returnSb = new StringBuilder();
            returnSb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
            returnSb.AppendLine(@"<Root>");
            try
            {
                var result = await geteFlowCount(mail?.Trim());

                successFlag = result.Item1;
                count = result.Item2;
                errMsg = result.Item3;
                
            }
            catch (Exception ex)
            {
                errMsg =  $@"InnerException:{ex.InnerException},Message:{ex.Message}.";
            } 
      
            if (successFlag)
            {
                //returnSb.AppendLine($@"<Result>Success</Result>");
                //returnSb.AppendLine($@"<Amount>{count}</Amount>");
                //returnSb.AppendLine(@"<ErrCode></ErrCode>");
                //returnSb.AppendLine(@"<ErrMessage></ErrMessage>");
                returnSb.AppendLine($@"<Result><![CDATA[{Environment.NewLine}Success{Environment.NewLine}]]></Result>");
                returnSb.AppendLine($@"<Amount><![CDATA[{Environment.NewLine}{count}{Environment.NewLine}]]></Amount>");
                returnSb.AppendLine(@"<ErrCode></ErrCode>");
                returnSb.AppendLine(@"<ErrMessage></ErrMessage>");
            }
            else
            {
                returnSb.AppendLine(@"<Result><![CDATA[Fail]]></Result>");
                returnSb.AppendLine(@"<Amount></Amount>");
                returnSb.AppendLine(@"<ErrCode>01</ErrCode>");
                returnSb.AppendLine($@"<ErrMessage>{errMsg}</ErrMessage>");
            }
            returnSb.AppendLine(@"</Root>");

            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(returnSb.ToString(), Encoding.UTF8, "application/xml");
            return response;
        }

        async internal Task<Tuple<bool, string , string>> geteFlowCount(string mail)
        {
            bool successFlag = false; //成功註記
            string countString = string.Empty; //回傳數量
            string errmsg = string.Empty; //錯誤訊息
            string MEM_ID = string.Empty; //ID

            try
            {
                if (!string.IsNullOrWhiteSpace(mail))
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DFDBUT"].ConnectionString))
                    {
                        conn.Open();
                        string strSql = string.Empty;
                        strSql = $@"
select MEM_ID from DF_SCH_MEM
where MEM_EMAIL like @MEM_EMAIL 
order by SYS_TYPE
";
                        var resultDSM = conn.QueryFirstOrDefault<dynamic>(strSql,
                        new
                        {
                            MEM_EMAIL = $@"{mail?.Trim()}@%"
                        });
                        MEM_ID = resultDSM == null ? string.Empty : ((IDictionary<string, object>)resultDSM)["MEM_ID"]?.ToString();
                    }
                }
                if (!string.IsNullOrWhiteSpace(MEM_ID))
                {
                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        using (HttpClient client = new HttpClient(handler))
                        {
                            #region 呼叫遠端 Web Service
                            string Url = ConfigurationManager.AppSettings["DeltaflowWS"];
                            HttpResponseMessage response = null;

                            #region  設定相關網址內容
                            //傳入資料
                            var fooJSON = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                      <soap:Envelope xmlns:xsi = ""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd = ""http://www.w3.org/2001/XMLSchema"" xmlns:soap = ""http://schemas.xmlsoap.org/soap/envelope/"">
                                      <soap:Body>
                                        <GetTTLCounts xmlns = ""http://tempuri.org/DeltaflowWS/Flow"">
                                          <UserName>{MEM_ID}</UserName>
                                        </GetTTLCounts>
                                      </soap:Body>
                                    </soap:Envelope> ";
                            //呼叫 Web Service 
                            using (var fooContent = new StringContent(fooJSON, Encoding.UTF8, "text/xml"))
                            {
                                response = await client.PostAsync(Url, fooContent);
                            }
                            //var a = new FlowServiceReference.FlowSoapClient().GetTTLCounts(MEM_ID);
                            #endregion
                            #endregion

                            #region 處理呼叫完成 Web API 之後的回報結果
                            if (response != null)
                            {
                                if (response.IsSuccessStatusCode == true)
                                {
                                    // 取得呼叫完成 API 後的回報內容
                                    String strResult = await response.Content.ReadAsStringAsync();
                                    //回傳的參數
                                    string variable = "<GetTTLCountsResult>";

                                    if (strResult.IndexOf(variable) < 0)
                                        throw new Exception("WEB SERVICE無此參數");

                                    //解析回傳資料
                                    string countStr = strResult.Substring(strResult.IndexOf(variable));
                                    countStr = (countStr.Substring(0, countStr.IndexOf("</")).Remove(0, variable.Length))?.Trim();
                                    NlogSet($@"countStr : {countStr}");
                                    //轉換數值
                                    if (string.IsNullOrWhiteSpace(countStr))
                                    {
                                        countString = "0";
                                    }
                                    else if (!isNum(countStr))
                                    {
                                        //只要回傳的不是數字，一律都顯示"?"
                                        countString = "?";
                                    }
                                    else
                                    {
                                        successFlag = true;
                                        if (Convert.ToInt32(countStr) > 9999)
                                        { //數量超過9999則顯示 "+"
                                            countString = "9999+";
                                        }
                                        else
                                        {
                                            countString = countStr;
                                        }
                                    }
                                }
                                else
                                {
                                    errmsg = $@"WEB SERVICE呼叫失敗 Error Code:{response.StatusCode }, Error Message:{response.RequestMessage}";
                                }
                            }
                            else
                            {
                                errmsg = $@"呼叫WEB SERVICE 發生異常";
                            }
                            #endregion                                                          
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errmsg = $@"InnerException:{ex.InnerException},Message:{ex.Message}.";
            }
           
            return new Tuple<bool, string , string>(successFlag, countString, errmsg);
        }

        private bool isNum(string str)
        {
            bool flag = false;
            int i = 0;
            if (string.IsNullOrWhiteSpace(str))
                return flag;
            return Int32.TryParse(str?.Trim(), out i);
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void NlogSet(string message, Nlog nlog = Nlog.Info)
        {
            switch (nlog)
            {
                // 用於追蹤，可以在程式裡需要追蹤的地方將訊息以Trace傳出。
                case Nlog.Trace:
                    logger.Trace(message);
                    break;
                // 用於開發，於開發時將一些需要特別關注的訊息以Debug傳出。
                case Nlog.Debug:
                    logger.Debug(message);
                    break;
                // 訊息，記錄不影響系統執行的訊息，通常會記錄登入登出或是資料的建立刪除、傳輸等。
                case Nlog.Info:
                    logger.Info(message);
                    break;
                // 警告，用於需要提示的訊息，例如庫存不足、貨物超賣、餘額即將不足等。
                case Nlog.Warn:
                    logger.Warn(message);
                    break;
                // 錯誤，記錄系統實行所發生的錯誤，例如資料庫錯誤、遠端連線錯誤、發生例外等。
                case Nlog.Error:
                    logger.Error(message);
                    break;
                // 致命，用來記錄會讓系統無法執行的錯誤，例如資料庫無法連線、重要資料損毀等。
                case Nlog.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }

        public enum Nlog
        {
            /// <summary>
            /// 用於追蹤，可以在程式裡需要追蹤的地方將訊息以Trace傳出。
            /// </summary>
            Trace = 1,

            /// <summary>
            /// 用於開發，於開發時將一些需要特別關注的訊息以Debug傳出。
            /// </summary>
            Debug = 2,

            /// <summary>
            /// 訊息，記錄不影響系統執行的訊息，通常會記錄登入登出或是資料的建立刪除、傳輸等。
            /// </summary>
            Info = 0,

            /// <summary>
            /// 警告，用於需要提示的訊息，例如庫存不足、貨物超賣、餘額即將不足等。
            /// </summary>
            Warn = 3,

            /// <summary>
            /// 錯誤，記錄系統實行所發生的錯誤，例如資料庫錯誤、遠端連線錯誤、發生例外等。
            /// </summary>
            Error = 4,

            /// <summary>
            /// 致命，用來記錄會讓系統無法執行的錯誤，例如資料庫無法連線、重要資料損毀等。
            /// </summary>
            Fatal = 5,
        }
    }
}
