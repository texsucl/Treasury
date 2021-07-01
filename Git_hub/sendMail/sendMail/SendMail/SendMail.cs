using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Http;
using Newtonsoft.Json;

/// <summary>
/// 寄信 namespace
/// </summary>
namespace SendMail
{
    /// <summary>
    /// 寄信
    /// </summary>
    public class SendMailSelf
    {

        #region Mail 參數

        /// <summary>
        /// 寄信標題
        /// </summary>
        public string mailTitle { get; set; }

        /// <summary>
        /// 寄信smtp server
        /// </summary>
        public string smtpServer { get; set; }

        /// <summary>
        /// 寄信smtp server的Port，預設25
        /// </summary>
        public int smtpPort { get; set; } = 25;

        /// <summary>
        /// 寄信帳號
        /// </summary>
        public string mailAccount { get; set; }

        /// <summary>
        /// 寄信密碼
        /// </summary>
        public string mailPwd { get; set; }

        #endregion

        /// <summary>
        /// 完整的寄信函數 , MailFrom(寄信人位置,寄信人名稱) , MailTos(收信人位置,收信人名稱) , Ccs(副本人位置,副本人名稱) , MailSub(主旨) , MailBody(內文) , isBodyHtml(是否為Html格式) , files(要夾帶的附檔) , 如果失敗回傳錯誤訊息
        /// </summary>
        /// <param name="MailFrom">寄信人Email Address 欄位一為位置,欄位二為名稱</param>
        /// <param name="MailTos">收信人Email Address 欄位一為位置,欄位二為名稱</param>
        /// <param name="Ccs">副本Email Address 欄位一為位置,欄位二為名稱</param>
        /// <param name="MailSub">主旨</param>
        /// <param name="MailBody">內文</param>
        /// <param name="isBodyHtml">是否為Html格式</param>
        /// <param name="files">要夾帶的附檔</param>
        /// <returns>回傳寄信是否成功(空值:成功,錯誤訊息:失敗)</returns>
        public string Mail_Send(Tuple<string, string> MailFrom, IEnumerable<Tuple<string, string>> MailTos, IEnumerable<Tuple<string, string>> Ccs, string MailSub, string MailBody, bool isBodyHtml = false, Dictionary<string, System.IO.Stream> files = null)
        {
            string result = string.Empty;
            try
            {
                //沒給寄信人mail address
                if (string.IsNullOrWhiteSpace(MailFrom.Item1))
                {//※有些公司的Smtp Server會規定寄信人的Domain Name須是該Smtp Server的Domain Name，例如底下的 system.com.tw
                    return "無寄信人";
                }

                //命名空間： System.Web.Mail已過時，http://msdn.microsoft.com/zh-tw/library/system.web.mail.mailmessage(v=vs.80).aspx
                //建立MailMessage物件
                MailMessage mms = new MailMessage();
                //指定一位寄信人MailAddress
                if (!string.IsNullOrWhiteSpace(MailFrom.Item2))
                    mms.From = new MailAddress(MailFrom.Item1.Trim(), MailFrom.Item2.Trim());
                else
                    mms.From = new MailAddress(MailFrom.Item1.Trim());
                //信件主旨
                mms.Subject = MailSub;
                //信件內容
                mms.Body = MailBody;
                //信件內容 是否採用Html格式
                mms.IsBodyHtml = isBodyHtml;

                if (MailTos != null && MailTos.Any())//防呆
                {
                    bool MailToFlag = false;
                    foreach (var MailTo in MailTos)
                    {
                        if (!string.IsNullOrWhiteSpace(MailTo.Item1))
                        {
                            MailToFlag = true;
                            if (!string.IsNullOrWhiteSpace(MailTo.Item2))
                                mms.To.Add(new MailAddress(MailTo.Item1, MailTo.Item2));
                            else
                                mms.To.Add(new MailAddress(MailTo.Item1));
                        }
                    }
                    if (!MailToFlag)
                        return "無收信人";
                }//End if//防呆
                if (Ccs != null && Ccs.Any()) //防呆
                {
                    foreach (var Cc in Ccs)
                    {
                        if (!string.IsNullOrWhiteSpace(Cc.Item1))
                        {
                            if (!string.IsNullOrWhiteSpace(Cc.Item2))
                                mms.CC.Add(new MailAddress(Cc.Item1, Cc.Item2));
                            else
                                mms.CC.Add(new MailAddress(Cc.Item1));
                        }
                    }
                }//End if (Ccs!=null) //防呆
                //附件處理
                if (files != null && files.Count > 0)//寄信時有夾帶附檔
                {
                    foreach (string fileName in files.Keys)
                    {
                        Attachment attfile = new Attachment(files[fileName], fileName);
                        mms.Attachments.Add(attfile);
                    }//end foreach
                }//end if 
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))//或公司、客戶的smtp_server
                {
                    if (!string.IsNullOrEmpty(mailAccount) && !string.IsNullOrEmpty(mailPwd))//.config有帳密的話
                    {//寄信要不要帳密？眾說紛紜Orz，分享一下經驗談....

                        //網友阿尼尼:http://www.dotblogs.com.tw/kkc123/archive/2012/06/26/73076.aspx
                        //※公司內部不用認證,寄到外部信箱要特別認證 Account & Password

                        //自家公司MIS:
                        //※要看smtp server的設定呀~

                        //結論...
                        //※程式在客戶那邊執行的話，問客戶，程式在自家公司執行的話，問自家公司MIS，最準確XD
                        client.Credentials = new System.Net.NetworkCredential(mailAccount, mailPwd);//寄信帳密
                    }
                    client.Send(mms);//寄出一封信
                }//end using 
                 //釋放每個附件，才不會Lock住
                if (mms.Attachments != null && mms.Attachments.Count > 0)
                {
                    for (int i = 0; i < mms.Attachments.Count; i++)
                    {
                        mms.Attachments[i].Dispose();
                    }
                }

