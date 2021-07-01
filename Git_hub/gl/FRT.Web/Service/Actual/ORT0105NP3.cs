using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;
using static FRT.Web.BO.Extension;

/// <summary>
/// 功能說明：跨系統勾稽 變更註記
/// 初版作者：20210602 Mark
/// 修改歷程：20210602 Mark
///           需求單號：202104270739-01
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105NP3 : IORT0105Report
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
                    var _getReportData = getReportData(_getData.Datas, type);
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

        public MSGReturnModel<Tuple<List<ORT0105NP1Model>, List<ORT0105NP1Model>,string,string>> getData(FRT_CROSS_SYSTEM_CHECK schedulerModel, string date_s = null, string date_e = null)
        {
            MSGReturnModel<Tuple<List<ORT0105NP1Model>, List<ORT0105NP1Model>,string,string>> result = new MSGReturnModel<Tuple<List<ORT0105NP1Model>, List<ORT0105NP1Model>,string,string>>();
            List<ORT0105NP1Model> AS400Data = new List<ORT0105NP1Model>();
            List<ORT0105NP1Model> WanpieData = new List<ORT0105NP1Model>();

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
CH.UPD_DATE AS Date,
CD.CHECK_NO AS Check_no,
CD.BANK_CODE AS Bank_Code
from FAPPYCH0 CH
join FAPPYCD0 CD
on CH.APPLY_NO = CD.APPLY_NO
WHERE CH.UPD_DATE between :DATE_S_1 and :DATE_E_1
AND CH.STATUS = '4'
WITH UR;
";
                        com.Parameters.Add("DATE_S_1", datas.Item4);
                        com.Parameters.Add("DATE_E_1", datas.Item5);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORT0105NP1Model();
                            model.Date = dbresult["Date"]?.ToString()?.Trim(); //比對日期
                            model.Check_no = dbresult["Check_no"]?.ToString()?.Trim(); //支票號碼
                            model.Bank_Code = dbresult["Bank_Code"]?.ToString()?.Trim(); //帳戶簡稱
                            AS400Data.Add(model);
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
M.VOUCHER_DATE AS Date,
M.BILL_NO AS Check_no,
(SELECT TOP 1 BILL_SIGN_CODE 
FROM GLSIACT..VWACC07020101_AP101_NOW 
WHERE CORP_NO=M.CORP_NO 
AND BILL_NO=M.BILL_NO 
AND PAY_BANK_ACCT_NO=M.PAY_BANK_ACCT_NO 
AND PAY_BANK_NO=M.PAY_BANK_NO) AS Bank_Code
from GLSIACT..ACC07024501 M (nolock)
where M.VOUCHER_DATE between @DATE_S_2 and @DATE_E_2
and M.DEPT_BOSS_ALLOW_YN = 'Y'
";
                    WanpieData = conn.Query<ORT0105NP1Model>(sql, 
                        new {
                            DATE_S_2 = datas.Item2,
                            DATE_E_2 = datas.Item3
                        }, null, true, getTimeout()).ToList();
                }
                result.RETURN_FLAG = true;
                result.Datas = new Tuple<List<ORT0105NP1Model>, List<ORT0105NP1Model>,string,string>(AS400Data, WanpieData, datas.Item2, datas.Item3);
            }
            catch (Exception ex) {
                var _msg = ex?.ToString();
                logger.Debug(_msg);
                result.DESCRIPTION = _msg;
            }

            return result;
        }

        public MSGReturnModel<ORT0105NP1ReportModel> getReportData(Tuple<List<ORT0105NP1Model>, List<ORT0105NP1Model>,string,string> datas, ReportType reportType )
        {
            MSGReturnModel<ORT0105NP1ReportModel> result = new MSGReturnModel<ORT0105NP1ReportModel>();
            ORT0105NP1ReportModel _ORT0105NP1ReportModel = new ORT0105NP1ReportModel();
            List<ORTReportDModel> mainModel = new List<ORTReportDModel>();
            List<ORTReportDDetailModel> subModel = new List<ORTReportDDetailModel>();

            try
            {
                var _re2 = string.Empty; // 性質代碼：異動資料比對_A、票據狀態餘額_B、變更註記_C、逾期未兌_D、支票回存_E
                var _mainModel = new ORTReportDModel()
                {
                    AS400_SYS_Count = datas.Item1.Count,
                    Wanpie_SYS_Count = datas.Item2.Count,
                    //Diff_Count = (datas.Item1.Count - datas.Item2.Count)
                };

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _SYS_CODE = db.SYS_CODE.AsNoTracking().FirstOrDefault(x =>
                    x.SYS_CD == "RT" &&
                    x.CODE_TYPE == "GOJ_TYPE_NP_GROUP" &&
                    x.CODE == "3");
                    _mainModel.Kind = _SYS_CODE?.CODE_VALUE ?? string.Empty;
                    _re2 = _SYS_CODE?.RESERVE2 ?? "C";
                }

                var _as400s = datas.Item1.GroupBy(x => new { x.Bank_Code, x.Check_no })
                    .Select(x => new NP3() {
                        Bank_Code = x.Key.Bank_Code, 
                        Check_no = x.Key.Check_no, 
                        Count = x.Count() })
                    .ToList();

                var _wans = datas.Item2.GroupBy(x => new { x.Bank_Code, x.Check_no })
                    .Select(x => new NP3
                    {
                        Bank_Code = x.Key.Bank_Code,
                        Check_no = x.Key.Check_no,
                        Count = x.Count()
                    }).ToList();

                //本次資料所有類別
                List<NP3> types = 
                    _as400s.Distinct().Union(_wans.Distinct())
                    .OrderBy(x => x.Check_no).ThenBy(x => x.Bank_Code).ToList();

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("RT", "RPTNP", qPreCode).ToString();
                var _no = $@"{_re2}{qPreCode}{cId.PadLeft(2, '0')}";

                var _a_c = 0;
                var _w_c = 0;
                var _d_c = 0;

                foreach (var _type in types)
                {
                    var _as400 = _as400s.FirstOrDefault(x => x.Bank_Code == _type.Bank_Code && x.Check_no == _type.Check_no);
                    var _wan = _wans.FirstOrDefault(x => x.Bank_Code == _type.Bank_Code && x.Check_no == _type.Check_no);
                    if ((_as400 == null) || (_wan == null) || (_as400.Count != _wan.Count))
                    {
                        var _AS400_Count = (_as400?.Count ?? 0);
                        var _Wanpie_Count = (_wan?.Count ?? 0);
                        var _Diff_Count = Math.Abs(_AS400_Count - _Wanpie_Count);
                        subModel.Add(new ORTReportDDetailModel()
                        {
                            NO = _no,
                            Kind = _mainModel.Kind,
                            Bank_Code = _type.Bank_Code, //帳戶簡稱
                            Check_no = _type.Check_no, //支票號碼
                            AS400_Count = _AS400_Count,
                            Wanpie_Count = _Wanpie_Count,
                            Diff_Count = _Diff_Count
                        });
                        _w_c += _Wanpie_Count;
                        _a_c += _AS400_Count;
                        _d_c += _Diff_Count;
                    }
                }

                _mainModel.Wanpie_Diff_Count = _w_c;
                _mainModel.AS400_Diff_Count = _a_c;
                _mainModel.Diff_Coune_D = _d_c;
                _mainModel.Diff_Count = _d_c;
                _mainModel.NO = _no;

                 mainModel.Add(_mainModel);

                List<FRT_CROSS_SYSTEM_REPORT_NP3> InsertMains = new List<FRT_CROSS_SYSTEM_REPORT_NP3>();
                List<FRT_CROSS_SYSTEM_REPORT_DETAIL_NP3> InsertSubs = new List<FRT_CROSS_SYSTEM_REPORT_DETAIL_NP3>();

                InsertMains.Add(new FRT_CROSS_SYSTEM_REPORT_NP3() {
                    NO = _no,
                    SEQ = 1,
                    Run_Style = reportType.ToString(),
                    Kind= _re2,
                    Start_Date = datas.Item3,
                    End_Date = datas.Item4,
                    Wanpie_SYS_Count = _mainModel.Wanpie_SYS_Count,
                    AS400_SYS_Count = _mainModel.AS400_SYS_Count,
                    Diff_Count = _mainModel.Diff_Count,
                    Wanpie_Diff_Count = _mainModel.Wanpie_Diff_Count,
                    AS400_Diff_Count = _mainModel.AS400_Diff_Count,
                    Diff_Coune_D = _mainModel.Diff_Coune_D
                });

                var _sub_seq = 0;
                foreach (var _sub in subModel)
                {
                    _sub_seq += 1;
                    InsertSubs.Add(new FRT_CROSS_SYSTEM_REPORT_DETAIL_NP3()
                    {
                        NO = _no,
                        SEQ = _sub_seq,
                        Check_no = _sub.Check_no,
                        Bank_Code = _sub.Bank_Code,
                        Wanpie_Count = _sub.Wanpie_Count,
                        AS400_Count = _sub.AS400_Count,
                        Diff_Count = _sub.Diff_Count
                    });
                }
                result.RETURN_FLAG = true;
                using (SqlConnection conn = new SqlConnection(DbUtil.GetDBFglConnStr()))
                {
                    string strSql = @"
INSERT INTO [FRT_CROSS_SYSTEM_REPORT]
           ([NO]
           ,[SEQ]
           ,[Run_Style]
           ,[Kind]
           ,[Start_Date]
           ,[End_Date]
           ,[AS400_SYS_Count]
           ,[AS400_Diff_Count]
           ,[Wanpie_Diff_Count]
           ,[Wanpie_SYS_Count]
           ,[Diff_Count]
           ,[Diff_Coune_D])
     VALUES
           (@NO,
            @SEQ,
            @Run_Style,
            @Kind,
            @Start_Date,
            @End_Date,
            @AS400_SYS_Count, 
            @AS400_Diff_Count,
            @Wanpie_Diff_Count, 
            @Wanpie_SYS_Count,
            @Diff_Count,
            @Diff_Coune_D) ;
";

                    string strSql2 = $@"
INSERT INTO [FRT_CROSS_SYSTEM_REPORT_DETAIL]
           ([NO]
           ,[SEQ]
           ,[Check_no]
           ,[Bank_Code]
           ,[AS400_Count]
           ,[Wanpie_Count]
           ,[Diff_Count])
     VALUES
           (@NO,
            @SEQ,
            @Check_no, 
            @Bank_Code,
            @AS400_Count, 
            @Wanpie_Count,
            @Diff_Count) ;
";

                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        var main_insert = conn.Execute(strSql, InsertMains, tran);
                        var sub_insert = 0;
                        if (InsertSubs.Any())
                        {
                            sub_insert = conn.Execute(strSql2, InsertSubs, tran);
                        }
                        if (main_insert == InsertMains.Count && sub_insert == InsertSubs.Count)
                            tran.Commit();
                        else
                        {
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = MessageType.insert_Fail.GetDescription();
                        }
                    }
                }
                _ORT0105NP1ReportModel.model = new Tuple<List<ORTReportDModel>, bool, List<ORTReportDDetailModel>>(mainModel, subModel.Any(), subModel);
                result.Datas = _ORT0105NP1ReportModel;
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

        public class NP3 : IEquatable<NP3>
        { 
            /// <summary>
            /// 支票號碼
            /// </summary>
            public string Check_no { get; set; }

            /// <summary>
            /// 帳戶簡稱
            /// </summary>
            public string Bank_Code { get; set; }

            /// <summary>
            /// 件數
            /// </summary>
            public int Count { get; set; }

            public bool Equals(NP3 other)
            {

                if (Object.ReferenceEquals(other, null)) return false;

                if (Object.ReferenceEquals(this, other)) return true;

                return Check_no.Equals(other.Check_no) && Bank_Code.Equals(other.Bank_Code);
            }


            public override int GetHashCode()
            {

                int hashProductCheck_no = Check_no == null ? 0 : Check_no.GetHashCode();

                int hashProductBank_Code = Bank_Code == null ? 0 : Bank_Code.GetHashCode();

                return hashProductCheck_no ^ hashProductBank_Code ;
            }
        }

        public class FRT_CROSS_SYSTEM_REPORT_NP3
        {
            /// <summary>
            /// 報表編號
            /// </summary>
            public string NO { get; set; }

            /// <summary>
            /// 序號
            /// </summary>
            public int SEQ { get; set; }

            /// <summary>
            /// 執行方式
            /// </summary>
            public string Run_Style { get; set; }

            /// <summary>
            /// 性質
            /// </summary>
            public string Kind { get; set; }

            /// <summary>
            /// 開始日
            /// </summary>
            public string Start_Date { get; set; }

            /// <summary>
            /// 截止日
            /// </summary>
            public string End_Date { get; set; }

            /// <summary>
            /// AS400系統件數(a)
            /// </summary>
            public int AS400_SYS_Count { get; set; }

            /// <summary>
            /// AS400差異件數(b)
            /// </summary>
            public int AS400_Diff_Count { get; set; }

            /// <summary>
            /// Wanpie差異件數(c)
            /// </summary>
            public int Wanpie_Diff_Count { get; set; }

            /// <summary>
            /// Wanpie系統件數
            /// </summary>
            public int Wanpie_SYS_Count { get; set; }

            /// <summary>
            /// 差異件數
            /// </summary>
            public int Diff_Count { get; set; }

            /// <summary>
            /// 差異件數(差異歸類)
            /// </summary>
            public int Diff_Coune_D { get; set; }
        }

        public class FRT_CROSS_SYSTEM_REPORT_DETAIL_NP3
        {
            /// <summary>
            /// 報表編號
            /// </summary>
            public string NO { get; set; }

            /// <summary>
            /// 序號
            /// </summary>
            public int SEQ { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string Check_no { get; set; }

            /// <summary>
            /// 帳戶簡稱
            /// </summary>
            public string Bank_Code { get; set; }

            /// <summary>
            /// AS400件數
            /// </summary>
            public int AS400_Count { get; set; }

            /// <summary>
            /// Wanpie件數
            /// </summary>
            public int Wanpie_Count { get; set; }

            /// <summary>
            /// 差異件數
            /// </summary>
            public int Diff_Count { get; set; }
        }
    }
}