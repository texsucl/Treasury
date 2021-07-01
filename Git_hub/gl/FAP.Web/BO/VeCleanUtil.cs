using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.EasycomClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;


/// <summary>
/// 功能說明：清理計畫相關
/// 初版作者：
/// 修改歷程：
/// 需求單號：
/// 修改內容：
/// ----------------------------------------------------
/// 修改歷程：20210527 daiyu
/// 需求單號：202103250638-02
/// 修改內容：結案報表加帶經辦人員的姓名(若無對應姓名，以帳號顯示)
///          案件”已覆核”、”覆核中”：以表單申請人為經辦
///          案件未申請覆核：以當下的列印人員為經辦
/// </summary>



namespace FAP.Web.BO
{
    public class VeCleanUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查支票對應的保單
        /// </summary>
        /// <param name="system"></param>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <returns></returns>
        public List<VeCleanModel> qryCheckPolicy(string system, string check_no, string check_acct_short, EacConnection conn400) {
            List<VeCleanModel> policyList = new List<VeCleanModel>();


            if ("A".Equals(system))
            {
                FFAPYCKDao fFAPYCKDao = new FFAPYCKDao();
                policyList = fFAPYCKDao.qryCheckPolicy(conn400, check_acct_short, check_no);
            }
            else {
                FAPPYCKDao fAPPYCKDao = new FAPPYCKDao();
                policyList = fAPPYCKDao.qryCheckPolicy(conn400, check_acct_short, check_no);
            }
                

            return policyList;
        }



        /// <summary>
        /// 結案報表
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="paid_name"></param>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="closed_no"></param>
        /// <param name="gridData"></param>
        /// <param name="UserID"></param>
        /// <param name="UserName"></param>
        /// <param name="closed_desc"></param>
        /// <param name="rptPracList"></param>
        /// <returns></returns>
        public string closedRpt(string paid_id, string paid_name, string level_1, string level_2, string closed_no, string closed_date
            , List<OAP0011Model> gridData, string UserID, string UserName, string closed_desc, List<VeCleanPracModel> rptPracList)
        {
            CommonUtil commonUtil = new CommonUtil();

            //SysCodeDao sysCodeDao = new SysCodeDao();
            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //原給付性質
            //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");
            Dictionary<string, string> oPaidCdMap = fPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", true);


            string aply_id = "";
            UserName = StringUtil.toString(UserName);

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

            #region  取結案報表說明、"經辦" add by daiyu 20200731
            if (!"".Equals(StringUtil.toString(closed_no))) {
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                FAP_VE_TRACE fAP_VE_TRACE = fAPVeTraceDao.qryByCheckNo(gridData[0].check_no, gridData[0].check_acct_short);
                closed_desc = fAP_VE_TRACE.closed_desc;

                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();  //add by daiyu 20210527 取結案報表"經辦"
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<APAplyRecModel> aplyList = fAPAplyRecDao.qryAplyType("C", "", closed_no, db);
                    try {
                        string[] _statArr = new string[] { "1", "2" };
                        APAplyRecModel aply = aplyList.Where(x => _statArr.Contains(x.appr_stat)).OrderByDescending(x => x.aply_no).FirstOrDefault();
                        aply_id = aply.create_id;

                        if (!"".Equals(aply_id)) {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(aply_id);
                            string aply_name = StringUtil.toString(adModel.name);
                            aply_id = aply_name == "" ? aply_id : aply_name;
                        }
                    } catch (Exception e) { 
                    }
                }
            } 
            #endregion end add 20200731


            //清理大類
            Dictionary<string, string> level1Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL1");
            if (level1Map.ContainsKey(level_1))
                level_1 = level1Map[level_1];

            //清理小類
            Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");
            if (level2Map.ContainsKey(level_2))
                level_2 = level2Map[level_2];

            List<VeCleanPracModel> rptProcList = new List<VeCleanPracModel>();

