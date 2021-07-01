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
    public class FRTCODEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        /// <summary>
        /// 異動快速付款相關代碼
        /// </summary>
        /// <param name="refNo"></param>
        public void updateFBApi(string refNo) {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            con.ConnectionString = CommonUtil.GetEasycomConn();
            con.Open();
            cmd.Connection = con;

            string strSQL = "";
            strSQL += "update LRTCODE1 " +
                        " set REF_NO = :REF_NO " +
                        " where SRCE_FROM = 'RT'" +
                        " AND GROUP_ID = 'FAST-API'";

            cmd.Parameters.Add("REF_NO", refNo);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }


        /// <summary>
        /// 櫃匯及快速付款出款資料查詢及列印
        /// </summary>
        /// <param name="field081B"></param>
        /// <param name="field081E"></param>
        /// <param name="paidType"></param>
        /// <param name="corpNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public ORTB010Model qryForORTB010(ORTB010Model oRTB010Model)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB010Model> rows = new List<ORTB010Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT SRCE_FROM, GROUP_ID, REF_NO" +
                    " FROM LRTCODE1 " +
                    " WHERE 1=1 " +
                    " AND SRCE_FROM = 'RT'" +
                    " AND GROUP_ID = 'FAST-API'";


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int groupIdId = result.GetOrdinal("GROUP_ID");
                int refNoId = result.GetOrdinal("REF_NO");


                while (result.Read())
                {
                    oRTB010Model.paraValue = StringUtil.toString(result.GetString(refNoId));
                    break;
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return oRTB010Model;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public List<FRTCODEModel> qryRTCode(string srceFrom, string groupId, string refNo)
        {
            bool bsrceFrom = StringUtil.isEmpty(srceFrom);
            bool bgroupId = StringUtil.isEmpty(groupId);
            bool brefNo = StringUtil.isEmpty(refNo);


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTCODEModel> rows = new List<FRTCODEModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT SRCE_FROM, GROUP_ID, REF_NO, TEXT" +
                    ",  ENTRY_ID, ENTRY_YY, ENTRY_MM, ENTRY_DD,  UPD_ID, UPD_YY, UPD_MM, UPD_DD, ENTRY_DD, APPR_ID, APPR_DATE" +
                    " FROM LRTCODE1 " +
                    " WHERE 1=1 ";

                if (!bsrceFrom) {
                    strSQL += " AND SRCE_FROM = :SRCE_FROM";
                    cmd.Parameters.Add("SRCE_FROM", srceFrom);
                }

                if (!bgroupId)
                {
                    strSQL += " AND GROUP_ID = :GROUP_ID";
                    cmd.Parameters.Add("GROUP_ID", groupId);
                }

                if (!brefNo)
                {
                    strSQL += " AND REF_NO = :REF_NO";
                    cmd.Parameters.Add("REF_NO", refNo);
                }

                strSQL += " ORDER BY SRCE_FROM, GROUP_ID, REF_NO";


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int groupIdId = result.GetOrdinal("GROUP_ID");
                int refNoId = result.GetOrdinal("REF_NO");
                int textId = result.GetOrdinal("TEXT");

                int entryIdId = result.GetOrdinal("ENTRY_ID");
                int entryYY = result.GetOrdinal("ENTRY_YY");
                int entryMM = result.GetOrdinal("ENTRY_MM");
                int entryDD = result.GetOrdinal("ENTRY_DD");

                int updIdId = result.GetOrdinal("UPD_ID");
                int updYY = result.GetOrdinal("UPD_YY");
                int updMM = result.GetOrdinal("UPD_MM");
                int updDD = result.GetOrdinal("UPD_DD");

                int apprIdId = result.GetOrdinal("APPR_ID");
                int apprDateId = result.GetOrdinal("APPR_DATE");

                while (result.Read())
                {
                    FRTCODEModel fRTCODEModel = new FRTCODEModel();
                    fRTCODEModel.srce_from = result.GetString(srceFromId);
                    fRTCODEModel.groupId = result.GetString(groupIdId);
                    fRTCODEModel.refNo = result.GetString(refNoId);
                    fRTCODEModel.text = result.GetString(textId);
                    fRTCODEModel.entryId = result[entryIdId].ToString();
                    fRTCODEModel.entryDate = result[entryYY].ToString() + result[entryMM].ToString().PadLeft(2, '0') + result[entryDD].ToString().PadLeft(2, '0');
                    fRTCODEModel.updId = result[updIdId].ToString();
                    fRTCODEModel.updDate = (result[updYY].ToString() == "0" ? "" : result[updYY].ToString())
                        + (result[updMM].ToString() == "0" ? "" : result[updMM].ToString().PadLeft(2, '0') )
                        + (result[updDD].ToString() == "0" ? "" :result[updDD].ToString().PadLeft(2, '0'));

                    fRTCODEModel.apprId = result[apprIdId].ToString();
                    fRTCODEModel.apprDate = result[apprDateId].ToString() == "0" ? "" : result[apprDateId].ToString();

                    rows.Add(fRTCODEModel);
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
        /// ORTB013"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void apprFRTCODE0(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTCODE0 begin!");

            switch (procData.status)
            {
                case "A":
                    if ("RT".Equals(procData.srceFrom) & "BKMSG_OTH".Equals(procData.groupId))
                        insertBkmsgOth(apprId, procData, conn, transaction);
                    else
                        insertFRTCODE0(apprId, procData, conn, transaction);
                    break;
                case "D":
                    if ("RT".Equals(procData.srceFrom) & "BKMSG_OTH".Equals(procData.groupId))
                        deleteBkmsgOth(apprId, procData, conn, transaction);
                    else
                        deleteFRTCODE0(apprId, procData, conn, transaction);
                    break;
                case "U":
                    if ("RT".Equals(procData.srceFrom) & "BKMSG_OTH".Equals(procData.groupId))
                        updateBkmsgOth(apprId, procData, conn, transaction);
                    else {
                        if (procData.refNo.Equals(procData.refNoN))
                            updateFRTCODE0(apprId, procData, conn, transaction);
                        else
                        {
                            insertFRTCODE0(apprId, procData, conn, transaction);
                            deleteFRTCODE0(apprId, procData, conn, transaction);
                        }
                    }
                        
                    break;
            }

            logger.Info("apprFRTCODE0 end!");
        }


        public void insertBkmsgOth(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction) {
            string strText = StringUtil.toString(procData.text);
            string refNoN = procData.refNoN;

            char[] c = StringUtil.toString(procData.text).ToCharArray();
            bool bHalfPre = true;
            bool bHalfNow = false;
            int rowLen = 0;
            string rowText = "";
            int row = 1;

            for (int i = 0; i < c.Length; i++)
            {
                //全形空格為12288，半形空格為32
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] == 32 || c[i] < 127)
                    bHalfNow = true;
                else
                    bHalfNow = false;


                if (bHalfPre != bHalfNow)
                    rowLen += 1;

                if (bHalfNow == true)
                    rowLen += 1;
                else
                    rowLen += 2;

                bHalfPre = bHalfNow;
                rowText += Convert.ToString(c[i]);


                if (rowLen >= 67)
                {
                    Console.WriteLine(rowText);
                    rowLen = 0;
                    bHalfPre = true;
                    bHalfNow = false;

                    procData.refNoN = refNoN + "_" + row.ToString();
                    procData.text = rowText;
                    insertFRTCODE0(apprId, procData, conn, transaction);

                    row++;
                    rowText = "";
                }

            }

            if (!"".Equals(rowText)) {
                procData.refNoN = refNoN + "_" + row.ToString();
                procData.text = rowText;
                insertFRTCODE0(apprId, procData, conn, transaction);
            }
        }


        public void updateBkmsgOth(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            deleteBkmsgOth(apprId, procData, conn, transaction);
            insertBkmsgOth(apprId, procData, conn, transaction);
        }

        public void deleteBkmsgOth(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            string strSQL = "";
            strSQL += @"DELETE LRTCODE1 
                        WHERE SRCE_FROM = :SRCE_FROM
                          AND GROUP_ID = :GROUP_ID
                          AND REF_NO LIKE :REF_NO";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("SRCE_FROM", procData.srceFrom);
            cmd.Parameters.Add("GROUP_ID", procData.groupId);
            cmd.Parameters.Add("REF_NO", StringUtil.toString(procData.refNo) + "_%");

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }

        public void insertFRTCODE0(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            DateTime iDt = Convert.ToDateTime(procData.updDateTime);
            DateTime nowDt = DateTime.Now;

            string strSQL = "";
            strSQL += "insert into LRTCODE1 ";
            strSQL += " (GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM, ENTRY_YY, ENTRY_MM, ENTRY_DD, ENTRY_TIME, ENTRY_ID ";
            strSQL += " , UPD_YY, UPD_MM, UPD_DD, UPD_ID, APPRV_FLG, APPR_STAT, APPR_ID, APPR_DATE, APPR_TIMEN) ";
            strSQL += " VALUES ";
            strSQL += " (:GROUP_ID, :TEXT_LEN, :REF_NO, :TEXT, :SRCE_FROM, :ENTRY_YY, :ENTRY_MM, :ENTRY_DD, :ENTRY_TIME, :ENTRY_ID ";
            strSQL += " , :UPD_YY, :UPD_MM, :UPD_DD, :UPD_ID, :APPRV_FLG, :APPR_STAT, :APPR_ID, :APPR_DATE, :APPR_TIMEN) ";


            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            string padStr = "";
            //padStr = padStr.PadRight(procData.text.Length, ' ');

            cmd.Parameters.Add("GROUP_ID", procData.groupId);
            cmd.Parameters.Add("TEXT_LEN", procData.textLen);
            cmd.Parameters.Add("REF_NO", procData.refNoN);
            cmd.Parameters.Add("TEXT", procData.text + padStr);
            cmd.Parameters.Add("SRCE_FROM", procData.srceFrom);
            cmd.Parameters.Add("ENTRY_YY", iDt.Year - 1911);
            cmd.Parameters.Add("ENTRY_MM", iDt.Month);
            cmd.Parameters.Add("ENTRY_DD", iDt.Day);
            cmd.Parameters.Add("ENTRY_TIME", iDt.ToString("HHmmssff"));
            cmd.Parameters.Add("ENTRY_ID", procData.updId);

            cmd.Parameters.Add("UPD_YY", iDt.Year - 1911);
            cmd.Parameters.Add("UPD_MM", iDt.Month);
            cmd.Parameters.Add("UPD_DD", iDt.Day);
            cmd.Parameters.Add("UPD_ID", procData.updId);

            cmd.Parameters.Add("APPRV_FLG", procData.apprvFlg);
            cmd.Parameters.Add("APPR_STAT", procData.apprStat);
            cmd.Parameters.Add("APPR_ID", procData.apprId);
            cmd.Parameters.Add("APPR_DATE", DateUtil.ADDateToChtDate(nowDt.ToString("yyyyMMdd")));
            cmd.Parameters.Add("APPR_TIMEN", nowDt.ToString("HHmmssff"));


            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;

            //同步寫回FDCBANKA0裡的REMARK_AR的欄位上Y，如果拿掉Y時，也要同時異動FDCBANKA0裡的欄位=空白
            FDCBANKADao fDCBANKADao = new FDCBANKADao();
            fDCBANKADao.updRemarkAr(procData.refNoN, "Y", conn, transaction);

        }

        public void updateFRTCODE0(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            DateTime uDt = Convert.ToDateTime(procData.updDateTime);
            DateTime nowDt = DateTime.Now;

            string strSQL = "";
            strSQL += @"
UPDATE LRTCODE1 
  SET REF_NO = :REF_NO_N
     ,TEXT = :TEXT
     ,UPD_YY = :UPD_YY
     ,UPD_MM = :UPD_MM
     ,UPD_DD = :UPD_DD
     ,UPD_ID = :UPD_ID
     ,APPRV_FLG = :APPRV_FLG
     ,APPR_STAT = :APPR_STAT
     ,APPR_ID = :APPR_ID
     ,APPR_DATE = :APPR_DATE
     ,APPR_TIMEN = :APPR_TIMEN
 WHERE SRCE_FROM = :SRCE_FROM
  AND GROUP_ID = :GROUP_ID
  AND REF_NO = :REF_NO";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("REF_NO_N", procData.refNoN);

            string padStr = "";
            //padStr = padStr.PadRight(procData.text.Length, ' ');

            cmd.Parameters.Add("TEXT", procData.text + padStr);
            cmd.Parameters.Add("UPD_YY", uDt.Year - 1911);
            cmd.Parameters.Add("UPD_MM", uDt.Month);
            cmd.Parameters.Add("UPD_DD", uDt.Day);
            cmd.Parameters.Add("UPD_ID", procData.updId);
            cmd.Parameters.Add("APPRV_FLG", procData.apprvFlg);
            cmd.Parameters.Add("APPR_STAT", procData.apprStat);
            cmd.Parameters.Add("APPR_ID", procData.apprId);
            cmd.Parameters.Add("APPR_DATE", DateUtil.ADDateToChtDate(nowDt.ToString("yyyyMMdd")));
            cmd.Parameters.Add("APPR_TIMEN", nowDt.ToString("HHmmssff"));

            cmd.Parameters.Add("SRCE_FROM", procData.srceFrom);
            cmd.Parameters.Add("GROUP_ID", procData.groupId);
            cmd.Parameters.Add("REF_NO", procData.refNo);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }

        public void deleteFRTCODE0(string apprId, FrtCodeHisModel procData, EacConnection conn, EacTransaction transaction)
        {
            string strSQL = "";
            strSQL += @"DELETE LRTCODE1 
                        WHERE SRCE_FROM = :SRCE_FROM
                          AND GROUP_ID = :GROUP_ID
                          AND REF_NO = :REF_NO";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("SRCE_FROM", procData.srceFrom);
            cmd.Parameters.Add("GROUP_ID", procData.groupId);
            cmd.Parameters.Add("REF_NO", procData.refNo);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;

            //同步寫回FDCBANKA0裡的REMARK_AR的欄位上Y，如果拿掉Y時，也要同時異動FDCBANKA0裡的欄位=空白
            FDCBANKADao fDCBANKADao = new FDCBANKADao();
            fDCBANKADao.updRemarkAr(procData.refNo, "", conn, transaction);
        }

    }
}