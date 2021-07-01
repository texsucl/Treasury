using FRT.Web.AS400Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400PGM
{
    public class SAAEMPYUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<string, SAAEMPYModel> callSAAEMPY(EacConnection con, EacCommand cmd
            , string usrId, Dictionary<string, SAAEMPYModel> userMap) {

            try
            {
                cmd.Parameters.Clear();
                EacParameter p1 = new EacParameter();
                p1.ParameterName = "SAAEMPY";
                p1.DbType = DbType.String;
                p1.Size = 36;
                p1.Direction = ParameterDirection.InputOutput;
                p1.Value = " " + usrId.PadRight(10, ' ') + "".PadRight(25, ' ');

                cmd.Parameters.Add(p1);
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                String strRtn = cmd.Parameters["SAAEMPY"].Value.ToString();
                SAAEMPYModel user = new SAAEMPYModel(strRtn);
                userMap.Add(usrId, user);
            }
            catch (Exception e) {
                logger.Error("usrId:" + usrId + "=" + e.ToString());
                SAAEMPYModel user = new SAAEMPYModel("");
                userMap.Add(usrId, user);
            }
            

            return userMap;
        }
    }
}