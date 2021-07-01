using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Transactions;

/// <summary>
/// 功能說明：AML D檔批次排程
/// 初版作者：20191127 黃黛鈺
/// 修改歷程：20191127 黃黛鈺 
/// 需求單號：201910290100-01
///           初版
/// </summary>
/// 

namespace FAP.WebScheduler
{
    public class AMLDFileJob : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");

            bool bExec = false;
            FAPScheduleJobDao fAPScheduleJobDao = new FAPScheduleJobDao();
            FAP_SCHEDULE_JOB job = new FAP_SCHEDULE_JOB();
            job = fAPScheduleJobDao.qryByName("AMLDFileJob");

            //System.Threading.Thread.Sleep(15000);

            String guid = Guid.NewGuid().ToString();

            int updCnt = 0;

            if (job != null)
            {
                if (job.start_exe_time == null)
                {
                    job.start_exe_time = DateTime.Now;
                    job.end_exe_time = null;
                    updCnt = fAPScheduleJobDao.updateByName(job, guid);
                }
                else
                {
                    TimeSpan diff = DateTime.Now - (DateTime)job.start_exe_time;
                    if (diff.TotalMinutes > Convert.ToInt64(job.scan_timec))
                    {
                        job.start_exe_time = DateTime.Now;
                        job.end_exe_time = null;
                        updCnt = fAPScheduleJobDao.updateByName(job, guid);
                    }
                }
            }
            else {
                job = new FAP_SCHEDULE_JOB();
                job.sched_name = "AMLDFileJob";
                job.remark = guid;
                job.start_exe_time = DateTime.Now;
                job.end_exe_time = null;
                job.scan_timec = "10";
                updCnt = fAPScheduleJobDao.insert(job);

            }

            if (updCnt > 0)
                bExec = true;

            //主程式
            if (bExec)
            {
                procDFile("VE30001", guid);
                procDFile("VE300", guid);


                job.end_exe_time = DateTime.Now;
                job.remark = guid;
                updCnt = fAPScheduleJobDao.updateByName(job, guid);

            }
            else {
                logger.Info("[Execute]未執行!!");
            }

            logger.Info("[Execute]執行結束!!");
        }



        private void procDFile(string unit, string guid) {
            logger.Info(unit + "procDFile begin");

            List<AMLDFileModel> dataList = new List<AMLDFileModel>();

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                try
                {
                    FIVJAMLDDao fIVJAMLDDao = new FIVJAMLDDao();
                    dataList = fIVJAMLDDao.qryForRenew(conn400, unit);

                    if (dataList.Count > 0)
                    {
                        foreach (AMLDFileModel d in dataList) {
                            d.cin_no = MaskUtil.maskAmlCinNo(d.cin_no);
                            d.paid_name = MaskUtil.maskName(d.paid_name);
                        }

                        string[] mailToArr = genRpt(dataList, guid + "_" + unit, unit);
                        writePiaLog(dataList, mailToArr);
                        fIVJAMLDDao.updReadMk(dataList, conn400);
                    }
                    else
                        logger.Info(unit + "[Execute]無資料!!");
                }
                catch (Exception e)
                {

                    logger.Error("unit + [execReviewR]其它錯誤：" + e.ToString());
                }
            }

        }



        private string[] genRpt(List<AMLDFileModel> dataList, string guid, string unit) {

            //寄送mail
            MailUtil mailUtil = new MailUtil();
            string mailGrp = "";
            string mailContent = "";
            string logKey = "";
            if ("VE30001".Equals(unit))
            {
                logKey = "逾期未兌領-AML疑似制裁名單異動通知";
                mailGrp = "VE_AML_RPT";
            }

            else
            {
                logKey = "繳款單-AML疑似黑名單異動需重新判定通知";
                mailGrp = "VE_AML_6210";
                mailContent = "AML疑似黑名單異動需重新判定通知";
            }


            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<AMLDFileModel>(dataList);

            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\AMLDFileP.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));

            //報表參數
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("RptTitle", logKey));
  


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "AMLDFile_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }

            string[] fileList = new string[] { };
            fileList = new string[] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName};


          
                

            List<UserBossModel> notify = mailUtil.getMailGrpId(mailGrp);
            

            string[] mailToArr = notify.Select(x => x.empMail).ToList().ToArray();
            string[] usrArr = notify.Select(x => x.usrId).ToList().ToArray();

            try
            {
                bool bSucess = mailUtil.sendMailMultiRTLog(notify
                , logKey
                , mailContent
                , true
               , ""
               , ""
               , fileList
               , true
               , true
               , logKey);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }
            

            return usrArr;
        }


        private void writePiaLog(List<AMLDFileModel> dataList, string[] mailTo)
        {
            string accessAccount = "";
            foreach (string mail in mailTo) {
                accessAccount += mail.Split('@')[0] + "|";
            }

            if (accessAccount.TrimEnd().Length > 10)
                accessAccount = accessAccount.Substring(0, 10);

            foreach (AMLDFileModel d in dataList)
            {
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();

                piaLogMain.EXECUTION_TYPE = "P";
                piaLogMain.ACCESS_ACCOUNT = accessAccount;
                piaLogMain.ACCOUNT_NAME = "";
                piaLogMain.AFFECT_ROWS = 1;
                piaLogMain.EXECUTION_CONTENT = d.cin_no;

                piaLogMain.TRACKING_TYPE = "A";
                piaLogMain.PROGFUN_NAME = "AMLDFile";
                piaLogMain.PIA_TYPE = "0100100000";
                piaLogMain.ACCESSOBJ_NAME = "LIVJAMLD3";
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                piaLogMainDao.Insert(piaLogMain);
            }

            

        }



    }
}