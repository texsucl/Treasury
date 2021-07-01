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
/// 功能說明：跨系統勾稽 已銷帳
/// 初版作者：20210523 Mark
/// 修改歷程：20210523 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105BD1 : IORT0105Report
    {

        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MSGReturnModel<Tuple<IORT0105ReportModel, bool>> check(FRT_CROSS_SYSTEM_CHECK schedulerModel, string UserId, ReportType type = ReportType.S, string date_s = null, string date_e = null, string deadline = null)
        {
            var _ORT0106 = new ORT0106();
            MSGReturnModel<Tuple<IORT0105ReportModel, bool>> resultModel = new MSGReturnModel<Tuple<IORT0105ReportModel, bool>>();
            try{
                _ORT0106.updateRunFlag(schedulerModel, "Y");
                var _getData = getData(schedulerModel, date_s, date_e);
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
                                resultModel.Datas = new Tuple<IORT0105ReportModel, bool>( _getReportData.Datas, false);
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
            catch (Exception ex){
                logger.Error(ex);
                resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();
            }
            finally {
                _ORT0106.updateRunFlag(schedulerModel, "N");
            }
            return resultModel;
        }

        public MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>>> getData(FRT_CROSS_SYSTEM_CHECK schedulerModel, string date_s = null, string date_e = null)
        {
            MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>>> result = new MSGReturnModel<Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>>>();
            List<ORT0105BD1Model> MAINData = new List<ORT0105BD1Model>();
            List<ORT0105BD1Model> WanpieData = new List<ORT0105BD1Model>();

            var datas = new GoujiReport().getReportDate(schedulerModel, date_s, date_e);
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    List<ORT0105BD1Model> MAINData_org = new List<ORT0105BD1Model>();
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
RMS.VRTS_NO AS NO,
RMS.CURRENCY AS CURRENCY,
(select  (LPAD(GL_YY,3,'0')) || (LPAD(GL_MM,2,'0')) ||  (LPAD(GL_DD,2,'0')) 
 FROM FARVRMH0 RMH WHERE RMH.VRTS_NO  = RMS.VRTS_NO
FETCH FIRST 1 ROWS ONLY ) AS DATE,
RMS.PASS_AMT AS AMT
from FARVRMS0 RMS
JOIN FARVRMH0 RMH
on RMS.VRTS_NO = RMH.VRTS_NO 
where ((LPAD(RMS.PASS_YY,3,'0')) || (LPAD(RMS.PASS_MM,2,'0')) || (LPAD(RMS.PASS_DD,2,'0'))) between :DATE_S_1 and :DATE_E_1
{getEXCEPT_ACCT_NOs("RMH.BANK_ACT")}
UNION ALL
select 
EMD.TRAN_SEQ AS NO,
EMD.CURRENCY AS CURRENCY,
(select EMH.WEDAY FROM FARREMH0 EMH 
WHERE EMH.TRAN_SEQ  = EMD.TRAN_SEQ  
FETCH FIRST 1 ROWS ONLY ) AS DATE,
EMD.PASS_AMT_D AS AMT
from FARREMD0 EMD
where ((LPAD(EMD.ENTRY_YY,3,'0')) || (LPAD(EMD.ENTRY_MM,2,'0')) || (LPAD(EMD.ENTRY_DD,2,'0'))) between :DATE_S_1 and :DATE_E_1
{getEXCEPT_ACCT_NOs("EMD.MT_ACNT")}
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
                            if(model.AMT != null && model.AMT != 0m)
                                MAINData_org.Add(model);
                        }
                        com.Dispose();
                    }
                    if (MAINData_org.Any())
                    {
                        foreach (var item in MAINData_org.GroupBy(x => new { x.NO, x.CURRENCY, x.DATE }))
                        {
                            MAINData.Add(new ORT0105BD1Model()
                            {
                                NO  = item.Key.NO,
                                DATE = item.Key.DATE,
                                CURRENCY = item.Key.CURRENCY,
                                AMT = item.Sum(x=>x.AMT)
                            });
                        }
                    }
                    conn.Dispose();
                    conn.Close();
                }

                using (SqlConnection conn = new SqlConnection(CommonUtil.GetGLSIACTConn()))
                {
                    conn.Open();
                    string sql = $@"
with temp as
(
select 
ACC.VOUCHER_NO AS VOUCHER_NO, -- 表單編號
ACC.FROM_XAC279350738229XX_VOUCHER_NO AS VOUCHER_NO_Y_N2, --表單編號(Y至N2的沖銷檔編號)  
(select top 1 JC8.VAR_CODE_NAME from GLSIACT..UUU85020101 JC8 WITH(NOLOCK) where JC8.CORP_NO=ACC.CORP_NO and JC8.VAR_CODE_FIELD_ID='ACC09110901_FROM_TABLE_ID' and JC8.VAR_CODE= ACC.FROM_TABLE_ID) AS VAR_CODE_NAME, --檔案代碼/名稱
case when (select top 1 JC8.VAR_CODE_NAME from GLSIACT..UUU85020101 JC8 WITH(NOLOCK) where JC8.CORP_NO=ACC.CORP_NO and JC8.VAR_CODE_FIELD_ID='ACC09110901_FROM_TABLE_ID' and JC8.VAR_CODE= ACC.FROM_TABLE_ID) = '繳款單'
     then (select top 1 CONVERT(date, _XAC.A_PASS_DATE) from GLSIACT..XAC279350738281 _XAC (nolock) where _XAC.VOUCHER_NO = ACC.FROM_VOUCHER_NO and _XAC.CORP_NO = ACC.CORP_NO)
     else ACC.VOUCHER_DATE
end AS PASS_DATE , --PASS_DATE
(select top 1 CONVERT(date, ACC_2.IO_DATE) from GLSIACT..ACC09050101 ACC_2 (nolock) where ACC_2.VOUCHER_NO = ACC.VOUCHER_NO_ACC09050101 and ACC_2.CORP_NO = ACC.CORP_NO) AS VOUCHER_DATE , --交易日期
CURR_CODE AS CURRENCY, --幣別
VOUCHER_NO_ACC09050101 AS NO, --表單編號(存摺收支) 交易序號 / 代收編號
case when ACC.FROM_TABLE_ID='ACC02010101' and ACC.DR_CR_TYPE<>(select top 1 ACC3.DR_CR_TYPE from GLSIACT..ACC09050101 ACC3 (nolock) where ACC3.CORP_NO=ACC.CORP_NO and ACC3.VOUCHER_NO=ACC.VOUCHER_NO_ACC09050101) 
then -abs(ACC.PLUS_OR_MINUS * ACC.VOUCHER_AMT) 
else  ACC.PLUS_OR_MINUS * ACC.VOUCHER_AMT 
end as AMT --銷帳金額
from GLSIACT..ACC09110901 ACC (nolock) --存摺沖銷檔(非存摺方
WHERE ACC.VOUCHER_DATE between @DATE_S_2 and @DATE_E_2  --沖銷日期
{getEXCEPT_ACCT_NOs("ACC.BANK_ACCT_NO")}
and ISNULL(BECOME_INVALID_YN,'') <> 'Y' --去除作廢
AND ACC.CORP_NO in ('FUBONLIFE','OIU')
--AND ISNULL(FROM_XAC279350738224XX_YN,'') <> 'Y' --N2至Y
--AND ISNULL(FROM_XAC279350738229XX_YN,'') <> 'Y' --Y至N2
)
select 
VOUCHER_NO,
VOUCHER_NO_Y_N2,
VAR_CODE_NAME,
VOUCHER_DATE,
CURRENCY,
NO,
--CASE WHEN VAR_CODE_NAME = '傳票'
--THEN ABS(AMT)
--ELSE AMT 
--END AS AMT
AMT
from temp
where PASS_DATE between @DATE_S_2 and @DATE_E_2  --沖銷日期
";
                    WanpieData = conn.Query<ORT0105BD1Model>(sql, 
                        new {
                            DATE_S_2 = datas.Item2,
                            DATE_E_2 = datas.Item3
                        }, null, true, getTimeout()).ToList();

                    #region 轉雜收 & 雜收反回 
                    foreach (var _g in WanpieData.GroupBy(x => x.NO))
                    {
                        var _a = _g.FirstOrDefault(x => x.VAR_CODE_NAME == "轉雜收");
                        var _b = _g.FirstOrDefault(x => x.VAR_CODE_NAME == "雜收返回");
                        if (_b != null && _a == null) //僅有 雜收返回 無 轉雜收 則排除 雜收返回
                        {
                            _b.AMT = 0m;
                        }
                    }
                    #endregion

                    var _VOUCHER_NO_Y_N2s = WanpieData.Where(x => x.VOUCHER_NO_Y_N2 != null).Select(x => x.VOUCHER_NO_Y_N2).Distinct().ToList();
                    WanpieData = WanpieData.Where(x => 
                    !_VOUCHER_NO_Y_N2s.Contains(x.VOUCHER_NO) && x.VOUCHER_NO_Y_N2 == null && x.AMT != null && x.AMT != 0m ) 
                        .GroupBy(x => new { x.NO, x.VOUCHER_DATE ,x.CURRENCY})
                        .Select(x => new ORT0105BD1Model() { 
                           NO = x.Key.NO,
                           DATE = x.Key.VOUCHER_DATE == null ? string.Empty : $@"{(x.Key.VOUCHER_DATE.Value.Year - 1911)}{x.Key.VOUCHER_DATE.Value.ToString("MMdd")}",
                           AMT = x.Where(z=>z.AMT != null).Sum(y => y.AMT),
                           CURRENCY = x.Key.CURRENCY,                        
                        }).ToList();
                }
                result.RETURN_FLAG = true;
                result.Datas = new Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>>(MAINData, WanpieData);
            }
            catch (Exception ex) {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }

            return result;
        }

        public MSGReturnModel<ORT0105BD1ReportModel> getReportData(Tuple<List<ORT0105BD1Model>, List<ORT0105BD1Model>> datas)
        {
            MSGReturnModel<ORT0105BD1ReportModel> result = new MSGReturnModel<ORT0105BD1ReportModel>();
            ORT0105BD1ReportModel _ORT0105BD1ReportModel = new ORT0105BD1ReportModel();
            ORTReportCModel mainModel = new ORTReportCModel();
            List<ORTReportCDetailModel> subModel = new List<ORTReportCDetailModel>();
            try
            {

                #region 資料整理
                var _as400_amt = datas.Item1.Where(x => x.AMT != null).Sum(x => x.AMT ?? 0m);
                var _wanpie_amt = datas.Item2.Where(x => x.AMT != null).Sum(x => x.AMT ?? 0m);
                mainModel.AS400_AMT = _as400_amt.formateThousand();
                mainModel.Wanpie_AMT = _wanpie_amt.formateThousand();
                mainModel.Diff_AMT = Math.Abs(_as400_amt - _wanpie_amt).formateThousand();
                var _allDatas = datas.Item1.Distinct().Union(datas.Item2.Distinct()).ToList();
                foreach (var item in _allDatas)
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
                        }
                    }
                }

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