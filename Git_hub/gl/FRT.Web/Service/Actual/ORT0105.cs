using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：比對報表勾稽_批次定義(OPEN跨系統勾稽)
/// 初版作者：20210406 Mark
/// 修改歷程：20210406 Mark
///           需求單號：202011050211-28
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{

    public class ORT0105 : Common , IORT0105
    {
        /// <summary>
        /// 查詢 跨系統勾稽作業_批次定義
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel<List<ORT0105ViewModel>> GetSearchData(string type = null, string kind = null)
        {
            MSGReturnModel<List<ORT0105ViewModel>> results = new MSGReturnModel<List<ORT0105ViewModel>>();
            List<ORT0105ViewModel> datas = new List<ORT0105ViewModel>();
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    datas = db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking()
                        .Where(x => x.type == type, !type.IsNullOrWhiteSpace() && type != "All")
                        .Where(x => x.kind == kind, !kind.IsNullOrWhiteSpace() && kind != "All")
                        .AsEnumerable()
                        .Select(x => new ORT0105ViewModel()
                        {
                            type = x.type, //類別
                            kind = x.kind, //性質
                            frequency = x.frequency, //頻率類別
                            frequency_value = x.frequency_value, //頻率參數
                            scheduler_time = x.scheduler_time, //執行時間
                            scheduler_time_hh = x.scheduler_time.Hours, //執行時間(小時)
                            scheduler_time_mm = x.scheduler_time.Minutes, //執行時間(分鐘)
                            start_date_type = x.start_date_type, //資料區間起始日類別
                            start_date_value = x.start_date_value, //資料區間起始日類別參數
                            mail_group = x.mail_group, //Mail群組
                            mail_key = x.mail_key, //MailKey
                            data_status = x.data_status, //資料狀態代碼
                            update_time_compare = x.update_datetime, //最後異動時間
                            check_id = x.check_id, //跨系統勾稽作業 pk_id
                            update_id = x.update_id,
                            update_datetime = x.update_datetime.ToString("yyyy/MM/dd"),
                            appr_id = x.appr_id,
                            appr_datetime = x.appr_datetime.ToString("yyyy/MM/dd")
                        }).ToList();
                    List<SYS_CODE> sys_codes = new List<SYS_CODE>();
                    var emplys = new List<V_EMPLY2>();
                    if (datas.Any())
                    {
                        using (DB_INTRAEntities db_intra = new DB_INTRAEntities())
                        {
                            emplys = db_intra.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
                        }
                        var gojtypes = GetSysCodes("RT", new List<string>() { "GOJ_TYPE" }).Select(x => $@"GOJ_TYPE_{x.CODE}_GROUP").ToList();
                        gojtypes.AddRange(new List<string>() { "DATA_STATUS", "GOJ_TYPE", "GOJ_PLATFORM_TYPE", "MAIL_GROUP", "GOJ_START_TYPE" });
                        sys_codes = GetSysCodes("RT", gojtypes);
                    }
                    foreach (var item in datas)
                    {
                        //類別(中文) 
                        item.type_d = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == item.type)?.CODE_VALUE ?? item.type;
                        //性質(中文) 
                        item.kind_d = sys_codes.FirstOrDefault(x => x.CODE_TYPE == $@"GOJ_TYPE_{item.type}_GROUP" && x.CODE == item.kind)?.CODE_VALUE ?? item.kind;
                        //頻率(中文)
                        item.frequency_d = frequency_d(item.frequency, item.frequency_value);
                        //執行時間(中文)
                        item.scheduler_time_d = item.scheduler_time.ToString(@"hh\:mm");
                        //資料區間起始日(中文)
                        item.start_date_d = start_date_d(item.start_date_type, item.start_date_value);
                        //Mail群組(中文)
                        item.mail_group_d = $@"{sys_codes.Where(x => x.CODE_TYPE == $@"MAIL_GROUP" && x.CODE == item.mail_group).Select(x => $@"{x.CODE_VALUE}").FirstOrDefault() ?? item.mail_group}";
                        //資料狀態(中文)
                        item.data_status_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "DATA_STATUS" && x.CODE == item.data_status)?.CODE_VALUE ?? item.data_status;
                        //修改人員(名子)
                        item.update_name = (emplys.FirstOrDefault(x => x.USR_ID == item.update_id)?.EMP_NAME?.Trim()) ?? item.update_id;
                        //覆核人員(名子)
                        item.appr_name = (emplys.FirstOrDefault(x => x.USR_ID == item.appr_id)?.EMP_NAME?.Trim()) ?? item.appr_id;
                        item.subDatas = db.FRT_CROSS_SYSTEM_CHECK_DETAIL.AsNoTracking()
                            .Where(x => x.check_id == item.check_id)
                            .Select(x => new ORT0105SubViewModel()
                            {
                                platform = x.platform, //平台
                                file_code = x.file_code, //檔案代碼
                                file_code_d = x.file_code_d, //檔案中文
                                memo = x.memo, //備註
                                update_time_cpmpare = x.update_datetime, //最後異動時間
                                sub_id = x.sub_id, //跨系統勾稽作業明細檔_id
                                check_id = x.check_id, //跨系統勾稽作業 pk_id
                            }).ToList();
                    }
                    results.RETURN_FLAG = true;
                    results.Datas = datas;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                results.DESCRIPTION = ex.Message;
            }


            return results;
        }

        /// <summary>
        /// 申請 跨系統勾稽作業_批次定義
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel ApplyDeptData(IEnumerable<ORT0105ViewModel> updateDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var updateStatus = new List<string>() { "A", "U", "D" };
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _ApplyDeptData = updateDatas.Where(x => updateStatus.Contains(x.exec_action)).ToList();
                    if (!_ApplyDeptData.Any())
                    {
                        resultModel.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                        return resultModel;
                    }
                    bool _sameFlag = false;
                    _ApplyDeptData.ForEach(x =>
                    {
                        if (CheckSameData(x))
                        {
                            resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                            _sameFlag = true;
                        }
                    });
                    if (_sameFlag)
                        return resultModel;
                    var check_updates = _ApplyDeptData.Where(x => x.exec_action != "A").ToList();
                    if (check_updates.Any())
                    {
                        foreach (var check in check_updates)
                        {
                            var _check = db.FRT_CROSS_SYSTEM_CHECK
                                .AsNoTracking().First(x => x.check_id == check.check_id);
                            if ((_check.data_status == "2") || (_check.update_datetime > check.update_time_compare))
                            {
                                resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                                return resultModel;
                            }
                        }
                    }
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    String qPreCode = (dtn.Year - 1911).ToString();
                    foreach (var item in _ApplyDeptData)
                    {
                        var _S0105 = sysSeqDao.qrySeqNo("RT", "S0105", qPreCode).ToString();
                        var _his_check_id = $@"S0105{qPreCode}{_S0105.ToString().PadLeft(4, '0')}";

                        #region 主檔
                        var _FRT_CROSS_SYSTEM_CHECK_HIS = new FRT_CROSS_SYSTEM_CHECK_HIS()
                        {
                            type = item.type, //類別
                            kind = item.kind, //性質
                            frequency = item.frequency, //頻率類別
                            frequency_value = item.frequency_value, //頻率參數
                            scheduler_time = item.scheduler_time, //執行時間
                            start_date_type = item.start_date_type, //資料區間起始日類別
                            start_date_value = item.start_date_value, //資料區間起始日類別參數
                            mail_group = item.mail_group, //Mail群組
                            mail_key = item.mail_key, //MailKey
                            apply_id = userId, //申請人員
                            apply_datetime = dtn, //申請時間
                            his_check_id = _his_check_id, //跨系統勾稽作業異動檔_pk_id
                            exec_action = item.exec_action, //A or D or U
                            apply_status = "1", //表單申請
                        };
                        if (item.exec_action == "D" || item.exec_action == "U")
                        {
                            _FRT_CROSS_SYSTEM_CHECK_HIS.check_id = item.check_id;
                            var _FRT_CROSS_SYSTEM_CHECK = db.FRT_CROSS_SYSTEM_CHECK
                                .First(x => x.check_id == item.check_id);
                            _FRT_CROSS_SYSTEM_CHECK.data_status = "2"; //異動中
                        }
                        db.FRT_CROSS_SYSTEM_CHECK_HIS.Add(_FRT_CROSS_SYSTEM_CHECK_HIS);
                        #endregion

                        #region 明細檔

                        List<FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS> _FRT_CROSS_SYSTEM_CHECK_DETAIL_HISs =
                            new List<FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS>();
                        if (item.exec_action == "D")
                        {
                            foreach (var sub in db.FRT_CROSS_SYSTEM_CHECK_DETAIL
                                .AsNoTracking().Where(x => x.check_id == item.check_id))
                            {
                                var _S0106 = sysSeqDao.qrySeqNo("RT", "S0106", qPreCode).ToString();
                                var _his_sub_id = $@"S0106{qPreCode}{_S0106.ToString().PadLeft(4, '0')}";
                                var _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS = new FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS()
                                {
                                    his_sub_id = _his_sub_id, //跨系統勾稽作業明細異動檔_pk_id
                                    platform = sub.platform, //平台
                                    file_code = sub.file_code, //檔案代碼
                                    file_code_d = sub.file_code_d, //檔案中文
                                    memo = sub.memo, //備註
                                    his_check_id = _his_check_id, //跨系統勾稽作業異動檔_pk_id
                                    apply_id = userId, //申請人員
                                    apply_datetime = dtn, //申請時間
                                    sub_id = sub.sub_id, //跨系統勾稽作業明細檔_id
                                    exec_action = item.exec_action //刪除
                                };
                                _FRT_CROSS_SYSTEM_CHECK_DETAIL_HISs.Add(_FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS);
                            }
                        }
                        else if (item.exec_action == "A")
                        {
                            foreach (var sub in item.subDatas)
                            {
                                var _S0106 = sysSeqDao.qrySeqNo("RT", "S0106", qPreCode).ToString();
                                var _his_sub_id = $@"S0106{qPreCode}{_S0106.ToString().PadLeft(4, '0')}";
                                var _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS = new FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS()
                                {
                                    his_sub_id = _his_sub_id, //跨系統勾稽作業明細異動檔_pk_id
                                    platform = sub.platform, //平台
                                    file_code = sub.file_code, //檔案代碼
                                    file_code_d = sub.file_code_d, //檔案中文
                                    memo = sub.memo, //備註
                                    apply_id = userId, //申請人員
                                    apply_datetime = dtn, //申請時間
                                    his_check_id = _his_check_id, //跨系統勾稽作業異動檔_pk_id
                                    exec_action = item.exec_action, //新增
                                };
                                _FRT_CROSS_SYSTEM_CHECK_DETAIL_HISs.Add(_FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS);
                            }
                        }
                        else if (item.exec_action == "U")
                        {
                            foreach (var sub in item.subDatas)
                            {
                                var _S0106 = sysSeqDao.qrySeqNo("RT", "S0106", qPreCode).ToString();
                                var _his_sub_id = $@"S0106{qPreCode}{_S0106.ToString().PadLeft(4, '0')}";
                                var _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS = new FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS();
                                if (sub.exec_action == "A") //新增
                                {
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_sub_id = _his_sub_id; //跨系統勾稽作業明細異動檔_pk_id
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.platform = sub.platform; //平台
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code = sub.file_code; //檔案代碼
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code_d = sub.file_code_d; //檔案中文
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.memo = sub.memo; //備註
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_id = userId; //申請人員
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_datetime = dtn; //申請時間
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_check_id = _his_check_id; //跨系統勾稽作業異動檔_pk_id
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.exec_action = item.exec_action; //新增                                   
                                }
                                else if (sub.exec_action == "U" || sub.exec_action == "D") //修改 or 刪除
                                {
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_sub_id = _his_sub_id; //跨系統勾稽作業明細異動檔_pk_id
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.platform = sub.platform; //平台
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code = sub.file_code; //檔案代碼
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code_d = sub.file_code_d; //檔案中文
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.memo = sub.memo; //備註
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_id = userId; //申請人員
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_datetime = dtn; //申請時間
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_check_id = _his_check_id; //跨系統勾稽作業異動檔_pk_id
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.exec_action = item.exec_action; //修改 or 刪除       
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.sub_id = sub.sub_id; //跨系統勾稽作業明細檔_id
                                }
                                else //無異動
                                {
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_sub_id = _his_sub_id; //跨系統勾稽作業明細異動檔_pk_id
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.platform = sub.platform; //平台
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code = sub.file_code; //檔案代碼
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.file_code_d = sub.file_code_d; //檔案中文
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.memo = sub.memo; //備註
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_id = userId; //申請人員
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.apply_datetime = dtn; //申請時間
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.his_check_id = _his_check_id; //跨系統勾稽作業異動檔_pk_id 
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.exec_action = " "; //無異動
                                    _FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.sub_id = sub.sub_id; //跨系統勾稽作業明細檔_id
                                }
                                _FRT_CROSS_SYSTEM_CHECK_DETAIL_HISs.Add(_FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS);
                            }
                        }
                        db.FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.AddRange(_FRT_CROSS_SYSTEM_CHECK_DETAIL_HISs);
                        #endregion
                    }
                    var validateMessage = db.GetValidationErrors().getValidateString();
                    if (validateMessage.Any())
                    {
                        resultModel.DESCRIPTION = validateMessage;
                    }
                    else {
                        try
                        {
                            db.SaveChanges();
                            resultModel.RETURN_FLAG = true;
                            resultModel.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            resultModel.DESCRIPTION = MessageType.Apply_Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                resultModel.DESCRIPTION = ex.Message;
            }


            return resultModel;
        }

        /// <summary>
        /// 檢核重覆資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public bool CheckSameData(ORT0105ViewModel viewModel)
        {
            bool sameFlag = false;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                sameFlag = db.FRT_CROSS_SYSTEM_CHECK_HIS.AsNoTracking()
                    .Any(x => x.type == viewModel.type && x.kind == viewModel.kind &&
                    x.apply_status == "1"); //有相同 原因代碼  的資料在申請中

                if (!sameFlag && (viewModel.exec_action == "A"))
                    sameFlag = db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking()
                        .Any(x => x.type == viewModel.type && x.kind == viewModel.kind);  //現行資料已有相同的 原因代碼
            }
            return sameFlag;
        }

        /// <summary>
        /// 執行頻率(中文)
        /// </summary>
        /// <param name="frequency">頻率類別</param>
        /// <param name="frequency_value">頻率參數</param>
        /// <returns></returns>
        public string frequency_d(string frequency, int frequency_value)
        {
            string result = string.Empty;

            if (frequency == "m")
                result = $@"執行工作天 : {frequency_value} (月頻率)";
            else if (frequency == "d")
                result = $@"系統基準日 : {frequency_value}";
            else
                result = $@"{frequency} : {frequency_value}";
            return result;
        }

        /// <summary>
        /// 資料區間起始日(中文)
        /// </summary>
        /// <param name="start_date_type">資料區間起始日類別</param>
        /// <param name="start_date_value">資料區間起始日類別參數</param>
        /// <returns></returns>
        public string start_date_d(string start_date_type, string start_date_value)
        {
            string result = string.Empty;
            if (start_date_type == "1")
                result = $@"固定日期 : {start_date_value}";
            else if (start_date_type == "2")
                result = $@"執行前 : {start_date_value} 月";
            else if (start_date_type == "3")
                result = "同系統基準日";
            else if (start_date_type == "4")
                result = $@"執行前 : {start_date_value} 日";
            else
                result = $@"{start_date_type} : {start_date_value}";
            return result;
        }

        public List<FRT_CROSS_SYSTEM_CHECK> getAllCheck()
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                DateTime dtn = DateTime.Now;
                TimeSpan _scheduler_time = new TimeSpan(dtn.Hour,dtn.Minute,0);
                return db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking().Where(x=>x.scheduler_time == _scheduler_time).ToList();
            }
        }


    }
}