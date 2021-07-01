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
/// 初版作者：20190506 黃黛鈺
/// 修改歷程：20190506 黃黛鈺 
/// 需求單號：2019
///           給付優化-未接收報表
/// ====================================================================
/// 修改歷程：
/// 需求單號：
/// ====================================================================
/// </summary>
/// 
namespace FGL.WebScheduler
{
    public class JobScheduler
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Start()

        {
            logger.Info("[JobScheduler]Start!!");

            string BGL0009Time = System.Configuration.ConfigurationManager.AppSettings.Get("BGL0009Time");

            logger.Info("[JobScheduler]BGL0009Time:" + BGL0009Time);

            if (BGL0009Time.Trim().Length > 0)
            {
                DirectSchedulerFactory.Instance.CreateVolatileScheduler(3);
                ISchedulerFactory factory = DirectSchedulerFactory.Instance;

                IScheduler scheduler = factory.GetScheduler();


                //給付優化-未接收報表
                if (BGL0009Time.Trim().Length > 0)
                {
                    JobKey BGL0009JobKey = JobKey.Create("BGL0009Job", "BGL0009");


                    IJobDetail BGL0009Job = JobBuilder.Create<BGL0009Job>().WithIdentity(BGL0009JobKey).Build();

                    ITrigger BGL0009Trigger = TriggerBuilder.Create()
                                                     .WithIdentity("Trigger", "BGL0009")
                                                     .StartNow()
                                                    .WithCronSchedule(BGL0009Time)
                                                    .Build();

                    scheduler.ScheduleJob(BGL0009Job, BGL0009Trigger);
                }


                scheduler.Start();
            }


        }
    }
}