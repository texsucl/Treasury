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
                    DateTime today = DateTime.Now;
                    for (var i = 0; i <= qryDay; i++)
                    {
                        string procDate = today.AddDays(i * -1).ToString("yyyyMMdd");

                        call522657List.Clear();

                        //呼叫622685電文
                        proc622685(procDate);

                        ////呼叫522657電文
                        //if (call522657List.Count > 0)
                        //    proc522657(procDate);
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
                if (p.ProcessName.ToLower() == "FRQxml")
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
                    //將本次的呼叫紀錄至 FRT_XML_T_522657
                    //bool bSuccess = writeT522657(true, xmlHeaderReq, reqBodyModel);


                    //處理回傳結果
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

                //將本次的呼叫紀錄至 FRT_XML_T_522657
                proc522657Res(false, fastNo, reqBodyModel, null, xmlHeaderReq, xmlBodyReq);
            }
            //}

        }


        /// <summary>
        /// 處理呼叫622685電文
        /// </summary>
        /// <param name="procDate"></param>
        private static async void proc622685(string procDate)
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
                ReqBody622685Model reqBodyModel = get622685ReqBodyPara(procDate);

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

                        Res622685 res622685 = await proc622685Res(xmlRes, xmlHeaderReq, xmlBodyReq, procDate);
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
        /// <param name="procDate"></param>
        /// <param name="procTime"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static ReqHeadModel getReqHeadPara(string type, string hretrn)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            ReqHeadModel reqHeadModel = new ReqHeadModel();
            reqHeadModel.htxtid = type == "622685" ? "TP622685" : "FPS522657";
            reqHeadModel.hwsid = "FBL_FPS";
            reqHeadModel.htlid = "7154151";
            reqHeadModel.hstano = qrySeqNo("RT", "FRQ", strToday).ToString().PadLeft(7, '0');
            reqHeadModel.hretrn = hretrn;

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
        /// <param name="procDate"></param>
        /// <param name="procTime"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static ReqBody622685Model get622685ReqBodyPara(string procDate)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");

            ReqBody622685Model reqBodyModel = new ReqBody622685Model();
            reqBodyModel.actDate = procDate;
            reqBodyModel.brhType01 = "";
            reqBodyModel.itemNo = strToday == procDate ? "0" : "2";

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
            reqBodyModel.irmtDate = procDate;
            reqBodyModel.irmtSrlno = irmtSrlno;
            reqBodyModel.brhCod = "715";

            addLog("ReqBody522657Model-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqBodyModel))
            {
                addLog(desc.Name + ":" + desc.GetValue(reqBodyModel));
            }

            return reqBodyModel;
        }


        /// <summary>
        /// 將本次的呼叫紀錄至 FRT_XML_T_522657
        /// </summary>
        /// <param name="bSucess"></param>
        /// <param name="xmlHeader"></param>
        /// <param name="reqBodyModel"></param>
        //private static bool writeT522657(bool bSucess, string xmlHeader, ReqBody522657Model reqBodyModel)
        //{
        //    addLog("--- writeT522657 begin ---");

        //    try
        //    {
        //        using (dbGLEntities db = new dbGLEntities())
        //        {
        //            db.FRT_XML_T_522657.Add(new FRT_XML_T_522657()
        //            {
        //                HEADER = xmlHeader,
        //                IRMT_DATE = reqBodyModel.irmtDate,
        //                IRMT_SRLNO = reqBodyModel.irmtSrlno,
        //                BRH_COD = reqBodyModel.brhCod,
        //                SQL_STS = bSucess == true ? "0" : "",
        //                CRT_TIME = DateTime.Now
        //            });

        //            db.SaveChanges();
        //            return true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        addLog(e.ToString());
        //        return false;
        //    }
        //    finally
        //    {
        //        addLog("--- writeT522657 end ---");
        //    }
        //}




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
                var connString = ConfigurationManager.ConnectionStrings["dbFGL"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"

INSERT INTO [FRT_XML_T_622685]
 (
HEADER
,ACT_DATE
,BRH_TYPE_01
,ITEM_NO
,SQL_STS
,EXEC_TYPE
,CRT_TIME
)
     VALUES
(
 @HEADER
,@ACT_DATE
,@BRH_TYPE_01
,@ITEM_NO
,@SQL_STS
,@EXEC_TYPE
,@CRT_TIME
)
        ";
                    SqlCommand command = conn.CreateCommand();
                    command.Connection = conn;

                    command.CommandText = sql;

                    command.Parameters.AddWithValue("@HEADER", xmlHeader);
                    command.Parameters.AddWithValue("@ACT_DATE", reqBodyModel.actDate);
                    command.Parameters.AddWithValue("@BRH_TYPE_01", reqBodyModel.brhType01);
                    command.Parameters.AddWithValue("@ITEM_NO", reqBodyModel.itemNo);
                    command.Parameters.AddWithValue("@EXEC_TYPE", "S");
                    command.Parameters.AddWithValue("@SQL_STS", bSucess == true ? "0" : "");
                    command.Parameters.AddWithValue("@CRT_TIME", DateTime.Now);

                    command.ExecuteNonQuery();

                    return true;
                }

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


        private static void writeR622685(dbGLEntities db, FRT_XML_R_622685 rec)
        {
            addLog("--- writeR622685 begin ---");

            db.FRT_XML_R_622685.Add(rec);

            db.SaveChanges();


            addLog("--- writeR622685 end ---");
        }




        /// <summary>
        /// 組合request xml的head部分
        /// </summary>
        /// <param name="reqHeadModel"></param>
        /// <returns></returns>
        private static string getReqHeadXml(ReqHeadModel reqHeadModel)
        {
            string xml = "";
            xml = $@"<TxHead><HTXTID>{reqHeadModel.htxtid}</HTXTID><HWSID>{reqHeadModel.hwsid}</HWSID><HTLID>{reqHeadModel.htlid}</HTLID><HSTANO>{reqHeadModel.hstano}</HSTANO><HRETRN>{reqHeadModel.hretrn}</HRETRN> </TxHead>";

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
            xml = $@"<TxBody><ACT_DATE>{reqBodyModel.actDate}</ACT_DATE><BRH_TYPE_01>{reqBodyModel.brhType01}</BRH_TYPE_01><ITEM_NO>{reqBodyModel.itemNo}</ITEM_NO></TxBody>";

            return xml;
        }

        private static string get522657ReqBodyXml(ReqBody522657Model reqBodyModel)
        {
            string xml = "";
            xml = $@"<TxBody><IRMT_DATE>{reqBodyModel.irmtDate}</IRMT_DATE><IRMT_SRLNO>{reqBodyModel.irmtSrlno}</IRMT_SRLNO><BRH_COD>{reqBodyModel.brhCod}</BRH_COD></TxBody>";

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
            string TxnId = type == "622685" ? "TP622685" : "FPS522657";

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
                    

                    //XmlDocument xmlRes = new XmlDocument();
                    //xmlRes.LoadXml(ServiceResult);

                    //return xmlRes;

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
        private static async Task<Res622685> proc622685Res(string strRes, string xmlHeaderReq, string xmlBodyReq, string procDate)
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
            res622685.hretrn = "";

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
                                string setlId = xn["SETL_ID"].InnerText;
                                string rmtType = xn["RMT_TYPE"].InnerText;

                                FRT_XML_R_622685 rec = new FRT_XML_R_622685();
                                rec.HEADER = strHead;
                                rec.ETARRY = "";
                                rec.SETL_ID = xn["SETL_ID"].InnerText.Trim();
                                rec.RMT_TYPE = xn["RMT_TYPE"].InnerText.Trim();
                                rec.STATUS = xn["STATUS"].InnerText.Trim();
                                rec.ERROR_COD = xn["ERROR_COD"].InnerText.Trim();
                                rec.ORG_RMT_SRLNO = xn["ORG_RMT_SRLNO"].InnerText.Trim();
                                rec.NEW_RMT_SRLNO = xn["NEW_RMT_SRLNO"].InnerText.Trim();
                                rec.IRMT_SRLNO = xn["IRMT_SRLNO"].InnerText.Trim();
                                rec.FAST_NO = rec.SETL_ID.Length == 20 ? rec.SETL_ID.Substring(10) : "";
                                rec.RMT_TYPE = xn["RMT_TYPE"].InnerText.Trim();
                                rec.STATUS = xn["STATUS"].InnerText.Trim();
                                rec.HERRID = strHERRID;
                                rec.CRT_TIME = DateTime.Now;

                                if ("".Equals(rec.FAST_NO))
                                    continue;

                                FRT_XML_R_622685 db622685 = db.FRT_XML_R_622685.Where(x => x.FAST_NO == rec.FAST_NO).FirstOrDefault();
                                if (db622685 != null) {
                                    addLog("rec.FAST_NO:" + rec.FAST_NO + "已存在 FRT_XML_R_622685");
                                    addLog("FRT_XML_R_622685.RMT_TYPE:" + db622685.RMT_TYPE + "；FubonApi.RMT_TYPE:" + rec.RMT_TYPE);
                                    addLog("FRT_XML_R_622685.STATUS:" + db622685.STATUS + "；FubonApi.STATUS:" + rec.STATUS);
                                    addLog("FRT_XML_R_622685.ERROR_COD:" + db622685.ERROR_COD + "；FubonApi.ERROR_COD:" + rec.ERROR_COD);
                                    addLog("FRT_XML_R_622685.IRMT_SRLNO:" + db622685.IRMT_SRLNO + "；FubonApi.IRMT_SRLNO:" + rec.IRMT_SRLNO);
                                    if (db622685.RMT_TYPE.Equals(rec.RMT_TYPE) & db622685.STATUS.Equals(rec.STATUS) 
                                        & db622685.ERROR_COD.Equals(rec.ERROR_COD) & db622685.IRMT_SRLNO.Equals(rec.IRMT_SRLNO))
                                    {
                                        addLog("已存在 FRT_XML_R_622685(不更新)");
                                    }
                                    else
                                    {
                                        db622685.HEADER = rec.HEADER;
                                        db622685.ETARRY = rec.ETARRY;
                                        db622685.RMT_TYPE = rec.RMT_TYPE;
                                        db622685.STATUS = rec.STATUS;
                                        db622685.ERROR_COD = rec.ERROR_COD;
                                        db622685.ORG_RMT_SRLNO = rec.ORG_RMT_SRLNO;
                                        db622685.NEW_RMT_SRLNO = rec.NEW_RMT_SRLNO;
                                        db622685.IRMT_SRLNO = rec.IRMT_SRLNO;
                                        db622685.SQL_STS = rec.SQL_STS;

                                        db622685.UPD_TIME = DateTime.Now;
                                        db.SaveChanges();

                                        //若錯誤代碼=RM99的...後續要呼叫522657電文
                                        if ("99".Equals(rec.ERROR_COD))
                                            proc522657(procDate, rec.FAST_NO, rec.IRMT_SRLNO);   //呼叫522657電文


                                        if (("1".Equals(rec.RMT_TYPE) || "2".Equals(rec.RMT_TYPE))
                                            && ("1".Equals(rec.STATUS) || "2".Equals(rec.STATUS)))
                                        {
                                            await FastErrorApi(rec.STATUS, rec.FAST_NO, rec.ERROR_COD);
                                        }
                                        addLog("已存在 FRT_XML_R_622685(更新)");
                                    }
                                }
                                    
                                else
                                {
                                    db.FRT_XML_R_622685.Add(rec);
                                    db.SaveChanges();

                                    //若錯誤代碼=RM99的...後續要呼叫522657電文
                                    if ("99".Equals(rec.ERROR_COD))
                                        proc522657(procDate, rec.FAST_NO, rec.IRMT_SRLNO);   //呼叫522657電文

                                    //call522657List.Add(rec);

                                    //系統收到622685的電文時，符合下列情境，需回寫FRTBARM0失敗
                                    //STATUS為1-匯出異常，通知財務部窗口，暫存檔狀態停留原狀態2-已匯款
                                    //(RMT_TYPE)類別:1 - 匯出異常(匯出匯款)  且(STATUS): 2表示人工退還客戶
                                    //(RMT_TYPE)類別:2 :匯出被退匯(匯入匯款)，(STATUS): 2表示人工退還客戶
                                    if (("1".Equals(rec.RMT_TYPE) || "2".Equals(rec.RMT_TYPE))
                                            && ("1".Equals(rec.STATUS) || "2".Equals(rec.STATUS)))
                                    {
                                        await FastErrorApi(rec.STATUS, rec.FAST_NO, rec.ERROR_COD);
                                    }


                                }
                            }
                        }
                    }
                    else
                    {
                        XmlNode bodyErrNode = xmlRes.SelectSingleNode("/Tx/TxBody");
                        string EMSGTXT = bodyErrNode["EMSGTXT"].InnerText.Trim();


                        addLog("622685 ERR--> EMSGTXT:" + EMSGTXT);

                        //FRT_XML_R_622685 rec = new FRT_XML_R_622685();
                        //rec.HEADER = strHead;
                        //rec.ETARRY = "";
                        //rec.SETL_ID = "";
                        //rec.HERRID = strHERRID;FubonBankUrl
                        //rec.EMSGTXT = EMSGTXT;
                        //rec.CRT_TIME = DateTime.Now;

                        //writeR622685(db, rec);

                    }
                }


            }


            addLog("-----proc622685Res end-----");



            res622685.hretrn = strHRETRN;   //modify by daiyu 20181228
            return res622685;

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
                rec.IRMT_SRLNO = reqBodyModel.irmtSrlno;
                rec.T_BRH_COD = reqBodyModel.brhCod;

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

                                rec.R_IRMT_DATE = bodyNode["IRMT_DATE"].InnerText.Trim();
                                rec.IRMT_TIME = bodyNode["IRMT_TIME"].InnerText.Trim();
                                rec.RMT_AMT = bodyNode["RMT_AMT"].InnerText.Trim();
                                rec.RMT_TYPE = bodyNode["RMT_TYPE"].InnerText.Trim();
                                rec.RMT_BNK = bodyNode["RMT_BNK"].InnerText.Trim();
                                rec.RMT_BNK_NAME = bodyNode["RMT_BNK_NAME"].InnerText.Trim();
                                rec.RCV_BNK = bodyNode["RCV_BNK"].InnerText.Trim();
                                rec.RCV_BNK_NAME = bodyNode["RCV_BNK_NAME"].InnerText.Trim();
                                rec.RCV_ACT_NO = bodyNode["RCV_ACT_NO"].InnerText.Trim();
                                rec.RCV_CUST_NAME = bodyNode["RCV_CUST_NAME"].InnerText.Trim();
                                rec.EC_RCV_ACT_NO = bodyNode["EC_RCV_ACT_NO"].InnerText.Trim();
                                rec.EC_CUST_NAME = bodyNode["EC_CUST_NAME"].InnerText.Trim();
                                rec.RCV_NAME = bodyNode["RCV_NAME"].InnerText.Trim();
                                rec.EC_RCV_NAME = bodyNode["EC_RCV_NAME"].InnerText.Trim();
                                rec.RMT_NAME = bodyNode["RMT_NAME"].InnerText.Trim();
                                rec.RMT_APX = bodyNode["RMT_APX"].InnerText.Trim();
                                rec.IRMT_STS = bodyNode["IRMT_STS"].InnerText.Trim();
                                rec.PRS_RSLT = bodyNode["PRS_RSLT"].InnerText.Trim();
                                rec.PRS_DATE = bodyNode["PRS_DATE"].InnerText.Trim();
                                rec.TLXSNO = bodyNode["TLXSNO"].InnerText.Trim();
                                rec.BTBSNO = bodyNode["BTBSNO"].InnerText.Trim();
                                rec.RETURN_DATE = bodyNode["RETURN_DATE"].InnerText.Trim();
                                rec.PRS_SRLNO = bodyNode["PRS_SRLNO"].InnerText.Trim();
                                rec.ORG_BTBSNO = bodyNode["ORG_BTBSNO"].InnerText.Trim();
                                rec.REJ_RSN_TXT = bodyNode["REJ_RSN_TXT"].InnerText.Trim();
                                rec.EMP_ID = bodyNode["EMP_ID"].InnerText.Trim();
                                rec.PRS_SUP_ID = bodyNode["PRS_SUP_ID"].InnerText.Trim();
                                rec.R_BRH_COD = bodyNode["BRH_COD"].InnerText.Trim();
                                rec.ORG_RMT_SRLNO = bodyNode["ORG_RMT_SRLNO"].InnerText.Trim();
                                rec.ORG_RMT_DATE = bodyNode["ORG_RMT_DATE"].InnerText.Trim();
                                rec.ERROR_COD = bodyNode["ERROR_COD"].InnerText.Trim();

                            }
                            else
                            {
                                XmlNode bodyErrNode = xmlRes.SelectSingleNode("/Tx/TxBody");
                                string EMSGTXT = bodyErrNode["EMSGTXT"].InnerText.Trim();
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


                        SYS_SEQ sysDeq = db.SYS_SEQ.Where(x => x.SYS_CD == sysCd & x.SEQ_TYPE == cType & x.PRECODE == cPreCode).FirstOrDefault<SYS_SEQ>();

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
            //}


        }

        private static async Task FastErrorApi(string status, string fastNo, string errorCode)
        {
            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 authorization header
                client.DefaultRequestHeaders.Add("authorization", "token {'6D9310E55EB72CA5D7BBC8F98DD517BC'}");

                // 準備寫入的 data
                ErrorModel postData = new ErrorModel() { ExecType = status, Fast_No = fastNo, ErrorCode = errorCode, ErrorMsg = "", TextType = "622685", EMSGTXT = "" };

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
        }

        /// <summary>
        /// 622685 Request Body 參數
        /// </summary>
        internal class ReqBody622685Model
        {
            public string actDate { get; set; }
            public string brhType01 { get; set; }
            public string itemNo { get; set; }
        }

        /// <summary>
        /// 522657 Request Body 參數
        /// </summary>
        internal class ReqBody522657Model
        {
            public string irmtDate { get; set; }
            public string irmtSrlno { get; set; }
            public string brhCod { get; set; }
        }


        /// <summary>
        /// Response Head 參數
        /// </summary>
        internal class ResHeadModel
        {
            public string hmsgid { get; set; }
            public string herrid { get; set; }
            public string hsyday { get; set; }
            public string hsytime { get; set; }
            public string hwsid { get; set; }
            public string hstano { get; set; }
            public string hdtlen { get; set; }
            public string hreqq1 { get; set; }
            public string hrepq1 { get; set; }
            public string hdrvq1 { get; set; }
            public string hpvdq1 { get; set; }
            public string hpvdq2 { get; set; }
            public string hsycvd { get; set; }
            public string htlid { get; set; }
            public string htxtid { get; set; }
            public string hfmtid { get; set; }
            public string hretrn { get; set; }
            public string hslgnf { get; set; }
            public string hspsck { get; set; }
            public string hrtncd { get; set; }
            public string hsbtrf { get; set; }
            public string hfill { get; set; }
        }


        /// <summary>
        /// Response Body 參數
        /// </summary>
        internal class ResBodyModel
        {
            public string setlId { get; set; }
            public string rmtType { get; set; }
            public string status { get; set; }
            public string errorCod { get; set; }
            public string orgRmtSrlno { get; set; }
            public string newRmtSrlno { get; set; }
            public string irmtSrlno { get; set; }
            public string herrid { get; set; }
            public string emsgtxt { get; set; }
            public string fastNo { get; set; }
        }


        /// <summary>
        /// 寫LOG FILE
        /// </summary>
        /// <param name="message"></param>
        private static void writeFile(string message)
        {
            //string path = @"LOG";

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
