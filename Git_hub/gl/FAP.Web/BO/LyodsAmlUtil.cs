using FAP.Web.AS400Models;
using FAP.Web.AS400PGM;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

/// <summary>
/// 功能說明：呼叫萊斯系統，進行相關作業
/// 初版作者：20191126 Daiyu
/// 修改歷程：20191126 Daiyu
///           需求單號：201910290100-01
///           初版
/// </summary>
///


namespace FAP.Web.BO
{
    public  class LyodsAmlUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string getUrl() {
            //取得萊斯URL
            SysParaDao sysParaDao = new SysParaDao();
            SYS_PARA lyodsAml = sysParaDao.qryByKey("AP", "LyodsAml", "LyodsAmlUrl");
            string amlUrl = StringUtil.toString(lyodsAml.PARA_VALUE);
            return amlUrl;
        }

        /// <summary>
        /// 名單檢查
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public LyodsAmlFSKWSModel fskws(string lyodsAmlUrl, LyodsAmlFSKWSModel model, EacConnection conn400) {

            //取得呼叫萊斯所需的資訊
            model = getXmlInfo( model,  conn400);
           
            //呼叫萊斯
            callLyod(lyodsAmlUrl, model);

            //回寫LOG
            writeLog(model, conn400);


            return model;
        }



        private void writeLog(LyodsAmlFSKWSModel model, EacConnection conn400) {
            FAPPPASModel ppas = new FAPPPASModel();
            FAPPPASDao fAPPPASDao = new FAPPPASDao();
            ObjectUtil.CopyPropertiesTo(model, ppas);

            if (!"Y".Equals(StringUtil.toString(model.hasPPAS)))
                fAPPPASDao.insertAmlResult(ppas, conn400);
            else
                fAPPPASDao.updateAmlResult(ppas, conn400);

        }


        /// <summary>
        /// 取得呼叫萊斯所需的資訊
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn400"></param>
        /// <returns></returns>
        private LyodsAmlFSKWSModel getXmlInfo(LyodsAmlFSKWSModel model, EacConnection conn400) {
            //先檢查客戶編號是否已存在
            FAPPPASDao fAPPPASDao = new FAPPPASDao();
            FAPPPASModel ppas = fAPPPASDao.qryByCinNo(conn400, model.cin_no);



            if (!"".Equals(ppas.cin_no)) {
                model.hasPPAS = "Y";

                if(StringUtil.toString(ppas.paid_name).Equals(StringUtil.toString(model.paid_name))
                    & "".Equals(StringUtil.toString(ppas.cancel_mk)))
                    model.calc = "3";
                else
                    model.calc = "0";
            }
                
            else {
                model.hasPPAS = "N";
                model.calc = "0";

                string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');
                EacCommand cmd = new EacCommand();
                cmd.Connection = conn400;

                SGLZ001Util sGLZ001Util = new SGLZ001Util();
                SGLZ001Model sglz = new SGLZ001Model();
                sglz.numtype = "A";
                sglz.sys_type = "F";
                sglz.srce_from = "AML";
                sglz.trns_itf = "AML";
                sglz.sys_date = chtDt[0];
                sglz = sGLZ001Util.callSGLZ001(conn400, cmd, sglz);
                model.appl_id = sglz.trns_no;
            }
                

            //判斷為個人CIFI或法人CIFC
            ValidateUtil validateUtil = new ValidateUtil();
            var _paid_id = StringUtil.toString(model.paid_id).PadRight(10, ' ');

            if (!"".Equals(StringUtil.toString(_paid_id)))
            {
                if ("".Equals(StringUtil.toString(_paid_id.Substring(8))) & validateUtil.IsNum(_paid_id.Substring(0, 8)))
                    model.callType = "CIFC";
                else
                    model.callType = "CIFI";
            }
            else {
                if("GC".Equals(model.o_paid_cd) || "RC".Equals(model.o_paid_cd)||"CB".Equals(model.o_paid_cd)
                    || "CPI".Equals(model.o_paid_cd)|| "TA".Equals(model.o_paid_cd))
                    model.callType = "CIFC";
                else
                    model.callType = "CIFI";
            }
           


            //取得查詢者的單位資訊
            V_EMPLY2 usr = new V_EMPLY2();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                OaEmpDao OaEmpDao = new OaEmpDao();
                usr = OaEmpDao.qryByUsrId(model.query_id, db);
                if (usr != null)
                {
                    model.query_name = StringUtil.toString(model.query_name) == "" ? StringUtil.toString(usr.EMP_NAME) : model.query_name;
                    model.dpt_cd = StringUtil.toString(usr.DPT_CD);
                    model.dpt_name = StringUtil.toString(usr.DPT_NAME);
                }
            }

