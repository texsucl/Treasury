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
    public class FRTCOPLDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 櫃匯及快速付款出款資料查詢及列印
        /// </summary>
        /// <param name="field081B"></param>
        /// <param name="field081E"></param>
        /// <param name="paidType"></param>
        /// <param name="corpNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public List<ORTB005Model> qryForORTB005Summary(string field081B, string field081E, string paidType, string corpNo, string currency
            ,string sqlNo, string vhrNo1)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB005Model> rows = new List<ORTB005Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                //modify by daiyu 20190211 修改case when語法 查詢不出資料問題
                strSQL += "SELECT COPL.PAID_TYPE, COPL.FIELD08_1, COPL.VHR_NO1, COPL.CORP_NO, COPL.CURRENCY, COPL.ACPT_ID" +
                        " ,COPL.REMIT_AMT " +
                        " ,CASE WHEN Copl.paid_type = 'Q' THEN " +
                        "           (SELECT GLPY.SQL_NO  FROM lp_fbdb / LGLGLPY7 GLPY " +
                        "             WHERE COPL.VHR_NO1 = GLPY.TEMP_NO AND GLPY.ACT_NUM = '2178010110' AND GLPY.ACT_CODE = 'C' " +
                        "               FETCH FIRST 1 ROWS ONLY) " +
                        "       WHEN Copl.paid_type = '3' THEN " +
                        "           (SELECT GLPY.SQL_NO  FROM lp_fbdb / LGLGLPY7 GLPY " +
                        "             WHERE COPL.VHR_NO1 = GLPY.TEMP_NO " +
                        "               FETCH FIRST 1 ROWS ONLY)  END AS SQL_NO " +

                    //" ,(SELECT GLPY.SQL_NO  FROM  LGLGLPY7 GLPY " +
                    //" WHERE COPL.VHR_NO1 = GLPY.TEMP_NO " +
                    //"  AND (COPL.PAID_TYPE = 'Q' AND GLPY.ACT_NUM = '2178010110' AND GLPY.ACT_CODE = 'C') " +
                    //" FETCH FIRST 1 ROWS ONLY) SQL_NO  " +
                    //" AND (GLPY.SRCE_PGM = 'SRT1813' OR GLPY.SRCE_PGM = 'PRT1813')) SQL_NO   " +  //delete by daiyu 20181019

                    " ,(SELECT " +
                    "  ((LPAD(COPD.PASS_YY, 3, '0')) || (LPAD(COPD.PASS_MM, 2, '0')) || (LPAD(COPD.PASS_DD, 2, '0'))) " +
                    "  FROM FRTCOPD0 COPD " +
                    "  WHERE COPL.VHR_NO1 = COPD.VHR_NO1 " +
                    "  and COPL.RESOURCE = COPD.RESOURCE " +
                    "  and COPL.ID_DUP = COPD.DUP_ID " +
                    "  and COPL.MEMBER_ID = COPD.MEMBER_ID " +
                    "  and COPL.CORP_NO = COPD.CORP_NO " +
                    "  and COPL.POLICY_NO = COPD.POLICY_NO " +
                    "  and COPL.POLICY_SEQ = COPD.POLICY_SEQ " +
                    "  FETCH FIRST 1 ROWS ONLY " +
                    "  ) AS PASS " +
                    "FROM LRTCOPL96 COPL " +
                    " WHERE 1=1 ";


                //strSQL += "SELECT COPL.PAID_TYPE, COPL.FIELD08_1, COPL.VHR_NO1, COPL.CORP_NO, COPL.CURRENCY," +
                //    " SUM(COPL.REMIT_AMT) REMIT_AMT, COUNT(*) DATACNT " +

                //    " ,(SELECT DISTINCT GLPY.SQL_NO  FROM  LGLGLPY7 GLPY " +
                //    " WHERE COPL.VHR_NO1 = GLPY.TEMP_NO  " +
                //    " AND (GLPY.SRCE_PGM = 'SRT1813' OR GLPY.SRCE_PGM = 'PRT1813')) SQL_NO   " +


                //    " FROM FRTCOPL0 COPL " +
                //    " WHERE 1=1 ";

                    strSQL += " AND COPL.FIELD08_1 >= :FIELD08_1_B and  COPL.FIELD08_1 <= :FIELD08_1_E";
                    cmd.Parameters.Add("FIELD08_1_B", field081B);
                    cmd.Parameters.Add("FIELD08_1_E", field081E);

                if (!"".Equals(StringUtil.toString(paidType)))
                {
                    strSQL += " AND COPL.PAID_TYPE = :PAID_TYPE";
                    cmd.Parameters.Add("PAID_TYPE", paidType == "1" ? "3" : (paidType == "2" ? "Q": paidType));
                }

                if (!"".Equals(StringUtil.toString(corpNo)))
                {
                    strSQL += " AND COPL.CORP_NO = :CORP_NO";
                    cmd.Parameters.Add("CORP_NO", corpNo);
                }

                if (!"".Equals(StringUtil.toString(currency)))
                {
                    strSQL += " AND COPL.CURRENCY = :CURRENCY";
                    cmd.Parameters.Add("CURRENCY", currency);
                }


                if (!"".Equals(StringUtil.toString(vhrNo1)))
                {
                    strSQL += " AND COPL.VHR_NO1 = :VHR_NO1";
                    cmd.Parameters.Add("VHR_NO1", vhrNo1);
                }

                strSQL += " ORDER BY COPL.FIELD08_1, COPL.VHR_NO1 ";
                //strSQL += " GROUP BY COPL.PAID_TYPE, COPL.FIELD08_1, COPL.VHR_NO1, COPL.CORP_NO, COPL.CURRENCY ";

                strSQL += " WITH UR ";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int sqlNoId = result.GetOrdinal("SQL_NO");
                int paidTypeId = result.GetOrdinal("PAID_TYPE");
                int field081Id = result.GetOrdinal("FIELD08_1");
                int vhrNo1Id = result.GetOrdinal("VHR_NO1");
                int corpNoId = result.GetOrdinal("CORP_NO");
                int currencyId = result.GetOrdinal("CURRENCY");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int acptIdId = result.GetOrdinal("ACPT_ID");

                while (result.Read())
                {
                    ORTB005Model d = new ORTB005Model();

                    d.paidType = StringUtil.toString(result.GetString(paidTypeId));
                    switch (d.paidType) {
                        case "3":
                            d.paidType = "櫃匯";
                            break;
                        case "Q":
                            d.paidType = "快速付款";
                            break;
                        default:
                            break;
                    }

                    var _field081 = result[1].ToString();
                    d.sqlNo = StringUtil.toString(result.GetString(sqlNoId));
                    d.vhrNo1 = StringUtil.toString(result.GetString(vhrNo1Id));
                    d.corpNo = StringUtil.toString(result.GetString(corpNoId));
                    d.currency = StringUtil.toString(result.GetString(currencyId));
                    d.remitAmt = result[remitAmtId].ToString();
                    //d.dataCnt = result[6].ToString();
                    d.acptId = StringUtil.toString(result.GetString(acptIdId));
                    var _pass = StringUtil.toString(result.GetString(result.GetOrdinal("PASS")));
                    if (!string.IsNullOrWhiteSpace(_pass))
                        d.field081 = _pass;
                    else
                        d.field081 = _field081;
                    if (!"".Equals(StringUtil.toString(sqlNo)) && !sqlNo.Equals(d.sqlNo))
                        continue;
                    else
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



        public List<FRTCOPL0Model> qryForORTB005Detail(string field081B, string field081E, string paidType, string corpNo, string currency
            , string sqlNo, string vhrNo1)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTCOPL0Model> rows = new List<FRTCOPL0Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                //modify by daiyu 20190211 修改case when語法 查詢不出資料問題
                strSQL += "SELECT COPL.PAID_TYPE, COPL.FIELD08_1, COPL.CURRENCY, COPL.CORP_NO, COPL.VHR_NO1, " + 
                    " COPL.RESOURCE, COPL.POLICY_NO, COPL.POLICY_SEQ, COPL.ID_DUP, COPL.CHANGE_ID, COPL.REMIT_AMT, " +
                    " COPL.UPD_YY, COPL.UPD_MM, COPL.UPD_DD, COPL.ACPT_ID, COPL.GEN_ID " +
                    " , COPL.FILLER_10, COPL.FIELD01_2 " +
                    " ,CASE WHEN Copl.paid_type = 'Q' THEN " +
                        "           (SELECT GLPY.SQL_NO  FROM lp_fbdb / LGLGLPY7 GLPY " +
                        "             WHERE COPL.VHR_NO1 = GLPY.TEMP_NO AND GLPY.ACT_NUM = '2178010110' AND GLPY.ACT_CODE = 'C' " +
                        "               FETCH FIRST 1 ROWS ONLY) " +
                        "       WHEN Copl.paid_type = '3' THEN " +
                        "           (SELECT GLPY.SQL_NO  FROM lp_fbdb / LGLGLPY7 GLPY " +
                        "             WHERE COPL.VHR_NO1 = GLPY.TEMP_NO " +
                        "               FETCH FIRST 1 ROWS ONLY)  END AS SQL_NO " +
                    // " ,(SELECT GLPY.SQL_NO  FROM  LGLGLPY7 GLPY " +
                    //" WHERE COPL.VHR_NO1 = GLPY.TEMP_NO FETCH FIRST 1 ROWS ONLY) SQL_NO  " +
                    ", ((LPAD( COPD.PASS_YY,3,'0')) || (LPAD( COPD.PASS_MM,2,'0')) || (LPAD(COPD.PASS_DD,2,'0'))) AS PASS " +
                    " FROM LRTCOPL96 COPL " +

                    "LEFT JOIN FRTCOPD0 COPD " +
                    "on COPL.VHR_NO1 = COPD.VHR_NO1 " +
                    "and COPL.RESOURCE = COPD.RESOURCE " +
                    "and COPL.ID_DUP = COPD.DUP_ID " +
                    "and COPL.MEMBER_ID = COPD.MEMBER_ID " +
                    "and COPL.CORP_NO = COPD.CORP_NO " +
                    "and COPL.POLICY_NO = COPD.POLICY_NO " +
                    "and COPL.POLICY_SEQ = COPD.POLICY_SEQ " +

                    " WHERE 1=1 ";

                strSQL += " AND COPL.FIELD08_1 >= :FIELD08_1_B and  COPL.FIELD08_1 <= :FIELD08_1_E";
                cmd.Parameters.Add("FIELD08_1_B", field081B);
                cmd.Parameters.Add("FIELD08_1_E", field081E);

                if (!"".Equals(StringUtil.toString(paidType)))
                {
                    strSQL += " AND COPL.PAID_TYPE = :PAID_TYPE";
                    cmd.Parameters.Add("PAID_TYPE", paidType == "1" ? "3" : (paidType == "2" ? "Q" : paidType));
                }

                if (!"".Equals(StringUtil.toString(corpNo)))
                {
                    strSQL += " AND COPL.CORP_NO = :CORP_NO";
                    cmd.Parameters.Add("CORP_NO", corpNo);
                }

                if (!"".Equals(StringUtil.toString(currency)))
                {
                    strSQL += " AND COPL.CURRENCY = :CURRENCY";
                    cmd.Parameters.Add("CURRENCY", currency);
                }


                if (!"".Equals(StringUtil.toString(vhrNo1)))
                {
                    strSQL += " AND VHR_NO1 = :VHR_NO1";
                    cmd.Parameters.Add("VHR_NO1", vhrNo1);
                }

                strSQL += " ORDER BY COPL.FIELD08_1, COPL.VHR_NO1 ";
                //strSQL += " GROUP BY COPL.PAID_TYPE, COPL.FIELD08_1, COPL.VHR_NO1, COPL.CORP_NO, COPL.CURRENCY ";

                strSQL += " WITH UR ";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int sqlNoId = result.GetOrdinal("SQL_NO");
                int paidTypeId = result.GetOrdinal("PAID_TYPE");
                int field081Id = result.GetOrdinal("FIELD08_1");
                int currencyId = result.GetOrdinal("CURRENCY");
                int corpNoId = result.GetOrdinal("CORP_NO");
                int vhrNo1Id = result.GetOrdinal("VHR_NO1");
                int resourceId = result.GetOrdinal("RESOURCE");
                int policyNoId = result.GetOrdinal("POLICY_NO");
                int policySeqId = result.GetOrdinal("POLICY_SEQ");
                int idDupId = result.GetOrdinal("ID_DUP");
                int changeIdId = result.GetOrdinal("CHANGE_ID");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int updYYId = result.GetOrdinal("UPD_YY");
                int updMMId = result.GetOrdinal("UPD_MM");
                int updDDId = result.GetOrdinal("UPD_DD");
                int acptIdId = result.GetOrdinal("ACPT_ID");
                int genIdId = result.GetOrdinal("GEN_ID");

                int filler10Id = result.GetOrdinal("FILLER_10");
                int field012Id = result.GetOrdinal("FIELD01_2");
                //int genUnitId = result.GetOrdinal("GEN_UNIT");
                //int stsNameId = result.GetOrdinal("STS_NAME");


                while (result.Read())
                {
                    FRTCOPL0Model d = new FRTCOPL0Model();


                    d.paidType = StringUtil.toString(result.GetString(paidTypeId));
                    var _field081 = result[field081Id].ToString();
                    //d.field081 = result[field081Id].ToString();
                    d.currency = result[currencyId].ToString();
                    d.corpNo = result[corpNoId].ToString();
                    d.vhrNo1 = result[vhrNo1Id].ToString();
                    d.resource = result[resourceId].ToString();
                    d.policyNo = result[policyNoId].ToString();
                    d.policySeq = result[policySeqId].ToString();
                    d.idDup = result[idDupId].ToString();
                    d.changeId = result[changeIdId].ToString();
                    d.remitAmt = result[remitAmtId].ToString();
                    d.endDate = result[updYYId].ToString() + "/" + result[updMMId].ToString().PadLeft(2, '0') + "/" + result[updDDId].ToString().PadLeft(2, '0');
                    d.acptId = result[acptIdId].ToString();
                    d.genId = result[genIdId].ToString();

                    var _pass = StringUtil.toString(result.GetString(result.GetOrdinal("PASS")));
                    if (!string.IsNullOrWhiteSpace(_pass))
                        d.field081 = _pass;
                    else
                        d.field081 = _field081;

                    d.filler10 = result[filler10Id].ToString();
                    d.field012 = result[field012Id].ToString();
                    try {
                        if ("R".Equals(d.filler10.Substring(1, 1)))
                            d.stsName = "退匯重匯";
                    }
                    catch (Exception e){

                    }

                    if ("".Equals(d.stsName)) {
                        if ("".Equals(StringUtil.toString(d.field012))) {
                            if("Q".Equals(d.paidType))
                                d.stsName = "快速付款";
                            else
                                d.stsName = "櫃匯";
                        } 
                        else
                            d.stsName = "應付未付";
                    }

                    
                    switch (d.paidType)
                    {
                        case "3":
                            d.paidType = "櫃匯";
                            break;
                        case "Q":
                            d.paidType = "快速付款";
                            break;
                        default:
                            break;
                    }

                    string sqlNoCopl = StringUtil.toString(result.GetString(sqlNoId));

                    if (!"".Equals(StringUtil.toString(sqlNo)) && !sqlNo.Equals(sqlNoCopl))
                        continue;
                    else
                        rows.Add(d);

                    //d.genUnit = result[1].ToString();
                    //d.stsName = result[stsNameId].ToString();

                   // rows.Add(d);
                    
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


        }
}