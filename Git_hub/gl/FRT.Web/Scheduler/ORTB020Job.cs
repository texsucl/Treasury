using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using FRT.Web.Service.Actual;
using FRT.Web.BO;

namespace FRT.WebScheduler
{
    public class ORTB020Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info($@"[Execute]ORTB020快速付款出款彙總報表執行開始!!");

            new ORTB020().sendData(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));

            logger.Info($@"[Execute]ORTB020快速付款出款彙總報表執行結束!!");
        }
    }
}