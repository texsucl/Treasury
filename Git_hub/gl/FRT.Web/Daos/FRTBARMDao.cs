using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// <summary>
/// 功能說明：
/// 初版作者：
/// 修改歷程：20190514 Mark
///           需求單號：201904100470
/// 修改歷程：20190819 Mark
///           需求單號：201907160360 
/// </summary>

namespace FRT.Web.Daos
{
    public class FRTBARMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        /// <summary>
        /// 重新發送電文作業
        /// </summary>
        /// <param name="fastNo"></param>
        /// <returns></returns>
        public ORTB009Model qryForORTB009(string fastNo)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            ORTB009Model oRTB009Model = new ORTB009Model();
            bool bShow = false;

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT DISTINCT M.FAST_NO, M.PAID_ID, M.BANK_CODE, M.SUB_BANK, M.BANK_ACT, M.RCV_NAME, M.REMIT_AMT " +
                    ", M.REMIT_STAT, M.TEXT_TYPE, case E.ERR_BELONG when '1' then '1' else '2' end ERR_BELONG " +
                    " FROM LRTBARM1 M JOIN LRTBERM1 E ON M.FAIL_CODE = E.TRANS_CODE " +
                    " WHERE  M.FAST_NO = :FAST_NO " +
                    " AND M.FILLER_20 = '' " + //20190815 新增判斷當FILLER_20(備用20)有值，不能重新發送電文作業 by mark
                    " AND ( " +
                    "   (M.REMIT_STAT = '1' AND M.TEXT_TYPE in ('','Q') ) " + //當狀態為1待匯款，電文型類需為空才能重新發送電文作業 20190513 類型加入Q(等待執行件)
                    "   OR (M.REMIT_STAT = '2' AND M.TEXT_SND = 'Y' AND M.TEXT_RCV = 'Y') " +
                    "   OR (M.REMIT_STAT = '4' AND E.ERR_BELONG <> '1') " +    //當狀態為4匯款失敗，錯誤歸屬為客戶，不能重新發送電文作業
                    " ) ";

                cmd.Parameters.Add("FAST_NO", fastNo);
                


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int fastNoId = result.GetOrdinal("FAST_NO");
                int paidIdId = result.GetOrdinal("PAID_ID");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int remitStatId = result.GetOrdinal("REMIT_STAT");
                int errBelongId = result.GetOrdinal("ERR_BELONG");
                int textTypeId = result.GetOrdinal("TEXT_TYPE");

