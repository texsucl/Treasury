using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TransferWorkingKey.Model;
using System.Web;
using System.Collections;
using NLog;
using SendMail;
using System.Transactions;

namespace TransferWorkingKey
{
    public class HSM
    {
        static string cdKeyLabel = "CDKeyforTest_20180820";
        static string WorkingKeyLabel = $"WorkingKey_{DateTime.Now.DayOfWeek.ToString("d")}";
        static string IV = "0000000000000000";
        static void Main(string[] args)
        {
            bool successFlag = false; //成功註記
            //creating object of program class to access methods  
            HSM obj = new HSM();
            WriteLog("程式開始");
            //Console.WriteLine("===程式開始===");
            //Console.WriteLine(string.Empty);
            DateTime dt = DateTime.MinValue;
            TimeSpan ts = TimeSpan.MinValue;
            try
            {
                var _cdKeyLabel = Properties.Settings.Default["CDKeyLabel"]?.ToString();
                var _WorkingKeyLabel = Properties.Settings.Default["WorkingKeyLabel"]?.ToString();
                if (!string.IsNullOrWhiteSpace(_cdKeyLabel))
                    cdKeyLabel = _cdKeyLabel;
                if (!string.IsNullOrWhiteSpace(_WorkingKeyLabel))
                    WorkingKeyLabel = _WorkingKeyLabel;
                int HSMReTry = 3; //HSM接收失敗重新次數
                int _HSMReTry = 0;
                if (Int32.TryParse(Properties.Settings.Default["HSMReTry"]?.ToString(), out _HSMReTry))
                    HSMReTry = _HSMReTry;
                int BankReTry = 1; //富邦銀行接收失敗重新次數
                int _BankReTry = 0;
                if (Int32.TryParse(Properties.Settings.Default["BankReTry"]?.ToString(), out _BankReTry))
                    BankReTry = _BankReTry;
                ChangeWorkingKeyResultModel CWKR = new ChangeWorkingKeyResultModel();
                HSMResultModel HSM = new HSMResultModel();
                var FRT = new FRT_WorkingKey();
                using (db_HSM_GL_Entities db = new db_HSM_GL_Entities())
                {
                    #region Call Service
                    for (var i = 0; (i < HSMReTry) && CWKR.ErrorCode != "0"; i++)
                    {
                        dt = DateTime.Now.Date;
                        ts = DateTime.Now.TimeOfDay;
                        CWKR = (ChangeWorkingKeyResultModel)obj.InvokeService(url.HSMUrl_ChangeWorkingKey);
                        FRT = new FRT_WorkingKey()
                        {
                            CreateDate = dt,
                            CreateTime = ts,
                            WorkingKeyLabel = CWKR.WorkingKeyLabel,
                            ErrorCode = CWKR.ErrorCode,
                            ErrorMessage = CWKR.ErrorMsg,
                            ECDKey = CWKR.ECDKey,
                            IsActive = "N",
                            Random = CWKR.Random,
                            Random1 = CWKR.Random1,
                        };
                        db.FRT_WorkingKey.Add(FRT);
                        try
                        {
                            //要存資料庫請拿掉下方註解
                            WriteLog($"新增資料 : {FRT.modelToString()}");
                            db.SaveChanges();
                            WriteLog($"新增資料 : 成功");
                            FRT = db.FRT_WorkingKey.FirstOrDefault(x =>
                                x.WorkingKeyLabel == FRT.WorkingKeyLabel &&
                                x.CreateDate == FRT.CreateDate &&
                                x.CreateTime == FRT.CreateTime);
                            WriteLog($"查詢資料 : {FRT.modelToString()}");
                            //Console.WriteLine($"新增資料 : {FRT.modelToString()}");
                        }
                        catch (DbUpdateException ex)
                        {
                            WriteLog($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}",Ref.Nlog.Error);
                            //Console.WriteLine($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}");
                        }
                    }
                    for (var i = 0; i < BankReTry; i++)
                    {
                        HSM = (HSMResultModel)obj.InvokeService(url.FubonBankUrl_1, CWKR);
                        try
                        {
                            var _chang = db.FRT_WorkingKey.FirstOrDefault(x => x.IsActive == "Y");
                            if (_chang != null)
                                _chang.IsActive = "R";
                            FRT.Random2 = HSM.RANDOM2;
                            FRT.FubonBankXml = HSM.FubonBankXml;
                            //FRT.FubonBankXml = string.Empty;
                            FRT.IsActive = "Y";

                            WriteLog($"Random2:{FRT.Random2}");
                            WriteLog($"FubonBankXml:{FRT.FubonBankXml}");
                            //Console.WriteLine("val:" + db.GetValidationErrors().getValidateString());
                            db.SaveChanges();
                            //Console.WriteLine($"Random2:{FRT.Random2}");
                            var s = (BitConverter.ToString(Encoding.Default.GetBytes(FRT.Random2)))?.Replace("-", "");
                            //Console.WriteLine($"Random2(Hex):{s}");
                            //var s = HttpUtility.UrlDecode(FRT.Random2, Encoding.GetEncoding("big5"));
                            //Console.WriteLine($"Random2:{s}");
                            //s = HttpUtility.UrlDecode(FRT.Random2, Encoding.UTF8);
                            //Console.WriteLine($"Random2:{s}");
                            //s = HttpUtility.UrlDecode(FRT.Random2, Encoding.ASCII);
                            //Console.WriteLine($"Random2:{s}");
                        }
                        catch (DbUpdateException ex)
                        {
                            WriteLog($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}",Ref.Nlog.Error);
                            //Console.WriteLine($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}");
                        }
                    }
                    for (var i = 0; i < BankReTry; i++)
                    {
                        HSM = (HSMResultModel)obj.InvokeService(url.FubonBankUrl_2, CWKR);
                        try
                        {
                            FRT.Random2_2 = HSM.RANDOM2;
                            FRT.FubonBankXml_2 = HSM.FubonBankXml;
                            if (string.IsNullOrWhiteSpace(FRT.Random2_2)) //回傳無參數 => 失敗
                            {
                                FRT.IsActive = "N";
                                var _chang = db.FRT_WorkingKey.FirstOrDefault(x => x.IsActive == "R");
                                if (_chang != null)
                                    _chang.IsActive = "Y";
                            }
                            else
                            {
                                successFlag = true;
                                foreach (var item in db.FRT_WorkingKey.Where(x => x.IsActive == "R"))
                                {
                                    item.IsActive = "N";
                                }
                            }
                            //Console.WriteLine("val:" + db.GetValidationErrors().getValidateString());
                            WriteLog($"Random2_2:{FRT.Random2_2}");
                            //Console.WriteLine($"Random2_2:{FRT.Random2_2}");
                            var s = HttpUtility.UrlDecode(FRT.Random2_2, Encoding.GetEncoding("big5"));
                            WriteLog($"Random2_2:{s}");
                            db.SaveChanges();
                            //Console.WriteLine($"Random2_2:{s}");
                        }
                        catch (DbUpdateException ex)
                        {
                            WriteLog($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}",Ref.Nlog.Error);
                            //Console.WriteLine($"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}");
                        }
                    }
                    #endregion
                }
                
            }
            catch (Exception ex)
            {
                WriteLog($"InnerException:{ex.InnerException},Message:{ex.Message}",Ref.Nlog.Error);
                //Console.WriteLine($"InnerException:{ex.InnerException},Message:{ex.Message}");
            }
            finally
            {
                WriteLog("寄信開始");
                int mailCount = sendMail(successFlag, "TRANSFERWORKINGKEY");
                WriteLog($@"寄送信件:{mailCount}");
                WriteLog("寄信結束");
                //Console.WriteLine(string.Empty);
                WriteLog("程式結束");
                //Console.WriteLine("===程式結束===");
            }
        }

