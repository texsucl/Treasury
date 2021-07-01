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
/// 功能說明：抽票原因維護
/// 初版作者：20200121 Mark
/// 修改歷程：20200121 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FAP.Web.Service.Actual
{
    public class OAP0027 : Common, IOAP0027
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public List<OAP0027ViewModel> GetSearchData(OAP0027SearchModel searchModel)
        {
            List<OAP0027ViewModel> resultModel = new List<OAP0027ViewModel>();
            var _data_status = new SysCodeDao().qryByType("AP", "DATA_STATUS");
            using (dbFGLEntities db = new dbFGLEntities())
            {
                resultModel = db.FAP_CODE.AsNoTracking()
                    .Where(x=>x.reason_code == searchModel.reason_code, searchModel.reason_code != "All")
                    .AsEnumerable()
                    .Select(x => new OAP0027ViewModel()
                    {
                        pk_id = x.pk_id,
                        data_status = x.data_status,
                        data_status_value = _data_status.FirstOrDefault(y => y.CODE == x.data_status)?.CODE_VALUE,
                        reason = x.reason,
                        reason_code = x.reason_code,
                        referral_dep = x.referral_dep,
                        create_id = x.create_id,
                        create_datetime = TypeTransfer.dateTimeNToStringNT(x.create_datetime),
                        update_id = x.update_id,
                        update_datetime = TypeTransfer.dateTimeNToStringNT(x.update_datetime),
                        appr_id = x.appr_id,
                        appr_datetime = TypeTransfer.dateTimeNToStringNT(x.appr_datetime)
                    }).ToList();
                var common = new Service.Actual.Common();
                var _fullDepName = common.getFullDepName(resultModel.Select(x => x.referral_dep).Distinct());
                var _allUserId = resultModel.Select(x => x.create_id).ToList();
                _allUserId.AddRange(resultModel.Select(x => x.update_id));
                _allUserId.AddRange(resultModel.Select(x => x.appr_id));
                var userMemo = GetMemoByUserId(_allUserId.Distinct(), true);
                foreach (var item in resultModel)
                {
                    item.referral_dep_name = _fullDepName.First(x => x.Item1 == item.referral_dep).Item2;
                    item.create_id = $@"{item.create_id}({userMemo.FirstOrDefault(y => y.Item1 == item.create_id).Item2})";
                    item.update_id = $@"{item.update_id}({userMemo.FirstOrDefault(y => y.Item1 == item.update_id).Item2})";
                    item.appr_id = $@"{item.appr_id}({userMemo.FirstOrDefault(y => y.Item1 == item.appr_id).Item2})";
                }
            }
            return resultModel;
        }

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="updateDatas">申請資料</param>
        /// <param name="userId">申請人員ID</param>
        /// <returns></returns>
        public MSGReturnModel ApplyDeptData(IEnumerable<OAP0027ViewModel> updateDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            var updateStatus = new List<string>() { "A", "U", "D" };
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _ApplyDeptData = updateDatas.Where(x => updateStatus.Contains(x.exec_action)).ToList();
                if (!_ApplyDeptData.Any())
                {
                    resultModel.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                    return resultModel;
                }
                var _reason_code = _ApplyDeptData.Select(x => x.reason_code).ToList();
                if (db.FAP_CODE_HIS.AsNoTracking()
                    .Where(x => _reason_code.Contains(x.reason_code))
                    .Any(x => x.apply_status == "1")) //異動檔裡面有在申請中的
                {
                    resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                }
                else
                {
                    DateTime dtn = DateTime.Now;
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                    String qPreCode = curDateTime[0];
                    foreach (var item in _ApplyDeptData)
                    {
                        var cId = sysSeqDao.qrySeqNo("AP", "S3", qPreCode).ToString();
                        var _aplyNo = $@"S3{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                        db.FAP_CODE_HIS.Add(new FAP_CODE_HIS()
                        {
                            aply_no = _aplyNo,
                            pk_id = item.exec_action == "A" ? string.Empty : item.pk_id,
                            exec_action = item.exec_action, //A or D
                            apply_status = "1", //表單申請
                            reason = item.reason,
                            reason_code = item.reason_code,
                            referral_dep = item.referral_dep,
                            apply_id = userId,
                            apply_datetime = dtn
                        });
                        if (item.exec_action == "D" || item.exec_action == "U")
                        {
                            var _FAP_CODE = db.FAP_CODE
                                .First(x => x.pk_id == item.pk_id);
                            _FAP_CODE.data_status = "2"; //異動中
                        }
                    }
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
            return resultModel;
        }

        /// <summary>
        /// 檢核重覆資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public bool CheckSameData(OAP0027ViewModel viewModel)
        {
            bool sameFlag = false;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                sameFlag = db.FAP_CODE_HIS.AsNoTracking()
                    .Any(x => x.reason_code == viewModel.reason_code &&
                    x.apply_status == "1"); //有相同 原因代碼  的資料在申請中

                if (!sameFlag && (viewModel.exec_action == "A"))
                    sameFlag = db.FAP_CODE.AsNoTracking()
                        .Any(x => x.reason_code == viewModel.reason_code);  //現行資料已有相同的 原因代碼
            }
            return sameFlag;
        }

        /// <summary>
        /// 查詢抽票原因 代碼 & 中文
        /// </summary>
        /// <param name="valueFlag"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> getData(bool valueFlag = false)
        {
            List<Tuple<string, string>> reason_codes = new List<Tuple<string, string>>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var par = string.Empty;
                foreach (var item in db.FAP_CODE.AsNoTracking().OrderBy(x=>x.reason_code))
                {
                    par = valueFlag ? $@"{item.reason_code} : " : string.Empty;
                    reason_codes.Add(new Tuple<string, string>(item.reason_code, $@"{par}{item.reason}"));
                }
            }
            return reason_codes;
        }
    }
}