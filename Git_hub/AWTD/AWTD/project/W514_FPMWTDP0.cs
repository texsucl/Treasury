using AWTD.Daos;
using AWTD.Model;
using AWTD.Utility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWTD.Enum.Ref;
using static AWTD.Utility.Log;

namespace AWTD.project
{
    internal class W514_FPMWTDP0
    {
        public void start()
        {
            var _logger = "W514_FPMWTDP0";
            try
            {
                NlogSet($@"W514 Start !", _logger);
                string sql = string.Empty;
                List<FPMWTDP0> FPMWTDP0s = new List<FPMWTDP0>();
                var _timeout = "EasyComTimeOut".getAppSettings();
                var _setdts = "W514DT_S".getAppSettings();
                var _setdte = "W514DT_E".getAppSettings();

                string _dts = string.Empty;
                string _dte = string.Empty;
                int _yyyMMdd_s = 0;
                int _yyyMMdd_e = 0;

                if (_setdts.Length >= 6 && _setdte.Length >= 6 && Int32.TryParse(_setdts, out _yyyMMdd_s) && Int32.TryParse(_setdte, out _yyyMMdd_e) && _yyyMMdd_e >= _yyyMMdd_s)
                {
                    //int _year = _yyyMM / 100;
                    //int _month = _yyyMM % 100;
                    //_dts = $@"{_year}{new DateTime(_year, _month, 1).ToString("MMdd")}";
                    //_dte = $@"{_year}{(new DateTime(_year, _month, 1).AddMonths(1).AddDays(-1)).ToString("MMdd")}";
                    _dts = $@"{_yyyMMdd_s}";
                    _dte = $@"{_yyyMMdd_e}";
                }

                if (_dts.IsNullOrWhiteSpace() || _dte.IsNullOrWhiteSpace())
                {
                    var _dtn = DateTime.Now;
                    DateTime dtn = _dtn.AddMonths(-1);
                    int _year = dtn.Year - 1911;
                    int _month = dtn.Month;

                    _dts = $@"{_year}{new DateTime(_dtn.Year, _month, 1).ToString("MMdd")}";
                    _dte = $@"{(_dtn.Year - 1911)}{_dtn.ToString("MMdd")}";
                }
                

                
                var _easycomConn = EasyCom.GetEasycomConn();
                if (!_timeout.IsNullOrWhiteSpace())
                    _easycomConn += $@";CommandTimeout={_timeout}";
                NlogSet($@"開始查詢AS400 (FPMWTDP0 保單所屬核保部門代號檔) 資料", _logger);
                NlogSet($@"查詢條件 => 時間範圍 {_dts} ~ {_dte}", _logger);
                using (EacConnection conn = new EacConnection(_easycomConn))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select  
SYS_TYPE,
POLICY_NO,
POLICY_SEQ,
ID_DUP,
WTR_DEPT,
UPD_ID,
UPD_DATE,
UPD_TIME,
UPD_PGM
from FPMWTDP0 
Where 1 = 1
";

                        sql += $@" and UPD_DATE between :UPD_DATE1 and :UPD_DATE2 ";
                        com.Parameters.Add($@"UPD_DATE1", _dts);
                        com.Parameters.Add($@"UPD_DATE2", _dte);

                        sql += " WITH UR ";

                        com.CommandText = sql;
                        com.Prepare();                 
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var _SYS_TYPE = dbresult["SYS_TYPE"]?.ToString()?.Trim(); //系統來源
                            var _POLICY_NO = dbresult["POLICY_NO"]?.ToString()?.Trim(); //保單號碼
                            var _POLICY_SEQ = dbresult["POLICY_SEQ"]?.ToString()?.Trim(); //保單序號
                            var _ID_DUP = dbresult["ID_DUP"]?.ToString()?.Trim() ?? string.Empty; //身份証重覆別
                            var _WTR_DEPT = dbresult["WTR_DEPT"]?.ToString()?.Trim(); //核保部門
                            var _UPD_ID = dbresult["UPD_ID"]?.ToString()?.Trim(); //異動人員
                            var _UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //異動日期
                            var _UPD_TIME = dbresult["UPD_TIME"]?.ToString()?.Trim(); //異動時間
                            var _UPD_PGM = dbresult["UPD_PGM"]?.ToString()?.Trim(); //異動程式
                            FPMWTDP0s.Add(new FPMWTDP0()
                            {
                                SYS_TYPE = _SYS_TYPE,
                                POLICY_NO = _POLICY_NO,
                                POLICY_SEQ = _POLICY_SEQ,
                                ID_DUP = _ID_DUP,
                                WTR_DEPT = _WTR_DEPT,
                                UPD_ID = _UPD_ID,
                                UPD_DATE = _UPD_DATE,
                                UPD_TIME = _UPD_TIME,
                                UPD_PGM = _UPD_PGM
                            });
                        }
                        com.Dispose();
                    }
                }