        /// <summary>
        /// 委派抓資料事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">http request</param>
        /// <param name="xml">發送的xml資料</param>
        /// <param name="model">回傳資料</param>
        /// <param name="type">傳送類別</param>
        /// <returns></returns>
        internal delegate IResultModel GetData(HttpWebRequest request, string xml, IResultModel model, url type, byte[] bs = null);

        /// <summary>
        /// 呼叫 service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        internal IResultModel InvokeService(url type, IResultModel data = null)
        {
            GetData myFunc = XmlToData; //抓取資料事件       
            string xml = string.Empty;
            //Console.WriteLine("===傳送資料部分===");
            //Console.WriteLine(string.Empty);
            IResultModel model = null;      
              
            //XmlWriterSettings setting = new XmlWriterSettings();
            //setting.Indent = true;
            //setting.OmitXmlDeclaration = false;
            //setting.NewLineOnAttributes = true;
            //setting.Encoding = Encoding.GetEncoding("big5");
            //XmlWriter xmlwriter = null;
            //MemoryStream ms = new MemoryStream();

            byte[] bs = null;
            switch (type)
            {
                case url.HSMUrl_ChangeWorkingKey:
                    //Console.WriteLine("===ChangeWorkingKey部分===");
                    //Console.WriteLine("===傳送參數===");
                    //Console.WriteLine($"CDKeyLabel:{cdKeyLabel}");
                    //Console.WriteLine($"WorkingKeyLabel:{WorkingKeyLabel}");
                    model = new ChangeWorkingKeyResultModel();
                    ((ChangeWorkingKeyResultModel)model).WorkingKeyLabel = WorkingKeyLabel;
                    xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>  
                          <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.hsm.twca.com.tw/"">  
                            <soap:Header/>
                            <soap:Body>  
                              <ser:ChangeWorkingKey>
                                  <CDKeyLabel>{cdKeyLabel}</CDKeyLabel>
                                  <WorkingKeyLabel>{WorkingKeyLabel}</WorkingKeyLabel>
                              </ser:ChangeWorkingKey>
                            </soap:Body>  
                          </soap:Envelope>";
                    break;
                case url.HSMUrl_MAC:
                    //Console.WriteLine("===MAC部分===");
                    //Console.WriteLine("===傳送參數===");
                    var _MACdata = ((MACResultModel)data).Data;
                    //Console.WriteLine($"WorkingKeyLabel:{WorkingKeyLabel}");
                    //Console.WriteLine($"Data:{_MACdata}");
                    //Console.WriteLine($"IV:{IV}");
                    model = data;
                    xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>  
                          <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.hsm.twca.com.tw/"">
                            <soap:Header/>
                             <soap:Body>
                                 <ser:MAC>
                                     <WorkingKeyLabel>{WorkingKeyLabel}</WorkingKeyLabel>
                                     <Data>{_MACdata}</Data>
                                     <IV>{IV}</IV>
                                 </ser:MAC>
                             </soap:Body>
                          </soap:Envelope>";
                    break;
                case url.FubonBankUrl_1:
                    //Console.WriteLine("===FubonBank第一次部分===");
                    //Console.WriteLine("===傳送參數===");
                    var _data = (ChangeWorkingKeyResultModel)data;
                    //Console.WriteLine($"ECDKey:{_data.ECDKey}");
                    //Console.WriteLine($"Random:{_data.Random}");
                    //Console.WriteLine($"Random1:{_data.Random1}");
                    model = new HSMResultModel();
                    xml = $@"
                          <Tx>
                             <FMPConnectionString>
                             <SPName>FBL_FPS</SPName>
                             <LoginID>FBL_FPS</LoginID>
                             <Password></Password>
                             <TxnId>FPS622150</TxnId>
                             </FMPConnectionString>
                             <TxHead>
                             <HWSID>FBL_FPS</HWSID>
                             <HSTANO>{DateTime.Now.ToString("HHmmssf")}</HSTANO>
                             <HTLID>7154151</HTLID>
                             <HTXTID>FPS622150</HTXTID>
                             </TxHead>
                             <TxBody>
                                <ECDKEY>{_data.ECDKey}</ECDKEY>
                                <RANDOM>{_data.Random}</RANDOM>
                                <RANDOM1>{_data.Random1}</RANDOM1>
                                <CONFIRM></CONFIRM>
                             </TxBody>
                          </Tx>
                          ";
                    //Console.WriteLine($"xml:{xml}");
                    //xmlwriter = XmlWriter.Create(ms, setting);
                    //xmlwriter.WriteStartElement("Tx");
                    //xmlwriter.WriteStartElement("FMPConnectionString");
                    //xmlwriter.WriteElementString("SPName", "");
                    //xmlwriter.WriteElementString("LoginID", "");
                    //xmlwriter.WriteElementString("Password", "");
                    //xmlwriter.WriteElementString("TxnId", "");
                    //xmlwriter.WriteEndElement();
                    //xmlwriter.WriteStartElement("TxHead");
                    //xmlwriter.WriteElementString("HWSID", "");
                    //xmlwriter.WriteElementString("HSTANO", "");
                    //xmlwriter.WriteElementString("HTLID", "");
                    //xmlwriter.WriteElementString("HTXTID", "");
                    //xmlwriter.WriteEndElement();
                    //xmlwriter.WriteStartElement("TxBody");
                    //xmlwriter.WriteElementString("ECDKEY", _data.ECDKey);
                    //xmlwriter.WriteElementString("RANDOM", _data.Random);
                    //xmlwriter.WriteElementString("HTLID", _data.Random1);
                    //xmlwriter.WriteElementString("CONFIRM", "");
                    //xmlwriter.WriteEndElement();
                    //xmlwriter.WriteEndElement();
                    //xmlwriter.Flush();
                    bs = Encoding.GetEncoding(950).GetBytes(xml);
                    //bs = Encoding.UTF8.GetBytes(xml);
                    ((HSMResultModel)model).FubonBankXml = xml;
                    break;
                case url.FubonBankUrl_2:
                    //Console.WriteLine("===FubonBank第二次部分===");
                    //Console.WriteLine("===傳送參數===");
                    var _data2 = (ChangeWorkingKeyResultModel)data;
                    //Console.WriteLine($"ECDKey:{_data2.ECDKey}");
                    //Console.WriteLine($"Random:{_data2.Random}");
                    //Console.WriteLine($"Random1:{_data2.Random1}");
                    model = new HSMResultModel();
                    xml = $@"
                          <Tx>
                             <FMPConnectionString>
                             <SPName>FBL_FPS</SPName>
                             <LoginID>FBL_FPS</LoginID>
                             <Password></Password>
                             <TxnId>FPS622150</TxnId>
                             </FMPConnectionString>
                             <TxHead>
                             <HWSID>FBL_FPS</HWSID>
                             <HSTANO>{DateTime.Now.ToString("HHmmssf")}</HSTANO>
                             <HTLID>7154151</HTLID>
                             <HTXTID>FPS622150</HTXTID>
                             </TxHead>
                             <TxBody>
                                <ECDKEY>{_data2.ECDKey}</ECDKEY>
                                <RANDOM>{_data2.Random}</RANDOM>
                                <RANDOM1>{_data2.Random1}</RANDOM1>
                                <CONFIRM>Y</CONFIRM>
                             </TxBody>
                          </Tx>
                          ";
                    //Console.WriteLine($"xml:{xml}");
                    bs = Encoding.GetEncoding(950).GetBytes(xml);
                    //bs = Encoding.UTF8.GetBytes(xml);
                    ((HSMResultModel)model).FubonBankXml = xml;
                    break;
            }
            return myFunc.Invoke(CreateSOAPWebRequest(type), xml, model, type, bs);
        }