            return model;

        }

        /// <summary>
        /// 呼叫萊斯名單檢核
        /// </summary>
        /// <param name="lyodsAmlUrl"></param>
        /// <param name="model"></param>
        private void callLyod(string lyodsAmlUrl, LyodsAmlFSKWSModel model) {
            HttpWebRequest request = CreateWebRequest(lyodsAmlUrl + "FSKWS?wsdl");
            XmlDocument soapEnvelopeXml = new XmlDocument();

            var unit = model.unit;


            string strXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://webservice.lyods.com/"">
                <soapenv:Header/>
                <soapenv:Body>
                <web:FofExecCif>
                        <appCode>TGP</appCode>	
                        <unit>" + model.unit + @"</unit>
                        <callType>" + model.callType + @"</callType>	
                        <messageId>" + model.cin_no + @"</messageId>
                        <fmt>CIF</fmt>
                        <usercode></usercode>
						<userdata></userdata>		
                        <calc>" + model.calc + @"</calc>
                        <isFull>false</isFull>		
                        <sendTime/>
                        <xml><![CDATA[
                       <cif>
							<CINO>" + model.cin_no + @"</CINO>	
                            <FIELD name=""status"">000</FIELD>     
                            <FIELD name=""name"">" + model.name + @"</FIELD>
                            <FIELD name=""enName"">" + model.enName + @"</FIELD>             
                            <FIELD name=""relCType"">I</FIELD>  
                            <FIELD name=""relCinNo"">" + model.appl_id + @"</FIELD>      
                            <FIELD name=""unit"">" + model.unit + @"</FIELD>                                
                            <FIELD name=""EXT_TRANSATION_ID"">" + model.cin_no + @"</FIELD>  
                            <FIELD name=""EXT_QUERY_ID"">" + model.query_id + @"</FIELD>                 
                            <FIELD name=""EXT_EMPLOYEE_NAME"">" + model.query_name + @"</FIELD>      
                            <FIELD name=""EXT_SOURCE_ID"">" + model.source_id + @"</FIELD>                   
                            <FIELD name=""EXT_BRANCH_ID"">" + model.dpt_cd + @"</FIELD>                  
                            <FIELD name=""EXT_BRANCH_NAME"">" + model.dpt_name + @"</FIELD>       
                            <FIELD name=""EXT_EMPLOYEE_MAIL"">NO</FIELD>           
                        </cif>
                        ]]></xml>
                </web:FofExecCif>
         </soapenv:Body >
                  </soapenv:Envelope>";


            logger.Info(strXml);
            soapEnvelopeXml.LoadXml(strXml);

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();

                    XmlDocument xmlRes = new XmlDocument();
                    xmlRes.LoadXml(soapResult);
                    logger.Info(soapResult);

                    XmlNamespaceManager ns = new XmlNamespaceManager(xmlRes.NameTable);
                    ns.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    ns.AddNamespace("ns2", "http://webservice.lyods.com/");

                    XmlNode retrunNode = xmlRes.DocumentElement.SelectSingleNode("//soap:Envelope/soap:Body/ns2:FofExecCifResponse/return", ns);

                    XmlDocument xmlReturn = new XmlDocument();
                    xmlReturn.LoadXml(retrunNode.InnerText.Trim());
                    XmlNode retNode = xmlReturn.DocumentElement.SelectSingleNode("ret");

                    model.status = StringUtil.toString(retNode["DecType"]?.InnerText).ToUpper();
                    model.is_san = StringUtil.toString(retNode["DecState"]?.InnerText).ToUpper();

                    if (retNode == null)
                        model.rtn_code = "1";
                    else
                        model.rtn_code = "0";
                }
            }
        }


        public static HttpWebRequest CreateWebRequest(string lyodsAmlUrl)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(lyodsAmlUrl);
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

    }
}