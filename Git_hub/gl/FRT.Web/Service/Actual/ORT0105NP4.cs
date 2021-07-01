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

/// <summary>
/// 功能說明：跨系統勾稽 逾期未兌領
/// 初版作者：20210602 Mark
/// 修改歷程：20210602 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105NP4 : IORT0105Report
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
                                var _sendReport = new GoujiReport().sendReport(_getReportData.Datas, schedulerModel, UserId);
                                resultModel.RETURN_FLAG = _sendReport.RETURN_FLAG;
                                resultModel.DESCRIPTION = _sendReport.DESCRIPTION;
                                break;
                            case ReportType.R:
                                resultModel.Datas = new Tuple<IORT0105ReportModel, bool>( _getReportData.Datas, _getReportData.Datas.model.Item2);
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

        public MSGReturnModel<Tuple<List<ORT0105AP2Model>, List<ORT0105AP2Model>>> getData(FRT_CROSS_SYSTEM_CHECK schedulerModel, string date_s = null, string date_e = null)
        {
            MSGReturnModel<Tuple<List<ORT0105AP2Model>, List<ORT0105AP2Model>>> result = new MSGReturnModel<Tuple<List<ORT0105AP2Model>, List<ORT0105AP2Model>>>();
            List<ORT0105AP2Model> MAINData = new List<ORT0105AP2Model>();
            List<ORT0105AP2Model> WanpieData = new List<ORT0105AP2Model>();

            var datas = new GoujiReport().getReportDate(schedulerModel, date_s, date_e);
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
FEE_SEQN AS FEE_SEQ,
CURRENCY AS CURR,
(INT((LPAD(REMIT_YY,3,'0')) + 1911)  || '-' || (LPAD(REMIT_MM,2,'0')) || '-' || (LPAD(REMIT_DD,2,'0'))) AS REMIT_DATE,
(INT((LPAD(UPD_YY,3,'0')) + 1911) || '-'  || (LPAD(UPD_MM,2,'0')) || '-' || (LPAD(UPD_DD,2,'0'))) AS UPD_DATE,
REMIT_AMT AS AMT
from FRTMAIN0
where ((LPAD(REMIT_YY,3,'0')) || (LPAD(REMIT_MM,2,'0')) || (LPAD(REMIT_DD,2,'0'))) between :DATE_S_1 and :DATE_E_1
and REMIT_STS in ('2','3','4')
AND FEE_SEQN not like 'Q%'
AND FEE_SEQN not like 'W%'
WITH UR;
";
                        com.Parameters.Add("DATE_S_1", datas.Item4);
                        com.Parameters.Add("DATE_E_1", datas.Item5);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105AP2Model();
                            model.FEE_SEQ = dbresult["FEE_SEQ"]?.ToString()?.Trim(); //匯費序號
                            model.CURR = dbresult["CURR"]?.ToString()?.Trim(); //幣別
                            model.REMIT_DATE = dbresult["REMIT_DATE"]?.ToString()?.Trim(); //基準日期
                            model.UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //異動日期
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            MAINData.Add(model);
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }

                using (SqlConnection conn = new SqlConnection(CommonUtil.GetGLSIACTConn()))
                {
                    conn.Open();
                    string sql = $@"
select 
FROM_REMIT_BANK_SERIAL_NO AS FEE_SEQ,
CURR_CODE_1 AS CURR,
CONVERT(varchar, REMIT_MONEY_DATE, 23) AS REMIT_DATE,
NEED_REMIT_AMT AS AMT
FROM GLSIACT..ACC03010101 (nolock)
WHERE REMIT_MONEY_DATE between @DATE_S_2 and @DATE_E_2
AND FROM_REMIT_BANK_SERIAL_NO not like 'Q%'
AND FROM_REMIT_BANK_SERIAL_NO not like 'W%'
";
                    WanpieData = conn.Query<ORT0105AP2Model>(sql, 
                        new {
                            DATE_S_2 = datas.Item2,
                            DATE_E_2 = datas.Item3
                        }, null, true, 3600).ToList();
                }
                result.RETURN_FLAG = true;
                result.Datas = new Tuple<List<ORT0105AP2Model>, List<ORT0105AP2Model>>(MAINData, WanpieData);
            }
            catch (Exception ex) {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }

            return result;
        }

        public MSGReturnModel<ORT0105AP2ReportModel> getReportData(Tuple<List<ORT0105AP2Model>, List<ORT0105AP2Model>> datas)
        {
            bool subReportCheck = false;
            MSGReturnModel<ORT0105AP2ReportModel> result = new MSGReturnModel<ORT0105AP2ReportModel>();
            ORT0105AP2ReportModel _ORT0108ReportModel = new ORT0105AP2ReportModel();
            List<ORTReportBModel> mainModel = new List<ORTReportBModel>();
            List<ORTReportBDetailModel> subModel = new List<ORTReportBDetailModel>();
            List<ORTReportBDetailModel> subModel_fix = new List<ORTReportBDetailModel>();
            try
            {
                //本次資料所有幣別
                List<string> CURR = datas.Item1.Select(x => x.CURR).Distinct()
                    .Union(datas.Item2.Select(x => x.CURR).Distinct())
                    .OrderBy(x => x != "NTD").ThenBy(x => x).ToList();

                #region 主檔

                List<Tuple<string, decimal>> groupSubData = new List<Tuple<string, decimal>>();

                foreach (var _curr in CURR) //每個幣別 會有一張 檢核報表
                {
                    var _main_all = datas.Item1.Where(x => x.CURR == _curr).ToList();  //AS400匯款主檔(FRTMAIN0)
                    var _wanpie_all = datas.Item2.Where(x => x.CURR == _curr).ToList(); //Wanpie匯款檔(匯款單多來源)
                    var _main_c = _main_all.Count;
                    var _wanpie_c = _wanpie_all.Count;
                    var _main_amt = _main_all.Where(x => x.AMT != null).Sum(x => x.AMT) ?? 0m;
                    var _wanpie_amt = _wanpie_all.Where(x => x.AMT != null).Sum(x => x.AMT) ?? 0m;

                    mainModel.Add(new ORTReportBModel()
                    {
                        Item = $@"幣別 : {_curr}",
                    });

                    mainModel.Add(new ORTReportBModel()
                    {
                        Item = $@"匯款總筆數(BY匯費序號)",
                        AS400_File = _main_c.formateThousand(), //AS400匯款主檔(FRTMAIN0) 總筆數
                        Wanpie_File = _wanpie_c.formateThousand(), //Wanpie匯款檔(匯款單多來源) 總筆數
                        Diff_value = Math.Abs(_main_c - _wanpie_c).formateThousand()
                    });

                    mainModel.Add(new ORTReportBModel()
                    {
                        Item = $@"總金額",
                        AS400_File = _main_amt.formateThousand(),
                        Wanpie_File = _wanpie_amt.formateThousand(),
                        Diff_value = Math.Abs(_main_amt - _wanpie_amt).formateThousand()
                    });

                    mainModel.Add(new ORTReportBModel());

                    if ((_main_c != _wanpie_c) || (_main_amt != _wanpie_amt))
                    {
                        decimal d = 0m;
                        foreach (var _FEE_SEQ in _main_all.Select(x => x.FEE_SEQ).Distinct()
                              .Union(_wanpie_all.Select(x => x.FEE_SEQ).Distinct()))
                        {
                            var _main = _main_all.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);
                            var _wanpie = _wanpie_all.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);

                            if (_main == null || _wanpie == null ||
                                (_main.AMT != _wanpie.AMT))
                            {
                                subModel.Add(new ORTReportBDetailModel()
                                {
                                    FEE_SEQ = _FEE_SEQ,
                                    CURR = _curr,
                                    Date_1_L = _main?.REMIT_DATE ?? "-", //匯款日期 (左邊)
                                    Date_2_L = _main?.UPD_DATE ?? "-", //異動日期 (左邊)
                                    AMT_L = _main?.AMT?.formateThousand() ?? "-", //金額 (左邊)
                                    Date_1_R = _wanpie?.REMIT_DATE ?? "-", //匯出日期 (右邊)
                                    AMT_R = _wanpie?.AMT?.formateThousand() ?? "-", //金額 (右邊)
                                    BorderType = "123D"                                  
                                });
                                d += getAbs(_main?.AMT, _wanpie?.AMT);
                            }
                        }
                        groupSubData.Add(new Tuple<string, decimal>(_curr, d));
                    }
                }
                if(mainModel.Any())
                    mainModel.RemoveAt(mainModel.Count - 1);
                if (subModel.Any())
                    subReportCheck = true;
                #region 明細資料整理
                subModel_fix = subModel.OrderBy(x => x.FEE_SEQ).ToList();               
                bool _first = true;
                foreach (var item in groupSubData.OrderBy(x => x.Item1))
                {
                    if (_first)
                    {
                        subModel_fix.Add(new ORTReportBDetailModel()
                        {
                            FEE_SEQ = "差異總計 :",
                            CURR = subModel.Count.ToString().formateThousand(),
                            Date_1_L = "總計金額 :",
                            Date_2_L = $@"{item.Item1}",
                            AMT_L = $@"{item.Item2.formateThousand()}"
                        });
                        _first = false;
                    }
                    else
                    {
                        subModel_fix.Add(new ORTReportBDetailModel()
                        {
                            Date_2_L = $@"{item.Item1}",
                            AMT_L = $@"{item.Item2.formateThousand()}"
                        });
                    }
                }
                #endregion

                #endregion
                result.RETURN_FLAG = true;
                _ORT0108ReportModel.model = new Tuple<List<ORTReportBModel>, bool, List<ORTReportBDetailModel>>(mainModel, subReportCheck, subModel_fix);
                result.Datas = _ORT0108ReportModel;
            }
            catch (Exception ex)
            {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }
            return result;
        }

        private decimal getAbs(decimal? a, decimal? b)
        {
            return Math.Abs((a ?? 0m) - (b ?? 0m));
        }
    }
}