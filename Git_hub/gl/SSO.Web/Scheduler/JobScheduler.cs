//using Quartz;
//using Quartz.Impl;
//using Quartz.Impl.Matchers;
//using Quartz.Listener;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Web;
///// <summary>
///// 功能說明：批次排程
///// 初版作者：20170817 黃黛鈺
///// 修改歷程：20170817 黃黛鈺 
///// 需求單號：201707240447-01 
/////           初版
///// ====================================================================
///// 修改歷程：20180212 黃黛鈺 
/////           需求單號：201801230413-00 
/////           加入電子發票異常報表排程
///// ====================================================================
///// 修改歷程：20180308 黃黛鈺 
/////           需求單號：201801230413-00 
/////           加入查詢總帳系統付款傳票號碼的資訊
///// </summary>
///// 
//namespace FGL.WebScheduler
//{
//    public class JobScheduler
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        public static void Start()

//        {
//            logger.Info("[JobScheduler]Start!!");

//            string SSSSynTime = System.Configuration.ConfigurationManager.AppSettings.Get("SSSSynTime");
//            string EInvoRptTime = System.Configuration.ConfigurationManager.AppSettings.Get("EInvoRptTime");    //add by daiyu 20180212
//            string GLSISynTime = System.Configuration.ConfigurationManager.AppSettings.Get("GLSISynTime");    //add by daiyu 20180308

//            logger.Info("[JobScheduler]SSSSynTime:" + SSSSynTime);
//            logger.Info("[JobScheduler]EInvoRptTime:" + EInvoRptTime);
//            logger.Info("[JobScheduler]GLSISynTime:" + GLSISynTime);



//            if (SSSSynTime.Trim().Length > 0 || EInvoRptTime.Trim().Length > 0 || GLSISynTime.Trim().Length > 0) {
//                DirectSchedulerFactory.Instance.CreateVolatileScheduler(3);
//                ISchedulerFactory factory = DirectSchedulerFactory.Instance;

//                IScheduler scheduler = factory.GetScheduler();

                
//                //SSS DB同步作業
//                if (SSSSynTime.Trim().Length > 0)
//                {
//                    JobKey userMaintainUnitJobKey = JobKey.Create("UserMaintainUnitJob", "SSS");
//                    JobKey codeUserJobKey = JobKey.Create("CodeUserJob", "SSS");
//                    JobKey srcOprUnitJobKey = JobKey.Create("SrcOprUnitJob", "SSS");

//                    //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

//                    //scheduler.Start();

//                    IJobDetail userMaintainUnitJob = JobBuilder.Create<UserMaintainUnitJob>().WithIdentity(userMaintainUnitJobKey).Build();

//                    IJobDetail codeUserJob = JobBuilder.Create<CodeUserJob>().WithIdentity(codeUserJobKey).StoreDurably(true).Build();

//                    IJobDetail srcOprUnitJob = JobBuilder.Create<SrcOprUnitJob>().WithIdentity(srcOprUnitJobKey).StoreDurably(true).Build();

//                    ITrigger firstJobTrigger = TriggerBuilder.Create()
//                                                     .WithIdentity("Trigger", "SSS")
//                                                     .StartNow()
//                                                    .WithCronSchedule(SSSSynTime)
//                                                    .Build();


//                    JobChainingJobListener listener = new JobChainingJobListener("SSS Chain");
//                    listener.AddJobChainLink(userMaintainUnitJobKey, codeUserJobKey);
//                    listener.AddJobChainLink(codeUserJobKey, srcOprUnitJobKey);

//                    scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.GroupEquals("SSS"));

//                    //scheduler.Start();
//                    scheduler.ScheduleJob(userMaintainUnitJob, firstJobTrigger);
//                    scheduler.AddJob(codeUserJob, false, true);
//                    scheduler.AddJob(srcOprUnitJob, false, true);
//                }

//                //電子發票異常報表
//                if (EInvoRptTime.Trim().Length > 0)
//                {
//                    JobKey EInvoRptJobKey = JobKey.Create("EInvoRptJob", "EInvo");


//                    IJobDetail eInvoRptJob = JobBuilder.Create<EInvoErrRptJob>().WithIdentity(EInvoRptJobKey).Build();

//                    ITrigger eInvoJobTrigger = TriggerBuilder.Create()
//                                                     .WithIdentity("Trigger", "EInvo")
//                                                     .StartNow()
//                                                    .WithCronSchedule(EInvoRptTime)
//                                                    .Build();

//                    scheduler.ScheduleJob(eInvoRptJob, eInvoJobTrigger);
//                }

//                //查詢總帳系統付款傳票號碼
//                if (GLSISynTime.Trim().Length > 0)
//                {
//                    JobKey GLSISynJobKey = JobKey.Create("GLSISynJob", "GLSISyn");

//                    IJobDetail GLSISynJob = JobBuilder.Create<GLSISynJob>().WithIdentity(GLSISynJobKey).Build();

//                    ITrigger GLSISynJobTrigger = TriggerBuilder.Create()
//                                                     .WithIdentity("Trigger", "GLSISyn")
//                                                     .StartNow()
//                                                    .WithCronSchedule(GLSISynTime)
//                                                    .Build();

//                    scheduler.ScheduleJob(GLSISynJob, GLSISynJobTrigger);
//                }



//                scheduler.Start();
//            }


           


//            //scheduler.Shutdown();



//            //ITrigger triggerMaintainUnit = TriggerBuilder.Create()
//            //                                .WithIdentity("UserMaintainUnitJob", "UserMaintainUnit")
//            //                                .StartNow()
//            //                                .WithSimpleSchedule(s => s
//            //                                .WithIntervalInSeconds(30000)
//            //                                .RepeatForever())
//            //                                .Build();
//            //ITrigger triggerUser = TriggerBuilder.Create()
//            //                                .WithIdentity("CodeUserJob", "CodeUser")
//            //                                .StartNow()
//            //                                .WithSimpleSchedule(s => s
//            //                                .WithIntervalInSeconds(30000)
//            //                                .RepeatForever())
//            //                                .Build();
//            //ITrigger triggerSrcOprUnit = TriggerBuilder.Create()
//            //                                .WithIdentity("SrcOprUnitJob", "SrcOprUnit")
//            //                                .StartNow()
//            //                                .WithSimpleSchedule(s => s
//            //                                .WithIntervalInHours(3)
//            //                                .RepeatForever())
//            //                                .Build();
//            //scheduler.ScheduleJob(srcOprUnitJob, triggerSrcOprUnit);


//            // 加载xml job的配置文件
//            //NameValueCollection properties = new NameValueCollection();
//            //properties["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz";
//            //properties["quartz.plugin.xml.fileNames"] = "~/quartz_jobs.xml";
//            ////开启job
//            //ISchedulerFactory sf = new StdSchedulerFactory(properties);
//            //IScheduler sched = sf.GetScheduler();
//            //if (!sched.IsStarted)
//            //    sched.Start();


//            //ITrigger trigger = TriggerBuilder.Create()

//            //    .WithIdentity("IDGJob", "IDG")
//            //    .WithCronSchedule("0/10 * * * * ?")
//            //    .WithPriority(1)

//            //    .Build();
//            //scheduler.ScheduleJob(srcOprUnit, trigger);


//        }
//    }
//}