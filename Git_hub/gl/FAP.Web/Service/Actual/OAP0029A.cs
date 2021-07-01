using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Utilitys;
using FAP.Web.Daos;
using FAP.Web.Models;
using System.Web.Mvc;

/// <summary>
/// 功能說明：應付票據抽票結果確認功能
/// 初版作者：20200207 Mark
/// 修改歷程：20200207 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0029A : Common, IOAP0029A
    {
        /// <summary>
        /// 查詢 應付票據抽票結果確認功能
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<OAP0029ViewModel> GetSearchData(OAP0029SearchModel searchModel, string userId)
        {
            List<OAP0029ViewModel> resultModel = new List<OAP0029ViewModel>();
            List<OAP0021DetailSubModel> subdatas = new List<OAP0021DetailSubModel>();
            string sql = string.Empty;
            List<FAP_CE_APPLY_HIS> _FAP_CE_APPLY_HISs = new List<FAP_CE_APPLY_HIS>();
            List<SYS_CODE> _CE_RESULTs = new List<SYS_CODE>();
            List<string> _REJ_RSNs = new List<string>();
            var _last = string.Empty;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                 _FAP_CE_APPLY_HISs = db.FAP_CE_APPLY_HIS.AsNoTracking()
                    .Where(x => x.ce_result != null)
                    .Where(x => x.ce_result == searchModel.ce_result_status, 
                    !searchModel.ce_result_status.IsNullOrWhiteSpace() && searchModel.ce_result_status != "All") //結果(執行動作)查詢
                    .Where(x => x.apply_no == searchModel.apply_no, !searchModel.apply_no.IsNullOrWhiteSpace()) //申請單號查詢
                    .Where(x => x.update_id == searchModel.apply_id, !searchModel.apply_id.IsNullOrWhiteSpace()) //申請人查詢
                    .ToList();
                _REJ_RSNs = db.SYS_CODE.AsNoTracking().Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "REJ_RSN")
                    .OrderBy(x => x.ISORTBY).Select(x => x.CODE_VALUE).ToList();
                if (_REJ_RSNs.Any())
                    _last = _REJ_RSNs.Last();
                _CE_RESULTs = db.SYS_CODE.AsNoTracking().Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "CE_RESULT")
                    .OrderBy(x => x.ISORTBY).ToList();
            }

            if (!_FAP_CE_APPLY_HISs.Any())
                return resultModel;


            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select EH.CHECK_NO, EH.APPLY_NO, EH.REJ_RSN, EH.CE_RPLY_ID , EH.APLY_NO , EH.APLY_SEQ , CK.STATUS , EH.APPLY_ID  from FAPPYEH0 EH
