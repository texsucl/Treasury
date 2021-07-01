using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;
using static FRT.Web.BO.Extension;

/// <summary>
/// 功能說明：跨系統勾稽 未銷帳
/// 初版作者：20210524 Mark
/// 修改歷程：20210524 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105BD2 : IORT0105Report
    {

        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MSGReturnModel<Tuple<IORT0105ReportModel, bool>> check(FRT_CROSS_SYSTEM_CHECK schedulerModel, string UserId, ReportType type = ReportType.S, string date_s = null, string date_e = null, string deadline = null)
        {
            var _ORT0106 = new ORT0106();
            MSGReturnModel<Tuple<IORT0105ReportModel, bool>> resultModel = new MSGReturnModel<Tuple<IORT0105ReportModel, bool>>();
            try
            {
                _ORT0106.updateRunFlag(schedulerModel, "Y");
                var _getData = getData(schedulerModel, date_s, date_e, deadline);
                if (_getData.RETURN_FLAG)
                {
                    var _getReportData = getReportData(_getData.Datas);
                    if (_getReportData.RETURN_FLAG)
                    {
                        switch (type)
                        {
                            case ReportType.S:
                                var _sendReport = new GoujiReport().sendReport(_getReportData.Datas, schedulerModel, UserId, date_s, date_e);
                                resultModel.RETURN_FLAG = _sendReport.RETURN_FLAG;
                                resultModel.DESCRIPTION = _sendReport.DESCRIPTION;
                                break;
                            case ReportType.R:
                                resultModel.Datas = new Tuple<IORT0105ReportModel, bool>(_getReportData.Datas, false);
                                resultModel.RETURN_FLAG = _getReportData.RETURN_FLAG;
                                resultModel.DESCRIPTION = _getReportData.DESCRIPTION;
                                break;
                        }
                    }
                    else
                    {
                        resultModel.DESCRIPTION = _getData.DESCRIPTION;
                    }
                }
                else
                {
                    resultModel.DESCRIPTION = _getData.DESCRIPTION;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();
            }
            finally
            {
                _ORT0106.updateRunFlag(schedulerModel, "N");
            }
            return resultModel;
        }

        public MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>, string>> getData(FRT_CROSS_SYSTEM_CHECK schedulerModel, string date_s = null, string date_e = null, string deadline = null)
        {
            MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>, string>> result = new MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>, string>>();
            List<ORT0105BD1Model> MAINData = new List<ORT0105BD1Model>();
            List<ORT0105BD1Model> WanpieData = new List<ORT0105BD1Model>();
            var _deadline_ = string.Empty; //未銷帳截止日 西元年
            var _deadline_t = string.Empty; //未銷帳截止日 民國年
            var datas = new GoujiReport().getReportDate(schedulerModel, date_s, date_e);
            if (!deadline.IsNullOrWhiteSpace())
            {
                var _deadline = new GoujiReport().getReportDate(null, deadline, deadline);
                _deadline_ = _deadline.Item3;
                _deadline_t = _deadline.Item5;
            }
            else
            {
                _deadline_ = datas.Item3;
                _deadline_t = datas.Item5;
            }
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    List<ORT0105BD1Model> FARREMH0s = new List<ORT0105BD1Model>();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
TRAN_SEQ AS NO,
CURRENCY AS CURRENCY,
WEDAY AS DATE,
SUM(TRANS_AMT - PASS_AMT)  AS AMT
from FARREMH0
where WEDAY  between :DATE_S_1 and :DATE_E_1
{getEXCEPT_ACCT_NOs("MT_ACNT")}
GROUP BY TRAN_SEQ,CURRENCY,WEDAY
WITH UR;
";
                        com.Parameters.Add("DATE_S_1", datas.Item4);
                        com.Parameters.Add("DATE_E_1", datas.Item5);
                        com.CommandText = sql; 
                        //com.CommandTimeout = getTimeout();
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105BD1Model();
                            model.NO = dbresult["NO"]?.ToString()?.Trim(); //序號
                            model.CURRENCY = dbresult["CURRENCY"]?.ToString()?.Trim(); //幣別
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //交易日期
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            FARREMH0s.Add(model);
                        }
                        com.Dispose();
                    }
                    if (FARREMH0s.Any())
                    {
                        List<ORT0105BD1Model> FARREMD0s = new List<ORT0105BD1Model>();
                        using (EacCommand com = new EacCommand(conn))
                        {
                            string sql = string.Empty;

                            sql = $@"
select 
TRAN_SEQ AS NO,
CURRENCY AS CURRENCY,
(LPAD(ENTRY_YY,3,'0')) || (LPAD(ENTRY_MM,2,'0')) || (LPAD(ENTRY_DD,2,'0')) AS DATE,
SUM(PASS_AMT_D) AS AMT
from FARREMD0
where (LPAD(ENTRY_YY,3,'0')) || (LPAD(ENTRY_MM,2,'0')) || (LPAD(ENTRY_DD,2,'0'))  > :DATE_END_2
AND TRAN_SEQ in (
select TRAN_SEQ from FARREMH0
where  WEDAY  between :DATE_S_2 and :DATE_E_2
{getEXCEPT_ACCT_NOs("MT_ACNT")}
)
AND PASS_AMT_D <> 0
GROUP BY TRAN_SEQ,CURRENCY,ENTRY_YY,ENTRY_MM,ENTRY_DD
WITH UR;
";
                            com.Parameters.Add("DATE_END_2", _deadline_t);
                            com.Parameters.Add("DATE_S_2", datas.Item4);
                            com.Parameters.Add("DATE_E_2", datas.Item5);
                            com.CommandText = sql;
                            //com.CommandTimeout = getTimeout();
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                var model = new ORT0105BD1Model();
                                model.NO = dbresult["NO"]?.ToString()?.Trim(); //序號
                                model.CURRENCY = dbresult["CURRENCY"]?.ToString()?.Trim(); //幣別
                                model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //交易日期
                                model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                                FARREMD0s.Add(model);
                            }
                            com.Dispose();
                        }
                        if (FARREMD0s.Any())
                        {
                            foreach (var _FARREMD0 in FARREMD0s.GroupBy(x => new { x.NO, x.CURRENCY }))
                            {
                                var _FARREMH0 = FARREMH0s.FirstOrDefault(x => x.NO == _FARREMD0.Key.NO && x.CURRENCY == _FARREMD0.Key.CURRENCY);
                                if (_FARREMH0 != null)
                                    _FARREMH0.AMT = (_FARREMH0.AMT ?? 0m) + (_FARREMD0.Where(x => x.AMT != null).Sum(x => x.AMT) ?? 0m );
                            }
                        }
                        var _FARREMH0s_fixed = FARREMH0s.Where(x => x.AMT != null && x.AMT != 0m).ToList();
                        if(_FARREMH0s_fixed.Any())
                            MAINData.AddRange(_FARREMH0s_fixed);
                    }
                    List<ORT0105BD1Model> FARVRMH0s = new List<ORT0105BD1Model>();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
