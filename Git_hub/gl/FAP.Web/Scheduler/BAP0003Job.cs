using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Transactions;
using System.Linq;
using System.Data.SqlClient;
using Fubon.Utility;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;

/// <summary>
/// 功能說明：BAP0003 電訪派件排程作業
/// 初版作者：20200904 黃黛鈺
/// 修改歷程：20200904 黃黛鈺 
/// 需求單號：202008120153-00
///           初版
/// </summary>
/// 

namespace FAP.WebScheduler
{
    public class BAP0003Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public  void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");

            bool bExec = false;
            FAPScheduleJobDao fAPScheduleJobDao = new FAPScheduleJobDao();
            FAP_SCHEDULE_JOB job = new FAP_SCHEDULE_JOB();
            job = fAPScheduleJobDao.qryByName("BAP0003Job");

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
                job.sched_name = "BAP0003Job";
                job.remark = guid;
                job.start_exe_time = DateTime.Now;
                job.end_exe_time = null;
                job.scan_timec = "1380";
                updCnt = fAPScheduleJobDao.insert(job);

            }

            if (updCnt > 0)
                bExec = true;

            //主程式
            if (bExec)
            {
                int iCnt = 0;



                procMain();



               if (iCnt > 0 )
                    logger.Info("[Execute]作業完成!!");
                else 
                    logger.Info("[Execute]無派件資料!!");


                job.end_exe_time = DateTime.Now;
                job.remark = guid;
                updCnt = fAPScheduleJobDao.updateByName(job, guid);

            }
            else {
                logger.Info("[Execute]未執行!!");
            }

            logger.Info("[Execute]執行結束!!");
        }


        private async Task procMain() {
            //電訪派件
            await getCheckList("tel_assign_case", "0", "");

            //簡訊派件(一年以上簡訊通知)
            await getCheckList("sms_assign_case", "", "0");

        }


        /// <summary>
        /// 寄送派件報表
        /// </summary>
        /// <param name="errList"></param>
        private void genMail(string type, string fullPath, ADModel aDModel)
        {
            string mailSub = "tel_assign_case" == type ? "電話訪問" : "一年以上簡訊通知";

            //組mail內文
            string mailContent = "tel_assign_case" == type ? "電話訪問" : "一年以上簡訊通知";

            string chDt = DateUtil.getCurChtDate(3);
            string _fullPath = fullPath.Replace(".xlsx", ".zip");

            using (var zip = new ZipFile())
            {
                zip.Password = aDModel.user_id + chDt;
                zip.AddFile(fullPath, "");

                zip.Save(_fullPath);
            }

            File.Delete(fullPath);

            MailUtil mailUtil = new MailUtil();
            bool bSucess = mailUtil.sendMail(
                         new string[] { aDModel.e_mail }    //MailTos
                      , mailSub    //MailSub
                      , mailContent //MailBody
                      , true    //isBodyHtml
                     , null //mailAccount
                     , "" //mailPwd
                     , new string[] { _fullPath } //filePaths
                     , true);    //deleteFileAttachment);

        }


        private async Task<int> getCheckList(string tel_std_type, string dispatch_status, string sms_status) {
            int procCnt = 0;

           FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

            //取得所有待派案件
            //List<TelDispatchRptModel> rowsD = fAPTelCheckDao.qryForBAP0003(tel_std_type, dispatch_status, sms_status);


            //if (rowsD.Count == 0)
            //    return 0;

            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

            //取得第一次電訪人員每月的最高件數
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            List<FAP_VE_CODE> procIdRows = new List<FAP_VE_CODE>();
            if ("tel_assign_case".Equals(tel_std_type))
                procIdRows = fAPVeCodeDao.qryByGrp("TEL_DISPATCH");
            else
                procIdRows = fAPVeCodeDao.qryByGrp("SMS_DISPATCH");

            CommonUtil commonUtil = new CommonUtil();
            Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

            //將屬於該派件人員的資料依"保局範圍+金額級距+歸戶條件"計算金額，派件時依金額大->小的順序派件
            foreach (FAP_VE_CODE _procId in procIdRows) {
                List<TelDispatchRptModel> rowsD = fAPTelCheckDao.qryForBAP0003(_procId.code_id, tel_std_type, dispatch_status, sms_status);


                List<TelDispatchRptModel> procIdCheckGrpRows = rowsD
                   .GroupBy(x => new { x.temp_id })
                   .Select(group => new TelDispatchRptModel
                   {
                       temp_id = group.Key.temp_id,
                       check_amt = group.Sum(o => o.main_amt)
                   }).ToList();

                List<TelDispatchRptModel> checkList = new List<TelDispatchRptModel>();

                if (procIdCheckGrpRows.Count == 0)
                    continue;

                if (!empMap.ContainsKey(_procId.code_id))
                {
                    ADModel adModel = new ADModel();
                    adModel = commonUtil.qryEmp(_procId.code_id);
                    empMap.Add(_procId.code_id, adModel);
                }
                

                try
                {

                    string strConn = DbUtil.GetDBFglConnStr();
                    using (SqlConnection conn = new SqlConnection(strConn))
                    {
                        conn.Open();

                        SqlTransaction transaction = conn.BeginTransaction("Transaction");

                        DateTime now = DateTime.Now;
                        string[] curDateTime = Web.BO.DateUtil.getCurChtDateTime(4).Split(' ');

                        String strDateTime = now.ToString("yyyy/MM/dd");
                        //strDateTime = (now.Year - 1911).ToString().PadLeft(3, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

                        try
                        {
                            int iCnt = 0;
                            int iTot = Convert.ToInt32(_procId.code_value);
                            int iTempIdCnt = 0;

                            foreach (TelDispatchRptModel d in procIdCheckGrpRows.OrderByDescending(x => x.check_amt))
                            {
                                iCnt++;

                                if (iCnt <= iTot)
                                {
                                    var aply_no = "";
                                    

                                    TelDispatchRptModel checkTmp = rowsD.Where(x => x.temp_id == d.temp_id).FirstOrDefault();
                                    List<TelDispatchRptModel> tempIdCheckList = new List<TelDispatchRptModel>();
                                    if (checkTmp.temp_id.Equals(StringUtil.toString(checkTmp.paid_id)))
                                        tempIdCheckList = fAPTelCheckDao.qryForBAP0003ByPaidId(checkTmp.temp_id, tel_std_type, dispatch_status, sms_status);
                                    else
                                        tempIdCheckList = rowsD.Where(x => x.temp_id == checkTmp.temp_id).ToList();

                                    iTempIdCnt += tempIdCheckList.Count();

                                    foreach (TelDispatchRptModel temp in tempIdCheckList)
                                    {
                                        //將明細紀錄起來，以供後續出派件報表
                                        temp.tel_interview_id = _procId.code_id;
                                        temp.tel_interview_name = empMap[_procId.code_id].name;
                                        temp.dispatch_date = strDateTime;
                                        checkList.Add(temp);


                                        //電話訪問
                                        if ("tel_assign_case".Equals(tel_std_type))
                                        {
                                            //異動[FAP_TEL_CHECK 電訪支票檔]
                                            fAPTelCheckDao.updForDispacthStatus(_procId.code_id, tel_std_type, temp.system, temp.check_acct_short, temp.check_no
                                                , aply_no, now, "1", conn, transaction);

                                        }
                                        else {  //簡訊
                                            //異動[FAP_TEL_CHECK 電訪支票檔]
                                            fAPTelCheckDao.updForDispacthStatus(_procId.code_id, tel_std_type, temp.system, temp.check_acct_short, temp.check_no
                                                , "", now, "1", conn, transaction);

                                        }
                                    }
                                }
                            }


                            //產出派件報表
                            string guid = "";
                            guid = Guid.NewGuid().ToString();
                            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                                            , string.Concat("BAP0003" + "_" + guid + "_" + tel_std_type, ".xlsx"));
                            VeTelUtil veTelUtil = new VeTelUtil();
                            await veTelUtil.genDispatchRpt("BAP0003", _procId.code_id, empMap[_procId.code_id].name
                                , tel_std_type, fullPath, checkList);


                            procCnt++;
                            transaction.Commit();


                            //MAIL派件報表
                            genMail(tel_std_type, fullPath, empMap[_procId.code_id]);

                        }
                        catch (Exception ex) {
                            transaction.Rollback();
                            throw ex;
                        }
                    }

                }
                catch (Exception e) {
                    logger.Error(e.ToString());
                    errMail(empMap[_procId.code_id]);

                }
            }


            if (procCnt > 0)
                logger.Info("[Execute]作業完成!!");
            else
                logger.Info("[Execute]無派件資料!!");

            return procCnt;
        }





        /// <summary>
        /// 寄送失敗報表給電訪人員
        /// </summary>
        /// <param name="user_id"></param>
        private void errMail(ADModel aDModel) { 
        
        
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