using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400PGM
{
    public class SGLGTBAUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<string, string> callSGLGTBAUtil(EacConnection con
            , string productType, string itemCon, string corpNo)
        {
            Dictionary<string, string> sglgtba = new Dictionary<string, string>();

            try
            {
                EacCommand cmd = new EacCommand();
                cmd.Connection = con;

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SGLGTBA";
                cmd.Parameters.Clear();

                EacParameter para = new EacParameter();
                para.ParameterName = "LINK-AREA";
                para.DbType = DbType.String;
                para.Size = 88;
                para.Direction = ParameterDirection.InputOutput;
                para.Value = (productType + itemCon + corpNo).PadRight(88, ' ');

                cmd.Parameters.Add(para);
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                string rtnStr = cmd.Parameters["LINK-AREA"].Value.ToString();

                if (!"N".Equals(rtnStr.Substring(3, 1)))
                {
                    int iSmp = 0;

                    while ((iSmp * 4) <= (rtnStr.Length - 1))
                    {
                        switch (iSmp)
                        {
                            case 0:
                                break;
                            case 1:
                                sglgtba.Add("acctCode", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 2:
                                sglgtba.Add("acctCodef", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 3:
                                sglgtba.Add("acctCoder", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 4:
                                sglgtba.Add("acctCodes", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 5:
                                sglgtba.Add("acctCodeg", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 6:
                                sglgtba.Add("acctCodet", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 7:
                                sglgtba.Add("coiCode", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 8:
                                sglgtba.Add("coiCodef", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 9:
                                sglgtba.Add("coiCoder", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 10:
                                sglgtba.Add("acctCodei", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 11:
                                sglgtba.Add("comuCodef", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 12:
                                sglgtba.Add("comuCoder", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 13:
                                sglgtba.Add("acctCodeo", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 14:
                                sglgtba.Add("comuPayf", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 15:
                                sglgtba.Add("comuPayr", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 16:
                                sglgtba.Add("acct4570", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 17:
                                sglgtba.Add("acct4571", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 18:
                                sglgtba.Add("acct4572", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 19:
                                sglgtba.Add("acct4573", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 20:
                                sglgtba.Add("acct4574", rtnStr.Substring((iSmp * 4), 4));
                                break;
                            case 21:
                                sglgtba.Add("acct4575", rtnStr.Substring((iSmp * 4), 4));
                                break;
                        }
                        iSmp++;
                    }
                }


                cmd.Dispose();

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return sglgtba;
        }


    }
}