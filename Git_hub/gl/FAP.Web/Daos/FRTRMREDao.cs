

using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FRTRMRE 繳款資料明細檔 
/// </summary>
namespace FAP.Web.Daos
{
    public class FRTRMREDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 「OAP0001上傳應付未付主檔/明細檔作業」資料檢核
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="proNo"></param>
        /// <returns>
        /// 空白:正常
        /// 1:狀態有誤
        /// </returns>
        public string qryForOAP0001(EacConnection conn400, string proNo)
        {
            logger.Info("qryForOAP0001 begin!");

            string rtnMsg = "";

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT PRO_NO
  FROM LRTRMRE1 
    WHERE PRO_NO = :PRO_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("PRO_NO", proNo);

                DbDataReader result = cmdQ.ExecuteReader();
                bool bExist = false;

                while (result.Read())
                {
                    bExist = true;
                }

                if (bExist)
                    rtnMsg = "";
                else
                    rtnMsg = "1";


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0001 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return rtnMsg;

        }

        
        

    }
}