VRTS_NO AS NO,
CURRENCY AS CURRENCY,
(LPAD(GL_YY,3,'0')) || (LPAD(GL_MM,2,'0')) ||  (LPAD(GL_DD,2,'0')) AS DATE,
SUM(BALA_AMT) AS AMT
from FARVRMH0
where (LPAD(GL_YY,3,'0')) || (LPAD(GL_MM,2,'0')) ||  (LPAD(GL_DD,2,'0'))  between :DATE_S_3 and :DATE_E_3
{getEXCEPT_ACCT_NOs("BANK_ACT")}
GROUP BY VRTS_NO,CURRENCY,GL_YY,GL_MM,GL_DD
WITH UR;
";
                        com.Parameters.Add("DATE_S_3", datas.Item4);
                        com.Parameters.Add("DATE_E_3", datas.Item5);
                        com.CommandText = sql;
                        //com.CommandTimeout = getTimeout();
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105BD1Model();
                            model.NO = dbresult["NO"]?.ToString()?.Trim(); //序號
                            model.CURRENCY = dbresult["CURRENCY"]?.ToString()?.Trim(); //幣別
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //交易日期
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            FARVRMH0s.Add(model);
                        }
                        com.Dispose();
                    }
                    if (FARVRMH0s.Any())
                    {
                        List<ORT0105BD1Model> FARVRMS0s = new List<ORT0105BD1Model>();
                        using (EacCommand com = new EacCommand(conn))
                        {
                            string sql = string.Empty;

                            sql = $@"
select 
VRTS_NO AS NO,
CURRENCY AS CURRENCY,
(LPAD(PASS_YY,3,'0')) || (LPAD(PASS_MM,2,'0')) ||  (LPAD(PASS_DD,2,'0')) AS DATE,
SUM(PASS_AMT) AS AMT
from FARVRMS0
where (LPAD(PASS_YY,3,'0')) || (LPAD(PASS_MM,2,'0')) ||  (LPAD(PASS_DD,2,'0'))   > :DATE_END_4
AND VRTS_NO in (
select VRTS_NO from FARVRMH0
where (LPAD(GL_YY,3,'0')) || (LPAD(GL_MM,2,'0')) ||  (LPAD(GL_DD,2,'0'))  between :DATE_S_4 and :DATE_E_4
{getEXCEPT_ACCT_NOs("BANK_ACT")}
)
AND PASS_AMT <> 0
GROUP BY VRTS_NO,CURRENCY,PASS_YY,PASS_MM,PASS_DD
WITH UR;
";
                            com.Parameters.Add("DATE_END_4", _deadline_t);
                            com.Parameters.Add("DATE_S_4", datas.Item4);
                            com.Parameters.Add("DATE_E_4", datas.Item5);
                            com.CommandText = sql;
                            //com.CommandTimeout = getTimeout();
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                var model = new ORT0105BD1Model();
                                model.NO = dbresult["NO"]?.ToString()?.Trim(); //序號
                                model.CURRENCY = dbresult["CURRENCY"]?.ToString()?.Trim(); //幣別
                                model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //交易日期
                                model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                                FARVRMS0s.Add(model);
                            }
                            com.Dispose();
                        }
                        if (FARVRMS0s.Any())
                        {
                            foreach (var _FARVRMS0 in FARVRMS0s.GroupBy(x => new { x.NO, x.CURRENCY }))
                            {
                                var _FARVRMH0 = FARVRMH0s.FirstOrDefault(x => x.NO == _FARVRMS0.Key.NO && x.CURRENCY == _FARVRMS0.Key.CURRENCY);
                                if (_FARVRMH0 != null)
                                    _FARVRMH0.AMT = (_FARVRMH0.AMT ?? 0m) + (_FARVRMS0.Where(x => x.AMT != null).Sum(x => x.AMT) ?? 0m);
                            }
                        }
                        var _FARVRMH0s_fixed = FARVRMH0s.Where(x => x.AMT != null && x.AMT != 0m).ToList();
                        if(_FARVRMH0s_fixed.Any())
                            MAINData.AddRange(_FARVRMH0s_fixed);
                    }
                    conn.Dispose();
                    conn.Close();
                }

                using (SqlConnection conn = new SqlConnection(CommonUtil.GetGLSIACTConn()))
                {
                    conn.Open();
                    List<ORT0105BD1Model> ACC09050101s = new List<ORT0105BD1Model>();
                    string sql = $@"
with temp as(
select 
M.VOUCHER_NO AS NO, --表單編號
M.CURR_CODE AS CURRENCY, --交易幣別
M.IO_DATE AS VOUCHER_DATE, --交易日期
isnull(M.VOUCHER_AMT,0)-isnull((
 select SUM(VOUCHER_AMT * PLUS_OR_MINUS) 
 from GLSIACT..ACC09110902 (nolock)
 where CORP_NO=M.CORP_NO 
 and  VOUCHER_NO_ACC09050101=M.VOUCHER_NO 
 and UNIQ_KEY_ACC09050101=M.UNIQ_KEY 
 and isnull(BECOME_INVALID_YN,'N')='N'
 and isnull(VOUCHER_FIX_TYPE_CODE,'''')<>'3'),0)
 as AMT, --未銷金額
isnull(M.VOUCHER_AMT,0) AS VOUCHER_AMT
from GLSIACT..ACC09050101 (nolock) M
WHERE M.IO_DATE between @DATE_S_1 and @DATE_E_1
{getEXCEPT_ACCT_NOs("M.BANK_ACCT_NO")}
AND (M.VOUCHER_FIX_TYPE_CODE is null or (not (M.VOUCHER_FIX_TYPE_CODE='9'))) -- 排除 性質別 9
AND isnull(M.BECOME_INVALID_YN,'N') = 'N'
AND M.CORP_NO in ('FUBONLIFE','OIU'))
select 
NO,
CURRENCY,
VOUCHER_DATE,
ABS(AMT) AS AMT
from temp
where (amt > 0 and VOUCHER_AMT >= amt) --【未沖銷】及【未完全沖帳】
-- amt = 0  完全沖帳
-- amt > 0 and amt > VOUCHER_AMT --超額沖銷
";
                    ACC09050101s = conn.Query<ORT0105BD1Model>(sql,
                    new
                    {
                        DATE_S_1 = datas.Item2,
                        DATE_E_1 = datas.Item3
                    }, null, true, getTimeout()).ToList();

                    if (ACC09050101s.Any())
                    {
                        List<ORT0105BD1Model> ACC09110901s = new List<ORT0105BD1Model>();
                        sql = $@"
with temp as
(
select 
ACC.VOUCHER_NO AS VOUCHER_NO, -- 表單編號
ACC.FROM_XAC279350738229XX_VOUCHER_NO AS VOUCHER_NO_Y_N2, --表單編號(Y至N2的沖銷檔編號)  
(select top 1 JC8.VAR_CODE_NAME from GLSIACT..UUU85020101 JC8 WITH(NOLOCK) where JC8.CORP_NO=ACC.CORP_NO and JC8.VAR_CODE_FIELD_ID='ACC09110901_FROM_TABLE_ID' and JC8.VAR_CODE= ACC.FROM_TABLE_ID) AS VAR_CODE_NAME, --檔案代碼/名稱
M.IO_DATE AS VOUCHER_DATE, --交易日期
ACC.CURR_CODE AS CURRENCY, --幣別
ACC.VOUCHER_NO_ACC09050101 AS NO, --表單編號(存摺收支) 交易序號 / 代收編號
--ACC.VOUCHER_AMT AS AMT --銷帳金額
case when ACC.FROM_TABLE_ID='ACC02010101' and ACC.DR_CR_TYPE<>(select top 1 ACC3.DR_CR_TYPE from GLSIACT..ACC09050101 ACC3 (nolock) where ACC3.CORP_NO=ACC.CORP_NO and ACC3.VOUCHER_NO=ACC.VOUCHER_NO_ACC09050101) 
then -abs(ACC.PLUS_OR_MINUS * ACC.VOUCHER_AMT) 
else  ACC.PLUS_OR_MINUS * ACC.VOUCHER_AMT 
end as ACC_AMT, --銷帳金額
isnull(M.VOUCHER_AMT,0)-isnull((
 select SUM(VOUCHER_AMT * PLUS_OR_MINUS) 
 from GLSIACT..ACC09110902 (nolock)
 where CORP_NO=M.CORP_NO 
 and  VOUCHER_NO_ACC09050101=M.VOUCHER_NO 
 and UNIQ_KEY_ACC09050101=M.UNIQ_KEY 
 and isnull(BECOME_INVALID_YN,'N')='N'
 and isnull(VOUCHER_FIX_TYPE_CODE,'''')<>'3'),0)
 as M_AMT, --未銷金額
 isnull(M.VOUCHER_AMT,0) AS M_VOUCHER_AMT
from GLSIACT..ACC09110901 ACC (nolock) --存摺沖銷檔(非存摺方
JOIN GLSIACT..ACC09050101 M (nolock)
on ACC.VOUCHER_NO_ACC09050101 = M.VOUCHER_NO
and ACC.CORP_NO = M.CORP_NO
WHERE ACC.VOUCHER_DATE > @DATE_END_2  --沖銷日期
AND M.IO_DATE between  @DATE_S_2 and @DATE_E_2   --交易日期
{getEXCEPT_ACCT_NOs("ACC.BANK_ACCT_NO")}
AND (M.VOUCHER_FIX_TYPE_CODE is null or (not (M.VOUCHER_FIX_TYPE_CODE='9'))) -- 排除 性質別 9
AND isnull(M.BECOME_INVALID_YN,'N') = 'N'
AND M.CORP_NO in ('FUBONLIFE','OIU')
and ISNULL(ACC.BECOME_INVALID_YN,'') <> 'Y' --去除作廢
AND ACC.CORP_NO in ('FUBONLIFE','OIU')
--AND ISNULL(ACC.FROM_XAC279350738224XX_YN,'') <> 'Y' --N2至Y
--AND ISNULL(ACC.FROM_XAC279350738229XX_YN,'') <> 'Y' --Y至N2
)
select 
VOUCHER_NO,
VOUCHER_NO_Y_N2,
VOUCHER_DATE,
CURRENCY,
NO,
--CASE WHEN VAR_CODE_NAME = '傳票'
--THEN ABS(ACC_AMT)
--ELSE ACC_AMT 
--END AS AMT
ACC_AMT AS AMT
from temp
where 
 (M_AMT > 0 and M_VOUCHER_AMT >= M_AMT) --【未沖銷】及【未完全沖帳】
 OR M_AMT = 0 --或是【完全沖帳】
";
                        ACC09110901s = conn.Query<ORT0105BD1Model>(sql,
                            new
                            {
                                DATE_END_2 = _deadline_,
                                DATE_S_2 = datas.Item2,
                                DATE_E_2 = datas.Item3
                            }, null, true, getTimeout()).ToList();

                        var _VOUCHER_NO_Y_N2s = ACC09110901s.Where(x => x.VOUCHER_NO_Y_N2 != null).Select(x => x.VOUCHER_NO_Y_N2).Distinct().ToList();
                        ACC09110901s = ACC09110901s.Where(x => !_VOUCHER_NO_Y_N2s.Contains(x.VOUCHER_NO) && x.VOUCHER_NO_Y_N2 == null)
                            //.GroupBy(x => new { x.NO, x.VOUCHER_DATE, x.CURRENCY })
                            .GroupBy(x => new { x.NO, x.CURRENCY,x.VOUCHER_DATE })
                            .Select(x => new ORT0105BD1Model()
                            {
                                NO = x.Key.NO,
                                VOUCHER_DATE = x.Key.VOUCHER_DATE,
                                //DATE = x.Key.VOUCHER_DATE == null ? string.Empty : $@"{(x.Key.VOUCHER_DATE.Value.Year - 1911)}{x.Key.VOUCHER_DATE.Value.ToString("MMdd")}",
                                AMT = x.Where(z => z.AMT != null).Sum(y => y.AMT),
                                CURRENCY = x.Key.CURRENCY,
                            }).ToList();

                        foreach (var item in ACC09110901s)
                        { 
                            var _ACC09050101 = ACC09050101s.FirstOrDefault(x => x.NO == item.NO && x.CURRENCY == item.CURRENCY);
                            if (_ACC09050101 != null)
                            {
                                if (_ACC09050101.AMT == null)
                                    _ACC09050101.AMT = 0m;
                                _ACC09050101.AMT += item.AMT;
                            }
                            else
                            {
                                ACC09050101s.Add(item);
                            }
                        }

                        foreach (var item in ACC09050101s)
                        {
                            item.DATE = item.VOUCHER_DATE == null ? string.Empty : $@"{(item.VOUCHER_DATE.Value.Year - 1911)}{item.VOUCHER_DATE.Value.ToString("MMdd")}";
                            //var _ACC09110901 = ACC09110901s.FirstOrDefault(x => x.NO == item.NO && x.CURRENCY == item.CURRENCY);
                            //if (_ACC09110901 != null && _ACC09110901.AMT != null)
                            //{
                            //    if (item.AMT == null)
                            //        item.AMT = 0m;
                            //    item.AMT += _ACC09110901.AMT;
                            //}
                            if (item.AMT == null)
                                item.AMT = 0m;
                        }
                        WanpieData = ACC09050101s.Where(x => x.AMT != 0m).ToList();
                    }
                }
                result.RETURN_FLAG = true;
                result.Datas = new Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>,string>(MAINData, WanpieData, _deadline_);
            }
            catch (Exception ex)
            {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }

            return result;
        }

        public MSGReturnModel<ORT0105BD1ReportModel> getReportData(Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>, string > datas)
        {
            MSGReturnModel<ORT0105BD1ReportModel> result = new MSGReturnModel<ORT0105BD1ReportModel>();
            ORT0105BD1ReportModel _ORT0105BD1ReportModel = new ORT0105BD1ReportModel();
            ORTReportCModel mainModel = new ORTReportCModel();
            List<ORTReportCDetailModel> subModel = new List<ORTReportCDetailModel>();
            try
            {

                #region 資料整理
                var _item1 = datas.Item1.Distinct().ToList();
                var _item2 = datas.Item2.Distinct().ToList();
                var _as400_amt = datas.Item1.Where(x => x.AMT != null).Sum(x => x.AMT ?? 0m);
                var _wanpie_amt = datas.Item2.Where(x => x.AMT != null).Sum(x => x.AMT ?? 0m);
                mainModel.AS400_AMT = _as400_amt.formateThousand();
                mainModel.AS400_Count = _item1.Count.formateThousand();
                mainModel.Wanpie_AMT = _wanpie_amt.formateThousand();
                mainModel.Wanpie_Count = _item2.Count.formateThousand();
                mainModel.Diff_AMT = Math.Abs(_as400_amt - _wanpie_amt).formateThousand();
                mainModel.Deadline = datas.Item3;
                int _diffCount = 0;
                var _all = _item1.Union(_item2).ToList();
                foreach (var item in _all)
                {
                    var _as400 = datas.Item1.FirstOrDefault(x => x.NO == item.NO && x.DATE == item.DATE && x.CURRENCY == item.CURRENCY && x.AMT != null && x.AMT != 0m);
                    if (_as400 != null)
                        datas.Item1.Remove(_as400);
                    var _wanpie = datas.Item2.FirstOrDefault(x => x.NO == item.NO && x.DATE == item.DATE && x.CURRENCY == item.CURRENCY && x.AMT != null && x.AMT != 0m);
                    if (_wanpie != null)
                        datas.Item2.Remove(_wanpie);
                    if (_as400 == null || _wanpie == null || (_as400.AMT != _wanpie.AMT))
                    {
                        if (!((_as400 == null || _as400.AMT == 0m) && (_wanpie == null || _wanpie.AMT == 0m)))
                        {
                            subModel.Add(new ORTReportCDetailModel()
                            {
                                NO = item.NO,
                                CURRENCY = item.CURRENCY,
                                DATE = item.DATE,
                                AS400_AMT = _as400?.AMT == 0m ? "-" : (_as400?.AMT?.formateThousand() ?? "-"),
                                Wanpie_AMT = _as400?.AMT == 0m ? "-" : (_wanpie?.AMT?.formateThousand() ?? "-"),
                                Diff_AMT = Math.Abs((_as400?.AMT ?? 0m) - (_wanpie?.AMT ?? 0m)).formateThousand()
                            });
                            _diffCount += 1;
                        }
                    }
                }

                mainModel.Diff_Count = _diffCount.formateThousand();

                if (subModel.Any())
                    mainModel.Compare_Result = "比對不符";
                else
                    mainModel.Compare_Result = "比對相符";

                #endregion
                result.RETURN_FLAG = true;
                _ORT0105BD1ReportModel.model = new Tuple<ORTReportCModel, List<ORTReportCDetailModel>>(mainModel, subModel);
                result.Datas = _ORT0105BD1ReportModel;
            }
            catch (Exception ex)
            {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }
            return result;
        }

        public string getEXCEPT_ACCT_NOs(string filed)
        {
            string result = string.Empty;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _EXCEPT_ACCT_NOs = db.FRT_CROSS_SYSTEM_EXCEPT_ACCT_NO.AsNoTracking().Select(x => x.bank_acct_no).ToList();
                result = _EXCEPT_ACCT_NOs.Any() ? $@" AND {filed} not in ('{string.Join("','", _EXCEPT_ACCT_NOs)}') " : string.Empty;
            }
            return result;
        }

        private decimal getAbs(decimal? a, decimal? b)
        {
            return Math.Abs((a ?? 0m) - (b ?? 0m));
        }
    }
}