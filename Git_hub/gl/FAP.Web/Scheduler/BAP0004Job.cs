using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using FAP.Web.Service.Actual;
using FAP.Web.Daos;
using FAP.Web.Models;

/// <summary>
/// 功能說明：BAP0004 逾期清理追蹤排程作業
/// 初版作者：20200921 張家華
/// 修改歷程：20200921 張家華 
/// 需求單號：202008120153-00
/// 修改內容：初版
/// ------------------------------------------
/// 需求單號：
/// 修改歷程：20210128 daiyu
/// 修改內容：1.修改排程重複執行問題
/// ------------------------------------------
/// </summary>
/// 

namespace FAP.WebScheduler
{
    public class BAP0004Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");

            try
            {
                bool bExec = false;
                FAPScheduleJobDao fAPScheduleJobDao = new FAPScheduleJobDao();
                FAP_SCHEDULE_JOB job = new FAP_SCHEDULE_JOB();
                job = fAPScheduleJobDao.qryByName("BAP0004Job");

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
                else
                {
                    job = new FAP_SCHEDULE_JOB();
                    job.sched_name = "BAP0004Job";
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
                    procMain();

                    logger.Info("[Execute]作業完成!!");

                    job.end_exe_time = DateTime.Now;
                    job.remark = guid;
                    fAPScheduleJobDao.updateByName(job, guid);

                }
                else
                {
                    logger.Info("[Execute]未執行!!");
                }

                logger.Info("[Execute]執行結束!!");
            }
            catch (Exception e) {

                logger.Error(e.ToString());
            }
        }


        private void procMain()
        {
            logger.Info("[Execute]BAP0004執行開始!!");
            // 比對出已達追蹤條件的資料，寄送追蹤報表給相關追蹤人員 
            var _result_1 = new BAP0004().VE_Clear_Scheduler("BAP0004", "M");
            if (_result_1.Item1)
            {
                logger.Info($@"比對出已達追蹤條件的資料，寄送追蹤報表給相關追蹤人員 , 成功!!");
            }
            else
            {
                logger.Info($@"比對出已達追蹤條件的資料，寄送追蹤報表給相關追蹤人員 , 失敗 => {_result_1.Item2} !!");
            }

            //delete by daiyu 20201201 與財務部討論取消三次追蹤需重新指定追蹤人員的資料
            ////比對資料，查已達N個月需重新派件的資料
            //var _result_2 = new BAP0004().VE_Clear_ReDispatch("BAP0004");
            //if (_result_2.Item1)
            //{
            //    logger.Info($@"比對資料，查已達N個月需重新派件的資料 , 成功!!");
            //}
            //else
            //{
            //    logger.Info($@"比對資料，查已達N個月需重新派件的資料 , 失敗 => {_result_2.Item2} !!");
            //}


            //比對資料，查已達N個月未聯繫的資料
            var _result_3 = new BAP0004().VE_Clear_Cust("BAP0004");
            if (_result_3.Item1)
            {
                logger.Info($@"比對資料，查已達N個月未聯繫的資料 , 成功!!");
            }
            else
            {
                logger.Info($@"比對資料，查已達N個月未聯繫的資料 , 失敗 => {_result_3.Item2} !!");
            }
            //比對資料，將符合設定期間的簡訊資訊清空
            var _result_4 = new BAP0004().SMS_Clear("BAP0004");
            if (_result_4.Item1)
            {
                logger.Info($@"比對資料，將符合設定期間的簡訊資訊清空 , 成功!!");
            }
            else
            {
                logger.Info($@"比對資料，將符合設定期間的簡訊資訊清空 , 失敗 => {_result_4.Item2} !!");
            }
            //產生清理暨逾期處理清單
            var _result_5 = new BAP0004().VE_Level_Detail("BAP0004", "M");
            if (_result_5.Item1)
            {
                logger.Info($@"產生清理暨逾期處理清單 , 成功!!");
            }
            else
            {
                logger.Info($@"產生清理暨逾期處理清單 , 失敗 => {_result_5.Item2} !!");
            }


            // 寄送追蹤報表給相關追蹤人員 
            var _result_6 = new BAP0004().Trace_Notify_Scheduler("BAP0004", "M");
            if (_result_6.Item1)
            {
                logger.Info($@"寄送追蹤報表給相關追蹤人員 , 成功!!");
            }
            else
            {
                logger.Info($@"寄送追蹤報表給相關追蹤人員 , 失敗 => {_result_6.Item2} !!");
            }

            logger.Info("[Execute]BAP0004執行結束!!");

        }
    }
}