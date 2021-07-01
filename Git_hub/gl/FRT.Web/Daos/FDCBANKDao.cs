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
    public class FDCBANKDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 以"銀行代號"查詢銀行檔
        /// </summary>
        /// <param name="bankNo"></param>
        /// <returns></returns>
        public List<FDCBANKModel> qryByBankNo(string bankNo)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FDCBANKModel> rows = new List<FDCBANKModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT BANK_NO, BANK_NAME, SHR_BQ " +
                    " FROM LDCBANK1 " +
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
                    FDCBANKModel d = new FDCBANKModel();
                    d.bankNo = StringUtil.toString(result.GetString(bankNoId));
                    d.bankName = StringUtil.toString(result.GetString(bankNameId));
                    d.shrBq = StringUtil.toString(result.GetString(shrBqId));
                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return rows;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}