using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Xml;
using FRQxml.Model;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

//20181228 modify by daiyu 
//當<HRETRN> 值為’C’時
//請將Response的<TxHead> + Request的<TxBody> 組起來後再送一次ESB
//直到<HRETRN> 值為’E’時結束

namespace FRQxml
{
    class Program
    {
        static string strLog = "";
        static int qryDay = 1;
        static List<FRT_XML_R_622685> call522657List = new List<FRT_XML_R_622685>();

        static void Main(string[] args)
        {
            //creating object of program class to access methods  
            Program obj = new Program();
            addLog("=====   begin   =====");
            //tt();
            //Console.WriteLine("===程式開始===");
            //Console.WriteLine(string.Empty);
            DateTime dt = DateTime.Now;

            //先檢查
            //1.是否已經有同樣的JOB在執行中，若已有同樣JOB，不可重複執行
            //2.是否在設定可送電文的時間區間
            if (!chkDupJob() & chkJobTime())
            {
                try
                {
                    //proc522657("20200513", "Q070300258", "0000029");
                    //proc522657("20200513", "Q070300304", "0000030");
                    DateTime today = DateTime.Now;
                    for (var i = 0; i <= qryDay; i++)
                    {
                        var _procDate = today.AddDays(i * -1);
                        string procDate = _procDate.ToString("yyyyMMdd");

                        call522657List.Clear();
                        //proc622685(procDate, (i == 0 ? "1" : "2"), _procDate);

                        #region 呼叫規則(整理前)
                        //if (i == 0) //當 帳務日為當天 發查一筆 FunCode = 1 (匯出異常) & FunCode = 2 (匯出被退匯)
                        //{
                        //    proc622685(procDate, "1" , _procDate);
                        //    proc622685(procDate, "2" , _procDate);
                        //}
                        //else //當 帳務日不為當天 發查一筆  FunCode = 2 (匯出被退匯)
                        //    proc622685(procDate, "2", _procDate);
                        #endregion

                        #region 呼叫規則(整理後)
                        if (i == 0) //當 帳務日為當天 多發查一筆 FunCode = 1 (匯出異常)                      
                            proc622685(procDate, "1", _procDate);
                        //發查一筆  FunCode = 2 (匯出被退匯)
                        proc622685(procDate, "2", _procDate);
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"InnerException:{ex.InnerException},Message:{ex.Message}");
                    addLog($"InnerException:{ex.InnerException},Message:{ex.Message}");
                }
            }
            
            addLog("=====   end   =====");
            writeFile(strLog);
            Console.WriteLine(string.Empty);
            Console.WriteLine("===程式結束===");
            //Console.ReadLine();
        }

