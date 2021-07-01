
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;


namespace FGL.Web.Daos
{
    public class FGLSMPLDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// "科目樣本險種類別維護作業"檢核
        /// </summary>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        public bool chkSmpNumForOGL10181(string smpNum)
        {
            bool bExist = false;
            logger.Info("chkSmpNumForOGL10181 begin!!");
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = "SELECT SMP_NUM,SMP_NAME FROM lp_fbdb/LGLSMPL1 where SMP_NUM = :SMP_NUM";

                cmd.Parameters.Add("SMP_NUM", smpNum);


                DbDataReader result = cmd.ExecuteReader();

                bExist = result.HasRows;


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());

            }

            logger.Info("chkSmpNumForOGL10181 end");
            return bExist;

        }

        /// <summary>
        /// 會計接收作業查詢
        /// </summary>
        /// <param name="smpMap"></param>
        /// <returns></returns>
        public Dictionary<string, OGL00003DModel> qryForOGL00005(Dictionary<string, OGL00003DModel> smpMap)
        {
            ValidateUtil validateUtil = new ValidateUtil();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT SMP_NAME, SMP_SHORT, NAME_CNT" +
                    " FROM LGLSMPL1 " +
                    " WHERE 1=1 " +
                    " AND SMP_NUM = :SMP_NUM";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;


                foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("SMP_NUM", item.Value.smpNum);

                    DbDataReader result = cmd.ExecuteReader();

                    while (result.Read())
                    {
                        item.Value.smpName = result["SMP_NAME"]?.ToString();
                        //item.Value.smpNameShort = result["SMP_SHORT"]?.ToString() ;
                        item.Value.smpNameCnt = result["NAME_CNT"]?.ToString();
                        item.Value.smpNameShort = result["SMP_SHORT"]?.ToString() + StringUtil.halfToFull(item.Value.item);
                        break;
                    }
                }

                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return smpMap;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }
        
        
        
        

    }
}