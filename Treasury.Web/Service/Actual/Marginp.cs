﻿using System;
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
/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存入保證金
/// 初版作者：20180810 王菁萱
/// 修改歷程：20180810 王菁萱 
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
    public class Marginp : Common, IMarginp
    {
        public Marginp()
        {

        }

        #region Get Date

        /// <summary>
        /// 抓取 存入保證金類別 項目
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMarginp_Take_Of_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "MARGIN_TAKE_OF_TYPE")
                    .Select( x => new SelectOption() {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }));
            }
            return result;
        }

        /// <summary>
        /// 抓取 存入保證金物品名稱 項目
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMarginp_Item()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "MARGPIN_ITEM")
                .Select(x => new SelectOption()
                {
                    Value = x.CODE,
                    Text = x.CODE_VALUE
                }));
            }
            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<MarginpViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<MarginpViewModel>();
            //result.
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去電子憑證庫存資料檔抓取資料
                    var details = db.ITEM_DEP_RECEIVED.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        result = GetDetailModel(details, _Inventory_types).ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        public List<MarginpViewModel> GetDbDataByUnit(string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<MarginpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_DEP_RECEIVED.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x =>
                    new MarginpViewModel() {
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                        vMarginp_Trad_Partners = x.TRAD_PARTNERS,
                        vItemId = x.ITEM_ID,
                        vMarginp_Amount = TypeTransfer.decimalNToString(x.AMOUNT),
                        vMarginp_Item = x.MARGIN_ITEM,
                        vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                        vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,
                        vMarginp_Effective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                        vMarginp_Effective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                        vDescription = x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vMarginp_Book_No = x.BOOK_NO,
                        vtakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                        result.AddRange(db.ITEM_DEP_RECEIVED.AsNoTracking().Where(
                        x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                        .Select(x =>
                         new MarginpViewModel()
                         {
                             vStatus = Ref.AccessInventoryType._4.GetDescription(),
                             vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                             vMarginp_Trad_Partners = x.TRAD_PARTNERS,
                             vItemId = x.ITEM_ID,
                             vMarginp_Amount = TypeTransfer.decimalNToString(x.AMOUNT),
                             vMarginp_Item = x.MARGIN_ITEM,
                             vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                             vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,
                             vMarginp_Effective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                             vMarginp_Effective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                             vMemo = x.MEMO,
                             vMarginp_Book_No = x.BOOK_NO,
                             vtakeoutFlag = true,
                             vLast_Update_Time = x.LAST_UPDATE_DT
                         }));
                }
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
                    var datas = (List<MarginpViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            var item_Seq = "E5"; //電子憑證流水號開頭編碼

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請
                            
                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                    _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString(logStr);
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
                                List<ITEM_DEP_RECEIVED> inserts = new List<ITEM_DEP_RECEIVED>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _IC = new ITEM_DEP_RECEIVED();

                                            if (item.vItemId.StartsWith(item_Seq)) //舊有資料
                                            {
                                                _IC = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IC.MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type; //類別
                                                _IC.TRAD_PARTNERS = item.vMarginp_Trad_Partners; //交易對象
                                                _IC.ITEM_ID = item.vItemId; //歸檔編號
                                               // _IC.AMOUNT = item.vMarginp_Amount; //金額
                                                _IC.MARGIN_ITEM = item.vMarginp_Item;//物品名稱
                                                _IC.MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer;//物品發行人
                                                _IC.PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No;//質押標的號碼
                                               // _IC.EFFECTIVE_DATE_B = item.vMarginp_Effective_Date_B;//有效期間(起)
                                               // _IC.EFFECTIVE_DATE_E = item.vMarginp_Effective_Date_E;//有效期間(迄)
                                                _IC.MEMO = item.vMemo; //備註說明
                                                _IC.BOOK_NO = item.vMarginp_Book_No;//冊號
                                                updateItemIds.Add(item.vItemId);
                                                logStr += _IC.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                                _IC = new ITEM_DEP_RECEIVED()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //歸檔編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type, //類別
                                                    TRAD_PARTNERS = item.vMarginp_Trad_Partners, //交易對象
                                                   // AMOUNT = item.vMarginp_Amount, //金額
                                                    MARGIN_ITEM = item.vMarginp_Item, //物品名稱 
                                                    MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer,//物品發行人
                                                    PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No,//質押標的號碼
                                                   // EFFECTIVE_DATE_B = item.vMarginp_Effective_Date_B,//有效期間(起)
                                                   // EFFECTIVE_DATE_E = item.vMarginp_Effective_Date_E,//有效期間(迄)
                                                    MEMO = item.vMemo, //備註說明
                                                    BOOK_NO = item.vMarginp_Book_No,//冊號
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                                               //PUT_DATE = dt, //存入日期時間
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };
                                                _IC_Item_Id = _IC.ITEM_ID;
                                                inserts.Add(_IC);
                                                logStr += _IC.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        var _IC = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vtakeoutFlag)
                                        {
                                            if (_IC.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _IC.INVENTORY_STATUS = "4"; //預約取出
                                                _IC.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IC.ITEM_ID);
                                            }
                                            else if (_IC.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_IC.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IC.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _IC.INVENTORY_STATUS = "1"; //預約取出
                                                _IC.LAST_UPDATE_DT = dt;  //最後修改時間
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_RECEIVED.RemoveRange(db.ITEM_DEP_RECEIVED.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_DEP_RECEIVED.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
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

                                #region 電子憑證庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _IC = new ITEM_DEP_RECEIVED()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type, //類別
                                                TRAD_PARTNERS = item.vMarginp_Trad_Partners, //交易對象
                                               // AMOUNT = item.vMarginp_Amount, //金額
                                                MARGIN_ITEM = item.vMarginp_Item, // 物品名稱
                                                MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer,//物品發行人
                                                PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No,//質押標的號碼
                                               // EFFECTIVE_DATE_B = item.vMarginp_Effective_Date_B,//有效期間(起)
                                               // EFFECTIVE_DATE_E = item.vMarginp_Effective_Date_E,//有效期間(迄)
                                                MEMO = item.vMemo, //備註說明
                                                BOOK_NO = item.vMarginp_Book_No,//冊號
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                                           //PUT_DATE = dt, //存入日期時間
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            db.ITEM_DEP_RECEIVED.Add(_IC);
                                            logStr += _IC.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            var _IC = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IC.INVENTORY_STATUS = "4"; //預約取出
                                                                        //_IRE.GET_DATE = dt; //取出日期時間
                                            _IC.LAST_UPDATE_DT = dt;  //最後修改時間
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    if (!_IC_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IC_Item_Id
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
                                        log.CFUNCTION = "申請覆核-新增電子憑證";
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態電子憑證庫存資料檔要復原
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態電子憑證庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除電子憑證庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_DEP_RECEIVED.RemoveRange(db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)));
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
        }

        #endregion

        #region privateFunction

        /// <summary>
        /// 電子憑證資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<MarginpViewModel> GetDetailModel(IEnumerable<ITEM_DEP_RECEIVED> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new MarginpViewModel()
            {
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,//類別
                vMarginp_Trad_Partners = x.TRAD_PARTNERS,//交易對象
                vItemId = x.ITEM_ID,//歸檔編號
               // vMarginp_Amount = x.AMOUNT,//金額
                vMarginp_Item = x.MARGIN_ITEM, // 物品名稱
                vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,// 物品發行人
                vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,//質押標的號碼
              //  vMarginp_Effective_Date_B = x.EFFECTIVE_DATE_B,//有效期間(起)
               // vMarginp_Effective_Date_E = x.EFFECTIVE_DATE_E,//有效期間(迄) 
                vMemo = x.MEMO,//備註說明
                vMarginp_Book_No = x.BOOK_NO,//冊號 
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}