        /// <summary>
        /// 檢查是否已經有同樣的JOB在執行中，若已有同樣JOB，不可重複執行
        /// </summary>
        /// <returns></returns>
        private static bool chkJobTime()
        {
            try
            {
                using (dbGLEntities db = new dbGLEntities())
                {
                    var result = db.SYS_PARA.Where(x => x.SYS_CD == "RT" & x.GRP_ID == "FRQxml").ToList();
                    string nowTime = DateTime.Now.ToString("HHmm");

                    string strTime = result.Where(x => x.PARA_ID == "str_time").Select(x => x.PARA_VALUE).FirstOrDefault().ToString();
                    string endTime = result.Where(x => x.PARA_ID == "end_time").Select(x => x.PARA_VALUE).FirstOrDefault().ToString();
                    string qryTime = result.Where(x => x.PARA_ID == "qry_time").Select(x => x.PARA_VALUE).FirstOrDefault().ToString();
                    qryDay = Convert.ToInt32(result.Where(x => x.PARA_ID == "qry_day").Select(x => x.PARA_VALUE).FirstOrDefault());

                    //現在的時間需介於"每日查詢啟動時間"、"每日查詢結束時間"
                    if (!(nowTime.CompareTo(strTime) >= 0 && nowTime.CompareTo(endTime) <= 0))
                    {
                        addLog("現在的時間需介於 每日查詢啟動時間 、 每日查詢結束時間");
                        return false;
                    }

                    //距離上次JOB的執行時間已超過"查詢頻率(min)"
                    DateTime lastUpdateTime = Convert.ToDateTime(db.FRT_XML_T_622685.Where(x => x.EXEC_TYPE == "S").Max(x => x.CRT_TIME));
                    if (lastUpdateTime != null)
                    {
                        if (DateTime.Now.AddMinutes(-1 * Convert.ToInt64(qryTime)).CompareTo(lastUpdateTime) <= 0)
                        {
                            addLog("距離上次JOB時間未達:" + qryTime + "分鐘!!");
                            addLog("上次JOB執行時間:" + lastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                addLog(e.ToString());
                return false;
            }
            return true;
        }


        /// <summary>
        /// 是否已經有同樣的JOB在執行中
        /// </summary>
        /// <returns></returns>
        private static bool chkDupJob()
        {
            bool dupJob = false;
            int jobCnt = 0;

            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.ProcessName == "FRQxml")
                {
                    jobCnt++;
                }
            }
            if (jobCnt > 1)
            {
                dupJob = true;
                addLog("已有同樣的JOB在執行中，本次JOB作業不執行!!");
            }
            return dupJob;
        }


        /// <summary>
        /// 處理呼叫522657電文
        /// </summary>
        /// <param name="procDate"></param>
        private static void proc522657(string procDate, string fastNo, string irmtSrlNo)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            addLog("---proc522657 begin ---");
            addLog("procDate:" + procDate);

            //foreach (FRT_XML_R_622685 d in call522657List)
            //{
            ReqHeadModel reqHeadModel = getReqHeadPara("522657", "");
            ReqBody522657Model reqBodyModel = get522657ReqBodyPara(procDate, irmtSrlNo);

            string xmlHeaderReq = getReqHeadXml(reqHeadModel);
            string xmlBodyReq = get522657ReqBodyXml(reqBodyModel);
            try
            {
                //呼叫北富銀WEB API
                string xmlRes = callFBBankApi("522657", xmlHeaderReq, xmlBodyReq);
                try
                {
                    //處理回傳結果 紀錄至 FRT_XML_522657
                    proc522657Res(true, fastNo, reqBodyModel, xmlRes, xmlHeaderReq, xmlBodyReq);
                }
                catch (Exception ex)
                {
                    addLog($" call api err InnerException:{ex.InnerException},Message:{ex.Message}");
                }
            }
            catch (Exception e)
            {
                addLog($" call api err InnerException:{e.InnerException},Message:{e.Message}");

                //將本次的呼叫紀錄至 FRT_XML_522657
                proc522657Res(false, fastNo, reqBodyModel, null, xmlHeaderReq, xmlBodyReq);
            }
        }


        /// <summary>
        /// 處理呼叫622685電文
        /// </summary>
        /// <param name="procDate"></param>
        private static async void proc622685(string procDate, string FunCode, DateTime _procDate)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            addLog("---proc622685 begin ---");
            addLog("procDate:" + procDate);
            bool bContinue = true;
            string hretrn = "";

            string xmlHeaderReq = "";
            string xmlHeaderRes = "";
            while (bContinue)
            {
                ReqHeadModel reqHeadModel = getReqHeadPara("622685", "");
                ReqBody622685Model reqBodyModel = get622685ReqBodyPara(procDate, FunCode);

                //modify by daiyu 20181228
        //        當<HRETRN>值為’C’時
        //        請將Response的 < TxHead > +Request的 < TxBody > 組起來後再送一次ESB
        //直到<HRETRN> 值為’E’時結束

                if ("C".Equals(hretrn))
                    xmlHeaderReq = xmlHeaderRes;
                else
                    xmlHeaderReq = getReqHeadXml(reqHeadModel);
                string xmlBodyReq = get622685ReqBodyXml(reqBodyModel);
                try
                {
                    //呼叫北富銀WEB API
                    string xmlRes = callFBBankApi("622685", xmlHeaderReq, xmlBodyReq);
                    try
                    {
                        //將本次的呼叫紀錄至 FRT_XML_T_622685
                        bool bSuccess = writeT622685(true, xmlHeaderReq, reqBodyModel);

                        //處理回傳結果
                        Res622685 res622685 = await proc622685Res(xmlRes, xmlHeaderReq, xmlBodyReq, procDate, _procDate);
                        hretrn = res622685.hretrn;

                        if ("C".Equals(res622685.hretrn))
                        {
                            bContinue = true;
                            xmlHeaderRes = "<TxHead>" + res622685.xmlHeaderRes + "</TxHead>";
                        }
                        else {
                            bContinue = false;
                        }                
                    }
                    catch (Exception ex)
                    {
                        bContinue = false;
                        addLog($" call api err InnerException:{ex.InnerException},Message:{ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    bContinue = false;
                    addLog($" call api err InnerException:{e.InnerException},Message:{e.Message}");
                    //將本次的呼叫紀錄至 FRT_XML_T_622685
                    writeT622685(false, xmlHeaderReq, reqBodyModel);
                }
            }
            addLog("---proc622685 end ---");
        }

        /// <summary>
        /// 取得request的 head 參數值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="hretrn"></param>
        /// <returns></returns>
        private static ReqHeadModel getReqHeadPara(string type, string hretrn)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            ReqHeadModel reqHeadModel = new ReqHeadModel();
            reqHeadModel.htxtid = type == "622685" ? "FEP622685" : "NB522657";
            reqHeadModel.hwsid = "FBL_FPS";
            reqHeadModel.htlid = type == "622685" ? "7154151" : "0071571541519"; // 522657 中逸 20200330 說改成 00715 71541519
            reqHeadModel.hstano = qrySeqNo("RT", "FRQ", strToday).ToString().PadLeft(7, '0');
            reqHeadModel.hretrn = hretrn;
            reqHeadModel.func = type == "622685" ? string.Empty : "1";
            addLog("ReqHeadModel-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqHeadModel))
            {
                addLog(desc.Name + ":" + desc.GetValue(reqHeadModel));
            }

            return reqHeadModel;
        }


        /// <summary>
        /// 取得request的 body 參數值
        /// </summary>
        /// <param name="ActDate">帳務日</param>
        /// <param name="UUID">交易序號</param>
        /// <param name="FunCode">選項</param>
        /// <returns></returns>
        private static ReqBody622685Model get622685ReqBodyPara(string ActDate, string FunCode)
        {
            ReqBody622685Model reqBodyModel = new ReqBody622685Model();
            reqBodyModel.ActDate = ActDate;
            reqBodyModel.FunCode = FunCode;
            addLog("ReqBody622685Model-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqBodyModel))
            {
                addLog(desc.Name + ":" + desc.GetValue(reqBodyModel));
            }
            return reqBodyModel;
        }

        private static ReqBody522657Model get522657ReqBodyPara(string procDate, string irmtSrlno)
        {
            ReqBody522657Model reqBodyModel = new ReqBody522657Model();
            reqBodyModel.RmtDate = procDate;
            reqBodyModel.RmtSRLNo = irmtSrlno;
            reqBodyModel.BrhCode = "0715"; //522657 中逸 20200330 說改成 BrhCode(4位) 右靠左補0
            addLog("ReqBody522657Model-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqBodyModel))
            {
                addLog(desc.Name + ":" + desc.GetValue(reqBodyModel));
            }
            return reqBodyModel;
        }

        /// <summary>
        /// 將本次的呼叫紀錄至 FRT_XML_T_622685
        /// </summary>
        /// <param name="bSucess"></param>
        /// <param name="xmlHeader"></param>
        /// <param name="reqBodyModel"></param>
        private static bool writeT622685(bool bSucess, string xmlHeader, ReqBody622685Model reqBodyModel)
        {
            addLog("--- writeT622685 begin ---");
            try
            {
                using (dbGLEntities db = new dbGLEntities())
                {
                    db.FRT_XML_T_622685_NEW.Add(new FRT_XML_T_622685_NEW()
                    {
                        HEADER = xmlHeader,
                        ActDate = reqBodyModel.ActDate,
                        UUID = reqBodyModel.UUID,
                        FunCode = reqBodyModel.FunCode,
                        SQL_STS = bSucess == true ? "0" : "",
                        EXEC_TYPE = "S",
                        CRT_TIME = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                addLog(e.ToString());
                return false;
            }
            finally
            {
                addLog("--- writeT622685 end ---");
            }
        }

        /// <summary>
        /// 組合request xml的head部分
        /// </summary>
        /// <param name="reqHeadModel"></param>
        /// <returns></returns>
        private static string getReqHeadXml(ReqHeadModel reqHeadModel)
        {
            string xml = "";
            if (reqHeadModel.htxtid.Equals("NB522657")) //522657 20200330 中逸 新增 HFUNC = 1 (固定)
                xml = $@"<TxHead><HTXTID>{reqHeadModel.htxtid}</HTXTID><HWSID>{reqHeadModel.hwsid}</HWSID><HTLID>{reqHeadModel.htlid}</HTLID><HSTANO>{reqHeadModel.hstano}</HSTANO><HRETRN>{reqHeadModel.hretrn}</HRETRN><HFUNC>{reqHeadModel.func}</HFUNC></TxHead>";
            else
               xml = $@"<TxHead><HTXTID>{reqHeadModel.htxtid}</HTXTID><HWSID>{reqHeadModel.hwsid}</HWSID><HTLID>{reqHeadModel.htlid}</HTLID><HSTANO>{reqHeadModel.hstano}</HSTANO><HRETRN>{reqHeadModel.hretrn}</HRETRN></TxHead>";
            return xml;
        }

        /// <summary>
        /// 組合request xml的body部分
        /// </summary>
        /// <param name="reqBodyModel"></param>
        /// <returns></returns>
        private static string get622685ReqBodyXml(ReqBody622685Model reqBodyModel)
        {
            string xml = "";
            xml = $@"<TxBody><ActDate>{reqBodyModel.ActDate}</ActDate><UUID>{reqBodyModel.UUID}</UUID><FunCode>{reqBodyModel.FunCode}</FunCode></TxBody>";
            return xml;
        }

        private static string get522657ReqBodyXml(ReqBody522657Model reqBodyModel)
        {
            string xml = "";
            xml = $@"<TxBody><RmtDate>{reqBodyModel.RmtDate}</RmtDate><RmtSRLNo>{reqBodyModel.RmtSRLNo}</RmtSRLNo><BrhCode>{reqBodyModel.BrhCode}</BrhCode></TxBody>";
            return xml;
        }


        /// <summary>
        /// 呼叫北富銀電文
        /// </summary>
        /// <param name="xmlHeaderReq"></param>
        /// <param name="xmlBodyReq"></param>
        /// <returns></returns>
        private static string callFBBankApi(string type, string xmlHeaderReq, string xmlBodyReq)
        {

            var url = Properties.Settings.Default["FubonBankUrl"]?.ToString();
            addLog("callFBBankApi url-->" + url);

            var SPName = Properties.Settings.Default["SPName"]?.ToString();
            var LoginID = Properties.Settings.Default["LoginID"]?.ToString();
            string TxnId = type == "622685" ? "FEP622685" : "NB522657";

            //Making Web Request  
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
            //SOAPAction  
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/Addition");
            //Content_type  
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method  
            Req.Method = "POST";

            XmlDocument SOAPReqBody = new XmlDocument();

            string xmlFMPConnectionString = $@"<FMPConnectionString><SPName>{SPName}</SPName><LoginID>{LoginID}</LoginID><TxnId>{TxnId}</TxnId></FMPConnectionString>";

            string xmlReq = $@"<Tx>{xmlFMPConnectionString}{xmlHeaderReq}{xmlBodyReq}</Tx>";

            addLog("send request-->" + xmlReq);

            SOAPReqBody.LoadXml(xmlReq);

            using (Stream stream = Req.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }

            using (WebResponse Serviceres = Req.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    var ServiceResult = rd.ReadToEnd();
                    addLog("response:" + ServiceResult);
                    return ServiceResult;
                }
            }
        }


        protected class Res622685
        {
            public Res622685()
            {
                xmlHeaderRes = "";
                hretrn = "";
            }
            public string xmlHeaderRes { get; set; }
            public string hretrn { get; set; }
        }

        /// <summary>
        /// 回寫API呼叫結果 FRT_XML_R_622685
        /// </summary>
        /// <param name="strRes"></param>
        /// <param name="xmlHeaderReq"></param>
        /// <param name="xmlBodyReq"></param>
        private static async Task<Res622685> proc622685Res(string strRes, string xmlHeaderReq, string xmlBodyReq, string procDate, DateTime _procDate)
        {
            addLog("-----proc622685Res begin-----");
            Res622685 res622685 = new Res622685();

            string strHRETRN = "";

            XmlDocument xmlRes = new XmlDocument();
            xmlRes.LoadXml(strRes);

            XmlNode headNode = xmlRes.SelectSingleNode("/Tx/TxHead");
            string strHead = headNode.InnerXml;
            addLog("strHead:" + strHead);
            res622685.xmlHeaderRes = strHead;

            if (headNode != null)
            {
                using (dbGLEntities db = new dbGLEntities())
                {
                    strHRETRN = headNode["HRETRN"].InnerText.Trim();
                    addLog("strHRETRN:" + strHRETRN);
                    string strHERRID = headNode["HERRID"].InnerText.Trim();
                    addLog("strHERRID:" + strHERRID);
                    if ("0000".Equals(strHERRID))
                    {
                        XmlNodeList dList = xmlRes.SelectNodes("/Tx/TxBody/TxRepeat");

                        foreach (XmlNode xn in dList)
                        {
                            if (xn != null)
                            {
                                string setlId = xn["SETL_ID"]?.InnerText;
                                string rmtType = xn["RMT_TYPE"]?.InnerText;
                                //update 622685New  20190730 by Mark
                                FRT_XML_R_622685_NEW rec_New = new FRT_XML_R_622685_NEW();
                                //rec.HEADER = strHead;
                                rec_New.HEADER = strHead;
                                //一去一回，每回最多放30筆
                                //rec.ETARRY = "";
                                //一去一回，每回最多放25筆
                                rec_New.BufferData = "";
                                //清算序號
                                rec_New.HeadUUID = xn["UUID"]?.InnerText?.Trim(); //UUID 判斷快速付款編號使用
                                //類別：1:匯出異常(匯出匯款) 2:匯出被退匯(匯入匯款)
                                rec_New.FunCode = xn["FunCode"]?.InnerText?.Trim(); //選項
                                rec_New.Status = xn["Status"]?.InnerText?.Trim(); //匯款狀況
                                //  FunCode = 0 (同步匯出匯款狀況)
                                //    Status = 0(處理中) pending狀態(後續狀態會變成 1 or 4 or 6)
                                //    Status = 1(匯出異常) 後續 => 等待 FunCode 1 查詢
                                //    Status = 2(人工退還客戶) 人壽不會發生)  (已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態)
                                //    Status = 4(已匯出) 後續 => End
                                //    Status = 8(請OP處理) 人壽不會發生(已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態)
                                //    Status = 6(無資料) 後續 => 人工查詢該筆資料狀態
                                //  
                                //  FunCode = 1(匯出異常)
                                //    Status = 1(匯出異常: 將轉隔日匯出) 後續 => 等待銀行下次匯出
                                //    Status = 2(人工退還客戶: 將款項歸還客戶)(已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態)
                                //    Status = 8(請OP處理) pending狀態(已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態)
                                //  
                                //  FunCode = 2(匯出被退匯)
                                //    Status = 3(匯出失敗: 人工退還客戶) 後續 => 查詢代碼判斷歸屬
                                //    Status = 5(匯出成功: 人工重匯) 後續 => End
                                //    Status = 9(被退匯未處理) pending狀態(後續狀態會變成 3 or 5)
                                //  (PS: 3 & 5 不會互相覆蓋，故同一筆快速付款有可能出現兩種狀態，這時候就須使用
                                //  ActDate(帳務日) & ucaIRMTSrlNo(匯出被退匯匯入序號) 來判斷最終狀態為何筆)

                                rec_New.ucaErrorCod = xn["FiscCode"]?.InnerText?.Trim(); //檢核錯誤代碼/金資異常代碼 
                                rec_New.ucaIRMTSrlNo = xn["IRMTSrlNo"]?.InnerText?.Trim(); //匯出被退匯匯入序號  FunCode = 2時才有                               
                                rec_New.ucaOrgRmtSrlNo = xn["OrgRmtSrlNo"]?.InnerText?.Trim(); //原匯出序號 
                                rec_New.NewRmtSrlNo = xn["NewRmtSrlNo"]?.InnerText?.Trim();  //新匯出序號                                
                                rec_New.TxnTime = xn["TxnTime"]?.InnerText?.Trim(); //匯出時間                               
                                rec_New.TelSeq = xn["TelSeq"]?.InnerText?.Trim(); //電文序號                               
                                rec_New.NextKey = xn["NextKey"]?.InnerText?.Trim(); //NextKey                                                                                 
                                //UUID組法 => L03 + ‘XM96.SETL_ID’(7154151ILI(固定值10碼)  + ‘快速付款編號(10碼)’ ex : 7154151ILIQ070100038) + ‘XM96.UUID_FLAG’(01~99) + 9000400(20200324 中逸回 9000400)  ex : L037154151ILIQ070100038019000400
                                rec_New.FAST_NO = (rec_New.HeadUUID != null && rec_New.HeadUUID.Length == 32) ? rec_New.HeadUUID?.Substring(13, 10) : "";
                                if (!string.IsNullOrWhiteSpace(rec_New.HeadUUID))
                                    addLog($@"Get UUID : {rec_New.HeadUUID} , FAST_NO : {rec_New.FAST_NO}, IRMTSrlNo : {rec_New.ucaIRMTSrlNo}");
                                rec_New.ActDate = _procDate.Date;
                                rec_New.HERRID = strHERRID;
                                rec_New.CRT_TIME = DateTime.Now;

                                if ("".Equals(rec_New.FAST_NO))
                                    continue;

                                FRT_XML_R_622685_NEW db622685_NEW = db.FRT_XML_R_622685_NEW.Where(x => x.FAST_NO == rec_New.FAST_NO && x.FunCode == rec_New.FunCode).OrderByDescending(x=>x.CRT_TIME).FirstOrDefault();
                                if (db622685_NEW != null)
                                {
                                    if (rec_New.FunCode == "1")
                                    {
                                        addLog("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                        addLog("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                        addLog("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                        addLog("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                        if (db622685_NEW.FunCode.Equals(rec_New.FunCode) && db622685_NEW.Status.Equals(rec_New.Status) && db622685_NEW.ucaErrorCod.Equals(rec_New.ucaErrorCod))
                                        {
                                            addLog("已存在 FRT_XML_R_622685(不更新)");
                                        }
                                        else
                                        {
                                            db622685_NEW.HEADER = rec_New.HEADER;
                                            db622685_NEW.BufferData = rec_New.BufferData;
                                            db622685_NEW.FunCode = rec_New.FunCode;
                                            db622685_NEW.Status = rec_New.Status;
                                            db622685_NEW.ucaErrorCod = rec_New.ucaErrorCod;
                                            db622685_NEW.ucaIRMTSrlNo = rec_New.ucaIRMTSrlNo;
                                            db622685_NEW.ucaOrgRmtSrlNo = rec_New.ucaOrgRmtSrlNo;
                                            db622685_NEW.NewRmtSrlNo = rec_New.NewRmtSrlNo;
                                            db622685_NEW.TxnTime = rec_New.TxnTime;
                                            db622685_NEW.TelSeq = rec_New.TelSeq;
                                            db622685_NEW.NextKey = rec_New.NextKey;
                                            db622685_NEW.SQL_STS = rec_New.SQL_STS;
                                            db622685_NEW.ActDate = rec_New.ActDate;
                                            db622685_NEW.UPD_TIME = DateTime.Now;
                                            db.SaveChanges();
                                            await check622685(rec_New, procDate);
                                            addLog("已存在 FRT_XML_R_622685(更新)");
                                        }
                                    }
                                    else if (rec_New.FunCode == "2" && rec_New.Status == "9") //接收狀態為過度檔
                                    {
                                        if (db622685_NEW.FunCode == "2" && db622685_NEW.Status == "9" && ((rec_New.ActDate == db622685_NEW.ActDate) && (strToint(rec_New.ucaIRMTSrlNo) == strToint(db622685_NEW.ucaIRMTSrlNo))))
                                        {
                                            //上一次狀態還是在過度檔狀態,且為同一筆資料時,不更新
                                            addLog("已存在 FRT_XML_R_622685(過度狀態 不更新)");
                                        }
                                        //傳送日期是新的 or 傳送日期為當天 且符合 北富銀回傳的匯出序號 須新增一筆新的
                                        else if (rec_New.ActDate > db622685_NEW.ActDate || ((rec_New.ActDate == db622685_NEW.ActDate) && strToint(rec_New.ucaIRMTSrlNo) > strToint(db622685_NEW.ucaIRMTSrlNo)))
                                        {
                                            db.FRT_XML_R_622685_NEW.Add(rec_New);
                                            db.SaveChanges();
                                            addLog("已新增 FRT_XML_R_622685(過度狀態 新增)");
                                        }
                                    }
                                    //目前資料庫的資料為這筆資料的過度狀態 僅更新該筆過度狀態
                                    else if (db622685_NEW.FunCode == "2" && db622685_NEW.Status == "9" && ((rec_New.ActDate == db622685_NEW.ActDate) && (strToint(rec_New.ucaIRMTSrlNo) == strToint(db622685_NEW.ucaIRMTSrlNo))))
                                    {
                                        addLog("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                        addLog("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                        addLog("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                        addLog("FRT_XML_R_622685.ucaIRMTSrlNo:" + db622685_NEW.ucaIRMTSrlNo + "；FubonApi.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);
                                        db622685_NEW.HEADER = rec_New.HEADER;
                                        db622685_NEW.BufferData = rec_New.BufferData;
                                        db622685_NEW.FunCode = rec_New.FunCode;
                                        db622685_NEW.Status = rec_New.Status;
                                        db622685_NEW.ucaErrorCod = rec_New.ucaErrorCod;
                                        db622685_NEW.ucaIRMTSrlNo = rec_New.ucaIRMTSrlNo;
                                        db622685_NEW.ucaOrgRmtSrlNo = rec_New.ucaOrgRmtSrlNo;
                                        db622685_NEW.NewRmtSrlNo = rec_New.NewRmtSrlNo;
                                        db622685_NEW.TxnTime = rec_New.TxnTime;
                                        db622685_NEW.TelSeq = rec_New.TelSeq;
                                        db622685_NEW.NextKey = rec_New.NextKey;
                                        db622685_NEW.SQL_STS = rec_New.SQL_STS;
                                        db622685_NEW.ActDate = rec_New.ActDate;
                                        db622685_NEW.UPD_TIME = DateTime.Now;
                                        db.SaveChanges();
                                        await check622685(rec_New, procDate);
                                        addLog("已存在 FRT_XML_R_622685(更新過度檔)");
                                    }
                                    //判斷回傳的資料與現有的資料的時間 (只更新最新的回傳資料) 條件 1.傳送日期是新的 or 2.傳送日期為當天 且符合 北富銀回傳的匯出序號
                                    else if (rec_New.FunCode == "2" && (rec_New.ActDate > db622685_NEW.ActDate || ((rec_New.ActDate == db622685_NEW.ActDate) && strToint(rec_New.ucaIRMTSrlNo) > strToint(db622685_NEW.ucaIRMTSrlNo))))
                                    {
                                        addLog("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                        addLog("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                        addLog("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                        addLog("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                        addLog("FRT_XML_R_622685.ucaIRMTSrlNo:" + db622685_NEW.ucaIRMTSrlNo + "；FubonApi.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);
                                        if (db622685_NEW.FunCode.Equals(rec_New.FunCode) && db622685_NEW.Status.Equals(rec_New.Status)
                                            && db622685_NEW.ucaErrorCod.Equals(rec_New.ucaErrorCod) && db622685_NEW.ucaIRMTSrlNo.Equals(rec_New.ucaIRMTSrlNo))
                                        {
                                            #region 不會有 FunCode = 0 , 排程只會有 1 & 2
                                            //第二次狀態為等待需通知財務部 20190730 by Mark
                                            //if (rec_New.FunCode == "0" && rec_New.Status == "0")
                                            //{
                                            //    await check622685(rec_New, procDate);
                                            //}
                                            #endregion
                                            addLog("已存在 FRT_XML_R_622685(不更新)");
                                        }
                                        else
                                        {
                                            //db622685_NEW.HEADER = rec_New.HEADER;
                                            //db622685_NEW.BufferData = rec_New.BufferData;
                                            //db622685_NEW.FunCode = rec_New.FunCode;
                                            //db622685_NEW.Status = rec_New.Status;
                                            //db622685_NEW.ucaErrorCod = rec_New.ucaErrorCod;
                                            //db622685_NEW.ucaIRMTSrlNo = rec_New.ucaIRMTSrlNo;
                                            //db622685_NEW.ucaOrgRmtSrlNo = rec_New.ucaOrgRmtSrlNo;
                                            //db622685_NEW.NewRmtSrlNo = rec_New.NewRmtSrlNo;
                                            //db622685_NEW.TxnTime = rec_New.TxnTime;
                                            //db622685_NEW.TelSeq = rec_New.TelSeq;
                                            //db622685_NEW.NextKey = rec_New.NextKey;
                                            //db622685_NEW.SQL_STS = rec_New.SQL_STS;
                                            //db622685_NEW.ActDate = rec_New.ActDate;
                                            //db622685_NEW.UPD_TIME = DateTime.Now;
                                            db.FRT_XML_R_622685_NEW.Add(rec_New);
                                            db.SaveChanges();
                                            addLog("已新增 FRT_XML_R_622685(新增)");
                                            await check622685(rec_New, procDate);
                                        }
                                    }                                   
                                }
                                else
                                {
                                    db.FRT_XML_R_622685_NEW.Add(rec_New);
                                    db.SaveChanges();
                                    addLog("已新增 FRT_XML_R_622685(新增)");
                                    await check622685(rec_New, procDate);
                                }
                            }
                        }
                    }
                    else
                    {
                        XmlNode bodyErrNode = xmlRes.SelectSingleNode("/Tx/TxBody");
                        string EMSGTXT = bodyErrNode["EMSGTXT"].InnerText.Trim();

                        addLog("622685 ERR--> EMSGTXT:" + EMSGTXT);
                    }
                }


            }
            addLog("-----proc622685Res end-----");

            res622685.hretrn = strHRETRN;   //modify by daiyu 20181228
            return res622685;
        }

        private static int strToint(string val)
        {
            int i = 0;
            if (string.IsNullOrWhiteSpace(val))
                return i;
            Int32.TryParse(val, out i);
            return i;
        }

        /// <summary>
        /// 檢核寄信 20190730 by Mark
        /// </summary>
        /// <param name="rec_New"></param>
        /// <param name="procDate"></param>
        /// <returns></returns>
        private static async Task check622685(FRT_XML_R_622685_NEW rec_New,string procDate)
        {
            //若錯誤代碼 = RM99的...後續要呼叫522657電文
            if ("99".Equals(rec_New.ucaErrorCod))
                proc522657(procDate, rec_New.FAST_NO, rec_New.ucaIRMTSrlNo);   //呼叫522657電文

            //系統收到622685的電文時，符合下列情境，需呼叫API來執行後續動作
            if ((rec_New.FunCode == "1" && rec_New.Status == "1") || //匯出異常(將轉隔日匯出)
                (rec_New.FunCode == "1" && rec_New.Status == "2") || //應該不會有此狀態
                (rec_New.FunCode == "2" && rec_New.Status == "3"))   //匯出失敗(人工退還客戶) 
            {
                await FastErrorApi(rec_New.Status, rec_New.FAST_NO, rec_New.ucaErrorCod, rec_New.FunCode);
            }
        }

        private static void proc522657Res(bool bSuccess, string fastNo, ReqBody522657Model reqBodyModel, string strRes, string xmlHeaderReq, string xmlBodyReq)
        {
            addLog("-----proc522657Res begin-----");

            try
            {
                XmlDocument xmlRes = new XmlDocument();
                xmlRes.LoadXml(strRes);

                XmlNode headNode = xmlRes.SelectSingleNode("/Tx/TxHead");

                FRT_XML_522657 rec = new FRT_XML_522657();
                rec.FAST_NO = fastNo;
                rec.T_HEADER = xmlHeaderReq;
                rec.IRMT_SRLNO = reqBodyModel.RmtSRLNo;
                rec.T_BRH_COD = reqBodyModel.BrhCode;

                using (dbGLEntities db = new dbGLEntities())
                {
                    if (bSuccess)   //有得到API回覆
                    {
                        if (headNode != null)
                        {
                            string strHead = headNode.InnerXml;
                            rec.R_HEADER = strHead;

                            string strHERRID = headNode["HERRID"].InnerText.Trim();


                            if ("0000".Equals(strHERRID))   //查詢結果為成功
                            {
                                XmlNode bodyNode = xmlRes.SelectSingleNode("/Tx/TxBody");

                                rec.R_IRMT_DATE = bodyNode["TxnDate"]?.InnerText?.Trim(); //匯款日期
                                rec.IRMT_TIME = bodyNode["TxnTime"]?.InnerText?.Trim(); //匯入時間
                                rec.RMT_AMT = bodyNode["TxnAmt"]?.InnerText?.Trim(); //匯款金額 
                                rec.RMT_TYPE = bodyNode["HostRmtTypeDesc"]?.InnerText?.Trim(); //匯款種類說明(中文)
                                rec.RMT_BNK = bodyNode["SendBankId"]?.InnerText?.Trim(); //匯款行
                                rec.RMT_BNK_NAME = bodyNode["SendBankName"]?.InnerText?.Trim(); //匯款行中文名稱
                                rec.RCV_BNK = bodyNode["RecvBankId"]?.InnerText?.Trim(); //解款行
                                rec.RCV_BNK_NAME = bodyNode["RecvBankFullName"]?.InnerText?.Trim(); //解款行中文全稱
                                rec.RCV_ACT_NO = bodyNode["ActNo"]?.InnerText?.Trim(); //收款人帳號
                                rec.RCV_CUST_NAME = bodyNode["AcctName1"]?.InnerText?.Trim(); //收款人戶名
                                rec.EC_RCV_ACT_NO = bodyNode["ECActNo"]?.InnerText?.Trim(); //新收款人帳號
                                rec.EC_CUST_NAME = bodyNode["CustomerName1"]?.InnerText?.Trim(); //新收款人戶名
                                rec.RCV_NAME = bodyNode["RecvCustName"]?.InnerText?.Trim(); //收款人姓名
                                rec.EC_RCV_NAME = bodyNode["ECActName"]?.InnerText?.Trim(); //新收款人姓名
                                rec.RMT_NAME = bodyNode["SendCustName"]?.InnerText?.Trim(); //匯款人姓名
                                rec.RMT_APX = bodyNode["ChiMemo"]?.InnerText?.Trim(); //附言/退匯附言

                                //下面為銀行內部參數 (人壽無需使用)
                                rec.IRMT_STS = bodyNode["PRSDesc"]?.InnerText?.Trim(); //處理結果
                                rec.PRS_RSLT = bodyNode["RmtCNTStsDesc"]?.InnerText?.Trim(); //匯入狀況說明(中文)
                                rec.PRS_DATE = bodyNode["PRSDateTime"]?.InnerText?.Trim(); //處理日期
                                rec.TLXSNO = bodyNode["TelSeq"]?.InnerText?.Trim(); //電文序號
                                rec.BTBSNO = bodyNode["RmtSeq"]?.InnerText?.Trim(); //通匯序號
                                rec.RETURN_DATE = bodyNode["DUMMY"]?.InnerText?.Trim(); //退匯日期(固定放空白)
                                rec.PRS_SRLNO = bodyNode["OrgRmtSRLNo"]?.InnerText?.Trim(); //重匯序號
                                rec.ORG_BTBSNO = bodyNode["OrgRmtSeq"]?.InnerText?.Trim(); //原通匯序號
                                rec.REJ_RSN_TXT = bodyNode["ReasonCodeDesc"]?.InnerText?.Trim(); //被退匯理由
                                rec.EMP_ID = bodyNode["EmpID"]?.InnerText?.Trim(); //櫃員編號
                                rec.PRS_SUP_ID = bodyNode["AuthEmpID"]?.InnerText?.Trim(); //解付主管
                                rec.R_BRH_COD = bodyNode["BrhCode"]?.InnerText?.Trim(); //交易行
                                rec.ORG_RMT_SRLNO = bodyNode["RmtSRLNo"]?.InnerText?.Trim(); //原匯出序號
                                rec.ORG_RMT_DATE = bodyNode["OrgRmtDate"]?.InnerText?.Trim(); //原匯出日期
                                rec.ERROR_COD = bodyNode["OrgPCode"]?.InnerText?.Trim(); //錯誤代碼
                            }
                            else
                            {
                                XmlNode bodyErrNode = xmlRes.SelectSingleNode("/Tx/TxBody");
                                string EMSGTXT = bodyErrNode["EMSGTXT"]?.InnerText?.Trim();
                                addLog("522657 ERR--> fastNo:" + fastNo + "；  EMSGTXT:" + EMSGTXT);
                            }
                        }
                    }

                    rec.CRT_TIME = DateTime.Now;

                    FRT_XML_522657 db522657 = db.FRT_XML_522657.Where(x => x.FAST_NO == rec.FAST_NO).FirstOrDefault();
                    if (db522657 != null)
                        db.FRT_XML_522657.Remove(db522657);
                        db.FRT_XML_522657.Add(rec);
                        db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                addLog("e:" + e.ToString());
            }

            addLog("-----proc522657Res end-----");
        }


        /// <summary>
        /// 取得序號
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="cType"></param>
        /// <param name="cPreCode"></param>
        /// <returns></returns>
        public static int qrySeqNo(string sysCd, string cType, string cPreCode)
        {
            int intseq = 0;
            int cnt = 0;
            using (dbGLEntities db = new dbGLEntities())
            {
                try
                {
                    if ("".Equals(cPreCode))
                    {
                        SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SYS_CD == sysCd & x.SEQ_TYPE == cType).FirstOrDefault<SYS_SEQ>();
                        sysDeq.SYS_CD = sysCd;
                        if (sysDeq == null)
                        {
                            sysDeq = new SYS_SEQ();
                            intseq = 1;
                            sysDeq.SYS_CD = sysCd;
                            sysDeq.SEQ_TYPE = cType;
                            sysDeq.PRECODE = "";
                            sysDeq.CURR_VALUE = intseq + 1;
                            db.SYS_SEQ.Add(sysDeq);
                            cnt = db.SaveChanges();
                        }
                        else
                        {
                            intseq = sysDeq.CURR_VALUE;
                            sysDeq.CURR_VALUE = intseq + 1;
                            cnt = db.SaveChanges();
                        }
                    }
                    else
                    {
                        SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SYS_CD == sysCd & x.SEQ_TYPE == cType & x.PRECODE == cPreCode).FirstOrDefault();
                        if (sysDeq == null)
                        {
                            sysDeq = new SYS_SEQ();
                            intseq = 1;
                            sysDeq.SYS_CD = sysCd;
                            sysDeq.SEQ_TYPE = cType;
                            sysDeq.PRECODE = cPreCode;
                            sysDeq.CURR_VALUE = intseq + 1;
                            db.SYS_SEQ.Add(sysDeq);
                            cnt = db.SaveChanges();
                        }
                        else
                        {
                            intseq = sysDeq.CURR_VALUE;
                            sysDeq.CURR_VALUE = intseq + 1;
                            cnt = db.SaveChanges();
                        }
                    }
                    return intseq;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private static async Task FastErrorApi(string status, string fastNo, string errorCode,string FunCode)
        {
            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 authorization header
                client.DefaultRequestHeaders.Add("authorization", "token {'6D9310E55EB72CA5D7BBC8F98DD517BC'}");

                // 準備寫入的 data
                ErrorModel postData = new ErrorModel() { ExecType = $@"{FunCode};{status}" , Fast_No = fastNo, ErrorCode = errorCode, ErrorMsg = "", TextType = "622685", EMSGTXT = "" };

                // 將 data 轉為 json
                string json = JsonConvert.SerializeObject(postData);

                // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                // 發出 post 並取得結果
                string url = Properties.Settings.Default.FastErrorApi;
                addLog("FastErrorApi url:" + url);

                HttpResponseMessage response = client.PostAsync(url, contentPost).Result;

                // 將回應結果內容取出
                string tet = response.ToString();
                var customerJsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Your response data is: " + customerJsonString);
            }
        }

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ErrorModel
        {
            public string ExecType { get; set; }
            /// <summary>
            /// 快速付款編號
            /// </summary>
            public string Fast_No { get; set; }
            /// <summary>
            /// 錯誤代碼
            /// </summary>
            public string ErrorCode { get; set; }
            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string ErrorMsg { get; set; }
            public string EMSGTXT { get; set; }
            public string TextType { get; set; }
        }

        /// <summary>
        /// Request Head 參數
        /// </summary>
        internal class ReqHeadModel
        {
            public string htxtid { get; set; }
            public string hwsid { get; set; }
            public string htlid { get; set; }
            public string hstano { get; set; }
            public string txmsrn { get; set; }
            public string hretrn { get; set; }
            public string func { get; set; }
        }

        /// <summary>
        /// 622685 Request Body 參數
        /// </summary>
        internal class ReqBody622685Model
        {
            public string ActDate { get; set; }
            public string UUID { get; set; }
            public string FunCode { get; set; }
        }

        /// <summary>
        /// 522657 Request Body 參數
        /// </summary>
        internal class ReqBody522657Model
        {
            public string RmtDate { get; set; }
            public string RmtSRLNo { get; set; }
            public string BrhCode { get; set; }
        }

        /// <summary>
        /// 寫LOG FILE
        /// </summary>
        /// <param name="message"></param>
        private static void writeFile(string message)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\LOG\\";
            addLog("path:" + path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string dataString = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string fileName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\LOG\\" + dataString + ".txt";
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(message);
            }
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddDays(-10))
                    fi.Delete();
            }
        }

        private static string addLog(string log)
        {
            strLog = strLog + log + "\r\n";
            return strLog;
        }
    }

}