            if (!"".Equals(StringUtil.toString(closed_no)))
            {
                string[] checkArr = gridData.Select(x => x.check_no).ToArray();
                rptProcList = qryPractice("rpt", checkArr, closed_no);    //modify by daiyu 20200803
            }
            else {
                rptProcList = genCodeDesc(rptPracList); 
            }



            foreach (OAP0011Model d in gridData)
            {
                string o_paid_cd = StringUtil.toString(d.o_paid_cd);
                if (oPaidCdMap.ContainsKey(o_paid_cd))
                    d.o_paid_cd = oPaidCdMap[o_paid_cd];

                if ("".Equals(StringUtil.toString(closed_no)))
                    closed_no = d.closed_no;

                try
                {
                    string[] _check_date_arr = d.check_date.Split('/');
                    d.check_date = _check_date_arr[0].PadLeft(3, '0') + "/" + _check_date_arr[1].PadLeft(2, '0') + "/" + _check_date_arr[2].PadLeft(2, '0');
                }
                catch (Exception e) { 
                
                }
            }


            
            DataTable dtMain = commonUtil.ConvertToDataTable<VeCleanPracModel>(rptProcList);

            DataTable dtDetail = commonUtil.ConvertToDataTable<OAP0011Model>(gridData.OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList());


            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\OAP0011P.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", dtDetail));

