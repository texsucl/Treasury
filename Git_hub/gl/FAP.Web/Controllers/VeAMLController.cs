using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Web.Http;
using System;
using System.Threading.Tasks;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using System.Data;
using Microsoft.Reporting.WebForms;

namespace FAP.Web.Controllers
{
    [RoutePrefix("VeAML")]
    public class VeAMLController : ApiController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 逾期未兌領AML報表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("AMLRpt")]
        [ValidateModel]
        public IHttpActionResult AMLRpt(AMLRptModel model)
        {
            string inputKey = model.type + "|" + model.exec_date + "|" + model.upd_id + "|" + model.upd_date;
            logger.Info("type:" + model.type);
            logger.Info("exec_date:" + model.exec_date);
            logger.Info("upd_id:" + model.upd_id);
            logger.Info("upd_date:" + model.upd_date);


            switch (StringUtil.toString(model.type))
            {
                case "BAP7001":
                case "BAP7029":
                    break;
                default:
                    model.rtnCode = "F";
                    logger.Error(inputKey + "==>類別輸入錯誤");
                    break;
            }


            if ("".Equals(model.exec_date) || "".Equals(model.upd_id) || "".Equals(model.upd_date))
            {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>類別輸入錯誤");
            }



            if (!"F".Equals(model.rtnCode))
                Task.Run(() => procAMLRpt(model));

            model.rtnCode = "S";


            logger.Info("inputKey:" + inputKey + "==> rtnCode:" + model.rtnCode);

            return Ok(model);
        }

        


        /// <summary>
        /// 處理AML報表的主程式段
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task procAMLRpt(AMLRptModel model)
        {
            await Task.Delay(1);


            try
            {
                //查詢出異動人員
                string user_id = "";
                string user_mail = "";
                string user_name = "";

                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    OaEmpDao OaEmpDao = new OaEmpDao();
                    V_EMPLY2 usr = OaEmpDao.qryByUsrId(model.upd_id, db);
                    if (usr != null)
                    {
                        user_id = StringUtil.toString(usr.USR_ID);
                        user_mail = StringUtil.toString(usr.EMAIL);
                        user_name = StringUtil.toString(usr.EMP_NAME);
                    }
                }


                //取得AS400的資料
                int dataCnt = qryAMLList(model.type, model.exec_date, model.upd_id, user_name);
                logger.Info("dataList count:" + dataCnt);


                //寫稽核軌跡
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.EXECUTION_TYPE = "I";
                piaLogMain.ACCESS_ACCOUNT = user_id;
                piaLogMain.ACCOUNT_NAME = user_name;
                piaLogMain.AFFECT_ROWS = dataCnt;
                piaLogMain.EXECUTION_CONTENT = model.type + "|" + model.exec_date + "|" + model.upd_id;
                writePiaLog(piaLogMain);


            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }
        }


        /// <summary>
        /// 執行"OAP0001 上傳應付未付主檔/明細檔作業"若為制裁名單，需產生"逾期未兌領支票-疑似禁制名單"
        /// </summary>
        /// <param name="amlList"></param>
        /// <param name="upd_id"></param>
        /// <param name="user_name"></param>
        public void fromOAP0001(List<OAP0001FileModel> amlList, string upd_id, string user_name) {
            List<VeAMLRptModel> dataListP1 = new List<VeAMLRptModel>();
            FMNPPAADao fMNPPAADao = new FMNPPAADao();

            //AML狀態結果
            SysCodeDao sysCodeDao = new SysCodeDao();
            Dictionary<string, string> amlStatusMap = sysCodeDao.qryByTypeDic("AP", "AML_STATUS");

            foreach (OAP0001FileModel aml in amlList) {
                VeAMLRptModel d = new VeAMLRptModel();
                d.check_no = aml.checkNo;
                d.check_acct_short = aml.checkShrt;
                d.paid_id = MaskUtil.maskId(aml.paidId);
                d.paid_name = MaskUtil.maskName(aml.paidName);
                d.policy_no = MaskUtil.maskPolicyNo(aml.policyNo);
                d.policy_seq = aml.policySeq;
                d.id_dup = aml.idDup;
                d.check_amt = aml.checkAmt;
                d.system = aml.system;
                d.data_flag = aml.dataFlag;

                string status = StringUtil.toString(d.data_flag);
                if (amlStatusMap.ContainsKey(status))
                    d.aml_desc = amlStatusMap[status];

                dataListP1.Add(d);
            }

            if (dataListP1.Count() > 0) {
                string guid = Guid.NewGuid().ToString();
                string[] fileList = new string[] { };

                procVeAMLP1(dataListP1.OrderBy(x => x.system).ThenBy(x => x.paid_id).ToList(), upd_id, user_name, guid);
                fileList = new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + "VeAMLP1_" + guid + ".pdf" };

                procSendMail(fileList, "疑似禁制名單");

                //寫稽核軌跡
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.EXECUTION_TYPE = "I";
                piaLogMain.ACCESS_ACCOUNT = upd_id;
                piaLogMain.ACCOUNT_NAME = user_name;
                piaLogMain.AFFECT_ROWS = dataListP1.Count();
                piaLogMain.EXECUTION_CONTENT = ("OAP0001" + "|" + upd_id);
                writePiaLog(piaLogMain);
            }

        }


        /// <summary>
        /// 查詢需出表的資料
        /// </summary>
        /// <param name="type"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <param name="user_name"></param>
        /// <returns></returns>
        private int qryAMLList(string type, string exec_date, string upd_id, string user_name)
        {
            

            int dataCnt = 0;

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();
                string guid = Guid.NewGuid().ToString();
                string[] fileList = new string[] { } ;

                switch (type) {
                    
                    case "BAP7001": //逾期未兌領支票-疑似禁制名單
                        List<VeAMLRptModel> dataListP1 = new List<VeAMLRptModel>();
                        FMNPPAADao fMNPPAADao = new FMNPPAADao();
                        dataListP1 = fMNPPAADao.qryForBAP7001(conn400, exec_date, upd_id, type);


                        //AML狀態結果
                        SysCodeDao sysCodeDao = new SysCodeDao();
                        Dictionary<string, string> amlStatusMap = sysCodeDao.qryByTypeDic("AP", "AML_STATUS");

                        foreach (VeAMLRptModel d in dataListP1)
                        {
                            d.paid_id = MaskUtil.maskId(d.paid_id);
                            d.paid_name = MaskUtil.maskName(d.paid_name);
                            d.policy_no = MaskUtil.maskPolicyNo(d.policy_no);

                            string status = StringUtil.toString(d.data_flag);
                            if (amlStatusMap.ContainsKey(status))
                                d.aml_desc = amlStatusMap[status];
                        }

                        dataCnt = dataListP1.Count();
                        procVeAMLP1(dataListP1, upd_id, user_name, guid);

                        fileList = new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + "VeAMLP1_" + guid + ".pdf"};

                        procSendMail(fileList, "疑似制裁名單");
                        break;
                    
                    case "BAP7029": //逾期未兌領支票-禁制名單_名單刪除通知
                        

                        List<VeAMLRptModel> dataListP2 = new List<VeAMLRptModel>();
                        List<VeAMLRptModel> dataListP3 = new List<VeAMLRptModel>();

                        FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();
                        dataListP2 = fAPPPAWDao.qryForBAP7029P1(conn400, exec_date, upd_id);
                        logger.Info("dataListP2 cnt:" + dataListP2.Count());

                        foreach (VeAMLRptModel d in dataListP2)
                        {
                            d.paid_id = MaskUtil.maskId(d.paid_id);
                            d.paid_name = MaskUtil.maskName(d.paid_name);
                            d.policy_no = MaskUtil.maskPolicyNo(d.policy_no);

                        }


                        procVeAMLP2(dataListP2, upd_id, user_name, guid);

                        dataListP3 = fAPPPAWDao.qryForBAP7029P2(conn400, exec_date, upd_id);
                        logger.Info("dataListP3 cnt:" + dataListP3.Count());

                        foreach (VeAMLRptModel d in dataListP3)
                        {
                            d.paid_id = MaskUtil.maskId(d.paid_id);
                            d.paid_name = MaskUtil.maskName(d.paid_name);
                            d.policy_no = MaskUtil.maskPolicyNo(d.policy_no);
                            d.r_bank_act = MaskUtil.maskBankAct(d.r_bank_act);
                            d.c_paid_nm = MaskUtil.maskName(d.c_paid_nm);
                        }
                        procVeAMLP3(dataListP3, upd_id, user_name, guid);

                        fileList = new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + "VeAMLP2_" + guid + ".pdf"
                                                    ,Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + "VeAMLP3_" + guid + ".pdf"};
                        procSendMail(fileList, "名單刪除通知");
                        break;

                }

                

                return dataCnt;

            }
        }


        /// <summary>
        /// 將報表MAIL給相關人員
        /// </summary>
        private void procSendMail(string[] fileList, string logKey) {
            //寄送mail
            MailUtil mailUtil = new MailUtil();
            List<UserBossModel> notify = mailUtil.getMailGrpId("VE_AML_RPT");
            string mailContent = "";

            string[] mailToArr = notify.Select(x => x.empMail).ToList().ToArray();
            bool bSucess = mailUtil.sendMailMultiRTLog(notify
                , "逾期未兌領-AML報表通知"
                , mailContent
                , true
               , ""
               , ""
               , fileList 
               , true, true, logKey);
        }

        /// <summary>
        /// 逾期未兌領支票-疑似禁制名單
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="upd_id"></param>
        /// <param name="user_name"></param>
        private void procVeAMLP1(List<VeAMLRptModel> dataList, string upd_id, string user_name, string guid) {

            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<VeAMLRptModel>(dataList);

            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\VeAMLP1.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));

            //報表參數
           // ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票-疑似禁制名單"));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", user_name));


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "VeAMLP1_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }


        }


        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知
        /// 通知類別=印信函
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="upd_id"></param>
        /// <param name="user_name"></param>
        private void procVeAMLP2(List<VeAMLRptModel> dataList, string upd_id, string user_name, string guid)
        {

            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<VeAMLRptModel>(dataList);

            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\VeAMLP2.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));

            //報表參數
            // ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票-疑似禁制名單"));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", user_name));


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "VeAMLP2_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }
        }



        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知
        /// 通知類別=可重新給付-D
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="upd_id"></param>
        /// <param name="user_name"></param>
        private void procVeAMLP3(List<VeAMLRptModel> dataList, string upd_id, string user_name, string guid)
        {

            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<VeAMLRptModel>(dataList);

            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\VeAMLP3.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));

            //報表參數
            // ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票-疑似禁制名單"));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", user_name));


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "VeAMLP3_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }
        }




        private void writePiaLog(PIA_LOG_MAIN piaLogMain)
        {
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.PROGFUN_NAME = "VeAMLController";
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.ACCESSOBJ_NAME = "FMNPPAA0";
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }



        public partial class rptModel
        {

            public string err_msg { get; set; }

            public string paid_id { get; set; }

            public string check_no { get; set; }

            public string check_acct_short { get; set; }

            public string check_date { get; set; }

            public string check_amt { get; set; }
            public string closed_date { get; set; }
            public string closed_no { get; set; }

            
        }

        

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class AMLRptModel
        {
            /// <summary>
            ///type
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 執行日期
            /// </summary>
            public string exec_date { get; set; }

            /// <summary>
            /// 異動人員
            /// </summary>
            public string upd_id { get; set; }

            /// <summary>
            /// 異動日期
            /// </summary>
            public string upd_date { get; set; }

            /// <summary>
            /// S:成功;F失敗
            /// </summary>
            public string rtnCode { get; set; }


        }


    }



}