        /// <summary>
        /// 呼叫httpRequest 並把Xml資料轉入類別中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">http request</param>
        /// <param name="xml">發送的xml資料</param>
        /// <param name="model">回傳資料</param>
        /// <param name="type">傳送類別</param>
        /// <returns></returns>
        internal IResultModel XmlToData(HttpWebRequest request, string xml, IResultModel model, url type, byte[] bs = null)
        {
            var str = string.Empty;
            //Console.WriteLine("===接收資料部分===");
            //Console.WriteLine(string.Empty);
            switch (type)
            {
                //utf-8
                case url.HSMUrl_ChangeWorkingKey:
                case url.HSMUrl_MAC:
                    #region utf-8
                    XmlDocument SOAPReqBody = new XmlDocument();

                    SOAPReqBody.LoadXml(xml);
                    //丟資料
                    using (Stream stream = request.GetRequestStream())
                    {
                        SOAPReqBody.Save(stream);
                    }
                    //Geting response from request  
                    using (WebResponse Serviceres = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                        {
                            var ServiceResult = rd.ReadToEnd();
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(ServiceResult);
                            switch (type)
                            {
                                case url.HSMUrl_ChangeWorkingKey:
                                    foreach (XmlNode item in doc.ChildNodes)
                                    {
                                        //Console.WriteLine(item?.InnerText);
                                        WriteLog(item?.InnerText);
                                    }
                                    //Console.WriteLine(string.Empty);
                                    str = doc.SelectSingleNode("//return")?.FirstChild?.Value;
                                    foreach (var item in model.GetType().GetProperties())
                                    {
                                        var _name = item.Name;
                                        var val = "錯誤的參數!";
                                        if (!string.IsNullOrWhiteSpace(str) && str.IndexOf($"<{_name}>") > -1)
                                        {
                                            var start = str.IndexOf($"<{_name}>") + $"<{_name}>".Length;
                                            val = str.Substring(start, (str.IndexOf($"</{_name}>") - start));
                                            item.SetValue(model, val);
                                            //WriteLog($"{_name}:{val}");
                                            //Console.WriteLine($"{_name}:{val}");
                                        }
                                    }
                                    break;
                                case url.HSMUrl_MAC:
                                    foreach (XmlNode item in doc.ChildNodes)
                                    {
                                        //WriteLog(item?.InnerText);
                                        //Console.WriteLine(item?.InnerText);
                                    }
                                    //Console.WriteLine(string.Empty);
                                    str = doc.SelectSingleNode("//return")?.FirstChild?.Value;
                                    foreach (var item in model.GetType().GetProperties())
                                    {
                                        var _name = item.Name;
                                        var val = "錯誤的參數!";
                                        if (!string.IsNullOrWhiteSpace(str) && str.IndexOf($"<{_name}>") > -1)
                                        {
                                            var start = str.IndexOf($"<{_name}>") + $"<{_name}>".Length;
                                            val = str.Substring(start, (str.IndexOf($"</{_name}>") - start));
                                            item.SetValue(model, val);
                                            //Console.WriteLine($"{_name}:{val}");
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    #endregion

                    break;
                //big5
                case url.FubonBankUrl_1:
                case url.FubonBankUrl_2:
                    #region big5
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(bs, 0, bs.Length);
                    }
                    using (WebResponse Serviceres = request.GetResponse())
                    {
                        str = string.Empty;
                        var read = new StreamReader(Serviceres.GetResponseStream()).ReadToEnd();
                        WriteLog("無轉碼:" + read);
                        //Console.WriteLine("無轉碼:" + read);
                        //str = HttpUtility.UrlDecode(request.RequestUri.Query, Encoding.GetEncoding("big5"));
                        str = HttpUtility.UrlDecode(read, Encoding.GetEncoding("big5"));
                        WriteLog("big5:" + str);
                        //Console.WriteLine("big5:" +str);
                        foreach (var item in model.GetType().GetProperties())
                        {
                            var _name = item.Name;
                            var val = "錯誤的參數!";
                            if (!string.IsNullOrWhiteSpace(str) && str.IndexOf($"<{_name}>") > -1)
                            {
                                var start = str.IndexOf($"<{_name}>") + $"<{_name}>".Length;
                                val = str.Substring(start, (str.IndexOf($"</{_name}>") - start));
                                item.SetValue(model, val);
                                WriteLog($"{_name}:{val}");
                                //Console.WriteLine($"{_name}:{val}");
                            }
                        }
                    }
                    #endregion

                    break;
            }
          
            return model;
        }



        /// <summary>
        /// 組合 Http Request 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal HttpWebRequest CreateSOAPWebRequest(url type)
        {
            var _url = Properties.Settings.Default[type.ToString().Split('_')[0]]?.ToString();
            //Making Web Request  
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(_url);
            //SOAPAction  
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/Addition");
            //Content_type  
            if (type == url.FubonBankUrl_1 || type == url.FubonBankUrl_2)
            {
                Req.ContentType = "application/x-www-form-urlencoded;charset=\"big5\"";
            }
            else
            {
                Req.ContentType = "text/xml;charset=\"utf-8\"";
                Req.Accept = "text/xml";
            }       
            //HTTP method  
            Req.Method = "POST";
            //return HttpWebRequest  
            return Req;
        }

        /// <summary>
        /// 呼叫 MAC 
        /// </summary>
        /// <param name="Data">Data參數</param>
        /// <returns></returns>
        public MACResultModel CallMac(string Data)
        {
            //Console.WriteLine("===程式開始===");
            //Console.WriteLine(string.Empty);
            MACResultModel result = new MACResultModel();
            DateTime dt = DateTime.MinValue;
            TimeSpan ts = TimeSpan.MinValue;
            dt = DateTime.Now.Date;
            ts = DateTime.Now.TimeOfDay;
            try
            {
                var _IV = Properties.Settings.Default["IV"]?.ToString();
                if (!string.IsNullOrWhiteSpace(_IV))
                    IV = _IV;
                var _WorkingKeyLabel = Properties.Settings.Default["WorkingKeyLabel"]?.ToString();
                if (!string.IsNullOrWhiteSpace(_WorkingKeyLabel))
                    WorkingKeyLabel = _WorkingKeyLabel;
                result.Data = Data;
                result.IV = IV;
                result.WorkingKeyLabel = WorkingKeyLabel;
                if (string.IsNullOrWhiteSpace(Data))
                {
                    var ErrorMsg = $"無輸入Data參數";
                    result.ErrorMsg = ErrorMsg;
                    //Console.WriteLine(ErrorMsg);
                }
                else
                {
                    result = (MACResultModel)(new HSM().InvokeService(url.HSMUrl_MAC, result));
                    using (db_HSM_GL_Entities db = new db_HSM_GL_Entities())
                    {
                        var FM = new FRT_MAC()
                        {
                            CreateDate = dt,
                            CreateTime = ts,
                            WorkingKeyLabel = result.WorkingKeyLabel,
                            Data = result.Data,
                            IV = result.IV,
                            ErrorCode = result.ErrorCode,
                            ErrorMessage = result.ErrorMsg,
                            MAC = result.MAC
                        };
                        db.FRT_MAC.Add(FM);
                        try
                        {
                            //要存資料庫請拿掉下方註解
                            db.SaveChanges();
                            var _FM = db.FRT_MAC.AsNoTracking().FirstOrDefault(x =>
                                x.CreateDate == dt &&
                                x.CreateTime == ts);
                            //Console.WriteLine($"新增資料 : {_FM.modelToString()}");
                        }
                        catch (DbUpdateException ex)
                        {
                            var _msg = $"新增資料錯誤 : InnerException:{ex.InnerException},Message:{ex.Message}";
                            result.ErrorMsg = _msg;
                            //Console.WriteLine(_msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _msg = $"錯誤訊息 : InnerException:{ex.InnerException},Message:{ex.Message}";
                result.ErrorMsg = _msg;
                //Console.WriteLine(_msg);
            }
            finally
            {
                //Console.WriteLine("===程式結束===");
                //Console.ReadLine();
            }
            return result;
        }

        #region Other
        public interface IResultModel
        {

        }

        /// <summary>
        /// ChangeWorkingKey 回傳參數
        /// </summary>
        internal class ChangeWorkingKeyResultModel : IResultModel
        {
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string ECDKey { get; set; }
            public string Random { get; set; }
            public string Random1 { get; set; }
            public string WorkingKeyLabel { get; set; }
        }

        internal class HSMResultModel : IResultModel
        {
            public string RANDOM2 { get; set; }

            public string FubonBankXml { get; set; }
        }

        public class MACResultModel : IResultModel
        {
            public string WorkingKeyLabel { get; set; }
            public string Data { get; set; }
            public string IV { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string MAC { get; set; }
        }

        /// <summary>
        /// 發送url類型
        /// </summary>
        protected internal enum url
        {
            [Description("ChangeWorkingKey")]
            HSMUrl_ChangeWorkingKey,

            [Description("連接富邦銀行第一次傳送")]
            FubonBankUrl_1,

            [Description("連接富邦銀行第二次傳送")]
            FubonBankUrl_2,

            [Description("MAC")]
            HSMUrl_MAC,
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected static void WriteLog(string log, Ref.Nlog type = Ref.Nlog.Info)
        {
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

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="successFlag"></param>
        /// <returns></returns>
        protected static int sendMail( bool successFlag , string groupCode = "TRANSFERWORKINGKEY")
        {
            int result = 0;
            DateTime dn = DateTime.Now;
            string dtn = $@"{(dn.Year - 1911)}/{dn.ToString("MM/dd")}";
            var emps = new List<V_EMPLY2>();
            var depts = new List<VW_OA_DEPT>();
            var sub = $@"{dtn} 快速付款換KEY通知";
            var strFlag = successFlag ? "已完成" : "未完成";
            var body = $@"{dtn} 換 KEY {strFlag} ,特此通知.";
            var msg = string.Empty;
            Dictionary<string, Tuple<string, string>> mailToList = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> ccToList = new Dictionary<string, Tuple<string, string>>();
            //mailToList.Add("SYS", new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"));
            bool Falg = false;
            try
            {
                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    emps = db.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null && x.EMP_NO != null && x.DPT_CD != null).ToList();
                    depts = db.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }
                using (db_HSM_GL_Entities db = new db_HSM_GL_Entities())
                {
                    List<FRT_MAIL_NOTIFY> _FMNs = db.FRT_MAIL_NOTIFY.AsNoTracking()
                         .Where(x => x.GROUP_CODE == groupCode).ToList();
                    _FMNs.ForEach(x =>
                    {
                        List<string> USRIDs = new List<string>();
                        if (x.EMP_TYPE == "U")
                        {
                            USRIDs.Add(x.RECEIVER_EMPNO?.Trim());
                        }
                        else if (x.EMP_TYPE == "R")
                        {
                            USRIDs.AddRange(db.CODE_ROLE.Where(y => y.ROLE_ID != null && y.ROLE_ID == x.RECEIVER_EMPNO && y.IS_DISABLED == "N")
                            .Join(db.CODE_USER_ROLE.AsNoTracking().Where(z => z.ROLE_ID != null && z.USER_ID != null),
                             i => i.ROLE_ID.Trim(),
                             j => j.ROLE_ID.Trim(),
                             (i, j) => j)
                             .Join(db.CODE_USER.AsNoTracking().Where(z => z.IS_DISABLED == "N" && z.USER_ID != null),
                             i => i.USER_ID.Trim(),
                             j => j.USER_ID.Trim(),
                             (i, j) => j).Where(y => y.USER_ID != null).Select(y => y.USER_ID.Trim()));
                        }
                        if (USRIDs.Any())
                        {
                            USRIDs = USRIDs.Distinct().ToList();
                            var mailDatas = getSendMail(USRIDs, x.IS_NOTIFY_MGR, x.IS_NOTIFY_DEPT_MGR, emps, depts);
                            foreach (var mail in mailDatas.Item1)
                            {
                                if (!mailToList.ContainsKey(mail.Key))
                                {
                                    mailToList.Add(mail.Key, mail.Value);
                                }
                            }
                            foreach (var cc in mailDatas.Item2)
                            {
                                //if (!ccToList.ContainsKey(cc.Key))
                                //{
                                //    ccToList.Add(cc.Key,cc.Value);
                                //}
                                if (!mailToList.ContainsKey(cc.Key))
                                {
                                    mailToList.Add(cc.Key, cc.Value);
                                }
                            }
                        }
                        else
                        {

                        }
                    });
                }
                var self = Properties.Settings.Default["sendMailTest"]?.ToString();
                if (self == "Y")
                    mailToList.Add("換key排程", new Tuple<string, string>(Properties.Settings.Default["mailAccount"]?.ToString(), "TransferWorkingKey"));
                result = mailToList.Values.Count;
                var sms = new SendMail.SendMailSelf();
                sms.smtpPort = 25;
                sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                msg = sms.Mail_Send(
                    new Tuple<string, string>(sms.mailAccount, "TransferWorkingKey"),
                    mailToList.Values.AsEnumerable(),
                    ccToList.Any() ? ccToList.Values.AsEnumerable() : null,
                    sub,
                    body
                    );
                if (string.IsNullOrWhiteSpace(msg))
                {
                    Falg = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog($"寄信: InnerException:{ex.InnerException},Message:{ex.Message}", Ref.Nlog.Error);
                Falg = false;
                msg = $"InnerException:{ex.InnerException},Message:{ex.Message}";
            }
            finally
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (db_HSM_GL_Entities db = new db_HSM_GL_Entities())
                        {
                            foreach (var mail in mailToList)
                            {
                                db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                                {
                                    SEQ = getMailSeq(db),
                                    MAIL_DATE = dn,
                                    MAIL_TIME = dn.TimeOfDay,
                                    RECEIVER_EMPNO = mail.Key,
                                    EMAIL = mail.Value.Item1,
                                    MAIL_RESULT = Falg ? "S" : "F",
                                    RESULT_DESC = msg.Length >= 250 ? msg.Substring(0, 250) : msg,
                                    MAIL_SUB = sub
                                });
                            }
                            foreach (var cc in ccToList)
                            {
                                db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                                {
                                    SEQ = getMailSeq(db),
                                    MAIL_DATE = dn,
                                    MAIL_TIME = dn.TimeOfDay,
                                    RECEIVER_EMPNO = cc.Key,
                                    EMAIL = cc.Value.Item1,
                                    MAIL_RESULT = Falg ? "S" : "F",
                                    RESULT_DESC = msg.Length >= 250 ? msg.Substring(0, 250) : msg,
                                    MAIL_SUB = sub
                                });
                            }
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                WriteLog(validateMessage, Ref.Nlog.Error);
                            }
                            db.SaveChanges();
                        }
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString(), Ref.Nlog.Error);
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢收信人
        /// </summary>
        /// <param name="USRIDs">收信人</param>
        /// <param name="IS_NOTIFY_MGR">IS_NOTIFY_MGR = Y時，除TABLE本身的人員要寄送，還要寄給科主管</param>
        /// <param name="IS_NOTIFY_DEPT_MGR">IS_NOTIFY_DEPT_MGR = Y時，除TABLE本身的人員要寄送，還要寄給部主管</param>
        /// <returns></returns>
        protected static Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>> getSendMail(
            List<string> USRIDs,
            string IS_NOTIFY_MGR,
            string IS_NOTIFY_DEPT_MGR,
            List<V_EMPLY2> emps,
            List<VW_OA_DEPT> depts)
        {       
            var sendEmps = emps.Where(x => USRIDs.Contains(x.USR_ID.Trim())).ToList();
            Dictionary<string, Tuple<string, string>> mailTos = new Dictionary<string, Tuple<string, string>>(); 
            sendEmps.ForEach(x =>
            {
                mailTos.Add(x.USR_ID?.Trim(), new Tuple<string, string>(x.EMAIL?.Trim(), x.EMP_NAME?.Trim()));
            });
            Dictionary<string,Tuple<string, string>> ccs = new Dictionary<string, Tuple<string, string>>();
            if (IS_NOTIFY_MGR == "Y" || IS_NOTIFY_DEPT_MGR == "Y")
            {
                List<string> ccUSRIDs = new List<string>();
                var _sendEmps = sendEmps;
                if (IS_NOTIFY_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給科主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }                              
                    });
                }
                if (IS_NOTIFY_DEPT_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給部主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "03") //部
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            var dept2 = depts.FirstOrDefault(y => y.DPT_CD.Trim() == dept.UP_DPT_CD.Trim());
                            var emp2 = emps.FirstOrDefault(y => y.EMP_NO.Trim() == dept2?.DPT_HEAD.Trim());
                            if(emp2 != null)
                                ccUSRIDs.Add(emp2.EMP_NO.Trim());                               
                        }
                    });
                }
                if (ccUSRIDs.Any())
                {
                    ccUSRIDs = ccUSRIDs.Distinct().ToList();

                    foreach (var item in emps.Where(x => ccUSRIDs.Contains(x.EMP_NO.Trim())))
                    {
                        if (!ccs.ContainsKey(item.USR_ID?.Trim()))
                        {
                            ccs.Add(item.USR_ID?.Trim(), new Tuple<string, string>(item.EMAIL?.Trim(), item.EMP_NAME?.Trim()));
                        }
                    }
                }       
            }
            return new Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>>(mailTos, ccs);
        }

