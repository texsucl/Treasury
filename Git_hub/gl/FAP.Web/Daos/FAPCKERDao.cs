

using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPCKER 支票作廢檔 
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPCKERDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 「OAP0001上傳應付未付主檔/明細檔作業」資料檢核
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="checkNo"></param>
        /// <param name="bankCode"></param>
        /// <returns>
        /// 空白:正常
        /// 1:狀態有誤
        /// </returns>
        public string qryForOAP0001(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("qryForOAP0001 begin!");

            string delCode = "";

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT CHECK_NO, BANK_CODE, DEL_CODE
  FROM LAPCKER1 
    WHERE BANK_CODE = :BANK_CODE 
      AND CHECK_NO = :CHECK_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("BANK_CODE", bankCode);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);
                
                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    delCode = result["DEL_CODE"]?.ToString();
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0001 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return delCode;

        }

        
        

    }
}