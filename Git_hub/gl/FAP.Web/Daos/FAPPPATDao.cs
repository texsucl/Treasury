

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPPPAT  逾期未兌領重新給付登錄檔
/// ------------------------------------------
/// add by daiyu 20191126
/// 需求單號：201910290100-01
/// 修改內容：「OAP0002逾期未兌領支票維護作業」執行修改作業時，檢查是否存在PPAT
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPPPATDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知(Filler_14=03*)
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <returns></returns>
        public bool chkExist(EacConnection conn400, string check_no, string check_shrt)
        {

            logger.Info("chkExist begin!");
            bool bExist = false;

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT DISTINCT CHECK_NO
      ,CHECK_SHRT
  FROM LAPPPAT5
    WHERE CHECK_NO = :check_no 
      AND CHECK_SHRT = :check_shrt
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("check_no", check_no);
                cmdQ.Parameters.Add("check_shrt", check_shrt);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    bExist = true;
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("chkExist end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return bExist;

        }

        

    }
}