                while (result.Read())
                {
                    bShow = false;
                    string remitStat = StringUtil.toString(result.GetString(remitStatId));
                    string errBelong = StringUtil.toString(result.GetString(errBelongId));
                    string textType = StringUtil.toString(result.GetString(textTypeId));

                    //下行電文錯誤歸屬為人壽或銀行，排除錯誤代碼1-匯出異常/RM01序號不符/RM02押碼不符，須人工確認原因排除後，再次發送電文
                    if ("2".Equals(remitStat))
                    {
                        if ("1".Equals(errBelong))
                        {
                            bShow = false;
                        }
                        else
                        {  //錯誤歸屬為人壽或銀行，排除錯誤代碼1-匯出異常/RM01序號不符/RM02押碼不符，須人工確認原因排除後，再次發送電文

                            // 狀態為2已匯款且發送電文次數為1時， 需要有下行電文回來且對應為1
                            //  甲、	下行電文的ERROR_CODE為0000或空白(表示成功)--> 問622685
                            //      i.無622685資料-->不可重發電文
                            //      ii.有622685資料-->判斷 匯出異常 / RM01序號不符 / RM02押碼不符-->不可重發電文
                            //  乙、	下行電文的ERROR_CODE不為0000且不為空白(表示成功)-->可重發電文



                            if ("1".Equals(textType) || "2".Equals(textType))   //EACH
                            {   
                                FRTXmlREachDao FRTXmlREachDao = new FRTXmlREachDao();
                                FRT_XML_R_eACH d = FRTXmlREachDao.qryForORTB009(fastNo);

                                if (d != null) {
                                    if(!"".Equals(StringUtil.toString(d.FAST_NO)) 
                                        && (
                                        !"".Equals(StringUtil.toString(d.EMSGID)) ||
                                        (!"".Equals(StringUtil.toString(d.ERROR_CODE)) && !"0000".Equals(StringUtil.toString(d.ERROR_CODE)))
                                        ))
                                        bShow = true;
                                }
                            }
                            else {  //金資
                                FRTXmlR622821Dao FRTXmlR622821Dao = new FRTXmlR622821Dao();
                                ORTB012Model d = FRTXmlR622821Dao.qryForORTB009(fastNo);
                                if (d != null)
                                {
                                    if (!"".Equals(StringUtil.toString(d.fastNo))) {
                                        if (
                                            !"".Equals(StringUtil.toString(d.emsgId)) ||
                                            (!"".Equals(StringUtil.toString(d.errorCode)) && !"0000".Equals(StringUtil.toString(d.errorCode)))
                                            
                                            )
                                            bShow = true;
                                        else
                                        {
                                            FRTXmlR622685Dao fRTXmlR622685Dao = new FRTXmlR622685Dao();
                                            //FRT_XML_R_622685 fRT_XML_R_622685 = fRTXmlR622685Dao.qryLstByFastNo(fastNo);
                                            var data_622685 = fRTXmlR622685Dao.qryLstByFastNo(fastNo);
                                            if (data_622685.Item3 != 3)
                                            {
                                                switch (data_622685.Item3)
                                                {
                                                    case 1:
                                                        if (StringUtil.toString(data_622685.Item1.FunCode) == "0" && StringUtil.toString(data_622685.Item1.Status) == "4") //622685 FunCode = 0+ Status=4時，不能重新發送電文作業
                                                            bShow = false;
                                                        else if (!"".Equals(StringUtil.toString(data_622685.Item1.FAST_NO)))
                                                        {
                                                            if ("1".Equals(StringUtil.toString(data_622685.Item1.Status)))
                                                                bShow = false;
                                                            else
                                                            {
                                                                if ("01".Equals(StringUtil.toString(data_622685.Item1.ucaErrorCod)) || "02".Equals(StringUtil.toString(data_622685.Item1.ucaErrorCod)))
                                                                    bShow = false;
                                                                else
                                                                    bShow = true;
                                                            }

                                                        }
                                                        break;
                                                    case 2:
                                                        if (!"".Equals(StringUtil.toString(data_622685.Item2.FAST_NO)))
                                                        {
                                                            if ("1".Equals(StringUtil.toString(data_622685.Item2.STATUS)))
                                                                bShow = false;
                                                            else
                                                            {
                                                                if ("01".Equals(StringUtil.toString(data_622685.Item2.ERROR_COD)) || "02".Equals(StringUtil.toString(data_622685.Item2.ERROR_COD)))
                                                                    bShow = false;
                                                                else
                                                                    bShow = true;
                                                            }

                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                       
                                }
                            }
                        }
                    }
                    else
                        bShow = true;



                    if (bShow) {
                        oRTB009Model.fastNo = StringUtil.toString(result.GetString(fastNoId));
                        oRTB009Model.paidId = StringUtil.toString(result.GetString(paidIdId));
                        oRTB009Model.bankCode = StringUtil.toString(result.GetString(bankCodeId)) + StringUtil.toString(result.GetString(subBankId));
                        //  d.subBank = StringUtil.toString(result.GetString(subBankId));
                        oRTB009Model.bankAct = StringUtil.toString(result.GetString(bankActId));
                        oRTB009Model.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                        oRTB009Model.remitAmt = result[remitAmtId].ToString();
                        oRTB009Model.remitStat = remitStat;
                    }
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return oRTB009Model;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }

      

        /// <summary>
        /// 快速付款報表查詢
        /// </summary>
        /// <param name="remitStat"></param>
        /// <returns></returns>
        public List<ORTB007Model> qryForORTB007(string remitStat, string qDate)
        {
            EacConnection conn = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB007Model> rows = new List<ORTB007Model>();

            string strSQL = "";
            try
            {
                conn.ConnectionString = CommonUtil.GetEasycomConn();
                conn.Open();
                cmd.Connection = conn;

                strSQL += "SELECT SYS_TYPE, SRCE_FROM, SRCE_KIND, POLICY_NO, POLICY_SEQ, ID_DUP, " +
                    " CHANGE_ID, SEQ, FAST_NO, BANK_CODE, SUB_BANK, BANK_ACT, CURRENCY,  REMIT_AMT, " +
                    " RCV_NAME, TEXT_RCVDT, TEXT_RCVTM, FAIL_CODE, ENTRY_ID, CLOSE_DATE, PAID_ID, MEMBER_ID, REMIT_DATE" +
                    " FROM LRTBARM1 " +
                    " WHERE 1 = 1 ";


                switch (remitStat)
                {
                    case "1":
                        strSQL += " AND REMIT_STAT = :REMIT_STAT";
                        cmd.Parameters.Add("REMIT_STAT", remitStat);
                        break;
                    case "4":
                        strSQL += " AND REMIT_STAT = :REMIT_STAT";
                        cmd.Parameters.Add("REMIT_STAT", remitStat);
                        strSQL += " AND FAST_NO_N = ''";
                        break;
                    case "1CloseDt":
                        strSQL += " AND CLOSE_DATE = :CLOSE_DATE";
                        strSQL += " AND (REMIT_DATE = 0 OR REMIT_DATE > CLOSE_DATE)";
                        cmd.Parameters.Add("CLOSE_DATE", qDate);
                        break;
                    case "4RemitDt":
                        strSQL += " AND REMIT_DATE = :REMIT_DATE";
                        strSQL += " AND REMIT_STAT = '4'";
                        strSQL += " AND VHR_NO3 <> ''";
                        cmd.Parameters.Add("REMIT_DATE", qDate);
                        break;
                    case "ORTB007P5":
                        strSQL += " AND REMIT_DATE = :REMIT_DATE";
                        strSQL += " AND REMIT_STAT = '5'";
                        cmd.Parameters.Add("REMIT_DATE", qDate);
                        break;
                }
                //strSQL += " AND REMIT_STAT = :REMIT_STAT";
                //cmd.Parameters.Add("REMIT_STAT", remitStat);

                //if ("4".Equals(remitStat)) {
                //    strSQL += " AND FAST_NO_N = ''";
                //}


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int sysTypeId = result.GetOrdinal("SYS_TYPE");
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int srceKindId = result.GetOrdinal("SRCE_KIND");
                int policyNoId = result.GetOrdinal("POLICY_NO");
                int policySeqId = result.GetOrdinal("POLICY_SEQ");
                int idDupId = result.GetOrdinal("ID_DUP");
                int changeIdId = result.GetOrdinal("CHANGE_ID");
                int changeSeqId = result.GetOrdinal("SEQ");
                int fastNoId = result.GetOrdinal("FAST_NO");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int currencyId = result.GetOrdinal("CURRENCY");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int textRcvdtId = result.GetOrdinal("TEXT_RCVDT");
                int textRcvtmId = result.GetOrdinal("TEXT_RCVTM");
                int failCodeId = result.GetOrdinal("FAIL_CODE");
                int entryIdId = result.GetOrdinal("ENTRY_ID");
                int closeDateId = result.GetOrdinal("CLOSE_DATE");
                int paidIdId = result.GetOrdinal("PAID_ID");
                int memberIdId = result.GetOrdinal("MEMBER_ID");
                int remitDateId = result.GetOrdinal("REMIT_DATE");

                while (result.Read())
                {
                    ORTB007Model d = new ORTB007Model();
                    d.sysType = StringUtil.toString(result.GetString(sysTypeId));
                    d.srceFrom = StringUtil.toString(result.GetString(srceFromId));
                    d.srceKind = StringUtil.toString(result.GetString(srceKindId));
                    d.policyNo = StringUtil.toString(result.GetString(policyNoId));
                    d.policySeq = result[policySeqId].ToString();
                    d.idDup = StringUtil.toString(result.GetString(idDupId));
                    d.changeId = StringUtil.toString(result.GetString(changeIdId));
                    d.changeSeq = result[changeSeqId].ToString();
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId));
                    d.subBank = StringUtil.toString(result.GetString(subBankId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.currency = StringUtil.toString(result.GetString(currencyId));
                    d.remitAmt = result[remitAmtId].ToString();
                    d.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                    d.textRcvdt = result[textRcvdtId].ToString();
                    d.textRcvtm = result[textRcvtmId].ToString();
                    d.failCode = StringUtil.toString(result.GetString(failCodeId));
                    d.entryId = StringUtil.toString(result.GetString(entryIdId));
                    d.closeDate = result[closeDateId].ToString();
                    d.paidId = StringUtil.toString(result.GetString(paidIdId));
                    d.memberId = StringUtil.toString(result.GetString(memberIdId));
                    d.remitDate = result[remitDateId].ToString();

                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;
                conn.Close();
                conn = null;

                #region 銀行回饋時間取代為622685的時間 
                if (rows.Any())
                {
                    List<string> fastNos = rows.Select(x => x.fastNo).ToList();
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        var _622685_news = db.FRT_XML_R_622685_NEW.AsNoTracking()
                            .Where(x => fastNos.Contains(x.FAST_NO) && x.FunCode == "2" && x.Status == "3").ToList();
                        var _622685s = db.FRT_XML_R_622685.AsNoTracking()
                            .Where(x => fastNos.Contains(x.FAST_NO) && x.RMT_TYPE == "2" && x.STATUS == "2").ToList();
                        foreach (var item in _622685_news)
                        {
                            var _row = rows.First(x => x.fastNo == item.FAST_NO);
                            if (item.UPD_TIME.HasValue)
                            {
                                var _dt = item.UPD_TIME.Value.AddYears(-1911);
                                _row.textRcvdt = $@"{_dt.Year}/{_dt.ToString("MM")}/{_dt.ToString("dd")}";
                                _row.textRcvtm = $@"{_dt.ToString("HH")}:{_dt.ToString("mm")}:{_dt.ToString("ss")}";
                            }
                            else
                            {
                                var _dt = item.CRT_TIME.AddYears(-1911);
                                _row.textRcvdt = $@"{_dt.Year}/{_dt.ToString("MM")}/{_dt.ToString("dd")}";
                                _row.textRcvtm = $@"{_dt.ToString("HH")}:{_dt.ToString("mm")}:{_dt.ToString("ss")}";
                            }
                        }
                        foreach (var item in _622685s)
                        {
                            var _row = rows.First(x => x.fastNo == item.FAST_NO);
                            if (item.UPD_TIME.HasValue)
                            {
                                var _dt = item.UPD_TIME.Value.AddYears(-1911);
                                _row.textRcvdt = $@"{_dt.Year}/{_dt.ToString("MM")}/{_dt.ToString("dd")}";
                                _row.textRcvtm = $@"{_dt.ToString("HH")}:{_dt.ToString("mm")}:{_dt.ToString("ss")}";
                            }
                            else
                            {
                                var _dt = item.CRT_TIME.AddYears(-1911);
                                _row.textRcvdt = $@"{_dt.Year}/{_dt.ToString("MM")}/{_dt.ToString("dd")}";
                                _row.textRcvtm = $@"{_dt.ToString("HH")}:{_dt.ToString("mm")}:{_dt.ToString("ss")}";
                            }
                        }
                    }
                }


                #endregion

                return rows;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        /// <summary>
        /// 以"快速付款編號"查詢"快速付款匯款申請檔"資料
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public FRTBARMModel qryByFastNo(string fastNo, EacConnection conn) {
            EacCommand cmd = new EacCommand();

            FRTBARMModel d = new FRTBARMModel();

            string strSQL = "";
            try
            {
                cmd.Connection = conn;
                strSQL += "SELECT SYS_TYPE, SRCE_FROM, SRCE_KIND, POLICY_NO, POLICY_SEQ, ID_DUP, "+
                    " CHANGE_ID, SEQ, FAST_NO, BANK_CODE, SUB_BANK, BANK_ACT, CURRENCY,  REMIT_AMT, " +
                    " RCV_NAME, TEXT_RCVDT, TEXT_RCVTM, FAIL_CODE, ENTRY_ID, REMIT_DATE, MEMBER_ID" +
                    " FROM LRTBARM1 " +
                    " WHERE 1 = 1 ";

                strSQL += " AND FAST_NO = :FAST_NO";
                cmd.Parameters.Add("FAST_NO", fastNo);
                
                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int sysTypeId = result.GetOrdinal("SYS_TYPE");
                int srceFromId = result.GetOrdinal("SRCE_FROM");
                int srceKindId = result.GetOrdinal("SRCE_KIND");
                int policyNoId = result.GetOrdinal("POLICY_NO");
                int policySeqId = result.GetOrdinal("POLICY_SEQ");
                int idDupId = result.GetOrdinal("ID_DUP");
                int changeIdId = result.GetOrdinal("CHANGE_ID");
                int changeSeqId = result.GetOrdinal("SEQ");
                int fastNoId = result.GetOrdinal("FAST_NO");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int currencyId = result.GetOrdinal("CURRENCY");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int textRcvdtId = result.GetOrdinal("TEXT_RCVDT");
                int textRcvtmId = result.GetOrdinal("TEXT_RCVTM");
                int failCodeId = result.GetOrdinal("FAIL_CODE");
                int entryIdId = result.GetOrdinal("ENTRY_ID");
                int remitDateId = result.GetOrdinal("REMIT_DATE");
                int memberIdId = result.GetOrdinal("MEMBER_ID");

                while (result.Read())
                {
                    
                    d.sysType = StringUtil.toString(result.GetString(sysTypeId));
                    d.srceFrom = StringUtil.toString(result.GetString(srceFromId));
                    d.srceKind = StringUtil.toString(result.GetString(srceKindId));
                    d.policyNo = StringUtil.toString(result.GetString(policyNoId));
                    d.policySeq = result[policySeqId].ToString();
                    d.idDup = StringUtil.toString(result.GetString(idDupId));
                    d.changeId = StringUtil.toString(result.GetString(changeIdId));
                    d.changeSeq = result[changeSeqId].ToString();
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId));
                    d.subBank = StringUtil.toString(result.GetString(subBankId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.currency = StringUtil.toString(result.GetString(currencyId));
                    d.remitAmt = result[remitAmtId].ToString();
                    d.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                    d.textRcvdt = result[textRcvdtId].ToString();
                    d.textRcvtm = result[textRcvtmId].ToString();
                    d.failCode = StringUtil.toString(result.GetString(failCodeId));
                    d.entryId = StringUtil.toString(result.GetString(entryIdId));
                    d.remitDate = result[remitDateId].ToString();
                    d.memberId = StringUtil.toString(result.GetString(memberIdId));
                }


                cmd.Dispose();
                cmd = null;

                return d;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }





        /// <summary>
        /// 查詢-快速付款匯款申請檔
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <param name="type">fast:快速付款,fbo:快速付款改FBO匯款</param>
        /// <returns></returns>
        public List<ORTB006Model> qryForORTB006(string fastNo, string policyNo, string policySeq, string idDup, string type)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB006Model> rows = new List<ORTB006Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT FAST_NO, REMIT_DATE, BANK_CODE, SUB_BANK, BANK_ACT, RCV_NAME, REMIT_AMT, FAIL_CODE , FILLER_20 " +
                    " FROM LRTBARM1 ";
                strSQL += " WHERE 1 = 1 ";
                if (type == "fast")
                {
                    strSQL += " AND FAIL_CODE = '#'  ";
                    strSQL += " AND NOT ";
                }
                if (type == "fbo")
                {
                    strSQL += " AND ";
                }
                strSQL += " ( REMIT_STAT in ('2','3') AND trim(NULLIF(FILLER_20,null)) <> '' ) ";
                if (!"".Equals(StringUtil.toString(fastNo)))
                {
                    strSQL += " AND FAST_NO = :FAST_NO";
                    cmd.Parameters.Add("FAST_NO", fastNo);
                }

                if (!"".Equals(StringUtil.toString(policyNo)))
                {
                    strSQL += " AND POLICY_NO = :POLICY_NO";
                    cmd.Parameters.Add("POLICY_NO", policyNo);
                }

                if (!"".Equals(StringUtil.toString(policySeq)))
                {
                    strSQL += " AND POLICY_SEQ = :POLICY_SEQ";
                    cmd.Parameters.Add("POLICY_SEQ", policySeq);
                }

                if (!"".Equals(StringUtil.toString(idDup)))
                {
                    strSQL += " AND ID_DUP = :ID_DUP";
                    cmd.Parameters.Add("ID_DUP", idDup);
                }


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int fastNoId = result.GetOrdinal("FAST_NO");
                int remitDateId = result.GetOrdinal("REMIT_DATE");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int failCodeId = result.GetOrdinal("FAIL_CODE");
                int filler_20Id = result.GetOrdinal("FILLER_20");

                while (result.Read())
                {
                    ORTB006Model d = new ORTB006Model();
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                    d.remitDate = result[remitDateId].ToString();
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId)) + StringUtil.toString(result.GetString(subBankId));
                    //  d.subBank = StringUtil.toString(result.GetString(subBankId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                    d.remitAmt = result[remitAmtId].ToString();
                    d.failCode = StringUtil.toString(result.GetString(failCodeId));
                    d.filler_20 = StringUtil.toString(result.GetString(filler_20Id));
                    rows.Add(d);
                }

                ///有些特殊符號為空白 sql 判斷不出來 程式再做一次處理
                if (type == "fbo")
                    rows = rows.Where(x => !string.IsNullOrWhiteSpace(x.filler_20)).ToList();

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
        /// 人工修改匯款失敗原因覆核作業"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public void apprFRTBARM0(string apprId, List<ORTB006Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTBARM0 begin!");

            foreach (ORTB006Model d in procData)
            {
                updateFRTBARM0(apprId, d, conn, transaction);
            }

            logger.Info("apprFRTBARM0 end!");

        }


        /// <summary>
        /// 異動"FRTBARM0  快速付款匯款申請檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateFRTBARM0(string apprId, ORTB006Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBARM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL += "update LRTBARM1 " +
                        " set FAIL_CODE = :FAIL_CODE ";
            if (!string.IsNullOrWhiteSpace(procData.filler_20))
                strSQL += " ,REMIT_STAT = '4' ";
            strSQL += " ,UPD_ID = :UPD_ID " +
                        " ,UPD_DATE = :UPD_DATE " +
                        " ,UPD_TIME = :UPD_TIME " +
                        " where FAST_NO = :FAST_NO";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("FAIL_CODE", StringUtil.toString(procData.failCode));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("FAST_NO", StringUtil.toString(procData.fastNo));

                cmd.CommandText = strSQL;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;
                
                logger.Info("updateFRTBARM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


    }
}