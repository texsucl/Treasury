using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-定存檢核表項目設定
/// 初版作者：20181107 侯蔚鑫
/// 修改歷程：20181107 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
namespace Treasury.Web.Service.Actual
{
    public class DepChkItem : Common, IDepChkItem
    {
        #region GetData
        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (DepChkItemSearchViewModel)searchModel;
            List<DepChkItemViewModel> result = new List<DepChkItemViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Dep_Chk_Item_HisList = db.DEP_CHK_ITEM_HIS.AsNoTracking()
                    .Where(x => x.APPR_STATUS == "1")
                    .Where(x => x.APPR_DATE == null)
                    .Where(x => x.ACCESS_TYPE != "O").ToList();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Data_Status_Name = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                result.AddRange(db.DEP_CHK_ITEM.AsNoTracking()
                    .Where(x => x.IS_DISABLED == searchData.vIs_Disabled, searchData.vIs_Disabled != "All")
                    .AsEnumerable()
                    .Select((x) => new DepChkItemViewModel()
                    {
                        vAccess_Type = x.ACCESS_TYPE,
                        vIsortby = x.ISORTBY,
                        vExec_Action = _Dep_Chk_Item_HisList.FirstOrDefault(y => y.ACCESS_TYPE == x.ACCESS_TYPE && y.ISORTBY == x.ISORTBY)?.EXEC_ACTION?.Trim(),
                        vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == _Dep_Chk_Item_HisList.FirstOrDefault(z => z.ACCESS_TYPE == x.ACCESS_TYPE && z.ISORTBY == x.ISORTBY)?.EXEC_ACTION?.Trim())?.CODE_VALUE?.Trim(),
                        vDep_Chk_Item_Desc = x.DEP_CHK_ITEM_DESC,
                        vIs_Disabled = x.IS_DISABLED,
                        vItem_Order = x.ITEM_ORDER,
                        vReplace = x.REPLACE,
                        vData_Status = x.DATA_STATUS,
                        vData_Status_Name = _Data_Status_Name.FirstOrDefault(y => y.CODE == x.DATA_STATUS)?.CODE_VALUE?.Trim(),
                        vLast_Update_Dt = x.LAST_UPDATE_DT,
                        vFreeze_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim(),
                        vAply_No = _Dep_Chk_Item_HisList.FirstOrDefault(y => y.ACCESS_TYPE == x.ACCESS_TYPE && y.ISORTBY == x.ISORTBY)?.APLY_NO?.Trim()
                    }).ToList());
            }

            return result;
        }

        /// <summary>
        /// 異動紀錄查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <param name="aply_No">申請單號</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetChangeRecordSearchData(ITinItem searchModel, string aply_No = null)
        {
            var searchData = (DepChkItemChangeRecordSearchViewModel)searchModel;
            List<DepChkItemChangeRecordViewModel> result = new List<DepChkItemChangeRecordViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();
                var _Appr_Status = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();

                if (aply_No.IsNullOrWhiteSpace())
                {

                }
                else
                {
                    result = db.DEP_CHK_ITEM_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .Where(x => x.ACCESS_TYPE == searchData.vAccess_Type, searchData.vAccess_Type != null)
                        .Where(x => x.ISORTBY == searchData.vIsortby, searchData.vIsortby != 0)
                        .AsEnumerable()
                        .Select((x) => new DepChkItemChangeRecordViewModel()
                        {
                            vAply_Date = x.APLY_DATE.ToString("yyyy/MM/dd"),
                            vAply_No = x.APLY_NO,
                            vAply_Uid_Name = !x.APLY_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME.Trim() : null,
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vDep_Chk_Item_Desc = x.DEP_CHK_ITEM_DESC,
                            vDep_Chk_Item_Desc_B = x.DEP_CHK_ITEM_DESC_B,
                            vIs_Disabled_Name = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE.Trim(),
                            vIs_Disabled_B_Name = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED_B)?.CODE_VALUE.Trim(),
                            vItem_Order = x.ITEM_ORDER,
                            vItem_Order_B = x.ITEM_ORDER_B,
                            vReplace = x.REPLACE,
                            vReplace_B = x.REPLACE_B,
                            vAppr_Status_Name = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAppr_Desc = x.APPR_DESC
                        }).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// 依交易別 抓取排序資料
        /// </summary>
        /// <param name="vAccess_Type">交易別</param>
        /// <returns></returns>
        public List<DepChkItemViewModel> GetOrderData(string vAccess_Type)
        {
            var result = new List<DepChkItemViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.DEP_CHK_ITEM.AsNoTracking()
                    .Where(x => x.ACCESS_TYPE == vAccess_Type)
                    .Where(x => x.IS_DISABLED == "N")
                    .AsEnumerable()
                    .Select((x) => new DepChkItemViewModel()
                    {
                        vAccess_Type = x.ACCESS_TYPE,
                        vIsortby = x.ISORTBY,
                        vDep_Chk_Item_Desc = x.DEP_CHK_ITEM_DESC,
                        vItem_Order = x.ITEM_ORDER
                    }).ToList());
            }

            return result;
        }

        /// <summary>
        /// 依交易別 檢查是否有排序資料
        /// </summary>
        /// <param name="vAccess_Type">交易別</param>
        /// <returns></returns>
        public bool CheckOrderData(string vAccess_Type)
        {
            bool _CheckOrderData = false;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _OrderData = db.DEP_CHK_ITEM_HIS
                    .Where(x => x.ACCESS_TYPE == vAccess_Type)
                    .Where(x => x.EXEC_ACTION == "O")
                    .Where(x => x.APPR_STATUS == "1")
                    .ToList();

                if(_OrderData.Any())
                {
                    _CheckOrderData = true;
                }
                else
                {
                    _CheckOrderData = false;
                }
            }

            return _CheckOrderData;
        }
        #endregion

        #region SaveData
        /// <summary>
        /// 金庫進出管理作業-申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITinItem>> TinApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel)
        {
            var searchData = (DepChkItemSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var datas = (List<DepChkItemViewModel>)saveData;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            string _Aply_No = string.Empty;
                            var cId = sysSeqDao.qrySeqNo("G9", qPreCode).ToString().PadLeft(3, '0');
                            _Aply_No = $@"G9{qPreCode}{cId}"; //申請單號 G9+系統日期YYYMMDD(民國年)+3碼流水號
                            string logStr = string.Empty; //log

                            foreach (var item in datas)
                            {
                                int _Isortby = 0, _Item_Order = 0;
                                var _DCI = new DEP_CHK_ITEM();
                                #region 定存檢核表項目設定檔
                                //判斷執行功能
                                switch (item.vExec_Action)
                                {
                                    case "A"://新增
                                        //判斷交易別
                                        if (item.vAccess_Type == "P")
                                        {
                                            _Isortby = sysSeqDao.qrySeqNo("DCI_P", string.Empty);
                                            //_Item_Order = GetItem_Order("P");
                                        }
                                        else if (item.vAccess_Type == "G")
                                        {
                                            _Isortby = sysSeqDao.qrySeqNo("DCI_G", string.Empty);
                                            //_Item_Order = GetItem_Order("G");
                                        }
                                        //_DCI = new DEP_CHK_ITEM()
                                        //{
                                        //    ACCESS_TYPE=item.vAccess_Type,
                                        //    ISORTBY= _Isortby,
                                        //    DEP_CHK_ITEM_DESC=item.vDep_Chk_Item_Desc,
                                        //    IS_DISABLED=item.vIs_Disabled,
                                        //    ITEM_ORDER= _Item_Order,
                                        //    REPLACE=item.vReplace,
                                        //    DATA_STATUS = "2",//凍結中
                                        //    CREATE_UID = searchData.vLast_Update_Uid,
                                        //    CREATE_DT = dt,
                                        //    LAST_UPDATE_UID = searchData.vLast_Update_Uid,
                                        //    LAST_UPDATE_DT = dt,
                                        //    FREEZE_UID = searchData.vLast_Update_Uid,
                                        //    FREEZE_DT = dt
                                        //};
                                        //db.DEP_CHK_ITEM.Add(_DCI);
                                        //logStr += "|";
                                        //logStr += _DCI.modelToString();
                                        break;
                                    case "U"://修改
                                        _DCI = db.DEP_CHK_ITEM.FirstOrDefault(x => x.ACCESS_TYPE == item.vAccess_Type && x.ISORTBY == item.vIsortby);
                                        if (_DCI.LAST_UPDATE_DT != null && _DCI.LAST_UPDATE_DT > item.vLast_Update_Dt)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _Isortby = item.vIsortby;
                                        _Item_Order = _DCI.ITEM_ORDER;
                                        _DCI.DATA_STATUS = "2";//凍結中
                                        _DCI.LAST_UPDATE_UID = searchData.vLast_Update_Uid;
                                        _DCI.LAST_UPDATE_DT = dt;
                                        _DCI.FREEZE_UID = searchData.vLast_Update_Uid;
                                        _DCI.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _DCI.modelToString();
                                        break;
                                    default:
                                        break;
                                }
                                #endregion

                                #region 定存檢核表項目設定異動檔
                                var _DCI_Data = db.DEP_CHK_ITEM.FirstOrDefault(x => x.ACCESS_TYPE == item.vAccess_Type && x.ISORTBY == item.vIsortby);
                                if (_DCI_Data == null)
                                {
                                    var _DCIH = new DEP_CHK_ITEM_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        ACCESS_TYPE = item.vAccess_Type,
                                        ISORTBY= _Isortby,
                                        EXEC_ACTION = item.vExec_Action,
                                        DEP_CHK_ITEM_DESC = item.vDep_Chk_Item_Desc,
                                        IS_DISABLED = item.vIs_Disabled,
                                        ITEM_ORDER = _Item_Order,
                                        REPLACE = item.vReplace,
                                        APPR_STATUS = "1",//表單申請
                                        APLY_UID = searchData.vLast_Update_Uid,
                                        APLY_DATE = dt
                                    };
                                    db.DEP_CHK_ITEM_HIS.Add(_DCIH);
                                    logStr += "|";
                                    logStr += _DCIH.modelToString();
                                }
                                else
                                {
                                    var _DCIH = new DEP_CHK_ITEM_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        ACCESS_TYPE = item.vAccess_Type,
                                        ISORTBY = _Isortby,
                                        EXEC_ACTION = item.vExec_Action,
                                        DEP_CHK_ITEM_DESC = item.vDep_Chk_Item_Desc,
                                        IS_DISABLED = item.vIs_Disabled,
                                        ITEM_ORDER = _Item_Order,
                                        REPLACE = item.vReplace,
                                        APPR_STATUS = "1",//表單申請
                                        DEP_CHK_ITEM_DESC_B = _DCI_Data.DEP_CHK_ITEM_DESC,
                                        IS_DISABLED_B = _DCI_Data.IS_DISABLED,
                                        ITEM_ORDER_B = _DCI_Data.ITEM_ORDER,
                                        REPLACE_B = _DCI_Data.REPLACE,
                                        APLY_UID = searchData.vLast_Update_Uid,
                                        APLY_DATE = dt
                                    };
                                    db.DEP_CHK_ITEM_HIS.Add(_DCIH);
                                    logStr += "|";
                                    logStr += _DCIH.modelToString();
                                }
                                #endregion
                            }

                            #region Save Db
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                result.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                try
                                {
                                    db.SaveChanges();

                                    #region LOG
                                    //新增LOG
                                    Log log = new Log();
                                    log.CFUNCTION = "申請覆核-新增定存檢核表項目";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, searchData.vLast_Update_Uid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_Aply_No}");
                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }

        /// <summary>
        /// 金庫進出管理作業-順序調整申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITinItem>> TinOrderApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel)
        {
            var searchData = (DepChkItemSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var datas = (List<DepChkItemViewModel>)saveData;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            string _Aply_No = string.Empty;
                            var cId = sysSeqDao.qrySeqNo("G9", qPreCode).ToString().PadLeft(3, '0');
                            _Aply_No = $@"G9{qPreCode}{cId}"; //申請單號 G9+系統日期YYYMMDD(民國年)+3碼流水號
                            string logStr = string.Empty; //log

                            foreach (var item in datas)
                            {
                                #region 定存檢核表項目設定異動檔
                                var _DCI_Data = db.DEP_CHK_ITEM.FirstOrDefault(x => x.ACCESS_TYPE == item.vAccess_Type && x.ISORTBY == item.vIsortby);
                                var _DCIH = new DEP_CHK_ITEM_HIS()
                                {
                                    APLY_NO = _Aply_No,
                                    ACCESS_TYPE = item.vAccess_Type,
                                    ISORTBY = item.vIsortby,
                                    EXEC_ACTION = "O",
                                    DEP_CHK_ITEM_DESC = _DCI_Data.DEP_CHK_ITEM_DESC,
                                    IS_DISABLED = _DCI_Data.IS_DISABLED,
                                    ITEM_ORDER = item.vItem_Order,
                                    REPLACE = _DCI_Data.REPLACE,
                                    APPR_STATUS = "1",//表單申請
                                    DEP_CHK_ITEM_DESC_B = _DCI_Data.DEP_CHK_ITEM_DESC,
                                    IS_DISABLED_B = _DCI_Data.IS_DISABLED,
                                    ITEM_ORDER_B = _DCI_Data.ITEM_ORDER,
                                    REPLACE_B = _DCI_Data.REPLACE,
                                    APLY_UID = searchData.vLast_Update_Uid,
                                    APLY_DATE = dt
                                };
                                db.DEP_CHK_ITEM_HIS.Add(_DCIH);
                                logStr += "|";
                                logStr += _DCIH.modelToString();
                                #endregion

                            }

                            #region Save Db
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                result.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                try
                                {
                                    db.SaveChanges();

                                    #region LOG
                                    //新增LOG
                                    Log log = new Log();
                                    log.CFUNCTION = "申請覆核-新增定存檢核表項目";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, searchData.vLast_Update_Uid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_Aply_No}");
                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }

        /// <summary>
        /// 金庫進出管理作業-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="aplyNos">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <param name="desc">覆核意見</param>
        /// <returns></returns>
        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId, string desc)
        {
            foreach (var aplyNo in aplyNos)
            {
                var _DepChkItemHisList = db.DEP_CHK_ITEM_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if(_DepChkItemHisList.Any())
                {
                    foreach(var DepChkItemHis in _DepChkItemHisList)
                    {
                        //定存檢核表項目設定檔
                        var _DepChkItem = db.DEP_CHK_ITEM.FirstOrDefault(x => x.ACCESS_TYPE == DepChkItemHis.ACCESS_TYPE && x.ISORTBY == DepChkItemHis.ISORTBY);
                        if (_DepChkItem != null)
                        {
                            _DepChkItem.DATA_STATUS = "1";//可異動
                            _DepChkItem.APPR_UID = userId;
                            _DepChkItem.APPR_DT = dt;

                            logStr += _DepChkItem.modelToString(logStr);
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, logStr);
                        }

                        //定存檢核表項目設定異動檔
                        var _DepChkItemHis = db.DEP_CHK_ITEM_HIS.FirstOrDefault(x => x.APLY_NO == DepChkItemHis.APLY_NO && x.ACCESS_TYPE == DepChkItemHis.ACCESS_TYPE && x.ISORTBY == DepChkItemHis.ISORTBY);
                        if (_DepChkItemHis != null)
                        {
                            _DepChkItemHis.APPR_STATUS = "3";//退回
                            _DepChkItemHis.APPR_DATE = dt;
                            _DepChkItemHis.APPR_UID = userId;
                            _DepChkItemHis.APPR_DESC = desc;

                            logStr += _DepChkItemHis.modelToString(logStr);
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, logStr);
                        }
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 金庫進出管理作業-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="aplyNos">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <returns></returns>
        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId)
        {
            SysSeqDao sysSeqDao = new SysSeqDao();
            foreach (var aplyNo in aplyNos)
            {
                var _DepChkItemHisList = db.DEP_CHK_ITEM_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if (_DepChkItemHisList.Any())
                {
                    foreach (var DepChkItemHis in _DepChkItemHisList)
                    {
                        //var _Isortby = 0;
                        var _Item_Order = 0;
                        //定存檢核表項目設定檔
                        var _DepChkItem = db.DEP_CHK_ITEM.FirstOrDefault(x => x.ACCESS_TYPE == DepChkItemHis.ACCESS_TYPE && x.ISORTBY == DepChkItemHis.ISORTBY);
                        if (_DepChkItem != null)
                        {
                            _DepChkItem.DATA_STATUS = "1";//可異動
                            _DepChkItem.DEP_CHK_ITEM_DESC = DepChkItemHis.DEP_CHK_ITEM_DESC;
                            _DepChkItem.IS_DISABLED = DepChkItemHis.IS_DISABLED;
                            _DepChkItem.ITEM_ORDER = DepChkItemHis.ITEM_ORDER;
                            _DepChkItem.REPLACE = DepChkItemHis.REPLACE;
                            _DepChkItem.APPR_UID = userId;
                            _DepChkItem.APPR_DT = dt;

                            logStr += _DepChkItem.modelToString(logStr);
                        }
                        else
                        {
                            //判斷交易別
                            if (DepChkItemHis.ACCESS_TYPE == "P")
                            {
                                //_Isortby = sysSeqDao.qrySeqNo("DCI_P", string.Empty);
                                _Item_Order = GetItem_Order("P");
                            }
                            else if (DepChkItemHis.ACCESS_TYPE == "G")
                            {
                                //_Isortby = sysSeqDao.qrySeqNo("DCI_G", string.Empty);
                                _Item_Order = GetItem_Order("G");
                            }
                            var _DCI = new DEP_CHK_ITEM()
                            {
                                ACCESS_TYPE = DepChkItemHis.ACCESS_TYPE,
                                ISORTBY = DepChkItemHis.ISORTBY,
                                DEP_CHK_ITEM_DESC = DepChkItemHis.DEP_CHK_ITEM_DESC,
                                IS_DISABLED = DepChkItemHis.IS_DISABLED,
                                ITEM_ORDER = _Item_Order,
                                REPLACE = DepChkItemHis.REPLACE,
                                DATA_STATUS = "1", //可異動
                                CREATE_UID = DepChkItemHis.APLY_UID,
                                CREATE_DT = dt,
                                LAST_UPDATE_UID = DepChkItemHis.APLY_UID,
                                LAST_UPDATE_DT = dt,
                                APPR_UID = userId,
                                APPR_DT = dt
                            };
                            db.DEP_CHK_ITEM.Add(_DCI);
                            logStr += _DCI.modelToString();

                            db.SaveChanges();
                        }

                        //定存檢核表項目設定異動檔
                        var _DepChkItemHis = db.DEP_CHK_ITEM_HIS.FirstOrDefault(x => x.APLY_NO == DepChkItemHis.APLY_NO && x.ACCESS_TYPE == DepChkItemHis.ACCESS_TYPE && x.ISORTBY == DepChkItemHis.ISORTBY);
                        if (_DepChkItemHis != null)
                        {
                            _DepChkItemHis.APPR_STATUS = "2";//覆核完成
                            _DepChkItemHis.APPR_DATE = dt;
                            _DepChkItemHis.APPR_UID = userId;

                            logStr += _DepChkItemHis.modelToString(logStr);
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, logStr);
                        }
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        //取得項目順序最大值+1
        private int GetItem_Order(string vAccess_Type)
        {
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var MaxItem_Order = db.DEP_CHK_ITEM.AsNoTracking()
                    .Where(x => x.ACCESS_TYPE == vAccess_Type)
                    .Max(x => x.ITEM_ORDER);

                return (int)(MaxItem_Order + 1);
            }
        }
        #endregion
    }
}