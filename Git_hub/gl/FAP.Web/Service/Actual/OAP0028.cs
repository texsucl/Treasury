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
/// 功能說明：應付票據抽票明細表（FOR財務部抽票使用）
/// 初版作者：20200207 Mark
/// 修改歷程：20200207 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FAP.Web.Service.Actual
{
    public class OAP0028 : Common, IOAP0028
    {
        /// <summary>
        /// 執行明細表產出
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <param name="entry_date">登打日期(不輸入預設為系統日)</param>
        /// <returns></returns>
        public Tuple<bool,string> changStatus( string userId, string entry_date = null)
        {
            bool flag = false;
            string msg = string.Empty;
            string sql = string.Empty;
            List<FAPPYEH0> _FAPPYEH0s = new List<FAPPYEH0>(); //應付票據抽票申請檔
            List<FAPPYEH0> _FAPPYEH0s_insert = new List<FAPPYEH0>(); //須新增的應付票據抽票申請檔
            List<tempData> searchtemps = new List<tempData>(); //無支票號碼的資料 去應付票據檔查詢支票號碼 & 付款帳戶 暫存檔
            List<compareData> compareDatas = new List<compareData>(); //判斷是否有重複資料的 暫存檔
            List<string> _checknos = new List<string>(); //支票號碼
            List<string> _aplynos = new List<string>(); //付款申請編號
            List<string> _aplyseqs = new List<string>(); //付款申請序號
            //List<string> _samechecknos = new List<string>(); //相同的支票號碼
            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";

            if (entry_date.IsNullOrWhiteSpace())
                entry_date = updatedt;
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();

                    #region 查詢符合的系統類資料 
                    using (EacCommand com = new EacCommand(conn))
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("查詢符合的系統類資料 Start!");

                        //系統類 依當日系統之開票件，找出處理註記為NYNN、YYNN、NYNY(取消禁背)；NNYN、YNYN、NNYY (取消劃線)資料
                        sql = $@"select CHECK_NO , KEYIN_ID , ADM_UNIT , RECEIVER ,  ZIP_CODE , ADDR , PROC_MARK , SEND_STYLE, SYS_TYPE , APLY_NO , APLY_SEQ , BANK_CODE  from FGLGPCK0
where PROC_MARK in ('NYNN','YYNN','NYNY','NNYN','YNYN','NNYY')
and ENTRY_DATE = :ENTRY_DATE        
";
                        com.Parameters.Add($@"ENTRY_DATE", entry_date); //登打日期
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();

                        List<string> _PROC_MARK1 = new List<string>() { "NNYN", "YNYN", "NNYY" }; //(取消劃線)
                        List<string> _PROC_MARK2 = new List<string>() { "NYNN", "YYNN", "NYNY" }; //(取消禁背)

                        while (dbresult.Read())
                        {
                            FAPPYEH0 model = new FAPPYEH0();
                            model.CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                            model.STATUS = "3"; //3:受理中
                            model.CHECK_WT = "Y"; //支票開立否
                            model.APPLY_ID = dbresult["KEYIN_ID"]?.ToString()?.Trim(); //送件人員
                            model.APPLY_UNIT = dbresult["ADM_UNIT"]?.ToString()?.Trim(); //行政單位
                            model.CE_RSN = "B"; //抽件原因
                            //model.RCV_UNIT = ""; //收件單位
                            model.RCV_ID = dbresult["RECEIVER"]?.ToString()?.Trim(); //收件人員
                            model.ZIP_CODE = dbresult["ZIP_CODE"]?.ToString()?.Trim(); //郵遞區號
                            model.ADDR = dbresult["ADDR"]?.ToString()?.Trim(); //地址
                            var _PROC_MARK = dbresult["PROC_MARK"]?.ToString()?.Trim(); //處理註記
                            model.MARK_TYPE2 = _PROC_MARK1.Contains(_PROC_MARK) ? "1" : (_PROC_MARK2.Contains(_PROC_MARK) ? "2" : string.Empty);
                            model.SEND_STYLE = dbresult["SEND_STYLE"]?.ToString()?.Trim(); //寄送方式 
                            model.SYSTEM = dbresult["SYS_TYPE"]?.ToString()?.Trim(); //系統來源 
                            model.APLY_NO = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號 
                            model.APLY_SEQ = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請編號 
                            model.BANK_CODE = dbresult["BANK_CODE"]?.ToString()?.Trim(); //付款帳戶 
                            _FAPPYEH0s.Add(model);
                        }
                        com.Dispose();

                        NLog.LogManager.GetCurrentClassLogger().Info("查詢符合的系統類資料 End!");
                    }
                    #endregion

                    #region 比對申請類 不須重複新增的 付款申請編號 & 付款申請序號 (條件加入 抽票原因為B類)
                    if (_FAPPYEH0s.Any())
                    {
                        using (EacCommand com = new EacCommand(conn))
                        {
                            NLog.LogManager.GetCurrentClassLogger().Info("比對申請類 不須重複新增的 付款申請編號 & 付款申請序號 (條件加入 抽票原因為B類) Start!");

                            sql = $@"select APLY_NO , APLY_SEQ from FAPPYEH0
where  STATUS in ('2','3','4') 
and CE_RSN = 'B' 
and aply_no in ( ";
                            int i = 0;
                            string c = string.Empty;
                            foreach (var item in _FAPPYEH0s.Select(x => x.APLY_NO).Distinct())
                            {
                                sql += $@" {c} :aply_no_{i} ";
                                com.Parameters.Add($@"aply_no_{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += @" ) and aply_seq in ( ";
                            i = 0;
                            c = string.Empty;
                            foreach (var item in _FAPPYEH0s.Select(x => x.APLY_SEQ).Distinct())
                            {
                                sql += $@" {c} :aply_seq_{i} ";
                                com.Parameters.Add($@"aply_seq_{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += @" )  ;";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                compareData model = new compareData();
                                model.aply_no = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                                model.aply_seq = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                                compareDatas.Add(model);
                            }
                            com.Dispose();

                            NLog.LogManager.GetCurrentClassLogger().Info("比對申請類 不須重複新增的 付款申請編號 & 付款申請序號 (條件加入 抽票原因為B類) End!");
                        }
                    }
                    #endregion

                    #region 排除相同的 付款申請編號 & 付款申請序號 不同則新增 , 檢核支票號碼(空值要取值)
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                    String qPreCode = curDateTime[0];
                    foreach (var item in _FAPPYEH0s)
                    {
                        //查詢是否有相同的 付款申請編號 & 付款申請序號
                        var _sameflag = compareDatas.FirstOrDefault(x => x.aply_no == item.APLY_NO && x.aply_seq == item.APLY_SEQ);
                        if (_sameflag == null) //不符合則新增
                        {
                            var cId = sysSeqDao.qrySeqNo("AP", "CS", qPreCode).ToString();
                            item.APPLY_NO = $@"CS{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                            if (item.CHECK_NO.IsNullOrWhiteSpace())
                            {
                                searchtemps.Add(new tempData()
                                {
                                    apply_no = item.APPLY_NO,
                                    aply_no = item.APLY_NO,
                                    aply_seq = item.APLY_SEQ
                                });
                            }
                            _FAPPYEH0s_insert.Add(item);
                        }
                    }
                    if (_FAPPYEH0s_insert.Any())
                    {
                        var userMemo = GetMemoByUserId(_FAPPYEH0s_insert.Select(x => x.APPLY_ID).Distinct());
                        foreach (var item in _FAPPYEH0s_insert)
                        {
                            var _userMemo = userMemo.FirstOrDefault(x => x.Item1 == item.APPLY_ID);
                            if (_userMemo != null)
                                item.RCV_UNIT = _userMemo.Item3; //放apply_id 的部門ID 2020/05/20 子嫺通知
                        }
                    }
                    #endregion

                    #region 查詢 應付票據抽票主檔狀態為2:完成申請 & 3:受理中,所有沒有支票號碼的資料 檢核支票號碼(空值要取值) 使用 付款申請編號 & 付款申請序號 去應付票據檔(LAPPYCK92 & LFAPYCKK7) 查詢支票號碼 & 付款帳戶
                    using (EacCommand com = new EacCommand(conn))
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("查詢 應付票據抽票主檔狀態為2:完成申請 & 3:受理中,所有沒有支票號碼的資料 檢核支票號碼(空值要取值) Start!");
                        sql = $@"
select 
EH1.APPLY_NO, 
EH1.APLY_NO, 
EH1.APLY_SEQ ,
CASE WHEN EH1.SYSTEM = 'A'
           THEN CKK7.CHECK_NO
           WHEN EH1.SYSTEM = 'F'
           THEN CK92.CHECK_NO
END    AS CHECK_NO,
CASE WHEN EH1.SYSTEM = 'A'
           THEN CKK7.ACCT_ABBR
           WHEN EH1.SYSTEM = 'F'
           THEN CK92.BANK_CODE
END    AS BANK_CODE
from LAPPYEH1 EH1
left join LAPPYCK92 CK92
on EH1.SYSTEM = 'F'
and EH1.APLY_NO = CK92.APLY_NO
and EH1.APLY_SEQ = CK92.APLY_SEQ
left join LFAPYCKK7 CKK7
on EH1.SYSTEM = 'A'
and EH1.APLY_NO = CKK7.APLY_NO
and EH1.APLY_SEQ = CKK7.APLY_SEQ
where EH1.STATUS in ('2','3')
and EH1.CHECK_NO = '' 
";


//                        sql = $@"
//select APPLY_NO, APLY_NO, APLY_SEQ  from LAPPYEH1
//where STATUS in ('2','3')
//and CHECK_NO = '' ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            searchtemps.Add(new tempData()
                            {
                                apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(),
                                aply_no = dbresult["APLY_NO"]?.ToString()?.Trim(),
                                aply_seq = dbresult["APLY_SEQ"]?.ToString()?.Trim(),
                                check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(),
                                bank_code = dbresult["BANK_CODE"]?.ToString()?.Trim()
                            });
                        }
                        com.Dispose();
                        NLog.LogManager.GetCurrentClassLogger().Info("查詢 應付票據抽票主檔狀態為2:完成申請 & 3:受理中,所有沒有支票號碼的資料 檢核支票號碼(空值要取值) End!");
                    }
                    #endregion

                    #region 處理 撈取應付票據檔 暫存資料 (本次新增系統類)
                    foreach (var temp in searchtemps)
                    {
                        //系統類
                        var _FAPPYEH0s_insert_f = _FAPPYEH0s_insert
                            .FirstOrDefault(x => x.APLY_NO == temp.aply_no && x.APLY_SEQ == temp.aply_seq);
                        if (_FAPPYEH0s_insert_f != null)
                        {
                            _FAPPYEH0s_insert_f.CHECK_NO = temp.check_no;
                            _FAPPYEH0s_insert_f.BANK_CODE = temp.bank_code;
                            temp.alreadyUpdate = true; //系統類修改
                        }
                    }
                    #endregion

                    #region 新增 應付票據抽票

                    if (_FAPPYEH0s_insert.Any())
                    {
                        #region 新增 應付票據抽票申請主檔

                        using (EacCommand com = new EacCommand(conn))
                        {
                            NLog.LogManager.GetCurrentClassLogger().Info("新增 應付票據抽票申請主檔 Start!");
                            sql = $@"
INSERT INTO LAPPYEH1 (APPLY_NO, STATUS, CHECK_WT, APPLY_ID, APPLY_UNIT, APPLY_DATE, APPLY_TIME, CE_RSN, RCV_UNIT, RCV_ID, ";
                            sql += $@" ZIP_CODE, ADDR, MARK_TYPE2, SEND_STYLE, SYSTEM, APLY_NO, APLY_SEQ, BANK_CODE, CHECK_NO, PRINT_ID, PRINT_DATE, PRINT_TIME ) 
VALUES 
";
                            var i = 0;
                            var c = string.Empty;
                            foreach (var item in _FAPPYEH0s_insert)
                            {
                                sql += $@" {c} ( :APPLY_NO_{i} , :STATUS_{i} , :CHECK_WT_{i} , :APPLY_ID_{i} , :APPLY_UNIT_{i} , :APPLY_DATE_{i} , :APPLY_TIME_{i} , :CE_RSN_{i} , :RCV_UNIT_{i} , :RCV_ID_{i} , :ZIP_CODE_{i} , ";
                                sql += $@" :ADDR_{i} , :MARK_TYPE2_{i} , :SEND_STYLE_{i} , :SYSTEM_{i} , :APLY_NO_{i} , :APLY_SEQ_{i} , :BANK_CODE_{i} , :CHECK_NO_{i} , :PRINT_ID_{i}, :PRINT_DATE_{i}, :PRINT_TIME_{i} ) ";
                                com.Parameters.Add($@"APPLY_NO_{i}", item.APPLY_NO.strto400DB()); //申請單號
                                com.Parameters.Add($@"STATUS_{i}", item.STATUS.strto400DB()); //狀態
                                com.Parameters.Add($@"CHECK_WT_{i}", item.CHECK_WT.strto400DB()); //支票開立否 
                                com.Parameters.Add($@"APPLY_ID_{i}", item.APPLY_ID.strto400DB()); //申請人員
                                com.Parameters.Add($@"APPLY_UNIT_{i}", item.APPLY_UNIT.strto400DB()); //申請人員部門
                                com.Parameters.Add($@"APPLY_DATE_{i}", updatedt); //申請日期
                                com.Parameters.Add($@"APPLY_TIME_{i}", updatetm); //申請時間
                                com.Parameters.Add($@"CE_RSN_{i}", "B"); //抽票原因
                                com.Parameters.Add($@"RCV_UNIT_{i}", item.RCV_UNIT.strto400DB()); //收件單位
                                com.Parameters.Add($@"RCV_ID_{i}", item.RCV_ID.strto400DB()); //收件人員
                                com.Parameters.Add($@"ZIP_CODE_{i}", item.ZIP_CODE.strto400DB()); //郵遞區號
                                com.Parameters.Add($@"ADDR_{i}", item.ADDR.strto400DB()); //地址
                                com.Parameters.Add($@"MARK_TYPE2_{i}", item.MARK_TYPE2.strto400DB()); //目前註記
                                com.Parameters.Add($@"SEND_STYLE_{i}", item.SEND_STYLE.strto400DB()); //寄送方式
                                com.Parameters.Add($@"SYSTEM_{i}", item.SYSTEM.strto400DB()); //系統別
                                com.Parameters.Add($@"APLY_NO_{i}", item.APLY_NO.strto400DB()); //付款申請編號
                                com.Parameters.Add($@"APLY_SEQ_{i}", item.APLY_SEQ.strto400DB()); //付款申請序號
                                com.Parameters.Add($@"BANK_CODE_{i}", item.BANK_CODE.strto400DB()); //付款帳戶
                                com.Parameters.Add($@"CHECK_NO_{i}", item.CHECK_NO.strto400DB()); //支票號碼
                                com.Parameters.Add($@"PRINT_ID_{i}", userId); //列印人員
                                com.Parameters.Add($@"PRINT_DATE_{i}", updatedt); //列印日期
                                com.Parameters.Add($@"PRINT_TIME_{i}", updatetm); //列印時間
                                i += 1;
                                c = " , ";
                            }
                            com.Transaction = transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            com.Dispose();
                            NLog.LogManager.GetCurrentClassLogger().Info("新增 應付票據抽票申請主檔 End!");
                        }
                        #endregion

                        #region 新增 應付票據抽票申請明細檔
                        using (EacCommand com = new EacCommand(conn))
                        {
                            NLog.LogManager.GetCurrentClassLogger().Info("新增 應付票據抽票申請明細檔 Start!");
                            sql = $@"
INSERT INTO LAPPYED1 (APPLY_NO, APPLY_SEQ, SYSTEM, BANK_CODE, CHECK_NO, UPD_ID, UPD_DATE, UPD_TIME ) 
VALUES 
";
                            var i = 0;
                            var c = string.Empty;
                            foreach (var item in _FAPPYEH0s_insert)
                            {
                                sql += $@" {c} ( :APPLY_NO_{i} , :APPLY_SEQ_{i} , :SYSTEM_{i}, :BANK_CODE_{i} , :CHECK_NO_{i} , :UPD_ID_{i} , :UPD_DATE_{i} , :UPD_TIME_{i} ) ";
                                com.Parameters.Add($@"APPLY_NO_{i}", item.APPLY_NO.strto400DB()); //申請單號
                                com.Parameters.Add($@"APPLY_SEQ_{i}", "1"); //申請序號
                                com.Parameters.Add($@"SYSTEM_{i}", item.SYSTEM.strto400DB()); //系統別
                                com.Parameters.Add($@"BANK_CODE_{i}", item.BANK_CODE.strto400DB()); //付款帳戶
                                com.Parameters.Add($@"CHECK_NO_{i}", item.CHECK_NO.strto400DB()); //支票號碼
                                com.Parameters.Add($@"UPD_ID_{i}", userId); //異動人員 
                                com.Parameters.Add($@"UPD_DATE_{i}", updatedt); //異動日期
                                com.Parameters.Add($@"UPD_TIME_{i}", updatetm); //異動時間                                             
                                i += 1;
                                c = " , ";
                            }
                            com.Transaction = transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            com.Dispose();
                            NLog.LogManager.GetCurrentClassLogger().Info("新增 應付票據抽票申請明細檔 End!");
                        }
                        #endregion
                    }

                    #endregion

                    #region 修改 應付票據抽票申請 檔案

                    #region 應付票據檔有找到的支票號碼 修改 應付票據抽票申請主檔 & 明細檔 (非本次新增系統類)

                    List<tempData> _tempDatas = searchtemps.Where(x => !x.alreadyUpdate && !x.check_no.IsNullOrWhiteSpace()).ToList();
                    if (_tempDatas.Any())
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("應付票據檔有找到的支票號碼 修改 應付票據抽票申請主檔 & 明細檔 (非本次新增系統類) Start!");
                        foreach (var item in _tempDatas) //(非本次新增系統類)
                        {
                            #region 修改 應付票據抽票申請主檔
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
UPDATE LAPPYEH1
set CHECK_NO = :CHECK_NO,
    BANK_CODE = :BANK_CODE
where APPLY_NO = :APPLY_NO ";
                                com.Parameters.Add($@"CHECK_NO", item.check_no.strto400DB()); //支票號碼
                                com.Parameters.Add($@"BANK_CODE", item.bank_code.strto400DB()); //付款帳戶
                                com.Parameters.Add($@"APPLY_NO", item.apply_no.strto400DB()); //申請單號
                                com.Transaction = transaction;
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                            #endregion

                            #region 修改 應付票據抽票申請明細檔
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
UPDATE LAPPYED1
set CHECK_NO = :CHECK_NO,
    BANK_CODE = :BANK_CODE,
    UPD_ID = :UPD_ID,
    UPD_DATE = :UPD_DATE,
    UPD_TIME = :UPD_TIME
where APPLY_NO = :APPLY_NO ";
                                com.Parameters.Add($@"CHECK_NO", item.check_no.strto400DB()); //支票號碼
                                com.Parameters.Add($@"BANK_CODE", item.bank_code.strto400DB()); //付款帳戶
                                com.Parameters.Add($@"UPD_ID", userId); //異動人員 
                                com.Parameters.Add($@"UPD_DATE", updatedt); //異動日期
                                com.Parameters.Add($@"UPD_TIME", updatetm); //異動時間     
                                com.Parameters.Add($@"APPLY_NO", item.apply_no.strto400DB()); //申請單號
                                com.Transaction = transaction;
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                            #endregion
                        }
                        NLog.LogManager.GetCurrentClassLogger().Info("應付票據檔有找到的支票號碼 修改 應付票據抽票申請主檔 & 明細檔 (非本次新增系統類) End!");
                    }

                    #endregion

                    #region 把申請類 狀態2=>3
                    using (EacCommand com = new EacCommand(conn))
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("把申請類 狀態2=>3 Start!");
                        //申請類 狀態為由 2(完成申請) => 3(受理中)
                        sql = $@"update LAPPYEH1
set STATUS = '3'
where STATUS = '2'
and LPAD(apply_no,2,'') = 'CE' 
";
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                        NLog.LogManager.GetCurrentClassLogger().Info("把申請類 狀態2=>3 End!");
                    }
                    #endregion

                    #endregion

                    try
                    {
                        transaction.Commit();
                        var _count = _FAPPYEH0s_insert.Count();
                        NLog.LogManager.GetCurrentClassLogger().Info($@"新增應付票據抽票申請主檔:{_count}筆");
                        flag = true;
                        if (_count > 0)
                        {
                            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                            piaLogMain.TRACKING_TYPE = "A";
                            piaLogMain.ACCESS_ACCOUNT = userId;
                            piaLogMain.ACCOUNT_NAME = "";
                            piaLogMain.PROGFUN_NAME = "OAP0028";
                            piaLogMain.EXECUTION_CONTENT = $@"entry_date:{entry_date}";
                            piaLogMain.AFFECT_ROWS = _count;
                            piaLogMain.PIA_TYPE = "0100000000";
                            piaLogMain.EXECUTION_TYPE = "A";
                            piaLogMain.ACCESSOBJ_NAME = "FGLGPCK";
                            piaLogMainDao.Insert(piaLogMain);
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = ex.exceptionMessage();
                        NLog.LogManager.GetCurrentClassLogger().Error(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.exceptionMessage();
                NLog.LogManager.GetCurrentClassLogger().Error(msg);
            }
            return new Tuple<bool, string>(msg.IsNullOrWhiteSpace(), msg);
        }

        /// <summary> 
        /// 要新增 應付票據抽票申請主檔 的暫存檔
        /// </summary>
        public class FAPPYEH0
        {
            /// <summary>
            /// 申請單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 狀態
            /// </summary>
            public string STATUS { get; set; }

            /// <summary>
            /// 支票開立否
            /// </summary>
            public string CHECK_WT { get; set; }

            /// <summary>
            /// 申請人員
            /// </summary>
            public string APPLY_ID { get; set; }

            /// <summary>
            /// 申請人員部門
            /// </summary>
            public string APPLY_UNIT { get; set; }

            /// <summary>
            /// 抽票原因
            /// </summary>
            public string CE_RSN { get; set; }

            /// <summary>
            /// 收件單位
            /// </summary>
            public string RCV_UNIT { get; set; }

            /// <summary>
            /// 收件人員
            /// </summary>
            public string RCV_ID { get; set; }

            /// <summary>
            /// 郵遞區號
            /// </summary>
            public string ZIP_CODE { get; set; }

            /// <summary>
            /// 地址 
            /// </summary>
            public string ADDR { get; set; }

            /// <summary>
            /// 目前註記
            /// </summary>
            public string MARK_TYPE2 { get; set; }

            /// <summary>
            /// 寄送方式
            /// </summary>
            public string SEND_STYLE { get; set; }

            /// <summary>
            /// 系統別
            /// </summary>
            public string SYSTEM { get; set; }

            /// <summary>
            /// 付款申請編號
            /// </summary>
            public string APLY_NO { get; set; }

            /// <summary>
            /// 付款申請序號
            /// </summary>
            public string APLY_SEQ { get; set; }

            /// <summary>
            /// 付款帳戶
            /// </summary>
            public string BANK_CODE { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string CHECK_NO { get; set; }


        }

        /// <summary>
        /// 用於比較重複資料的 類別
        /// </summary>
        public class compareData
        {
            /// <summary>
            /// 付款申請編號
            /// </summary>
            public string aply_no { get; set; }

            /// <summary>
            /// 付款申請序號
            /// </summary>
            public string aply_seq { get; set; }
        }

        /// <summary>
        /// 撈取應付票據檔 暫存資料
        /// </summary>
        public class tempData
        {
            /// <summary>
            /// 申請單號
            /// </summary>
            public string apply_no { get; set; } 

            /// <summary>
            /// 付款申請編號
            /// </summary>
            public string aply_no { get; set; }

            /// <summary>
            /// 付款申請序號
            /// </summary>
            public string aply_seq { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string check_no { get; set; }

            /// <summary>
            /// 付款帳戶
            /// </summary>
            public string bank_code { get; set; }

            /// <summary>
            /// 是否已修改
            /// </summary>
            public bool alreadyUpdate { get; set; }
        }

    }
}