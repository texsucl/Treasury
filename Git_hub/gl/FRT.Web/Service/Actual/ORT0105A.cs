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
/// 功能說明：比對報表勾稽_批次覆核(OPEN跨系統勾稽)
/// 初版作者：20210406 Mark
/// 修改歷程：20210406 Mark
///           需求單號：202011050211-28
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORT0105A : Common , IORT0105A
    {
        private ORT0105 ORT0105 = null;
        public ORT0105A()
        {
            ORT0105 = new ORT0105();
        }

        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public MSGReturnModel<List<ORT0105ViewModel>> GetSearchData(string userid)
        {
            MSGReturnModel<List<ORT0105ViewModel>> resultModel = new MSGReturnModel<List<ORT0105ViewModel>>();
            List<ORT0105ViewModel> datas = new List<ORT0105ViewModel>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                datas = db.FRT_CROSS_SYSTEM_CHECK_HIS.AsNoTracking()
                    .Where(x => x.apply_status == "1").AsEnumerable() // 狀態為=>表單申請
                    .Select(x => new ORT0105ViewModel()
                    {
                        exec_action = x.exec_action, //執行功能
                        type = x.type, //類別
                        kind = x.kind, //性質
                        frequency = x.frequency, //頻率類別
                        frequency_value = x.frequency_value, //頻率參數
                        scheduler_time = x.scheduler_time, //執行時間
                        start_date_type = x.start_date_type, //資料區間起始日類別
                        start_date_value = x.start_date_value, //資料區間起始日類別參數
                        mail_group = x.mail_group, //Mail群組
                        mail_key = x.mail_key, //MailKey
                        update_id = x.apply_id, 
                        update_datetime = x.apply_datetime.ToString("yyyy/MM/dd"),
                        his_check_id = x.his_check_id, //跨系統勾稽作業異動檔_pk_id
                        check_id = x.check_id, //跨系統勾稽作業 pk_id
                        review_flag = !(x.apply_id == userid)
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
                    gojtypes.AddRange(new List<string>() { "STATUS", "DATA_STATUS", "GOJ_TYPE", "GOJ_PLATFORM_TYPE", "MAIL_GROUP", "GOJ_START_TYPE" });
                    sys_codes = GetSysCodes("RT", gojtypes);
                }
                foreach (var item in datas)
                {
                    //類別(中文) 
                    item.type_d = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == item.type)?.CODE_VALUE ?? item.type;
                    //性質(中文) 
                    item.kind_d = sys_codes.FirstOrDefault(x => x.CODE_TYPE == $@"GOJ_TYPE_{item.type}_GROUP" && x.CODE == item.kind)?.CODE_VALUE ?? item.kind;
                    //頻率(中文)
                    item.frequency_d = ORT0105.frequency_d(item.frequency, item.frequency_value);
                    //執行時間(中文)
                    item.scheduler_time_d = item.scheduler_time.ToString(@"hh\:mm");
                    //資料區間起始日(中文)
                    item.start_date_d = ORT0105.start_date_d(item.start_date_type, item.start_date_value);
                    //Mail群組(中文)
                    item.mail_group_d = $@"{sys_codes.Where(x => x.CODE_TYPE == $@"MAIL_GROUP" && x.CODE == item.mail_group).Select(x => $@"{x.CODE_VALUE}").FirstOrDefault() ?? item.mail_group}";
                    //資料狀態(中文)
                    item.data_status_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "DATA_STATUS" && x.CODE == item.data_status)?.CODE_VALUE ?? item.data_status;
                    //執行功能(中文)
                    item.exec_action_value = sys_codes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == item.exec_action)?.CODE_VALUE ?? item.exec_action;
                    //修改人員(名子)
                    item.update_name = (emplys.FirstOrDefault(x => x.USR_ID == item.update_id)?.EMP_NAME?.Trim()) ?? item.update_id;

                    item.subDatas = db.FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS.AsNoTracking()
                        .Where(x => x.his_check_id == item.his_check_id)
                        .AsEnumerable()
                        .Select(x => new ORT0105SubViewModel()
                        {
                            exec_action = x.exec_action, //執行功能
                            exec_action_value = (sys_codes.FirstOrDefault(y => y.CODE_TYPE == "STATUS" && y.CODE == x.exec_action)?.CODE_VALUE ?? x.exec_action),
                            platform = x.platform, //平台
                            file_code = x.file_code, //檔案代碼
                            file_code_d = x.file_code_d, //檔案中文
                            memo = x.memo, //備註
                            sub_id = x.sub_id, //跨系統勾稽作業明細檔_id
                            his_sub_id = x.his_sub_id, //跨系統勾稽作業明細異動檔_pk_id
                            his_check_id = x.his_check_id, //跨系統勾稽作業異動檔_pk_id 
                        }).ToList();
                }
                resultModel.RETURN_FLAG = true;
                resultModel.Datas = datas;
            }
            return resultModel;
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="apprDatas">待核可資料</param>
        /// <param name="userId">核可ID</param>
        /// <returns></returns>
        public MSGReturnModel ApprovedData(IEnumerable<ORT0105ViewModel> apprDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;

            var updateStatus = new List<string>() { "A", "U", "D" };
            try
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his_check_ids = apprDatas.Select(x => x.his_check_id).ToList();
                    if (db.FRT_CROSS_SYSTEM_CHECK_HIS.AsNoTracking()
                          .Where(x => his_check_ids.Contains(x.his_check_id))
                          .Any(x => x.apply_status != "1")) //有任一筆資料不為 表單申請 
                    {
                        resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                    }
                    else
                    {
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        String qPreCode = (dtn.Year - 1911).ToString();
                        foreach (var item in db.FRT_CROSS_SYSTEM_CHECK_HIS
                            .Where(x => his_check_ids.Contains(x.his_check_id)))
                        {
                            item.appr_id = userId;
                            item.appr_datetime = dtn;
                            item.apply_status = "2";
                            var _check_id = string.Empty;

                            switch (item.exec_action)
                            {
                                case "A":
                                    var _B0105 = sysSeqDao.qrySeqNo("RT", "B0105", qPreCode).ToString();
                                    _check_id = $@"B0105{qPreCode}{_B0105.ToString().PadLeft(4, '0')}";
                                    db.FRT_CROSS_SYSTEM_CHECK.Add(new FRT_CROSS_SYSTEM_CHECK()
                                    {
                                        check_id = _check_id, //跨系統勾稽作業 pk_id
                                        type = item.type, //類別
                                        kind = item.kind, //性質
                                        frequency = item.frequency, //頻率類別
                                        frequency_value = item.frequency_value, //頻率參數
                                        scheduler_time = item.scheduler_time, //執行時間
                                        start_date_type = item.start_date_type, //資料區間起始日類別
                                        start_date_value = item.start_date_value, //資料區間起始日類別參數
                                        mail_group = item.mail_group, //Mail群組
                                        mail_key = item.mail_key, //MailKey
                                        data_status = "1", //可異動
                                        create_id = item.apply_id,
                                        create_datetime = item.apply_datetime,
                                        update_id = item.apply_id,
                                        update_datetime = item.apply_datetime,
                                        appr_id = userId,
                                        appr_datetime = dtn,                                     
                                    });
                                    foreach (var sub in db.FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS
                                        .Where(x => x.his_check_id == item.his_check_id))
                                    {
                                        var _B0106 = sysSeqDao.qrySeqNo("RT", "B0106", qPreCode).ToString();
                                        var _sub_id = $@"B0106{qPreCode}{_B0106.ToString().PadLeft(4, '0')}";
                                        db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Add(new FRT_CROSS_SYSTEM_CHECK_DETAIL()
                                        {
                                            sub_id = _sub_id,
                                            check_id = _check_id,
                                            platform = sub.platform, //平台
                                            file_code = sub.file_code, //檔案代碼
                                            file_code_d = sub.file_code_d, //檔案中文
                                            memo = sub.memo, //備註
                                            create_id = sub.apply_id,
                                            create_datetime = sub.apply_datetime,
                                            update_id = sub.apply_id,
                                            update_datetime = sub.apply_datetime
                                        });
                                        sub.appr_id = userId;
                                        sub.appr_datetime = dtn;
                                        sub.sub_id = _sub_id;
                                    }
                                    break;
                                case "U":
                                    var _FRT_CROSS_SYSTEM_CHECK = db.FRT_CROSS_SYSTEM_CHECK.Single(x => x.check_id == item.check_id);
                                    _FRT_CROSS_SYSTEM_CHECK.type = item.type;
                                    _FRT_CROSS_SYSTEM_CHECK.kind = item.kind;
                                    _FRT_CROSS_SYSTEM_CHECK.frequency = item.frequency;
                                    _FRT_CROSS_SYSTEM_CHECK.frequency_value = item.frequency_value;
                                    _FRT_CROSS_SYSTEM_CHECK.mail_group = item.mail_group;
                                    _FRT_CROSS_SYSTEM_CHECK.mail_key = item.mail_key;
                                    _FRT_CROSS_SYSTEM_CHECK.scheduler_time = item.scheduler_time;
                                    _FRT_CROSS_SYSTEM_CHECK.start_date_type = item.start_date_type;
                                    _FRT_CROSS_SYSTEM_CHECK.start_date_value = item.start_date_value;
                                    _FRT_CROSS_SYSTEM_CHECK.data_status = "1"; //可異動
                                    _FRT_CROSS_SYSTEM_CHECK.appr_id = userId;
                                    _FRT_CROSS_SYSTEM_CHECK.appr_datetime = dtn;
                                    _FRT_CROSS_SYSTEM_CHECK.update_id = item.apply_id;
                                    _FRT_CROSS_SYSTEM_CHECK.update_datetime = item.apply_datetime;
                                    foreach (var sub in db.FRT_CROSS_SYSTEM_CHECK_DETAIL_HIS
                                        .Where(x => x.his_check_id == item.his_check_id && updateStatus.Contains(x.exec_action)))
                                    {
                                        switch (sub.exec_action)
                                        {
                                            case "A":
                                                var _B0106 = sysSeqDao.qrySeqNo("RT", "B0106", qPreCode).ToString();
                                                var _sub_id = $@"B0106{qPreCode}{_B0106.ToString().PadLeft(4, '0')}";
                                                db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Add(new FRT_CROSS_SYSTEM_CHECK_DETAIL()
                                                {
                                                    sub_id = _sub_id,
                                                    check_id = _check_id,
                                                    platform = sub.platform, //平台
                                                    file_code = sub.file_code, //檔案代碼
                                                    file_code_d = sub.file_code_d, //檔案中文
                                                    memo = sub.memo, //備註
                                                    create_id = sub.apply_id,
                                                    create_datetime = sub.apply_datetime,
                                                    update_id = sub.apply_id,
                                                    update_datetime = sub.apply_datetime
                                                });
                                                sub.sub_id = _sub_id;
                                                break;
                                            case "U":
                                                var _FRT_CROSS_SYSTEM_CHECK_DETAIL = db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Single(x => x.sub_id == sub.sub_id);
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.platform = sub.platform; //平台
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.file_code = sub.file_code; //檔案代碼
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.file_code_d = sub.file_code_d; //檔案中文
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.memo = sub.memo; //備註
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.update_id = sub.apply_id;
                                                _FRT_CROSS_SYSTEM_CHECK_DETAIL.update_datetime = sub.apply_datetime;
                                                break;
                                            case "D":
                                                db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Remove(
                                                    db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Single(x=>x.sub_id == sub.sub_id));
                                                break;
                                        }
                                        sub.appr_id = userId;
                                        sub.appr_datetime = dtn;
                                    }
                                    break;
                                case "D":
                                    db.FRT_CROSS_SYSTEM_CHECK.Remove(
                                        db.FRT_CROSS_SYSTEM_CHECK.Single(x => x.check_id == item.check_id));
                                    db.FRT_CROSS_SYSTEM_CHECK_DETAIL.RemoveRange(
                                        db.FRT_CROSS_SYSTEM_CHECK_DETAIL.Where(x => x.check_id == item.check_id));
                                    break;
                            }
                        }
                        try
                        {
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                resultModel.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                db.SaveChanges();
                                resultModel.RETURN_FLAG = true;
                                resultModel.DESCRIPTION = MessageType.Audit_Success.GetDescription();
                            }
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            resultModel.DESCRIPTION = MessageType.Audit_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
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
        /// 駁回
        /// </summary>
        /// <param name="rejDatas">待駁回資料</param>
        /// <param name="userId">駁回Id</param>
        /// <returns></returns>
        public MSGReturnModel RejectedData(IEnumerable<ORT0105ViewModel> rejDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var his_check_ids = rejDatas.Select(x => x.his_check_id).ToList();
                if (db.FRT_CROSS_SYSTEM_CHECK_HIS.AsNoTracking()
                    .Where(x => his_check_ids.Contains(x.his_check_id)).Any(x => x.apply_status != "1"))  //有任一筆資料不為 表單申請 
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    foreach (var item in db.FRT_CROSS_SYSTEM_CHECK_HIS.Where(x => his_check_ids.Contains(x.his_check_id)))
                    {
                        item.appr_id = userId;
                        item.appr_datetime = dtn;
                        item.apply_status = "3"; //退回/駁回
                        switch (item.exec_action)
                        {
                            case "D":
                            case "U":
                                var _FAP_CODE = db.FRT_CROSS_SYSTEM_CHECK.First(x => x.check_id == item.check_id);
                                _FAP_CODE.data_status = "1";
                                break;
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                        resultModel.RETURN_FLAG = true;
                        resultModel.DESCRIPTION = MessageType.Reject_Success.GetDescription();
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                        resultModel.DESCRIPTION = MessageType.Reject_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                    }
                }
            }
            return resultModel;
        }
    }
}