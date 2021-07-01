using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Transactions;

/// <summary>
/// 功能說明：逾期未兌領支票異動未覆核報表
/// 初版作者：20190424 黃黛鈺
/// 修改歷程：20190424 黃黛鈺 
/// 需求單號：201904100495
///           初版
/// </summary>
/// 

namespace FAP.WebScheduler
{
    public class BAP0002Job : IJob
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
            job = fAPScheduleJobDao.qryByName("BAP0002Job");

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
                job.sched_name = "BAP0002Job";
                job.remark = guid;
                job.start_exe_time = DateTime.Now;
                job.end_exe_time = null;
                job.scan_timec = "1380";
                updCnt = fAPScheduleJobDao.insert(job);

            }

            if (updCnt > 0)
                bExec = true;

            //主程式(查詢未覆核資料)
            if (bExec)
            {
                List<APAplyRecModel> rows002 = new List<APAplyRecModel>();
                List<APAplyRecModel> rows003 = new List<APAplyRecModel>();

                using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();

                        //查詢『OAP0002A逾期未兌領支票維護覆核作業』的未覆核資料
                        rows002 = fAPAplyRecDao.qryAplyType("A", "1", "", db);

                        //查詢『OAP0003A逾期未兌領支票維護覆核作業-明細資料』的未覆核資料
                        rows003 = fAPAplyRecDao.qryAplyType("B", "1", "", db);

                    }
                }

                if (rows002.Count > 0 || rows003.Count > 0)
                    genMail(rows002, rows003);
                else 
                    logger.Info("[Execute]無未覆核資料!!");


                job.end_exe_time = DateTime.Now;
                job.remark = guid;
                updCnt = fAPScheduleJobDao.updateByName(job, guid);

            }
            else {
                logger.Info("[Execute]未執行!!");
            }

            logger.Info("[Execute]執行結束!!");
        }



        /// <summary>
        /// 處理異常資料寄送MAIL
        /// </summary>
        /// <param name="errList"></param>
        private void genMail(List<APAplyRecModel> rows002, List<APAplyRecModel> rows003)
        {
            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            List<FPMCODEModel> mailRec = fPMCODEDao.qryFPMCODE("VE_DEPT", "", "DEPT_ID");

            if (mailRec.Count == 0) {
                logger.Info("[Execute]查無寄送對象!!");
                return;
            }

            
            Dictionary<string, UserBossModel> mailEmpMap = new Dictionary<string, UserBossModel>();

            List<rptModel> rptList = new List<rptModel>();

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                //查詢寄送對象的EMAIL
                OaEmpDao oaEmpDao = new OaEmpDao();
                foreach (FPMCODEModel d in mailRec)
                {
                    if (!mailEmpMap.ContainsKey(d.text.TrimEnd()))
                    {
                        UserBossModel userBossModel = oaEmpDao.getEmpBoss(d.text.TrimEnd(), conn400);
                        mailEmpMap.Add(d.text.TrimEnd(), userBossModel);
                    }
                }

                //回查AS400 PPAA的資料
                FMNPPAADao fMNPPAADao = new FMNPPAADao();
                foreach (APAplyRecModel d in rows002) {
                    string[] qCon = d.appr_mapping_key.Split('|');
                    string checkNo = qCon[0];
                    string checkShrt = qCon[1];

                    List<FMNPPAAModel> ppaaList = fMNPPAADao.qryForOAP0002(conn400, checkShrt, checkNo);
                    if (ppaaList.Count > 0)
                    {
                        rptModel rptData = new rptModel();
                        ObjectUtil.CopyPropertiesTo(ppaaList[0], rptData);
                        rptData.create_id = d.create_id;
                        rptData.create_dt = d.create_dt == "" ? "" : d.create_dt.Split(' ')[0];

                        rptList.Add(rptData);
                    }
                }

                //回查AS400 PPAD的資料
                FMNPPADDao fMNPPADDao = new FMNPPADDao();
                foreach (APAplyRecModel d in rows003)
                {
                    List<OAP0003PoliModel> ppadList = fMNPPADDao.qryForOAP0003(conn400, d.appr_mapping_key);
                    if (ppadList.Count > 0) {
                        rptModel rptData = new rptModel();
                        ObjectUtil.CopyPropertiesTo(ppadList[0], rptData);
                        rptData.create_id = d.create_id;
                        rptData.create_dt = d.create_dt == "" ? "" : d.create_dt.Split(' ')[0];

                        rptList.Add(rptData);
                    }
                }
            }

            //查詢異動經辦姓名
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (rptModel d in rptList)
                {
                    createUid = StringUtil.toString(d.create_id);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.create_id = userNameMap[createUid];
                    }
                }
            }

            string[] mailTo = new string[mailEmpMap.Count];
            int i = 0;
            foreach (KeyValuePair<string, UserBossModel> d in mailEmpMap)
            {
                mailTo[i] = d.Value.empMail.TrimEnd();
                i++;
            }
            


            //組mail內文
            string mailContent = "";
            mailContent += "<table border='1' > <tr><td colspan='6' align='center'>逾期未兌領支票異動未覆核報表</td></tr>";
            mailContent += "<tr><td>　系統別　</td><td>　支票號碼　</td><td>　帳戶簡稱　</td><td>　信函編號　</td><td>　異動日期　</td><td>　異動經辦　</td></tr>";

            foreach (rptModel d in rptList) {
                mailContent += "<tr>";
                mailContent += "<td align='center'>　" + d.system + "　</td>";
                mailContent += "<td>　" + d.check_no + "　</td>";
                mailContent += "<td align='center'>　" + d.check_shrt + "　</td>";
                mailContent += "<td>　" + d.report_no + "　</td>";
                mailContent += "<td>　" + d.create_dt + "　</td>";
                mailContent += "<td>　" + MaskUtil.maskName(d.create_id) + "　</td>";
                mailContent += "</tr>";
            }

            mailContent += "</table>";


            MailUtil mailUtil = new MailUtil();
            bool bSucess = mailUtil.sendMail(
                         mailTo    //MailTos
                      , "逾期未兌領支票異動未覆核報表"    //MailSub
                      , mailContent //MailBody
                      , true    //isBodyHtml
                     , null //mailAccount
                     , "" //mailPwd
                     , null //filePaths
                     , false);    //deleteFileAttachment);

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