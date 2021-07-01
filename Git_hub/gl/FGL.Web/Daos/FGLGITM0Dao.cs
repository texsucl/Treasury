
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLGITM0 短年期商品 TABLE 檔 
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLGITM0Dao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 查詢，FOR OGL00009 商品年期及躉繳商品設定作業
        /// </summary>
        /// <param name="item_type"></param>
        /// <param name="item"></param>
        /// <param name="sys_type"></param>
        /// <param name="prem_y_tp"></param>
        /// <returns></returns>
        public List<FGLGITM0Model> qryOGL00009(string item_type, string item, string sys_type, string prem_y_tp)
        {
            logger.Info("qryOGL00009 begin!");

            List<FGLGITM0Model> dataList = new List<FGLGITM0Model>();

            int bItem = "".Equals(StringUtil.toString(item)) ? 1 : 0;
            int bSysType = "".Equals(StringUtil.toString(sys_type)) ? 1 : 0;
            int bPremYTp = "".Equals(StringUtil.toString(prem_y_tp)) ? 1 : 0;

            switch (sys_type) {
                case "1":
                    sys_type = "A";
                    break;
                case "2":
                    sys_type = "F";
                    break;
                case "3":
                    sys_type = "";
                    break;
            }


            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();
                EacCommand cmdQ = new EacCommand();
                string strSQLQ = @"
SELECT ITEM, SYS_TYPE, YEAR, ITEM_TYPE, PREM_Y_TP, AGE, UPD_ID, UPD_DATE, UPD_TIME
  FROM LGLGITM1 
    WHERE ITEM_TYPE = :ITEM_TYPE
  AND (1 = :bItem OR (ITEM = :ITEM))
  AND (1 = :bSysType OR (SYS_TYPE = :SYS_TYPE))
  AND (1 = :bPremYTp OR (PREM_Y_TP = :PREM_Y_TP))";

                try
                {
                    cmdQ.Connection = conn400;
                    cmdQ.CommandText = strSQLQ;

                    cmdQ.Parameters.Clear();
                    
                    cmdQ.Parameters.Add("ITEM_TYPE", item_type);
                    cmdQ.Parameters.Add("bItem", bItem);
                    cmdQ.Parameters.Add("ITEM", item);
                    cmdQ.Parameters.Add("bSysType", bSysType);
                    cmdQ.Parameters.Add("SYS_TYPE", sys_type);
                    cmdQ.Parameters.Add("bPremYTp", bPremYTp);
                    cmdQ.Parameters.Add("PREM_Y_TP", prem_y_tp);

                    DbDataReader result = cmdQ.ExecuteReader();

                    while (result.Read())
                    {
                        FGLGITM0Model d = new FGLGITM0Model();
                        d.item = result["ITEM"]?.ToString();

                        d.sys_type = result["SYS_TYPE"]?.ToString();
                        switch (d.sys_type)
                        {
                            case "A":
                                d.sys_type = "1";
                                break;
                            case "F":
                                d.sys_type = "2";
                                break;
                            case "":
                                d.sys_type = "3";
                                break;
                        }



                        d.year = result["YEAR"]?.ToString();
                        d.item_type = result["ITEM_TYPE"]?.ToString();
                        d.prem_y_tp = result["PREM_Y_TP"]?.ToString();
                        d.age = result["AGE"]?.ToString();
                        d.upd_id = result["UPD_ID"]?.ToString();
                        d.upd_date = result["UPD_DATE"]?.ToString();
                        d.upd_time = result["UPD_TIME"]?.ToString();

                        dataList.Add(d);
                    }


                    cmdQ.Dispose();
                    cmdQ = null;

                    logger.Info("dataList end!");

                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    throw e;
                }
            }

            return dataList;

        }



        public void procAppr(FGL_GITM_HIS procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procAppr begin!");

            switch (procData.sys_type)
            {
                case "1":
                    procData.sys_type = "A";
                    break;
                case "2":
                    procData.sys_type = "F";
                    break;
                case "3":
                    procData.sys_type = "";
                    break;
            }

            switch (procData.sys_type_n)
            {
                case "1":
                    procData.sys_type_n = "A";
                    break;
                case "2":
                    procData.sys_type_n = "F";
                    break;
                case "3":
                    procData.sys_type_n = "";
                    break;
            }

            switch (procData.exec_action)
            {
                case "A":
                        insert(procData, conn, transaction);
                    break;
                case "D":
                        delete( procData, conn, transaction);
                    break;
                case "U":
                    if ((procData.item_type + procData.sys_type + procData.item).Equals(procData.item_type_n + procData.sys_type_n + procData.item_n))
                        update(procData, conn, transaction);
                    else
                    {
                        insert(procData, conn, transaction);
                        delete(procData, conn, transaction);
                    }

                    break;
            }

            logger.Info("procAppr end!");
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_GITM_HIS procData, EacConnection conn, EacTransaction transaction)
        {
            DateTime nowDt = DateTime.Now;

            string strSQL = "";

            strSQL += "insert into LGLGITM1 ";
            strSQL += " (ITEM_TYPE, SYS_TYPE, ITEM, YEAR, PREM_Y_TP, AGE, ENTRY_ID, ENTRY_DATE, ENTRY_TIME, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:ITEM_TYPE, :SYS_TYPE, :ITEM, :YEAR, :PREM_Y_TP, :AGE, :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME, :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("ITEM_TYPE", procData.item_type_n);
            cmd.Parameters.Add("SYS_TYPE", procData.sys_type_n);
            cmd.Parameters.Add("ITEM", procData.item_n);

            cmd.Parameters.Add("YEAR", procData.year);
            cmd.Parameters.Add("PREM_Y_TP", procData.prem_y_tp);
            cmd.Parameters.Add("AGE", procData.age);

            cmd.Parameters.Add("ENTRY_ID", procData.update_id);
            cmd.Parameters.Add("ENTRY_DATE", DateUtil.ADDateToChtDate(nowDt.ToString("yyyyMMdd")));
            cmd.Parameters.Add("ENTRY_TIME", nowDt.ToString("HHmmssff"));
            cmd.Parameters.Add("UPD_ID", procData.update_id);
            cmd.Parameters.Add("UPD_DATE", DateUtil.ADDateToChtDate(nowDt.ToString("yyyyMMdd")));
            cmd.Parameters.Add("UPD_TIME", nowDt.ToString("HHmmssff"));

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }


        public void update(FGL_GITM_HIS procData, EacConnection conn, EacTransaction transaction)
        {
            DateTime nowDt = DateTime.Now;

            string strSQL = "";
            strSQL += "update LGLGITM1 ";
            strSQL += " set ITEM_TYPE = :ITEM_TYPE_N ";
            strSQL += " , SYS_TYPE = :SYS_TYPE_N ";
            strSQL += " , ITEM = :ITEM_N ";
            strSQL += " , YEAR = :YEAR ";
            strSQL += " , PREM_Y_TP = :PREM_Y_TP ";
            strSQL += " , AGE = :AGE ";
            strSQL += " , UPD_ID = :UPD_ID ";
            strSQL += " , UPD_DATE = :UPD_DATE ";
            strSQL += " , UPD_TIME = :UPD_TIME ";

            strSQL += " where ITEM_TYPE = :ITEM_TYPE ";
            strSQL += " and SYS_TYPE = :SYS_TYPE ";
            strSQL += " and ITEM = :ITEM ";


            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("ITEM_TYPE_N", procData.item_type_n);
            cmd.Parameters.Add("SYS_TYPE_N", procData.sys_type_n);
            cmd.Parameters.Add("ITEM_N", procData.item_n);

            cmd.Parameters.Add("YEAR", procData.year);
            cmd.Parameters.Add("PREM_Y_TP", procData.prem_y_tp);
            cmd.Parameters.Add("AGE", procData.age);

            cmd.Parameters.Add("UPD_ID", procData.update_id);
            cmd.Parameters.Add("UPD_DATE", DateUtil.ADDateToChtDate(nowDt.ToString("yyyyMMdd")));
            cmd.Parameters.Add("UPD_TIME", nowDt.ToString("HHmmssff"));

            cmd.Parameters.Add("ITEM_TYPE", procData.item_type);
            cmd.Parameters.Add("SYS_TYPE", procData.sys_type);
            cmd.Parameters.Add("ITEM", procData.item);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }


        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delete(FGL_GITM_HIS procData, EacConnection conn, EacTransaction transaction)
        {

            string strSQL = "";
            strSQL += "delete LGLGITM1 ";
            strSQL += " where ITEM_TYPE = :ITEM_TYPE ";
            strSQL += " and SYS_TYPE = :SYS_TYPE ";
            strSQL += " and ITEM = :ITEM ";
            

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("ITEM_TYPE", procData.item_type);
            cmd.Parameters.Add("SYS_TYPE", procData.sys_type);
            cmd.Parameters.Add("ITEM", procData.item);
            

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }

    }
}