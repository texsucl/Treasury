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
/// 功能說明：信封標籤檔案-恢復作業
/// 初版作者：20200302 Mark
/// 修改歷程：20200302 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FAP.Web.Service.Actual
{

    public class OAP0032 : Common, IOAP0032
    {
        /// <summary>
        /// 查詢 信封標籤檔案-恢復作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<List<OAP0031ViewModel>> Search_OAP0032(OAP0032SearchModel searchModel)
        {
            MSGReturnModel<List<OAP0031ViewModel>> result = new MSGReturnModel<List<OAP0031ViewModel>>();
            var update_date_s = TypeTransfer.stringToADDateTimeN(searchModel.update_date);
            var update_date_e = TypeTransfer.stringToADDateTimeN(searchModel.update_date)?.DateToLatestTime();
            if (update_date_s == null || update_date_e == null)
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            else
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var datas = db.FAP_MAIL_LABEL.AsNoTracking()
                        .Where(x => x.update_datetime != null &&
                        x.update_datetime >= update_date_s &&
                        x.update_datetime <= update_date_e)
                        .Where(x => x.label_no != null)
                        .Where(x => x.update_id == searchModel.update_id, !searchModel.update_id.IsNullOrWhiteSpace())
                        .Where(x => x.label_no == searchModel.label_no , !searchModel.label_no.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select(x => new OAP0031ViewModel() {
                            pkid = x.id, //pkid
                            label_no = x.label_no, //標籤號碼
                            bulk_no = x.bulk_no, //大宗掛號號碼
                            update_date = TypeTransfer.dateTimeNToStringNT(x.update_datetime), //異動日期
                            update_id = x.update_id, //異動人員
                            addr = x.addr, //地址
                            rcv_id = x.rcv_id, //收件人
                            memo = x.memo, //備註
                            number = x.number?.ToString(), //張數
                        }).ToList();
                    if (datas.Any())
                    {
                        result.RETURN_FLAG = true;
                        var users = datas.Where(x => !x.rcv_id.IsNullOrWhiteSpace()).Select(x => x.rcv_id).Distinct().ToList();
                        users.AddRange(datas.Where(x => !x.update_id.IsNullOrWhiteSpace()).Select(x => x.update_id).Distinct());
                        var userMemo = GetMemoByUserId(users.Distinct());
                        foreach (var item in datas)
                        {
                            var _rcv_id = userMemo.FirstOrDefault(x => x.Item1 == item.rcv_id)?.Item2;
                            item.rcv_id = _rcv_id.IsNullOrWhiteSpace() ? item.rcv_id : _rcv_id;
                            var _update_id = userMemo.FirstOrDefault(x => x.Item1 == item.update_id)?.Item2;
                            item.update_id = _update_id.IsNullOrWhiteSpace() ? item.update_id : _update_id;
                        }
                        result.Datas = datas;
                        result.RETURN_FLAG = true;
                    }
                    else
                    {
                        result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 清空大宗號碼
        /// </summary>
        /// <param name="label_nos">標籤號碼</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel Clearbulk_no(IEnumerable<string> bulk_nos,string userid)
        {
            MSGReturnModel result = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var _bulk_nos = bulk_nos.ToList();
            //bool _changeFlag = false;
            if (_bulk_nos.Any())
            {
                try
                {
                    var apply_nos = new List<string>();
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        EacTransaction transaction = conn.BeginTransaction();
                        using (dbFGLEntities db = new dbFGLEntities())
                        {
                            foreach (var item in db.FAP_MAIL_LABEL.Where(x => _bulk_nos.Contains(x.bulk_no)))
                            {
                                item.bulk_no = null;
                                item.update_datetime = dtn;
                                item.update_id = userid;
                                //_changeFlag = true;
                                foreach (var sub in db.FAP_MAIL_LABEL_D.AsNoTracking().Where(x => x.id == item.id))
                                {
                                    apply_nos.Add(sub.apply_no);
                                }
                            }
                            if (apply_nos.Any())
                            {
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    com.Transaction = transaction;
                                    string sql = $@" update LAPPYCH1
set
REG_NO = ''
where APPLY_NO in ( ";
                                    string c = string.Empty;
                                    int i = 0;
                                    foreach (var item in apply_nos)
                                    {
                                        sql += $@" {c} :APPLY_NO_{i} ";
                                        com.Parameters.Add($@"APPLY_NO_{i}", item); //申請單號
                                        c = ",";
                                        i += 1;
                                    }
                                    sql += " ) ";
                                    com.CommandText = sql;
                                    com.Prepare();
                                    var updateNum = com.ExecuteNonQuery();
                                    com.Dispose();
                                }
                                transaction.Commit();
                                try
                                {
                                    db.SaveChanges();
                                    result.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                                    result.RETURN_FLAG = true;
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                                    result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                }
            }
            else
            {
                result.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
            }
            return result;
        }

        /// <summary>
        /// 清空標籤號碼
        /// </summary>
        /// <param name="label_nos">標籤號碼</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        public MSGReturnModel Clearlabel_no(IEnumerable<string> label_nos, string userid)
        {
            MSGReturnModel result = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var _label_nos = label_nos.ToList();
            if (_label_nos.Any())
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var datas = db.FAP_MAIL_LABEL.Where(x => _label_nos.Contains(x.label_no)).ToList();
                    if (datas.Any(x => x.bulk_no != null))
                    {
                        result.DESCRIPTION = "待清空標籤號碼資料中有大宗號碼欄位有值!";
                        return result;
                    }
                    foreach (var item in datas)
                    {
                        item.label_no = null;
                        item.update_datetime = dtn;
                        item.update_id = userid;
                    }
                    foreach (var item in db.FAP_MAIL_LABEL_D.Where(x => _label_nos.Contains(x.label_no)))
                    {
                        item.label_no = null;
                    }
                    try
                    {
                        db.SaveChanges();
                        result.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                        result.RETURN_FLAG = true;
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                        result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
            }
            else
            {
                result.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
            }
            return result;
        }
    }
}