using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using FRT.Web.Service.Actual;

namespace FRT.WebScheduler
{
    public class ORT0105Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            // 比對出執行時間相符的資料，寄送追蹤報表給相關追蹤人員 
            foreach (var check in new ORT0105().getAllCheck())
            {
                if (check.frequency == "d")
                {
                    logger.Info($@"[Execute]ORT0105勾稽報表執行開始!! id => {check.check_id}");
                    CallReport(check);
                    logger.Info($@"[Execute]ORT0105勾稽報表執行結束!! id => {check.check_id}");
                }
                else if (check.frequency == "m")
                {
                   var _date =  new GoujiReport().getReportDate(check);
                    if (_date.Item1 == DateTime.Now.Date)
                    {
                        logger.Info($@"[Execute]ORT0105勾稽報表執行開始!! id => {check.check_id}");
                        CallReport(check);
                        logger.Info($@"[Execute]ORT0105勾稽報表執行結束!! id => {check.check_id}");
                    }
                }
            }
        }

        private void CallReport(Web.Models.FRT_CROSS_SYSTEM_CHECK data)
        {
            switch (data.type)
            {
                case "AP":
                    if (data.kind == "1")
                    {
                        new ORT0105AP1().check(data, "System");
                    }
                    else if (data.kind == "2")
                    {
                        new ORT0105AP2().check(data, "System");
                    }
                    else if (data.kind == "3")
                    {
                        new ORT0105AP3().check(data, "System");
                    }
                    break;
                case "NP":
                    if (data.kind == "1")
                    {
                        //new ORT0105BD1().check(data, "System");
                    }
                    else if (data.kind == "3")
                    {
                        new ORT0105NP3().check(data, "System");
                    }
                    break;
                case "BD":
                    if (data.kind == "1")
                    {
                        new ORT0105BD1().check(data, "System");
                    }
                    else if (data.kind == "2")
                    {
                        new ORT0105BD2().check(data, "System");
                    }
                    break;
            }
        }
    }
}