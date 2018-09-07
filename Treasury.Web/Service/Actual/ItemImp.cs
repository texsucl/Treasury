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
using Treasury.WebControllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 重要物品
/// 初版作者：20180810 陳宥穎
/// 修改歷程：20180810 陳宥穎 
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
    public class ItemImp : Common, IItemImp
    {
        public ItemImp()
        {

        }

        #region Get Date

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<ItemImpViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<ItemImpViewModel>();
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
                    //使用物品編號去重要物品庫存資料檔抓取資料
                    var details = db.ITEM_IMPO.AsNoTracking()
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
        
        public List<ItemImpViewModel> GetDbDataByUnit(string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<ItemImpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_IMPO.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x => 
                    new ItemImpViewModel() {
                        vItemId = x.ITEM_ID,
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vItemImp_Name = x.ITEM_NAME,
                        vItemImp_Quantity = x.QUANTITY,
                        vItemImp_Amount = x.AMOUNT,
                        //vItemImp_Expected_Date_1 = x.EXPECTED_ACCESS_DATE == null ? null : x.EXPECTED_ACCESS_DATE.Value.DateToTaiwanDate(9),
                        vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                        vDescription = x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vtakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                    
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                        result.AddRange(db.ITEM_IMPO.AsNoTracking().Where(
                        x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                        .Select(x =>
                         new ItemImpViewModel()
                         {
                             vItemId = x.ITEM_ID,
                             vStatus = Ref.AccessInventoryType._4.GetDescription(),
                             vItemImp_Name = x.ITEM_NAME,
                        vItemImp_Quantity = x.QUANTITY,
                        vItemImp_Amount = x.AMOUNT,
                        //vItemImp_Expected_Date_1 = x.EXPECTED_ACCESS_DATE == null ? null : x.EXPECTED_ACCESS_DATE.Value.DateToTaiwanDate(9),
                        vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                        vDescription = x.DESCRIPTION,
                        vMemo = x.MEMO,
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
        /// 申請覆核 重要物品
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
                    var datas = (List<ItemImpViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            var item_Seq = "E8"; //重要物品流水號開頭編碼

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

                                #region 重要物品庫存資料檔

                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId 
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<string> cancelItemIds = new List<string>(); //取消 itemId
                                List<ITEM_IMPO> inserts = new List<ITEM_IMPO>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _II = new ITEM_IMPO();

                                            if (item.vItemId.StartsWith(item_Seq)) //舊有資料
                                            {
                                                _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                { 
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _II.ITEM_NAME = item.vItemImp_Name; //重要物品名稱
                                                _II.QUANTITY = item.vItemImp_Quantity; //重要物品數量
                                                _II.AMOUNT = item.vItemImp_Amount; //重要物品金額
                                                _II.EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date); //重要物品預計提取日期
                                                _II.DESCRIPTION = item.vDescription;//說明
                                                _II.MEMO = item.vMemo; //備註
                                                updateItemIds.Add(item.vItemId);
                                                logStr += _II.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');                                               
                                                _II = new ITEM_IMPO()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    ITEM_NAME = item.vItemImp_Name, //重要物品名稱
                                                    QUANTITY = item.vItemImp_Quantity, //重要物品數量
                                                    AMOUNT = item.vItemImp_Amount, //重要物品金額
                                                    EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date), //重要物品預計提取日期
                                                    DESCRIPTION = item.vDescription,//說明
                                                    MEMO = item.vMemo, //備註
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                                               //PUT_DATE = dt, //存入日期時間
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };
                                                _IC_Item_Id = _II.ITEM_ID;
                                                inserts.Add(_II);
                                                logStr += _II.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        var _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vtakeoutFlag)
                                        {
                                            if (_II.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _II.INVENTORY_STATUS = "4"; //預約取出
                                                _II.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_II.ITEM_ID);
                                            }
                                            else if (_II.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_II.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_II.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _II.INVENTORY_STATUS = "1"; //預約取出
                                                _II.LAST_UPDATE_DT = dt;  //最後修改時間
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_IMPO.RemoveRange(db.ITEM_IMPO.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_IMPO.AddRange(inserts);
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

                                #region 重要物品庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _II_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _II = new ITEM_IMPO()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                ITEM_NAME = item.vItemImp_Name, //重要物品名稱
                                                QUANTITY = item.vItemImp_Quantity, //重要物品數量
                                                AMOUNT = item.vItemImp_Amount, //重要物品金額
                                                EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date), //重要物品預計提取日期
                                                DESCRIPTION = item.vDescription,//說明
                                                MEMO = item.vMemo, //備註
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                                           //PUT_DATE = dt, //存入日期時間
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _II_Item_Id = _II.ITEM_ID;
                                            db.ITEM_IMPO.Add(_II);
                                            logStr += _II.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            var _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _II_Item_Id = _II.ITEM_ID;
                                            if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _II.INVENTORY_STATUS = "4"; //預約取出
                                                                        //_IRE.GET_DATE = dt; //取出日期時間
                                            _II.LAST_UPDATE_DT = dt;  //最後修改時間
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    if (!_II_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _II_Item_Id
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
                                        log.CFUNCTION = "申請覆核-新增重要物品";
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態重要物品庫存資料檔要復原
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態重要物品庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除重要物品庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_IMPO.RemoveRange(db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)));
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
        }

        #endregion

        #region privateFunction

        /// <summary>
        /// 重要物品資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<ItemImpViewModel> GetDetailModel(IEnumerable<ITEM_IMPO> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new ItemImpViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vItemImp_Name = x.ITEM_NAME, //重要物品名稱
                vItemImp_Quantity = x.QUANTITY, //重要物品數量
                vItemImp_Amount = x.AMOUNT, //重要物品金額
                //vItemImp_Expected_Date_1 = x.EXPECTED_ACCESS_DATE == null ? null : x.EXPECTED_ACCESS_DATE.Value.DateToTaiwanDate(9),
                vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE), //重要物品預計提取日期
                vDescription = x.DESCRIPTION,//說明
                vMemo = x.MEMO, //備註
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}