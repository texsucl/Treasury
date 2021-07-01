using FGL.Web.BO;
using FGL.Web.Service.Interface;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using static FGL.Web.Enum.Ref;
using static FGL.Web.BO.Utility;
using FGL.Web.Utilitys;
using FGL.Web.Daos;
using FGL.Web.Models;
using System.Web.Mvc;

/// <summary>
/// 功能說明：退費類別覆核
/// 初版作者：20200710 Mark
/// 修改歷程：20200710 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FGL.Web.Service.Actual
{
    public class OGL00010A : Common , IOGL00010A
    {
        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel<List<OGL00010ViewModel>> GetSearchData(string payclass,string userId)
        {
            MSGReturnModel<List<OGL00010ViewModel>> result = new MSGReturnModel<List<OGL00010ViewModel>>();
            result.Datas = new List<OGL00010ViewModel>();
            List <OGL00010ViewModel> mainData = new List<OGL00010ViewModel>();
            List<OGL00010ViewSubModel> subData = new List<OGL00010ViewSubModel>();
            List<SYS_CODE> sys_codes = new List<SYS_CODE>();
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<string> code_types = new List<string>() {
                        "PREM_KIND", //保費類別
                        "CONT_TYPE", //合約別
                        "PROD_TYPE", //商品別
                        "EXEC_ACTION", //執行動作
                        "APPR_STAT", //申請狀態
                        "DATA_STATUS" //資料狀態
                    };

                    sys_codes = db.SYS_CODE.AsNoTracking()
                       .Where(x => code_types.Contains(x.CODE_TYPE) && x.SYS_CD == "GL").ToList();

                    mainData = db.FGL_PAY_MAIN_HIS.AsNoTracking().Where(x => x.apply_status == "1")
                        .Where(x => x.pay_class == payclass, !payclass.IsNullOrWhiteSpace())
                        .OrderBy(x => x.pay_class)
                        .AsEnumerable()
                        .Select(x => new OGL00010ViewModel() {
                            pay_class = x.pay_class, //退費項目類別
                            memo_n = x.memo_n, //備註_新
                            memo_o = x.memo_o, //備註_舊
                            item_yn_n = x.item_yn_n, //險種否_新
                            item_yn_o = x.item_yn_o, //險種否_舊
                            year_yn_n = x.year_yn_n, //年次否_新
                            year_yn_o = x.year_yn_o, //年次否_舊
                            prem_yn_n = x.prem_yn_n, //保費類別否_新
                            prem_yn_o = x.prem_yn_o, //保費類別否_舊
                            unit_yn_n = x.unit_yn_n, //費用單位否_新
                            unit_yn_o = x.unit_yn_o, //費用單位否_舊
                            recp_yn_n = x.recp_yn_n, //送金單否_新
                            recp_yn_o = x.recp_yn_o, //送金單否_舊
                            cont_yn_n = x.cont_yn_n, //合約別否_新
                            cont_yn_o = x.cont_yn_o, //合約別否_舊
                            corp_yn_n = x.corp_yn_n, //帳本否_新
                            corp_yn_o = x.corp_yn_o, //帳本否_舊
                            exec_action = x.exec_action, //執行動作
                            exec_action_D = getCodeValue(sys_codes, "EXEC_ACTION", x.exec_action), //執行動作中文
                            apply_id = x.apply_id, //申請人員
                            pk_id = x.pk_id, //pk_id
                            review_flag = x.apply_id != userId, //覆核權限
                            SubDatas = new List<OGL00010ViewSubModel>()
                        }).ToList();

                    List<string> pk_ids = mainData.Select(x => x.pk_id).ToList();
                    foreach (var item in db.FGL_PAY_SUB_HIS.AsNoTracking()
                        .Where(x => pk_ids.Contains(x.main_pk_id))
                        .GroupBy(x=>x.main_pk_id))
                    {
                        var _main = mainData.First(x => x.pk_id == item.Key);
                        _main.SubDatas.AddRange(item.Select(x => new OGL00010ViewSubModel()
                        {
                            pay_class = x.pay_class, //退費項目類別
                            prem_kind_n = x.prem_kind_n, //保費類別_新
                            prem_kind_n_D = getCodeValue(sys_codes, "PREM_KIND", x.prem_kind_n), //保費類別_新_中文
                            prem_kind_o = x.prem_kind_o, //保費類別_舊
                            prem_kind_o_D = getCodeValue(sys_codes, "PREM_KIND", x.prem_kind_o), //保費類別_舊_中文
                            cont_type_n = x.cont_type_n, //合約別_新
                            cont_type_n_D = getCodeValue(sys_codes, "CONT_TYPE", x.cont_type_n), //合約別_新_中文
                            cont_type_o = x.cont_type_o, //合約別_舊
                            cont_type_o_D = getCodeValue(sys_codes, "CONT_TYPE", x.cont_type_o), //合約別_舊_中文
                            prod_type_n = x.prod_type_n, //商品別_新
                            prod_type_n_D = getCodeValue(sys_codes, "PROD_TYPE", x.prod_type_n), //商品別_新_中文
                            prod_type_o = x.prod_type_o, //商品別_舊
                            prod_type_o_D = getCodeValue(sys_codes, "PROD_TYPE", x.prod_type_o), //商品別_舊_中文
                            corp_no_n = x.corp_no_n, //帳本別_新
                            corp_no_o = x.corp_no_o, //帳本別_舊
                            actnum_yn_n = x.actnum_yn_n, //取會科否_新
                            actnum_yn_o = x.actnum_yn_o, //取會科否_舊
                            acct_code_n = x.acct_code_n, //保費收入首年首期_新
                            acct_code_o = x.acct_code_o, //保費收入首年首期_舊
                            acct_codef_n = x.acct_codef_n, //保費收入首年續期_新
                            acct_codef_o = x.acct_codef_o, //保費收入首年續期_舊
                            acct_coder_n = x.acct_coder_n, //續年度_新
                            acct_coder_o = x.acct_coder_o, //續年度_舊
                            exec_action = x.exec_action, //執行動作
                            exec_action_D = getCodeValue(sys_codes, "EXEC_ACTION", x.exec_action), //執行動作中文
                            pk_id = x.pk_id
                        }));
                    }
                    if (mainData.Any())
                    {
                        result.RETURN_FLAG = true;
                        result.Datas = mainData;
                    }
                    else
                    {
                        result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                result.DESCRIPTION = MessageType.sys_Error.GetDescription(null, ex.exceptionMessage());
            }
            return result;
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="datas">核可資料</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel ApprovedData(IEnumerable<OGL00010ViewModel> _datas, string userId)
        {
            MSGReturnModel result = new MSGReturnModel();
            List<FRTITEM0> FRT_inserts = new List<FRTITEM0>();
            List<Tuple<FRTITEM0,FRTITEM0>> FRT_updates = new List<Tuple<FRTITEM0, FRTITEM0>>(); //item1舊資料 item2新資料
            List<FRTITEM0> FRT_deletes = new List<FRTITEM0>();
            List<FRTITEM0> FRT_deletes_all = new List<FRTITEM0>();
            List<FPMCODE0> FPM_inserts = new List<FPMCODE0>();
            List<FPMCODE0> FPM_updates = new List<FPMCODE0>();
            List<FPMCODE0> FPM_deletes = new List<FPMCODE0>();

            DateTime dtn = DateTime.Now;
            var chtyear = (dtn.Year - 1911).ToString();
            var chtmonth = dtn.ToString("MM");
            var chtday = dtn.ToString("dd");
            var chtDate = $@"{chtyear}{chtmonth}{chtday}";
            var chtTime = $@"{dtn.ToString("HHmmssff")}";
            var datas = _datas.ToList();
            List<string> pk_ids = datas.Select(x => x.pk_id).ToList();
            try
            {
                if (!datas.Any())
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
                else
                {
                    #region 檢核資料異動
                    using (dbFGLEntities db = new dbFGLEntities())
                    {    
                        foreach (var item in db.FGL_PAY_MAIN_HIS.Where(x => pk_ids.Contains(x.pk_id)))
                        {
                            if (item.apply_status != "1") //狀態非為 表單申請 代表資料已被異動
                            {
                                result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                return result;
                            }
                        }
                    }
                    #endregion

                    #region 組合異動資料
                    foreach (var data in datas)
                    {
                        switch (data.exec_action)
                        {
                            case "A":

                                #region insert FRTITEM0
                                foreach (var item in data.SubDatas)
                                {
                                    FRT_inserts.Add(new FRTITEM0()
                                    {
                                        PAY_CLASS = data.pay_class, //退費項目類別
                                        ITEM_YN = data.item_yn_n, //險種否
                                        YEAR_YN = data.year_yn_n, //年次否
                                        PREM_YN = data.prem_yn_n, //保費類別否
                                        UNIT_YN = data.unit_yn_n, //費用單位否
                                        RECP_YN = data.recp_yn_n, //送金單否
                                        CONT_YN = data.cont_yn_n, //合約別否
                                        CORP_YN = data.corp_yn_n, //帳本否
                                        PREM_KIND = item.prem_kind_n, //保費類別
                                        CONT_TYPE = item.cont_type_n, //合約別
                                        PROD_TYPE = item.prod_type_n, //商品別
                                        CORP_NO = item.corp_no_n, //帳本別
                                        ACTNUM_YN = item.actnum_yn_n, //取會科否
                                        ACCT_CODE = item.acct_code_n, //保費收入首年首期
                                        ACCT_CODEF = item.acct_codef_n, //保費收入首年續期
                                        ACCT_CODER = item.acct_coder_n, //續年度
                                        //UPD_ID = data.apply_id, //異動人員
                                        UPD_ID = userId, //異動人員
                                        UPD_DATE = chtDate, //異動日期
                                        UPD_TIME = chtTime //異動時間
                                    });
                                }
                                #endregion

                                #region insert FPMCODE0
                                FPM_inserts.Add(new FPMCODE0()
                                {
                                    GROUP_ID = "RM-CODE", //組別碼
                                    TEXT_LEN = "42", //文字長度
                                    REF_NO = data.pay_class, //參考號碼 => 寫退費類別進來
                                    TEXT = data.memo_n, //說明 => 寫退費類別中文進來
                                    SRCE_FROM = "RT", //資料來源
                                    USE_MARK = " ", //使用註記 
                                    ENTRY_YY = chtyear, //輸入日期–年 => 民國年
                                    ENTRY_MM = chtmonth, //輸入日期–月
                                    ENTRY_DD = chtday, //輸入日期–日
                                    ENTRY_TIME = chtTime, //登打日期 - 時間
                                    //ENTRY_ID = data.apply_id, //輸入人員
                                    ENTRY_ID = userId, //輸入人員
                                    UPD_YY = "0", //異動日期–年 => 民國年
                                    UPD_MM = "0", //異動日期–月
                                    UPD_DD = "0", //異動日期–日
                                    UPD_ID = " " //異動人員代號 => 五碼ＩＤ
                                });
                                #endregion

                                break;

                            case "U":
                                var updateDatas = new List<OGL00010ViewSubModel>();
                                #region update FRTITEM0
                                if ((data.item_yn_n != data.item_yn_o) || (data.year_yn_n != data.year_yn_o) ||
                                   (data.prem_yn_n != data.prem_yn_o) || (data.unit_yn_n != data.unit_yn_o) ||
                                   (data.recp_yn_n != data.recp_yn_o) || (data.cont_yn_n != data.cont_yn_o) ||
                                   (data.corp_yn_n != data.corp_yn_o)) //Group資料有異動
                                {
                                    updateDatas = data.SubDatas;
                                }
                                else //Group資料無異動
                                {
                                    updateDatas = data.SubDatas.Where(
                                        z => !z.exec_action.IsNullOrWhiteSpace()).ToList();
                                }
                                foreach (var item in updateDatas)
                                {
                                    if ((item.prem_kind_o != item.prem_kind_n) || (item.cont_type_o != item.cont_type_n) ||
                                       (item.prod_type_o != item.prod_type_n) || (item.corp_no_o != item.corp_no_n))
                                    {
                                        #region 當 key 有異動 , 修改需先刪除再新增 , 新增&刪除就只為新增&刪除
                                        if (item.exec_action != "A") //當資料不為新增 都需刪除舊資料
                                        {
                                            FRT_deletes.Add(new FRTITEM0()  //刪除資料
                                            {
                                                PAY_CLASS = data.pay_class, //退費項目類別
                                                PREM_KIND = item.prem_kind_o, //保費類別(舊)
                                                CONT_TYPE = item.cont_type_o, //合約別(舊)
                                                PROD_TYPE = item.prod_type_o, //商品別(舊) 
                                                CORP_NO = item.corp_no_o, //帳本別(舊)          
                                            });
                                        }
                                        if (item.exec_action != "D") //當資料不為刪除 都須新增資料
                                        {
                                            FRT_inserts.Add(new FRTITEM0() //新增資料
                                            {
                                                PAY_CLASS = data.pay_class, //退費項目類別
                                                ITEM_YN = data.item_yn_n, //險種否(新)
                                                YEAR_YN = data.year_yn_n, //年次否(新)
                                                PREM_YN = data.prem_yn_n, //保費類別否(新)
                                                UNIT_YN = data.unit_yn_n, //費用單位否(新)
                                                RECP_YN = data.recp_yn_n, //送金單否(新)
                                                CONT_YN = data.cont_yn_n, //合約別否(新)
                                                CORP_YN = data.corp_yn_n, //帳本否(新)
                                                PREM_KIND = item.prem_kind_n, //保費類別(新)
                                                CONT_TYPE = item.cont_type_n, //合約別(新)
                                                PROD_TYPE = item.prod_type_n, //商品別(新)
                                                CORP_NO = item.corp_no_n, //帳本別(新)
                                                ACTNUM_YN = item.actnum_yn_n, //取會科否(新)
                                                ACCT_CODE = item.acct_code_n, //保費收入首年首期(新)
                                                ACCT_CODEF = item.acct_codef_n, //保費收入首年續期(新)
                                                ACCT_CODER = item.acct_coder_n, //續年度(新)
                                                //UPD_ID = data.apply_id,
                                                UPD_ID = userId,
                                                UPD_DATE = chtDate, //異動日期
                                                UPD_TIME = chtTime //異動時間
                                            });
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 當 key 無異動 , 修改直接修改 , 新增&刪除就只為新增&刪除
                                        if (item.exec_action == "A")
                                        {
                                            FRT_inserts.Add(new FRTITEM0() //新增資料
                                            {
                                                PAY_CLASS = data.pay_class, //退費項目類別
                                                ITEM_YN = data.item_yn_n, //險種否(新)
                                                YEAR_YN = data.year_yn_n, //年次否(新)
                                                PREM_YN = data.prem_yn_n, //保費類別否(新)
                                                UNIT_YN = data.unit_yn_n, //費用單位否(新)
                                                RECP_YN = data.recp_yn_n, //送金單否(新)
                                                CONT_YN = data.cont_yn_n, //合約別否(新)
                                                CORP_YN = data.corp_yn_n, //帳本否(新)
                                                PREM_KIND = item.prem_kind_n, //保費類別(新)
                                                CONT_TYPE = item.cont_type_n, //合約別(新)
                                                PROD_TYPE = item.prod_type_n, //商品別(新)
                                                CORP_NO = item.corp_no_n, //帳本別(新)
                                                ACTNUM_YN = item.actnum_yn_n, //取會科否(新)
                                                ACCT_CODE = item.acct_code_n, //保費收入首年首期(新)
                                                ACCT_CODEF = item.acct_codef_n, //保費收入首年續期(新)
                                                ACCT_CODER = item.acct_coder_n, //續年度(新)
                                                //UPD_ID = data.apply_id,
                                                UPD_ID = userId,
                                                UPD_DATE = chtDate, //異動日期
                                                UPD_TIME = chtTime //異動時間
                                            });
                                        }
                                        else if ((item.exec_action == "U") || (item.exec_action == null))
                                        {
                                            FRT_updates.Add(new Tuple<FRTITEM0, FRTITEM0>(
                                                new FRTITEM0() //舊資料
                                                {
                                                    PAY_CLASS = data.pay_class, //退費項目類別
                                                    PREM_KIND = item.prem_kind_o, //保費類別(舊)
                                                    CONT_TYPE = item.cont_type_o, //合約別(舊)
                                                    PROD_TYPE = item.prod_type_o, //商品別(舊)
                                                    CORP_NO = item.corp_no_o, //帳本別(舊)
                                                },
                                                new FRTITEM0() //新資料
                                                {
                                                    PAY_CLASS = data.pay_class, //退費項目類別
                                                    ITEM_YN = data.item_yn_n, //險種否(新)
                                                    YEAR_YN = data.year_yn_n, //年次否(新)
                                                    PREM_YN = data.prem_yn_n, //保費類別否(新)
                                                    UNIT_YN = data.unit_yn_n, //費用單位否(新)
                                                    RECP_YN = data.recp_yn_n, //送金單否(新)
                                                    CONT_YN = data.cont_yn_n, //合約別否(新)
                                                    CORP_YN = data.corp_yn_n, //帳本否(新)
                                                    PREM_KIND = item.prem_kind_n, //保費類別(新)
                                                    CONT_TYPE = item.cont_type_n, //合約別(新)
                                                    PROD_TYPE = item.prod_type_n, //商品別(新)
                                                    CORP_NO = item.corp_no_n, //帳本別(新)
                                                    ACTNUM_YN = item.actnum_yn_n, //取會科否(新)
                                                    ACCT_CODE = item.acct_code_n, //保費收入首年首期(新)
                                                    ACCT_CODEF = item.acct_codef_n, //保費收入首年續期(新)
                                                    ACCT_CODER = item.acct_coder_n, //續年度(新)
                                                    //UPD_ID = data.apply_id,
                                                    UPD_ID = userId,
                                                    UPD_DATE = chtDate, //異動日期
                                                    UPD_TIME = chtTime //異動時間
                                                }));
                                        }
                                        else if (item.exec_action == "D")
                                        {
                                            FRT_deletes.Add(new FRTITEM0()  //刪除資料
                                            {
                                                PAY_CLASS = data.pay_class, //退費項目類別
                                                PREM_KIND = item.prem_kind_o, //保費類別(舊)
                                                CONT_TYPE = item.cont_type_o, //合約別(舊)
                                                PROD_TYPE = item.prod_type_o, //商品別(舊) 
                                                CORP_NO = item.corp_no_o, //帳本別(舊)          
                                            });
                                        }
                                        #endregion
                                    }
                                }
                                #endregion

                                #region update FPMCODE0
                                if (data.memo_n != data.memo_o)
                                {
                                    FPM_updates.Add(new FPMCODE0()
                                    {
                                        GROUP_ID = "RM-CODE", //組別碼
                                        TEXT_LEN = "42", //文字長度
                                        REF_NO = data.pay_class, //參考號碼 => 寫退費類別進來
                                        TEXT = data.memo_n, //說明 => 寫退費類別中文進來
                                        SRCE_FROM = "RT", //資料來源
                                        UPD_YY = chtyear, //異動日期–年 => 民國年
                                        UPD_MM = chtmonth, //異動日期–月
                                        UPD_DD = chtday, //異動日期–日
                                        UPD_ID = userId //異動人員代號 => 五碼ＩＤ
                                    });
                                }
                                #endregion

                                break;
                            case "D":

                                #region delete FRTITEM0
                                FRT_deletes_all.Add(new FRTITEM0()
                                {
                                    PAY_CLASS = data.pay_class //退費項目類別
                                });
                                #endregion

                                #region delete FPMCODE0
                                FPM_deletes.Add(new FPMCODE0()
                                {
                                    SRCE_FROM = "RT",
                                    GROUP_ID = "RM-CODE",
                                    REF_NO = data.pay_class
                                });
                                #endregion

                                break;
                        }
                    }
                    #endregion

                    #region 資料庫異動
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        string sql = string.Empty;
                        EacTransaction transaction = conn.BeginTransaction();

                        #region 刪除 退費類別設定檔 by key值
                        if (FRT_deletes.Any())
                        {
                            foreach (var item in FRT_deletes)
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
DELETE FRTITEM0
WHERE PAY_CLASS = :PAY_CLASS 
AND   PREM_KIND = :PREM_KIND 
AND   CONT_TYPE = :CONT_TYPE 
AND   PROD_TYPE = :PROD_TYPE 
AND   CORP_NO = :CORP_NO
";
                                    com.Parameters.Add($@"PAY_CLASS", item.PAY_CLASS.strto400DB()); //退費項目類別
                                    com.Parameters.Add($@"PREM_KIND", item.PREM_KIND.strto400DB()); //保費類別
                                    com.Parameters.Add($@"CONT_TYPE", item.CONT_TYPE.strto400DB()); //合約別
                                    com.Parameters.Add($@"PROD_TYPE", item.PROD_TYPE.strto400DB()); //商品別
                                    com.Parameters.Add($@"CORP_NO", item.CORP_NO.strto400DB()); //帳本別
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                            }
                        }
                        #endregion

                        #region 刪除 退費類別設定檔 by 退費項目類別
                        if (FRT_deletes_all.Any())
                        {
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
DELETE FRTITEM0
WHERE PAY_CLASS IN (
";
                                int i = 0;
                                string c = string.Empty;
                                foreach (var item in FRT_deletes_all.Select(x => x.PAY_CLASS))
                                {
                                    sql += $@" {c} :PAY_CLASS_{i} ";
                                    com.Parameters.Add($@"PAY_CLASS_{i}", item);
                                    c = " , ";
                                    i += 1;
                                }
                                sql += ")";
                                com.Transaction = transaction;
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                        }
                        #endregion

                        #region 新增 退費類別設定檔
                        if (FRT_inserts.Any())
                        {
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
INSERT INTO FRTITEM0
(PAY_CLASS, ITEM_YN, YEAR_YN, PREM_YN, UNIT_YN, RECP_YN, CONT_YN, CORP_YN, PREM_KIND, CONT_TYPE, 
PROD_TYPE, CORP_NO, ACTNUM_YN, ACCT_CODE, ACCT_CODEF, ACCT_CODER, UPD_ID, UPD_DATE, UPD_TIME)
VALUES
";
                                var i = 0;
                                var c = string.Empty;
                                foreach (var item in FRT_inserts)
                                {
                                    sql += $@" {c} ( :PAY_CLASS_{i} , :ITEM_YN_{i} , :YEAR_YN_{i} , :PREM_YN_{i} , :UNIT_YN_{i} , :RECP_YN_{i} , :CONT_YN_{i} , :CORP_YN_{i} , :PREM_KIND_{i} , :CONT_TYPE_{i}, ";
                                    sql += $@" :PROD_TYPE_{i} , :CORP_NO_{i} , :ACTNUM_YN_{i} , :ACCT_CODE_{i} , :ACCT_CODEF_{i} , :ACCT_CODER_{i} , :UPD_ID_{i} , :UPD_DATE_{i} , :UPD_TIME_{i}) ";
                                    com.Parameters.Add($@"PAY_CLASS_{i}", item.PAY_CLASS.strto400DB()); //退費項目類別
                                    com.Parameters.Add($@"ITEM_YN_{i}", item.ITEM_YN.strto400DB()); //險種否
                                    com.Parameters.Add($@"YEAR_YN_{i}", item.YEAR_YN.strto400DB()); //年次否
                                    com.Parameters.Add($@"PREM_YN_{i}", item.PREM_YN.strto400DB()); //保費類別否
                                    com.Parameters.Add($@"UNIT_YN_{i}", item.UNIT_YN.strto400DB()); //費用單位否
                                    com.Parameters.Add($@"RECP_YN_{i}", item.RECP_YN.strto400DB()); //送金單否
                                    com.Parameters.Add($@"CONT_YN_{i}", item.CONT_YN.strto400DB()); //合約別否
                                    com.Parameters.Add($@"CORP_YN_{i}", item.CORP_YN.strto400DB()); //帳本否
                                    com.Parameters.Add($@"PREM_KIND_{i}", item.PREM_KIND.strto400DB()); //保費類別
                                    com.Parameters.Add($@"CONT_TYPE_{i}", item.CONT_TYPE.strto400DB()); //合約別
                                    com.Parameters.Add($@"PROD_TYPE_{i}", item.PROD_TYPE.strto400DB()); //商品別
                                    com.Parameters.Add($@"CORP_NO_{i}", item.CORP_NO.strto400DB()); //帳本別
                                    com.Parameters.Add($@"ACTNUM_YN_{i}", item.ACTNUM_YN.strto400DB()); //取會科否
                                    com.Parameters.Add($@"ACCT_CODE_{i}", item.ACCT_CODE.strto400DB()); //保費收入首年首期
                                    com.Parameters.Add($@"ACCT_CODEF_{i}", item.ACCT_CODEF.strto400DB()); //保費收入首年續期
                                    com.Parameters.Add($@"ACCT_CODER_{i}", item.ACCT_CODER.strto400DB()); //續年度
                                    com.Parameters.Add($@"UPD_ID_{i}", item.UPD_ID.strto400DB()); //異動人員
                                    com.Parameters.Add($@"UPD_DATE_{i}", item.UPD_DATE.strto400DB()); //異動日期
                                    com.Parameters.Add($@"UPD_TIME_{i}", item.UPD_TIME.strto400DB()); //異動時間
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
                        #endregion

                        #region 更新 退費類別設定檔
                        if (FRT_updates.Any())
                        {
                            foreach (var item in FRT_updates)
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
UPDATE FRTITEM0
SET ITEM_YN = :ITEM_YN,
    YEAR_YN = :YEAR_YN,
    PREM_YN = :PREM_YN,
    UNIT_YN = :UNIT_YN,
    RECP_YN = :RECP_YN,
    CONT_YN = :CONT_YN,
    CORP_YN = :CORP_YN,
    ACTNUM_YN = :ACTNUM_YN,
    ACCT_CODE = :ACCT_CODE,
    ACCT_CODEF = :ACCT_CODEF,
    ACCT_CODER = :ACCT_CODER,
    UPD_ID = :UPD_ID,
    UPD_DATE = :UPD_DATE,
    UPD_TIME = :UPD_TIME
WHERE PAY_CLASS = :PAY_CLASS 
AND   PREM_KIND = :PREM_KIND 
AND   CONT_TYPE = :CONT_TYPE 
AND   PROD_TYPE = :PROD_TYPE 
AND   CORP_NO = :CORP_NO
";
                                    com.Parameters.Add($@"ITEM_YN", item.Item2.ITEM_YN.strto400DB()); //險種否
                                    com.Parameters.Add($@"YEAR_YN", item.Item2.YEAR_YN.strto400DB()); //年次否
                                    com.Parameters.Add($@"PREM_YN", item.Item2.PREM_YN.strto400DB()); //保費類別否
                                    com.Parameters.Add($@"UNIT_YN", item.Item2.UNIT_YN.strto400DB()); //費用單位否
                                    com.Parameters.Add($@"RECP_YN", item.Item2.RECP_YN.strto400DB()); //送金單否
                                    com.Parameters.Add($@"CONT_YN", item.Item2.CONT_YN.strto400DB()); //合約別否
                                    com.Parameters.Add($@"CORP_YN", item.Item2.CORP_YN.strto400DB()); //帳本否
                                    com.Parameters.Add($@"ACTNUM_YN", item.Item2.ACTNUM_YN.strto400DB()); //取會科否
                                    com.Parameters.Add($@"ACCT_CODE", item.Item2.ACCT_CODE.strto400DB()); //保費收入首年首期
                                    com.Parameters.Add($@"ACCT_CODEF", item.Item2.ACCT_CODEF.strto400DB()); //保費收入首年續期
                                    com.Parameters.Add($@"ACCT_CODER", item.Item2.ACCT_CODER.strto400DB()); //續年度
                                    com.Parameters.Add($@"UPD_ID", item.Item2.UPD_ID.strto400DB()); //異動人員
                                    com.Parameters.Add($@"UPD_DATE", item.Item2.UPD_DATE.strto400DB()); //異動日期
                                    com.Parameters.Add($@"UPD_TIME", item.Item2.UPD_TIME.strto400DB()); //異動時間
                                    com.Parameters.Add($@"PAY_CLASS", item.Item1.PAY_CLASS.strto400DB()); //退費項目類別
                                    com.Parameters.Add($@"PREM_KIND", item.Item1.PREM_KIND.strto400DB()); //保費類別
                                    com.Parameters.Add($@"CONT_TYPE", item.Item1.CONT_TYPE.strto400DB()); //合約別
                                    com.Parameters.Add($@"PROD_TYPE", item.Item1.PROD_TYPE.strto400DB()); //商品別
                                    com.Parameters.Add($@"CORP_NO", item.Item1.CORP_NO.strto400DB()); //帳本別
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                            }
                        }
                        #endregion

                        #region 刪除設定檔
                        if (FPM_deletes.Any())
                        {
                            foreach (var item in FPM_deletes)
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
DELETE FPMCODE0
WHERE GROUP_ID = :GROUP_ID
AND   REF_NO = :REF_NO
AND   SRCE_FROM = :SRCE_FROM
";
                                    com.Parameters.Add($@"GROUP_ID", item.GROUP_ID.strto400DB()); //組別碼
                                    com.Parameters.Add($@"REF_NO", item.REF_NO.strto400DB()); //參考號碼 => 寫退費類別進來
                                    com.Parameters.Add($@"SRCE_FROM", item.SRCE_FROM.strto400DB()); //資料來源
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                            }
                        }
                        #endregion

                        #region 新增設定檔
                        if (FPM_inserts.Any())
                        {
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
INSERT INTO FPMCODE0 
(GROUP_ID, TEXT_LEN, REF_NO, TEXT, SRCE_FROM, USE_MARK, ENTRY_YY, ENTRY_MM, ENTRY_DD,
ENTRY_TIME, ENTRY_ID, UPD_YY, UPD_MM, UPD_DD, UPD_ID)
VALUES ";
                                var i = 0;
                                var c = string.Empty;
                                foreach (var item in FPM_inserts)
                                {
                                    sql += $@" {c} ( :GROUP_ID_{i} , :TEXT_LEN_{i} , :REF_NO_{i} , :TEXT_{i} , :SRCE_FROM_{i} , :USE_MARK_{i} , :ENTRY_YY_{i} , :ENTRY_MM_{i} , :ENTRY_DD_{i} , ";
                                    sql += $@" :ENTRY_TIME_{i} , :ENTRY_ID_{i} , :UPD_YY_{i} , :UPD_MM_{i} , :UPD_DD_{i} , :UPD_ID_{i}) ";
                                    com.Parameters.Add($@"GROUP_ID_{i}", item.GROUP_ID.strto400DB()); //組別碼
                                    com.Parameters.Add($@"TEXT_LEN_{i}", item.TEXT_LEN.strto400DB()); //文字長度
                                    com.Parameters.Add($@"REF_NO_{i}", item.REF_NO.strto400DB()); //參考號碼 => 寫退費類別進來
                                    com.Parameters.Add($@"TEXT_{i}", item.TEXT.strto400DB()); //說明 => 寫退費類別中文進來
                                    com.Parameters.Add($@"SRCE_FROM_{i}", item.SRCE_FROM.strto400DB()); //資料來源
                                    com.Parameters.Add($@"USE_MARK_{i}", item.USE_MARK.strto400DB()); //使用註記 
                                    com.Parameters.Add($@"ENTRY_YY_{i}", item.ENTRY_YY.strto400DB()); //輸入日期–年 => 民國年
                                    com.Parameters.Add($@"ENTRY_MM_{i}", item.ENTRY_MM.strto400DB()); //輸入日期–月
                                    com.Parameters.Add($@"ENTRY_DD_{i}", item.ENTRY_DD.strto400DB()); //輸入日期–日
                                    com.Parameters.Add($@"ENTRY_TIME_{i}", item.ENTRY_TIME.strto400DB()); //登打日期 - 時間
                                    com.Parameters.Add($@"ENTRY_ID_{i}", item.ENTRY_ID.strto400DB()); //輸入人員
                                    com.Parameters.Add($@"UPD_YY_{i}", item.UPD_YY.strto400DB()); //異動日期–年 => 民國年
                                    com.Parameters.Add($@"UPD_MM_{i}", item.UPD_MM.strto400DB()); //異動日期–月
                                    com.Parameters.Add($@"UPD_DD_{i}", item.UPD_DD.strto400DB()); //異動日期–日
                                    com.Parameters.Add($@"UPD_ID_{i}", item.UPD_ID.strto400DB()); //異動人員代號 => 五碼ＩＤ
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
                        #endregion

                        #region 修改設定檔
                        if (FPM_updates.Any())
                        {
                            foreach (var item in FPM_updates)
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    sql = $@"
UPDATE FPMCODE0
SET TEXT = :TEXT,
    UPD_YY = :UPD_YY,
    UPD_MM = :UPD_MM,
    UPD_DD = :UPD_DD,
    UPD_ID = :UPD_ID
WHERE GROUP_ID = :GROUP_ID
AND   REF_NO = :REF_NO
AND   SRCE_FROM = :SRCE_FROM
";
                                    com.Parameters.Add($@"TEXT", item.TEXT.strto400DB()); //說明 => 寫退費類別中文進來
                                    com.Parameters.Add($@"UPD_YY", item.UPD_YY.strto400DB()); //異動日期–年 => 民國年
                                    com.Parameters.Add($@"UPD_MM", item.UPD_MM.strto400DB()); //異動日期–月
                                    com.Parameters.Add($@"UPD_DD", item.UPD_DD.strto400DB()); //異動日期–日
                                    com.Parameters.Add($@"UPD_ID", item.UPD_ID.strto400DB()); //異動人員代號 => 五碼ＩＤ
                                    com.Parameters.Add($@"GROUP_ID", item.GROUP_ID.strto400DB()); //組別碼
                                    com.Parameters.Add($@"REF_NO", item.REF_NO.strto400DB()); //參考號碼 => 寫退費類別進來
                                    com.Parameters.Add($@"SRCE_FROM", item.SRCE_FROM.strto400DB()); //資料來源
                                    com.Transaction = transaction;
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                            }

                        }
                        #endregion

                        transaction.Commit();
                        try
                        {
                            using (dbFGLEntities db = new dbFGLEntities())
                            {
                                foreach (var item in db.FGL_PAY_MAIN_HIS.Where(x => pk_ids.Contains(x.pk_id)))
                                {
                                    item.apply_status = "2"; //覆核完成
                                    item.appr_id = userId;
                                    item.appr_datetime = dtn;
                                }
                                try
                                {
                                    db.SaveChanges();
                                    result.DESCRIPTION = MessageType.Audit_Success.GetDescription();
                                    result.RETURN_FLAG = true;
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                                    result.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                            result.DESCRIPTION = MessageType.sys_Error.GetDescription(null, ex.exceptionMessage());
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                result.DESCRIPTION = MessageType.sys_Error.GetDescription(null, ex.exceptionMessage());
            }
            return result;
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="datas">駁回資料</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel RejectedData(IEnumerable<OGL00010ViewModel> datas, string userId)
        {
            MSGReturnModel result = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            try
            {
                if (!datas.Any())
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
                else
                {
                    List<string> pk_ids = datas.Select(x => x.pk_id).ToList();
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        foreach (var item in db.FGL_PAY_MAIN_HIS.Where(x => pk_ids.Contains(x.pk_id)))
                        {
                            if (item.apply_status != "1") //狀態非為 表單申請 代表資料已被異動
                            {
                                result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                return result;
                            }
                            item.apply_status = "3"; //申請狀態 => 3.退回
                            item.appr_id = userId; //執行人員
                            item.appr_datetime = dtn; //執行時間
                        }
                        db.SaveChanges();
                        result.DESCRIPTION = MessageType.Reject_Success.GetDescription();
                        result.RETURN_FLAG = true;
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                result.DESCRIPTION = MessageType.sys_Error.GetDescription(null, ex.exceptionMessage());
            }
            return result;
        }
    }
}