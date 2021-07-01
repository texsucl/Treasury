using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Listener;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
/// <summary>
/// 功能說明：批次排程
/// 初版作者：20190307 黃黛鈺
/// 修改歷程：20190307 黃黛鈺 
/// 需求單號：201808170384-33
///           雙系統銀行分行整併第二階段AS400需求
///           發送錯誤報表通知，預計由OPEN端設控一支排程，每日定時讀取FRTDIRL0檔案，將讀到的內容直接發送MAIL給承辦及承辦的主管。
/// ====================================================================
/// 修改歷程：
/// 需求單號：
/// ====================================================================
/// </summary>
/// 
namespace FRT.WebScheduler
{
    public class JobScheduler
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Start()

        {
            logger.Info("[JobScheduler]Start!!");

            string BRTNBCKTime = System.Configuration.ConfigurationManager.AppSettings.Get("BRTNBCKTime");

            logger.Info("[JobScheduler]BRTNBCKTime:" + BRTNBCKTime);

            if (BRTNBCKTime.Trim().Length > 0)
            {
                DirectSchedulerFactory.Instance.CreateVolatileScheduler(3);
                ISchedulerFactory factory = DirectSchedulerFactory.Instance;

                IScheduler scheduler = factory.GetScheduler();


                //保單匯款基本資料維護錯誤通知
                if (BRTNBCKTime.Trim().Length > 0)
                {
                    JobKey BrtnbckMailJobKey = JobKey.Create("BrtnbckMailJob", "BrtnbckMail");


                    IJobDetail brtnbckMailJob = JobBuilder.Create<BrtnbckMailJob>().WithIdentity(BrtnbckMailJobKey).Build();

                    ITrigger brtnbckMailTrigger = TriggerBuilder.Create()
                                                     .WithIdentity("Trigger", "BrtnbckMail")
                                                     .StartNow()
                                                    .WithCronSchedule(BRTNBCKTime)
                                                    .Build();

                    scheduler.ScheduleJob(brtnbckMailJob, brtnbckMailTrigger);
                }


                scheduler.Start();
            }

            string ORT0105Time = System.Configuration.ConfigurationManager.AppSettings.Get("ORT0105Time");  //add by Mark 20210326

            IScheduler schedulerDaily = StdSchedulerFactory.GetDefaultScheduler();
            schedulerDaily.Start();

            #region ORT0105 勾稽報表
            if (ORT0105Time != null && ORT0105Time.Trim().Length > 0)
            {
                logger.Info("[JobScheduler]ORT0105 勾稽報表排程作業:" + ORT0105Time);
                IJobDetail _ORT0105job = JobBuilder.Create<ORT0105Job>().Build();
                ITrigger _ORT0105trigger = TriggerBuilder.Create()
                    .WithIdentity("ORT0105trigger", "ORT0105")
                    .WithCronSchedule(ORT0105Time.Trim())
                    .Build();
                schedulerDaily.ScheduleJob(_ORT0105job, _ORT0105trigger);
            }
            #endregion

            string ORTB020Time = System.Configuration.ConfigurationManager.AppSettings.Get("ORTB020Time");  //add by Mark 20210427

            #region ORB020 勾稽報表
            if (ORTB020Time != null && ORTB020Time.Trim().Length > 0)
            {
                //logger.Info("[JobScheduler]ORTB020 快速付款出款彙總排程作業:" + ORTB020Time);
                //IJobDetail _ORTB020job = JobBuilder.Create<ORTB020Job>().Build();
                //ITrigger _ORTB020trigger = TriggerBuilder.Create()
                //    .WithIdentity("ORTB020trigger", "ORTB020")
                //    .WithCronSchedule(ORTB020Time.Trim())
                //    .Build();
                //schedulerDaily.ScheduleJob(_ORTB020job, _ORTB020trigger);
            }
            #endregion

            //scheduler.Shutdown();



            //ITrigger triggerMaintainUnit = TriggerBuilder.Create()
            //                                .WithIdentity("UserMaintainUnitJob", "UserMaintainUnit")
            //                                .StartNow()
            //                                .WithSimpleSchedule(s => s
            //                                .WithIntervalInSeconds(30000)
            //                                .RepeatForever())
            //                                .Build();
            //ITrigger triggerUser = TriggerBuilder.Create()
            //                                .WithIdentity("CodeUserJob", "CodeUser")
            //                                .StartNow()
            //                                .WithSimpleSchedule(s => s
            //                                .WithIntervalInSeconds(30000)
            //                                .RepeatForever())
            //                                .Build();
            //ITrigger triggerSrcOprUnit = TriggerBuilder.Create()
            //                                .WithIdentity("SrcOprUnitJob", "SrcOprUnit")
            //                                .StartNow()
            //                                .WithSimpleSchedule(s => s
            //                                .WithIntervalInHours(3)
            //                                .RepeatForever())
            //                                .Build();
            //scheduler.ScheduleJob(srcOprUnitJob, triggerSrcOprUnit);


            // 加载xml job的配置文件
            //NameValueCollection properties = new NameValueCollection();
            //properties["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz";
            //properties["quartz.plugin.xml.fileNames"] = "~/quartz_jobs.xml";
            ////开启job
            //ISchedulerFactory sf = new StdSchedulerFactory(properties);
            //IScheduler sched = sf.GetScheduler();
            //if (!sched.IsStarted)
            //    sched.Start();


            //ITrigger trigger = TriggerBuilder.Create()

            //    .WithIdentity("IDGJob", "IDG")
            //    .WithCronSchedule("0/10 * * * * ?")
            //    .WithPriority(1)

            //    .Build();
            //scheduler.ScheduleJob(srcOprUnit, trigger);


        }
    }
}