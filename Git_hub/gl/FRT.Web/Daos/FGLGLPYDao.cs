using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FLGLGLPYDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

/// <summary>
/// 以"憑證編號+來源"查詢
/// </summary>
/// <param name="tempNo"></param>
/// <param name="srceFrom"></param>
/// <returns></returns>
        public List<ORTB005Model> qryForORTB005Summary(List<ORTB005Model> rows)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT DISTINCT SQL_NO " + 
                    " FROM LGLGLPY7 " +
                    " WHERE 1=1 " +
                    " AND TEMP_NO = :TEMP_NO" +
                    " AND (SRCE_PGM = 'SRT1813' OR SRCE_PGM = 'PRT1813')";



                foreach (ORTB005Model d in rows)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("TEMP_NO", d.vhrNo1);
                    

                    DbDataReader result = cmd.ExecuteReader();

                    while (result.Read())
                    {
                        d.sqlNo = result[0].ToString();

                    }
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