
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLCALEDao 日曆檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLCALEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public string isREST()
        {
            logger.Info("isREST begin!");
            var CORP_REST = "N";

            DateTime dt = DateTime.Now;

            int CALE1_YEAR, CALE1_MONTH, CALE1_DAY = 0;

            CALE1_YEAR = dt.Year - 1911;
            CALE1_MONTH = dt.Month;
            CALE1_DAY = dt.Day;

            try
            {
                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();

                    EacCommand cmd = new EacCommand();
                    string strSQL = @"
select CALE1_CORP_REST from LGLCALE1 WHERE CALE1_YEAR = :CALE1_YEAR AND CALE1_MONTH = :CALE1_MONTH AND CALE1_DAY = :CALE1_DAY
";


                    cmd.Connection = conn400;
                    cmd.CommandText = strSQL;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("CALE1_YEAR", CALE1_YEAR);
                    cmd.Parameters.Add("CALE1_MONTH", CALE1_MONTH);
                    cmd.Parameters.Add("CALE1_DAY", CALE1_DAY);

                    DbDataReader result = cmd.ExecuteReader();

                    while (result.Read())
                    {
                        var CALE1_CORP_REST = result["CALE1_CORP_REST"]?.ToString();
                        CORP_REST = CALE1_CORP_REST == "Y" ? "Y" : "N";

                    }

                    cmd.Dispose();
                    cmd = null;

                    logger.Info("isREST end!");

                    return CORP_REST;


                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }




    }
}