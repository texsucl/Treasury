using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Service.Actual;
using FAP.Web.ViewModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Transactions;

/// <summary>
/// 功能說明：應付票據 自動執行明細表產出
/// 初版作者：20200513 張家華
/// 修改歷程：20200513 張家華 
/// 需求單號：202005131512
///           初版
/// </summary>
/// 

namespace FAP.WebScheduler
{
    public class OAP0028Job : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 主程式段
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");
            var _result = new OAP0028().changStatus("A0317");
            if (string.IsNullOrWhiteSpace(_result.Item2))
            {
                logger.Info($@"執行明細表產出 , 成功!!");
            }
            else
            {
                logger.Info($@"執行明細表產出 , 失敗 => {_result.Item2} !!");
            }
            logger.Info("[Execute]執行結束!!");
        }

    }
}