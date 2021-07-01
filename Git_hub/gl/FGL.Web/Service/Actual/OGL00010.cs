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
/// 功能說明：退費類別維護
/// 初版作者：20200708 Mark
/// 修改歷程：20200708 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FGL.Web.Service.Actual
{
    public class OGL00010 : Common , IOGL00010
    {
        /// <summary>
        /// 查詢 退費類別維護
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <returns></returns>
        public MSGReturnModel<List<OGL00010ViewModel>> GetSearchData(string payclass)
        {
            MSGReturnModel<List<OGL00010ViewModel>> result = new MSGReturnModel<List<OGL00010ViewModel>>();
            result.Datas = new List<OGL00010ViewModel>();
            List<FRTITEM0> FRTITEM_datas = new List<FRTITEM0>();
            List<OGL00010ViewModel> all_datas = new List<OGL00010ViewModel>();
            List<FGL_PAY_MAIN_HIS> main_datas = new List<FGL_PAY_MAIN_HIS>();
            List<FGL_PAY_SUB_HIS> sub_datas = new List<FGL_PAY_SUB_HIS>();
            List<SYS_CODE> sys_codes = new List<SYS_CODE>();
            Dictionary<string, string> memos = new Dictionary<string, string>();

            try
            {
                #region 查詢 AS400 資料
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
SELECT PAY_CLASS,ITEM_YN,YEAR_YN,PREM_YN,UNIT_YN,RECP_YN,CONT_YN,CORP_YN,PREM_KIND,CONT_TYPE,PROD_TYPE,CORP_NO,
ACTNUM_YN,ACCT_CODE,ACCT_CODEF,ACCT_CODER,UPD_DATE,UPD_TIME  FROM FRTITEM0
";
                        if (!payclass.IsNullOrWhiteSpace())
                        {
                            sql += $@" where PAY_CLASS = :PAY_CLASS ";
                            com.Parameters.Add($@"PAY_CLASS", payclass.Trim()); //退費項目類別
                        }
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            FRTITEM0 _data = new FRTITEM0();
                            _data.PAY_CLASS = dbresult["PAY_CLASS"]?.ToString()?.Trim(); //退費項目類別
                            _data.ITEM_YN = dbresult["ITEM_YN"]?.ToString()?.Trim(); //險種否
                            _data.YEAR_YN = dbresult["YEAR_YN"]?.ToString()?.Trim(); //年次否
                            _data.PREM_YN = dbresult["PREM_YN"]?.ToString()?.Trim(); //保費類別否
                            _data.UNIT_YN = dbresult["UNIT_YN"]?.ToString()?.Trim(); //費用單位否
                            _data.RECP_YN = dbresult["RECP_YN"]?.ToString()?.Trim(); //送金單否
                            _data.CONT_YN = dbresult["CONT_YN"]?.ToString()?.Trim(); //合約別否
                            _data.CORP_YN = dbresult["CORP_YN"]?.ToString()?.Trim(); //帳本否
                            _data.PREM_KIND = dbresult["PREM_KIND"]?.ToString()?.Trim(); //保費類別
                            _data.CONT_TYPE = dbresult["CONT_TYPE"]?.ToString()?.Trim(); //合約別
                            _data.PROD_TYPE = dbresult["PROD_TYPE"]?.ToString()?.Trim(); //商品別
                            _data.CORP_NO = dbresult["CORP_NO"]?.ToString()?.Trim(); //帳本別
                            _data.ACTNUM_YN = dbresult["ACTNUM_YN"]?.ToString()?.Trim(); //取會科否
                            _data.ACCT_CODE = dbresult["ACCT_CODE"]?.ToString()?.Trim(); //保費收入首年首期
                            _data.ACCT_CODEF = dbresult["ACCT_CODEF"]?.ToString()?.Trim(); //保費收入首年續期
                            _data.ACCT_CODER = dbresult["ACCT_CODER"]?.ToString()?.Trim(); //續年度
                            _data.UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(); //異動日期
                            _data.UPD_TIME = dbresult["UPD_TIME"]?.ToString()?.Trim(); //異動時間
                            FRTITEM_datas.Add(_data);
                        }
                        com.Dispose();
                    }
                    if (FRTITEM_datas.Any())
                    {
                        using (EacCommand com = new EacCommand(conn))
                        {
                            string sql = $@"
select REF_NO,TEXT from FPMCODE0
where SRCE_FROM = 'RT'
and GROUP_ID  = 'RM-CODE'
";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                var _REF_NO = dbresult["REF_NO"]?.ToString()?.Trim(); //退費項目類別
                                var _TEXT = dbresult["TEXT"]?.ToString()?.Trim(); //備註
                                memos.Add(_REF_NO, _TEXT);
                            }
                            com.Dispose();
                        }
                    }
                }
                foreach (var items in FRTITEM_datas.GroupBy(x => new {
                    x.PAY_CLASS, //退費項目類別
                    x.ITEM_YN,   //險種否
                    x.YEAR_YN,   //年次否
                    x.PREM_YN,   //保費類別否
                    x.UNIT_YN,   //費用單位否
                    x.RECP_YN,   //送金單否
                    x.CONT_YN,   //合約別否
                    x.CORP_YN    //帳本否
                }).OrderBy(x=>x.Key.PAY_CLASS))
                {
                    var _data = new OGL00010ViewModel();
                    _data.pk_id = Guid.NewGuid().ToString();
                    _data.pay_class = items.Key.PAY_CLASS; //退費項目類別
                    _data.item_yn_o = items.Key.ITEM_YN; //險種否_舊
                    _data.item_yn_n = items.Key.ITEM_YN; //險種否_新
                    _data.year_yn_o = items.Key.YEAR_YN; //年次否_舊
                    _data.year_yn_n = items.Key.YEAR_YN; //年次否_新
                    _data.prem_yn_o = items.Key.PREM_YN; //保費類別否_舊
                    _data.prem_yn_n = items.Key.PREM_YN; //保費類別否_新
                    _data.unit_yn_o = items.Key.UNIT_YN; //費用單位否_舊
                    _data.unit_yn_n = items.Key.UNIT_YN; //費用單位否_新
                    _data.recp_yn_o = items.Key.RECP_YN; //送金單否_舊
                    _data.recp_yn_n = items.Key.RECP_YN; //送金單否_新
                    _data.cont_yn_o = items.Key.CONT_YN; //合約別否_舊
                    _data.cont_yn_n = items.Key.CONT_YN; //合約別否_新
                    _data.corp_yn_o = items.Key.CORP_YN; //帳本否_舊
                    _data.corp_yn_n = items.Key.CORP_YN; //帳本否_新
                    _data.LastDateTime = items.Select(x => x.UPD_DATE + x.UPD_TIME).First();
                    var _memo = string.Empty;
                    if (memos.TryGetValue(_data.pay_class, out _memo))
                    {
                        _data.memo_o = _memo; //備註_舊
                        _data.memo_n = _memo; //備註_新
                    }                   
                    _data.data_status = "1"; //可異動
                    List<OGL00010ViewSubModel> _sub_data = new List<OGL00010ViewSubModel>();
                    foreach (var item in items)
                    {
                        _sub_data.Add(new OGL00010ViewSubModel()
                        {
                            pk_id = Guid.NewGuid().ToString(),
                            pay_class = items.Key.PAY_CLASS, //退費項目類別
                            prem_kind_o = item.PREM_KIND, //保費類別_舊
                            prem_kind_n = item.PREM_KIND, //保費類別_新
                            cont_type_o = item.CONT_TYPE, //合約別_舊
                            cont_type_n = item.CONT_TYPE, //合約別_新
                            prod_type_o = item.PROD_TYPE, //商品別_舊
                            prod_type_n = item.PROD_TYPE, //商品別_新
                            corp_no_o = item.CORP_NO, //帳本別_舊
                            corp_no_n = item.CORP_NO, //帳本別_新
                            actnum_yn_o = item.ACTNUM_YN, //取會科否_舊
                            actnum_yn_n = item.ACTNUM_YN, //取會科否_新
                            acct_code_o = item.ACCT_CODE, //保費收入首年首期_舊
                            acct_code_n = item.ACCT_CODE, //保費收入首年首期_新
                            acct_codef_o = item.ACCT_CODEF, //保費收入首年續期_舊
                            acct_codef_n = item.ACCT_CODEF, //保費收入首年續期_新
                            acct_coder_o = item.ACCT_CODER, // 續年度_舊     
                            acct_coder_n = item.ACCT_CODER, // 續年度_新           
                        });
                    }
                    _data.SubDatas = _sub_data;
                    all_datas.Add(_data);
                }
                #endregion

                #region 查詢 Ms sql 資料
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

                    main_datas = db.FGL_PAY_MAIN_HIS.AsNoTracking()
                        .Where(x => x.apply_status == "1").ToList();

                    var _AddData = main_datas.Where(x => x.exec_action == "A").ToList();

                    var ids = _AddData.Select(x => x.pk_id).ToList();

                    //foreach (var item in _AddData)
                    //{
                    //    all_datas.Add(new OGL00010ViewModel()
                    //    {
                    //        pk_id = Guid.NewGuid().ToString(),
                    //        SubDatas = db.FGL_PAY_SUB_HIS.AsNoTracking()
                    //                     .Where(x => ids.Contains(x.main_pk_id)).Select(x=>new OGL00010ViewSubModel() {

                    //                     }).ToList()
                    //    });
                    //}

                }
                #endregion

                #region 處理資料
                all_datas.ForEach(x =>
                {
                    var _main = main_datas.FirstOrDefault(y => y.pay_class == x.pay_class);
                    if (_main != null)
                    {
                        x.data_status = "2";
                        x.exec_action = _main.exec_action;     
                        x.exec_action_D = getCodeValue(sys_codes, "EXEC_ACTION", x.exec_action);
                        x.pk_id = _main.pk_id;
                    }
                    x.data_status_D = getCodeValue(sys_codes, "DATA_STATUS", x.data_status);
                    x.SubDatas.ForEach(z =>
                    {
                        z.prem_kind_n_D = getCodeValue(sys_codes, "PREM_KIND", z.prem_kind_n);
                        z.cont_type_n_D = getCodeValue(sys_codes, "CONT_TYPE", z.cont_type_n);
                        z.prod_type_n_D = getCodeValue(sys_codes, "PROD_TYPE", z.prod_type_n);
                    });
                });
                #endregion

                if (all_datas.Any())
                {
                    result.RETURN_FLAG = true;
                    result.Datas = all_datas;
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                result.DESCRIPTION = MessageType.sys_Error.GetDescription(null,ex.exceptionMessage());
            }
            return result;
        }

        /// <summary>
        /// 申請異動資料
        /// </summary>
        /// <param name="applyDatas">異動資料</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel ApplyData(IEnumerable<OGL00010ViewModel> applyDatas, string userid)
        {
            MSGReturnModel result = new MSGReturnModel();
            List<FGL_PAY_MAIN_HIS> main_datas = new List<FGL_PAY_MAIN_HIS>();
            List<FGL_PAY_SUB_HIS> sub_datas = new List<FGL_PAY_SUB_HIS>();
            if (!applyDatas.Any())
            {
                result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                return result;
            }
            try
            {
                SysSeqDao sysSeqDao = new SysSeqDao();
                var dtn = DateTime.Now;
                String qPreCode = dtn.ToString("yyyyMMdd");
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    foreach (var x in applyDatas)
                    {
                        if (db.FGL_PAY_MAIN_HIS.AsNoTracking()
                            .FirstOrDefault(y=>y.pay_class == x.pay_class &&
                            y.apply_status == "1") != null) //該筆退費項目類別目前是在申請狀態
                        {
                            result.DESCRIPTION = MessageType.already_Change.GetDescription(null,$@"退費項目類別:{x.pay_class}, 已在申請中!");
                            return result;
                        }
                        var _m_id = $@"PAY1{qPreCode}{sysSeqDao.qrySeqNo("FGL", "PAY1", qPreCode).ToString().PadLeft(3, '0')}";
                        main_datas.Add(new FGL_PAY_MAIN_HIS()
                        {
                            pk_id = _m_id,
                            pay_class = x.pay_class, //退費項目類別
                            memo_n = x.memo_n,  //備註_新
                            memo_o = x.memo_o, // //備註_舊
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
                            apply_status = x.apply_status, //申請狀態
                            apply_id = userid, //執行人員
                            apply_datetime = dtn //執行時間
                        });
                        x.SubDatas.ForEach(y =>
                        {
                            var _exec_action = y.exec_action;
                            if (x.exec_action == "A")
                                _exec_action = "A";
                            else if(x.exec_action == "D")
                                _exec_action = "D";
                            sub_datas.Add(new FGL_PAY_SUB_HIS()
                            {
                                main_pk_id = _m_id,
                                pk_id = $@"PAY2{qPreCode}{sysSeqDao.qrySeqNo("FGL", "PAY2", qPreCode).ToString().PadLeft(3, '0')}",
                                pay_class = x.pay_class, //退費項目類別
                                prem_kind_n = y.prem_kind_n, //保費類別_新
                                prem_kind_o = y.prem_kind_o, //保費類別_舊
                                cont_type_n = y.cont_type_n, //合約別_新
                                cont_type_o = y.cont_type_o, //合約別_舊
                                prod_type_n = y.prod_type_n, //商品別_新
                                prod_type_o = y.prod_type_o, //商品別_舊
                                corp_no_n = y.corp_no_n, //帳本別_新
                                corp_no_o = y.corp_no_o, //帳本別_舊
                                actnum_yn_n = y.actnum_yn_n, //取會科否_新
                                actnum_yn_o = y.actnum_yn_o, //取會科否_舊
                                acct_code_n = y.acct_code_n, //保費收入首年首期_新
                                acct_code_o = y.acct_code_o, //保費收入首年首期_舊
                                acct_codef_n = y.acct_codef_n, //保費收入首年續期_新
                                acct_codef_o = y.acct_codef_o, //保費收入首年續期_舊
                                acct_coder_n = y.acct_coder_n, //續年度_新
                                acct_coder_o = y.acct_coder_o, //續年度_舊
                                exec_action = _exec_action, //執行動作
                            });
                        });
                    }
                    db.FGL_PAY_MAIN_HIS.AddRange(main_datas);
                    db.FGL_PAY_SUB_HIS.AddRange(sub_datas);
                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (validateMessage.Any())
                    {
                        result.DESCRIPTION = validateMessage;
                    }
                    else
                    {
                        db.SaveChanges();
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription();
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                result.DESCRIPTION = MessageType.Apply_Audit_Fail.GetDescription(null, ex.exceptionMessage());
            }
            return result;
        }

        /// <summary>
        /// 檢核 是否有該退費項目類別 (True => 有重複資料)
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <param name="action">動作</param>
        /// <returns></returns>
        public bool CheckData(string payclass,string action)
        {
            bool result = false;

            #region 查詢400資料是否有重複
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = $@"
SELECT PAY_CLASS FROM FRTITEM0
";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read() && !result)
                    {
                        var _PAY_CLASS = dbresult["PAY_CLASS"]?.ToString()?.Trim(); //退費項目類別
                        if (_PAY_CLASS == payclass && action == "A") //新增時不能有重複
                            result = true;
                    }
                    com.Dispose();
                }
            }
            #endregion

            #region 查詢申請資料是否有重複
            if (!result)
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    result = db.FGL_PAY_MAIN_HIS.AsNoTracking()
                        .Any(x => x.pay_class == payclass && x.apply_status == "1");
                    //於申請表內有相同的退費項目類別在申請中
                }
            }
            #endregion

            return result;
        }
    }
}