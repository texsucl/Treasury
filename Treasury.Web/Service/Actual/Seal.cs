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

namespace Treasury.Web.Service.Actual
{
    public class Seal : ISeal
    {
        protected INTRA intra { private set; get; }

        public Seal()
        {
            intra = new INTRA();
        }
        #region Get Date

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<SealViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<SealViewModel>();
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
        /// 查詢庫存資料
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        public List<SealViewModel> GetDbDataByUnit(string itemId, string vAplyUnit = null)
        {
            var result = new List<SealViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_SEAL.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1" && x.TREA_ITEM_NAME == itemId) //庫存
                    .AsEnumerable()
                    .Select(x => 
                    new SealViewModel() {
                        vItemId = x.ITEM_ID,
                        vStatus = "在庫",
                        vSeal_Desc = x.SEAL_DESC,
                        vMemo = x.MEMO,
                        vtakeoutFlag = false,
                        vTrea_Item_Name = x.TREA_ITEM_NAME,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 申請覆核 不動產
        /// </summary>
        /// <param name="insertDatas"></param>
        /// <param name="taData"></param>
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

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                var _ARH = db.APLY_REC_HIS.First(x => x.APLY_NO == taData.vAplyNo);
                                _ARH.APLY_STATUS = _APLY_STATUS;
                                _ARH.PROC_DT = dt;
                                #endregion


                                #region 印章庫存資料檔

                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                List<string> oldItemIds = new List<string>(); //原有 itemId
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

                                            if (item.vItemId.StartsWith("E3"))
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

                                                logStr += "|";
                                                logStr += _IS.modelToString();
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo("E4", string.Empty).ToString().PadLeft(8, '0');
                                                _IS_Item_Id = item_id;
                                                _IS = new ITEM_SEAL()
                                                {
                                                    ITEM_ID = $@"E4{item_id}", //物品編號
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
                                                inserts.Add(_IS);
                                                logStr += "|";
                                                logStr += _IS.modelToString();
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
                                            }
                                        }
                                        else
                                        {
                                            if (_IS.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _IS.INVENTORY_STATUS = "1"; //預約取出
                                                _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IS.ITEM_ID);
                                            }
                                        }
                                    }

                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x));
                                        db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)));
                                        db.ITEM_SEAL.RemoveRange(db.ITEM_SEAL.Where(x => delItemId.Contains(x.ITEM_ID)));
                                        db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = taData.vAplyNo,
                                            ITEM_ID = x.ITEM_ID
                                        }));
                                        db.ITEM_SEAL.AddRange(inserts);
                                    }
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                    {
                                        db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && cancelItemIds.Contains(x.ITEM_ID)));
                                        db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = taData.vAplyNo,
                                            ITEM_ID = x
                                        }));
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region 申請單紀錄檔
                                var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                                _TAR = new TREA_APLY_REC()
                                {
                                    APLY_NO = $@"G6{qPreCode}{cId}", //申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                                    APLY_FROM = AccessProjectStartupType.M.ToString(), //人工
                                    ITEM_ID = taData.vItem, //申請項目
                                    ACCESS_TYPE = taData.vAccessType, //存入(P) or 取出(G)
                                    ACCESS_REASON = taData.vAccessReason, //申請原因
                                    APLY_STATUS = AccessProjectFormStatus.A01.ToString(), //表單申請
                                    EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(taData.vExpectedAccessDate), //預計存取日期
                                    APLY_UNIT = taData.vAplyUnit, //申請單位
                                    APLY_UID = taData.vAplyUid, //申請人
                                    APLY_DT = dt,
                                    CREATE_UID = taData.vCreateUid, //新增人
                                    CREATE_DT = dt,
                                    LAST_UPDATE_UID = taData.vCreateUid,
                                    LAST_UPDATE_DT = dt
                                };
                                if (taData.vAplyUid != taData.vCreateUid) //當申請人不是新增人(代表為覆核單位代申請)
                                {
                                    _TAR.CUSTODY_UID = taData.vCreateUid; //覆核單位直接帶 新增人
                                    _TAR.CONFIRM_DT = dt;
                                }
                                logStr += _TAR.modelToString();
                                db.TREA_APLY_REC.Add(_TAR);
                                #endregion

                                #region 申請單歷程檔
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _TAR.APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
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
                                            var item_id = sysSeqDao.qrySeqNo("E4", string.Empty).ToString().PadLeft(8, '0');
                                            _IS_Item_Id = item_id;
                                            var _IS = new ITEM_SEAL()
                                            {
                                                ITEM_ID = $@"E4{item_id}", //物品編號
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
                                            db.ITEM_SEAL.Add(_IS);
                                            logStr += "|";
                                            logStr += _IS.modelToString();
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
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    db.OTHER_ITEM_APLY.Add(
                                    new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _TAR.APLY_NO,
                                        ITEM_ID = _IS_Item_Id
                                    });
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
                                        LogDao.Insert(log, _TAR.CREATE_UID);
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

        #endregion

        #region privateFunction

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