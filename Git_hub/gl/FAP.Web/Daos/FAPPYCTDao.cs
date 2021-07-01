

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPPYCT0   應付票據一年內未兌現清單檔 
/// ------------------------------------------
/// add by daiyu 20201005
/// 需求單號：201910290100-01
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPPYCTDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public List<FAP_TEL_SMS_TEMP> qryFAPPYCT0(EacConnection conn400)
        {
            logger.Info("qryFAPPYCT0 begin!");
            List<FAP_TEL_SMS_TEMP> dataList = new List<FAP_TEL_SMS_TEMP>();
            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT 
CHECK_NO,
CHECK_SHRT,
CHECK_DATE,
AMOUNT,
O_PAID_CD,
PAID_ID,
PAID_NAME,
SYSTEM,
POLICY_NO,
POLICY_SEQ,
ID_DUP,
APPL_ID,
APPL_NAME,
INS_ID,
INS_NAME,
SEC_STAT
FROM FAPPYCT0 
";
            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    FAP_TEL_SMS_TEMP d = new FAP_TEL_SMS_TEMP();

                    d.tel_std_aply_no = "";
                    d.check_no = StringUtil.toString(result["CHECK_NO"].ToString());
                    d.check_acct_short = StringUtil.toString(result["CHECK_SHRT"].ToString());

                    string check_date = StringUtil.toString(result["CHECK_DATE"]?.ToString());
                    if (!"".Equals(check_date)) {
                        try
                        {
                            d.check_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(check_date.PadLeft(7, '0')));
                        }
                        catch (Exception e) { 
                        }
                    }
                        

                    string check_amt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    if (!"".Equals(check_amt)) {
                        try
                        {
                            d.check_amt = Convert.ToDecimal(check_amt);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                       

                    d.o_paid_cd = result["O_PAID_CD"]?.ToString()?.Trim();
                    d.paid_id = result["PAID_ID"]?.ToString()?.Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString()?.Trim();
                    d.system = result["SYSTEM"]?.ToString()?.Trim();
                    d.policy_no = result["POLICY_NO"]?.ToString()?.Trim();

                    string policy_seq = StringUtil.toString(result["POLICY_SEQ"]?.ToString());
                    if (!"".Equals(policy_seq)) {
                        try
                        {
                            d.policy_seq = Convert.ToInt16(policy_seq);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                        

                    d.id_dup = result["ID_DUP "]?.ToString()?.Trim();
                    d.appl_id = result["APPL_ID "]?.ToString()?.Trim();
                    d.appl_name = result["APPL_NAME"]?.ToString()?.Trim();
                    d.ins_id = result["INS_ID"]?.ToString()?.Trim();
                    d.ins_name = result["INS_NAME"]?.ToString()?.Trim();
                    d.sec_stat = result["SEC_STAT"]?.ToString()?.Trim();

                    dataList.Add(d);
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryFAPPYCT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
            return dataList;
        }


    }
}