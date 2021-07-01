using FAP.Web.AS400Models;
using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// <summary>
/// SAPGETID  取支票件給付對象 ID 
/// </summary>

namespace FAP.Web.AS400PGM
{

    public class SAPGETIDUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string callSAPGETID(EacConnection conn, string system, string aply_no, string aply_seq)
        {
            logger.Info("call SAPGETID==>" + system + "|" + aply_no + "|" + aply_seq);
            string paid_id = "";

            try
            {
                EacCommand cmd = new EacCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SAPGETID";


                cmd.Parameters.Clear();

                EacParameter area = new EacParameter();
                area.ParameterName = "LINK-AREA";
                area.DbType = DbType.String;
                area.Size = 25;
                area.Direction = ParameterDirection.InputOutput;
                area.Value = (StringUtil.toString(system).PadRight(1, ' ') +
                    StringUtil.toString(aply_no).PadRight(10, ' ') +
                    StringUtil.toString(aply_seq).PadLeft(4, '0')) 
                    .PadRight(25, ' ');
                
                cmd.Parameters.Add(area);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                string rtnArea = cmd.Parameters["LINK-AREA"].Value.ToString();


                try
                {
                    if(StringUtil.toString(rtnArea).Length == 25)
                        paid_id = rtnArea.Substring(15, 10);
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

            

            return paid_id;
        }
    }
}