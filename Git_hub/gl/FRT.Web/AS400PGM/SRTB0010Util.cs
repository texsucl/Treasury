using FRT.Web.AS400Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400PGM
{
    public class SRTB0010Util
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SRTB0010Model callSRTB0010(EacConnection con, EacCommand cmd, string usrId)
        {

            try
            {

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SRTB0010";


                cmd.Parameters.Clear();

                EacParameter id = new EacParameter();
                id.ParameterName = "LK-ID";
                id.DbType = DbType.String;
                id.Size = 10;
                id.Direction = ParameterDirection.InputOutput;
                id.Value = usrId.Trim();

                EacParameter rtnCode = new EacParameter();
                rtnCode.ParameterName = "LK-RCODE";
                rtnCode.DbType = DbType.String;
                rtnCode.Size = 1;
                rtnCode.Direction = ParameterDirection.InputOutput;
                rtnCode.Value = "";

                EacParameter empNo = new EacParameter();
                empNo.ParameterName = "LK-EMP-NO";
                empNo.DbType = DbType.String;
                empNo.Size = 6;
                empNo.Direction = ParameterDirection.InputOutput;
                empNo.Value = "";

                EacParameter empId = new EacParameter();
                empId.ParameterName = "LK-EMP-ID";
                empId.DbType = DbType.String;
                empId.Size = 10;
                empId.Direction = ParameterDirection.InputOutput;
                empId.Value = "";

                EacParameter empAd = new EacParameter();
                empAd.ParameterName = "LK-EMP-AD";
                empAd.DbType = DbType.String;
                empAd.Size = 10;
                empAd.Direction = ParameterDirection.InputOutput;
                empAd.Value = "";

                EacParameter empName = new EacParameter();
                empName.ParameterName = "LK-EMP-NM";
                empName.DbType = DbType.String;
                empName.Size = 10;
                empName.Direction = ParameterDirection.InputOutput;
                empName.Value = "";

                EacParameter empUnit = new EacParameter();
                empUnit.ParameterName = "LK-EMP-UT";
                empUnit.DbType = DbType.String;
                empUnit.Size = 10;
                empUnit.Direction = ParameterDirection.InputOutput;
                empUnit.Value = "";

                EacParameter empMgrNo = new EacParameter();
                empMgrNo.ParameterName = "LK-MGR-NO";
                empMgrNo.DbType = DbType.String;
                empMgrNo.Size = 6;
                empMgrNo.Direction = ParameterDirection.InputOutput;
                empMgrNo.Value = "";

                cmd.Parameters.Add(id);
                cmd.Parameters.Add(rtnCode);
                cmd.Parameters.Add(empNo);
                cmd.Parameters.Add(empId);
                cmd.Parameters.Add(empAd);
                cmd.Parameters.Add(empName);
                cmd.Parameters.Add(empUnit);
                cmd.Parameters.Add(empMgrNo);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                SRTB0010Model user = new SRTB0010Model();
                user.rtnCode = cmd.Parameters["LK-RCODE"].Value.ToString();
                user.empNo = cmd.Parameters["LK-EMP-NO"].Value.ToString();
                user.empId = cmd.Parameters["LK-EMP-ID"].Value.ToString();
                user.empAd = cmd.Parameters["LK-EMP-AD"].Value.ToString();
                user.empName = cmd.Parameters["LK-EMP-NM"].Value.ToString();
                user.empUnit = cmd.Parameters["LK-EMP-UT"].Value.ToString();
                user.empMgrNo = cmd.Parameters["LK-MGR-NO"].Value.ToString();

                return user;
            }
            catch (Exception e)
            {
                logger.Error("usrId:" + usrId + "=" + e.ToString());
                throw e;
            }
        }



        public Dictionary<string, SRTB0010Model> callSRTB0010(EacConnection con, EacCommand cmd
            , string usrId, Dictionary<string, SRTB0010Model> userMap) {

            try
            {
                userMap.Add(usrId, callSRTB0010(con, cmd, usrId));
            }
            catch (Exception e) {
                logger.Error("usrId:" + usrId + "=" + e.ToString());
                SRTB0010Model user = new SRTB0010Model();
                userMap.Add(usrId, user);
            }
            

            return userMap;
        }
    }
}