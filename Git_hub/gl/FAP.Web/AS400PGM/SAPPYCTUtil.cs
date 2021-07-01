using FAP.Web.AS400Models;
using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// <summary>
/// SAPPYCT 產生應付票據一年內未兌現清單
/// </summary>

namespace FAP.Web.AS400PGM
{

    public class SAPPYCTUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string callSAPPYCT(EacConnection conn, string check_date_b, string check_date_e)
        {
            logger.Info("callSAPPYCT==>" + check_date_b + "|" + check_date_e);
            string rtn_code = "";

            try
            {
                EacCommand cmd = new EacCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SAPPYCT";


                cmd.Parameters.Clear();

                EacParameter area = new EacParameter();
                area.ParameterName = "LINK-AREA";
                area.DbType = DbType.String;
                area.Size = 17;
                area.Direction = ParameterDirection.InputOutput;
                area.Value = (StringUtil.toString(check_date_b).PadLeft(8, '0') +
                    StringUtil.toString(check_date_e).PadLeft(8, '0')).PadRight(17, ' ');
                
                cmd.Parameters.Add(area);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                string rtnArea = cmd.Parameters["LINK-AREA"].Value.ToString();


                try
                {
                    check_date_b = rtnArea.Substring(0, 8);
                    check_date_e = rtnArea.Substring(8, 8);
                    rtn_code = rtnArea.Substring(16, 1);

                    logger.Info("rtn_code:" + StringUtil.toString(rtn_code));
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    throw e;
                }

                cmd.Dispose();
                cmd = null;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            

            return rtn_code;
        }
    }
}