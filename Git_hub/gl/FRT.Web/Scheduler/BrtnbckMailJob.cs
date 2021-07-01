using Quartz;
using System;
using System.Collections.Generic;

using FRT.Web.AS400Models;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System.Data.EasycomClient;
using FRT.Web.BO;
using FRT.Web.Models;

/// <summary>
/// =====================================================================
/// 功能說明：保單匯款基本資料維護錯誤通知
/// 初版作者：20190307 黃黛鈺
/// 修改歷程：20190307 黃黛鈺 
/// 需求單號：201808170384-33
///           初版
/// ======================================================================
/// 需求單號：201905310551-06
/// 修改日期：20191108
/// 修改人員：B0077
/// 修改項目：通知內容的"幣別"、"行庫代號"、"錯誤原因"從檔案的對應欄位取得
/// =======================================================================
/// </summary>
/// 

namespace FRT.WebScheduler
{
    public class BrtnbckMailJob : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("[Execute]執行開始!!");

                bool bExec = false;
                FRTScheduleJobDao fRTScheduleJobDao = new FRTScheduleJobDao();
                FRT_SCHEDULE_JOB job = new FRT_SCHEDULE_JOB();
                job = fRTScheduleJobDao.qryByName("BrtnbckMailJob");

                //System.Threading.Thread.Sleep(15000);

                String guid = Guid.NewGuid().ToString();

                int updCnt = 0;

                if (job != null)
                {
                    if (job.start_exe_time == null)
                    {
                        job.start_exe_time = DateTime.Now;
                        job.end_exe_time = null;
                        updCnt = fRTScheduleJobDao.updateByName(job, guid);
                    }
                    else
                    {
                        TimeSpan diff = DateTime.Now - (DateTime)job.start_exe_time;
                        if (diff.TotalMinutes > Convert.ToInt64(job.scan_timec))
                        {
                            job.start_exe_time = DateTime.Now;
                            job.end_exe_time = null;
                            updCnt = fRTScheduleJobDao.updateByName(job, guid);
                        }
                    }
                }
                else
                {
                    job = new FRT_SCHEDULE_JOB();
                    job.sched_name = "BrtnbckMailJob";
                    job.remark = guid;
                    job.start_exe_time = DateTime.Now;
                    job.end_exe_time = null;
                    job.scan_timec = "1380";
                    updCnt = fRTScheduleJobDao.insert(job);

                }

                if (updCnt > 0)
                    bExec = true;

                logger.Info("bExec:" + bExec);


                if (bExec)
                {
                    FRTDIRLDao fRTDIRLDao = new FRTDIRLDao();
                    List<FRTDIRLModel> dataList = new List<FRTDIRLModel>();
                    dataList = fRTDIRLDao.qryRemitInfoErr();

                    if (dataList.Count > 0)
                        genMail(dataList);
                    else
                        logger.Info("[Execute]無異常資料!!");

                    job.end_exe_time = DateTime.Now;
                    job.remark = guid;
                    updCnt = fRTScheduleJobDao.updateByName(job, guid);

                }
                else
                {
                    logger.Info("[Execute]未執行!!");
                }

                logger.Info("[Execute]執行結束!!");
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
             
        }



        /// <summary>
        /// 處理異常資料寄送MAIL
        /// </summary>
        /// <param name="errList"></param>
        private void genMail(List<FRTDIRLModel> dataList)
        {
            MailUtil mailUtil = new MailUtil();
            OaEmpDao oaEmpDao = new OaEmpDao();
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

            Dictionary<string, UserBossModel> empMap = new Dictionary<string, UserBossModel>();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();
            con.ConnectionString = CommonUtil.GetEasycomConn();
            con.Open();
            cmd.Connection = con;

            foreach (FRTDIRLModel d in dataList) {
                try {
                    if (!empMap.ContainsKey(d.updId))
                    {
                        UserBossModel userBossModel = oaEmpDao.getEmpBoss(d.updId, con, cmd);
                        empMap.Add(d.updId, userBossModel);
                    }

                    UserBossModel user = empMap[d.updId];
                    if (user != null)
                    {
                        string mailContent = "";
                        mailContent =
                            "   資料來源 = " + d.resource + "<br/>" +
                            "   保單號碼 = " + d.policyNo + "<br/>" +
                            "   幣別 =" + d.currency + "<br/>" +  //modify by daiyu 20191108
                            "   異動人員 = " + StringUtil.toString(user.empName) + "<br/>" +
                            "   行庫代號 = " + d.bankNo + "<br/>" + //modify by daiyu 20191108
                            "   帳號 = " + MaskUtil.maskBankAct(d.bankAct) + "<br/>" +
                            "   受款人戶名 = " + d.payment + "<br/>" +
                            "   錯誤原因 = " + StringUtil.toString(d.payfErrtx);    //modify by daiyu 20191108


                        bool bSucess = mailUtil.sendMail(empMap[d.updId]
                      , "保單匯款基本資料維護錯誤通知"
                      , mailContent
                      , true
                     , ""
                     , ""
                     , null
                     , true
                     , true, false
                     , false
                     , d.policyNo);
                    }


                    //加寫稽核軌跡
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = d.updId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "BrtnbckMailJob";
                    piaLogMain.EXECUTION_CONTENT = "TYPE = '1' AND SRCEPGM = 'BRTNBCK'";
                    piaLogMain.AFFECT_ROWS = 1;
                    piaLogMain.PIA_TYPE = "0001100000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FRTDIRL";
                    piaLogMain.PIA_OWNER1 = d.policyNo;
                    piaLogMainDao.Insert(piaLogMain);


                }
                catch (Exception e) {
                    logger.Error("updId = " + d.updId + " policyNo = " + d.policyNo + " : "+  e.ToString());
                }
            }


            cmd.Dispose();
            cmd = null;
            con.Close();
            con = null;

        }

        
    }
}