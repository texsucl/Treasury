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
/// 功能說明：信封標籤檔案作業及查詢
/// 初版作者：20200302 Mark
/// 修改歷程：20200302 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0031 : Common, IOAP0031
    {
        /// <summary>
        /// 查詢 信封標籤檔案作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public List<OAP0031ViewModel> Search_OAP0031(OAP0031SearchModel searchModel)
        {
            List<OAP0031ViewModel> results = new List<OAP0031ViewModel>();
            var create_date_s = TypeTransfer.stringToADDateTimeN(searchModel.create_date_s);
            var create_date_e = TypeTransfer.stringToADDateTimeN(searchModel.create_date_e)?.DateToLatestTime();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                results.AddRange(
                    db.FAP_MAIL_LABEL.AsNoTracking()
                    .Where(x => x.create_datetime >= create_date_s, create_date_s != null)
                    .Where(x => x.create_datetime <= create_date_e, create_date_e != null)
                    .AsEnumerable()
                    .Where(x => x.label_no.IsNullOrWhiteSpace() , searchModel.label_no_flag != "All" && searchModel.label_no_flag == "Y")
                    .Where(x => !x.label_no.IsNullOrWhiteSpace(), searchModel.label_no_flag != "All" && searchModel.label_no_flag == "N")
                    .Where(x => x.rece_id == searchModel.rece_id, !searchModel.rece_id.IsNullOrWhiteSpace())
                    .Select(x => new OAP0031ViewModel()
                    {
                        pkid = x.id, //PKID
                        label_no = x.label_no, //標籤號碼
                        send_style = x.send_style, //寄送方式
                        send_style_D = x.send_style == "1" ? "直寄" : (x.send_style == "2" ? "轉送" : string.Empty),
                        zip_code = x.zip_code, //郵遞區號
                        addr = x.addr, //地址
                        rcv_id = x.rcv_id, //收件人員
                        memo = x.memo, //備註
                        number = x.number?.ToString(), //張數
                        apply_name = x.apply_name, //行政單位
                        apply_id = x.apply_id, //申請人員
                        bulk_no = x.bulk_no, //大宗掛號號碼,
                        create_date =  x.create_datetime?.ToString("yyyy/MM/dd HH:mm:ss"), //新增日期
                        update_date = x.update_datetime?.ToString("yyyy/MM/dd HH:mm:ss"), //更新日期
                        update_id = x.update_id, //異動人員
                        rece_id = x.rece_id, //接收人員
                    }));
                var users = results.Where(x => !x.rcv_id.IsNullOrWhiteSpace()).Select(x => x.rcv_id).Distinct().ToList();
                foreach (var item in results.Where(x => !x.apply_id.IsNullOrWhiteSpace()).Select(x => x.apply_id).Distinct())
                {
                    users.AddRange(item.Split(','));
                }               
                users.AddRange(results.Where(x => !x.update_id.IsNullOrWhiteSpace()).Select(x => x.update_id).Distinct());
                //users.AddRange(results.Where(x => !x.rece_id.IsNullOrWhiteSpace()).Select(x => x.rece_id).Distinct());
                var userMemo = GetMemoByUserId(users.Distinct());
                var deps = new List<string>();
                foreach (var item in results.Where(x => !x.apply_name.IsNullOrWhiteSpace()).Select(x => x.apply_name).Distinct())
                {
                    deps.AddRange(item.Split(','));
                }
                var fullDepName = getFullDepName(deps);
                foreach (var item in results)
                {
                    var _rcv_id = userMemo.FirstOrDefault(x => x.Item1 == item.rcv_id)?.Item2;
                    item.rcv_id = _rcv_id.IsNullOrWhiteSpace() ? item.rcv_id : _rcv_id;

                    List<string> _apply_ids = new List<string>();
                    item.apply_id = item.apply_id ?? string.Empty;
                    foreach (var i in item.apply_id.Split(','))
                    {
                        var _apply_id = userMemo.FirstOrDefault(x => x.Item1 == i)?.Item2;
                        _apply_ids.Add(_apply_id.IsNullOrWhiteSpace() ? i : _apply_id);
                    }
                    item.apply_id = string.Join(",", _apply_ids.Distinct());
                    //var _apply_id = userMemo.FirstOrDefault(x => x.Item1 == item.apply_id)?.Item2;
                    //item.apply_id = _apply_id.IsNullOrWhiteSpace() ? item.apply_id : _apply_id;

                    var _update_id = userMemo.FirstOrDefault(x => x.Item1 == item.update_id)?.Item2;
                    item.update_id = _update_id.IsNullOrWhiteSpace() ? item.update_id : _update_id;
                    //var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id)?.Item2;
                    //item.rece_id = _rece_id.IsNullOrWhiteSpace() ? item.rece_id : _rece_id;
                    List<string> _apply_name = new List<string>();
                    item.apply_name = item.apply_name ?? string.Empty;
                    foreach (var i in item.apply_name.Split(','))
                    {
                        _apply_name.Add(fullDepName.First(x => x.Item1 == i).Item2);                                        
                    }
                    item.apply_name = string.Join(",",_apply_name.Distinct());
                    //item.apply_name = fullDepName.First(x => x.Item1 == item.apply_name).Item2;
                }
            }
            return results;
        }

        /// <summary>
        /// 產出 標籤號碼
        /// </summary>
        /// <param name="models">傳入資料</param>
        /// <param name="userid">使用者ID</param>
        /// <returns></returns>
        public MSGReturnModel SetLabel_No(IEnumerable<OAP0031ViewModel> models, string userid)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (models.Any(x => !x.label_no.IsNullOrWhiteSpace()))
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return result;
            }
            List<FAP_MAIL_LABEL> FMLs = new List<FAP_MAIL_LABEL>();
            List<FAP_MAIL_LABEL_D> FMLDs = new List<FAP_MAIL_LABEL_D>();
            DateTime dtn = DateTime.Now;
            SysSeqDao sysSeqDao = new SysSeqDao();
            string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
            String qPreCode = curDateTime[0];
            var _count = 0; //轉換信封標籤檔案數量
            List<string> temp_ids = models.Where(x => x.checkFlag).Select(x => x.pkid).ToList();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var datas = db.FAP_MAIL_LABEL
                    .Where(x => x.label_no == null)
                    .Where(x => temp_ids.Contains(x.id)).ToList(); //查詢信封標籤主檔
                var data_ds = db.FAP_MAIL_LABEL_D
                    .Where(x => x.label_no == null)
                    .Where(x => temp_ids.Contains(x.id)).ToList(); //查詢信封標籤明細檔
                if (datas.Any())
                {
                    bool checkFlag = false;
                    foreach (var item in datas)
                    {
                        _count += 1;
                        var _send_style = item.send_style; //寄送方式
                        var cId = string.Empty;
                        string _label_no = null; //標籤號碼
                        // send_type = 1 or 2 (寄送方式 1=> 直寄,2=>轉送) + 系統日期YYYYMMDD(西元年) + 3碼流水號 (取號規則 : S5 or S6)
                        switch (_send_style)
                        {
                            case "1": 
                                cId = sysSeqDao.qrySeqNo("AP", "S5", qPreCode).ToString();
                                _label_no = $@"{_send_style}{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                break;
                            case "2":
                                cId = sysSeqDao.qrySeqNo("AP", "S6", qPreCode).ToString();
                                _label_no = $@"{_send_style}{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                break;
                            default:
                                checkFlag = true;
                                break;
                        }
                        item.label_no = _label_no;
                        item.update_id = userid;
                        item.update_datetime = dtn;
                        foreach (var detail in data_ds.Where(x => x.id == item.id))
                        {
                            detail.label_no = _label_no;
                        }
                    }
                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (!validateMessage.IsNullOrWhiteSpace())
                    {
                        result.DESCRIPTION = validateMessage;
                    }
                    else
                    {
                        if (checkFlag)
                        {
                            result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "資料寄送方式有誤!!");
                        }
                        else
                        {
                            try
                            {
                                db.SaveChanges();
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = MessageType.Exec_Success.GetDescription(null, $@"共轉換信封標籤主檔數量:{_count}");
                            }
                            catch (Exception ex)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                                result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                            }
                        }
                    }
                }
                else
                {
                    result.DESCRIPTION = "無任何需產生標籤的暫存檔檔案";
                }
            }
            return result;
        }

        /// <summary>
        /// 匯入 大宗掛號號碼
        /// </summary>
        /// <param name="models">傳入資料</param>
        /// <param name="userid">使用者ID</param>
        /// <returns></returns>
        public MSGReturnModel Setbulk_no(IEnumerable<OAP0031ViewModel> models, string userid)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var _models = models.ToList();
            bool changFlag = false;
            if (_models.Any())
            {
                try
                {
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        EacTransaction transaction = conn.BeginTransaction();

                        using (dbFGLEntities db = new dbFGLEntities())
                        {
                            var label_nos = _models.Select(x => x.label_no).ToList();
                            
                            foreach (var item in db.FAP_MAIL_LABEL.Where(x => label_nos.Contains(x.label_no)))
                            {
                                var _model = _models.First(x => x.label_no == item.label_no);
                                if (item.bulk_no.IsNullOrWhiteSpace() && !_model.bulk_no.IsNullOrWhiteSpace())
                                {
                                    item.bulk_no = _model.bulk_no?.Trim();
                                    item.update_id = userid;
                                    item.update_datetime = dtn;

                                    foreach (var sub in db.FAP_MAIL_LABEL_D.AsNoTracking().Where(x => x.id == item.id))
                                    {
                                        using (EacCommand com = new EacCommand(conn))
                                        {
                                            com.Transaction = transaction;
                                            string sql =
                                                $@"update LAPPYCH1
set
REG_NO = :REG_NO
where APPLY_NO = :APPLY_NO ";
                                            com.Parameters.Add("REG_NO", item.bulk_no); //掛號號碼
                                            com.Parameters.Add("APPLY_NO", sub.apply_no); //申請單號
                                            com.CommandText = sql;
                                            com.Prepare();
                                            var updateNum = com.ExecuteNonQuery();
                                            com.Dispose();
                                        }
                                    }
                                    changFlag = true;
                                }
                            }
                            if (changFlag)
                            {
                                transaction.Commit();
                                try
                                {
                                    db.SaveChanges();
                                    resultModel.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                                    resultModel.RETURN_FLAG = true;
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                                    resultModel.DESCRIPTION = MessageType.Exec_Fail.GetDescription();
                                }
                            }
                            else
                            {
                                resultModel.DESCRIPTION = "無比對到可異動資料!";
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
                    resultModel.DESCRIPTION = MessageType.Exec_Fail.GetDescription();
                }
            }
            else
            {
                resultModel.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            }
            return resultModel;
        }
    }
}