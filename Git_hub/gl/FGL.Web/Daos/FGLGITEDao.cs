
using FGL.Web.AS400Models;
using FGL.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLGITE0 會計商品資訊檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLGITEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 檢查"保險商品編號"是否重複
        /// </summary>
        /// <param name="prodNo"></param>
        /// <returns></returns>
        public string qryByProdNo(string prodNo, string item)
        {
            logger.Info("qryByProdNo begin!");
            string dupItem = "";

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();
                EacCommand cmdQ = new EacCommand();
                string strSQLQ = @"
SELECT ITEM 
  FROM LGLGITE2 
    WHERE ITEM <> :ITEM AND NUM = :NUM";

                try
                {
                    cmdQ.Connection = conn400;
                    cmdQ.CommandText = strSQLQ;

                    cmdQ.Parameters.Clear();
                    
                    cmdQ.Parameters.Add("ITEM", item);
                    cmdQ.Parameters.Add("NUM", prodNo);

                    DbDataReader result = cmdQ.ExecuteReader();

                    while (result.Read())
                    {
                        dupItem = result["ITEM"]?.ToString();
                     
                    }


                    cmdQ.Dispose();
                    cmdQ = null;

                    logger.Info("qryByProdNo end!");

                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    throw e;
                }

            }

            return dupItem;

        }



        /// <summary>
        /// "OGL00005A 會計接收"後，回寫AS400
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void proc(string execAction, List<FGLGITE0Model> procData, EacConnection conn, EacTransaction transaction)
        {

            if ("D".Equals(execAction))
                procInvalidate(procData, conn, transaction);
            else
                procEffective(procData, conn, transaction);

        }


        public void procInvalidate(List<FGLGITE0Model> aactList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procInvalidate LGLGITE2 begin!");

            EacCommand cmd = new EacCommand();

            string strSQL = @"DELETE LGLGITE2 WHERE ITEM = :ITEM AND SYS_TYPE = :SYS_TYPE";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FGLGITE0Model procData in aactList)
                {
                    logger.Info("ITEM:" + procData.item);
                    logger.Info("SYS_TYPE:" + procData.sysType);

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                    cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(procData.sysType));

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("procInvalidate LGLGITE2 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void procEffective(List<FGLGITE0Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procEffective FGLGITE begin!");
            bool bASys = false;
            bool bFSys = false;

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT SYS_TYPE, ITEM 
  FROM LGLGITE2 
    WHERE ITEM = :ITEM";

            try
            {
                cmdQ.Connection = conn;
                cmdQ.Transaction = transaction;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ITEM", StringUtil.toString(procData[0].item));

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    if (result["SYS_TYPE"]?.ToString() == "A")
                        bASys = true;
                    else
                        bFSys = true;
                }

                foreach (FGLGITE0Model d in procData) {

                    //A系統
                    if ("A".Equals(d.sysType)) {
                        if (bASys)
                            update("A", d, conn, transaction);
                        else
                            insert("A", d, conn, transaction);
                    }


                    //F系統
                    if ("F".Equals(d.sysType)) {
                        if (bFSys)
                            update("F", d, conn, transaction);
                        else
                            insert("F", d, conn, transaction);
                    }
                }

                


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("procEffective FGLGITE end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void update(string sysType, FGLGITE0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("update FGLGITE0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
UPDATE LGLGITE2 SET
  NUM_VRSN = :NUM_VRSN
, NUM = :NUM
, UPD_ID = :UPD_ID
, UPD_DATE = :UPD_DATE
, UPD_TIME = :UPD_TIME
  WHERE ITEM = :ITEM
    AND SYS_TYPE = :SYS_TYPE";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                
                cmd.Parameters.Add("NUM_VRSN", StringUtil.toString(procData.numVrsn));
                cmd.Parameters.Add("NUM", StringUtil.toString(procData.num));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", StringUtil.toString(procData.updDate));
                cmd.Parameters.Add("UPD_TIME", StringUtil.toString(procData.updTime));

                cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(procData.sysType));
                
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("update FGLGITE0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public void insert(string sysType, FGLGITE0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FGLGITE0 begin!");

            
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"insert into FGLGITE0 
(SYS_TYPE, ITEM, NUM_VRSN, NUM, ENTRY_ID, ENTRY_DATE, ENTRY_TIME
) VALUES (
 :SYS_TYPE, :ITEM, :NUM_VRSN, :NUM, :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("SYS_TYPE", StringUtil.toString(procData.sysType));
                cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                cmd.Parameters.Add("NUM_VRSN", StringUtil.toString(procData.numVrsn));
                cmd.Parameters.Add("NUM", StringUtil.toString(procData.num));
                cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("ENTRY_DATE", StringUtil.toString(procData.updDate));
                cmd.Parameters.Add("ENTRY_TIME", StringUtil.toString(procData.updTime));

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("insert FGLGITE0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }





    }
}