
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLGRAT0 佣薪獎稅務別 TABLE 檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLGRATDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

     

        public void proc(string execAction, List<FGLGRAT0Model> gratList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("proc FGLGRAT0 begin!");
            bool bExist = false;

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"SELECT ACT_NUM FROM LGLGRAT1 WHERE ACT_NUM = :ACT_NUM";

            try
            {
                cmdQ.Connection = conn;
                cmdQ.Transaction = transaction;
                cmdQ.CommandText = strSQLQ;

                foreach (FGLGRAT0Model procData in gratList)
                {
                    bExist = false;

                    cmdQ.Parameters.Clear();

                    cmdQ.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));

                    DbDataReader result = cmdQ.ExecuteReader();

                    while (result.Read())
                    {
                        bExist = true;
                        break;
                    }

                    if (!bExist & !"D".Equals(execAction))
                        insert(procData, conn, transaction);

                    //20190806 執行刪除商品作業一併刪除相關資料
                    if (bExist & "D".Equals(execAction))
                        delete(procData, conn, transaction);
                }



                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("proc FGLGRAT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void delete(FGLGRAT0Model d, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delete FGLAACT0 begin!");


            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"delete FGLGRAT0  WHERE ACT_NUM = :ACT_NUM ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("ACT_NUM", StringUtil.toString(d.actNum));


                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("delete FGLGRAT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

        public void insert(FGLGRAT0Model d, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFGLAACT0 begin!");


            EacCommand cmd = new EacCommand();

            string strSQLI = "";
            strSQLI = @"insert into FGLGRAT0 
 (ACT_NUM, TAX_TYPE, ENTRY_ID, ENTRY_DATE, ENTRY_TIME) 
 VALUES 
 (:ACT_NUM, :TAX_TYPE, :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQLI;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("ACT_NUM", StringUtil.toString(d.actNum));
                cmd.Parameters.Add("TAX_TYPE", StringUtil.toString(d.taxType));
                cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(d.entryId));
                cmd.Parameters.Add("ENTRY_DATE", StringUtil.toString(d.entryDate));
                cmd.Parameters.Add("ENTRY_TIME", StringUtil.toString(d.entryTime));
               

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("insertFGLGRAT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


    }
}