                return result;//成功
            }
            catch (Exception ex)
            {
                //寄信失敗，寫Log文字檔
                //NLog.LogManager.GetCurrentClassLogger().ErrorException("寄信失敗", ex);
                return $"message: {ex.Message}" +
                   $", inner message {ex.InnerException}";
            }
        }//End 寄信

        private static HttpClient HttpClient;

        #region M_plus 參數

        /// <summary>
        /// M+ server location
        /// </summary>
        public string Mplus_BaseAddress { get; set; }

        /// <summary>
        /// 執行傳送 M+ 的Url
        /// </summary>
        public string Mplus_Url { get; set; }

        #endregion


        /// <summary>
        /// 發送 M+ 函數 
        /// </summary>
        /// <param name="systemName">(必要)於Web Service平台註冊的系統名稱 (ex:MAPP)</param>
        /// <param name="mplusId">(必要)發送訊息的企業帳號代碼 (ex:DCC => 行政企業帳號)</param>
        /// <param name="target">(必要)發送對象的Email Address或手機門號，因業務員會有共用手機門號的情形，所以建議使用O365的Email進行訊息發送(ex:xxx.xx@fubon.com or 09xxxxxxxx)</param>
        /// <param name="sendType">(必要)寄送類型 (ex: T => 文字,U => 圖片URL路徑,I => 圖片檔案路徑,圖片容量大小不可超過500KB)</param>
        /// <param name="message">(必要)訊息內容 </param>
        /// <param name="policyNo">保單號碼</param>
        /// <param name="hideSysName">隱藏系統名稱 (ex: Y)</param>
        /// <param name="senderId">發訊人員5碼AD帳號</param>
        /// <param name="senderName">發訊人員姓名</param>
        /// <param name="unitId">發訊人員單位代碼</param>
        /// <returns>參數1:成功失敗 , 參數2:Api的returCode 中文 , 參數3:Api的returnMsg , 參數4:guid</returns>
        public Tuple<bool,string,string,string> Mplus_Send(string systemName, string mplusId, string target, string sendType, string message, string policyNo = null, string hideSysName = null, string senderId = null, string senderName = null, string unitId = null)
        {
            bool _resultFlg = false;
            string _resultMsg = string.Empty;
            string _returnMessage = string.Empty;
            string _guid = string.Empty;
            try
            {
                HttpClient = new HttpClient();
                HttpClient.BaseAddress = new System.Uri(Mplus_BaseAddress);
                HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var _request = new Mplus_Request()
                {
                    systemName = systemName,
                    mplusId = mplusId,
                    target = target,
                    message = message,
                    hideSysName = hideSysName,
                    senderId = senderId,
                    policyNo= policyNo,
                    senderName = senderName,
                    unitId = unitId,
                    sendType = sendType
                };
                var response = HttpClient.PostAsJsonAsync($@"{Mplus_Url}", _request).Result;
                if (response.IsSuccessStatusCode)
                {
                    Mplus_Response data = JsonConvert.DeserializeObject<Mplus_Response>(response.Content.ReadAsStringAsync().Result);

                    switch (data?.returnCode)
                    {
                        case "00":
                            _resultMsg = "API 呼叫成功";
                            _resultFlg = true;
                            break;
                        case "20":
                            _resultMsg = $@"參數錯誤";
                            break;
                        case "21":
                            _resultMsg = $@"無操作權限";
                            break;
                        case "23":
                            _resultMsg = $@"無發訊權限，請洽貴司負責窗口";
                            break;
                        case "30":
                            _resultMsg = $@"該用戶未加入企業帳號";
                            break;
                        case "38":
                            _resultMsg = $@"用戶 未註冊 M+";
                            break;
                        case "54":
                            _resultMsg = $@"發送名單內容錯誤";
                            break;
                        case "C1":
                            _resultMsg = $@"保單號碼格式錯誤";
                            break;
                        case "E1":
                            _resultMsg = $@"網路錯誤";
                            break;
                        case "E2":
                            _resultMsg = $@"系統錯誤";
                            break;
                        case "E3":
                            _resultMsg = $@"輸入錯誤 (發送對象非Email或手機門號/M+企業帳號不存在)";
                            break;
                        case "E4":
                            _resultMsg = $@"檔案不存在";
                            break;
                        case "E5":
                            _resultMsg = $@"檔案容量超過500KB";
                            break;
                        default:
                            _resultMsg = $@"returnCode : {data.returnCode}, ";
                            break;
                    }
                    _returnMessage = data?.returnMessage;
                    _guid = data?.guid;
                }
                else
                {
                    _resultMsg = string.Format("Status Code : {0}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _resultMsg = $"message: {ex.Message}" +
                   $", inner message {ex.InnerException}";
            }
            finally {
                HttpClient.Dispose();
            }
            return new Tuple<bool, string, string,string>(_resultFlg, _resultMsg, _returnMessage, _guid);
        }
    }

    public class Mplus_Response
    {
        /// <summary>
        /// 於Web Service平台註冊的系統名稱
        /// </summary>
        public string systemName { get; set; }

        /// <summary>
        /// 訊息序號，呼叫系統建議紀錄此序號
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 結果代碼
        /// </summary>
        public string returnCode { get; set; }

        //結果代碼
        //00	API 呼叫成功
        //20	參數錯誤
        //21	無操作權限
        //23	無發訊權限，請洽貴司負責窗口
        //30	該用戶未加入企業帳號
        //38	用戶 未註冊 M+
        //54	發送名單內容錯誤
        //C1    保單號碼格式錯誤
        //E1    網路錯誤
        //E2    系統錯誤
        //E3    輸入錯誤 (發送對象非Email或手機門號/M+企業帳號不存在)
        //E4    檔案不存在
        //E5    檔案容量超過500KB

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string returnMessage { get; set; }
    }

    public class Mplus_Request
    {
        /// <summary>
        /// (必要)於Web Service平台註冊的系統名稱 (ex:MAPP)
        /// </summary>
        public string systemName { get; set; }

        /// <summary>
        /// (必要)發送訊息的企業帳號代碼 (ex:DCC => 行政企業帳號)
        /// </summary>
        public string mplusId { get; set; }

        /// <summary>
        /// (必要)發送對象的Email Address或手機門號，因業務員會有共用手機門號的情形，所以建議使用O365的Email進行訊息發送(ex:xxx.xx@fubon.com or 09xxxxxxxx)
        /// </summary>
        public string target { get; set; }

        /// <summary>
        /// (必要)訊息內容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 隱藏系統名稱 (ex: Y)
        /// </summary>
        public string hideSysName { get; set; }

        /// <summary>
        /// 發訊人員5碼AD帳號
        /// </summary>
        public string senderId { get; set; }

        /// <summary>
        /// 保單號碼
        /// </summary>
        public string policyNo { get; set; }

        /// <summary>
        /// 發訊人員姓名
        /// </summary>
        public string senderName { get; set; }

        /// <summary>
        /// 發訊人員單位代碼
        /// </summary>
        public string unitId { get; set; }

        /// <summary>
        /// (必要)寄送類型 (ex: T => 文字,U => 圖片URL路徑,I => 圖片檔案路徑,圖片容量大小不可超過500KB)
        /// </summary>
        public string sendType { get; set; }
    }
}
