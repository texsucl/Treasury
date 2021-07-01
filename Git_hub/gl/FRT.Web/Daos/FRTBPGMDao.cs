using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTBPGMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢-判斷快速付款維護作業
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        public List<ORTB001Model> qryForORTB001(string sysType, string srceFrom, string srceKind, string srcePgm)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB001Model> rows = new List<ORTB001Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT SYS_TYPE, SRCE_FROM, SRCE_KIND, SRCE_PGM FROM LRTBPGM1 WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(sysType))) {
                    strSQL += " AND SYS_TYPE = :SYS_TYPE";
                    cmd.Parameters.Add("SYS_TYPE", sysType);
                }

                if (!"".Equals(StringUtil.toString(srceFrom)))
                {
                    strSQL += " AND SRCE_FROM = :SRCE_FROM";
                    cmd.Parameters.Add("SRCE_FROM", srceFrom);
                }

                if (!"".Equals(StringUtil.toString(srceKind)))
                {
                    strSQL += " AND SRCE_KIND = :SRCE_KIND";
                    cmd.Parameters.Add("SRCE_KIND", srceKind);
                }

                if (!"".Equals(StringUtil.toString(srcePgm)))
                {
                    strSQL += " AND SRCE_PGM = :SRCE_PGM";
                    cmd.Parameters.Add("SRCE_PGM", srcePgm);
                }

                //strSQL += StringUtil.toString(sysType) == "" ? "" : " AND SYS_TYPE = :SYS_TYPE";
                //strSQL += StringUtil.toString(srceFrom) == "" ? "" : " AND SRCE_FROM = :SRCE_FROM";
                //strSQL += StringUtil.toString(srceKind) == "" ? "" : " AND SRCE_KIND = :SRCE_KIND";
                //strSQL += StringUtil.toString(srcePgm) == "" ? "" : " AND SRCE_PGM = :SRCE_PGM";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;
                //cmd.Parameters.Add("SYS_TYPE", DbType.String, sysType);
                //cmd.Parameters.Add("SRCE_FROM", DbType.String, srceFrom);
                //cmd.Parameters.Add("SRCE_KIND", DbType.String, srceKind);
                //cmd.Parameters.Add("SRCE_PGM", DbType.String, srcePgm);


                DbDataReader result = cmd.ExecuteReader();
                int sysTypeId = result.GetOrdinal("SYS_TYPE");
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int srceKindId = result.GetOrdinal("SRCE_KIND");
                int srcePgmId = result.GetOrdinal("SRCE_PGM");


                while (result.Read())
                {
                    ORTB001Model d = new ORTB001Model();
                    string tempId = StringUtil.toString(result.GetString(sysTypeId))
                        + StringUtil.toString(result.GetString(srceFromId))
                        + StringUtil.toString(result.GetString(srceKindId))
                        + StringUtil.toString(result.GetString(srcePgmId));
                    d.tempId = tempId;
                    d.sysType = StringUtil.toString(result.GetString(sysTypeId));
                    d.srceFrom = StringUtil.toString(result.GetString(srceFromId));
                    d.srceKind = StringUtil.toString(result.GetString(srceKindId));
                    d.srcePgm = StringUtil.toString(result.GetString(srcePgmId));
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
                throw (e);

            }

        }

        /// <summary>
        /// 判斷快速付款維護覆核作業"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public void apprFRTBPGM0(string apprId, List<ORTB001Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTBPGM0 begin!");

            
                foreach (ORTB001Model d in procData)
                {
                    if ("A".Equals(d.status))
                        insertFRTBPGM0(apprId, d, conn, transaction);
                    else 
                        deleteFRTBPGM0(apprId, d, conn, transaction);
                }

                logger.Info("apprFRTBPGM0 end!");
          
          

        }


        /// <summary>
        /// 新增"FRTBPGM0  快速付款來源程式資料檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insertFRTBPGM0(string apprId, ORTB001Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFRTBPGM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI += "insert into LRTBPGM1 ";
            strSQLI += " (SYS_TYPE, SRCE_FROM, SRCE_KIND, SRCE_PGM, UPD_ID, UPD_DATE, UPD_TIME, APPR_ID, APPR_DATE, APPR_TIME) ";
            strSQLI += " VALUES ";
            strSQLI += " (:SYS_TYPE, :SRCE_FROM, :SRCE_KIND, :SRCE_PGM, :UPD_ID, :UPD_DATE, :UPD_TIME, :APPR_ID, :APPR_DATE, :APPR_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(procData.sysType));
                cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(procData.srceFrom));
                cmd.Parameters.Add("SRCE_KIND", StringUtil.toString(procData.srceKind));
                cmd.Parameters.Add("SRCE_PGM", StringUtil.toString(procData.srcePgm));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                cmd.CommandText = strSQLI;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("insertFRTBPGM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 刪除"FRTBPGM0  快速付款來源程式資料檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void deleteFRTBPGM0(string apprId, ORTB001Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("deleteFRTBPGM0 begin!");

            EacCommand cmd = new EacCommand();


            string strSQLD = "";
            strSQLD += "DELETE LRTBPGM1 " +
                        " WHERE  1 = 1 " +
                        " AND SYS_TYPE = :SYS_TYPE " +
                        " AND SRCE_FROM = :SRCE_FROM " +
                        " AND SRCE_KIND = :SRCE_KIND " +
                        " AND SRCE_PGM = :SRCE_PGM ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;


                cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(procData.sysType));
                cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(procData.srceFrom));
                cmd.Parameters.Add("SRCE_KIND", StringUtil.toString(procData.srceKind));
                cmd.Parameters.Add("SRCE_PGM", StringUtil.toString(procData.srcePgm));

                cmd.CommandText = strSQLD;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("deleteFRTBPGM0 end!");
       
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}