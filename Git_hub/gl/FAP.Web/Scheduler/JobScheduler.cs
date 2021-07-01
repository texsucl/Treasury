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
/// 初版作者：20190424 黃黛鈺
/// 修改歷程：20190424 黃黛鈺 
/// 需求單號：201904100495
///           逾期未兌領支票異動未覆核報表
/// ====================================================================
/// 修改歷程：20200804 黃黛鈺 
/// 需求單號：202008120153-00
/// 修改說明：增加"BAP0003 電訪派件排程作業"
/// ====================================================================
/// </summary>
/// 
namespace FAP.WebScheduler
{
    public class JobScheduler
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Start()

        {
            logger.Info("[JobScheduler]Start!!");

            string BAP0002Time = System.Configuration.ConfigurationManager.AppSettings.Get("BAP0002Time");
            string BAP0003Time = System.Configuration.ConfigurationManager.AppSettings.Get("BAP0003Time");  //add by daiyu 20200904
            string DailyTime = System.Configuration.ConfigurationManager.AppSettings.Get("DailyTime");
            string AMLDFileTime = System.Configuration.ConfigurationManager.AppSettings.Get("AMLDFileTime");
            string FAPPYREL0Time = System.Configuration.ConfigurationManager.AppSettings.Get("FAPPYREL0Time");
            string OAP0028Time = System.Configuration.ConfigurationManager.AppSettings.Get("OAP0028Time");
            string BAP0004Time = System.Configuration.ConfigurationManager.AppSettings.Get("BAP0004Time");  //add by Mark 20200921

            if (string.IsNullOrWhiteSpace(DailyTime))
                DailyTime = string.Empty;

            if (string.IsNullOrWhiteSpace(FAPPYREL0Time))
                FAPPYREL0Time = string.Empty;

            if (string.IsNullOrWhiteSpace(OAP0028Time))
                OAP0028Time = string.Empty;

            if (string.IsNullOrWhiteSpace(BAP0004Time))
                BAP0004Time = string.Empty;

            logger.Info("[JobScheduler]BAP0002Time:" + BAP0002Time);
            logger.Info("[JobScheduler]BAP0003Time:" + BAP0003Time);
            logger.Info("[JobScheduler]BAP0004Time:" + BAP0004Time);
            logger.Info("[JobScheduler]AMLDFileTime:" + AMLDFileTime);

            if (BAP0002Time.Trim().Length > 0)
            {
                DirectSchedulerFactory.Instance.CreateVolatileScheduler(3);
                ISchedulerFactory factory = DirectSchedulerFactory.Instance;

                IScheduler scheduler = factory.GetScheduler();


                //逾期未兌領支票異動未覆核報表
                if (BAP0002Time.Trim().Length > 0)
                {
                    JobKey BAP0002JobKey = JobKey.Create("BAP0002Job", "BAP0002");


                    IJobDetail brtnbckMailJob = JobBuilder.Create<BAP0002Job>().WithIdentity(BAP0002JobKey).Build();

                    ITrigger brtnbckMailTrigger = TriggerBuilder.Create()
                                                     .WithIdentity("Trigger", "BAP0002")
                                                     .StartNow()
                                                    .WithCronSchedule(BAP0002Time)
                                                    .Build();

                    scheduler.ScheduleJob(brtnbckMailJob, brtnbckMailTrigger);
                }


                //AML D檔批次排程
                if (AMLDFileTime.Trim().Length > 0)
                {
                    JobKey AMLDFileJobKey = JobKey.Create("AMLDFileJob", "AMLDFile");


                    IJobDetail amlDMailJob = JobBuilder.Create<AMLDFileJob>().WithIdentity(AMLDFileJobKey).Build();

                    ITrigger amlDMailTrigger = TriggerBuilder.Create()
                                                     .WithIdentity("Trigger", "AMLDFile")
                                                     .StartNow()
                                                    .WithCronSchedule(AMLDFileTime)
                                                    .Build();

                    scheduler.ScheduleJob(amlDMailJob, amlDMailTrigger);
                }

                scheduler.Start();
            }

            IScheduler schedulerDaily = StdSchedulerFactory.GetDefaultScheduler();
            schedulerDaily.Start();

            if (DailyTime.Trim().Length > 0)
            {
                logger.Info("[JobScheduler]每日排程寄信時間: " + DailyTime);
                IJobDetail job = JobBuilder.Create<DailyRoutineJob>().Build();
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("triggerName", "groupName")
                    //.StartNow()
                    .WithCronSchedule(DailyTime)
                    //.WithSimpleSchedule(t =>
                    //t.WithIntervalInSeconds(600)
                    // .RepeatForever())
                    .Build();
                schedulerDaily.ScheduleJob(job, trigger);
            }

            //scheduler.Shutdown();

            #region B06 應付票據比對不符以郵件通知
            if (FAPPYREL0Time.Trim().Length > 0)
            {
                logger.Info("[JobScheduler]B06 應付票據比對不符以郵件通知:" + FAPPYREL0Time);
                IJobDetail _FAPPYREL0job = JobBuilder.Create<FAPPYRELJob>().Build();
                ITrigger _FAPPYREL0trigger = TriggerBuilder.Create()
                    .WithIdentity("FAPPYREL0Trigger", "FAPPYREL0")
                    .WithCronSchedule(FAPPYREL0Time)
                    .Build();
                schedulerDaily.ScheduleJob(_FAPPYREL0job, _FAPPYREL0trigger);
            }

            #endregion

            #region OAP0028 執行明細表產出
            if (OAP0028Time.Trim().Length > 0)
            {
                logger.Info("[JobScheduler]OAP0028 執行明細表產出:" + OAP0028Time);
                IJobDetail _OAP0028job = JobBuilder.Create<OAP0028Job>().Build();
                ITrigger _OAP0028trigger = TriggerBuilder.Create()
                    .WithIdentity("OAP0028trigger", "OAP0028")
                    .WithCronSchedule(OAP0028Time)
                    .Build();
                schedulerDaily.ScheduleJob(_OAP0028job, _OAP0028trigger);
            }

            #endregion

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


            #region BAP0003 電訪派件排程作業
            if (BAP0003Time.Trim().Length > 0)
            {
                IJobDetail _BAP0003job = JobBuilder.Create<BAP0003Job>().Build();
                ITrigger _BAP0003trigger = TriggerBuilder.Create()
                    .WithIdentity("BAP0003trigger", "BAP0003")
                    .WithCronSchedule(BAP0003Time)
                    .Build();
                schedulerDaily.ScheduleJob(_BAP0003job, _BAP0003trigger);
            }
            #endregion


            #region BAP0004 逾期清理追蹤排程作業
            if (BAP0004Time.Trim().Length > 0)
            {
                logger.Info("[JobScheduler]BAP0004 逾期清理追蹤排程作業:" + BAP0004Time);
                IJobDetail _BAP0004job = JobBuilder.Create<BAP0004Job>().Build();
                ITrigger _BAP0004trigger = TriggerBuilder.Create()
                    .WithIdentity("BAP0004trigger", "BAP0004")
                    .WithCronSchedule(BAP0004Time)
                    .Build();
                schedulerDaily.ScheduleJob(_BAP0004job, _BAP0004trigger);
            }
            #endregion
        }
    }
}