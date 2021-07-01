
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// PMCODEDao 相關代碼檔
/// -----------------------------------------
/// 修改歷程：20200730   daiyu
/// 需求單號：202007230106-02
/// 修改內容：增加"新增"、"修改"、"刪除"
/// </summary>
namespace FGL.Web.Daos
{
    public class FPMCODEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public void delete(FPMCODEModel procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delete FPMCODE0 begin!");
            logger.Info("GROUP_ID:" + procData.groupId);
            logger.Info("SRCE_FROM:" + procData.srce_from);
            logger.Info("REF_NO:" + procData.refNo);


            EacCommand cmd = new EacCommand();
            string strSQL = @"
DELETE LP_FBDB/FPMCODE0 
 WHERE GROUP_ID = :GROUP_ID
  AND SRCE_FROM = :SRCE_FROM
  AND REF_NO = :REF_NO";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();


                cmd.Parameters.Add("GROUP_ID", StringUtil.toString(procData.groupId));
                cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(procData.srce_from));
                cmd.Parameters.Add("REF_NO", StringUtil.toString(procData.refNo));

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("FPMCODE0 FGLITEM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }
        
        public void update(FPMCODEModel procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("update FPMCODE0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI = @"
UPDATE FPMCODE0 SET
  TEXT = :TEXT
, UPD_YY = :UPD_YY
, UPD_MM = :UPD_MM
, UPD_DD = :UPD_DD
, UPD_ID = :UPD_ID
  WHERE GROUP_ID = :GROUP_ID
    AND SRCE_FROM = :SRCE_FROM
    AND REF_NO = :REF_NO";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQLI;

                cmd.Parameters.Clear();


                cmd.Parameters.Add("TEXT", StringUtil.toString(procData.text));
                cmd.Parameters.Add("UPD_YY", StringUtil.toString(procData.updYy));
                cmd.Parameters.Add("UPD_MM", StringUtil.toString(procData.updMm));
                cmd.Parameters.Add("UPD_DD", StringUtil.toString(procData.updDd));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));

                cmd.Parameters.Add("GROUP_ID", StringUtil.toString(procData.groupId));
                cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(procData.srce_from));
                cmd.Parameters.Add("REF_NO", StringUtil.toString(procData.refNo));
                

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("update FPMCODE0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public void insert(FPMCODEModel procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FPMCODE0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI = @"insert into FPMCODE0 
 (GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM
, ENTRY_YY, ENTRY_MM, ENTRY_DD, ENTRY_TIME, ENTRY_ID
) 
 VALUES 
 (:GROUP_ID, :TEXT_LEN, :REF_NO, :TEXT, :SRCE_FROM
, :ENTRY_YY, :ENTRY_MM, :ENTRY_DD, :ENTRY_TIME, :ENTRY_ID) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("GROUP_ID", StringUtil.toString(procData.groupId));
                cmd.Parameters.Add("TEXT_LEN", StringUtil.toString(procData.textLen));
                cmd.Parameters.Add("REF_NO", StringUtil.toString(procData.refNo));
                cmd.Parameters.Add("TEXT", StringUtil.toString(procData.text));
                cmd.Parameters.Add("SRCE_FROM", StringUtil.toString(procData.srce_from));
                cmd.Parameters.Add("ENTRY_YY", StringUtil.toString(procData.entryYy));
                cmd.Parameters.Add("ENTRY_MM", StringUtil.toString(procData.entryMm));
                cmd.Parameters.Add("ENTRY_DD", StringUtil.toString(procData.entryDd));
                cmd.Parameters.Add("ENTRY_TIME", StringUtil.toString(procData.entryTime));
                cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(procData.entryId));
                

                cmd.CommandText = strSQLI;

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("insert FPMCODE0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 查詢FPMCODE
        /// </summary>
        /// <param name="bankNo"></param>
        /// <returns></returns>
        public List<FPMCODEModel> qryFPMCODE(string groupId, string srceFrom, string refNo, EacConnection con)
        {
            EacCommand cmd = new EacCommand();

            List<FPMCODEModel> rows = new List<FPMCODEModel>();

            string strSQL = "";
            try
            {
                cmd.Connection = con;
                strSQL += "SELECT GROUP_ID, SRCE_FROM, REF_NO, TEXT " +
                    " FROM LP_FBDB/FPMCODE0 " +
                    " WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(groupId)))
                {
                    strSQL += " AND GROUP_ID = :GROUP_ID";
                    cmd.Parameters.Add("GROUP_ID", groupId);
                }

                if (!"".Equals(StringUtil.toString(srceFrom)))
                {
                    strSQL += " AND SRCE_FROM = :SRCE_FROM";
                    cmd.Parameters.Add("SRCE_FROM", srceFrom);
                }

                if (!"".Equals(StringUtil.toString(refNo)))
                {
                    strSQL += " AND REF_NO = :REF_NO";
                    cmd.Parameters.Add("REF_NO", refNo);
                }

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();


                while (result.Read())
                {
                    FPMCODEModel d = new FPMCODEModel();
                    d.groupId = result["GROUP_ID"]?.ToString();
                    d.srce_from = result["SRCE_FROM"]?.ToString();
                    d.refNo = result["REF_NO"]?.ToString();
                    d.text = result["TEXT"]?.ToString();
                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;

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