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
using static Treasury.Web.Enum.Ref;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
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
                        var _code_type = SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
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
        /// <param name="itemId">物品標號</param>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        public List<SealViewModel> GetDbDataByUnit(string itemId, string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<SealViewModel>();
            List<string> _itemIds = new List<string>();
            _itemIds.Add(itemId);
            if (itemId == TreaItemType.D1008.ToString())
                _itemIds.Add(TreaItemType.D1005.ToString());
            if (itemId == TreaItemType.D1009.ToString())
                _itemIds.Add(TreaItemType.D1006.ToString());
            if (itemId == TreaItemType.D1010.ToString())
                _itemIds.Add(TreaItemType.D1007.ToString());
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
                        vStatus = AccessInventoryType._1.GetDescription(),
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
                             vStatus = AccessInventoryType._4.GetDescription(),
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
                            var _APLY_STATUS = AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                    _APLY_STATUS = AccessProjectFormStatus.A05.ToString(); //為重新申請案例
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
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
                                        {
                                            var _IS = new ITEM_SEAL();

                                            if (item.vItemId.StartsWith(item_Seq))
                                            {
                                                _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = MessageType.already_Change.GetDescription();
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
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                    {
                                        var _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = MessageType.already_Change.GetDescription();
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

                                if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
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
                                else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
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
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
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
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == AccessInventoryType._4.GetDescription())
                                        {
                                            var _IS = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IS_Item_Id = _IS.ITEM_ID;
                                            if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = MessageType.already_Change.GetDescription();
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
                                        result.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}");
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
                        result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
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
            if (access_Type == AccessProjectTradeType.G.ToString()) //取出狀態印章庫存資料檔要復原
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
            if (access_Type == AccessProjectTradeType.G.ToString()) //取出狀態印章庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
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


        #endregion

        #region privateFunction

        /// <summary>
        /// 印章資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<SealViewModel> GetDetailModel(IEnumerable<ITEM_SEAL> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new SealViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vSeal_Desc = x.SEAL_DESC, //印章內容
                vMemo = x.MEMO, //備註
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}