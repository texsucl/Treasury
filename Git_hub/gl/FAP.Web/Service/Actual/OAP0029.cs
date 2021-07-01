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
/// 功能說明：應付票據抽票結果回覆功能(支票號碼及結果)
/// 初版作者：20200207 Mark
/// 修改歷程：20200207 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FAP.Web.Service.Actual
{
    public class OAP0029 : Common, IOAP0029
    {
        
        string _update_checkNo_flag = "Y"; //設定可以修改checkNo的參數設定

        /// <summary>
        /// 查詢 應付票據抽票結果回覆功能
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<OAP0029ViewModel> GetSearchData(OAP0029SearchModel searchModel,string userId)
        {
            List<OAP0029ViewModel> resultModel = new List<OAP0029ViewModel>();
            List<OAP0021DetailSubModel> subdatas = new List<OAP0021DetailSubModel>();
            DateTime dtn = DateTime.Now;
            //List<string> _applyNos = new List<string>();
            List<string> _REJ_RSNs = new List<string>();
            var _last = string.Empty;
            string sql = string.Empty;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                //_applyNos = db.FAP_CE_APPLY_HIS.AsNoTracking()
                //    .Where(x => x.update_checkNo_flag == _update_checkNo_flag).Select(x => x.apply_no).ToList();
                _REJ_RSNs = db.SYS_CODE.AsNoTracking().Where(x => x.SYS_CD == "AP" && x.CODE_TYPE == "REJ_RSN")
                    .OrderBy(x => x.ISORTBY).Select(x => x.CODE_VALUE).ToList();
                if(_REJ_RSNs.Any())
                    _last = _REJ_RSNs.Last();
            }
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select EH.CHECK_NO, EH.APPLY_NO, EH.REJ_RSN , EH.APLY_NO , EH.APLY_SEQ , CK.STATUS  from FAPPYEH0 EH
join FGLGPCK0 CK
on EH.APLY_NO = CK.APLY_NO
and EH.APLY_SEQ = CK.APLY_SEQ
where EH.STATUS = '3' 
and EH.CE_RPLY_ID = ''
";
                    if (searchModel.send_style != "All")
                    {
                        sql += $@"
and CK.SEND_STYLE = :SEND_STYLE ; ";
                        com.Parameters.Add($@"SEND_STYLE", searchModel.send_style);

                    }
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        var model = new OAP0029ViewModel();
                        model.check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼(抽)
                        model.apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                        model.rej_rsn = dbresult["REJ_RSN"]?.ToString()?.Trim(); //退件原因
                        model.memo = model.rej_rsn;
                        resultModel.Add(model);
                        if(!model.check_no.IsNullOrWhiteSpace())
                            subdatas.Add(new OAP0021DetailSubModel() { check_no = model.check_no });
                    }
                    com.Dispose();
                }
                if (subdatas.Any())
                {
                    new OAP0021().getSubData(subdatas);
                }           
                foreach (var item in resultModel)
                {
                    item.scre_from = getScre_FormInApplyNo(item.apply_no);
                    var _subdatas = subdatas.FirstOrDefault(x => x.check_no == item.check_no);
                    item.amount = _subdatas?.amount; //支票面額
                    item.receiver = _subdatas?.receiver; //支票抬頭
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
            }
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _FAP_CE_APPLY_HISs = db.FAP_CE_APPLY_HIS.AsNoTracking().ToList();
                foreach (var item in resultModel)
                {
                    var _FAP_CE_APPLY_HIS = _FAP_CE_APPLY_HISs.FirstOrDefault(x => x.apply_no == item.apply_no);
                    //修改註記 條件 => 無支票 or 歷程紀錄可更新
                    if (_FAP_CE_APPLY_HIS == null)
                    {
                        var update_checkNo_flag = item.check_no.IsNullOrWhiteSpace() ? _update_checkNo_flag : string.Empty;
                        db.FAP_CE_APPLY_HIS.Add(new FAP_CE_APPLY_HIS()
                        {
                            apply_no = item.apply_no,
                            update_checkNo_flag = update_checkNo_flag,
                            update_id = userId,
                            update_datetime = dtn
                        });
                        item.update_flag = !update_checkNo_flag.IsNullOrWhiteSpace();
                    }
                    else
                    {
                        item.update_flag = !_FAP_CE_APPLY_HIS.update_checkNo_flag.IsNullOrWhiteSpace();
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                }
            }

            return resultModel.OrderByDescending(x=>x.check_no).ThenBy(x=>x.apply_no).ToList();
        }

        /// <summary>
        /// 回覆 成功 or 退件
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel ApplyDeptData(IEnumerable<OAP0029ViewModel> updateDatas, string userId)
        {
            MSGReturnModel result = new MSGReturnModel();
            List<string> updates = new List<string>(); //是否有資料被異動了
            List<string> updateStatus = new List<string>() { "Y", "R"}; //可已更新的狀態 Y:成功, R:退件
            DateTime dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            var _updateDatas = updateDatas.Where(x => updateStatus.Contains(x.ce_result_status)).ToList();
            if (!_updateDatas.Any())
            {
                result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                return result;
            }
            string sql = string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
                 select CE_RPLY_ID, APPLY_NO from FAPPYEH0
                 where STATUS = '3' 
                 and CE_RPLY_ID <> ''
                 and APPLY_NO in ( ";
                    int i = 0;
                    string c = string.Empty;
                    foreach (var item in _updateDatas.Select(x => x.apply_no))
                    {
                        sql += $@" {c} :APPLY_NO_{i} ";
                        com.Parameters.Add($@"APPLY_NO_{i}", item);
                        c = " , ";
                        i += 1;
                    }
                    sql += @" ) ; ";

                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();

                    while (dbresult.Read())
                    {
                        var _CE_RPLY_ID = dbresult["CE_RPLY_ID"]?.ToString()?.Trim(); //抽票回覆人員
                        updates.Add(dbresult["APPLY_NO"]?.ToString()?.Trim() ?? string.Empty); 
                    }
                    com.Dispose();
                }
                if (updates.Any()) //資料已變更
                {
                    result.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        var _FAP_CE_APPLY_HISs = db.FAP_CE_APPLY_HIS.ToList();

                        EacTransaction transaction = conn.BeginTransaction();

                        foreach (var item in _updateDatas)
                        {
                            var _FAP_CE_APPLY_HIS = _FAP_CE_APPLY_HISs.FirstOrDefault(x => x.apply_no == item.apply_no);
                            if (_FAP_CE_APPLY_HIS == null)
                            {
                                db.FAP_CE_APPLY_HIS.Add(new FAP_CE_APPLY_HIS()
                                {
                                    apply_no = item.apply_no,
                                    ce_result = item.ce_result_status,
                                    update_checkNo_flag = item.Ischecked ? _update_checkNo_flag : string.Empty,
                                    update_id = userId,
                                    update_datetime = dtn
                                });
                            }
                            else
                            {
                                _FAP_CE_APPLY_HIS.ce_result = item.ce_result_status;
                                _FAP_CE_APPLY_HIS.update_id = userId;
                                _FAP_CE_APPLY_HIS.update_datetime = dtn;
                            }


                            using (EacCommand com = new EacCommand(conn))
                            {
                                com.Transaction = transaction;
                                sql = $@"
update LAPPYEH1
set 
CE_RPLY_ID = :CE_RPLY_ID,
CE_RPLY_DT = :CE_RPLY_DT,
CE_RPLY_TM = :CE_RPLY_TM,
REJ_RSN = :REJ_RSN
where APPLY_NO = :APPLY_NO ;
";
                                com.Parameters.Add("CE_RPLY_ID", userId); //抽票回覆人員
                                com.Parameters.Add("CE_RPLY_DT", updatedt); //抽票回覆日期
                                com.Parameters.Add("CE_RPLY_TM", updatetm); //抽票回覆時間
                                com.Parameters.Add("REJ_RSN", item.rej_rsn.strto400DB()); //退件原因
                                com.Parameters.Add("APPLY_NO", item.apply_no.strto400DB()); //申請單號
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                        }
                        try
                        {
                            transaction.Commit();
                            db.SaveChanges();
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                            result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null,ex.exceptionMessage());
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 使用支票號碼 獲得資料 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<OAP0029ViewModel> getCheckNo(OAP0029ViewModel searchModel)
        {
            MSGReturnModel<OAP0029ViewModel> resultModel = new MSGReturnModel<OAP0029ViewModel>();
            if (searchModel.apply_no.IsNullOrWhiteSpace() || searchModel.check_no.IsNullOrWhiteSpace())
            {
                resultModel.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            else
            {
                var _datas = new List<OAP0021DetailSubModel>() { new OAP0021DetailSubModel()
                {
                    check_no = searchModel.check_no
                }};
                new OAP0021().getSubData(_datas);
                var _f = _datas.First();
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    searchModel.amount = _f?.amount; //支票面額
                    searchModel.receiver = _f?.receiver; //支票抬頭
                    searchModel.bank_code = _f?.bank_code; //付款帳戶
                    if (_f.system.IsNullOrWhiteSpace())
                    {
                        resultModel.DESCRIPTION = $@"找不到支票號碼:{searchModel.check_no},的資料!!";
                    }
                    else
                    {
                        resultModel.RETURN_FLAG = true;
                        resultModel.Datas = searchModel;
                    }
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 設定支票號碼
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public MSGReturnModel setCheckNo(OAP0029ViewModel model,string userid)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            var dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            if (model.apply_no.IsNullOrWhiteSpace())
            {
                resultModel.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            else
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    var _bank_code = string.Empty;
                    if (!model.check_no.IsNullOrWhiteSpace())
                    {
                        var _datas = new List<OAP0021DetailSubModel>() { new OAP0021DetailSubModel()
                        {
                           check_no = model.check_no
                       }};
                        new OAP0021().getSubData(_datas);
                        var _f = _datas.First();
                        _bank_code = _f.bank_code;
                    }
                    EacTransaction transaction = conn.BeginTransaction();
                    string sql = string.Empty;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"update LAPPYEH1 
                         set CHECK_NO = :CHECK_NO,
                             BANK_CODE = :BANK_CODE
                         where APPLY_NO = :APPLY_NO ; ";
                        com.Parameters.Add("CHECK_NO", model.check_no.strto400DB());
                        com.Parameters.Add("BANK_CODE", _bank_code.strto400DB());
                        com.Parameters.Add("APPLY_NO", model.apply_no.strto400DB());
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                        resultModel.RETURN_FLAG = true;
                    }

                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"update LAPPYED1 
                         set CHECK_NO = :CHECK_NO,
                             BANK_CODE = :BANK_CODE,
                             UPD_ID = :UPD_ID,
                             UPD_DATE = :UPD_DATE,
                             UPD_TIME = :UPD_TIME
                         where APPLY_NO = :APPLY_NO 
                         and   APPLY_SEQ = '1' ; ";
                        com.Parameters.Add("CHECK_NO", model.check_no.strto400DB());
                        com.Parameters.Add("BANK_CODE", _bank_code.strto400DB());
                        com.Parameters.Add("UPD_ID", userid);
                        com.Parameters.Add("UPD_DATE", updatedt);
                        com.Parameters.Add("UPD_TIME", updatetm);
                        com.Parameters.Add("APPLY_NO", model.apply_no.strto400DB());
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                    }
                    try
                    {
                        transaction.Commit();
                        resultModel.RETURN_FLAG = true;
                        resultModel.DESCRIPTION = MessageType.update_Success.GetDescription();
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex);
                        resultModel.DESCRIPTION = MessageType.update_Fail.GetDescription();
                    }          
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 用申請單號碼判斷 申請來源
        /// </summary>
        /// <param name="applyNo"></param>
        /// <returns></returns>
        public string getScre_FormInApplyNo(string applyNo)
        {
            if (applyNo.IsNullOrWhiteSpace() || applyNo.Length < 2)
                return string.Empty;
            var _check = applyNo.Substring(0, 2);
            return _check == "CE" ? "申請" : (_check == "CS" ? "系統" : string.Empty);
        }


    }
}