                NlogSet($@"查詢AS400 (FPMWTDP0 保單所屬核保部門代號檔) 資料共:{FPMWTDP0s.Count}筆", _logger);

                using (SqlConnection conn = new SqlConnection(
                    System.Configuration.ConfigurationManager.
                    ConnectionStrings["WanpieConnection"].ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
//                        NlogSet($@"開始刪除 (XXX279350734514_FPMWTDP0 保單所屬核保部門代號檔) 資料", _logger);

//                        sql = $@"
//delete GLSIEXT..XXX279350734514_FPMWTDP0
//where  UPD_DATE  between  @UPD_DATEs and @UPD_DATEe ";

//                        var _del = new MSSql().Execute(sql, new { UPD_DATEs = _dts, UPD_DATEe = _dte }, _logger, null ,tran , conn, false, 7200);

//                        NlogSet($@"刪除完成 (XXX279350734514_FPMWTDP0 保單所屬核保部門代號檔) 資料共:{_del.Item2}筆", _logger);

                        string sql_d = $@"
delete GLSIEXT..XXX279350734514_FPMWTDP0
where SYS_TYPE = @SYS_TYPE
and POLICY_NO = @POLICY_NO
and POLICY_SEQ = @POLICY_SEQ
and ID_DUP = @ID_DUP 
";

                        sql = $@"
insert into GLSIEXT..XXX279350734514_FPMWTDP0
(SYS_TYPE,
POLICY_NO,
POLICY_SEQ,
ID_DUP,
WTR_DEPT,
UPD_ID,
UPD_DATE,
UPD_TIME,
UPD_PGM
)
values 
(@SYS_TYPE,
@POLICY_NO,
@POLICY_SEQ,
@ID_DUP,
@WTR_DEPT,
@UPD_ID,
@UPD_DATE,
@UPD_TIME,
@UPD_PGM
)
";

                        int i = 100000; //一次新增幾筆
                        var _t = (FPMWTDP0s.Count / i) + 1;
                        int _complete_count = 0;
                        NlogSet($@"開始轉入XXX279350734514_FPMWTDP0 共: {_t} 批 每批: {i} 筆", _logger);
                        NlogSet($@"使用Transaction全部成功才會一起交易完成", _logger);
                        var _errormsg = string.Empty;
                        for (int j = 0; j < _t; j ++ )
                        {
                            if (_errormsg.IsNullOrWhiteSpace())
                            {
                                NlogSet($@"開始轉入XXX279350734514_FPMWTDP0 第{j + 1}批", _logger);

                                var _datas = FPMWTDP0s.Skip(j * i).Take(i).ToList();

                                var _result_d = new MSSql().Execute(sql_d, _datas, _logger, null, tran, conn, true, 7200, false);
                                if(_result_d.Item2 > 0)
                                    NlogSet($@"轉入XXX279350734514_FPMWTDP0 第{j + 1}批 刪除重複資料{_result_d.Item2}筆!", _logger);
                                var _result = new MSSql().Execute(sql, _datas, _logger, null, tran, conn, true, 7200, false);
                                if (_result.Item1.IsNullOrWhiteSpace())
                                {
                                    _complete_count += _result.Item2;
                                    NlogSet($@"轉入XXX279350734514_FPMWTDP0 第{j + 1}批 成功!", _logger);
                                }
                                else
                                {
                                    _errormsg = _result.Item1;
                                    NlogSet($@"轉入XXX279350734514_FPMWTDP0 第{j + 1}批 失敗原因:{_errormsg}!", _logger, Nlog.Error);
                                }
                            }
                        }
                        if (_errormsg.IsNullOrWhiteSpace() && _complete_count == FPMWTDP0s.Count)
                        {
                            tran.Commit();
                            NlogSet($@"執行成功 => 新增筆數:{_complete_count}", _logger);
                        }
                        else
                        { 
                            tran.Rollback();
                            NlogSet($@"執行失敗", _logger);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ex = ex.exceptionMessage();
                NlogSet(_ex, _logger, Nlog.Info);
                NlogSet(_ex, _logger, Nlog.Error);
            }
            finally
            {
                NlogSet($@"W514 End !", _logger);
            }
        }
    }
}
