using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：AS400_匯款檢核
/// 初版作者：20210406 Mark
/// 修改歷程：20210406 Mark
///           需求單號：202011050211-28
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105AP1 : IORT0105Report
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 呼叫 AS400_匯款檢核
        /// </summary>
        /// <param name="schedulerModel">執行排程</param>
        /// <param name="UserId">執行人員</param>
        /// <param name="type">S => 寄信 , R => 報表</param>
        /// <returns></returns>
        public MSGReturnModel<Tuple<IORT0105ReportModel,bool>> check(FRT_CROSS_SYSTEM_CHECK schedulerModel, string UserId , ReportType type = ReportType.S, string date_s = null, string date_e = null, string deadline = null)
        {
            var _ORT0106 = new ORT0106();
            MSGReturnModel<Tuple<IORT0105ReportModel, bool>> resultModel = new MSGReturnModel<Tuple<IORT0105ReportModel, bool>>();
            try
            {
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
            catch (Exception ex)
            {
                logger.Error(ex);
                resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();
            }
            finally {
                _ORT0106.updateRunFlag(schedulerModel, "N");
            }
            return resultModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schedulerModel"></param>
        /// <returns></returns>
        public MSGReturnModel<Tuple<List<ORT0105AP1Model>,List<ORT0105AP1Model>,List<ORT0105AP1Model>, List<ORT0105AP1Model>>> getData(FRT_CROSS_SYSTEM_CHECK schedulerModel, string date_s = null, string date_e = null)
        {
            MSGReturnModel<Tuple<List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>>> result = new MSGReturnModel<Tuple<List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>>>();
            List<ORT0105AP1Model> MAINDatas = new List<ORT0105AP1Model>(); //匯款主檔
            List<ORT0105AP1Model> RVMNDatas = new List<ORT0105AP1Model>(); //退匯記錄檔
            List<ORT0105AP1Model> REMHDatas = new List<ORT0105AP1Model>(); //A匯款主檔 台幣
            List<ORT0105AP1Model> REFHDatas = new List<ORT0105AP1Model>(); //A匯款主檔 外幣
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    var datas = new GoujiReport().getReportDate(schedulerModel, date_s, date_e);
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
((LPAD(REMIT_YY,3,'0')) || (LPAD(REMIT_MM,2,'0')) || (LPAD(REMIT_DD,2,'0'))) AS DATE,
FEE_SEQN AS FEE_SEQ,
PRO_NO AS PRO_NO,
FBO_NO AS SYS,
REMIT_STS AS STATUS,
REMIT_AMT AS AMT,
CURRENCY AS CURR,
((LPAD(UPD_YY,3,'0')) || (LPAD(UPD_MM,2,'0')) || (LPAD(UPD_DD,2,'0'))) AS UPD_DATE
from FRTMAIN0
where STS_DATE between :DATE_S_1 and :DATE_E_1
and REMIT_STS in ('1','2','3','4')
and FBO_NO <> ''
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
                            var model = new ORT0105AP1Model();
                            model.FileName = "FRTMAIN0"; //匯款主檔
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //匯款日
                            model.FEE_SEQ= dbresult["FEE_SEQ"]?.ToString()?.Trim(); //匯費序號
                            model.PRO_NO = dbresult["PRO_NO"]?.ToString()?.Trim(); //處理序號
                            model.SYS = dbresult["SYS"]?.ToString()?.Trim(); //系統別
                            model.Status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            model.CURR = dbresult["CURR"]?.ToString()?.Trim(); //幣別
                            model.UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //退匯日期
                            MAINDatas.Add(model);
                        }
                        com.Dispose();
                    }

                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        //sql = $@"
                        //select 
                        //FAIL_DATE AS DATE,
                        //FEE_SEQN AS FEE_SEQ,
                        //FILLER1 AS STATUS,
                        //REMIT_AMT AS AMT,
                        //CURRENCY AS CURR,
                        //UPD_DATE AS UPD_DATE
                        //from FRTRVMN0
                        //where UPD_DATE between :DATE_S_2 and :DATE_E_2
                        //and FILLER1 in ('3','4')
                        //AND FEE_SEQN not like 'Q%'
                        //AND FEE_SEQN not like 'W%'
                        //WITH UR;
                        //";

                        sql = $@"
                        select 
                        REMIT_DATE AS DATE,
                        FEE_SEQN AS FEE_SEQ,
                        FILLER1 AS STATUS,
                        REMIT_AMT AS AMT,
                        CURRENCY AS CURR
                        from FRTRVMN0
                        where FAIL_DATE between :DATE_S_2 and :DATE_E_2
                        and FILLER1 = '3'
                        AND FEE_SEQN not like 'Q%'
                        AND FEE_SEQN not like 'W%'
                        UNION ALL
                        select 
                        REMIT_DATE AS DATE,
                        FEE_SEQN AS FEE_SEQ,
                        FILLER1 AS STATUS,
                        REMIT_AMT AS AMT,
                        CURRENCY AS CURR
                        from FRTRVMN0
                        where UPD_DATE between :DATE_S_2 and :DATE_E_2
                        and FILLER1 = '4'
                        AND FEE_SEQN not like 'Q%'
                        AND FEE_SEQN not like 'W%'
                        WITH UR;
                        ";

                        com.Parameters.Add("DATE_S_2", datas.Item4);
                        com.Parameters.Add("DATE_E_2", datas.Item5);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105AP1Model();
                            model.FileName = "FRTVMN0"; //退匯記錄檔
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //匯款日
                            model.FEE_SEQ = dbresult["FEE_SEQ"]?.ToString()?.Trim(); //匯費序號
                            model.Status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            model.CURR = dbresult["CURR"]?.ToString()?.Trim(); //幣別
                            RVMNDatas.Add(model);
                        }
                        com.Dispose();
                    }

                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
