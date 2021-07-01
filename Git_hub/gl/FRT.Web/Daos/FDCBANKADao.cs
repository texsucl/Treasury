using FRT.Web.AS400Models;
using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FDCBANKADao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 以"銀行代號"查詢銀行檔
        /// </summary>
        /// <param name="bankNo"></param>
        /// <returns></returns>
        public FDCBANKAModel qryByBankNo(string bankNo)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            FDCBANKAModel d = new FDCBANKAModel();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT BANK_NO, BANK_NAME, SHR_BQ " +
                    " FROM LDCBANKA1 " +
                    " WHERE BANK_NO = :BANK_NO " +
                    " ORDER BY BANK_NO" ;

                cmd.Parameters.Add("BANK_NO", bankNo);

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int bankNoId = result.GetOrdinal("BANK_NO");
                int bankNameId = result.GetOrdinal("BANK_NAME");
                int shrBqId = result.GetOrdinal("SHR_BQ");

                while (result.Read())
                {
                    d.bankNo = StringUtil.toString(result.GetString(bankNoId));
                    d.bankName = StringUtil.toString(result.GetString(bankNameId));
                    d.shrBq = StringUtil.toString(result.GetString(shrBqId));
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return d;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

        /// <summary>
        /// CODE TABLE維護BKMSG_AR的時候要同步寫回FDCBANKA0裡的REMARK_AR的欄位上Y，如果拿掉Y時，也要同時異動FDCBANKA0裡的欄位=空白
        /// </summary>
        /// <param name="bankNo"></param>
        /// <param name="remarkAr"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updRemarkAr(string bankNo, string remarkAr, EacConnection conn, EacTransaction transaction)
        {
            string strSQL = "";
            strSQL += @"UPDATE LDCBANKA1 
                        SET REMARK_AR = :REMARK_AR
                        WHERE BANK_NO = :BANK_NO";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("REMARK_AR", remarkAr);
            cmd.Parameters.Add("BANK_NO", bankNo);
            

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }
    }
}