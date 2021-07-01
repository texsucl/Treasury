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
using FAP.Web.Models;
using FAP.Web.Daos;

/// <summary>
/// 功能說明：應付票據變更接收作業
/// 初版作者：20191114 Mark
/// 修改歷程：20191114 Mark
///           需求單號：201910030636
///           初版
/// 修改歷程：20200318 Mark
///           需求單號：202003130558         
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0021 : Common, IOAP0021
    {
        /// <summary>
        /// 查詢 應付票據變更接收作業
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public List<OAP0021Model> Search_OAP0021(OAP0021SearchModel searchData)
        {
            List<OAP0021Model> result = new List<OAP0021Model>();

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;

                using (EacCommand com = new EacCommand(conn))
                {
                    var date_s = searchData.apply_date_s.DPformateTWdate(); //日期起
                    var date_e = searchData.apply_date_e.DPformateTWdate(); //日期迄
                    var apply_no = searchData.apply_no;  //申請單號
                    var apply_id = searchData.apply_id; //申請人員
               
                    sql = $@"
select APPLY_NO,APPLY_DATE,APPLY_ID from FAPPYCH0
where STATUS in ('2','6')
";
                    if (!date_s.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPLY_DATE >= :APPLY_DATE_S ";
                        com.Parameters.Add("APPLY_DATE_S", date_s);
                    }
                    if (!date_e.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPLY_DATE <= :APPLY_DATE_E ";
                        com.Parameters.Add("APPLY_DATE_E", date_e);
                    }
                    if (!apply_no.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPLY_NO = :APPLY_NO ";
                        com.Parameters.Add("APPLY_NO", apply_no.Trim());
                    }
                    if (!apply_id.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPLY_ID = :APPLY_ID ";
                        com.Parameters.Add("APPLY_ID", apply_id.Trim());
                    }

                    sql += " ORDER by APPLY_DATE , APPLY_NO ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        var data = new OAP0021Model()
                        {
                            apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                            apply_date = dbresult["APPLY_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(), //申請日
                            apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(), //申請人
                        };
                        result.Add(data);
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }
            var userMemo = GetMemoByUserId(result.Select(x => x.apply_id).Distinct(),true);
            foreach (var item in result)
            {
                var _memo = userMemo.FirstOrDefault(x => x.Item1 == item.apply_id);
                if (_memo != null)
                {
                    item.apply_id_D = _memo.Item2;
                    item.apply_dep = _memo.Item3;
                    item.apply_dep_D = _memo.Item4;
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public List<OAP0021Model> Search_OAP0021A(OAP0021ASearchModel searchData,string userId)
        {
            List<OAP0021Model> result = new List<OAP0021Model>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    var date_s = searchData.rece_date_s.DPformateTWdate(); //日期起
                    var date_e = searchData.rece_date_e.DPformateTWdate(); //日期迄
                    var apply_no = searchData.apply_no;  //申請單號
                    var rece_id = searchData.rece_id; //接收人員

                    sql = $@"
select APPLY_NO,APPLY_DATE,RECE_DATE,APPLY_ID,RECE_ID from FAPPYCH0
where STATUS = '3'
";
                    if (!date_s.IsNullOrWhiteSpace())
                    {
                        sql += " and  RECE_DATE >= :RECE_DATE_S ";
                        com.Parameters.Add("RECE_DATE_S", date_s);
                    }
                    if (!date_e.IsNullOrWhiteSpace())
                    {
                        sql += " and  RECE_DATE <= :RECE_DATE_E ";
                        com.Parameters.Add("RECE_DATE_E", date_e);
                    }
                    if (!apply_no.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPLY_NO = :APPLY_NO ";
                        com.Parameters.Add("APPLY_NO", apply_no.Trim());
                    }
                    if (!rece_id.IsNullOrWhiteSpace())
                    {
                        sql += " and  RECE_ID = :RECE_ID ";
                        com.Parameters.Add("RECE_ID", rece_id.Trim());
                    }

                    sql += " ORDER BY RECE_DATE DESC, RECE_TIME DESC, APPLY_NO ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        var _rece_id = dbresult["RECE_ID"]?.ToString()?.Trim();
                        var _apprFlag = (_rece_id != userId); //接收者不可覆核自己案件
                        var data = new OAP0021Model()
                        {
                            apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                            apply_date = dbresult["APPLY_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(), //申請日
                            rece_date = dbresult["RECE_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(), //接收日
                            apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(), //申請人
                            rece_id = _rece_id, //接收人
                            apprFlag = _apprFlag, //接收人為自己不可覆核
                            checkFlag = _apprFlag //接收者為自己初始為不勾選
                        };
                        result.Add(data);
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }
            var users = result.Select(x => x.apply_id).Distinct().ToList();
            users.AddRange(result.Select(x => x.rece_id).Distinct());
            var userMemo = GetMemoByUserId(users.Distinct(),true);
            foreach (var item in result)
            {
                var _apply = userMemo.FirstOrDefault(x => x.Item1 == item.apply_id);
                if (_apply != null)
                {
                    item.apply_id_D = _apply.Item2;
                    item.apply_dep = _apply.Item3;
                    item.apply_dep_D = _apply.Item4;
                }
                var _rece = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id);
                if (_rece != null)
                {
                    item.rece_id_D = _rece.Item2;
                }
            }
            return result;
        }

        /// <summary>
        /// 應付票據接收檔 明細
        /// </summary>
        /// <param name="apply_no">申請單號</param>
        /// <returns></returns>
        public MSGReturnModel<OAP0021DetailModel> GetDetailData(string apply_no, string userId = null)
        {
            MSGReturnModel<OAP0021DetailModel> resultMSGModel = new MSGReturnModel<OAP0021DetailModel>();
            resultMSGModel.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            resultMSGModel.Datas = new OAP0021DetailModel();
            var resultModel = resultMSGModel.Datas;
            if (apply_no.Length < 2)
            {         
                return resultMSGModel;
            }
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;

                    #region 應付票據變更接收明細
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select 
APPLY_NO,
APPLY_ID,
APPLY_UNIT,
ZIP_CODE,
ADDR,
RCV_NAME,
REG_NO,
ERASE_ID,
ERASE_DATE,
ADD_RSN,
UPD_DATE,
UPD_TIME,
TEL,
MARK_RSN2,
MARK_MTH2,
SEND_ID,
SEND_UNIT,
REG_YN,
STATUS,
APPR_ID1,
RECE_DATE,
RECE_TIME,
RECE_ID,
APPR_ID2,
APPR_DATE2
from FAPPYCH0
where APPLY_NO = :APPLY_NO
";
                        com.Parameters.Add("APPLY_NO", apply_no);
                        sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            resultModel.apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                            resultModel.apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人
                            resultModel.apply_unit = dbresult["APPLY_UNIT"]?.ToString()?.Trim(); //申請人單位
                            resultModel.apply_unit_D = getFullDepName(new List<string>() { resultModel.apply_unit })?.FirstOrDefault()?.Item2; //申請人單位中文
                            resultModel.zip_code = dbresult["ZIP_CODE"]?.ToString()?.Trim(); //郵遞區號
                            resultModel.addr = dbresult["ADDR"]?.ToString()?.Trim(); //地址
                            resultModel.rcv_name = dbresult["RCV_NAME"]?.ToString()?.Trim(); //收件人
                            resultModel.reg_no = dbresult["REG_NO"]?.ToString()?.Trim(); //掛號號碼
                            resultModel.erase_id = dbresult["ERASE_ID"]?.ToString()?.Trim(); //刪除人員
                            resultModel.erase_date = dbresult["ERASE_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(); //刪除日期
                            resultModel.add_rsn = dbresult["ADD_RSN"]?.ToString()?.Trim(); //補件原因
                            resultModel.upd_date = dbresult["UPD_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(); //更新日期
                            resultModel.upd_time = dbresult["UPD_TIME"]?.ToString()?.Trim()?.stringTimeFormate(); //更新時間
                            resultModel.tel = dbresult["TEL"]?.ToString()?.Trim(); //保戶電話
                            resultModel.mark_rsn2 = setDefault(dbresult["MARK_RSN2"]?.ToString()?.Trim(), "4"); //變更原因
                            resultModel.mark_mth2 = setDefault(dbresult["MARK_MTH2"]?.ToString()?.Trim(), "1"); //申辦方式
                            resultModel.send_id = dbresult["SEND_ID"]?.ToString()?.Trim(); //送件人員
                            resultModel.send_unit = dbresult["SEND_UNIT"]?.ToString()?.Trim(); //送件單位
                            resultModel.reg_yn = dbresult["REG_YN"]?.ToString()?.Trim(); //雙掛號回執
                            resultModel.reg_yn = resultModel.reg_yn == "1" ? "Y" : (resultModel.reg_yn == "2" ? "N" : resultModel.reg_yn);
                            resultModel.status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                            resultModel.appr_id1 = dbresult["APPR_ID1"]?.ToString()?.Trim(); //覆核人員１(簽收)
                            resultModel.rece_date = dbresult["RECE_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(); //接收日期
                            resultModel.rece_time = dbresult["RECE_TIME"]?.ToString()?.Trim()?.stringTimeFormate(); //接收時間
                            resultModel.rece_id = dbresult["RECE_ID"]?.ToString()?.Trim(); //接收人員
                            resultModel.appr_id2 = dbresult["APPR_ID2"]?.ToString()?.Trim(); //覆核人員2 
                            resultModel.appr_date2 = dbresult["APPR_DATE2"]?.ToString()?.Trim()?.stringTimeFormate(); //覆核日期2 
                            if (!resultModel.send_id.IsNullOrWhiteSpace())
                            {
                                resultModel.send_id_D = callEBXGXFK(resultModel.send_id).Item2;
                            }
                            resultModel.check_nos = new List<OAP0021DetailSubModel>();
                        }
                        com.Dispose();
                    }
                    #endregion

                    #region 檢核
                    if (resultModel.apply_no == null)
                    {
                        resultMSGModel.DESCRIPTION = MessageType.data_Not_Compare.GetDescription();
                        return resultMSGModel;
                    }
                    if (!(resultModel.status == "2" || resultModel.status == "3" || resultModel.status == "6"))
                    {
                        resultMSGModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                        return resultMSGModel;
                    }
                    #endregion

                    #region  應付票據變更接收明細 支票檔
                    using (EacCommand com = new EacCommand(conn))
                    {
                        if (!resultModel.apply_no.IsNullOrWhiteSpace())
                        {
                            var codes = new SysCodeDao().qryByType("AP", "MARK_TYPE");
                            sql = $@"
select 
CHECK_NO,
MARK_TYPE2,
NEW_HEAD
from FAPPYCD0
where APPLY_NO = :APPLY_NO
order by CHECK_NO
";
                            com.Parameters.Add("APPLY_NO", apply_no); //申請單號
                            sql += " ;";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                OAP0021DetailSubModel result = new OAP0021DetailSubModel();
                                result.apply_no = apply_no;
                                result.check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                                result.mark_type2 = setDefault(dbresult["MARK_TYPE2"]?.ToString()?.Trim(),"1"); //目前註記
                                result.mark_type2_D = codes.Where(x => x.CODE == result.mark_type2).Select(x => x.CODE + ":" + x.CODE_VALUE).FirstOrDefault();
                                result.new_head = dbresult["NEW_HEAD"]?.ToString()?.Trim(); //新抬頭
                                resultModel.check_nos.Add(result);
                            }
                            com.Dispose();
                        }
                    }
                    if (resultModel.check_nos.Any())
                    {
                        #region 應付票據檔 
                        getSubData(resultModel.check_nos);
                        #endregion
                    }
                    #endregion

                    conn.Dispose();
                    conn.Close();
                    resultMSGModel.RETURN_FLAG = true;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                resultMSGModel.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            }

            return resultMSGModel;
        }

        /// <summary>
        /// 應付票據接收檔 之票明細 by 支票號碼
        /// </summary>
        /// <param name="resultModel"></param>
        public MSGReturnModel getSubData(List<OAP0021DetailSubModel> resultModel)
        {
            MSGReturnModel model = new MSGReturnModel();
            model.DESCRIPTION = MessageType.data_Not_Compare.GetDescription("支票明細");
            if (resultModel.Any())
            {
                List<SYS_CODE> check_status_F = new List<SYS_CODE>();
                List<SYS_CODE> check_status_A = new List<SYS_CODE>();
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    check_status_F = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "F_CHECK_STATUS").ToList();
                    check_status_A = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "A_CHECK_STATUS").ToList();
                }
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;
                    int i = 0;
                    string c = string.Empty;
                    var check_nos = resultModel.Select(x => x.check_no).Distinct().ToList();
                    #region 應付票據檔 F檔 : LAPPYCK92
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select
CHECK_NO,
AMOUNT,
(CHECK_YY) ||  LPAD(CHECK_MM ,2,'0')  || LPAD(CHECK_DD ,2,'0')  AS CHECK_DATE ,
CHECK_STAT,
RECEIVER,
APLY_NO,
APLY_SEQ,
BANK_CODE
from LAPPYCK92
where 1 = 1 
and check_no in ( ";
                        foreach (var item in check_nos)
                        {
                            sql += $@" {c} :check_no{i} ";
                            com.Parameters.Add($@"check_no{i}", item);
                            c = " , ";
                            i += 1;
                        }

                        sql += " ) ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                            foreach (var _check_no in resultModel.Where(x => x.check_no == check_no))
                            {
                                var _check_stat = dbresult["CHECK_STAT"]?.ToString()?.Trim(); //支票狀態
                                _check_no.amount = dbresult["AMOUNT"]?.ToString()?.Trim().formateThousand(); //金額
                                _check_no.check_date = dbresult["CHECK_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(); //支票到期日
                                _check_no.check_stat = _check_stat;
                                _check_no.check_stat_D = check_status_F.FirstOrDefault(x => x.CODE == _check_stat)?.CODE_VALUE ?? _check_stat;
                                _check_no.receiver = getReceiver(dbresult["RECEIVER"]?.ToString()?.Trim()); //付款對象
                                _check_no.aply_no = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                                _check_no.aply_seq = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                                _check_no.bank_code = dbresult["BANK_CODE"]?.ToString()?.Trim(); //付款帳戶
                                _check_no.system = "F";
                            }
                        }
                        com.Dispose();

                    }
                    #endregion

                    #region 應付票據檔 A檔 : LFAPYCKK7

                    i = 0;
                    c = string.Empty;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select 
CHECK_NO,
AMOUNT,
CHECK_DATE ,
CHECK_STAT,
RECEIVER,
APLY_NO,
APLY_SEQ,
ACCT_ABBR
from LFAPYCKK7
where 1 = 1 
and check_no in ( ";
                        foreach (var item in check_nos)
                        {
                            sql += $@" {c} :check_no{i} ";
                            com.Parameters.Add($@"check_no{i}", item);
                            c = " , ";
                            i += 1;
                        }

                        sql += " ) ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                            foreach (var _check_no in resultModel.Where(x => x.check_no == check_no))
                            {
                                var _check_stat = dbresult["CHECK_STAT"]?.ToString()?.Trim(); //支票狀態
                                _check_no.amount = dbresult["AMOUNT"]?.ToString()?.Trim().formateThousand(); //金額
                                _check_no.check_date = dbresult["CHECK_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(); //支票到期日
                                _check_no.check_stat = _check_stat;
                                _check_no.check_stat_D = check_status_A.FirstOrDefault(x => x.CODE == _check_stat)?.CODE_VALUE ?? _check_stat;
                                _check_no.receiver = getReceiver(dbresult["RECEIVER"]?.ToString()?.Trim()); //付款對象
                                _check_no.aply_no = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                                _check_no.aply_seq = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                                _check_no.bank_code = dbresult["ACCT_ABBR"]?.ToString()?.Trim(); //付款帳戶
                                _check_no.system = "A";
                            }
                        }
                        com.Dispose();
                    }
                    #endregion

                    #region 支票中介檔 : LGLGPCK3

                    var aply_nos = resultModel.Where(x => x.aply_no != null).Select(x => x.aply_no).Distinct().ToList();
                    var aply_seqs = resultModel.Where(x => x.aply_seq != null).Select(x => x.aply_seq).Distinct().ToList();

                    if (aply_nos.Any() && aply_seqs.Any())
                    {
                        using (EacCommand com = new EacCommand(conn))
                        {

                            sql = $@"
select APLY_NO,APLY_SEQ,PROC_MARK,PAY_CLASS,ADM_UNIT from LGLGPCK3
where 1 = 1
";
                            i = 0;
                            c = string.Empty;
                            sql += " and aply_no in ( ";
                            foreach (var item in aply_nos)
                            {
                                sql += $@" {c} :aply_no{i} ";
                                com.Parameters.Add($@"aply_no{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ";
                            i = 0;
                            c = string.Empty;
                            sql += " and aply_seq in ( ";
                            foreach (var item in aply_seqs)
                            {
                                sql += $@" {c} :aply_seq{i} ";
                                com.Parameters.Add($@"aply_seq{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                var aply_no = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                                var aply_seq = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                                foreach (var _check_no in resultModel.Where(x => x.aply_no == aply_no && x.aply_seq == aply_seq))
                                {
                                    _check_no.proc_mark = dbresult["PROC_MARK"]?.ToString()?.Trim(); //處理註記
                                    _check_no.pay_class = dbresult["PAY_CLASS"]?.ToString()?.Trim(); //給付類型
                                    _check_no.adm_unit = dbresult["ADM_UNIT"]?.ToString()?.Trim(); //行政單位
                                }
                            }
                            com.Dispose();
                        }
                        model.RETURN_FLAG = true;
                        model.DESCRIPTION = MessageType.query_Success.GetDescription();
                    }

                    #endregion
                    conn.Dispose();
                    conn.Close();
                }
            }
            return model;
        }

        /// <summary>
        /// 接收應付票據變更申請檔
        /// </summary>
        /// <param name="updateModel">接收資料</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel RECEFAPPYCH0(OAP0021DetailModel updateModel, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();

            resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();

            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                string sql = string.Empty;
                conn.Open();
                var apply_no = updateModel.apply_no;

                #region 檢核資料有無異動             
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select 
STATUS
from FAPPYCH0
where APPLY_NO = :APPLY_NO
";
                    com.Parameters.Add("APPLY_NO", apply_no); //申請單號
                    sql += " ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    var status = string.Empty;
                    while (dbresult.Read())
                    {
                        status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                    }
                    if (!(status == "2" || status == "6"))
                    {
                        resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                        return resultModel;
                    }
                    com.Dispose();
                }
                #endregion

                EacTransaction transaction = conn.BeginTransaction();

                #region 更新應付票據變更申請主檔
                using (EacCommand com = new EacCommand(conn))
                {
                    com.Transaction = transaction;
                    sql = $@"
update LAPPYCH1
set 
ZIP_CODE = :ZIP_CODE,
ADDR = :ADDR,
RCV_NAME = :RCV_NAME,
TEL = :TEL,
MARK_RSN2 = :MARK_RSN2,
MARK_MTH2 = :MARK_MTH2,
SEND_ID = :SEND_ID,
SEND_UNIT = :SEND_UNIT,
STATUS = :STATUS,
RECE_ID = :RECE_ID,
RECE_DATE = :RECE_DATE,
RECE_TIME = :RECE_TIME,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where APPLY_NO = :APPLY_NO
";
                    com.Parameters.Add("ZIP_CODE", updateModel.zip_code.strto400DB()); //郵遞區號
                    com.Parameters.Add("ADDR", updateModel.addr.strto400DB()); //地址
                    com.Parameters.Add("RCV_NAME", updateModel.rcv_name.strto400DB()); //收件人
                    com.Parameters.Add("TEL", updateModel.tel.strto400DB()); //保戶電話
                    com.Parameters.Add("MARK_RSN2", updateModel.mark_rsn2.strto400DB()); //目前變更原因
                    com.Parameters.Add("MARK_MTH2", updateModel.mark_mth2.strto400DB()); //目前申辦方式
                    com.Parameters.Add("SEND_ID", updateModel.send_id.strto400DB()); //送件人員
                    com.Parameters.Add("SEND_UNIT", updateModel.send_unit.strto400DB()); //送件單位
                    com.Parameters.Add("STATUS", "3"); //3:受理中
                    com.Parameters.Add("RECE_ID", userId.strto400DB()); //接收人員
                    com.Parameters.Add("RECE_DATE", updatedt); //接收日期
                    com.Parameters.Add("RECE_TIME", updatetm);//接收時間
                    com.Parameters.Add("UPD_DATE", updatedt); //更新日期
                    com.Parameters.Add("UPD_TIME", updatetm); //更新時間
                    com.Parameters.Add("APPLY_NO", updateModel.apply_no.strto400DB()); //申請單號
                    com.CommandText = sql;
                    com.Prepare();
                    var updateNum = com.ExecuteNonQuery();
                    com.Dispose();
                }
                #endregion

                #region 更新明細檔
                var msg = updateFAPPYCD0(updateModel,userId,conn,transaction,updatedt,updatetm);
                #endregion
                if (msg.IsNullOrWhiteSpace())
                {
                    try
                    {
                        transaction.Commit();
                        resultModel.RETURN_FLAG = true;
                        resultModel.DESCRIPTION = MessageType.RECE_Success.GetDescription();
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                        resultModel.DESCRIPTION = MessageType.RECE_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
                else
                {
                    resultModel.DESCRIPTION = msg;
                }
                conn.Dispose();
                conn.Close();
            }
            return resultModel;
        }

        /// <summary>
        /// 退回應付票據變更申請檔
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel REJFAPPYCH0(OAP0021DetailModel updateModel, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();

            resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();

            List<FAP_MAIL_LABEL_TEMP> FMLTs = new List<FAP_MAIL_LABEL_TEMP>();
            SysSeqDao sysSeqDao = new SysSeqDao();
            string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
            String qPreCode = curDateTime[0];

            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                string sql = string.Empty;
                conn.Open();
                var apply_no = updateModel.apply_no;
                try
                {
                    #region 檢核資料有無異動
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select 
STATUS
from FAPPYCH0
where APPLY_NO = :APPLY_NO
";
                        com.Parameters.Add("APPLY_NO", apply_no);
                        sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        var status = string.Empty;
                        while (dbresult.Read())
                        {
                            status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                        }
                        if (!(status == "2" || status == "6"))
                        {
                            resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                            return resultModel;
                        }
                        com.Dispose();
                    }
                    #endregion
                
                    EacTransaction transaction = conn.BeginTransaction();
                
                    #region 更新應付票據變更申請主檔
                    using (EacCommand com = new EacCommand(conn))
                    {
                        com.Transaction = transaction;
                        sql = $@"
update LAPPYCH1
set 
REJ_RSN = :REJ_RSN,
CHECK_FLAG = :CHECK_FLAG,
STATUS = :STATUS,
RECE_ID = :RECE_ID,
RECE_DATE = :RECE_DATE,
RECE_TIME = :RECE_TIME,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where APPLY_NO = :APPLY_NO
";                      
                        com.Parameters.Add("REJ_RSN", updateModel.rej_rsn.strto400DB()); //退件原因
                        com.Parameters.Add("CHECK_FLAG", updateModel.check_flag); //實體支票
                        com.Parameters.Add("STATUS", "5"); //5:退件
                        com.Parameters.Add("RECE_ID", userId); //接收人員
                        com.Parameters.Add("RECE_DATE", updatedt); //接收日期
                        com.Parameters.Add("RECE_TIME", updatetm); //接收時間
                        com.Parameters.Add("UPD_DATE", updatedt); //更新日期
                        com.Parameters.Add("UPD_TIME", updatetm); //更新時間
                        com.Parameters.Add("APPLY_NO", apply_no);
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();  
                        com.Dispose();
                    }
                    #endregion
                 
                    #region 更新應付票據變更申請明細檔
                    var msg =  updateFAPPYCD0(updateModel, userId, conn, transaction, updatedt, updatetm);
                    #endregion

                    if (msg.IsNullOrWhiteSpace())
                    {
                        #region 退件確認後處理進度異動為 5:退件，實體支票=Y時把資料寫入應付票據支票簽收檔
                        if (updateModel.check_flag == "Y" && updateModel.check_nos.Any())
                        {

                            sql = $@"
INSERT INTO LAPPYSN1 (APPLY_NO, ACCT_ABBR, CHECK_NO, APPLY_ID, ENTRY_ID, ENTRY_DATE, ENTRY_TIME, FLAG, SRCE_FROM, UNIT_CODE) 
VALUES 
";
                            using (EacCommand com = new EacCommand(conn))
                            {
                                var i = 0;
                                var c = string.Empty;

                                var _UNIT_CODE = GetMemoByUserId(new List<string>() { updateModel.apply_id })?.FirstOrDefault()?.Item3;

                                var _apply_unit = updateModel.apply_unit; //申請單位
                                var _apply_id = updateModel.apply_id; //申請人員

                                foreach (var item in updateModel.check_nos)
                                {
                                    sql += $@" {c} ( :APPLY_NO_{i} , :ACCT_ABBR_{i} , :CHECK_NO_{i} , :APPLY_ID_{i} , :ENTRY_ID_{i} , :ENTRY_DATE_{i} , :ENTRY_TIME_{i} , :FLAG_{i} , :SRCE_FROM_{i} , :UNIT_CODE_{i}) ";
                                    com.Parameters.Add($@"APPLY_NO_{i}", updateModel.apply_no.strto400DB()); //申請單號
                                    com.Parameters.Add($@"ACCT_ABBR_{i}", item.bank_code.strto400DB()); //帳戶簡稱
                                    com.Parameters.Add($@"CHECK_NO_{i}", item.check_no); //支票號碼 
                                    com.Parameters.Add($@"APPLY_ID_{i}", updateModel.apply_id.strto400DB()); //申請人員
                                    com.Parameters.Add($@"ENTRY_ID_{i}", userId.strto400DB()); //輸入人員
                                    com.Parameters.Add($@"ENTRY_DATE_{i}", updatedt); //輸入日期
                                    com.Parameters.Add($@"ENTRY_TIME_{i}", updatetm); //輸入時間
                                    com.Parameters.Add($@"FLAG_{i}", "N"); //簽收否
                                    com.Parameters.Add($@"SRCE_FROM_{i}", "2"); //資料來源 2:票據變更
                                    com.Parameters.Add($@"UNIT_CODE_{i}", _UNIT_CODE.strto400DB());
                                    i += 1;
                                    c = " , ";

                                    #region 應付票據變更退件(有實體支票) 把資料寫入 信封標籤檔案

                                    FMLTs.Add(new FAP_MAIL_LABEL_TEMP()
                                    {
                                        send_style = updateModel.reg_yn == "Y" ? "1" : (updateModel.reg_yn == "N" ? "2" : updateModel.reg_yn), //寄送方式
                                        zip_code = updateModel.zip_code?.Trim(), //郵遞區號
                                        addr = updateModel.addr?.Trim(), //地址
                                        rcv_id = updateModel.rcv_name?.Trim(), //收件人員
                                        apply_no = updateModel.apply_no, //申請單號
                                        check_no = item.check_no?.Trim(), //支票號碼
                                        apply_name = _apply_unit?.Trim(), //申請人單位
                                        apply_id = _apply_id?.Trim(), //申請人
                                        rece_id = userId
                                    });
                                    #endregion
                                }
                                com.Transaction = transaction;
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                        }
                        #endregion
                        transaction.Commit();
                        try
                        {
                            if (FMLTs.Any())
                            {
                                using (dbFGLEntities db = new dbFGLEntities())
                                {
                                    List<FAP_MAIL_LABEL> FMLs = new List<FAP_MAIL_LABEL>();
                                    List<FAP_MAIL_LABEL_D> FMLDs = new List<FAP_MAIL_LABEL_D>();
                                    foreach (var items in FMLTs.GroupBy(x => new
                                    {
                                        x.send_style,
                                        x.zip_code,
                                        x.addr,
                                        x.rcv_id
                                    }))
                                    {
                                        var cId = sysSeqDao.qrySeqNo("AP", "S4", qPreCode).ToString();
                                        var _id = $@"S4{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                        FMLs.Add(new FAP_MAIL_LABEL()
                                        {
                                            id = _id, //pkid
                                            send_style = items.Key.send_style, //寄送方式
                                            zip_code = items.Key.zip_code, //郵遞區號
                                            addr = items.Key.addr, //地址
                                            rcv_id = items.Key.rcv_id, //收件人員
                                            memo = GetCheckmemo(items.Select(x => x.check_no)), //備註
                                            number = items.Count(), //張數
                                            apply_name = string.Join(",", items.Select(x => x.apply_name).Distinct().OrderBy(x => x)), //行政單位
                                            apply_id = string.Join(",", items.Select(x => x.apply_id).Distinct().OrderBy(x => x)), //申請人員
                                            rece_id = string.Join(",", items.Select(x => x.rece_id).Distinct().OrderBy(x => x)), //接收人員
                                            create_datetime = dtn
                                        });
                                        foreach (var item in items)
                                        {
                                            FMLDs.Add(new FAP_MAIL_LABEL_D()
                                            {
                                                id = _id,  //pkid
                                                apply_no = item.apply_no, //應付票據變更主檔申請單號碼
                                                check_no = item.check_no //支票號碼
                                            });
                                        }
                                    }
                                    db.FAP_MAIL_LABEL.AddRange(FMLs);
                                    db.FAP_MAIL_LABEL_D.AddRange(FMLDs);
                                    db.SaveChanges();

                                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "OAP0021";
                                    piaLogMain.EXECUTION_CONTENT = $@"apply_no:{updateModel.apply_no}";
                                    piaLogMain.AFFECT_ROWS = FMLs.Count;
                                    piaLogMain.PIA_TYPE = "0100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAPPYCH";
                                    piaLogMainDao.Insert(piaLogMain);
                                }
                            }
                            resultModel.RETURN_FLAG = true;
                            resultModel.DESCRIPTION = MessageType.REJ_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            resultModel.DESCRIPTION = MessageType.REJ_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }
                    else
                    {
                        resultModel.DESCRIPTION = msg;
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    resultModel.DESCRIPTION = MessageType.REJ_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                }
                conn.Dispose();
                conn.Close();
            }

            return resultModel;
        }

        /// <summary>
        /// 應付票據變更申請檔 補件中案件
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel ADFAPPYCH0(OAP0021DetailModel updateModel, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();

            resultModel.DESCRIPTION = MessageType.sys_Error.GetDescription();

            List<FAP_MAIL_LABEL_TEMP> FMLTs = new List<FAP_MAIL_LABEL_TEMP>();
            SysSeqDao sysSeqDao = new SysSeqDao();

            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                string sql = string.Empty;
                conn.Open();
                var apply_no = updateModel.apply_no;
                try
                {
                    #region 檢核資料有無異動
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select 
STATUS
from FAPPYCH0
where APPLY_NO = :APPLY_NO
";
                        com.Parameters.Add("APPLY_NO", apply_no);
                        sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        var status = string.Empty;
                        while (dbresult.Read())
                        {
                            status = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                        }
                        if (!(status == "2" || status == "6"))
                        {
                            resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                            return resultModel;
                        }
                        com.Dispose();
                    }
                    #endregion

                    EacTransaction transaction = conn.BeginTransaction();

                    #region 更新應付票據變更申請主檔
                    using (EacCommand com = new EacCommand(conn))
                    {
                        com.Transaction = transaction;
                        sql = $@"
update LAPPYCH1
set 
ADD_RSN = :ADD_RSN,
STATUS = :STATUS,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where APPLY_NO = :APPLY_NO
";
                        com.Parameters.Add("ADD_RSN", updateModel.add_rsn.strto400DB()); //退件原因
                        com.Parameters.Add("STATUS", "6"); //6:補件中案件
                        com.Parameters.Add("UPD_DATE", updatedt); //更新日期
                        com.Parameters.Add("UPD_TIME", updatetm); //更新時間
                        com.Parameters.Add("APPLY_NO", apply_no);
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                    }
                    #endregion

                    #region 更新應付票據變更申請明細檔
                    var msg = updateFAPPYCD0(updateModel, userId, conn, transaction, updatedt, updatetm);
                    #endregion
                    if (msg.IsNullOrWhiteSpace())
                    {
                        transaction.Commit();
                        resultModel.RETURN_FLAG = true;
                        resultModel.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                    }
                    else
                    {
                        resultModel.DESCRIPTION = msg;
                    }                      
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    resultModel.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                }
                conn.Dispose();
                conn.Close();
            }

            return resultModel;
        }

        /// <summary>
        /// 執行 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="updateModel">待處理資料</param>
        /// <param name="userId"></param>
        /// <param name="flag">true = 核准 false = 駁回</param>
        /// <returns></returns>
        public MSGReturnModel UpdateOAP0021A(List<OAP0021Model> apprModel,  string userId, bool flag)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            List<OAP0021DetailModel> updateModel = new List<OAP0021DetailModel>();
            List<FAP_MAIL_LABEL_TEMP> FMLTs = new List<FAP_MAIL_LABEL_TEMP>();
            List<FAP_MAIL_LABEL> FMLs = new List<FAP_MAIL_LABEL>();
            List<FAP_MAIL_LABEL_D> FMLDs = new List<FAP_MAIL_LABEL_D>();

            SysSeqDao sysSeqDao = new SysSeqDao();
            string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
            String qPreCode = curDateTime[0];
            List<SYS_CODE> _SYS_CODEs = new List<SYS_CODE>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                _SYS_CODEs = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "MARK_RSN").ToList();
            }

            foreach (var item in apprModel)
            {
                var _searchdata = GetDetailData(item.apply_no);
                if (_searchdata.RETURN_FLAG && _searchdata.Datas.status == "3") //檢查資料有無異動
                    updateModel.Add(_searchdata.Datas);
                else if (_searchdata.RETURN_FLAG && _searchdata.Datas.status != "3")
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                    return resultModel;
                }
                else
                {
                    resultModel.DESCRIPTION = _searchdata.DESCRIPTION;
                    return resultModel;
                }            
            }       
            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    string sql = string.Empty;
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();
                    var _status = flag ? "7" : "2"; //接收 => 7.待用印 , 駁回 => 2.完成申請

                    #region 更新應付票據變更申請主檔 狀態
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
update LAPPYCH1 set
STATUS = :STATUS,
APPR_ID2 = :APPR_ID2,
APPR_DATE2 = :APPR_DATE2,
APPR_TIME2 = :APPR_TIME2,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where APPLY_NO in (
";
                        var c = string.Empty;
                        var i = 0;
                        com.Parameters.Add($@"STATUS", _status.strto400DB());
                        com.Parameters.Add($@"APPR_ID2", userId.strto400DB());
                        com.Parameters.Add($@"APPR_DATE2", updatedt);
                        com.Parameters.Add($@"APPR_TIME2", updatetm);
                        com.Parameters.Add($@"UPD_DATE", updatedt);
                        com.Parameters.Add($@"UPD_TIME", updatetm);
                        foreach (var item in updateModel.Select(x => x.apply_no))
                        {
                            sql += $@" {c} :APPLY_NO{i} ";
                            com.Parameters.Add($@"APPLY_NO{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += " ) ";
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                    }
                    #endregion

                    #region FFAPYDF取消禁背及劃線註記記錄檔 & 雙掛號回執為N則寫入應付票據簽收檔
                    foreach (var item in updateModel)
                    {
                        #region 核准
                        if (flag)
                        {
                            var _send_style = item.reg_yn == "Y" ? "1" : (item.reg_yn == "N" ? "2" : item.reg_yn); //寄送方式
                            var _zip_code = item.zip_code; //郵遞區號
                            var _addr = item.addr; //地址
                            var _apply_unit = item.apply_unit; //申請單位
                            var _apply_id = item.apply_id; //申請人
                            var _UNIT_CODE = GetMemoByUserId(new List<string>() { item.apply_id })?.FirstOrDefault()?.Item3;
                            foreach (var check_no_item in item.check_nos)
                            {
                                bool alreadyFlag = false;

                                #region 判斷有無現有資料
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
select CHECK_NO from FFAPYDF
where CHECK_NO = :CHECK_NO
";
                                    com.Parameters.Add($@"CHECK_NO", check_no_item.check_no);
                                    com.CommandText = sql;
                                    com.Prepare();
                                    DbDataReader dbresult = com.ExecuteReader();
                                    while (dbresult.Read())
                                    {
                                        alreadyFlag = true; //已有資料
                                    }
                                    com.Dispose();
                                }
                                #endregion

                                #region 新增 or 異動 FFAPYDF取消禁背及劃線註記記錄檔
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    if (alreadyFlag) //更新資料
                                    {
                                        sql = $@"
update FFAPYDF set
ACCT_ABBR = :ACCT_ABBR,
MARK_TYPE1 = MARK_TYPE2,
MARK_TYPE2 = :MARK_TYPE2,
CMPIND = 'Y',
ENTRY_DATE = :ENTRY_DATE,
ENTRY_ID  = :ENTRY_ID,
NEW_HEAD1 = NEW_HEAD,
NEW_HEAD = :NEW_HEAD,
MARK_MTH1 = MARK_MTH2,
MARK_MTH2 = :MARK_MTH2,
MARK_RSN1 = MARK_RSN2,
MARK_RSN2 = :MARK_RSN2,
SEND_ID = :SEND_ID,
SEND_UNIT = :SEND_UNIT,
EDIT_CNT = (EDIT_CNT + 1),
MARK_MEMO1 = MARK_MEMO,
MARK_MEMO = :MARK_MEMO,
UPD_ID = :UPD_ID,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME,
STAT_ID = :STAT_ID,
STAT_DATE = :STAT_DATE,
FIELD10 = :FIELD10,
FIELD15 = :FIELD15
where CHECK_NO = :CHECK_NO
";
                                        com.Parameters.Add($@"ACCT_ABBR", check_no_item.bank_code.strto400DB());                                   
                                        com.Parameters.Add($@"MARK_TYPE2", check_no_item.mark_type2.strto400DB());
                                        com.Parameters.Add($@"ENTRY_DATE", item.rece_date);
                                        com.Parameters.Add($@"ENTRY_ID", item.rece_id.strto400DB());
                                        com.Parameters.Add($@"NEW_HEAD", check_no_item.new_head.strto400DB());
                                        com.Parameters.Add($@"MARK_MTH2", item.mark_mth2.strto400DB());
                                        com.Parameters.Add($@"MARK_RSN2", item.mark_rsn2.strto400DB());
                                        com.Parameters.Add($@"SEND_ID", item.send_id.strto400DB());
                                        com.Parameters.Add($@"SEND_UNIT,", item.send_unit.strto400DB());
                                        com.Parameters.Add($@"MARK_MEMO,", (item.mark_rsn2 == "4") ? _SYS_CODEs.FirstOrDefault(x=>x.CODE == item.mark_rsn2)?.CODE_VALUE : string.Empty);
                                        com.Parameters.Add($@"UPD_ID", item.rece_id.strto400DB());
                                        com.Parameters.Add($@"UPD_DATE", item.rece_date);
                                        com.Parameters.Add($@"UPD_TIME", item.rece_time);
                                        com.Parameters.Add($@"STAT_ID", userId.strto400DB());
                                        com.Parameters.Add($@"STAT_DATE", updatedt);
                                        com.Parameters.Add($@"FIELD10", item.apply_id.strto400DB());
                                        com.Parameters.Add($@"FIELD15", item.apply_no.strto400DB());
                                        com.Parameters.Add($@"CHECK_NO", check_no_item.check_no.strto400DB());
                                    }
                                    else //新增資料
                                    {
                                        sql = $@"
insert into FFAPYDF  (ACCT_ABBR, CHECK_NO, MARK_TYPE2, CMPIND, ENTRY_DATE, ENTRY_ID, NEW_HEAD, MARK_MTH2, MARK_RSN2, 
SEND_ID, SEND_UNIT, EDIT_CNT, MARK_MEMO, UPD_ID, UPD_DATE, UPD_TIME, STAT_ID, STAT_DATE, FIELD10, FIELD15) 
VALUES ( :ACCT_ABBR, :CHECK_NO, :MARK_TYPE2, 'Y', :ENTRY_DATE, :ENTRY_ID, :NEW_HEAD, :MARK_MTH2, :MARK_RSN2, 
:SEND_ID, :SEND_UNIT, 1, :MARK_MEMO, :UPD_ID, :UPD_DATE, :UPD_TIME, :STAT_ID, :STAT_DATE, :FIELD10, :FIELD15)
";
                                        com.Parameters.Add($@"ACCT_ABBR", check_no_item.bank_code.strto400DB());
                                        com.Parameters.Add($@"CHECK_NO", check_no_item.check_no.strto400DB());
                                        com.Parameters.Add($@"MARK_TYPE2", check_no_item.mark_type2.strto400DB());
                                        com.Parameters.Add($@"ENTRY_DATE", item.rece_date);
                                        com.Parameters.Add($@"ENTRY_ID", item.rece_id.strto400DB());
                                        com.Parameters.Add($@"NEW_HEAD", check_no_item.new_head.strto400DB());
                                        com.Parameters.Add($@"MARK_MTH2", item.mark_mth2.strto400DB());
                                        com.Parameters.Add($@"MARK_RSN2", item.mark_rsn2.strto400DB());
                                        com.Parameters.Add($@"SEND_ID", item.send_id.strto400DB());
                                        com.Parameters.Add($@"SEND_UNIT,", item.send_unit.strto400DB());
                                        com.Parameters.Add($@"MARK_MEMO,", (item.mark_rsn2 == "4") ? _SYS_CODEs.FirstOrDefault(x => x.CODE == item.mark_rsn2)?.CODE_VALUE : string.Empty);
                                        com.Parameters.Add($@"UPD_ID", item.rece_id.strto400DB());
                                        com.Parameters.Add($@"UPD_DATE", item.rece_date);
                                        com.Parameters.Add($@"UPD_TIME", item.rece_time);
                                        com.Parameters.Add($@"STAT_ID", userId.strto400DB());
                                        com.Parameters.Add($@"STAT_DATE", updatedt);
                                        com.Parameters.Add($@"FIELD10", item.apply_id.strto400DB());
                                        com.Parameters.Add($@"FIELD15", item.apply_no.strto400DB());
                                    }
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                                #endregion

                                #region 雙掛號回執為N則寫入應付票據簽收檔
                                if (item.reg_yn == "N")
                                {
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"
INSERT INTO LAPPYSN1 (APPLY_NO, ACCT_ABBR, CHECK_NO, APPLY_ID, ENTRY_ID, ENTRY_DATE, ENTRY_TIME, FLAG, SRCE_FROM, UNIT_CODE) 
VALUES (:APPLY_NO, :ACCT_ABBR, :CHECK_NO, :APPLY_ID, :ENTRY_ID, :ENTRY_DATE, :ENTRY_TIME, :FLAG, :SRCE_FROM, :UNIT_CODE) 
";
                                        com.Parameters.Add($@"APPLY_NO", check_no_item.apply_no.strto400DB()); //申請單號
                                        com.Parameters.Add($@"ACCT_ABBR", check_no_item.bank_code.strto400DB()); //帳戶簡稱
                                        com.Parameters.Add($@"CHECK_NO", check_no_item.check_no.strto400DB()); //支票號碼
                                        com.Parameters.Add($@"APPLY_ID", item.apply_id.strto400DB()); //申請人員
                                        com.Parameters.Add($@"ENTRY_ID", userId.strto400DB()); //輸入人員
                                        com.Parameters.Add($@"ENTRY_DATE", updatedt); //輸入日期
                                        com.Parameters.Add($@"ENTRY_TIME", updatetm); //輸入時間
                                        com.Parameters.Add($@"FLAG", "N"); //簽收否
                                        com.Parameters.Add($@"SRCE_FROM", "2"); //資料來源 2:票據變更                                            
                                        com.Parameters.Add($@"UNIT_CODE", _UNIT_CODE.strto400DB()); //申請人部門
                                        com.Transaction = transaction;
                                        com.CommandText = sql;
                                        com.Prepare();
                                        var updateNum = com.ExecuteNonQuery();
                                        com.Dispose();
                                    }
                                }
                                #endregion

                                #region 新增信封標籤暫存檔 移置執行 用印檢視確認 執行 20200519
                                //FMLTs.Add(new FAP_MAIL_LABEL_TEMP()
                                //{
                                //    send_style = item.reg_yn == "Y" ? "1" : (item.reg_yn == "N" ? "2" : item.reg_yn), //寄送方式
                                //    zip_code = item.zip_code, //郵遞區號
                                //    addr = item.addr, //地址
                                //    rcv_id = item.rcv_name, //收件人員
                                //    apply_no = item.apply_no, //申請單號
                                //    check_no = check_no_item.check_no, //支票號碼
                                //    apply_name = _apply_unit, //申請人單位
                                //    apply_id = _apply_id, //申請人
                                //    rece_id = item.rece_id //接收者
                                //});
                                #endregion
                            }
                        }
                        #endregion
                    }
                    #endregion
                    transaction.Commit();
                    resultModel.RETURN_FLAG = true;
                    resultModel.DESCRIPTION = flag ?
                        MessageType.Audit_Success.GetDescription() :
                        MessageType.Reject_Success.GetDescription();
                    //try
                    //{
                    //    foreach (var items in FMLTs.GroupBy(x => new
                    //    {
                    //        x.send_style,
                    //        x.zip_code,
                    //        x.addr,
                    //        x.rcv_id
                    //    }))
                    //    {
                    //        var cId = sysSeqDao.qrySeqNo("AP", "S4", qPreCode).ToString();
                    //        var _id = $@"S4{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                    //        FMLs.Add(new FAP_MAIL_LABEL()
                    //        {
                    //            id = _id, //pkid
                    //            send_style = items.Key.send_style, //寄送方式
                    //            zip_code = items.Key.zip_code, //郵遞區號
                    //            addr = items.Key.addr, //地址
                    //            rcv_id = items.Key.rcv_id, //收件人員
                    //            memo = GetCheckmemo(items.Select(x => x.check_no)), //備註
                    //            number = items.Count(), //張數
                    //            apply_name = string.Join(",", items.Select(x => x.apply_name).Distinct().OrderBy(x => x)), //行政單位
                    //            apply_id = string.Join(",", items.Select(x => x.apply_id).Distinct().OrderBy(x => x)), //申請人員
                    //            rece_id = string.Join(",", items.Select(x => x.rece_id).Distinct().OrderBy(x => x)), //接收人員
                    //            create_datetime = dtn
                    //        });
                    //        foreach (var item in items)
                    //        {
                    //            FMLDs.Add(new FAP_MAIL_LABEL_D()
                    //            {
                    //                id = _id,  //pkid
                    //                apply_no = item.apply_no, //應付票據變更主檔申請單號碼
                    //                check_no = item.check_no //支票號碼
                    //            });
                    //        }
                    //    }
                    //    using (dbFGLEntities db = new dbFGLEntities())
                    //    {
                    //        db.FAP_MAIL_LABEL.AddRange(FMLs);
                    //        db.FAP_MAIL_LABEL_D.AddRange(FMLDs);
                    //        db.SaveChanges();
                    //    }
                    //    resultModel.RETURN_FLAG = true;
                    //    resultModel.DESCRIPTION = flag ?
                    //        MessageType.Audit_Success.GetDescription() :
                    //        MessageType.Reject_Success.GetDescription();
                    //}
                    //catch (Exception ex)
                    //{
                    //    transaction.Rollback();
                    //    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    //    resultModel.DESCRIPTION = flag ?
                    //        MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!") :
                    //        MessageType.REJ_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    //}
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                resultModel.DESCRIPTION = flag ? 
                    MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!"): 
                    MessageType.REJ_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
            }
            return resultModel;
        }

        /// <summary>
        /// 更新 應付票據變更申請明細檔 FAPPYCD0
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="userId"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <param name="updatedt"></param>
        /// <param name="updatetm"></param>
        private string updateFAPPYCD0(OAP0021DetailModel updateModel, string userId, EacConnection conn, EacTransaction transaction,string updatedt, string updatetm)
        {
            string msg = string.Empty;
            string sql = string.Empty;
            try
            {
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
delete LAPPYCD1
where APPLY_NO = :APPLY_NO
";
                    com.Transaction = transaction;
                    com.Parameters.Add($@"APPLY_NO", updateModel.apply_no); //申請單號
                    com.CommandText = sql;
                    com.Prepare();
                    var updateNum = com.ExecuteNonQuery();
                    com.Dispose();
                }
                if (updateModel.check_nos.Any())
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
INSERT INTO LAPPYCD1 (APPLY_NO, APPLY_SEQ, SYSTEM, BANK_CODE, CHECK_NO, UPD_ID, UPD_DATE, UPD_TIME, MARK_TYPE2, NEW_HEAD) 
VALUES 
";
                        var i = 0;
                        var c = string.Empty;
                        //申請單號 同申請主檔的申請單號
                        foreach (var item in updateModel.check_nos)
                        {
                            sql += $@" {c} ( :APPLY_NO_{i} , :APPLY_SEQ_{i} , :SYSTEM_{i} , :BANK_CODE_{i} , :CHECK_NO_{i} , :UPD_ID_{i} , :UPD_DATE_{i} , :UPD_TIME_{i} , :MARK_TYPE2_{i} , :NEW_HEAD_{i} ) ";
                            com.Parameters.Add($@"APPLY_NO_{i}", updateModel.apply_no.strto400DB()); //申請單號
                            com.Parameters.Add($@"APPLY_SEQ_{i}", i + 1); //申請序號 同一申請單號 流水號1,2,3…
                            com.Parameters.Add($@"SYSTEM_{i}", item.system); //A/F
                            com.Parameters.Add($@"BANK_CODE_{i}", item.bank_code.strto400DB()); //付款帳戶
                            com.Parameters.Add($@"CHECK_NO_{i}", item.check_no.strto400DB()); //支票號碼
                            com.Parameters.Add($@"UPD_ID_{i}", userId.strto400DB()); //異動人員
                            com.Parameters.Add($@"UPD_DATE_{i}", updatedt); //異動日期
                            com.Parameters.Add($@"UPD_TIME_{i}", updatetm); //異動時間
                            com.Parameters.Add($@"MARK_TYPE2_{i}", item.mark_type2.strto400DB()); //目前註記
                            com.Parameters.Add($@"NEW_HEAD_{i}", item.new_head.strto400DB()); //新支票抬頭
                            i += 1;
                            c = " , ";
                        }
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                msg = ex.ToString();
            }
            return msg;
        }

        /// <summary>
        /// 付款對象 調整 PS:(富小邦) = (  富小邦) = (富小邦  ) = (富  小  邦) 
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private string getReceiver(string receiver)
        {
            string result = string.Empty;

            if (receiver.IsNullOrWhiteSpace())
                return result;

            receiver = receiver.Trim();

            foreach (var item in receiver)
            {
                if (!item.ToString().IsNullOrWhiteSpace())
                    result += item;
            }

            return result;
        }

        /// <summary>
        /// 查詢業務人員資料 id , 姓名 , unit_id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Tuple<string, string, string> callEBXGXFK(string userId)
        {
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandText = "*PGM/EBXGXFK";
                    com.Parameters.Clear();

                    EacParameter P0RTN = new EacParameter(); //P0RTN 
                    P0RTN.ParameterName = "P0RTN";
                    P0RTN.DbType = DbType.String;
                    P0RTN.Size = 7;
                    P0RTN.Direction = ParameterDirection.Input;
                    P0RTN.Value = "";

                    EacParameter WP0001 = new EacParameter(); //業務員代號 
                    WP0001.ParameterName = "WP0001";
                    WP0001.DbType = DbType.String;
                    WP0001.Size = 10;
                    WP0001.Direction = ParameterDirection.Input;
                    WP0001.Value = userId;

                    EacParameter WP0002 = new EacParameter(); //身份證號碼
                    WP0002.ParameterName = "WP0002";
                    WP0002.DbType = DbType.String;
                    WP0002.Size = 10;
                    WP0002.Direction = ParameterDirection.Output;
                    WP0002.Value = "";

                    EacParameter WP0003 = new EacParameter(); //登錄證號
                    WP0003.ParameterName = "WP0003";
                    WP0003.DbType = DbType.String;
                    WP0003.Size = 10;
                    WP0003.Direction = ParameterDirection.Output;
                    WP0003.Value = "";

                    EacParameter WP0004 = new EacParameter(); //身分別  
                    WP0004.ParameterName = "WP0004";
                    WP0004.DbType = DbType.String;
                    WP0004.Size = 2;
                    WP0004.Direction = ParameterDirection.Output;
                    WP0004.Value = "";

                    EacParameter WP0005 = new EacParameter(); //單位代號
                    WP0005.ParameterName = "WP0005";
                    WP0005.DbType = DbType.String;
                    WP0005.Size = 5;
                    WP0005.Direction = ParameterDirection.Output;
                    WP0005.Value = "";

                    EacParameter WP0006 = new EacParameter(); //單位序號
                    WP0006.ParameterName = "WP0006";
                    WP0006.DbType = DbType.String;
                    WP0006.Size = 4;
                    WP0006.Direction = ParameterDirection.Output;
                    WP0006.Value = "";

                    EacParameter WP0007 = new EacParameter(); //單位 X(06) 
                    WP0007.ParameterName = "WP0007";
                    WP0007.DbType = DbType.String;
                    WP0007.Size = 6;
                    WP0007.Direction = ParameterDirection.Output;
                    WP0007.Value = "";

                    EacParameter WP0008 = new EacParameter(); //姓名
                    WP0008.ParameterName = "WP0008";
                    WP0008.DbType = DbType.String;
                    WP0008.Size = 14;
                    WP0008.Direction = ParameterDirection.Output;
                    WP0008.Value = "";

                    EacParameter WP0009 = new EacParameter(); //識別碼
                    WP0009.ParameterName = "WP0009";
                    WP0009.DbType = DbType.String;
                    WP0009.Size = 1;
                    WP0009.Direction = ParameterDirection.Output;
                    WP0009.Value = "";

                    EacParameter WP0010 = new EacParameter(); //RETURN CODE  
                    WP0010.ParameterName = "WP0010";
                    WP0010.DbType = DbType.String;
                    WP0010.Size = 1;
                    WP0010.Direction = ParameterDirection.Output;
                    WP0010.Value = "";

                    com.Parameters.Add(P0RTN);
                    com.Parameters.Add(WP0001);
                    com.Parameters.Add(WP0002);
                    com.Parameters.Add(WP0003);
                    com.Parameters.Add(WP0004);
                    com.Parameters.Add(WP0005);
                    com.Parameters.Add(WP0006);
                    com.Parameters.Add(WP0007);
                    com.Parameters.Add(WP0008);
                    com.Parameters.Add(WP0009);
                    com.Parameters.Add(WP0010);

                    com.Prepare();
                    try
                    {
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    com.Dispose();
                    return new Tuple<string, string, string>(WP0001.Value?.ToString()?.Trim(), WP0008.Value?.ToString()?.Trim(), WP0005.Value?.ToString()?.Trim());
                }
            }
        }

        private string setDefault(string value, string _default = null)
        {
            if (value.IsNullOrWhiteSpace())
                return _default ?? value;
            return value;
        }
    }
}