REMT_DATE AS DATE,
FB_NO AS PRO_NO,
REMT_STS AS STATUS,
AMOUNT AS AMT,
UPD_DATE AS UPD_DATE
from FFAREMH
where UPD_DATE between :DATE_S_3 and :DATE_E_3
and REMT_STS in ('1','2','3','4')
WITH UR;
";
                        com.Parameters.Add("DATE_S_3", datas.Item4);
                        com.Parameters.Add("DATE_E_3", datas.Item5);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105AP1Model();
                            model.FileName = "FFAREMH"; //匯款檔(台幣)
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //匯款日
                            model.PRO_NO = dbresult["PRO_NO"]?.ToString()?.Trim(); //處理序號
                            model.Status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            model.UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //退匯日期
                            model.CURR = "NTD"; //台幣
                            REMHDatas.Add(model);
                        }
                        com.Dispose();
                    }

                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;

                        sql = $@"
select 
REMT_DATE AS DATE,
FB_NO AS PRO_NO,
REMT_STS AS STATUS,
AMOUNT AS AMT,
ACT_CURR AS CURR,
UPD_DATE AS UPD_DATE
from FFAREFH
where UPD_DATE between :DATE_S_4 and :DATE_E_4
and REMT_STS in ('1','2','3','4')
WITH UR;
";
                        com.Parameters.Add("DATE_S_4", datas.Item4);
                        com.Parameters.Add("DATE_E_4", datas.Item5);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105AP1Model();
                            model.FileName = "FFAREFH"; //匯款檔(外幣)
                            model.DATE = dbresult["DATE"]?.ToString()?.Trim(); //匯款日
                            model.PRO_NO = dbresult["PRO_NO"]?.ToString()?.Trim(); //處理序號
                            model.Status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                            model.AMT = dbresult["AMT"]?.ToString().stringToDecimalN();  //金額
                            model.CURR = dbresult["CURR"]?.ToString()?.Trim();  //幣別
                            model.UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //退匯日期
                            REFHDatas.Add(model);
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                    result.RETURN_FLAG = true;
                    result.Datas = new Tuple<List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>>(MAINDatas, RVMNDatas, REMHDatas, REFHDatas);
                }
            }
            catch (Exception ex)
            {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }
            return result;
        }

        /// <summary>
        /// 由400資料 組出 Tuple<List<檢核主報表>, bool, List<檢核差異明細表>> 
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public MSGReturnModel<ORT0105AP1ReportModel> getReportData(Tuple<List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>, List<ORT0105AP1Model>> datas)
        {
            bool subReportCheck = false;
            MSGReturnModel<ORT0105AP1ReportModel> result = new MSGReturnModel<ORT0105AP1ReportModel>();
            ORT0105AP1ReportModel _ORT0105AP1ReportModel = new ORT0105AP1ReportModel();
            List<ORTReportAModel> mainModel = new List<ORTReportAModel>();
            List<ORTReportADetailModel> subModel = new List<ORTReportADetailModel>();
            List<ORTReportADetailModel> subModel_fix = new List<ORTReportADetailModel>();
            try
            {
                //本次資料所有幣別
                List<string> CURR = datas.Item1.Select(x => x.CURR).Distinct()
                    .Union(datas.Item2.Select(x => x.CURR).Distinct())
                    .Union(datas.Item3.Select(x => x.CURR).Distinct())
                    .Union(datas.Item4.Select(x => x.CURR).Distinct())
                    .OrderBy(x => x != "NTD").ThenBy(x => x).ToList();
                #region 匯款成功

                string curr_item = string.Empty; //A匯款主檔(FFAREMH) or A匯款主檔(FFAREFH)
                string _FRTMAIN = "F匯款主檔(FRTMAIN0)"; //F匯款主檔
                string _FRTRVMN = "退匯記錄檔(FRTRVMN0)"; //退匯記錄檔
                string _FFAREMH = "A匯款主檔(FFAREMH)"; //台幣
                string _FFAREFH = "A匯款主檔(FFAREFH)"; //外幣
                string _Flag = "※";
                int _addspace = 4; 
                var _CURR_Count = CURR.Count;
                int _int = 0;

                foreach (var _curr in CURR) //每個幣別 會有一張 檢核報表
                {
                    _int += 1;

                    bool curr_NTD = (_curr == "NTD");

                    #region F匯款主檔(FRTMAIN0) All (含 (1)待匯款 , (2)已匯款 , (3)退匯 , (4)退匯已處理)
                    //F匯款主檔(FRTMAIN0) (含A&F的資料)
                    var main_all = datas.Item1.Where(x => x.CURR == _curr);
                    var main_all_count = 0;
                    var main_all_amt = 0m;
                    //F匯款主檔(FRTMAIN0) F系統                   
                    var main_all_f_count = 0;
                    var main_all_f_amt = 0m; 
                    //F匯款主檔(FRTMAIN0) A系統
                    var main_all_a_count = 0;
                    var main_all_a_amt = 0m;
                    #endregion

                    #region F匯款主檔(FRTMAIN0) (1)待匯款
                    //F匯款主檔(FRTMAIN0) A+F系統 (1)待匯款
                    var main_w_all_count = 0;
                    var main_w_all_amt = 0m;
                    //F匯款主檔(FRTMAIN0) F系統 (1)待匯款
                    var main_w_f = new List<ORT0105AP1Model>();
                    var main_w_f_count = 0;
                    var main_w_f_amt = 0m; 
                    //F匯款主檔(FRTMAIN0) A系統 (1)待匯款
                    var main_w_a = new List<ORT0105AP1Model>();
                    var main_w_a_count = 0;
                    var main_w_a_amt = 0m;
                    #endregion

                    #region F匯款主檔(FRTMAIN0) (2)已匯款
                    //F匯款主檔(FRTMAIN0) A+F系統 (2)已匯款
                    var main_s_all_count = 0;
                    var main_s_all_amt = 0m;
                    //F匯款主檔(FRTMAIN0) F系統 (2)已匯款
                    var main_s_f = new List<ORT0105AP1Model>();
                    var main_s_f_count = 0;
                    var main_s_f_amt = 0m; 
                    //F匯款主檔(FRTMAIN0) A系統 (2)已匯款
                    var main_s_a = new List<ORT0105AP1Model>();
                    var main_s_a_count = 0;
                    var main_s_a_amt = 0m;
                    #endregion

                    #region F匯款主檔(FRTMAIN0) (3)退匯
                    //F匯款主檔(FRTMAIN0) A+F系統 (3)退匯
                    var main_f_all_count = 0;
                    var main_f_all_amt = 0m;
                    //F匯款主檔(FRTMAIN0) F系統 (3)退匯
                    var main_f_f = new List<ORT0105AP1Model>();
                    var main_f_f_count = 0;
                    var main_f_f_amt = 0m; 
                    //F匯款主檔(FRTMAIN0) A系統 (3)退匯
                    var main_f_a = new List<ORT0105AP1Model>();
                    var main_f_a_count = 0;
                    var main_f_a_amt = 0m;
                    #endregion

                    #region F匯款主檔(FRTMAIN0) (4)退匯已處理
                    //F匯款主檔(FRTMAIN0) A+F系統 (4)退匯已處理
                    var main_fs_all_count = 0;
                    var main_fs_all_amt = 0m;
                    //F匯款主檔(FRTMAIN0) F系統 (4)退匯已處理
                    var main_fs_f = new List<ORT0105AP1Model>();
                    var main_fs_f_count = 0;
                    var main_fs_f_amt = 0m;
                    //F匯款主檔(FRTMAIN0) A系統 (4)退匯已處理
                    var main_fs_a = new List<ORT0105AP1Model>();
                    var main_fs_a_count = 0;
                    var main_fs_a_amt = 0m;
                    #endregion

                    #region F匯款主檔(FRTMAIN0) 資料
                    foreach (var _all in main_all)
                    {
                        bool _check = true;
                        decimal _amt = 0m;
                        if (_all.SYS == "F")
                        {
                            switch (_all.Status)
                            {
                                case "1": //(1)待匯款
                                    main_w_f.Add(_all);
                                    _amt = getAmt(_all);
                                    main_w_f_amt += _amt;
                                    break;
                                case "2": //(2)已匯款
                                    main_s_f.Add(_all);
                                    _amt = getAmt(_all);
                                    main_s_f_amt += _amt;
                                    break;
                                case "3": //(3)退匯
                                    main_f_f.Add(_all);
                                    _amt = getAmt(_all);
                                    main_f_f_amt += _amt;
                                    break;
                                case "4": //(4)退匯已處理
                                    main_fs_f.Add(_all);
                                    _amt = getAmt(_all);
                                    main_fs_f_amt += _amt;
                                    break;
                                default:
                                    _check = false;
                                    break;
                            }
                            if (_check)
                            {
                                main_all_f_amt += _amt;
                            }
                        }
                        else if (_all.SYS == "A")
                        {
                            switch (_all.Status)
                            {
                                case "1": //(1)待匯款
                                    main_w_a.Add(_all);
                                    _amt = getAmt(_all);
                                    main_w_a_amt += _amt;
                                    break;
                                case "2": //(2)已匯款
                                    main_s_a.Add(_all);
                                    _amt = getAmt(_all);
                                    main_s_a_amt += _amt;
                                    break;
                                case "3": //(3)退匯
                                    main_f_a.Add(_all);
                                    _amt = getAmt(_all);
                                    main_f_a_amt += _amt;
                                    break;
                                case "4": //(4)退匯已處理
                                    main_fs_a.Add(_all);
                                    _amt = getAmt(_all);
                                    main_fs_a_amt += _amt;
                                    break;
                                default:
                                    _check = false;
                                    break;
                            }
                            if (_check)
                            {
                                main_all_a_amt += _amt;
                            }
                        }
                    }

                    main_w_f_count = main_w_f.Count;
                    main_s_f_count = main_s_f.Count;
                    main_f_f_count = main_f_f.Count;
                    main_fs_f_count = main_fs_f.Count;
                    main_all_f_count = (main_w_f_count + main_s_f_count + main_f_f_count + main_fs_f_count);
                    main_w_a_count = main_w_a.Count;
                    main_s_a_count = main_s_a.Count;
                    main_f_a_count = main_f_a.Count;
                    main_fs_a_count = main_fs_a.Count;
                    main_w_all_count = main_w_f_count + main_w_a_count;
                    main_w_all_amt = main_w_f_amt + main_w_a_amt;
                    main_s_all_count = main_s_f_count + main_s_a_count;
                    main_s_all_amt = main_s_f_amt + main_s_a_amt;
                    main_f_all_count = main_f_f_count + main_f_a_count;
                    main_f_all_amt = main_f_f_amt + main_f_a_amt;
                    main_fs_all_count = main_fs_f_count + main_fs_a_count;
                    main_fs_all_amt = main_fs_f_amt + main_fs_a_amt;
                    main_all_a_count = (main_w_a_count + main_s_a_count + main_f_a_count + main_fs_a_count);
                    main_all_count = (main_all_f_count + main_all_a_count);
                    main_all_amt = main_all_a_amt + main_all_f_amt;
                    #endregion

                    #region 退匯記錄檔(FRTRVMN0) All (含 (3)退匯 , (4)退匯已處理)
                    //退匯記錄檔(FRTRVMN0) 只有匯款成功
                    var rvmn_all = datas.Item2.Where(x => x.CURR == _curr);
                    #endregion

                    #region 退匯記錄檔(FRTRVMN0) 無系統別區分 (3)退匯
                    //退匯記錄檔(FRTRVMN0) (3)退匯
                    var rvmn_f_all = new List<ORT0105AP1Model>();
                    var rvmn_f_all_count = 0;
                    var rvmn_f_all_amt = 0m;
                    #endregion

                    #region 退匯記錄檔(FRTRVMN0) 無系統別區分 (4)退匯已處理
                    // 退匯記錄檔(FRTRVMN0) (4)退匯已處理
                    var rvmn_fs_all = new List<ORT0105AP1Model>();
                    var rvmn_fs_all_count = 0;
                    var rvmn_fs_all_amt = 0m;
                    #endregion

                    #region 退匯記錄檔(FRTRVMN0) 資料
                    foreach (var _all in rvmn_all)
                    {
                        var _amt = getAmt(_all);
                        switch (_all.Status)
                        {
                            case "3": //(3)退匯
                                rvmn_f_all.Add(_all);
                                rvmn_f_all_amt += _amt;
                                break;
                            case "4": //(4)退匯已處理
                                rvmn_fs_all.Add(_all);
                                rvmn_fs_all_amt += _amt;
                                break;
                        }
                    }
                    rvmn_f_all_count = rvmn_f_all.Count;
                    rvmn_fs_all_count = rvmn_fs_all.Count;
                    #endregion

                    #region A匯款主檔(FFAREFH) 台/外幣 只有A系統 All (含 (1)待匯款 , (2)已匯款 , (3)退匯 , (4)退匯已處理)
                    //A匯款主檔(FFAREFH) 外幣 只有A系統
                    List<ORT0105AP1Model> ffar_all_a = new List<ORT0105AP1Model>();
                    #endregion

                    #region A匯款主檔(FFAREFH) 台/外幣 只有A系統 (1)待匯款
                    // A匯款主檔(FFAREFH) 外幣 只有A系統 (1)待匯款
                    List<ORT0105AP1Model> ffar_w_a = new List<ORT0105AP1Model>();
                    var ffar_w_a_count = 0;
                    var ffar_w_a_amt = 0m;
                    #endregion

                    #region A匯款主檔(FFAREFH) 台/外幣 只有A系統 (2)已匯款
                    //A匯款主檔(FFAREFH) 外幣 只有A系統 (2)已匯款
                    List<ORT0105AP1Model> ffar_s_a = new List<ORT0105AP1Model>();
                    var ffar_s_a_count = 0;
                    var ffar_s_a_amt = 0m;
                    #endregion

                    #region A匯款主檔(FFAREFH) 台/外幣 只有A系統 (3)退匯
                    //A匯款主檔(FFAREFH) 外幣 只有A系統 (3)退匯
                    List<ORT0105AP1Model> ffar_f_a = new List<ORT0105AP1Model>();
                    var ffar_f_a_count = 0;
                    var ffar_f_a_amt = 0m;
                    #endregion

                    #region A匯款主檔(FFAREFH) 台/外幣 只有A系統 (4)退匯已處理
                    //A匯款主檔(FFAREFH) 外幣 只有A系統 (4)退匯已處理
                    List<ORT0105AP1Model> ffar_fs_a = new List<ORT0105AP1Model>();
                    var ffar_fs_a_count = 0;
                    var ffar_fs_a_amt = 0m;
                    #endregion

                    if (curr_NTD) //A匯款主檔(FFAREMH) 台幣
                    {
                        curr_item = _FFAREMH;

                        foreach (var _all in datas.Item3)
                        {
                            decimal _amt = getAmt(_all);
                            switch (_all.Status)
                            {
                                case "1": //(1)待匯款
                                    ffar_w_a.Add(_all);
                                    ffar_w_a_amt += _amt;
                                    break;
                                case "2": //(2)已匯款
                                    ffar_s_a.Add(_all);
                                    ffar_s_a_amt += _amt;
                                    break;
                                case "3": //(3)退匯
                                    ffar_f_a.Add(_all);
                                    ffar_f_a_amt += _amt;
                                    break;
                                case "4": //(4)退匯已處理
                                    ffar_fs_a.Add(_all);
                                    ffar_fs_a_amt += _amt;
                                    break;
                            }
                        }
                    }
                    else  //A匯款主檔(FFAREFH) 外幣
                    {
                        curr_item = _FFAREFH;

                        foreach (var _all in datas.Item4.Where(x => x.CURR == _curr))
                        {
                            decimal _amt = getAmt(_all);
                            switch (_all.Status)
                            {
                                case "1": //(1)待匯款
                                    ffar_w_a.Add(_all);
                                    ffar_w_a_amt += _amt;
                                    break;
                                case "2": //(2)已匯款
                                    ffar_s_a.Add(_all);
                                    ffar_s_a_amt += _amt;
                                    break;
                                case "3": //(3)退匯
                                    ffar_f_a.Add(_all);
                                    ffar_f_a_amt += _amt;
                                    break;
                                case "4": //(4)退匯已處理
                                    ffar_fs_a.Add(_all);
                                    ffar_fs_a_amt += _amt;
                                    break;
                            }
                        }
                    }

                    ffar_w_a_count = ffar_w_a.Count;
                    ffar_s_a_count = ffar_s_a.Count;
                    ffar_f_a_count = ffar_f_a.Count;
                    ffar_fs_a_count = ffar_fs_a.Count;

                    //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (1)待匯款 
                    var main_ffar_w_title = main_w_a_count != ffar_w_a_count || main_w_a_amt != ffar_w_a_amt;
                    //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (2)已匯款 
                    var main_ffar_s_title = main_s_a_count != ffar_s_a_count || main_s_a_amt != ffar_s_a_amt;
                    //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (3)退匯
                    var main_ffar_f_title = main_f_a_count != ffar_f_a_count || main_f_a_amt != ffar_f_a_amt;
                    //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (4)退匯已處理
                    var main_ffar_fs_title = main_fs_a_count != ffar_fs_a_count || main_fs_a_amt != ffar_fs_a_amt;
                    //F匯款主檔(FRTMAIN0) & 退匯記錄檔(FRTRVMN0) => (3)退匯
                    var main_rvmn_f_title = main_f_all_count != rvmn_f_all_count || main_f_all_amt != rvmn_f_all_amt;
                    //F匯款主檔(FRTMAIN0) & 退匯記錄檔(FRTRVMN0) => (4)退匯已處理
                    var main_rvmn_fs_title = main_fs_all_count != rvmn_fs_all_count || main_fs_all_amt != rvmn_fs_all_amt;

                    #region 主表
                    //幣別
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        FileName = $@"幣別:{_curr}",
                        AF_amt = "A+F系統",
                        A_amt = "A系統",
                        F_amt = "F系統",
                        BorderType = "345"
                    });

                    //Title
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        FileName = "檔案別",
                        Item = "項目",
                        AF_count = "筆數",
                        AF_amt = "金額",
                        A_count = "筆數",
                        A_amt = "金額",
                        F_count = "筆數",
                        F_amt = "金額",
                        BorderType = "12345",
                        PositionType = "M1"
                    });

                    //F匯款主檔(FRTMAIN0) 匯款狀態
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        FileName = _FRTMAIN,
                        Item = "匯款狀態",
                        AF_count = main_all_count.formateThousand(_curr),
                        AF_amt = main_all_amt.formateThousand(_curr),
                        A_count = main_all_a_count.formateThousand(_curr),
                        A_amt = main_all_a_amt.formateThousand(_curr),
                        F_count = main_all_f_count.formateThousand(_curr),
                        F_amt = main_all_f_amt.formateThousand(_curr),
                        BorderType = "2345"
                    });

                    //F匯款主檔(FRTMAIN0) (1)待匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(1)待匯款",
                        AF_count = main_w_all_count.formateThousand(_curr),
                        AF_amt = main_w_all_amt.formateThousand(_curr),
                        A_count = main_w_a_count.formateThousand(_curr),
                        A_amt = main_w_a_amt.formateThousand(_curr),
                        F_count = main_w_f_count.formateThousand(_curr),
                        F_amt = main_w_f_amt.formateThousand(_curr),
                        //ColorType = main_ffar_w_title ? _Flag : null
                    });

                    //F匯款主檔(FRTMAIN0) (2)已匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(2)已匯款",
                        AF_count = main_s_all_count.formateThousand(_curr),
                        AF_amt = main_s_all_amt.formateThousand(_curr),
                        A_count = main_s_a_count.formateThousand(_curr),
                        A_amt = main_s_a_amt.formateThousand(_curr),
                        F_count = main_s_f_count.formateThousand(_curr),
                        F_amt = main_s_f_amt.formateThousand(_curr),
                        //ColorType = main_ffar_s_title ? _Flag : null
                    });

                    //F匯款主檔(FRTMAIN0) (3)退匯
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(3)退匯",
                        AF_count = main_f_all_count.formateThousand(_curr),
                        AF_amt = main_f_all_amt.formateThousand(_curr),
                        A_count = main_f_a_count.formateThousand(_curr),
                        A_amt = main_f_a_amt.formateThousand(_curr),
                        F_count = main_f_f_count.formateThousand(_curr),
                        F_amt = main_f_f_amt.formateThousand(_curr),
                        //ColorType = (main_ffar_f_title || main_rvmn_f_title) ? _Flag : null
                    });

                    //F匯款主檔(FRTMAIN0) (4)退匯已處理
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(4)退匯已處理",
                        AF_count = main_fs_all_count.formateThousand(_curr),
                        AF_amt = main_fs_all_amt.formateThousand(_curr),
                        A_count = main_fs_a_count.formateThousand(_curr),
                        A_amt = main_fs_a_amt.formateThousand(_curr),
                        F_count = main_fs_f_count.formateThousand(_curr),
                        F_amt = main_fs_f_amt.formateThousand(_curr),
                        //ColorType = (main_ffar_fs_title || main_rvmn_fs_title) ? _Flag : null
                    });

                    mainModel.Add(new ORTReportAModel() { CURR = _curr});

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) 匯款狀態
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        FileName = curr_item,
                        Item = "匯款狀態",
                        A_count = "筆數",
                        A_amt = "金額",
                        BorderType = "24",
                        PositionType = "M2"
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (1)待匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(1)待匯款",
                        A_count = ffar_w_a_count.formateThousand(_curr),
                        A_amt = ffar_w_a_amt.formateThousand(_curr),
                        ColorType = main_ffar_w_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (2)已匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(2)已匯款",
                        A_count = ffar_s_a_count.formateThousand(_curr),
                        A_amt = ffar_s_a_amt.formateThousand(_curr),
                        ColorType = main_ffar_s_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (3)退匯
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(3)退匯",
                        A_count = ffar_f_a_count.formateThousand(_curr),
                        A_amt = ffar_f_a_amt.formateThousand(_curr),
                        ColorType = main_ffar_f_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (4)退匯已處理
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(4)退匯已處理",
                        A_count = ffar_fs_a_count.formateThousand(_curr),
                        A_amt = ffar_fs_a_amt.formateThousand(_curr),
                        BorderType = "24",
                        ColorType = main_ffar_fs_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) 差異項:(1)待匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(1)待匯款",
                        A_count = getCompare(main_w_a_count , ffar_w_a_count, _curr),
                        A_amt = getCompare(main_w_a_amt , ffar_w_a_amt, _curr),
                        ColorType = main_ffar_w_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) 差異項:(2)已匯款
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(2)已匯款",
                        A_count = getCompare(main_s_a_count , ffar_s_a_count, _curr),
                        A_amt = getCompare(main_s_a_amt , ffar_s_a_amt, _curr),
                        ColorType = main_ffar_s_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) 差異項:(3)退匯
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(3)退匯",
                        A_count = getCompare(main_f_a_count , ffar_f_a_count, _curr),
                        A_amt = getCompare(main_f_a_amt , ffar_f_a_amt, _curr),
                        ColorType = main_ffar_f_title ? _Flag : null
                    });

                    //(A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) 差異項:(4)退匯已處理
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(4)退匯已處理",
                        A_count = getCompare(main_fs_a_count , ffar_fs_a_count, _curr),
                        A_amt = getCompare(main_fs_a_amt , ffar_fs_a_amt, _curr),
                        ColorType = main_ffar_fs_title ? _Flag : null
                    });

                    mainModel.Add(new ORTReportAModel() { CURR = _curr});

                    //退匯記錄檔(FRTRVMN0) 匯款狀態
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        FileName = _FRTRVMN,
                        Item = "匯款狀態",
                        AF_count = "筆數",
                        AF_amt = "金額",
                        BorderType = "23",
                        PositionType = "M3"
                    });

                    //退匯記錄檔(FRTRVMN0) (3)退匯
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(3)退匯",
                        AF_count = rvmn_f_all_count.formateThousand(_curr),
                        AF_amt = rvmn_f_all_amt.formateThousand(_curr),
                        ColorType = main_rvmn_f_title ? _Flag : null
                    });

                    //退匯記錄檔(FRTRVMN0) (4)退匯已處理
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "(4)退匯已處理",
                        AF_count = rvmn_fs_all_count.formateThousand(_curr),
                        AF_amt = rvmn_fs_all_amt.formateThousand(_curr),
                        ColorType = main_rvmn_fs_title ? _Flag : null,
                        BorderType = "23",
                    });

                    //退匯記錄檔(FRTRVMN0) 差異項:(3)退匯
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(3)退匯",
                        AF_count = getCompare(main_f_all_count , rvmn_f_all_count, _curr),
                        AF_amt = getCompare(main_f_all_amt , rvmn_f_all_amt, _curr),
                        ColorType = main_rvmn_f_title ? _Flag : null
                    });

                    //退匯記錄檔(FRTRVMN0) 差異項:(4)退匯已處理
                    mainModel.Add(new ORTReportAModel()
                    {
                        CURR = _curr,
                        Item = "差異項:(4)退匯已處理",
                        AF_count = getCompare(main_fs_all_count , rvmn_fs_all_count, _curr),
                        AF_amt = getCompare(main_fs_all_amt , rvmn_fs_all_amt, _curr),
                        ColorType = main_rvmn_fs_title ? _Flag : null
                    });
                    #endregion

                    //if (_CURR_Count != _int)
                    //{
                    //    for (int _i = 0; _i < _addspace; _i++)
                    //    {
                    //        mainModel.Add(new ORTReportAModel());
                    //    }
                    //}

                    #region 明細資料
                    if (main_ffar_w_title) //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (1)待匯款 
                    {
                        foreach (var _PRO_NO in main_w_a.Select(x => x.PRO_NO).Distinct()
                            .Union(ffar_w_a.Select(x => x.PRO_NO).Distinct()))
                        {
                            var _main = main_w_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            var _ffar = ffar_w_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            if (_main == null || _ffar == null || ((_main.AMT ?? 0m) != (_main.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "1",
                                    FEE_SEQ = _main?.FEE_SEQ,
                                    PRO_NO = _PRO_NO,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _ffar?.DATE,
                                    UPDDate_R = _ffar?.UPD_DATE,
                                    Amt_R = _ffar?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _ffar?.AMT,
                                    BorderType = "12345D"
                                });
                            }
                        }
                    }
                    if (main_ffar_s_title)  //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (2)已匯款 
                    {
                        foreach (var _PRO_NO in main_s_a.Select(x => x.PRO_NO).Distinct()
                            .Union(ffar_s_a.Select(x => x.PRO_NO).Distinct()))
                        {
                            var _main = main_s_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            var _ffar = ffar_s_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            if (_main == null || _ffar == null || ((_main.AMT ?? 0m) != (_main.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "2",
                                    FEE_SEQ = _main?.FEE_SEQ,
                                    PRO_NO = _PRO_NO,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _ffar?.DATE,
                                    UPDDate_R = _ffar?.UPD_DATE,
                                    Amt_R = _ffar?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _ffar?.AMT,
                                    BorderType = "12345D"
                                });
                            }
                        }
                    }
                    if (main_ffar_f_title) //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (3_1)退匯
                    {
                        foreach (var _PRO_NO in main_f_a.Select(x => x.PRO_NO).Distinct()
                            .Union(ffar_f_a.Select(x => x.PRO_NO).Distinct()))
                        {
                            var _main = main_f_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            var _ffar = ffar_f_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            if (_main == null || _ffar == null || ((_main.AMT ?? 0m) != (_main.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "3_1",
                                    FEE_SEQ = _main?.FEE_SEQ,
                                    PRO_NO = _PRO_NO,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _ffar?.DATE,
                                    UPDDate_R = _ffar?.UPD_DATE,
                                    Amt_R = _ffar?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _ffar?.AMT,
                                    BorderType = "12345D"
                                });
                            }
                        }
                    }
                    if (main_ffar_fs_title) //F匯款主檔(FRTMAIN0) & (A匯款主檔(FFAREMH) 台幣 or A匯款主檔(FFAREFH) 外幣) => (4_1)退匯已處理
                    {
                        foreach (var _PRO_NO in main_fs_a.Select(x => x.PRO_NO).Distinct()
                            .Union(ffar_fs_a.Select(x => x.PRO_NO).Distinct()))
                        {
                            var _main = main_fs_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            var _ffar = ffar_fs_a.FirstOrDefault(x => x.PRO_NO == _PRO_NO);
                            if (_main == null || _ffar == null || ((_main.AMT ?? 0m) != (_main.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "4_1",
                                    FEE_SEQ = _main?.FEE_SEQ,
                                    PRO_NO = _PRO_NO,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _ffar?.DATE,
                                    UPDDate_R = _ffar?.UPD_DATE,
                                    Amt_R = _ffar?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _ffar?.AMT,
                                    BorderType = "12345D"
                                });
                            }
                        }
                    }
                    if (main_rvmn_f_title)  //F匯款主檔(FRTMAIN0) & 退匯記錄檔(FRTRVMN0) => (3_2)退匯
                    {
                        foreach (var _FEE_SEQ in main_f_f.Select(x => x.FEE_SEQ).Distinct()
                            .Union(main_f_a.Select(x => x.FEE_SEQ).Distinct())
                            .Union(rvmn_f_all.Select(x => x.FEE_SEQ).Distinct()))
                        {
                            var _main = main_f_f.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ) ?? main_f_a.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);
                            var _rvmn = rvmn_f_all.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);
                            if (_main == null || _rvmn == null || ((_main.AMT ?? 0m) != (_rvmn.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "3_2",
                                    FEE_SEQ = _FEE_SEQ,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _rvmn?.DATE,
                                    UPDDate_R = _rvmn?.UPD_DATE,
                                    Amt_R = _rvmn?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _rvmn?.AMT,
                                    BorderType = "1234D"
                                });
                            }
                        }
                    }
                    if (main_rvmn_fs_title) //F匯款主檔(FRTMAIN0) & 退匯記錄檔(FRTRVMN0) => (4_2)退匯已處理
                    {
                        foreach (var _FEE_SEQ in main_fs_f.Select(x => x.FEE_SEQ).Distinct()
                            .Union(main_fs_a.Select(x => x.FEE_SEQ).Distinct())
                            .Union(rvmn_fs_all.Select(x => x.FEE_SEQ).Distinct()))
                        {
                            var _main = main_fs_f.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ) ?? main_fs_a.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);
                            var _rvmn = rvmn_fs_all.FirstOrDefault(x => x.FEE_SEQ == _FEE_SEQ);
                            if (_main == null || _rvmn == null || ((_main.AMT ?? 0m) != (_rvmn.AMT ?? 0m)))
                            {
                                subModel.Add(new ORTReportADetailModel()
                                {
                                    Status = "4_2",
                                    FEE_SEQ = _FEE_SEQ,
                                    CURR = _curr,
                                    Date_L = _main?.DATE,
                                    UPDDate_L = _main?.UPD_DATE,
                                    Amt_L = _main?.AMT?.formateThousand(_curr),
                                    Amt_L_D = _main?.AMT,
                                    Date_R = _rvmn?.DATE,
                                    UPDDate_R = _rvmn?.UPD_DATE,
                                    Amt_R = _rvmn?.AMT?.formateThousand(_curr),
                                    Amt_R_D = _rvmn?.AMT,
                                    BorderType = "1234D"
                                });
                            }
                        }
                    }
                    #endregion
                }

                if (subModel.Any())
                    subReportCheck = true;
                #region 明細資料整理
                foreach (var items in subModel.GroupBy(x => x.Status).OrderBy(x => x.Key))
                {
                    var _t1 = new ORTReportADetailModel();
                    var _t2 = new ORTReportADetailModel();

                    switch (items.Key)
                    {
                        case "1":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(1)待匯款",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                                BorderType = "345S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(1)待匯款",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                            //    BorderType = "345S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                PRO_NO = "FB流水號",
                                BorderType = "12345S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    PRO_NO = "FB流水號",
                            //    BorderType = "12345S",
                            //    PositionType = "M1"
                            //});
                            break;
                        case "2":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(2)已匯款",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                                BorderType = "345S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(2)已匯款",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                            //    BorderType = "345S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                PRO_NO = "FB流水號",
                                BorderType = "12345S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    PRO_NO = "FB流水號",
                            //    BorderType = "12345S",
                            //    PositionType = "M1"
                            //});
                            break;
                        case "3_1":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(3)退匯",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                                BorderType = "345S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(3)退匯",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                            //    BorderType = "345S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                PRO_NO = "FB流水號",
                                BorderType = "12345S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    PRO_NO = "FB流水號",
                            //    BorderType = "12345S",
                            //    PositionType = "M1"
                            //});
                            break;
                        case "3_2":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(3)退匯",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "退匯記錄檔\r\n(FRTRVMN0)",
                                BorderType = "34S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(3)退匯",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "退匯記錄檔\r\n(FRTRVMN0)",
                            //    BorderType = "34S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                BorderType = "1234S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    BorderType = "1234S",
                            //    PositionType = "M1"
                            //});
                            break;
                        case "4_1":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(4)退匯已處理",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                                BorderType = "345S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(4)退匯已處理",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "A匯款主檔\r\n(FFAREMH/FFAREFH)",
                            //    BorderType = "345S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                PRO_NO = "FB流水號",
                                BorderType = "12345S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    PRO_NO = "FB流水號",
                            //    BorderType = "12345S",
                            //    PositionType = "M1"
                            //});
                            break;
                        case "4_2":
                            _t1 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "(4)退匯已處理",
                                UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                                UPDDate_R = "退匯記錄檔\r\n(FRTRVMN0)",
                                BorderType = "34S"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "(4)退匯已處理",
                            //    UPDDate_L = "F匯款主檔\r\n(FRTMAIN0)",
                            //    UPDDate_R = "退匯記錄檔\r\n(FRTRVMN0)",
                            //    BorderType = "34S"
                            //});
                            _t2 = new ORTReportADetailModel()
                            {
                                //Status = items.Key,
                                FEE_SEQ = "匯費序號",
                                CURR = "幣別",
                                Date_L = "匯款日期",
                                UPDDate_L = "異動日期",
                                Amt_L = "金額",
                                Date_R = "匯款日期",
                                UPDDate_R = "異動日期",
                                Amt_R = "金額",
                                BorderType = "1234S",
                                PositionType = "M1"
                            };
                            //subModel_fix.Add(new ORTReportADetailModel()
                            //{
                            //    Status = items.Key,
                            //    FEE_SEQ = "匯費序號",
                            //    CURR = "幣別",
                            //    Date_L = "匯款日期",
                            //    UPDDate_L = "異動日期",
                            //    Amt_L = "金額",
                            //    Date_R = "匯款日期",
                            //    UPDDate_R = "異動日期",
                            //    Amt_R = "金額",
                            //    BorderType = "1234S",
                            //    PositionType = "M1"
                            //});
                            break;
                    }
                    List<Tuple<string, List<decimal>>> groupCurr = new List<Tuple<string, List<decimal>>>();

                    var _status = string.Empty;
                    int size = 20;
                    var _items = items.OrderBy(x => x.FEE_SEQ).ToList();
                    var _l = 0;
                    for (int q = 0; (_items.Count() / size) >= q; q += 1)
                    {
                        _l = q;
                        _status = $@"{items.Key}_{q}";
                        var _t1c = _t1.ModelConvert<ORTReportADetailModel, ORTReportADetailModel>();
                        var _t2c = _t2.ModelConvert<ORTReportADetailModel, ORTReportADetailModel>();
                        _t1c.Status = _status;
                        _t2c.Status = _status;
                        subModel_fix.Add(_t1c);
                        subModel_fix.Add(_t2c);
                        foreach (var item in _items.Skip((q) * size).Take(size))
                        {
                            if (item.Status == "1" || item.Status == "2" || item.Status == "3_1" || item.Status == "4_1")
                            {
                                subModel_fix.Add(new ORTReportADetailModel()
                                {
                                    Status = _status,
                                    FEE_SEQ = item.FEE_SEQ,
                                    CURR = item.CURR,
                                    UPDDate_L = item.UPDDate_L,
                                    Date_L = item.Date_L,
                                    Amt_L = item.Amt_L,
                                    UPDDate_R = item.UPDDate_R,
                                    Date_R = item.Date_R,
                                    Amt_R = item.Amt_R,
                                    PRO_NO = item.PRO_NO,
                                    BorderType = "12345D"
                                });
                            }
                            else if (item.Status == "3_2" || item.Status == "4_2")
                            {
                                subModel_fix.Add(new ORTReportADetailModel()
                                {
                                    Status = _status,
                                    FEE_SEQ = item.FEE_SEQ,
                                    CURR = item.CURR,
                                    UPDDate_L = item.UPDDate_L,
                                    Date_L = item.Date_L,
                                    Amt_L = item.Amt_L,
                                    UPDDate_R = item.UPDDate_R,
                                    Date_R = item.Date_R,
                                    Amt_R = item.Amt_R,
                                    PRO_NO = item.PRO_NO,
                                    BorderType = "1234D"
                                });
                            }
                            var _groupCurr = groupCurr.FirstOrDefault(x => x.Item1 == item.CURR);
                            if (_groupCurr != null)
                            {
                                _groupCurr.Item2.Add(getAbs(item.Amt_L_D, item.Amt_R_D));
                            }
                            else
                            {
                                groupCurr.Add(new Tuple<string, List<decimal>>(item.CURR, new List<decimal>() { getAbs(item.Amt_L_D, item.Amt_R_D) }));
                            }
                        }
                    }

                    //foreach (var item in items.OrderBy(x => x.FEE_SEQ))
                    //{
                    //    if (item.Status == "1" || item.Status == "2" || item.Status == "3_1" || item.Status == "4_1")
                    //    {
                    //        subModel_fix.Add(new ORTReportADetailModel()
                    //        {
                    //            Status = item.Status,
                    //            FEE_SEQ = item.FEE_SEQ,
                    //            CURR = item.CURR,
                    //            UPDDate_L = item.UPDDate_L,
                    //            Date_L = item.Date_L,
                    //            Amt_L = item.Amt_L,
                    //            UPDDate_R = item.UPDDate_R,
                    //            Date_R = item.Date_R,
                    //            Amt_R = item.Amt_R,
                    //            PRO_NO = item.PRO_NO,
                    //            BorderType = "12345D"
                    //        });
                    //    }
                    //    else if (item.Status == "3_2" || item.Status == "4_2")
                    //    {
                    //        subModel_fix.Add(new ORTReportADetailModel()
                    //        {
                    //            Status = item.Status,
                    //            FEE_SEQ = item.FEE_SEQ,
                    //            CURR = item.CURR,
                    //            UPDDate_L = item.UPDDate_L,
                    //            Date_L = item.Date_L,
                    //            Amt_L = item.Amt_L,
                    //            UPDDate_R = item.UPDDate_R,
                    //            Date_R = item.Date_R,
                    //            Amt_R = item.Amt_R,
                    //            PRO_NO = item.PRO_NO,
                    //            BorderType = "1234D"
                    //        });
                    //    }
                    //    var _groupCurr = groupCurr.FirstOrDefault(x => x.Item1 == item.CURR);
                    //    if (_groupCurr != null)
                    //    {
                    //        _groupCurr.Item2.Add(getAbs(item.Amt_L_D, item.Amt_R_D));
                    //    }
                    //    else
                    //    {
                    //        groupCurr.Add(new Tuple<string, List<decimal>>(item.CURR, new List<decimal>() { getAbs(item.Amt_L_D, item.Amt_R_D) }));
                    //    }
                    //}
                    //subModel_fix.Add(new ORTReportADetailModel() { Status = items.Key});
                    bool totleTitle = true;
                    var _BorderType = string.Empty;
                    switch (items.Key)
                    {
                        case "1":
                        case "2":
                        case "3_1":
                        case "4_1":
                            _BorderType = "12345S";
                            break;
                        case "3_2":
                        case "4_2":
                            _BorderType = "1234S";
                            break;
                    }
                    var _last = subModel_fix.Last();
                    _last.BorderType = _BorderType;

                    foreach (var _currs in groupCurr.OrderBy(x => x.Item1))
                    {
                        if (totleTitle)
                        {
                            subModel_fix.Add(new ORTReportADetailModel()
                            {
                                Status = _status,
                                FEE_SEQ = "差異項總計:",
                                CURR = $@"{_currs.Item2.Count}筆",
                                Date_L = "統計金額:",
                                UPDDate_L = $@"{_currs.Item1}",
                                Amt_L = $@"{_currs.Item2.Sum(x => x).formateThousand()}"
                            });
                            totleTitle = false;
                        }
                        else
                        {
                            subModel_fix.Add(new ORTReportADetailModel()
                            {
                                Status = _status,
                                CURR = $@"{_currs.Item2.Count}筆",
                                UPDDate_L = $@"{_currs.Item1}",
                                Amt_L = $@"{_currs.Item2.Sum(x => x).formateThousand()}"
                            });
                        }
                    }
                    //subModel_fix.Add(new ORTReportADetailModel() { Status = items.Key });

                    _last = subModel_fix.Last();
                    _last.BorderType = _BorderType;
                }
                #endregion

                #endregion
                result.RETURN_FLAG = true;
                _ORT0105AP1ReportModel.model = new Tuple<List<ORTReportAModel>, bool, List<ORTReportADetailModel>>(mainModel, subReportCheck, subModel_fix);
                result.Datas = _ORT0105AP1ReportModel;
            }
            catch (Exception ex)
            {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }   
            return result;
        }

        private decimal getAmt(ORT0105AP1Model model)
        {
            decimal d = 0m;
            return model.AMT ?? d;
        }

        private string getCompare(int a, int b, string curr = "NTD")
        {
            return a == b ? "-" : (a - b).formateThousand(curr);
        }

        private string getCompare(decimal a,decimal b, string curr = "NTD")
        {
            return a == b ? "-" : (a - b).formateThousand(curr);
        }

        private decimal getAbs(decimal? a, decimal? b)
        {
            return Math.Abs((a ?? 0m) - (b ?? 0m));
        }
    }
}