        protected static long getMailSeq(db_HSM_GL_Entities db)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var seq = db.SYS_SEQ.FirstOrDefault(x => x.SYS_CD == "RT" && x.SEQ_TYPE == "mail" && x.PRECODE == date);
            string seqNo = string.Empty;
            if (seq == null)
            {
                seqNo = $@"{date}{"1".PadLeft(5, '0')}";
                db.SYS_SEQ.Add(new SYS_SEQ()
                {
                    SYS_CD = "RT",
                    SEQ_TYPE = "mail",
                    PRECODE = date,
                    CURR_VALUE = 2
                });
                db.SaveChanges();
            }
            else
            {
                seqNo = $@"{date}{seq.CURR_VALUE.ToString().PadLeft(5,'0')}";
                seq.CURR_VALUE = seq.CURR_VALUE + 1;
            }
            return Convert.ToInt64(seqNo);
        }

        #endregion
    }

    internal static class Extension
    {
        public static string modelToString<T>(this T model, string log = null)
        {
            var result = string.Empty;
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(log))
                    result += "|";
                StringBuilder sb = new StringBuilder();
                var Type = model.GetType();
                sb.Append($@"TableName:{Type.Name}|");
                var Pros = Type.GetProperties();
                Pros.ToList().ForEach(x =>
                {
                    sb.Append($@"{x.Name}:{x.GetValue(model)?.ToString()}|");
                });
                if (sb.Length > 0)
                {
                    result = sb.ToString();
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }
        public static string getValidateString(this IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
            {
                result = string.Join(" ", errors.Select(y => string.Join(",", y.ValidationErrors.Select(z => z.ErrorMessage))));
            }
            return result;
        }
    }


}
