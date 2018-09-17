using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Listener;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Treasury.Web.Scheduler;
/// <summary>
/// 功能說明：批次排程
/// 初版作者：20180913 張家華
/// 修改歷程：20180913 張家華 
/// 需求單號：
/// </summary>
/// 
namespace Treasury.WebScheduler
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<DailyRoutineJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerName", "groupName")
                .WithSimpleSchedule(t =>
                t.WithIntervalInSeconds(60)
                 .RepeatForever()).Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}