join FGLGPCK0 CK
on EH.APLY_NO = CK.APLY_NO
and EH.APLY_SEQ = CK.APLY_SEQ
where EH.STATUS = '3' ";
                    if (searchModel.send_style != "All")
                    {
                        sql += $@"
and CK.SEND_STYLE = :SEND_STYLE  ";
                        com.Parameters.Add($@"SEND_STYLE", searchModel.send_style);

                    }
                       sql += @" and EH.APPLY_NO in ( ";

                    int i = 0;
                    string c = string.Empty;
                    foreach (var item in _FAP_CE_APPLY_HISs.Select(x=>x.apply_no))
                    {
                        sql += $@" {c} :APPLY_NO_{i} ";
                        com.Parameters.Add($@"APPLY_NO_{i}", item);
                        c = " , ";
                        i += 1;
                    }
                    sql += @" ) ";
                    if (!searchModel.ce_rply_dt.IsNullOrWhiteSpace()) //抽票回覆日期
                    {
                        var _ce_rply_dt = searchModel.ce_rply_dt?.DPformateTWdate() ?? string.Empty;
                        sql += $@" and  EH.CE_RPLY_DT = :CE_RPLY_DT ";
                        com.Parameters.Add($@"CE_RPLY_DT", _ce_rply_dt);
                    }
                    sql += @" ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();

                    while (dbresult.Read())
                    {
                        var model = new OAP0029ViewModel();
                        model.check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼(抽)
                        model.apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                        model.rej_rsn = dbresult["REJ_RSN"]?.ToString()?.Trim(); //退件原因
                        model.ce_rply_id = dbresult["CE_RPLY_ID"]?.ToString()?.Trim(); //抽票回覆人員
                        model.memo = model.rej_rsn;
                        model.apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                        resultModel.Add(model);
                        if (!model.check_no.IsNullOrWhiteSpace())
                            subdatas.Add(new OAP0021DetailSubModel() { check_no = model.check_no });
                    }
                    com.Dispose();
                }
                if (subdatas.Any())
                {
                    new OAP0021().getSubData(subdatas);
                }
                var _OAP0029 = new OAP0029();
                foreach (var item in resultModel)
                {
                    item.scre_from = _OAP0029.getScre_FormInApplyNo(item.apply_no);
                    var _subdatas = subdatas.FirstOrDefault(x => x.check_no == item.check_no);
                    item.amount = _subdatas?.amount; //支票面額
                    item.receiver = _subdatas?.receiver; //支票抬頭

                    var _F = _FAP_CE_APPLY_HISs.FirstOrDefault(x => x.apply_no == item.apply_no);
                    if (_F != null)
                    {
                        item.ce_result = _F.ce_result;
                        item.ce_result_status = _F.ce_result;
                        var _CE_RESULT = _CE_RESULTs.FirstOrDefault(x => x.CODE == _F.ce_result);
                        if (_CE_RESULT != null)
                            item.ce_result_status += $@" : {_CE_RESULT.CODE_VALUE}";
                    }
                    if (_F.ce_result == "Y")
                    {
                        item.memo = null;
                        item.rej_rsn = null;
                    }
                    else
                    {
                        if (!(_REJ_RSNs.Contains(item.rej_rsn) && (item.rej_rsn != _REJ_RSNs.Last())))
                        {
                            item.memo = item.rej_rsn;
                            item.rej_rsn = _last; //退件原因改為'其他'
                        }
                        else
                        {
                            item.memo = null; //退件原因不為'其他'時, 其他說明清空
                        }
                    }

                    //覆核權限 申請人不能為自己
                    var _review_flag = !(item.ce_rply_id == userId);
                    item.review_flag = _review_flag;
                    item.Ischecked = _review_flag;
                }
            }

            return resultModel.OrderByDescending(x => x.check_no).ThenBy(x => x.apply_no).ToList();
        }

        /// <summary>
        /// 覆核 選擇的案例
        /// </summary>
        /// <param name="apprDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel ApprovedData(IEnumerable<OAP0029ViewModel> apprDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            List<FAPPYCH0> insertDatas_FAPPYCH0 = new List<FAPPYCH0>();
            List<FAPPYSN0> insertDatas_FAPPYSN0 = new List<FAPPYSN0>();
            DateTime dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";

            if (checkData(apprDatas.Select(x => x.apply_no)))
            {
                resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
            }
            else
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _FAP_CE_APPLY_HIS = db.FAP_CE_APPLY_HIS.AsNoTracking().ToList();

                    var Data_Y = _FAP_CE_APPLY_HIS.Where(x => x.ce_result == "Y").Select(x => x.apply_no).ToList();
                    var Data_R = _FAP_CE_APPLY_HIS.Where(x => x.ce_result == "R").Select(x => x.apply_no).ToList();

                    //查詢抽票原因維護檔,抽票原因,指定部門為空白時須新增票據簽收檔
                    var _reason_code = db.FAP_CODE.AsNoTracking().AsEnumerable() 
                        .Where(x=>x.referral_dep.IsNullOrWhiteSpace()).Select(x=>x.reason_code).ToList();

                    var apprs = apprDatas.Where(x => Data_Y.Contains(x.apply_no)).ToList();
                    var rejs = apprDatas.Where(x => Data_R.Contains(x.apply_no)).ToList();
                    try
                    {
                        using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                        {
                            conn.Open();
                            EacTransaction transaction = conn.BeginTransaction();
                            string sql = string.Empty;
                            int i = 0;
                            string c = string.Empty;

                            #region 經辦選擇 核可案例
                            if (apprs.Any())
                            {
                                #region 新增 應付票據簽收檔 (抽票原因 => 查詢抽票原因維護,指定部門為空白的資料)
                                if (_reason_code.Any())
                                {
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"
                            select APPLY_NO , BANK_CODE , CHECK_NO , APPLY_ID , APPLY_UNIT  from FAPPYEH0
                            where CE_RSN in ( ";
                                        i = 0;
                                        c = string.Empty;
                                        foreach (var reason_code in _reason_code)
                                        {
                                            sql += $@" {c} :CE_RSN_{i} ";
                                            com.Parameters.Add($@"CE_RSN_{i}", reason_code);
                                            c = " , ";
                                            i += 1;
                                        }
                                        sql += @" ) and  APPLY_NO in (  ";
                                        i = 0;
                                        c = string.Empty;
                                        foreach (var appr in apprs)
                                        {
                                            sql += $@" {c} :APPLY_NO_{i} ";
                                            com.Parameters.Add($@"APPLY_NO_{i}", appr.apply_no);
                                            c = " , ";
                                            i += 1;
                                        }
                                        sql += @" ); ";
                                        com.CommandText = sql;
                                        com.Prepare();
                                        DbDataReader dbresult = com.ExecuteReader();
                                        while (dbresult.Read())
                                        {
                                            var model = new FAPPYSN0();
                                            model.APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                                            model.ACCT_ABBR = dbresult["BANK_CODE"]?.ToString()?.Trim(); //帳戶簡稱 
                                            model.CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                                            model.APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                                            model.APPLY_UNIT = dbresult["APPLY_UNIT"]?.ToString()?.Trim(); //申請人員部門
                                            insertDatas_FAPPYSN0.Add(model);
                                        }
                                        com.Dispose();
                                    }

                                    if (insertDatas_FAPPYSN0.Any())
                                    {
                                        using (EacCommand com = new EacCommand(conn))
                                        {
                                            sql = $@"
                            insert into LAPPYSN1 
                            (APPLY_NO, ACCT_ABBR , CHECK_NO, APPLY_ID, UNIT_CODE , ENTRY_ID, ENTRY_DATE, ENTRY_TIME, FLAG, SRCE_FROM )
                            VALUES  ";
                                            i = 0;
                                            c = string.Empty;
                                            foreach (var item in insertDatas_FAPPYSN0)
                                            {
                                                sql += $@" {c} ( :APPLY_NO_{i} , :ACCT_ABBR_{i} , :CHECK_NO_{i} , :APPLY_ID_{i} , :UNIT_CODE_{i} , :ENTRY_ID_{i} , :ENTRY_DATE_{i} , :ENTRY_TIME_{i} , :FLAG_{i} , :SRCE_FROM_{i})  ";
                                                com.Parameters.Add($@"APPLY_NO_{i}", item.APPLY_NO.strto400DB()); //申請單號
                                                com.Parameters.Add($@"ACCT_ABBR_{i}", item.ACCT_ABBR.strto400DB()); //帳戶簡稱
                                                com.Parameters.Add($@"CHECK_NO_{i}", item.CHECK_NO.strto400DB()); //支票號碼  
                                                com.Parameters.Add($@"APPLY_ID_{i}", item.APPLY_ID.strto400DB()); //申請人員
                                                com.Parameters.Add($@"UNIT_CODE_{i}", item.APPLY_UNIT.strto400DB()); //申請人員部門
                                                com.Parameters.Add($@"ENTRY_ID_{i}", userId.strto400DB()); //輸入人員
                                                com.Parameters.Add($@"ENTRY_DATE_{i}", updatedt.strto400DB()); //輸入日期
                                                com.Parameters.Add($@"ENTRY_TIME_{i}", updatetm.strto400DB()); //輸入時間
                                                com.Parameters.Add($@"FLAG_{i}", "N"); //簽收否 
                                                com.Parameters.Add($@"SRCE_FROM_{i}", "1"); //資料來源 1=>抽票
                                                c = " , ";
                                                i += 1;
                                            }
                                            com.Transaction = transaction;
                                            com.CommandText = sql;
                                            com.Prepare();
                                            var updateNum = com.ExecuteNonQuery();
                                            com.Dispose();
                                        }
                                    }
                                }

                                #endregion

                                #region 新增 應付票據變更申請主檔&明細檔 (抽票原因 => B)

                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
select APPLY_NO, APPLY_ID, APPLY_UNIT, APPLY_DATE, APPLY_TIME, APPR_ID, APPR_DATE ,APPR_TIME, MARK_TYPE2, SYSTEM ,BANK_CODE ,CHECK_NO ,ZIP_CODE ,ADDR ,RCV_ID ,SEND_STYLE  from LAPPYEH1
where CE_RSN = 'B'
and   APPLY_NO in (
";
                                    i = 0;
                                    c = string.Empty;
                                    foreach (var appr in apprs)
                                    {
                                        sql += $@" {c} :APPLY_NO_{i} ";
                                        com.Parameters.Add($@"APPLY_NO_{i}", appr.apply_no);
                                        c = ",";
                                        i += 1;
                                    }
                                    sql += @" ); ";
                                    com.CommandText = sql;
                                    com.Prepare();
                                    DbDataReader dbresult = com.ExecuteReader();
                                    while (dbresult.Read())
                                    {
                                        var model = new FAPPYCH0();
                                        model.APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                                        model.APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                                        model.APPLY_UNIT = dbresult["APPLY_UNIT"]?.ToString()?.Trim(); //申請人員部門
                                        model.APPLY_DATE = dbresult["APPLY_DATE"]?.ToString()?.Trim(); //申請日期
                                        model.APPLY_TIME = dbresult["APPLY_TIME"]?.ToString()?.Trim(); //申請時間
                                        model.APPR_ID = dbresult["APPR_ID"]?.ToString()?.Trim(); //覆核人員
                                        model.APPR_DATE = dbresult["APPR_DATE"]?.ToString()?.Trim(); //覆核日期
                                        model.APPR_TIME = dbresult["APPR_TIME"]?.ToString()?.Trim(); //覆核時間
                                        model.MARK_TYP2 = dbresult["MARK_TYPE2"]?.ToString()?.Trim(); //目前註記
                                        model.SYSTEM = dbresult["SYSTEM"]?.ToString()?.Trim(); //系統別
                                        model.BANK_CODE = dbresult["BANK_CODE"]?.ToString()?.Trim(); //付款帳戶
                                        model.CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                                        model.ZIP_CODE = dbresult["ZIP_CODE"]?.ToString()?.Trim(); //郵遞區號
                                        model.ADDR = dbresult["ADDR"]?.ToString()?.Trim(); //地址
                                        model.RCV_NAME = dbresult["RCV_ID"]?.ToString()?.Trim(); //收件人
                                        model.SEND_STYLE = dbresult["SEND_STYLE"]?.ToString()?.Trim(); //寄送方式
                                        insertDatas_FAPPYCH0.Add(model);
                                    }
                                    com.Dispose();
                                }
                                if (insertDatas_FAPPYCH0.Any())
                                {
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"insert into LAPPYCH1 
(APPLY_NO, STATUS, APPLY_ID, APPLY_UNIT, APPLY_DATE, APPLY_TIME, APPR_ID1, APPR_DATE1, APPR_TIME1, ZIP_CODE, ADDR, RCV_NAME, REG_YN)
VALUES  ";
                                        i = 0;
                                        c = string.Empty;
                                        foreach (var item in insertDatas_FAPPYCH0)
                                        {
                                            sql += $@" {c} ( :APPLY_NO_{i} , :STATUS_{i} , :APPLY_ID_{i} , :APPLY_UNIT_{i} , :APPLY_DATE_{i} , :APPLY_TIME_{i} , :APPR_ID1_{i} , :APPR_DATE1_{i} , :APPR_TIME1_{i} , :ZIP_CODE_{i} , :ADDR_{i} , :RCV_NAME_{i} , :REG_YN_{i})  ";
                                            com.Parameters.Add($@"APPLY_NO_{i}", item.APPLY_NO.strto400DB()); //申請單號
                                            com.Parameters.Add($@"STATUS_{i}", "2"); //狀態 2:完成申請
                                            com.Parameters.Add($@"APPLY_ID_{i}", item.APPLY_ID.strto400DB()); //申請人員
                                            com.Parameters.Add($@"APPLY_UNIT_{i}", item.APPLY_UNIT.strto400DB()); //申請人員部門
                                            com.Parameters.Add($@"APPLY_DATE_{i}", item.APPLY_DATE.strto400DB()); //申請日期
                                            com.Parameters.Add($@"APPLY_TIME_{i}", item.APPLY_TIME.strto400DB()); //申請時間
                                            com.Parameters.Add($@"APPR_ID1_{i}", item.APPR_ID.strto400DB()); //覆核人員
                                            com.Parameters.Add($@"APPR_DATE1_{i}", item.APPR_DATE.strto400DB()); //覆核日期
                                            com.Parameters.Add($@"APPR_TIME1_{i}", item.APPR_TIME.strto400DB()); //覆核時間     
                                            com.Parameters.Add($@"ZIP_CODE_{i}", item.ZIP_CODE.strto400DB()); //郵遞區號   
                                            com.Parameters.Add($@"ADDR_{i}", item.ADDR.strto400DB()); //地址
                                            com.Parameters.Add($@"RCV_NAME_{i}", item.RCV_NAME.strto400DB()); //收件人   
                                            com.Parameters.Add($@"REG_YN_{i}", item.SEND_STYLE.strto400DB()); //雙掛號回執 (寄送方式)    
                                            c = ",";
                                            i += 1;
                                        }
                                        com.Transaction = transaction;
                                        com.CommandText = sql;
                                        com.Prepare();
                                        var updateNum = com.ExecuteNonQuery();
                                        com.Dispose();
                                    }
                                    using (EacCommand com = new EacCommand(conn))
                                    {
                                        sql = $@"
                            insert into LAPPYCD1 
                            (APPLY_NO, APPLY_SEQ, SYSTEM, BANK_CODE, CHECK_NO, UPD_ID, UPD_DATE, UPD_TIME, MARK_TYPE2)
                            VALUES ";
                                        i = 0;
                                        c = string.Empty;
                                        foreach (var item in insertDatas_FAPPYCH0)
                                        {
                                            sql += $@" {c} ( :APPLY_NO_{i} , :APPLY_SEQ_{i} , :SYSTEM_{i} , :BANK_CODE_{i} , :CHECK_NO_{i} , :UPD_ID_{i} , :UPD_DATE_{i} , :UPD_TIMEN_{i} , :MARK_TYPE2_{i} )  ";
                                            com.Parameters.Add($@"APPLY_NO_{i}", item.APPLY_NO.strto400DB()); //申請單號
                                            com.Parameters.Add($@"APPLY_SEQ_{i}", "1"); //申請序號
                                            com.Parameters.Add($@"SYSTEM_{i}", item.SYSTEM.strto400DB()); //系統別
                                            com.Parameters.Add($@"BANK_CODE_{i}", item.BANK_CODE.strto400DB()); //付款帳戶
                                            com.Parameters.Add($@"CHECK_NO_{i}", item.CHECK_NO.strto400DB()); //支票號碼
                                            com.Parameters.Add($@"UPD_ID_{i}", userId.strto400DB()); //異動人員
                                            com.Parameters.Add($@"UPD_DATE_{i}", updatedt.strto400DB()); //異動日期
                                            com.Parameters.Add($@"UPD_TIME_{i}", updatetm.strto400DB()); //異動時間
                                            com.Parameters.Add($@"MARK_TYPE2_{i}", item.MARK_TYP2.strto400DB()); //目前註記
                                            c = ",";
                                            i += 1;
                                        }
                                        com.Transaction = transaction;
                                        com.CommandText = sql;
                                        com.Prepare();
                                        var updateNum = com.ExecuteNonQuery();
                                        com.Dispose();
                                    }
                                }
                                #endregion

                                #region 修改 應付票據抽票申請主檔
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
                        update LAPPYEH1
                        set STATUS = '4',
                            CE_APPR_ID = :CE_APPR_ID,
                            CE_APPR_DT = :CE_APPR_DT,
                            CE_APPR_TM = :CE_APPR_TM,
                            CE_RESULT = 'Y',
                            REJ_RSN = ''
                        where APPLY_NO in ( ";
                                    i = 0;
                                    c = string.Empty;
                                    com.Parameters.Add($@"CE_APPR_ID", userId);
                                    com.Parameters.Add($@"CE_APPR_DT", updatedt);
                                    com.Parameters.Add($@"CE_APPR_TM", updatetm);
                                    foreach (var appr in apprs)
                                    {
                                        sql += $@" {c} :APPLY_NO_{i} ";
                                        com.Parameters.Add($@"APPLY_NO_{i}", appr.apply_no);
                                        c = ",";
                                        i += 1;
                                    }
                                    sql += @" ); ";
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                                #endregion
                            }
                            #endregion

                            #region 經辦選擇 駁回案例
                            if (rejs.Any())
                            {
                                #region 修改 應付票據抽票申請主檔
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql  = $@"
                                update LAPPYEH1
                                set STATUS = '5',
                                    CE_APPR_ID = :CE_APPR_ID,
                                    CE_APPR_DT = :CE_APPR_DT,
                                    CE_APPR_TM = :CE_APPR_TM,
                                    CE_RESULT = 'R' 
                                where APPLY_NO in ( ";
                                    com.Parameters.Add($@"CE_APPR_ID", userId);
                                    com.Parameters.Add($@"CE_APPR_DT", updatedt);
                                    com.Parameters.Add($@"CE_APPR_TM", updatetm);
                                    i = 0;
                                    c = string.Empty;
                                    foreach (var rej in rejs)
                                    {
                                        sql += $@" {c} :APPLY_NO_{i} ";
                                        com.Parameters.Add($@"APPLY_NO_{i}", rej.apply_no);
                                        i += 1;
                                        c = ",";
                                    }
                                    sql += @" ); ";
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                                #endregion
                            }
                            #endregion

                            bool rollbackFlag = false;
                            try
                            {
                                List<string> applynos = new List<string>();
                                applynos.AddRange(apprs.Select(x => x.apply_no));
                                applynos.AddRange(rejs.Select(x => x.apply_no));
                                db.FAP_CE_APPLY_HIS.RemoveRange(db.FAP_CE_APPLY_HIS.Where(x => applynos.Contains(x.apply_no)));
                                transaction.Commit();
                                rollbackFlag = true;
                                db.SaveChanges();
                                resultModel.RETURN_FLAG = true;
                                resultModel.DESCRIPTION = MessageType.Audit_Success.GetDescription();
                                if (insertDatas_FAPPYCH0.Any())
                                {
                                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                                    piaLogMain.TRACKING_TYPE = "A";
                                    piaLogMain.ACCESS_ACCOUNT = userId;
                                    piaLogMain.ACCOUNT_NAME = "";
                                    piaLogMain.PROGFUN_NAME = "OAP0029A";
                                    piaLogMain.EXECUTION_CONTENT = $@"apply_no:{string.Join(",",insertDatas_FAPPYCH0.Select(x=>x.APPLY_NO))}";
                                    piaLogMain.AFFECT_ROWS = insertDatas_FAPPYCH0.Count;
                                    piaLogMain.PIA_TYPE = "0100000000";
                                    piaLogMain.EXECUTION_TYPE = "A";
                                    piaLogMain.ACCESSOBJ_NAME = "FAPPYEH";
                                    piaLogMainDao.Insert(piaLogMain);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (rollbackFlag)
                                    transaction.Rollback();
                                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                                resultModel.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex);
                        resultModel.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
            }

            return resultModel;
        }

        /// <summary>
        /// 駁回 選擇的案例
        /// </summary>
        /// <param name="rejDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel RejectedData(IEnumerable<OAP0029ViewModel> rejDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            var appnos = rejDatas.Select(x => x.apply_no).ToList();
            if (checkData(appnos))
            {
                resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
            }
            else
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    string sql = string.Empty;
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        EacTransaction transaction = conn.BeginTransaction();
                        int i = 0;
                        string c = string.Empty;
                        using (EacCommand com = new EacCommand(conn))
                        {
                            sql = $@"
                        update LAPPYEH1
                        set CE_RPLY_ID = '',
                            CE_RPLY_DT = 0 ,
                            CE_RPLY_TM = 0
                        where APPLY_NO in ( ";
                            foreach (var app in appnos)
                            {
                                sql += $@" {c} :APPLY_NO_{i} ";
                                com.Parameters.Add($@"APPLY_NO_{i}", app);
                                c = " , ";
                                i += 1;
                            }
                            sql += @" ) ;";
                            com.Transaction = transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            com.Dispose();
                        }
                        bool rollbackFlag = false;
                        try
                        {
                            foreach (var item in db.FAP_CE_APPLY_HIS.Where(x => appnos.Contains(x.apply_no)))
                            {
                                item.ce_result = null;
                                item.update_id = userId;
                                item.update_datetime = dtn;
                            }
                            transaction.Commit();
                            rollbackFlag = true;
                            db.SaveChanges();
                            resultModel.RETURN_FLAG = true;
                            resultModel.DESCRIPTION = MessageType.Reject_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            if (rollbackFlag)
                                transaction.Rollback();
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                            resultModel.DESCRIPTION = MessageType.Reject_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }
                }
            }

            return resultModel;
        }

        /// <summary>
        /// 檢核 抽票覆核是否資料異動
        /// </summary>
        /// <param name="applyNos"></param>
        /// <returns>true為有異動</returns>
        private bool checkData(IEnumerable<string> applyNos)
        {
            bool flag = false;
            string sql = string.Empty;
            List<string> udpates = new List<string>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select APPLY_NO, CE_RPLY_ID, CE_APPR_ID  from FAPPYEH0
where  STATUS = '3' 
and  APPLY_NO in ( 
";
                    int i = 0;
                    string c = string.Empty;
                    foreach (var item in applyNos)
                    {
                        sql += $@" {c} :APPLY_NO_{i} ";
                        com.Parameters.Add($@"APPLY_NO_{i}", item);
                        c = " , ";
                        i += 1;
                    }
                    sql += @" ) ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        var _APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                        var _CE_RPLY_ID = dbresult["CE_RPLY_ID"]?.ToString()?.Trim(); //抽票回覆人員
                        var _CE_APPR_ID = dbresult["CE_APPR_ID"]?.ToString()?.Trim(); //抽票覆核人員

                        if (_CE_RPLY_ID.IsNullOrWhiteSpace() || !_CE_APPR_ID.IsNullOrWhiteSpace())
                            udpates.Add(_APPLY_NO);
                    }
                }
            }
            return udpates.Any();
        }

        /// <summary>
        /// 應付票據變更主檔
        /// </summary>
        private class FAPPYCH0
        {
            /// <summary>
            /// 申請單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 申請人員
            /// </summary>
            public string APPLY_ID { get; set; }

            /// <summary>
            /// 申請人員部門
            /// </summary>
            public string APPLY_UNIT { get; set; }

            /// <summary>
            /// 申請日期
            /// </summary>
            public string APPLY_DATE { get; set; }

            /// <summary>
            /// 申請時間
            /// </summary>
            public string APPLY_TIME { get; set; }

            /// <summary>
            /// 覆核人員
            /// </summary>
            public string APPR_ID { get; set; }

            /// <summary>
            /// 覆核日期
            /// </summary>
            public string APPR_DATE { get; set; }

            /// <summary>
            /// 覆核時間
            /// </summary>
            public string APPR_TIME { get; set; }

            /// <summary>
            /// 郵遞區號
            /// </summary>
            public string ZIP_CODE { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            public string ADDR { get; set; }

            /// <summary>
            /// 收件人
            /// </summary>
            public string RCV_NAME { get; set; }

            /// <summary>
            /// 目前註記
            /// </summary>
            public string MARK_TYP2 { get; set; }

            /// <summary>
            /// 系統別
            /// </summary>
            public string SYSTEM { get; set; }

            /// <summary>
            /// 付款帳戶
            /// </summary>
            public string BANK_CODE { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string CHECK_NO { get; set; }

            /// <summary>
            /// 寄送方式
            /// </summary>
            public string SEND_STYLE { get; set; }
        }

        /// <summary>
        /// 應付票據簽收檔
        /// </summary>
        private class FAPPYSN0
        {
            /// <summary>
            /// 申請單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 帳戶簡稱
            /// </summary>
            public string ACCT_ABBR { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string CHECK_NO { get; set; }

            /// <summary>
            /// 申請人員
            /// </summary>
            public string APPLY_ID { get; set; }

            /// <summary>
            /// 申請人員部門
            /// </summary>
            public string APPLY_UNIT { get; set; }
         }
    }
}