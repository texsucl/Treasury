using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using System.Data;
using FAP.Web.Daos;
using System.Web.Mvc;
using FAP.Web.Models;

/// <summary>
/// 功能說明：應付票據–轉入支票簽收 (簽收資料個案產出)
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0023 : Common, IOAP0023
    {
        /// <summary>
        /// 查詢 應付票據–轉入支票簽收 (簽收資料個案產出)
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public List<OAP0023Model> Search_OAP0023(OAP0023SearchModel searchModel)
        {
            List<OAP0023Model> result = new List<OAP0023Model>();      
            List<Tuple<string, string>> ref_nos = new List<Tuple<string, string>>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                string c = string.Empty;
                int i = 0;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select REF_NO,TEXT from LPMCODE1
where group_id = 'AP_PAID'
and srce_from = 'AP' ; ";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        ref_nos.Add(new Tuple<string, string>(dbresult["REF_NO"]?.ToString()?.Trim(), dbresult["TEXT"]?.ToString()?.Trim()));
                    }
                    com.Dispose();
                }
                if (searchModel.type == "1")
                {
                    var date_s = searchModel.stat_date_s.DPformateTWdate(); //開票日期起
                    var date_e = searchModel.stat_date_e.DPformateTWdate(); //開票日期迄
                    var pay_class = searchModel.pay_class; //給付類型
                    var _s = searchModel.stat_date_s.Split('/');
                    var _e = searchModel.stat_date_e.Split('/');
                    var _yy = new List<string>() {
                            _s[0],_e[0]
                        }.Distinct().ToList();
                    var _mm = new List<string>() {
                            _s[1],_e[1]
                        }.Distinct().ToList();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select  CHECK_NO, (CHECK_YY) ||  LPAD(CHECK_MM ,2,'0')  || LPAD(CHECK_DD ,2,'0')  AS CHECK_DATE 
from LAPPYCK92
where CHECK_STAT = '2'
and CHECK_YY in (
";
                        c = string.Empty;
                        i = 0;
                        foreach (var item in _yy)
                        {
                            sql += $@" {c} :CHECK_YY{i} ";
                            com.Parameters.Add($@"CHECK_YY{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += @" ) and CHECK_MM in ( ";
                        c = string.Empty;
                        i = 0;
                        foreach (var item in _mm)
                        {
                            sql += $@" {c} :CHECK_MM{i} ";
                            com.Parameters.Add($@"CHECK_MM{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += @" ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var data = new OAP0023Model()
                            {
                                check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                                check_date = dbresult["CHECK_DATE"]?.ToString()?.Trim(), //開票日
                            };
                            result.Add(data);
                        }
                        com.Dispose();
                    }
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select  CHECK_NO, CHECK_DATE
from LFAPYCKK7
where CHECK_STAT = '2'
and CHECK_DATE between  :CHECK_DATE_1 and :CHECK_DATE_2 ;
";
                        com.Parameters.Add($@"CHECK_DATE_1", date_s);
                        com.Parameters.Add($@"CHECK_DATE_2", date_e);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var data = new OAP0023Model()
                            {
                                check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                                check_date = dbresult["CHECK_DATE"]?.ToString()?.Trim(), //開票日
                            };
                            result.Add(data);
                        }
                        com.Dispose();
                    }
                    var subDatas = result.Where(
                        x => Convert.ToInt32(x.check_date) >= Convert.ToInt32(date_s) &&
                        Convert.ToInt32(x.check_date) <= Convert.ToInt32(date_e))
                        .Select(x => new OAP0021DetailSubModel() { check_no = x.check_no }).ToList();
                    new OAP0021().getSubData(subDatas);
                    result = new List<OAP0023Model>();
                    result.AddRange(
                        subDatas.Where(x => !x.pay_class.IsNullOrWhiteSpace() && x.pay_class.Substring(0, 1) == pay_class)
                        .OrderBy(x=>x.check_no)
                        .Select(x => new OAP0023Model() {
                            check_no = x.check_no,
                            receiver = x.receiver,
                            check_date = x.check_date,
                            check_mount = x.amount,
                            pay_class = x.pay_class,
                            pay_class_text = ref_nos.FirstOrDefault(y => y.Item1 == x.pay_class)?.Item2,
                            adm_unit = x.adm_unit,
                            acct_abbr = x.bank_code
                        }));
                }
                else if (searchModel.type == "2")
                {
                    List<Tuple<string, string>> parms = new List<Tuple<string, string>>();
                    if (!searchModel.check_no_1_s.IsNullOrWhiteSpace() && !searchModel.check_no_1_e.IsNullOrWhiteSpace())
                        parms.Add(new Tuple<string, string>(searchModel.check_no_1_s.Trim(), searchModel.check_no_1_e.Trim()));
                    if (!searchModel.check_no_2_s.IsNullOrWhiteSpace() && !searchModel.check_no_2_e.IsNullOrWhiteSpace())
                        parms.Add(new Tuple<string, string>(searchModel.check_no_2_s.Trim(), searchModel.check_no_2_e.Trim()));
                    if (!searchModel.check_no_3_s.IsNullOrWhiteSpace() && !searchModel.check_no_3_e.IsNullOrWhiteSpace())
                        parms.Add(new Tuple<string, string>(searchModel.check_no_3_s.Trim(), searchModel.check_no_3_e.Trim()));
                    if (parms.Any())
                    {
                        foreach (var item in parms)
                        {
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
select  CHECK_NO
from LAPPYCK92
where CHECK_STAT = '2'
and CHECK_NO between :CHECK_NO_1 and :CHECK_NO_2
";
                                com.Parameters.Add($@"CHECK_NO_1", item.Item1);
                                com.Parameters.Add($@"CHECK_NO_2", item.Item2);
                                com.CommandText = sql;
                                com.Prepare();
                                DbDataReader dbresult = com.ExecuteReader();
                                while (dbresult.Read())
                                {
                                    var data = new OAP0023Model()
                                    {
                                        check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                                    };
                                    result.Add(data);
                                }
                                com.Dispose();
                            }

                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
select  CHECK_NO
from LFAPYCKK7
where CHECK_STAT = '2'
and CHECK_NO between :CHECK_NO_1 and :CHECK_NO_2
";
                                com.Parameters.Add($@"CHECK_NO_1", item.Item1);
                                com.Parameters.Add($@"CHECK_NO_2", item.Item2);
                                com.CommandText = sql;
                                com.Prepare();
                                DbDataReader dbresult = com.ExecuteReader();
                                while (dbresult.Read())
                                {
                                    var data = new OAP0023Model()
                                    {
                                        check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                                    };
                                    result.Add(data);
                                }
                                com.Dispose();
                            }
                        }
                        var subDatas = result.Select(x => new OAP0021DetailSubModel() { check_no = x.check_no }).ToList();
                        new OAP0021().getSubData(subDatas);
                        foreach (var item in result)
                        {
                            var _subData = subDatas.FirstOrDefault(x => x.check_no == item.check_no);
                            item.receiver = _subData.receiver;
                            item.check_date = _subData.check_date;
                            item.check_mount = _subData.amount;
                            item.pay_class = _subData.pay_class;
                            item.pay_class_text = ref_nos.FirstOrDefault(y => y.Item1 == _subData.pay_class)?.Item2;
                            item.adm_unit = _subData.adm_unit;
                            item.acct_abbr = _subData.bank_code;
                        }
                    }
                }
                if(result.Any())
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select  CHECK_NO
from FAPPYSN0
where FLAG = 'N'
and CHECK_NO in ( 
";
                        c = string.Empty;
                        i = 0;
                        foreach (var item in result.Select(x=>x.check_no))
                        {
                            sql += $@" {c} :CHECK_NO_{i} ";
                            com.Parameters.Add($@"CHECK_NO_{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += @" ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var _check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //應付票據簽收檔有支票號碼尚未簽收
                            var _result = result.First(x => x.check_no == _check_no);
                            _result.flag = "N";
                            _result.checkFlag = false;
                            _result.flag_D = "應付票據簽收檔有支票號碼尚未簽收";
                        }
                        com.Dispose();
                    }
                }
                conn.Dispose();
                conn.Close();
            }
            return result;
        }


        /// <summary>
        /// 執行轉入新增 FAPPYSN0應付票據簽收檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId">執行人員</param>
        /// <param name="apt_id">簽收窗口</param>
        /// <param name="dep_id">簽收窗口單位</param>
        /// <returns></returns>
        public MSGReturnModel InsertFAPPYSN0(List<OAP0023Model> model,string userId,string apt_id,string dep_id)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.DESCRIPTION = MessageType.insert_Fail.GetDescription(null,"無轉入資料");
            var aplyNo = string.Empty;

            if (model.Any(x => x.checkFlag))
            {
                var _UNIT_CODE = GetMemoByUserId(new List<string>() { userId })?.FirstOrDefault()?.Item3;
                var dtn = DateTime.Now;
                var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
                var updatetm = $@"{dtn.ToString("HHmmssff")}";

                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    string sql = string.Empty;
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();

                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                    String qPreCode = curDateTime[0];                  
                    var cId = sysSeqDao.qrySeqNo("AP", "CP", qPreCode).ToString();
                    aplyNo = $@"CP{qPreCode}{cId.ToString().PadLeft(3, '0')}";

                    foreach (var item in model.Where(x => x.checkFlag))
                    {
                        using (EacCommand com = new EacCommand(conn))
                        {
                            sql = $@"
INSERT INTO LAPPYSN1 (APPLY_NO, ACCT_ABBR, CHECK_NO, APPLY_ID, ENTRY_ID, ENTRY_DATE, ENTRY_TIME, FLAG, SRCE_FROM, UNIT_CODE, ADM_UNIT) 
VALUES (:APPLY_NO, :ACCT_ABBR, :CHECK_NO, :APPLY_ID, :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME, :FLAG, :SRCE_FROM, :UNIT_CODE, :ADM_UNIT) 
";
                            com.Parameters.Add($@"APPLY_NO", aplyNo.strto400DB()); //申請單號
                            com.Parameters.Add($@"ACCT_ABBR", item.acct_abbr.strto400DB()); //帳戶簡稱
                            com.Parameters.Add($@"CHECK_NO", item.check_no.strto400DB()); //支票號碼
                            com.Parameters.Add($@"APPLY_ID", apt_id.strto400DB()); //申請人員
                            com.Parameters.Add($@"ENTRY_ID", userId.strto400DB()); //輸入人員
                            com.Parameters.Add($@"ENTRY_DATE", updatedt); //輸入日期
                            com.Parameters.Add($@"ENTRY_TIME", updatetm); //輸入時間
                            com.Parameters.Add($@"FLAG", "N"); //簽收否
                            com.Parameters.Add($@"SRCE_FROM", "3"); //資料來源 3:批次轉入                                            
                            com.Parameters.Add($@"UNIT_CODE", dep_id.strto400DB()); //申請人部門
                            com.Parameters.Add($@"ADM_UNIT", item.adm_unit.strto400DB()); //收件部門
                            com.Transaction = transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            com.Dispose();
                        }
                    }
                    try
                    {
                        transaction.Commit();
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = MessageType.insert_Success.GetDescription(null,$@"申請單號為 : {aplyNo}");
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                        result.DESCRIPTION = MessageType.insert_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
            }
            return result;

        }

        /// <summary>
        /// 獲取 支票收件部門明細
        /// </summary>
        /// <returns></returns>
        public SelectList GetDepGroup()
        {
            List<SelectOption> _groups = new List<SelectOption>() {
                 new SelectOption() { Value = " ",Text = "請選擇"}
            };
            var depts = GetDepts();
            var emps = GetEmps();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                foreach (var item in db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking())
                {
                    var _dept = item.dep_id;
                    var _emp = item.apt_id;
                    var _empd = string.Empty;
                    if (_emp.Length == 5)
                        _empd = emps.FirstOrDefault(x => x.MEM_MEMO1 == _emp)?.MEM_NAME;
                    else
                        _empd = emps.FirstOrDefault(x => x.MEM_ID == _emp)?.MEM_NAME;
                    if (_empd.IsNullOrWhiteSpace())
                        _empd = item.apt_name;
                    var _deptd = depts.FirstOrDefault(x => x.DEP_ID == _dept)?.DEP_NAME;
                    _groups.Add(new SelectOption()
                    {
                        Text = $@"{_deptd}({_empd})",
                        Value = $@"{_dept},{_emp}"
                    });
                }
            }
            return new SelectList(
                items: _groups,
                dataValueField: "Value",
                dataTextField: "Text"
                );
        }
    }
}