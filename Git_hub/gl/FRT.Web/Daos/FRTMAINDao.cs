using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTMAINDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// ORT0104【申請覆核】前請檢核該處理序號需存於匯款主檔(MAIN)的PRO_NO(處理序號) 中
        /// </summary>
        /// <param name="pro_no"></param>
        /// <returns></returns>
        public string chkForORT0104(string pro_no)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            string _pro_no = "";
            try
            {

                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT PRO_NO " +
                    " from LRTMAIN1 " +
                    " where pro_no = :pro_no " +
                    " fetch first 1 rows only  ";

                cmd.Parameters.Add("pro_no", StringUtil.toString(pro_no));


                //logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    _pro_no = result["PRO_NO"]?.ToString().Trim();
                    break;
                }



                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return _pro_no;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



    }
}