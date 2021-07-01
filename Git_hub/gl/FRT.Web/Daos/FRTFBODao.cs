using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Text;
using System.Transactions;

namespace FRT.Web.Daos
{
    /// <summary>
    /// 快速付款改FBO作業
    /// </summary>
    public class FRTFBODao
    {
        /// <summary>
        /// 查詢 快速付款改FBO匯款作業
        /// </summary>
        /// <param name="fastNo_S">快速付款起</param>
        /// <param name="fastNo_E">快速付款迄</param>
        /// <param name="filler_20">匯款轉檔批號</param>
        /// <returns></returns>
        public MSGReturnModel<List<FBOModel>> qryForORTB017(string fastNo_S, string fastNo_E, string filler_20)
        {
            MSGReturnModel<List<FBOModel>> resultMsg = new MSGReturnModel<List<FBOModel>>();
            List<FBOModel> results = new List<FBOModel>();
            List<FBOModel> AllDatas = new List<FBOModel>();
            List<BERM> berms = new List<BERM>();
            filler_20 = filler_20?.Trim();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = $@"
SELECT
FAST_NO,
PAID_ID,
(BANK_CODE || SUB_BANK) AS BANK_NO ,
BANK_ACT,
REMIT_AMT,
RCV_NAME,
REMIT_STAT,
FAIL_CODE,
FILLER_20
FROM LRTBARM1
WHERE TEXT_TYPE IN ('1','2','3')
and REMIT_STAT IN ('1','2')
";
                    //if (!string.IsNullOrWhiteSpace(fastNo_S))
                    //{
                    //    sql += " and FAST_NO >= :FAST_NO_S ";
                    //    com.Parameters.Add("FAST_NO_S", fastNo_S.Trim());
                    //}
                    //if (!string.IsNullOrWhiteSpace(fastNo_E))
                    //{
                    //    sql += " and FAST_NO <= :FAST_NO_E ";
                    //    com.Parameters.Add("FAST_NO_E", fastNo_E.Trim());
                    //}
                    if (!string.IsNullOrWhiteSpace(filler_20))
                    {
                        sql += " and  FILLER_20 like :FILLER_20 ";
                        com.Parameters.Add("FILLER_20", filler_20.Trim().Split('*')[0]+"%");
                    }
                    //else
                    //{
                    //    sql += " and locate('*',FILLER_20) = 0 ";
                    //}
                    sql += " ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader result = com.ExecuteReader();
                    //com.Parameters.Add("FAST_NO", fastNo);
                    while (result.Read())
                    {
                        var data = new FBOModel()
                        {
                            FAST_NO = result["FAST_NO"]?.ToString()?.Trim(), //快速付款編號
                            PAID_ID = result["PAID_ID"]?.ToString()?.Trim(), //收款人ID
                            BANK_CODE_SUB_BANK = result["BANK_NO"]?.ToString()?.Trim(), //銀行代號
                            BANK_ACT = result["BANK_ACT"]?.ToString()?.Trim(), //匯款帳號
                            REMIT_AMT = result["REMIT_AMT"]?.ToString()?.Trim(), //匯款金額
                            RCV_NAME = result["RCV_NAME"]?.ToString()?.Trim(), //收款人戶名
                            FAIL_CODE = result["FAIL_CODE"]?.ToString()?.Trim(), //錯誤代碼
                            REMIT_STAT = result["REMIT_STAT"]?.ToString()?.Trim(), //匯款狀態
                            FILLER_20 = result["FILLER_20"]?.ToString()?.Trim() //轉檔批號
                        };
                        AllDatas.Add(data);
                    }
                    com.Dispose();
                }
                if(!fastNo_S.IsNullOrWhiteSpace())
                     AllDatas = AllDatas.Where(x => x.FAST_NO.CompareTo(fastNo_S.Trim()) >= 0).ToList();
                if(!fastNo_E.IsNullOrWhiteSpace())
                     AllDatas = AllDatas.Where(x => x.FAST_NO.CompareTo(fastNo_E.Trim()) <= 0).ToList();
                //2.1.2當電文格式為1/2/3時，匯款狀態為1-待匯款，可執行此作業
                results.AddRange(AllDatas.Where(x => x.REMIT_STAT == "1"));
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = $@"
select ERR_BELONG,TRANS_CODE from LRTBERM1 ;
";
                    com.CommandText = sql;
                    com.Prepare();

