using FGL.Web.AS400Models;
using FGL.Web.AS400PGM;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using Microsoft.Reporting.WebForms;
using System.IO;

/// <summary>
/// 功能說明：給付優化-未接收報表
/// 初版作者：20190506 黃黛鈺
/// 修改歷程：20190506 黃黛鈺
/// 需求單號：
///           初版
/// ------------------------------------
/// 修改歷程：20191114 黃黛鈺
/// 需求單號：201910240537
/// 修改內容：1.若該次排程啟動時，沒有符合條件的資料，亦要產生空白報表MAIL給相關人員。
/// 　　　　　2.非工作天時，不用產出報表寄送。
/// </summary>
/// 

namespace FGL.WebScheduler
{
    public class BGL0009Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]begin!!");

            //先從參數檔內找是否已到了排程的時間
            SysParaDao sysParaDao = new SysParaDao();
            List<SYS_PARA> paraList = sysParaDao.qryByGrpId("GL", "BGL0009Job");

            
            DateTime end_date = StringUtil.toString(paraList.Where(x => x.RESERVE2 == "end_date").FirstOrDefault().PARA_VALUE) == "" ? DateTime.Now : Convert.ToDateTime(paraList.Where(x => x.RESERVE2 == "end_date").FirstOrDefault().PARA_VALUE);
            logger.Info("end_date:" + end_date);

            string caseMI = StringUtil.toString(paraList.Where(x => x.PARA_ID == "unitMI").FirstOrDefault().PARA_VALUE);
            string caseOther = StringUtil.toString(paraList.Where(x => x.PARA_ID == "unitOther").FirstOrDefault().PARA_VALUE);

            logger.Info("caseMI:" + caseMI);
            logger.Info("caseOther:" + caseOther);

            /*----test----*/
            //List<BGL00009Model> rows1 = qryRptData(end_date, caseMI, caseOther);

            //foreach (BGL00009Model d in rows1.GroupBy(o => new { o.SEND_GRP }).Select(group => new BGL00009Model { SEND_GRP = group.Key.SEND_GRP }).ToList<BGL00009Model>())
            //{
            //    string fileName = genRpt(rows1.Where(w => w.SEND_GRP == d.SEND_GRP).ToList());   //產報表
            //}
            /*----test----*/

            bool bExec = false;

           

            string strJobTime = "";
            foreach (SYS_PARA d in paraList.Where(x => x.RESERVE2 == "exec_time").ToList()) {
                string[] paraTime = d.PARA_VALUE.Split(':');

                if(paraTime.Length == 2)
                {
                    DateTime dtJobTime = DateTime.Now;
                    TimeSpan ts = new TimeSpan(Convert.ToInt16(paraTime[0]), Convert.ToInt16(paraTime[1]), 0);
                    dtJobTime = dtJobTime.Date + ts;

                    TimeSpan diff = DateTime.Now - dtJobTime;
                    if (0 <= diff.TotalSeconds & diff.TotalSeconds <= Convert.ToInt64(d.RESERVE1)) {
                        bExec = true;
                        strJobTime = d.PARA_VALUE;
                        break;
                    }
                }
            }


            //若未到排程時間，直接結束
            if (!bExec)
            {
                return;
            }
            else
                bExec = false;


            /*---add by daiyu 20191114 判斷是否為假日---*/
            FGLCALEDao fGLCALEDao = new FGLCALEDao();
            var isREST = fGLCALEDao.isREST();
            if ("Y".Equals(isREST)) {
                logger.Info("假日不執行!!");
                return;
            }
                
            /*---end add 20191114---*/


            logger.Info("[Execute]執行開始!!");

            FGLScheduleJobDao fGLScheduleJobDao = new FGLScheduleJobDao();
            FGL_SCHEDULE_JOB job = new FGL_SCHEDULE_JOB();
            job = fGLScheduleJobDao.qryByName("BGL0009Job");

            //System.Threading.Thread.Sleep(15000);

            String guid = strJobTime + "-" + Guid.NewGuid().ToString();

            int updCnt = 0;

            if (job != null)
            {
                if (job.start_exe_time == null)
                {
                    job.start_exe_time = DateTime.Now;
                    job.end_exe_time = null;
                    updCnt = fGLScheduleJobDao.updateByName(job, guid);
                }
                else
                {
                    TimeSpan diff = DateTime.Now - (DateTime)job.start_exe_time;
                    if (!job.remark.StartsWith(strJobTime) || (job.remark.StartsWith(strJobTime) & diff.TotalMinutes > Convert.ToInt64(job.scan_timec)))
                    {
                        job.start_exe_time = DateTime.Now;
                        job.end_exe_time = null;
                        updCnt = fGLScheduleJobDao.updateByName(job, guid);
                    }
                }
            }
            else {
                job = new FGL_SCHEDULE_JOB();
                job.sched_name = "BGL0009Job";
                job.remark = guid;
                job.start_exe_time = DateTime.Now;
                job.end_exe_time = null;
                job.scan_timec = "10";
                updCnt = fGLScheduleJobDao.insert(job);

            }

            if (updCnt > 0)
                bExec = true;

            //主程式(查詢未接收資料)
            if (bExec)
            {
                try
                {
                    logger.Info("[Execute]主程式段!!");

                    
                    List<BGL00009Model> rows = qryRptData(end_date, caseMI, caseOther);

                    logger.Info("rows:" + rows);

                    //modify by daiyu 20191114
                    string fileNameMI = genRpt(rows.Where(w => w.SEND_GRP == caseMI).ToList(), caseMI);   //產報表
                    string fileNameOther = genRpt(rows.Where(w => w.SEND_GRP == caseOther).ToList(), caseOther);   //產報表


                    //if (rows.Count == 0)
                    //    logger.Info("[Execute]無未接收資料!!");
                    //else
                    //{

                    //    logger.Info("caseMI:" + caseMI);
                    //    logger.Info("caseOther:" + caseOther);

                    //    foreach (BGL00009Model d in rows.GroupBy(o => new { o.SEND_GRP }).Select(group => new BGL00009Model { SEND_GRP = group.Key.SEND_GRP }).ToList<BGL00009Model>())
                    //    {
                    //        string fileName = genRpt(rows.Where(w => w.SEND_GRP == d.SEND_GRP).ToList());   //產報表
                    //    }
                    //}
                }
                catch (Exception e) {
                    logger.Error(e.ToString());
                }
                
                    

                job.end_exe_time = DateTime.Now;
                job.remark = guid;
                updCnt = fGLScheduleJobDao.updateByName(job, guid);
            }
            else {
                logger.Info("[Execute]未執行(本次排程已有JOB執行)!!");
            }

            logger.Info("[Execute]執行結束!!");
        }


        private void sendMail(string fileName, string sendGrp)
        {
            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                FGLSENDDao fGLSENDDao = new FGLSENDDao();
                List<FGLSEND0Model> sendList = fGLSENDDao.qryByPgmDep("RRT9500A", sendGrp, conn400);

                if (sendList.Count == 0)
                {
                    logger.Info("查無對應的寄送人員!!");

                }
                else {
                    MailUtil mailUtil = new MailUtil();
                    Dictionary<string, V_EMPLY2> mailList = new Dictionary<string, V_EMPLY2>();

                    EacCommand cmd = new EacCommand();
                    cmd.Connection = conn400;

                    OaEmpDao oaEmpDao = new OaEmpDao();

                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        foreach (FGLSEND0Model d in sendList)
                        {
                            mailList = mailUtil.qryMailListByUserId(d.sendId, mailList, conn400, cmd, oaEmpDao, dbIntra);
                        }
                    }
              
                    string mailContent = "給付介面自動傳送＿批次未接收報表";
                    string[] filePaths = new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName };


                    bool bSucess = mailUtil.sendMailMulti(mailList.Values.ToList().Select(x => x.EMAIL).ToArray()
                        , "給付介面自動傳送＿批次未接收報表"
                        , mailContent
                        , true
                       , ""
                       , ""
                       , filePaths
                       , true);
                }
            }
        }

        /// <summary>
        /// 產生報表
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private string genRpt(List<BGL00009Model> rows, string send_grp) {
            string fileName = "";

            try
            {

                //add by diayu 20191114
                if (rows.Count == 0) {
                    BGL00009Model model = new BGL00009Model();
                    model.RESOURCE_DESC = "無資料";
                    model.AMT = null;

                    rows.Add(model);
                }

                CommonUtil commonUtil = new CommonUtil();
                DataTable dtMain = commonUtil.ConvertToDataTable<BGL00009Model>(rows);
                var ReportViewer1 = new ReportViewer();

                //清除資料來源
                ReportViewer1.LocalReport.DataSources.Clear();
                //指定報表檔路徑   
                ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\BGL00009P.rdlc");
                //設定資料來源
                ReportViewer1.LocalReport.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dtMain));
                
                //報表參數
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "給付介面自動傳送＿批次未接收報表"));

               // ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(subReportProcessing);

                ReportViewer1.LocalReport.Refresh();

                Microsoft.Reporting.WebForms.Warning[] tWarnings;
                string[] tStreamids;
                string tMimeType;
                string tEncoding;
                string tExtension;
                fileName = "BGL00009P_" + DateUtil.getCurDateTime("") + ".pdf";
                //呼叫ReportViewer.LoadReport的Render function，將資料轉成想要轉換的格式，並產生成Byte資料
                byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);

                using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
                {
                    fs.Write(tBytes, 0, tBytes.Length);
                }

                sendMail(fileName, send_grp);

                return fileName;
            }
            catch (Exception e) {
                logger.Error(e.ToString());

                return "";
            }
           

        }




        /// <summary>
        /// 查詢符合出表條件的資料
        /// </summary>
        /// <returns></returns>
        private List<BGL00009Model> qryRptData(DateTime end_date, string caseMI, string caseOther)
        {
            List<BGL00009Model> rows = new List<BGL00009Model>();
            //DateTime dt = DateTime.Now;

            try
            {
                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();

                    FPMCODEDao code = new FPMCODEDao();

                    #region 查詢符合出表條件的資料
                    //FRTCOPH0 給付主檔
                    FRTCOPHDao coph = new FRTCOPHDao();
                    rows = coph.qryForBGL0009(rows, conn400, end_date);

                    //FAPNBAS0 應付溢繳退費檔
                    FAPNBASDao nbas = new FAPNBASDao();
                    rows = nbas.qryForBGL0009(rows, conn400, end_date);

                    //FAPMCRF0 應付變更退費檔
                    FAPMCRFDao mcrf = new FAPMCRFDao();
                    rows = mcrf.qryForBGL0009(rows, conn400, end_date);

                    //FRTRMRE0 繳款資料明細檔 (繳款單)
                    List<FPMCODEModel> codeCk = code.qryFPMCODE("AUTO_CK", "", "", conn400);
                    FRTRMREDao rmre = new FRTRMREDao();
                    rows = rmre.qryForBGL0009(rows, codeCk, conn400, end_date);

                    //FFAFAPH 人工申請主檔 (人工CR)
                    List<FPMCODEModel> codeCr = code.qryFPMCODE("AUTO_CR", "", "", conn400);
                    FFAFAPHDao faph = new FFAFAPHDao();
                    rows = faph.qryForBGL0009(rows, codeCr, conn400, end_date);
                    #endregion


                    #region 查詢"FGLVRID 新增人員設定檔 "取得承辦人員
                    SRTB0010Util srtb0010 = new SRTB0010Util();
                    Dictionary<string, SRTB0010Model> userMap = new Dictionary<string, SRTB0010Model>();

                    EacCommand cmd = new EacCommand();
                    cmd.Connection = conn400;

                    //取得登打人員姓名
                    foreach (BGL00009Model d in rows) {
                        if (!userMap.ContainsKey(d.ENTRY_ID))
                            userMap = srtb0010.callSRTB0010(conn400, cmd, d.ENTRY_ID, userMap);

                        d.ENTRY_NAME = StringUtil.toString(userMap[d.ENTRY_ID].empName);
                        d.SEND_GRP = d.RESOURCE.StartsWith("MI") ? caseMI : caseOther;
                    }


                    //取得各"系統別+來源別"對應的承辦人員
                    FGLVRIDDao vrid = new FGLVRIDDao();
                    List<FGLVRID0Model> procIdList = vrid.qryByKey("A06", "", "", conn400);


                    //取得承辦人員姓名
                    foreach (FGLVRID0Model d in procIdList.GroupBy(o => new { o.entryId }).Select(group => new FGLVRID0Model { entryId = group.Key.entryId }).ToList<FGLVRID0Model>())
                    {
                        if (!userMap.ContainsKey(d.entryId))
                            userMap = srtb0010.callSRTB0010(conn400, cmd, d.entryId, userMap);

                        string empName = StringUtil.toString(userMap[d.entryId].empName);
                        if ("".Equals(empName))
                            procIdList.Where(w => w.entryId == d.entryId).ToList().ForEach(i => i.entryName = d.entryId);
                        else
                            procIdList.Where(w => w.entryId == d.entryId).ToList().ForEach(i => i.entryName = empName);

                    }


                    foreach (FGLVRID0Model d in procIdList)
                    {
                        rows.Where(w => w.SYS_TYPE == d.sysType & w.RESOURCE == d.srceFrom).ToList().ForEach(i => i.PROC_ID = d.entryId);
                        rows.Where(w => w.SYS_TYPE == d.sysType & w.RESOURCE == d.srceFrom).ToList().ForEach(i => i.PROC_NAME = d.entryName);
                    }

                    cmd.Dispose();
                    cmd = null;
                    #endregion


                }
            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }


            


            return rows;
        }


        public class rptModel
        {
            public string system { get; set; }

            public string check_no { get; set; }

            public string check_shrt { get; set; }

            public string report_no { get; set; }

            public string create_id { get; set; }

            public string create_dt { get; set; }
        }
    }
}