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
    public class FRTBPGHDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        


        /// <summary>
        /// 依條件查詢待覆核的資料
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        public List<ORTB001Model> qryForSTAT1(string sysType, string srceFrom, string srceKind, string srcePgm)
        {

            logger.Info("sysType:" + sysType);
            logger.Info("srceFrom:" + srceFrom);
            logger.Info("srceKind:" + srceKind);
            logger.Info("srcePgm:" + srcePgm);

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB001Model> rows = new List<ORTB001Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT APPLY_NO, SYS_TYPE, SRCE_FROM, SRCE_KIND, SRCE_PGM, STATUS, UPD_ID, UPD_DATE, UPD_TIME " + 
                            " FROM LRTBPGH1 " + 
                           " WHERE 1 = 1 AND APPR_STAT = '1'";

                if (!"".Equals(StringUtil.toString(sysType)))
                {
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

                logger.Info("strSQL:" + strSQL);
                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int aplyNoId = result.GetOrdinal("APPLY_NO");
                int sysTypeId = result.GetOrdinal("SYS_TYPE");
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int srceKindId = result.GetOrdinal("SRCE_KIND");
                int srcePgmId = result.GetOrdinal("SRCE_PGM");
                int status = result.GetOrdinal("STATUS");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");

                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB001Model d = new ORTB001Model();
                    d.tempId = StringUtil.toString(result.GetString(aplyNoId)) + "|" 
                        + StringUtil.toString(result.GetString(sysTypeId)) + "|" 
                        + StringUtil.toString(result.GetString(srceFromId)) + "|" 
                        + StringUtil.toString(result.GetString(srceKindId)) + "|" 
                        + StringUtil.toString(result.GetString(srcePgmId));
                    d.aplyNo = StringUtil.toString(result.GetString(aplyNoId));
                    d.sysType = StringUtil.toString(result.GetString(sysTypeId));
                    d.srceFrom = StringUtil.toString(result.GetString(srceFromId));
                    d.srceKind = StringUtil.toString(result.GetString(srceKindId));
                    d.srcePgm = StringUtil.toString(result.GetString(srcePgmId));
                    d.status = StringUtil.toString(result.GetString(status));
                    d.statusDesc = d.status == "A" ? "新增" : "刪除";
                    d.updId = StringUtil.toString(result.GetString(updId));
                    d.updDate = result[updDateId].ToString();
                    d.updTime = result[updTimeId].ToString();
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


        /// <summary>
        /// 查詢-判斷快速付款維護覆核作業
        /// </summary>
        /// <returns></returns>
        public List<ORTB001Model> qryForORTB001A()
        {
            return qryForSTAT1("", "", "", "");
        }


        /// <summary>
        /// 新增"FRTBPGH0 快速付款來源程式異動檔"
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="procData"></param>
        /// <returns></returns>
        public int insertFRTBPGH0(string applyNo, List<ORTB001Model> procData)
        {
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB001Model> rows = new List<ORTB001Model>();

            string strSQL = "";
            strSQL += "insert into LRTBPGH1 ";
            strSQL += " (APPLY_NO, SYS_TYPE, SRCE_FROM, SRCE_KIND, SRCE_PGM, STATUS, APPR_STAT, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:APPLY_NO, :SYS_TYPE, :SRCE_FROM, :SRCE_KIND, :SRCE_PGM, :STATUS, :APPR_STAT, :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = strSQL;

                foreach (ORTB001Model d in procData) {
                    if (!"".Equals(d.status)) {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("APPLY_NO", applyNo);
                        cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(d.sysType));
                        cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(d.srceFrom));
                        cmd.Parameters.Add("SRCE_KIND", StringUtil.toString(d.srceKind));
                        cmd.Parameters.Add("SRCE_PGM", StringUtil.toString(d.srcePgm));
                        cmd.Parameters.Add("STATUS", StringUtil.toString(d.status));
                        cmd.Parameters.Add("APPR_STAT", "1");
                        cmd.Parameters.Add("UPD_ID", StringUtil.toString(d.updId));
                        cmd.Parameters.Add("UPD_DATE", nowStr[0]);
                        cmd.Parameters.Add("UPD_TIME", nowStr[1]);

                        cmd.ExecuteNonQuery();
                        execCnt++;
                    }
                }

               

                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return execCnt;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;

            }

        }



        /// <summary>
        /// 異動"FRTBPGH0 快速付款來源程式異動檔"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateFRTBPGH0(string apprId, string apprStat, List<ORTB001Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBPGH0 begin! apprStat = " + apprStat);
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();

            List<ORTB001Model> rows = new List<ORTB001Model>();

            string strSQL = "";
            strSQL += "UPDATE LRTBPGH1 " +
                      "  SET APPR_STAT = :APPR_STAT " +
                          " ,APPR_ID = :APPR_ID  " +
                    " ,APPR_DATE = :APPR_DATE  " +
                    " ,APPR_TIME = :APPR_TIME  " +
                    " WHERE  1 = 1 " +
                    " AND APPLY_NO = :APPLY_NO " +
                    " AND SYS_TYPE = :SYS_TYPE " +
                    " AND SRCE_FROM = :SRCE_FROM " +
                    " AND SRCE_KIND = :SRCE_KIND " +
                    " AND SRCE_PGM = :SRCE_PGM ";

            logger.Info("strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (ORTB001Model d in procData)
                {
                    cmd.Parameters.Clear();

                    logger.Info("APPLY_NO = " + d.aplyNo);
                    logger.Info("SYS_TYPE = " + d.sysType);
                    logger.Info("SRCE_FROM = " + d.srceFrom);
                    logger.Info("SRCE_KIND = " + d.srceKind);
                    logger.Info("SRCE_PGM = " + d.srcePgm);

                    logger.Info("APPR_STAT = " + apprStat);
                    logger.Info("APPR_ID = " + apprId);


                    cmd.Parameters.Add("APPR_STAT", apprStat);
                    cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                    cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                    cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                    cmd.Parameters.Add("APPLY_NO", StringUtil.toString(d.aplyNo));
                    cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(d.sysType));
                    cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(d.srceFrom));
                    cmd.Parameters.Add("SRCE_KIND", StringUtil.toString(d.srceKind));
                    cmd.Parameters.Add("SRCE_PGM", StringUtil.toString(d.srcePgm));
                    cmd.ExecuteNonQuery();
                    execCnt++;


                }

                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBPGH0 end! ");
                return execCnt;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;

            }

        }

    }
}