using FAP.Web.AS400Models;
using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// <summary>
/// SAP7018 取得保單聯絡電話
/// </summary>
namespace FAP.Web.AS400PGM
{
    public class SAP7018Util
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<SAP7018TelModel> callSAP7018(EacConnection con, SAP7018Model model)
        {
            logger.Info("callSAP7018==>" + model.system + "|" + model.policy_no + "|" + model.policy_seq + "|" + model.id_dup);

            List<SAP7018TelModel> telList = new List<SAP7018TelModel>();

            try
            {
                EacCommand cmd = new EacCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SAP7018";


                cmd.Parameters.Clear();

                EacParameter area1 = new EacParameter();
                area1.ParameterName = "LINK-AREA1";
                area1.DbType = DbType.String;
                area1.Size = 1695;
                area1.Direction = ParameterDirection.InputOutput;
                area1.Value = (StringUtil.toString(model.system).PadRight(1, ' ') +
                    StringUtil.toString(model.policy_no).PadRight(10, ' ') +
                    StringUtil.toString(model.policy_seq).PadLeft(2, '0') +
                    StringUtil.toString(model.id_dup).PadRight(1, ' ')).PadRight(1695, ' ');

                EacParameter area2 = new EacParameter();
                area2.ParameterName = "LINK-AREA2";
                area2.DbType = DbType.String;
                area2.Size = 207;
                area2.Direction = ParameterDirection.InputOutput;
                area2.Value = "".PadRight(207, ' ');

                cmd.Parameters.Add(area1);
                cmd.Parameters.Add(area2);

                cmd.Prepare();
                cmd.ExecuteNonQuery();

                string rtnArea1 = cmd.Parameters["LINK-AREA1"].Value.ToString();
                string rtnArea2 = cmd.Parameters["LINK-AREA2"].Value.ToString();

                try
                {
                    model.system = rtnArea1.Substring(0, 1);
                    model.policy_no = rtnArea1.Substring(1, 10);
                    model.policy_seq = rtnArea1.Substring(11, 2);
                    model.id_dup = rtnArea1.Substring(13, 1);
                    model.rtn_code = rtnArea1.Substring(14, 1);
                  

                    logger.Info("rtn_code:" + StringUtil.toString(model.rtn_code));

                    if ("2".Equals(model.rtn_code)) {
                        model.tel = rtnArea1.Substring(15);
                        int telLen = 56;
                        int cnt = Convert.ToInt16(StringUtil.toString(model.tel).Length) / telLen;

                        for (int i = 1; i <= cnt; i++) {
                            string strTel = model.tel.Substring((56 * (i - 1)), 56);

                            SAP7018TelModel telModel = new SAP7018TelModel();

                            telModel.tel_type = strTel.Substring(0, 1);
                            telModel.tel = strTel.Substring(1, 40);
                            telModel.upd_date = strTel.Substring(41, 15);
                            telModel.address = StringUtil.toString(rtnArea2);
                            telList.Add(telModel);
                        }
                    }

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

            

            return telList;
        }
    }
}