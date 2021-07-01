using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Enum;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml;

/// <summary>
/// 功能說明：622685+522657單筆查詢作業
/// 初版作者：20180912 Daiyu
/// 修改歷程：20180912 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB011Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB011/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            return View();
        }


        /// <summary>
        /// 查詢"FRTBARM0  快速付款匯款申請檔"
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="funCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult callBankApi(string fastNo,string funCode)
        {
            logger.Info("callBankApi begin!!");
            
            fastNo = fastNo?.Trim();
            SysParaDao sysParaDao = new SysParaDao();
            List<SYS_PARA> apiPara = sysParaDao.qryForGrpId("RT", "FBBankApi");

            string url = "";
            string SPName = "";
            string LoginID = "";
            string errorMsg = string.Empty;

            if (apiPara == null)
                return Json(new { success = false, err = "無北富銀API相關設定!!" }, JsonRequestBehavior.AllowGet);
            else
            {
                url = apiPara.Where(x => x.PARA_ID == "FubonBankUrl").FirstOrDefault()?.PARA_VALUE;
                SPName = apiPara.Where(x => x.PARA_ID == "SPName").FirstOrDefault()?.PARA_VALUE;
                LoginID = apiPara.Where(x => x.PARA_ID == "LoginID").FirstOrDefault()?.PARA_VALUE;

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(SPName) || string.IsNullOrWhiteSpace(LoginID))
                    return Json(new { success = false, err = "無北富銀API相關設定!!" }, JsonRequestBehavior.AllowGet);
            }

            FRTBARMModel fRTBARMModel = new FRTBARMModel();
            //查詢該筆快速付款案件於AS400的內容
            using (EacConnection con = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                con.Open();

                FRTBARMDao fRTBARMDao = new FRTBARMDao();

                fRTBARMModel = fRTBARMDao.qryByFastNo(fastNo, con);
                if (string.IsNullOrWhiteSpace(fRTBARMModel.fastNo))
                    return Json(new { success = false, err = "FRTBARM0查無對應的快速付款編號!!" }, JsonRequestBehavior.AllowGet);
                else
                    if (fRTBARMModel.remitDate.Trim().Length < 7)
                    return Json(new { success = false, err = "FRTBARM0 匯款日期錯誤:" + fRTBARMModel.remitDate.Trim() }, JsonRequestBehavior.AllowGet);
            }

            string procDate = DateUtil.As400ChtDateToADDate(fRTBARMModel.remitDate);

            try
            {
                FRT_XML_R_622685_NEW rec622685New = new FRT_XML_R_622685_NEW();
                string _msg = string.Empty;

                var _data = new FRTXmlR622685Dao().qryLstByFastNo(fastNo?.Trim());

                string _actDate = procDate;
                string _uuid = null;
                string _funCode = funCode?.Trim();

                if (_funCode == "0")
                {
                    if (procDate != DateTime.Now.ToString("yyyyMMdd"))
                    {
                        _msg = $@"此快速付款編號:{fastNo},匯出日期為:{procDate},匯出日期需為當日才能使用 FunCode:0 發查!";
                    }
                    else
                    {
                        using (dbFGLEntities db = new dbFGLEntities())
                        {
                            var f = db.FRT_XML_T_622823.AsNoTracking().Where(x => x.FAST_NO == fastNo)
                                .OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                            _uuid = $@"L03{f?.SETL_ID}{f?.UUID_FLG}9000400"; //20191118 宗德回L03 20191118 仁薳回 後7位9000004 20200324 中逸回  後7位9000400
                        }
                        var _result = get622685(_actDate, _uuid, _funCode, url, SPName, LoginID, fastNo);
                        rec622685New = _result.Item1;
                        _msg = _result.Item2;
                    }
                }
                else if (_funCode == "1")
                {
                    if (procDate == DateTime.Now.ToString("yyyyMMdd"))
                    {
                        var _result = get622685(_actDate, _uuid, _funCode, url, SPName, LoginID, fastNo);
                        rec622685New = _result.Item1;
                        _msg = _result.Item2;
                    }
                    else
                    {
                        _msg = @"該筆快速付款匯出日期不為今日! (FunCode:1 匯出異常只查詢今日資料)";
                    }
                }
                else if (_funCode == "2")
                {
                    var _result = get622685(_actDate, _uuid, _funCode, url, SPName, LoginID, fastNo);
                    rec622685New = _result.Item1;
                    _msg = _result.Item2;
                }
                else
                {
                    _msg = Ref.MessageType.parameter_Error.GetDescription();
                }
                if (!_msg.IsNullOrWhiteSpace())
                {
                    return Json(new { success = false, err = _msg }, JsonRequestBehavior.AllowGet);
                }

                switch (rec622685New.FunCode)
                {
                    case "0":
                        _msg = $@"FunCode : 0 (同步匯出匯款狀況) ";
                        break;
                    case "1":
                        _msg = $@"FunCode : 1 匯出異常(匯出匯款) ";
                        break;
                    case "2":
                        _msg = $@"FunCode : 2 匯出被退匯(匯入匯款) ";
                        break;
                    default:
                        _msg = $@"FunCode : {rec622685New.FunCode} ";
                        break;
                }

                switch (rec622685New.Status)
                {
                    case "0":
                        _msg += $@"Status : 0 (處理中) pending狀態 (後續狀態會變成 1 or 4 or 6) ";
                        break;
                    case "1":
                        _msg += $@"Status : 1 (匯出異常)  後續 => 等待 FunCode 1 查詢 ";
                        break;
                    case "2":
                        _msg += $@"Status : 2 (人工退還客戶) 人壽不會發生  (已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態) ";
                        break;
                    case "3":
                        _msg += $@"Status : 3 (匯出失敗) 後續 => 查詢代碼判斷歸屬 ";
                        break;
                    case "4":
                        _msg += $@"Status : 4 (已匯出) ";
                        break;
                    case "5":
                        _msg += $@"Status : 5 (匯出成功 : 人工重匯) ";
                        break;
                    case "6":
                        _msg += $@"Status : 6 (無資料) 後續 => 請人工查詢該筆資料狀態 ";
                        break;
                    case "8":
                        _msg += $@"Status = 8 (請OP處理) pending狀態 (已和北富銀約定颱風天轉下個工作日匯出，故不會出現此狀態) ";
                        break;
                    case "9":
                        _msg += $@"Status = 9 (被退匯未處理) pending狀態 (後續狀態會變成 3 or 5) ";
                        break;
                    //PS : 3 & 5 不會互相覆蓋，故同一筆快速付款有可能出現兩種狀態，這時候就須使用
                    // ActDate(帳務日) & ucaIRMTSrlNo(匯出被退匯匯入序號) 來判斷最終狀態為何筆
                    default:
                        _msg += $@"Status = {rec622685New.Status} ";
                        break;
                }
                var jsonData = new { success = true, msg = _msg };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 呼叫 622685 電文 , 並回傳結果 或 錯誤訊息
        /// </summary>
        /// <param name="actDate">帳務日</param>
        /// <param name="uuid">交易序號</param>
        /// <param name="funCode">選項</param>
        /// <param name="url">FubonBankUrl</param>
        /// <param name="SPName">SPName</param>
        /// <param name="LoginID">LoginID</param>
        /// <returns></returns>
        private Tuple<FRT_XML_R_622685_NEW,string> get622685(string actDate, string uuid, string funCode,string url,string SPName,string LoginID,string fastNo)
        {
            ResHeadModel resHead = new ResHeadModel();
            ResHeadModel qResHead = new ResHeadModel();
            string _msg = string.Empty;
            FRT_XML_R_622685_NEW rec622685New = new FRT_XML_R_622685_NEW();
            FRT_XML_T_622685_NEW T622685_new = new FRT_XML_T_622685_NEW();
            bool bContinue = true;
            string irmtSrlNo = "";
            //modify by daiyu20181228
            //當<HRETRN>值為’C’時
            //請將Response的 < TxHead > +Request的 < TxBody > 組起來後再送一次ESB
            //直到<HRETRN> 值為’E’時結束
            string hretrn = "";
            string xmlHeaderReq = "";
            string xmlHeaderRes = "";
            //FastErrModel _fastErrModel_1 = procAS400Err(new FRT_XML_R_622685_NEW()
            //{
            //    FAST_NO = "Q070300258",
            //    FunCode = "2",
            //    Status = "3",
            //    ucaIRMTSrlNo = "0000029"
            //});
            //FastErrModel _fastErrModel_2 = procAS400Err(new FRT_XML_R_622685_NEW()
            //{
            //    FAST_NO = "Q070300304",
            //    FunCode = "2",
            //    Status = "3",
            //    ucaIRMTSrlNo = "0000030"
            //});
            try
            {
                #region 呼叫622685電文
                while (bContinue)
                {
                    ReqHeadModel reqHeadModel = getReqHeadPara("622685", "");
                    #region 新版622685
                    ReqBody622685Model_New reqBodyModel = get622685NewReqBodyPara(actDate, funCode, uuid);

                    if ("C".Equals(hretrn))
                        xmlHeaderReq = xmlHeaderRes;
                    else
                        xmlHeaderReq = getReqHeadXml(reqHeadModel);

                    string xmlBodyReq = get622685NewReqBodyXml(reqBodyModel);

                    //呼叫北富銀WEB API
                    string xmlRes = "";
                    try
                    {
                        T622685_new.HEADER = xmlHeaderReq;
                        T622685_new.ActDate = reqBodyModel.ActDate;
                        T622685_new.UUID = reqBodyModel.UUID;
                        T622685_new.FunCode = reqBodyModel.FunCode;
                        T622685_new.CRT_TIME = DateTime.Now;
                        T622685_new.EXEC_TYPE = "W";
                        T622685_new.SQL_STS = "0";
                        T622685_new.CRT_UID = Session["UserID"]?.ToString();
                        var _w622685msg = new FRTXmlT622685Dao().writeT622685New(T622685_new);
                        if (!string.IsNullOrWhiteSpace(_w622685msg))
                            logger.Error(_w622685msg);
                        xmlRes = callFBBankApi("622685", xmlHeaderReq, xmlBodyReq, url, SPName, LoginID);
                    }
                    catch (Exception e)
                    {
                        bContinue = false;
                        logger.Error(e.ToString());
                        return new Tuple<FRT_XML_R_622685_NEW, string>(null, "呼叫北富銀失敗!!" + e.Message);
                    }
                    try
                    {
                        //處理回傳結果
                        resHead = proc622685NewRes(fastNo, xmlRes, xmlHeaderReq, xmlBodyReq, irmtSrlNo, rec622685New, funCode);

                        hretrn = resHead.hretrn;

                        if (resHead.fastNo != null) //有找到該筆快速付款編號
                        {
                            qResHead = resHead;
                        }

                        if ("C".Equals(resHead.hretrn)) //hretrn 等於 C => 有下一批資料,等於 E => 錯誤 
                        {
                            bContinue = true;
                            xmlHeaderRes = "<TxHead>" + resHead.xmlHeaderRes + "</TxHead>";
                        }
                        else
                        {
                            bContinue = false;
                        }
                        if (!"0000".Equals(resHead.herrid)) // herrid 等於 ERA0 => 查無資料
                        {
                            logger.Error(Session["UserID"].ToString());
                            logger.Error(resHead.emsgtxt);
                            _msg = "呼叫北富銀結果：" + resHead.emsgtxt;
                        }
                    }
                    catch (Exception ex)
                    {
                        bContinue = false;
                        logger.Error(ex.ToString());
                        resHead.progErr = ex.Message;
                    }
                    #endregion
                }
                #endregion
                if (!_msg.IsNullOrWhiteSpace())
                    return new Tuple<FRT_XML_R_622685_NEW, string>(null, _msg);

                var _fastNo = rec622685New.FAST_NO;
                var _funCode = rec622685New.FunCode;
                var _status = rec622685New.Status;

                #region 呼叫522657電文
                logger.Info("qResHead.irmtSrlNo:" + qResHead.irmtSrlNo);
                if (!string.IsNullOrWhiteSpace(qResHead.irmtSrlNo))
                    proc522657(fastNo, qResHead.irmtSrlNo, actDate, url, SPName, LoginID);
                #endregion

                logger.Info(_fastNo + "==>resHead.progErr:" + qResHead.progErr);

                #region 處理AS400相關作業
                if ("".Equals(StringUtil.toString(qResHead.progErr)))
                {
                    logger.Info(_fastNo + "==>rec622685.FunCode:" + _funCode);
                    logger.Info(_fastNo + "==>rec622685.Status:" + _status);

                    if (
                        //(rec622685New.FunCode == "0" && rec622685New.Status == "6") || //無資料
                        //(rec622685New.FunCode == "0" && rec622685New.Status == "0") || //處理中
                        (rec622685New.FunCode == "1" && rec622685New.Status == "1") || //匯出異常(將轉隔日匯出)
                        (rec622685New.FunCode == "1" && rec622685New.Status == "2") || //應該不會有此狀態
                        (rec622685New.FunCode == "2" && rec622685New.Status == "3"))  //匯出失敗(人工退還客戶) 
                    {
                        logger.Info(_fastNo + "==>procAS400Err");
                        FastErrModel fastErrModel = procAS400Err(rec622685New);
                        if (!"".Equals(StringUtil.toString(fastErrModel.errorMsg)))
                        {
                            logger.Error(_fastNo + "==>fastErrModel.errorMsg:" + fastErrModel.errorMsg);
                            _msg = fastErrModel.errorMsg;
                        }
                    }
                }
                else
                {
                    logger.Error(_fastNo + "==> resHead.progErr :" + qResHead.progErr);
                    _msg = qResHead.progErr;
                }
                #endregion
            }
            catch (Exception ex)
            {
                _msg = ex.ToString();
                logger.Error(_msg);
            }

            return new Tuple<FRT_XML_R_622685_NEW, string>(rec622685New, _msg);
        }

        private FastErrModel procAS400Err(FRT_XML_R_622685_NEW rec622685_new)
        {
            FastErrModel fastErrModel = new FastErrModel();
            FastErrUtil fastErrUtil = new FastErrUtil();
            fastErrModel = fastErrUtil.procFailNotify("S", $@"{rec622685_new.FunCode};{rec622685_new.Status}", rec622685_new.FAST_NO, "622685", rec622685_new.ucaErrorCod, "");         
            return fastErrModel;
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
            reqHeadModel.htxtid = type == "622685" ? "FEP622685" : "NB522657"; //20200326 中逸 NB522657
            reqHeadModel.hwsid = "FBL_FPS";
            reqHeadModel.htlid = type == "622685" ? "7154151" : "0071571541519"; //20200330 中逸 0071571541519

            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = strToday;
            var cId = sysSeqDao.qrySeqNo("RT", "FRQ", qPreCode).ToString();

            reqHeadModel.hstano = cId;
            reqHeadModel.hretrn = hretrn;
            reqHeadModel.func = type == "622685" ? string.Empty : "1"; //20200330 中逸 新增 HFUNC = 1 (固定)

            logger.Info("ReqHeadModel-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqHeadModel))
            {
                logger.Info(desc.Name + ":" + desc.GetValue(reqHeadModel));
            }

            return reqHeadModel;
        }

        /// <summary>
        /// 取得request的 body 參數值
        /// </summary>
        /// <param name="procDate"></param>
        /// <param name="funCode"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        private static ReqBody622685Model_New get622685NewReqBodyPara(string procDate, string  funCode ,string uuid)
        {
            ReqBody622685Model_New reqBodyModel = new ReqBody622685Model_New();
            reqBodyModel.ActDate = procDate;
            reqBodyModel.FunCode = funCode;
            reqBodyModel.UUID = uuid;

            logger.Info("ReqBody622685Model_New-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqBodyModel))
            {
                logger.Info(desc.Name + ":" + desc.GetValue(reqBodyModel));
            }
            return reqBodyModel;
        }


        /// <summary>
        /// 組合request xml的head部分
        /// </summary>
        /// <param name="reqHeadModel"></param>
        /// <returns></returns>
        private static string getReqHeadXml(ReqHeadModel reqHeadModel)
        {
            string xml = "";
            if (reqHeadModel.htxtid.Equals("NB522657"))
                xml = $@"<TxHead><HTXTID>{reqHeadModel.htxtid}</HTXTID><HWSID>{reqHeadModel.hwsid}</HWSID><HTLID>{reqHeadModel.htlid}</HTLID><HSTANO>{reqHeadModel.hstano}</HSTANO><HRETRN>{reqHeadModel.hretrn}</HRETRN><HFUNC>{reqHeadModel.func}</HFUNC></TxHead>";
            else
                xml = $@"<TxHead><HTXTID>{reqHeadModel.htxtid}</HTXTID><HWSID>{reqHeadModel.hwsid}</HWSID><HTLID>{reqHeadModel.htlid}</HTLID><HSTANO>{reqHeadModel.hstano}</HSTANO><HRETRN>{reqHeadModel.hretrn}</HRETRN></TxHead>";
            return xml;
        }

        /// <summary>
        ///  組合request xml的body部分
        /// </summary>
        /// <param name="reqBodyModel"></param>
        /// <returns></returns>
        private static string get622685NewReqBodyXml(ReqBody622685Model_New reqBodyModel)
        {
            string xml = "";
            xml = $@"<TxBody><ActDate>{reqBodyModel.ActDate}</ActDate><UUID>{reqBodyModel.UUID}</UUID><FunCode>{reqBodyModel.FunCode}</FunCode></TxBody>";
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

        /// <summary>
        /// 呼叫北富銀電文
        /// </summary>
        /// <param name="xmlHeaderReq"></param>
        /// <param name="xmlBodyReq"></param>
        /// <returns></returns>
        private static string callFBBankApi(string type, string xmlHeaderReq, string xmlBodyReq, string url, string SPName, string LoginID)
        {

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

            logger.Info("send request-->" + xmlReq);

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
                    logger.Info("response:" + ServiceResult);
                    return ServiceResult;
                }
            }
        }


        private ResHeadModel proc622685NewRes(string fastNo, string strRes, string xmlHeaderReq, string xmlBodyReq, string irmtSrlNo, FRT_XML_R_622685_NEW rec622685,string funCode)
        {
            ResHeadModel resHead = new ResHeadModel();
            string strHRETRN = "";

            XmlDocument xmlRes = new XmlDocument();
            xmlRes.LoadXml(strRes);

            DateTime dtn = DateTime.Now;

            XmlNode headNode = xmlRes.SelectSingleNode("/Tx/TxHead");
            string strHead = headNode.InnerXml;
            resHead.xmlHeaderRes = strHead;

            if (headNode != null)
            {

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    strHRETRN = headNode["HRETRN"].InnerText.Trim();
                    string strHERRID = headNode["HERRID"].InnerText.Trim();
                    resHead.herrid = strHERRID;
                    resHead.hretrn = strHRETRN;

                    if ("0000".Equals(strHERRID))
                    {
                        XmlNodeList dList = xmlRes.SelectNodes("/Tx/TxBody/TxRepeat");

                        foreach (XmlNode xn in dList)
                        {
                            if (xn != null)
                            {

                                FRT_XML_R_622685_NEW rec_New = new FRT_XML_R_622685_NEW();
                                rec_New.HEADER = strHead;
                                rec_New.BufferData = "";
                                rec_New.HeadUUID = xn["UUID"]?.InnerText?.Trim(); //UUID組法 => L03 + ‘XM96.SETL_ID’(7154151ILI(固定值10碼)  + ‘快速付款編號(10碼)’ ex : 7154151ILIQ070100038) + ‘XM96.UUID_FLAG’(01~99) + 9000400(20200324 中逸回 9000400)  ex : L037154151ILIQ070100038019000400
                                rec_New.FAST_NO = (rec_New.HeadUUID != null && rec_New.HeadUUID.Length == 32) ? rec_New.HeadUUID.Substring(13, 10) : "";
                                
                                if (fastNo.Equals(rec_New.FAST_NO))
                                {
                                    resHead.fastNo = fastNo; //快速付款編號
                                    rec_New.FunCode = xn["FunCode"]?.InnerText?.Trim(); //選項
                                    rec_New.Status = xn["Status"]?.InnerText?.Trim(); //匯款狀況
                                    rec_New.ucaErrorCod = xn["FiscCode"]?.InnerText?.Trim(); //檢核錯誤代碼/金資異常代碼
                                    rec_New.ucaIRMTSrlNo = xn["IRMTSrlNo"]?.InnerText?.Trim(); //匯出被退匯匯入序號
                                    rec_New.ucaOrgRmtSrlNo = xn["OrgRmtSrlNo"]?.InnerText?.Trim(); //原匯出序號
                                    rec_New.NewRmtSrlNo = xn["NewRmtSrlNo"]?.InnerText?.Trim(); //新匯出序號
                                    rec_New.HERRID = strHERRID; 
                                    rec_New.CRT_TIME = dtn;
                                    rec_New.ActDate = dtn.Date;
                                    //匯出時間
                                    rec_New.TxnTime = xn["TxnTime"]?.InnerText?.Trim();
                                    //電文序號
                                    rec_New.TelSeq = xn["TelSeq"]?.InnerText?.Trim();
                                    //NextKey
                                    rec_New.NextKey = xn["NextKey"]?.InnerText?.Trim();

                                    FRT_XML_R_622685_NEW db622685_NEW = db.FRT_XML_R_622685_NEW.Where(x => x.FAST_NO == rec_New.FAST_NO && x.FunCode == funCode).OrderByDescending(x=>x.CRT_TIME).FirstOrDefault();

                                    if (db622685_NEW != null)
                                    {
                                        if (rec_New.FunCode == "0" || rec_New.FunCode == "1")
                                        {
                                            logger.Info("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                            logger.Info("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                            logger.Info("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                            logger.Info("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                            if (db622685_NEW.FunCode.Equals(rec_New.FunCode) && db622685_NEW.Status.Equals(rec_New.Status) && db622685_NEW.ucaErrorCod.Equals(rec_New.ucaErrorCod))
                                            {
                                                resHead.progErr = "已存在 FRT_XML_R_622685(不更新)";
                                                logger.Info("已存在 FRT_XML_R_622685(不更新)");
                                            }
                                            else
                                            {
                                                rec622685.FAST_NO = rec_New.FAST_NO;
                                                rec622685.ucaErrorCod = rec_New.ucaErrorCod;
                                                rec622685.FunCode = rec_New.FunCode;
                                                rec622685.Status = rec_New.Status;

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
                                                db622685_NEW.UPD_TIME = dtn;
                                                db.SaveChanges();
                                                if ("99".Equals(rec_New.ucaErrorCod))
                                                    resHead.irmtSrlNo = rec_New.ucaIRMTSrlNo;
                                                logger.Info("已存在 FRT_XML_R_622685(更新)");
                                            }
                                            break;
                                        }
                                        //接收狀態為過度檔
                                        else if (rec_New.FunCode == "2" && rec_New.Status == "9")
                                        {
                                            if (db622685_NEW.FunCode == "2" && db622685_NEW.Status == "9" && ((rec_New.ActDate == db622685_NEW.ActDate) && ((rec_New.ucaIRMTSrlNo.stringToInt()) == (db622685_NEW.ucaIRMTSrlNo.stringToInt()))))
                                            {
                                                //上一次狀態還是在過度檔狀態,且為同一筆資料時,不更新
                                                logger.Info("已存在 FRT_XML_R_622685(過度狀態 不更新)");
                                            }
                                            //傳送日期是新的 or 傳送日期為當天 且符合 北富銀回傳的匯出序號 須新增一筆新的
                                            else if (rec_New.ActDate > db622685_NEW.ActDate || ((rec_New.ActDate == db622685_NEW.ActDate) && (rec_New.ucaIRMTSrlNo.stringToInt()) > (db622685_NEW.ucaIRMTSrlNo.stringToInt())))
                                            {
                                                rec622685.FAST_NO = rec_New.FAST_NO;
                                                rec622685.ucaErrorCod = rec_New.ucaErrorCod;
                                                rec622685.FunCode = rec_New.FunCode;
                                                rec622685.Status = rec_New.Status;
                                                db.FRT_XML_R_622685_NEW.Add(rec_New);
                                                db.SaveChanges();
                                                logger.Info("已新增 FRT_XML_R_622685(過度狀態 新增)");
                                            }
                                            break;
                                        }
                                        //目前資料庫的資料為這筆資料的過度狀態 僅更新該筆過度狀態
                                        else if (db622685_NEW.FunCode == "2" && db622685_NEW.Status == "9" && ((rec_New.ActDate == db622685_NEW.ActDate) && ((rec_New.ucaIRMTSrlNo.stringToInt()) == (db622685_NEW.ucaIRMTSrlNo.stringToInt()))))
                                        {
                                            logger.Info("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                            logger.Info("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                            logger.Info("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                            logger.Info("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                            logger.Info("FRT_XML_R_622685.ucaIRMTSrlNo:" + db622685_NEW.ucaIRMTSrlNo + "；FubonApi.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);
                                            rec622685.FAST_NO = rec_New.FAST_NO;
                                            rec622685.ucaErrorCod = rec_New.ucaErrorCod;
                                            rec622685.FunCode = rec_New.FunCode;
                                            rec622685.Status = rec_New.Status;

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
                                            if ("99".Equals(rec_New.ucaErrorCod))
                                                resHead.irmtSrlNo = rec_New.ucaIRMTSrlNo;
                                            logger.Info("已存在 FRT_XML_R_622685(更新過度檔)");
                                            break;
                                        }
                                        //判斷回傳的資料與現有的資料的時間 (只更新最新的回傳資料) 條件 1.傳送日期是新的 or 2.傳送日期為當天 但是要看 北富銀回傳的流水號
                                        else if ((rec_New.ActDate > db622685_NEW.ActDate || ((rec_New.ActDate == db622685_NEW.ActDate) && rec_New.ucaIRMTSrlNo.stringToInt() > db622685_NEW.ucaIRMTSrlNo.stringToInt())))
                                        {
                                            logger.Info("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                            logger.Info("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                            logger.Info("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                            logger.Info("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                            logger.Info("FRT_XML_R_622685.ucaIRMTSrlNo:" + db622685_NEW.ucaIRMTSrlNo + "；FubonApi.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);

                                            if (db622685_NEW.FunCode.Equals(rec_New.FunCode) && db622685_NEW.Status.Equals(rec_New.Status)
                                                && db622685_NEW.ucaErrorCod.Equals(rec_New.ucaErrorCod) && db622685_NEW.ucaIRMTSrlNo.Equals(rec_New.ucaIRMTSrlNo))
                                            {
                                                resHead.progErr = "已存在 FRT_XML_R_622685(不更新)";
                                                logger.Info("已存在 FRT_XML_R_622685(不更新)");
                                            }
                                            else
                                            {
                                                rec622685.FAST_NO = rec_New.FAST_NO;
                                                rec622685.ucaErrorCod = rec_New.ucaErrorCod;
                                                rec622685.FunCode = rec_New.FunCode;
                                                rec622685.Status = rec_New.Status;

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
                                                //db622685_NEW.UPD_TIME = dtn;
                                                db.FRT_XML_R_622685_NEW.Add(rec_New);
                                                db.SaveChanges();
                                                if ("99".Equals(rec_New.ucaErrorCod))
                                                    resHead.irmtSrlNo = rec_New.ucaIRMTSrlNo;
                                                logger.Info("已存在 FRT_XML_R_622685(更新)");
                                            }
                                            break;
                                        }
                                        //資料跟上次接收的參數是一樣的
                                        else if ((rec_New.FunCode == db622685_NEW.FunCode) && (rec_New.Status == db622685_NEW.Status) &&
                                            (rec_New.ActDate >= db622685_NEW.ActDate) && (rec_New.ucaIRMTSrlNo.stringToInt() == db622685_NEW.ucaIRMTSrlNo.stringToInt()))
                                        {
                                            logger.Info("rec.FAST_NO:" + rec_New.FAST_NO + "已存在 FRT_XML_R_622685");
                                            logger.Info("FRT_XML_R_622685.FunCode:" + db622685_NEW.FunCode + "；FubonApi.FunCode:" + rec_New.FunCode);
                                            logger.Info("FRT_XML_R_622685.Status:" + db622685_NEW.Status + "；FubonApi.Status:" + rec_New.Status);
                                            logger.Info("FRT_XML_R_622685.ucaErrorCod:" + db622685_NEW.ucaErrorCod + "；FubonApi.ucaErrorCod:" + rec_New.ucaErrorCod);
                                            logger.Info("FRT_XML_R_622685.ucaIRMTSrlNo:" + db622685_NEW.ucaIRMTSrlNo + "；FubonApi.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);
                                            resHead.progErr = "已存在 FRT_XML_R_622685(不更新)";
                                            logger.Info("已存在 FRT_XML_R_622685(不更新)");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info(rec622685.FAST_NO + ":新增");
                                        rec622685.FAST_NO = rec_New.FAST_NO;
                                        rec622685.ucaErrorCod = rec_New.ucaErrorCod;
                                        rec622685.FunCode = rec_New.FunCode;
                                        rec622685.Status = rec_New.Status;
                                        db.FRT_XML_R_622685_NEW.Add(rec_New);
                                        db.SaveChanges();

                                        logger.Info("rec.ucaErrorCod:" + rec_New.ucaErrorCod);
                                        logger.Info("rec.ucaIRMTSrlNo:" + rec_New.ucaIRMTSrlNo);
                                        //若錯誤代碼=RM99的...後續要呼叫522657電文
                                        if ("99".Equals(rec_New.ucaErrorCod))
                                            resHead.irmtSrlNo = rec_New.ucaIRMTSrlNo;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        XmlNode bodyErrNode = xmlRes.SelectSingleNode("/Tx/TxBody");
                        string EMSGTXT = bodyErrNode["EMSGTXT"].InnerText;
                        resHead.emsgtxt = EMSGTXT;
                    }
                }
            }
            logger.Info("resHead.irmtSrlNo:" + resHead.irmtSrlNo);

            return resHead;
        }

        /// <summary>
        /// 處理呼叫522657電文
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="irmtSrlNo"></param>
        /// <param name="procDate"></param>
        private void proc522657(string fastNo, string irmtSrlNo, string procDateq, string url, string SPName, string LoginID)
        {
            string strToday = DateTime.Now.ToString("yyyyMMdd");   
            ReqHeadModel reqHeadModel = getReqHeadPara("522657", "");
            ReqBody522657Model reqBodyModel = get522657ReqBodyPara(procDateq, irmtSrlNo);
            string xmlHeaderReq = getReqHeadXml(reqHeadModel);
            string xmlBodyReq = get522657ReqBodyXml(reqBodyModel);
            try
            {
                //呼叫北富銀WEB API
                string xmlRes = callFBBankApi("522657", xmlHeaderReq, xmlBodyReq, url, SPName, LoginID);
                try
                {
                    //處理回傳結果
                    proc522657Res(true, fastNo, reqBodyModel, xmlRes, xmlHeaderReq, xmlBodyReq);
                }
                catch (Exception ex)
                {
                    logger.Info(ex.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                //將本次的呼叫紀錄至 FRT_XML_T_522657
                proc522657Res(false, fastNo, reqBodyModel, null, xmlHeaderReq, xmlBodyReq);
            }
        }


        private static ReqBody522657Model get522657ReqBodyPara(string procDate, string irmtSrlno)
        {
            ReqBody522657Model reqBodyModel = new ReqBody522657Model();
            reqBodyModel.RmtDate = procDate;
            reqBodyModel.RmtSRLNo = irmtSrlno;
            reqBodyModel.BrhCode = "0715"; //522657 中逸 20200330 說改成 BrhCode(4位) 右靠左補0

            logger.Info("ReqBody522657Model-->");
            foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(reqBodyModel))
            {
                logger.Info(desc.Name + ":" + desc.GetValue(reqBodyModel));
            }
            return reqBodyModel;
        }


        private static string get522657ReqBodyXml(ReqBody522657Model reqBodyModel)
        {
            string xml = "";
            xml = $@"<TxBody><RmtDate>{reqBodyModel.RmtDate}</RmtDate><RmtSRLNo>{reqBodyModel.RmtSRLNo}</RmtSRLNo><BrhCode>{reqBodyModel.BrhCode}</BrhCode></TxBody>";
            return xml;
        }




        private static void proc522657Res(bool bSuccess, string fastNo, ReqBody522657Model reqBodyModel, string strRes, string xmlHeaderReq, string xmlBodyReq)
        {
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

                using (dbFGLEntities db = new dbFGLEntities())
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
                                string EMSGTXT = bodyErrNode["EMSGTXT"].InnerText.Trim();
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
                logger.Info(e.ToString());
            }
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

        internal class ReqBody622685Model_New
        {
            public string ActDate { get; set; }
            public string UUID { get; set; }
            public string FunCode { get; set; }
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
            public string RmtDate { get; set; }
            public string RmtSRLNo { get; set; }
            public string BrhCode { get; set; }
        }


        /// <summary>
        /// Response Head 參數
        /// </summary>
        internal class ResHeadModel
        {
            public string xmlHeaderRes { get; set; }

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

            public string emsgtxt { get; set; }

            public string fastNo { get; set; }

            public string progErr { get; set; }
            public string irmtSrlNo { get; set; }
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
            public string irmtSrlNo { get; set; }            
        }
    }
}