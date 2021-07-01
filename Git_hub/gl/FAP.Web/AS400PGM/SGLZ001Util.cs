using FAP.Web.AS400Models;
using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400PGM
{
    public class SGLZ001Util
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SGLZ001Model callSGLZ001(EacConnection con, EacCommand cmd, SGLZ001Model model)
        {

            try
            {
                string strArea = StringUtil.toString(model.numtype);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SGLZ001";


                cmd.Parameters.Clear();

                EacParameter area = new EacParameter();
                area.ParameterName = "AREA";
                area.DbType = DbType.String;
                area.Size = 37;
                area.Direction = ParameterDirection.InputOutput;
                area.Value = StringUtil.toString(model.numtype).PadLeft(1, ' ') +
                    StringUtil.toString(model.sys_type).PadLeft(3, ' ') +
                    StringUtil.toString(model.srce_from).PadLeft(3, ' ') +
                    StringUtil.toString(model.trns_itf).PadLeft(3, ' ') +
                    StringUtil.toString(model.sys_date).PadLeft(8, '0') +
                    StringUtil.toString(model.trns_no).PadLeft(18, ' ') +
                    StringUtil.toString(model.rtn_code).PadLeft(1, ' ');

                
                cmd.Parameters.Add(area);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                string rtnArea = cmd.Parameters["AREA"].Value.ToString();
                SGLZ001Model rtn = new SGLZ001Model();
                try
                {
                    rtn.numtype = rtnArea.Substring(0, 1);
                    rtn.sys_type = rtnArea.Substring(1, 3);
                    rtn.srce_from = rtnArea.Substring(4, 3);
                    rtn.trns_itf = rtnArea.Substring(7, 3);
                    rtn.sys_date = rtnArea.Substring(10, 8);
                    rtn.trns_no = rtnArea.Substring(18, 18);
                    rtn.rtn_code = rtnArea.Substring(36, 1);
                }
                catch (Exception e) {
                    logger.Error(e.ToString());
                    throw e;
                }
                

                return rtn;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }
    }
}