            //報表參數
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票清理結案報表"));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId",UserID + UserName));


            ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_id", paid_id));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_name", paid_name));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("level_1", level_1));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("level_2", level_2));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("closed_no", closed_no));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("closed_date", closed_date));   //add by daiyu 20201209
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("memo", closed_desc));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("aply_id", aply_id == "" ? (UserName == "" ? UserID : UserName) : aply_id));  //add by daiyu 20210527

            String guid = Guid.NewGuid().ToString();


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "OAP0011P_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }

            writePiaLog(gridData.Count, paid_id, "P", UserID, UserName);

            return guid;

        }


        /// <summary>
        /// 查出踐行程序
        /// </summary>
        /// <param name="checkArr"></param>
        /// <returns></returns>
        public List<VeCleanPracModel> qryPractice(string qType, string[] checkArr, string closed_no) {
            List<VeCleanPracModel> rptProcList = new List<VeCleanPracModel>();

            if("rpt".Equals(qType))
                rptProcList = pracForClosedRpt(closed_no);
            else
                rptProcList = pracForCheckNo(checkArr);
            

            rptProcList = rptProcList.OrderBy(x => Convert.ToDateTime(x.exec_date).AddYears(1911))
                .ThenBy(x => x.practice.Length).ThenBy(x => x.practice).ToList();
            rptProcList = genCodeDesc(rptProcList);

            return rptProcList;
        }


        private List<VeCleanPracModel> pracForClosedRpt(string closed_no)
        {
            rptModel rptModel = new rptModel();
            int i = 0;

            List<VeCleanPracModel> rptProcList = new List<VeCleanPracModel>();

            FAPVeClosedProcDao fAPVeClosedProcDao = new FAPVeClosedProcDao();
            List<FAP_VE_CLOSED_PROC> rowsF = fAPVeClosedProcDao.qryByClosedNo(closed_no);

            string[] rptPrac = new string[] { };

            if (rowsF.Count == 0)  //結案報表的踐行程序來源為"暫存檔"
            {
                FAPVeClosedProcHisDao fAPVeClosedProcHisDao = new FAPVeClosedProcHisDao();
                List<FAP_VE_CLOSED_PROC_HIS> rowsH = fAPVeClosedProcHisDao.qryByClosedNo(closed_no, new string[] { "1" });

                foreach (FAP_VE_CLOSED_PROC_HIS d in rowsH)
                {
                    string key = StringUtil.toString(d.practice)
                    + "|" + StringUtil.toString(d.cert_doc)
                    + "|" + StringUtil.toString(d.exec_date.Year.ToString().PadLeft(4, '0') + d.exec_date.Month.ToString().PadLeft(2, '0') + d.exec_date.Day.ToString().PadLeft(2, '0'))
                    + "|" + StringUtil.toString(d.proc_desc);

                    string seq = "";
                    if (!rptModel.procList.ContainsKey(key))
                    {
                        i++;
                        rptModel.procList.Add(key, new VeCleanPracModel());
                        seq = i.ToString();
                    }
                    else
                        seq = rptModel.procList[key].seq;

                    rptModel.procList[key] = genRptProcMain(rptModel.procList[key], d.practice, d.exec_date, d.cert_doc, d.proc_desc, seq);

                }
            }
            else
            {  //結案報表的踐行程序來源為"正式檔"
                foreach (FAP_VE_CLOSED_PROC d in rowsF)
                {
                    string key = StringUtil.toString(d.practice)
                    + "|" + StringUtil.toString(d.cert_doc)
                    + "|" + StringUtil.toString(d.exec_date.Year.ToString().PadLeft(4, '0') + d.exec_date.Month.ToString().PadLeft(2, '0') + d.exec_date.Day.ToString().PadLeft(2, '0'))
                    + "|" + StringUtil.toString(d.proc_desc);

                    string seq = "";
                    if (!rptModel.procList.ContainsKey(key))
                    {
                        i++;
                        rptModel.procList.Add(key, new VeCleanPracModel());
                        seq = i.ToString();
                    }
                    else
                        seq = rptModel.procList[key].seq;

                    rptModel.procList[key] = genRptProcMain(rptModel.procList[key], d.practice, d.exec_date, d.cert_doc, d.proc_desc, seq);

                }
            }



            foreach (KeyValuePair<string, VeCleanPracModel> item in rptModel.procList)
            {
                rptProcList.Add(item.Value);
            }

            return rptProcList;
        }



        private List<VeCleanPracModel> pracForCheckNo(string[] checkArr) {
            List<VeCleanPracModel> rptProcList = new List<VeCleanPracModel>();

            List<VeCleanPracModel> as400_rptProcList = new List<VeCleanPracModel>();
            //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】的踐行程序一~五
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();


            List<FAP_VE_TRACE> traceMainList = fAPVeTraceDao.qryForOAP0011Rpt(checkArr)
                .GroupBy(d => new { d.proc_desc,
                    d.practice_1,
                    d.exec_date_1,
                    d.cert_doc_1,
                    d.practice_2,
                    d.exec_date_2,
                    d.cert_doc_2,
                    d.practice_3,
                    d.exec_date_3,
                    d.cert_doc_3,
                    d.practice_4,
                    d.exec_date_4,
                    d.cert_doc_4,
                    d.practice_5,
                    d.exec_date_5,
                    d.cert_doc_5
                }).Select(group => new FAP_VE_TRACE
                {
                    proc_desc = group.Key.proc_desc,
                    practice_1 = group.Key.practice_1,
                    exec_date_1 = group.Key.exec_date_1,
                    cert_doc_1 = group.Key.cert_doc_1,
                    practice_2 = group.Key.practice_2,
                    exec_date_2 = group.Key.exec_date_2,
                    cert_doc_2 = group.Key.cert_doc_2,
                    practice_3 = group.Key.practice_3,
                    exec_date_3 = group.Key.exec_date_3,
                    cert_doc_3 = group.Key.cert_doc_3,
                    practice_4 = group.Key.practice_4,
                    exec_date_4 = group.Key.exec_date_4,
                    cert_doc_4 = group.Key.cert_doc_4,
                    practice_5 = group.Key.practice_5,
                    exec_date_5 = group.Key.exec_date_5,
                    cert_doc_5 = group.Key.cert_doc_5
                }).OrderBy(x => x.check_amt).ToList<FAP_VE_TRACE>(); ;



            rptModel rptModel = new rptModel();
            int i = 0;

            

            foreach (FAP_VE_TRACE d in traceMainList)
            {
                if (!"".Equals(StringUtil.toString(d.practice_1)))
                {
                    i++;
                    rptModel.practice1 = genRptProcMain(rptModel.practice1, d.practice_1, d.exec_date_1, d.cert_doc_1, d.proc_desc, i.ToString());
                    as400_rptProcList.Add(rptModel.practice1);
                }

                if (!"".Equals(StringUtil.toString(d.practice_2)))
                {
                    i++;
                    rptModel.practice2 = genRptProcMain(rptModel.practice2, d.practice_2, d.exec_date_2, d.cert_doc_2, d.proc_desc, i.ToString());
                    as400_rptProcList.Add(rptModel.practice2);
                }

                if (!"".Equals(StringUtil.toString(d.practice_3)))
                {
                    i++;
                    rptModel.practice3 = genRptProcMain(rptModel.practice3, d.practice_3, d.exec_date_3, d.cert_doc_3, d.proc_desc, i.ToString());
                    as400_rptProcList.Add(rptModel.practice3);
                }

                if (!"".Equals(StringUtil.toString(d.practice_4)))
                {
                    i++;
                    rptModel.practice4 = genRptProcMain(rptModel.practice4, d.practice_4, d.exec_date_4, d.cert_doc_4, d.proc_desc, i.ToString());
                    as400_rptProcList.Add(rptModel.practice4);
                }

                if (!"".Equals(StringUtil.toString(d.practice_5)))
                {
                    i++;
                    rptModel.practice5 = genRptProcMain(rptModel.practice5, d.practice_5, d.exec_date_5, d.cert_doc_5, d.proc_desc, i.ToString());
                    as400_rptProcList.Add(rptModel.practice5);
                }

            }

            //modify by daiyu 20201209 修改同一給付對象ID有多張票..同一踐行程序重複顯示問題
            i = 0;
            string _practice = "";
            string _cert_doc = "";
            string _exec_date = "";
            foreach (VeCleanPracModel d in as400_rptProcList.OrderBy(x => x.practice).ThenBy(x => x.cert_doc).ThenBy(x => x.exec_date)) {
                if (!_practice.Equals(StringUtil.toString(d.practice))
                    || !_cert_doc.Equals(StringUtil.toString(d.cert_doc))
                    || !_exec_date.Equals(StringUtil.toString(d.exec_date))) {
                    i++;

                    d.seq = StringUtil.transToChtNumber(i, true);
                    rptProcList.Add(d);

                    _practice = d.practice;
                    _cert_doc = d.cert_doc;
                    _exec_date = d.exec_date;
                }
            }


            //查詢【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】的踐行程序
            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
            List<FAP_VE_TRACE_PROC> procList = fAPVeTrackProcDao.qryByCheckNoList(checkArr);

            foreach (FAP_VE_TRACE_PROC d in procList)
            {
                //modify by daiyu 20190911 非AS400的踐行程序需每一次都列...
                string key = StringUtil.toString(d.practice)
                    + "|" + StringUtil.toString(d.cert_doc)
                    + "|" + StringUtil.toString(d.exec_date.Year.ToString().PadLeft(4, '0') + d.exec_date.Month.ToString().PadLeft(2, '0') + d.exec_date.Day.ToString().PadLeft(2, '0'))
                    + "|" + StringUtil.toString(d.proc_desc);

                string seq = "";
                if (!rptModel.procList.ContainsKey(key))
                {
                    i++;
                    rptModel.procList.Add(key, new VeCleanPracModel());
                    seq = i.ToString();
                }
                else
                    seq = rptModel.procList[key].seq;


                rptModel.procList[key] = genRptProcMain(rptModel.procList[key], d.practice, d.exec_date, d.cert_doc, d.proc_desc, seq);
            }

            foreach (KeyValuePair<string, VeCleanPracModel> item in rptModel.procList)
            {
                rptProcList.Add(item.Value);
            }

            return rptProcList;
        }




        /// <summary>
        /// 取得特定支票的最後執行日期
        /// </summary>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <returns></returns>
        public DateTime? qryMaxExecDate(string check_no, string check_acct_short)
        {
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

            FAP_VE_TRACE d = fAPVeTraceDao.qryByCheckNo(check_no, check_acct_short);
            var exec_date = d.exec_date_1;

            if (d.exec_date_2 != null)
                exec_date = exec_date < d.exec_date_2 ? d.exec_date_2 : exec_date;

            if (d.exec_date_3 != null)
                exec_date = exec_date < d.exec_date_3 ? d.exec_date_3 : exec_date;

            if (d.exec_date_4 != null)
                exec_date = exec_date < d.exec_date_4 ? d.exec_date_4 : exec_date;

            if (d.exec_date_5 != null)
                exec_date = exec_date < d.exec_date_5 ? d.exec_date_5 : exec_date;

            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
            List<OAP0010DModel> procList = fAPVeTrackProcDao.qryByCheckNo(check_no, check_acct_short);
            if (procList.Count > 0)
            {
                var procMaxDate = procList.Max(x => Convert.ToDateTime(x.exec_date).AddYears(1911));
                if (procMaxDate != null)
                {
                    if (exec_date == null)
                        exec_date = procMaxDate;
                    else
                        exec_date = exec_date < procMaxDate ? procMaxDate : exec_date;
                }

            }


            return exec_date;
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType, string UserID, string UserName)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = UserID;
            piaLogMain.ACCOUNT_NAME = UserName;
            piaLogMain.PROGFUN_NAME = "VeCleanUtil";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        private VeCleanPracModel genRptProcMain(VeCleanPracModel model, string practice, DateTime? exec_date, string cert_doc, string proc_desc, string seq)
        {
            bool bDup = false;
            try
            {
                if (new ValidateUtil().IsNum(seq))
                    model.seq = StringUtil.transToChtNumber(Convert.ToInt16(seq), true);

                if (exec_date != null)
                {
                    string strDate = exec_date.Value.Year - 1911 + "/" + exec_date.Value.Month + "/" + exec_date.Value.Day;
                    if (model.exec_date == null)
                        bDup = true;
                    else if (strDate.CompareTo(model.exec_date) > 0)
                        bDup = true;


                    if (bDup)
                    {

                        model.practice = practice;
                        model.cert_doc = cert_doc;
                        model.exec_date = strDate;
                        model.proc_desc = proc_desc;
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }


            return model;
        }

        private List<VeCleanPracModel> genCodeDesc(List<VeCleanPracModel> rptProcList)
        {

            List<VeCleanPracModel> order_list = new List<VeCleanPracModel>();

            if (rptProcList == null)
                return order_list;

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //踐行程序
            Dictionary<string, string> practiceMap = fAPVeCodeDao.qryByTypeDic("CLR_PRACTICE");

            //証明文件
            Dictionary<string, string> certDacMap = fAPVeCodeDao.qryByTypeDic("CLR_CERT_DOC");

            int i = 0;
            foreach (VeCleanPracModel d in rptProcList.OrderBy(x => DateUtil.ChtDateToADDate(x.exec_date, '/'))
                .ThenBy(x => x.practice.Length).ThenBy(x => x.practice).ToList())
            {
                i++;
                d.seq = StringUtil.transToChtNumber(i, true);

                if (practiceMap.ContainsKey(d.practice))
                    d.practice_desc = practiceMap[d.practice];
                else
                    d.practice_desc = d.practice;

                if (certDacMap.ContainsKey(d.cert_doc))
                    d.cert_doc_desc = certDacMap[d.cert_doc];
                else
                    d.cert_doc_desc = d.cert_doc;


                order_list.Add(d);  //modify by daiyu 20210201
            }


            return order_list;   //modify by daiyu 20210201
        }


        internal class rptModel
        {

            public VeCleanPracModel practice1 { get; set; }
            public VeCleanPracModel practice2 { get; set; }
            public VeCleanPracModel practice3 { get; set; }
            public VeCleanPracModel practice4 { get; set; }
            public VeCleanPracModel practice5 { get; set; }

            public Dictionary<string, VeCleanPracModel> procList = new Dictionary<string, VeCleanPracModel>();

            public rptModel()
            {

                practice1 = new VeCleanPracModel();
                practice2 = new VeCleanPracModel();
                practice3 = new VeCleanPracModel();
                practice4 = new VeCleanPracModel();
                practice5 = new VeCleanPracModel();
            }
        }
       

    }
}