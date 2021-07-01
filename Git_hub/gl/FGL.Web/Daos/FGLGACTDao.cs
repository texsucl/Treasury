
using FGL.Web.AS400Models;
using FGL.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLGACTDao 會計科目轉換檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLGACTDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public void proc(string execAction, List<FGLGACT0Model> aactList, EacConnection conn, EacTransaction transaction)
        {
            if ("D".Equals(execAction))
                procInvalidate(aactList, conn, transaction);
            else
                procEffective(aactList, conn, transaction);
            
        }

        /// <summary>
        /// 處理新增
        /// </summary>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGLGACT0Model procData, EacConnection conn, EacTransaction transaction) {
            string strSQL = "";
            string strSQLI = "";
            strSQLI = @"insert into FGLGACT0 
             (ACT_NUM, SQL_ACTNUM, FIELD10_1, FIELD10_2, FIELD10_3, FIELD08_1, FIELD08_2, FIELD08_3 
            , ENTRY_ID, ENTRY_DATE, ENTRY_TIME, UPD_ID, UPD_DATE, UPD_TIME, ACT_NAME, SQL_ACTNM) 
             VALUES 
             (:ACT_NUM, :SQL_ACTNUM, :FIELD10_1, :FIELD10_2, :FIELD10_3, :FIELD08_1, :FIELD08_2, :FIELD08_3 
            , :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME, :UPD_ID, :UPD_DATE, :UPD_TIME ";

            EacCommand cmdQ = new EacCommand();
            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            string strSQLNm = "";

            string actName = StringUtil.toString(procData.actName);
            string sqlActnm = StringUtil.toString(procData.sqlActnm);

            actName = (actName.Length > 20 ? actName.Substring(0, 20) : actName);
            sqlActnm = (sqlActnm.Length > 20 ? sqlActnm.Substring(0, 20) : sqlActnm);

            strSQLNm += " ,'" + actName + "' ";
            strSQLNm += " ,'" + sqlActnm + "' ";
            strSQLNm += " ) ";
            cmd.CommandText = "";
            strSQL = strSQLI + strSQLNm;
            cmd.CommandText = strSQL;

            cmd.Parameters.Clear();

            cmd.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));
            //cmd.Parameters.Add("ACT_NAME", StringUtil.toString(procData.actName));
            cmd.Parameters.Add("SQL_ACTNUM", StringUtil.toString(procData.sqlActnum));
            //cmd.Parameters.Add("SQL_ACTNM", StringUtil.toString(procData.sqlActnm));
            cmd.Parameters.Add("FIELD10_1", StringUtil.toString(procData.field101));
            cmd.Parameters.Add("FIELD10_2", StringUtil.toString(procData.field102));
            cmd.Parameters.Add("FIELD10_3", StringUtil.toString(procData.field103));
            cmd.Parameters.Add("FIELD08_1", StringUtil.toString(procData.field081));
            cmd.Parameters.Add("FIELD08_2", StringUtil.toString(procData.field082));
            cmd.Parameters.Add("FIELD08_3", StringUtil.toString(procData.field083));
            cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(procData.entryId));
            cmd.Parameters.Add("ENTRY_DATE", StringUtil.toString(procData.entryDate));
            cmd.Parameters.Add("ENTRY_TIME", StringUtil.toString(procData.entryTime));
            cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.entryId));
            cmd.Parameters.Add("UPD_DATE", StringUtil.toString(procData.entryDate));
            cmd.Parameters.Add("UPD_TIME", StringUtil.toString(procData.entryTime));

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }



        public void update(FGLGACT0Model procData, EacConnection conn, EacTransaction transaction)
        {
            string strSQL = "";
            string strSQLU = "";
            strSQLU = @"UPDATE FGLGACT0 SET 
  SQL_ACTNUM = :SQL_ACTNUM
, UPD_ID = :UPD_ID
, UPD_DATE = :UPD_DATE
, UPD_TIME = :UPD_TIME";

            string strSQLNm = "";

            string actName = StringUtil.toString(procData.actName);
            string sqlActnm = StringUtil.toString(procData.sqlActnm);

            actName = (actName.Length > 20 ? actName.Substring(0, 20) : actName);
            sqlActnm = (sqlActnm.Length > 20 ? sqlActnm.Substring(0, 20) : sqlActnm);

            strSQLNm += ", ACT_NAME = '" + actName + "' ";
            strSQLNm += ", SQL_ACTNM = '" + sqlActnm + "' ";


            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;


            cmd.CommandText = "";
            strSQL = strSQLU + strSQLNm + " WHERE ACT_NUM = :ACT_NUM";
            cmd.CommandText = strSQL;

            cmd.Parameters.Clear();
            cmd.Parameters.Add("SQL_ACTNUM", StringUtil.toString(procData.sqlActnum));
            cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.entryId));
            cmd.Parameters.Add("UPD_DATE", StringUtil.toString(procData.entryDate));
            cmd.Parameters.Add("UPD_TIME", StringUtil.toString(procData.entryTime));
            cmd.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));
          
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }




        /// <summary>
        /// 處理有效資料
        /// </summary>
        /// <param name="aactList"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procEffective(List<FGLGACT0Model>aactList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procEffective FGLGACT0 begin!");
            bool bExist = false;
            
            
            EacCommand cmdQ = new EacCommand();


            string strSQLQ = @"SELECT ACT_NUM FROM LGLGACT1 WHERE ACT_NUM = :ACT_NUM";

            try
            {
                
                cmdQ.Connection = conn;
                cmdQ.Transaction = transaction;
                cmdQ.CommandText = strSQLQ;

                foreach (FGLGACT0Model procData in aactList) {
                    bExist = false;

                    cmdQ.Parameters.Clear();
                    cmdQ.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));

                    DbDataReader result = cmdQ.ExecuteReader();

                    while (result.Read())
                    {
                        bExist = true;
                        break;
                    }

                    if (!bExist)
                        insert(procData, conn, transaction);
                    else
                        update(procData, conn, transaction);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("procEffective FGLGACT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 處理刪除
        /// </summary>
        /// <param name="aactList"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procInvalidate(List<FGLGACT0Model> aactList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procInvalidate FGLGACT0 begin!");
            
            EacCommand cmd = new EacCommand();

            string strSQL = @"DELETE LGLGACT1 WHERE ACT_NUM = :ACT_NUM";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FGLGACT0Model procData in aactList)
                {
                    logger.Info("ACT_NUM:" + procData.actNum);

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("procInvalidate FGLGACT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



    }
}