                    DbDataReader result = com.ExecuteReader();
                    while (result.Read())
                    {
                        var data = new BERM()
                        {
                            ERR_BELONG = result["ERR_BELONG"]?.ToString()?.Trim(),
                            TRANS_CODE = result["TRANS_CODE"]?.ToString()?.Trim()
                        };
                        berms.Add(data);
                    }
                    com.Dispose();
                }
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<string> fastNos = AllDatas.Select(x => x.FAST_NO).Distinct().ToList();
                    var _T_622823s = db.FRT_XML_T_622823.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();
                    var _R_622823s = db.FRT_XML_R_622823.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();
                    var _T_622821s = db.FRT_XML_T_622821.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();
                    var _R_622821s = db.FRT_XML_R_622821.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();
                    var _T_eachs = db.FRT_XML_T_eACH.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();
                    var _R_eachs = db.FRT_XML_R_eACH.AsNoTracking().Where(x => fastNos.Contains(x.FAST_NO)).ToList();

                    foreach (var data in AllDatas.Where(x => x.REMIT_STAT == "2" ))
                    {
                        FRT_XML_T_eACH _T_each = _T_eachs.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        FRT_XML_R_eACH _R_each = _R_eachs.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        FRT_XML_T_622823 _T_622823 = _T_622823s.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        FRT_XML_R_622823 _R_622823 = _R_622823s.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        FRT_XML_T_622821 _T_622821 = _T_622821s.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        FRT_XML_R_622821 _R_622821 = _R_622821s.Where(x => x.FAST_NO == data.FAST_NO).OrderByDescending(x => x.CRT_TIME).FirstOrDefault();
                        if (_T_each != null)
                        {
                            //有下行電文
                            if (_R_each != null && _T_each.GUID == _R_each.GUID)
                            {
                                var berm = berms.FirstOrDefault(x => x.TRANS_CODE == data.FAIL_CODE);
                                //2.1.3當電文格式為1/2/3時，匯款狀態為2-已匯款且ERROR_CODE歸屬於人壽或銀行，可執行此作業 (ERR_BELONG:1為客戶問題) 
                                //20190815 加入 => 加排除ERROR_CODE等於”0000”或空白
                                if (!(berm != null && berm.ERR_BELONG == "1") && !(_R_each.ERROR_CODE == "0000" || _R_each.ERROR_CODE?.Trim() == ""))
                                    results.Add(data);
                            }
                            //2.1.4當電文格式為1/2/3時，匯款狀態為2-已匯款且無下行電文，可執行此作業
                            else
                                results.Add(data);
                        }
                        else if (_T_622823 != null)
                        {
                            //有下行電文
                            if (_R_622823 != null && _T_622823.GUID == _R_622823.GUID)
                            {
                                var berm = berms.FirstOrDefault(x => x.TRANS_CODE == data.FAIL_CODE);
                                //2.1.3當電文格式為1/2/3時，匯款狀態為2-已匯款且ERROR_CODE歸屬於人壽或銀行，可執行此作業 (ERR_BELONG:1為客戶問題)
                                //20190815 加入 => 加排除ERROR_CODE等於”0000”或空白
                                if (!(berm != null && berm.ERR_BELONG == "1"))
                                {
                                    var _errCode = _R_622823.RC_CODE == null ? _R_622823.EMSGID : _R_622823.RC_CODE;
                                    if (!(_errCode == "0000" || _errCode?.Trim() == ""))
                                        results.Add(data);
                                }
                            }
                            //2.1.4當電文格式為1/2/3時，匯款狀態為2-已匯款且無下行電文，可執行此作業
                            else
                                results.Add(data);
                        }
                        else if (_T_622821 != null)
                        {
                            //有下行電文
                            if (_R_622821 != null && _T_622821.GUID == _R_622821.GUID)
                            {
                                var berm = berms.FirstOrDefault(x => x.TRANS_CODE == data.FAIL_CODE);
                                //2.1.3當電文格式為1/2/3時，匯款狀態為2-已匯款且ERROR_CODE歸屬於人壽或銀行，可執行此作業 (ERR_BELONG:1為客戶問題)
                                //20190815 加入 => 加排除ERROR_CODE等於”0000”或空白
                                if (!(berm != null && berm.ERR_BELONG == "1"))
                                {
                                    var _errCode = _R_622821.RC_CODE == null ? _R_622821.EMSGID : _R_622821.RC_CODE;
                                    if (!(_errCode == "0000" || _errCode?.Trim() == ""))
                                        results.Add(data);
                                }
                            }
                            //2.1.4當電文格式為1/2/3時，匯款狀態為2-已匯款且無下行電文，可執行此作業
                            else
                                results.Add(data);
                        }
                        else
                        {
                            //2.1.4當電文格式為1/2/3時，匯款狀態為2-已匯款且無下行電文，可執行此作業
                            results.Add(data);
                        }
                    }
                }
                conn.Dispose();
                conn.Close();
            }
            if (results.Any()) //有查詢到資料
            {
                if (!filler_20.IsNullOrWhiteSpace()) //有轉檔批號
                {
                    var _filler_20 = filler_20.Split('*')[0];
                    var _r1 = results.Where(x => x.FILLER_20 == _filler_20).ToList();
                    if (_r1.Count > 0) //有轉檔回復後資料
                    {
                        resultMsg.RETURN_FLAG = true;
                        resultMsg.Datas = results.Where(x=>x.FILLER_20 == _filler_20).OrderBy(x => x.FAST_NO).ToList();
                    }
                    else //無執行轉檔回復
                    {
                        resultMsg.RETURN_FLAG = false;
                        resultMsg.DESCRIPTION = $@"已有轉檔批號:{_filler_20} , 查詢資料尚未轉檔回復，請確認!";
                    }
                }
                else //快速付款編號 (無轉檔批號)
                {
                    var _FILLER_20s = results.Where(x => !string.IsNullOrWhiteSpace(x.FILLER_20)).Select(x=>x.FILLER_20).Distinct().ToList();
                    if (_FILLER_20s.Any()) //有查詢到轉檔批號
                    {
                        resultMsg.RETURN_FLAG = false;
                        if (_FILLER_20s.Any(x => x.Split('*').Count() == 1)) //
                            resultMsg.DESCRIPTION = $@"已有轉檔批號:{string.Join("、", _FILLER_20s.Select(x => x.Split('*')[0]).Distinct().OrderBy(x => x))} ,請改用匯款轉檔批號查詢!";
                        else
                            resultMsg.DESCRIPTION = $@"已有轉檔批號:{string.Join("、", _FILLER_20s.Select(x => x.Split('*')[0]).OrderBy(x => x))}，查詢資料尚未轉檔回復，請確認!";
                    }
                    else
                    {
                        resultMsg.RETURN_FLAG = true;
                        resultMsg.Datas = results.OrderBy(x => x.FAST_NO).ToList();
                    }
                }

            }
            else
            {
                resultMsg.RETURN_FLAG = false;
                resultMsg.DESCRIPTION = "查無資料，請確認!";
            }
            return resultMsg;
        }

        /// <summary>
        /// 查詢 快速付款改FBO匯款轉檔回復作業
        /// </summary>
        /// <param name="filler_20"></param>
        /// <returns></returns>
        public MSGReturnModel<ORTB018Model> qryForORTB018(string filler_20)
        {
            MSGReturnModel<ORTB018Model> result = new MSGReturnModel<ORTB018Model>();
            result.DESCRIPTION = "無查詢資料!";
            var emplys = new List<V_EMPLY2>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emplys = db.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
            }

            using (dbFGLEntities db = new dbFGLEntities())
            {
                filler_20 = filler_20 + "*";
                var _FBO = db.FRT_FBO.Where(x => x.remit_transfer_no == filler_20 &&
                x.recovery_flag != "Y")
                .OrderByDescending(x => x.remit_date)
                .ThenByDescending(x => x.remit_time).FirstOrDefault();
                if (_FBO != null)
                {
                    result.RETURN_FLAG = true;
                    result.Datas = new ORTB018Model()
                    {
                        TRANSFER_ID = _FBO.transfer_id + emplys.FirstOrDefault(x => x.USR_ID == _FBO.transfer_id)?.EMP_NAME?.Trim() ?? string.Empty,
                        TRANSFER_NO = filler_20,
                        VOUCHER_AMT = _FBO.remit_amt.decimalNTointN()?.ToString(),
                        VOUCHER_COUNT = _FBO.remit_count?.ToString(),
                        VOUCHER_DATE = _FBO.remit_date.ToString("yyyy/MM/dd") + " " + _FBO.remit_time.ToString(@"hh\:mm\:ss")
                    };
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢 快速付款人工比對作業
        /// </summary>
        /// <param name="fastNo_S">快速付款起</param>
        /// <param name="fastNo_E">快速付款迄</param>
        /// <returns></returns>
        public MSGReturnModel<List<FBOModel>> qryForORTB019(string fastNo_S, string fastNo_E)
        {
            MSGReturnModel<List<FBOModel>> resultMsg = new MSGReturnModel<List<FBOModel>>();
            List<FBOModel> results = new List<FBOModel>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = $@"
SELECT
FAST_NO,
PAID_ID,
(BANK_CODE || SUB_BANK) AS BANK_NO ,
BANK_ACT,
REMIT_AMT,
RCV_NAME,
FILLER_20
FROM LRTBARM1
WHERE REMIT_STAT = '3'
and trim(NULLIF(FILLER_20,null)) <> ''
and CHK_MARK1 <> 'Y'
";
                    if (!string.IsNullOrWhiteSpace(fastNo_S))
                    {
                        sql += " and FAST_NO >= :FAST_NO_S ";
                        com.Parameters.Add("FAST_NO_S", fastNo_S.Trim());
                    }
                    if (!string.IsNullOrWhiteSpace(fastNo_E))
                    {
                        sql += " and FAST_NO <= :FAST_NO_E ";
                        com.Parameters.Add("FAST_NO_E", fastNo_E.Trim());
                    }
                    sql += " ORDER BY FAST_NO ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader result = com.ExecuteReader();
                    while (result.Read())
                    {
                        var _FILLER_20 = result["FILLER_20"]?.ToString()?.Trim(); //匯款轉檔批號
                        if (!string.IsNullOrWhiteSpace(_FILLER_20))
                        {
                            var data = new FBOModel()
                            {
                                FAST_NO = result["FAST_NO"]?.ToString()?.Trim(), //快速付款編號
                                PAID_ID = result["PAID_ID"]?.ToString()?.Trim(), //收款人ID
                                BANK_CODE_SUB_BANK = result["BANK_NO"]?.ToString()?.Trim(), //銀行代號
                                BANK_ACT = result["BANK_ACT"]?.ToString()?.Trim(), //匯款帳號
                                REMIT_AMT = result["REMIT_AMT"]?.ToString()?.Trim(), //匯款金額
                                RCV_NAME = result["RCV_NAME"]?.ToString()?.Trim(), //收款人戶名
                                FILLER_20 = _FILLER_20, //匯款轉檔批號
                                checkFlag = true
                            };
                            results.Add(data);
                        }
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }
            if (results.Any())
            {
                resultMsg.RETURN_FLAG = true;
                resultMsg.Datas = results;
            }
            else
            {
                resultMsg.RETURN_FLAG = false;
                resultMsg.DESCRIPTION = "查無資料!";
            }
            return resultMsg;
        }

        /// <summary>
        /// 獲取匯款轉檔批號
        /// </summary>
        /// <param name="fastNos">快速付款批號</param>
        /// <param name="userId">userId</param>
        /// <param name="remit_amt">匯款金額</param>
        /// <param name="FILLER_20">上一次匯款轉檔批號</param>
        /// <returns></returns>
        public MSGReturnModel<string> getFiller_20(List<string> fastNos, string userId, decimal remit_amt, string FILLER_20 = null)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            string _filler_20 = FILLER_20; //匯款轉檔批號
            DateTime dt = DateTime.MinValue;
            TimeSpan ts = TimeSpan.MinValue;
            DateTime dtn = DateTime.Now;
            dt = dtn.Date;
            ts = dtn.TimeOfDay;
            if (string.IsNullOrWhiteSpace(_filler_20))
            {

                _filler_20 = dtn.ToString("yyyyMMdd") + new SysSeqDao().qrySeqNo("RT", "B017", dtn.ToString("yyyyMMdd")).ToString().PadLeft(2, '0') + "*"; //*號表示已經做過 不能重新執行
            }
            else
            {
                //var _dateTime = FILLER_20.Substring(0, 8);
                //var _timespan = FILLER_20.Substring(8, 6);
                //dt = DateTime.ParseExact(_dateTime, "yyyyMMdd", null,
                //System.Globalization.DateTimeStyles.AllowWhiteSpaces);
                //ts = TimeSpan.ParseExact(_timespan, "hh''mm''ss", null);
                _filler_20 += "*";
            }
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        com.Transaction = transaction;
                        string sql = $@"
update LRTBARM1
set FILLER_20 = :FILLER_20 ,
REMIT_DATE = '{$@"{dtn.Year - 1911}{dtn.ToString("MMdd")}"}' ,
REMIT_STAT = '2' ,
TEXT_SNDDT = '{$@"{dtn.Year - 1911}{dtn.ToString("MMdd")}"}' ,
TEXT_SNDTM = '{dtn.ToString("HHmmssff")}'
where 1 = 1
";              
                        com.Parameters.Add("FILLER_20", _filler_20);
                        sql += " AND FAST_NO in ( ";
                        foreach (var fastNo in fastNos.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            var _fastNo = fastNo.Trim();
                            sql += (" :fastNo_" + _fastNo + " ,");
                            com.Parameters.Add("fastNo_" + _fastNo, _fastNo);
                        }
                        sql = sql.Substring(0, sql.Length - 1);
                        sql += " ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        if (updateNum == fastNos.Count)
                        {
                            using (dbFGLEntities db = new dbFGLEntities())
                            {
                                using (TransactionScope scope = new TransactionScope())
                                {
                                    db.FRT_FBO.Add(new FRT_FBO()
                                    {
                                        remit_transfer_no = _filler_20,
                                        remit_date = dt,
                                        remit_time = ts,
                                        remit_count = updateNum,
                                        transfer_id = userId,
                                        remit_amt = remit_amt
                                    });
                                    db.SaveChanges();
                                    try
                                    {
                                        scope.Complete();
                                        transaction.Commit();
                                        result.RETURN_FLAG = true;
                                        result.Datas = _filler_20;
                                        result.DESCRIPTION = $@"轉檔批號 : {_filler_20.Replace("*", "")}";
                                    }
                                    catch
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = "更新筆數不一致!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            }
            return result;
        }

        /// <summary>
        /// 快速付款改FBO匯款轉檔回復作業
        /// </summary>
        /// <param name="Filler_20"></param>
        /// <returns></returns>
        public MSGReturnModel<string> updForORTB018(string Filler_20)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    Filler_20 = Filler_20 + "*";
                    EacTransaction transaction = conn.BeginTransaction();
                    List<string> fast_nos = new List<string>();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
select FAST_NO
from LRTBARM1
where Filler_20 = :Filler_20 ;
";
                        com.Parameters.Add("Filler_20", Filler_20);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dr = com.ExecuteReader();
                        while (dr.Read())
                        {
                            if (!string.IsNullOrWhiteSpace(dr["FAST_NO"]?.ToString()))
                            {
                                fast_nos.Add(dr["FAST_NO"].ToString());
                            }
                        }
                        com.Dispose();
                    }
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
update LRTBARM1
set Filler_20 = :Filler_20_new
where 1 = 1 
";
                        com.Transaction = transaction;
                        com.Parameters.Add("Filler_20_new", Filler_20.Split('*')[0]);
                        sql += " AND FAST_NO in ( ";
                        foreach (var fastNo in fast_nos.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            var _fastNo = fastNo.Trim();
                            sql += (" :fastNo_" + _fastNo + " ,");
                            com.Parameters.Add("fastNo_" + _fastNo, _fastNo);
                        }
                        sql = sql.Substring(0, sql.Length - 1);
                        sql += " ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        if (updateNum != 0)
                        {
                            using (TransactionScope scope = new TransactionScope())
                            {
                                using (dbFGLEntities db = new dbFGLEntities())
                                {
                                    var _fbo = db.FRT_FBO
                                        .Where(x => x.remit_transfer_no == Filler_20 &&
                                                 x.recovery_flag != "Y")
                                        .OrderByDescending(x => x.remit_date)
                                        .ThenByDescending(x => x.remit_time)
                                        .First();
                                    _fbo.recovery_flag = "Y";
                                    try
                                    {
                                        db.SaveChanges();
                                        scope.Complete();
                                        transaction.Commit();
                                        result.RETURN_FLAG = true;
                                        result.DESCRIPTION = "更新成功!";
                                        result.Datas = $@"總共更新:{updateNum}筆 資料!";
                                    }
                                    catch
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = "無更新資料!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            }
            return result;
        }

        /// <summary>
        /// 快速付款人工比對作業
        /// </summary>
        /// <param name="fastNos"></param>
        /// <returns></returns>
        public MSGReturnModel<string> updForORTB019(List<string> fastNos)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
update LRTBARM1
set CHK_MARK1 = 'Y'
where 1 = 1
";
                        com.Transaction = transaction;
                        sql += " AND FAST_NO in ( ";
                        foreach (var fastNo in fastNos.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            var _fastNo = fastNo.Trim();
                            sql += (" :fastNo_" + _fastNo + " ,");
                            com.Parameters.Add("fastNo_" + _fastNo, _fastNo);
                        }
                        sql = sql.Substring(0, sql.Length - 1);
                        sql += " ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        if (updateNum == fastNos.Count)
                        {
                            using (TransactionScope scope = new TransactionScope())
                            {
                                using (dbFGLEntities db = new dbFGLEntities())
                                {
                                    foreach (var fastNo in fastNos)
                                    {
                                        DateTime dtn = DateTime.Now;
                                        var _fno = db.FRT_XML_LOG.AsNoTracking()
                                            .Where(x => x.fast_no == fastNo)
                                            .FirstOrDefault();
                                        if (_fno == null)
                                        {
                                            db.FRT_XML_LOG.Add(new FRT_XML_LOG()
                                            {
                                                fast_no = fastNo,
                                                create_date = dtn,
                                                create_time_hms = dtn.TimeOfDay
                                            });
                                        }
                                        db.FRT_XML_LOG_DETAIL.Add(new FRT_XML_LOG_DETAIL()
                                        {
                                            fast_no = fastNo,
                                            create_date = dtn,
                                            create_time_hms = dtn.TimeOfDay,
                                            operation_status = "SB" //BCP
                                        });
                                    }
                                    try
                                    {
                                        db.SaveChanges();
                                        scope.Complete();
                                        transaction.Commit();
                                        result.RETURN_FLAG = true;
                                        result.DESCRIPTION = $@"總共更新:{fastNos.Count}筆 資料!";
                                    }
                                    catch
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = "更新筆數不一致!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            }
            return result;
        }

        /// <summary>
        /// 獲得 FBOQ11 txt 資料
        /// </summary>
        /// <param name="filler_20"></param>
        /// <param name="downloadType">FBO:FBO,Counter:臨櫃</param>
        /// <returns></returns>
        public Tuple<string,int> getFBOQ11txt(string filler_20,string downloadType)
        {
            string txtStr = string.Empty;
            int i = 0;
            var workDatas = new List<FRT_WORD>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                workDatas = db.FRT_WORD.AsNoTracking().ToList();
            }
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = $@"
select
FAST_NO ,
REMIT_AMT ,
BANK_ACT ,
RCV_NAME ,
BANK_CODE || SUB_BANK AS BANK,
PAID_ID ,
SYS_TYPE , 
SRCE_FROM , 
SRCE_KIND
from LRTBARM1
where Filler_20 = :Filler_20 ;
";
                    com.Parameters.Add("Filler_20", filler_20);
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader result = com.ExecuteReader();
                    List<FBOQ11txt> datas = new List<FBOQ11txt>();
                    DateTime dtn = DateTime.Now;
                    string dtnStr = $@"{dtn.ToString("yyyyMMdd")}";
                    var FBOFlag = downloadType == "FBO";
                    StringBuilder sb = new StringBuilder();
                    while (result.Read())
                    {
                        if (!string.IsNullOrWhiteSpace(result["FAST_NO"]?.ToString()))
                        {
                            var workData = "富邦人壽" + workDatas.FirstOrDefault(z => 
                            z.frt_sys_type == result["SYS_TYPE"]?.ToString()  && 
                            z.frt_srce_from == result["SRCE_FROM"]?.ToString()  && 
                            z.frt_srce_kind == result["SRCE_KIND"]?.ToString())?.frt_memo_apx;
                            if (!FBOFlag)
                                workData = workData.CustomCHECK_FORMULA();
                            FBOQ11txt data = new FBOQ11txt();
                            data.PRE_REMIT_MONEY_DATE = dtnStr.CustomPadRight(8);
                            data.REMIT_MONEY_NET_AMOU = FBOFlag ? (result["REMIT_AMT"]?.ToString()).CustomPadLeft(11,'0') : (strDecimalToStr((result["REMIT_AMT"]?.ToString()),2).CustomPadLeft(14, '0') ); //快速付款優化 調整
                            data.REMIT_MONEY_BANK_ACC = "737102710668".CustomPadLeft(16, '0');
                            data.LOCK_DATA_1 = string.Empty.CustomPadRight(3);
                            data.CHECK_FORMULA_1 = FBOFlag ? workData.CustomPadRight(60, ' ' ,true) : (CashEmp(workData) + workData.CustomPadRight(78, ' ' ,true));
                            data.LOCK_DATA_2 = string.Empty.CustomPadRight(2);
                            data.BANK_ACCT_NO = FBOFlag ? (result["BANK_ACT"]?.ToString()).CustomPadRight( 17 ) : (result["BANK_ACT"]?.ToString()).CustomPadLeft(14,'0');
                            data.BANK_ACCT_NAME = FBOFlag ? (result["RCV_NAME"]?.ToString()).CustomPadRight( 59, ' ' ,true ) : (CashEmp(result["RCV_NAME"]?.ToString()) + (result["RCV_NAME"]?.ToString()).CustomPadRight(78, ' ',true));
                            data.BANK_NO = (string.Empty + result["BANK"]?.ToString()).CustomPadRight(7);
                            data.LOCK_DATA_3 = string.Empty.CustomPadRight(152);
                            data.CHECK_FORMULA_2 = "15".CustomPadRight(3);
                            data.REMIT_BANK_SERIAL_NO = result["FAST_NO"]?.ToString().CustomPadRight(30);
                            data.BUSINESS_CORP_NO = string.Empty.CustomPadRight(29);
                            data.REMIT_SHEET_NO = string.Empty.CustomPadRight(76);
                            data.CHECK_FORMULA_3 = result["PAID_ID"]?.ToString().CustomPadRight(12);
                            data.CHECK_FORMULA_4 = CashEmp(string.Empty) + string.Empty.CustomPadRight(78);
                            data.LOCK_DATA_4 = string.Empty.CustomPadRight(2);
                            if (FBOFlag) //FBO
                                sb.AppendLine($@"{data.PRE_REMIT_MONEY_DATE}{data.REMIT_MONEY_NET_AMOU}{data.REMIT_MONEY_BANK_ACC}{data.LOCK_DATA_1}{data.CHECK_FORMULA_1}{data.LOCK_DATA_2}{data.BANK_ACCT_NO}{data.BANK_ACCT_NAME}{data.BANK_NO}{data.LOCK_DATA_3}{data.CHECK_FORMULA_2}{data.REMIT_BANK_SERIAL_NO}{data.BUSINESS_CORP_NO}{data.REMIT_SHEET_NO}{data.CHECK_FORMULA_3}");
                            else //臨櫃
                                sb.AppendLine($@"{data.BANK_NO}{data.BANK_ACCT_NO}{data.REMIT_MONEY_NET_AMOU}{data.BANK_ACCT_NAME}{data.CHECK_FORMULA_1}{data.CHECK_FORMULA_4}{data.LOCK_DATA_4}");
                            datas.Add(data);
                        }
                    }
                    com.Dispose();
                    if (datas.Any() && !FBOFlag) //臨櫃有末筆資料
                        sb.AppendLine($@"{datas.Count.ToString().CustomPadLeft(7,'0')}{dtnStr.CustomPadRight(14)}{datas.Sum(x=>Convert.ToInt64(x.REMIT_MONEY_NET_AMOU)).ToString().CustomPadLeft(14,'0')}{string.Empty.CustomPadRight(242)}");
                    txtStr = sb.ToString();
                    i = datas.Count;
                }
            }
            return new Tuple<string, int>(txtStr, i);
        }

        public static string CashEmp(string value)
        {
            return ((2 + System.Text.Encoding.Default.GetBytes(value?.Trim() ?? string.Empty).Length).ToString().CustomPadLeft(2,'0'));
        }

        public static string strDecimalToStr(string value, int round = 0, bool defaultvalue = false)
        {
            if (value.IsNullOrWhiteSpace())
                return defaultvalue ? "0" : string.Empty;
            decimal d = 0M;
            if (decimal.TryParse(value, out d))
            {
                if (round != 0)
                    d = Math.Round(d, round);
                var ds = d.ToString($@"F{round}").Split('.');
                return ds[0] + (ds.Length > 1 ? ds[1] : string.Empty.CustomPadRight(round,'0'));
            }
            else
                return defaultvalue ? "0" : string.Empty;
        }

        public class BERM
        {
            public string ERR_BELONG { get; set; }
            public string TRANS_CODE { get; set; }
        }

        public class FBOQ11txt
        {
            /// <summary>
            /// 付款日期_FBO_1 (8) 左靠右補空白
            /// </summary>
            public string PRE_REMIT_MONEY_DATE { get; set; }

            /// <summary>
            /// 付款金額_FBO_2 (11) , 匯款金額_臨櫃_3 (14) 右靠左補0
            /// </summary>
            public string REMIT_MONEY_NET_AMOU { get; set; }

            /// <summary>
            /// 付款帳戶_FBO_3 (16)  右靠左補0
            /// </summary>
            public string REMIT_MONEY_BANK_ACC { get; set; }

            /// <summary>
            /// 預留空白欄_FBO_4 (3) 左靠右補空白
            /// </summary>
            public string LOCK_DATA_1 { get; set; }

            /// <summary>
            /// 付款戶名_FBO_5 (60) , 匯款人姓名_臨櫃_5 (80) 左靠右補空白
            /// </summary>
            public string CHECK_FORMULA_1 { get; set; }

            /// <summary>
            /// 付款銀行_FBO_6 (2) 左靠右補空白
            /// </summary>
            public string LOCK_DATA_2 { get; set; }

            /// <summary>
            /// 收款帳號_FBO_7 (17) 左靠右補空白 , 收款行帳號_臨櫃_2 (14) 右靠左補0
            /// </summary>
            public string BANK_ACCT_NO { get; set; }

            /// <summary>
            /// 收款戶名_FBO_8 (59) , 收款人姓名_臨櫃_4 (80) 左靠右補空白
            /// </summary>
            public string BANK_ACCT_NAME { get; set; }

            /// <summary>
            /// 收款銀行_FBO_9 (7) , 收款行代號_臨櫃_1 (7) 左靠右補空白
            /// </summary>
            public string BANK_NO { get; set; }

            /// <summary>
            /// 不考慮欄位合併_FBO_10 (152) 左靠右補空白
            /// </summary>
            public string LOCK_DATA_3 { get; set; }

            /// <summary>
            /// 手續費負擔別_FBO_11 (3) 左靠右補空白
            /// </summary>
            public string CHECK_FORMULA_2 { get; set; }

            /// <summary>
            /// 對帳單號_FBO_12 (30) 左靠右補空白
            /// </summary>
            public string REMIT_BANK_SERIAL_NO { get; set; }

            /// <summary>
            /// 廠商代號_FBO_13 (29) 左靠右補空白
            /// </summary>
            public string BUSINESS_CORP_NO { get; set; }

            /// <summary>
            /// 銷帳參考資料_FBO_14 (76) 左靠右補空白
            /// </summary>
            public string REMIT_SHEET_NO { get; set; }

            /// <summary>
            /// 身份證ID_FBO_15 (12) 左靠右補空白
            /// </summary>
            public string CHECK_FORMULA_3 { get; set; }

            /// <summary>
            /// 附言_臨櫃_6 (80) 左靠右補空白
            /// </summary>
            public string CHECK_FORMULA_4 { get; set; }

            /// <summary>
            /// ??_臨櫃_7 (2) 左靠右補空白
            /// </summary>
            public string LOCK_DATA_4 { get; set; }
        }
    }

    public static class Utility
    {
        public static int? decimalNTointN(this decimal? d)
        {
            if (d == null)
                return null;
            int i = 0;
            try
            {
                i = Convert.ToInt32(d);
                return i;
            }
            catch
            {
                return null;
            }
        }
    }
}