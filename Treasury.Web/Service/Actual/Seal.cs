using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using Treasury.Web.Controllers;
/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 印章
/// 初版作者：20180716 張家華
/// 修改歷程：20180716 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Service.Actual
{
    public class Seal : Common, ISeal
    {


        public Seal()
        {

        }

        #region Get Date

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<SealViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<SealViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去印章庫存資料檔抓取資料
                    var details = db.ITEM_SEAL.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        bool _accessStatus = (_TAR.APLY_STATUS == Ref.AccessProjectFormStatus.E01.ToString()) && (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString());
                        result = GetDetailModel(details, _Inventory_types, _accessStatus).ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="itemId">物品標號</param>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        public List<SealViewModel> GetDbDataByUnit(string itemId, string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<SealViewModel>();
            List<string> _itemIds = new List<string>();
            _itemIds.Add(itemId);
            if (itemId == Ref.TreaItemType.D1008.ToString())
                _itemIds.Add(Ref.TreaItemType.D1005.ToString());
            if (itemId == Ref.TreaItemType.D1009.ToString())
                _itemIds.Add(Ref.TreaItemType.D1006.ToString());
            if (itemId == Ref.TreaItemType.D1010.ToString())
                _itemIds.Add(Ref.TreaItemType.D1007.ToString());
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_SEAL.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1" && _itemIds.Contains(x.TREA_ITEM_NAME)) //庫存
                    .AsEnumerable()
                    .Select(x => 
                    new SealViewModel() {
                        vItemId = x.ITEM_ID,
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vSeal_Desc = x.SEAL_DESC,
                        vMemo = x.MEMO,
                        vtakeoutFlag = false,
                        vTrea_Item_Name = x.TREA_ITEM_NAME,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                        result.AddRange(db.ITEM_SEAL.AsNoTracking().Where(
                        x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                        .Select(x =>
                         new SealViewModel()
                         {
                             vItemId = x.ITEM_ID,
                             vStatus = Ref.AccessInventoryType._4.GetDescription(),
                             vSeal_Desc = x.SEAL_DESC,
                             vMemo = x.MEMO,
                             vtakeoutFlag = true,
                             vTrea_Item_Name = x.TREA_ITEM_NAME,
                             vLast_Update_Time = x.LAST_UPDATE_DT
                         }));
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢 CDC 股票資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="aply_No">資料庫異動申請單紀錄檔  INVENTORY_CHG_APLY 單號</param>
        /// <returns></returns>
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null, string charge_Dept = null, string charge_Sect = null)
        {
            List<CDCSealViewModel> result = new List<CDCSealViewModel>();
            
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var PUT_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_From);
                    var PUT_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_To).DateToLatestTime();
                    var GET_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_From);
                    var GET_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_To).DateToLatestTime();
                    result.AddRange(db.ITEM_SEAL.AsNoTracking()
                        .Where(x => x.TREA_ITEM_NAME == searchModel.vJobProject, searchModel.vJobProject != "All")
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.CHARGE_DEPT == charge_Dept, !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect, !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select((x) => new CDCSealViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vTrea_Item_Name = x.TREA_ITEM_NAME,
                            vStatus = x.INVENTORY_STATUS,
                            vPUT_Date = x.PUT_DATE_ACCESS?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAPLY_UID = x.APLY_UID,
                            vAPLY_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vSeal_Desc = x.SEAL_DESC,
                            vSeal_Desc_AFT = x.SEAL_DESC_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidName(itemIds);
                            result.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.Where(z => z.confirmDate != null)
                                .OrderByDescending(z => z.confirmDate)
                                .FirstOrDefault(y => y.itemId == x.vItemId)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_SEAL.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCSealViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPUT_Date = x.PUT_DATE_ACCESS?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAPLY_UID = x.APLY_UID,
                            vAPLY_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vSeal_Desc = x.SEAL_DESC,
                            vSeal_Desc_AFT = x.SEAL_DESC_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                result.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCharge_Sect_Name.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name : x.vCharge_Dept_Name;
                    x.vCharge_Name_AFT = !x.vCharge_Sect_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name_AFT : (!x.vCharge_Dept_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Dept_Name_AFT : null);
                });
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 申請覆核 印章
        /// </summary>
        /// <param name="insertDatas">資料</param>
        /// <param name="taData">申請單資料</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITreaItem>> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;
            try
            {
                if (insertDatas != null)
                {
                    var datas = (List<SealViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            var item_Seq = "E4"; //印章流水號開頭編碼    
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (CustodyAppr.Contains(_TAR.APLY_STATUS))
                                {
                                    _APLY_STATUS = CustodyConfirmStatus;
                                    _TAR.CUSTODY_UID = AccountController.CurrentUserId; //保管單位直接帶使用者
                                    _TAR.CUSTODY_DT = dt;
                                }
                                else
                                {
                                    if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                        _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                }
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                var _ARH = new APLY_REC_HIS()
                                {
                                    APLY_NO = taData.vAplyNo,
                                    APLY_STATUS = _APLY_STATUS,
                                    PROC_DT = dt,
                                    PROC_UID = taData.vCreateUid
                                };
                                logStr += _ARH.modelToString(logStr);
                                db.APLY_REC_HIS.Add(_ARH);
                                #endregion

                                #region 印章庫存資料檔

                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId 
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<string> cancelItemIds = new List<string>(); //取消 itemId
                                List<ITEM_SEAL> inserts = new List<ITEM_SEAL>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IS_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _IS = new ITEM_SEAL();

                                            if (item.vItemId.StartsWith(item_Seq))
                                            {
                                                _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IS.TREA_ITEM_NAME = taData.vItem; //申請項目
                                                _IS.SEAL_DESC = item.vSeal_Desc; //印章內容
                                                _IS.MEMO = item.vMemo; //備註說明
                                                updateItemIds.Add(item.vItemId);

                                                logStr += _IS.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');                                               
                                                _IS = new ITEM_SEAL()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    TREA_ITEM_NAME = taData.vItem, //申請項目
                                                    SEAL_DESC = item.vSeal_Desc, //印章內容
                                                    MEMO = item.vMemo, //備註說明
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                                               //PUT_DATE = dt, //存入日期時間
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };
                                                _IS_Item_Id = _IS.ITEM_ID;
                                                inserts.Add(_IS);
                                                logStr += _IS.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                    {
                                        var _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vtakeoutFlag)
                                        {
                                            if (_IS.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _IS.INVENTORY_STATUS = "4"; //預約取出
                                                _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IS.ITEM_ID);
                                                logStr += _IS.modelToString(logStr);
                                            }
                                            else if (_IS.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_IS.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IS.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _IS.INVENTORY_STATUS = "1"; //預約取出
                                                _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                logStr += _IS.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_SEAL.RemoveRange(db.ITEM_SEAL.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_SEAL.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                {
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x
                                    }));
                                }
                                #endregion
                            }
                            else
                            {
                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion

                                #region 印章庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IS_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _IS = new ITEM_SEAL()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                TREA_ITEM_NAME = taData.vItem, //申請項目
                                                SEAL_DESC = item.vSeal_Desc, //印章內容
                                                MEMO = item.vMemo, //備註說明
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                                           //PUT_DATE = dt, //存入日期時間
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _IS_Item_Id = _IS.ITEM_ID;
                                            db.ITEM_SEAL.Add(_IS);
                                            logStr += _IS.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            var _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IS_Item_Id = _IS.ITEM_ID;
                                            if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IS.INVENTORY_STATUS = "4"; //預約取出
                                                                        //_IRE.GET_DATE = dt; //取出日期時間
                                            _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                            logStr += _IS.modelToString(logStr);
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    if (!_IS_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IS_Item_Id
                                        });
                                    }
                                    #endregion
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
                                        log.CFUNCTION = "申請覆核-新增印章";
                                        log.CACTION = "A";
                                        log.CCONTENT = logStr;
                                        LogDao.Insert(log, taData.vCreateUid);
                                        #endregion

                                        result.RETURN_FLAG = true;
                                        result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}");
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
        /// 作廢
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> ObSolete(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            var itemIds = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態印章庫存資料檔要復原
            {
                foreach (var item in db.ITEM_SEAL.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_SEAL.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "7"; //改為已取消
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
        }

        /// <summary>
        /// 刪除申請
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CancelApply(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            var otherItemAplys = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == aply_No).ToList();
            var itemIds = otherItemAplys.Select(y => y.ITEM_ID).ToList();
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態印章庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_SEAL.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除印章庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_SEAL.RemoveRange(db.ITEM_SEAL.Where(x => itemIds.Contains(x.ITEM_ID)));
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
        }

        /// <summary>
        /// 庫存異動資料-申請覆核
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ICDCItem>> CDCApplyAudit(IEnumerable<ICDCItem> saveData, CDCSearchViewModel searchModel)
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            string logStr = string.Empty;
            DateTime dt = DateTime.Now;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                bool changFlag = false;
                var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt);
                logStr = _data.Item2;
                foreach (CDCSealViewModel model in saveData)
                {
                    var _seal = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                    if (_seal != null && !changFlag)
                    {
                        if (_seal.LAST_UPDATE_DT > model.vLast_Update_Time || _seal.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }
                        if (!changFlag)
                        {
                            _seal.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _seal.SEAL_DESC_AFT = model.vSeal_Desc_AFT;
                            _seal.MEMO_AFT = model.vMemo_AFT;
                            _seal.LAST_UPDATE_DT = dt;

                            logStr = _seal.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _seal.ITEM_ID
                            };

                            db.OTHER_ITEM_APLY.Add(_OIA);

                            logStr = _OIA.modelToString(logStr);
                        }
                    }
                    else
                    {
                        changFlag = true;
                    }
                }
                if (changFlag)
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                }
                else
                {
                    db.SaveChanges();
                    #region LOG
                    //新增LOG
                    Log log = new Log();
                    log.CFUNCTION = "申請覆核-資料庫異動:印章";
                    log.CACTION = "A";
                    log.CCONTENT = logStr;
                    LogDao.Insert(log, searchModel.vCreate_Uid);
                    #endregion
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"申請單號:{_data.Item1}");
                    result.Datas = GetCDCSearchData(searchModel);
                }
            }
            return result;
        }

        /// <summary>
        /// 庫存異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _seal = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_seal != null)
                {
                    _seal.INVENTORY_STATUS = "1"; //在庫
                    _seal.MEMO_AFT = null;
                    _seal.SEAL_DESC_AFT = null;
                    _seal.LAST_UPDATE_DT = dt;
                    logStr = _seal.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _seal = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_seal != null)
                {
                    _seal.INVENTORY_STATUS = "1"; //在庫
                    _seal.MEMO = GetNewValue(_seal.MEMO, _seal.MEMO_AFT);
                    _seal.MEMO_AFT = null;
                    _seal.SEAL_DESC = GetNewValue(_seal.SEAL_DESC, _seal.SEAL_DESC_AFT);
                    _seal.SEAL_DESC_AFT = null;
                    _seal.LAST_UPDATE_DT = dt;
                    logStr = _seal.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _seal = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_seal != null)
                {
                    _seal.INVENTORY_STATUS = "1"; //在庫
                    _seal.CHARGE_DEPT_AFT = null;
                    _seal.CHARGE_SECT_AFT = null;
                    _seal.LAST_UPDATE_DT = dt;
                    logStr = _seal.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _seal = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_seal != null)
                {
                    _seal.INVENTORY_STATUS = "1"; //在庫
                    _seal.CHARGE_DEPT = _seal.CHARGE_DEPT_AFT;
                    _seal.CHARGE_DEPT_AFT = null;
                    _seal.CHARGE_SECT = _seal.CHARGE_SECT_AFT;
                    _seal.CHARGE_SECT_AFT = null;
                    _seal.LAST_UPDATE_DT = dt;
                    logStr = _seal.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        #endregion

        #region privateFunction

        /// <summary>
        /// 印章資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<SealViewModel> GetDetailModel(IEnumerable<ITEM_SEAL> data,List<SYS_CODE> _Inventory_types,bool accessStatus)
        {
            return data.Select(x => new SealViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vSeal_Desc = accessStatus ? x.SEAL_DESC_ACCESS : x.SEAL_DESC, //印章內容
                vMemo = accessStatus ? x.MEMO_ACCESS : x.MEMO, //備註
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}