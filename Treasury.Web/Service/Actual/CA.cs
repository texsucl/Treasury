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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 電子憑證
/// 初版作者：20180723 張家華
/// 修改歷程：20180723 張家華 
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
    public class CA : ICA
    {
        protected INTRA intra { private set; get; }

        public CA()
        {
            intra = new INTRA();
        }

        #region Get Date

        public List<SelectOption> GetCA_Use()
        {
            var result = new List<SelectOption>();
            using (dbTreasuryEntities db = new dbTreasuryEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "CA_USE")
                    .Select( x => new SelectOption() {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }));
            }
            return result;
        }

        public List<SelectOption> GetCA_Desc()
        {
            var result = new List<SelectOption>();
            using (dbTreasuryEntities db = new dbTreasuryEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "CA_DESC")
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
        public List<CAViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<CAViewModel>();
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
                    var details = db.ITEM_CA.AsNoTracking()
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
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        public List<CAViewModel> GetDbDataByUnit(string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<CAViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_CA.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x => 
                    new CAViewModel() {
                        vItemId = x.ITEM_ID,
                        vStatus = AccessInventoryType._1.GetDescription(),
                        vCA_Desc = x.CA_DESC,
                        vCA_Use = x.CA_USE,
                        vBank = x.BANK,
                        vCA_Number = x.CA_NUMBER,
                        vMemo = x.MEMO,
                        vtakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                        result.AddRange(db.ITEM_CA.AsNoTracking().Where(
                        x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                        .Select(x =>
                         new CAViewModel()
                         {
                             vItemId = x.ITEM_ID,
                             vStatus = AccessInventoryType._4.GetDescription(),
                             vCA_Desc = x.CA_DESC,
                             vCA_Use = x.CA_USE,
                             vBank = x.BANK,
                             vCA_Number = x.CA_NUMBER,
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
                    var datas = (List<CAViewModel>)insertDatas;
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
                            var _APLY_STATUS = AccessProjectFormStatus.A01.ToString(); //表單申請
                            
                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                    _APLY_STATUS = AccessProjectFormStatus.A05.ToString(); //為重新申請案例
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
                                List<ITEM_CA> inserts = new List<ITEM_CA>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
                                        {
                                            var _IC = new ITEM_CA();

                                            if (item.vItemId.StartsWith(item_Seq)) //舊有資料
                                            {
                                                _IC = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IC.CA_DESC = item.vCA_Desc; //電子憑證品項
                                                _IC.CA_USE = item.vCA_Use; //電子憑證用途
                                                _IC.BANK = item.vBank; //銀行/廠商
                                                _IC.CA_NUMBER = item.vCA_Number; //電子憑證號碼
                                                _IC.MEMO = item.vMemo; //備註說明
                                                updateItemIds.Add(item.vItemId);
                                                logStr += _IC.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');                                               
                                                _IC = new ITEM_CA()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    CA_DESC = item.vCA_Desc, //電子憑證品項
                                                    CA_USE = item.vCA_Use, //電子憑證用途
                                                    BANK = item.vBank, //銀行/廠商
                                                    CA_NUMBER = item.vCA_Number, //電子憑證號碼
                                                    MEMO = item.vMemo, //備註說明
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
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                    {
                                        var _IC = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = MessageType.already_Change.GetDescription();
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

                                if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_CA.RemoveRange(db.ITEM_CA.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_CA.AddRange(inserts);
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
                                if (taData.vAplyUid != taData.vCreateUid) //當申請人不是新增人(代表為保管單位代申請)
                                {
                                    _TAR.CUSTODY_UID = taData.vCreateUid; //覆核單位直接帶 新增人
                                    _TAR.CONFIRM_DT = dt;
                                }
                                logStr += _TAR.modelToString(logStr);
                                db.TREA_APLY_REC.Add(_TAR);
                                #endregion

                                #region 申請單歷程檔
                                var _ARH = new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _TAR.APLY_STATUS,
                                    PROC_DT = dt,
                                    PROC_UID = _TAR.CREATE_UID
                                };
                                logStr += _ARH.modelToString(logStr);
                                db.APLY_REC_HIS.Add(_ARH);
                                #endregion

                                #region 電子憑證庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _IC = new ITEM_CA()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                CA_DESC = item.vCA_Desc, //電子憑證品項
                                                CA_USE = item.vCA_Use, //電子憑證用途
                                                BANK = item.vBank, //銀行/廠商
                                                CA_NUMBER = item.vCA_Number, //電子憑證號碼
                                                MEMO = item.vMemo, //備註說明
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                                           //PUT_DATE = dt, //存入日期時間
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            db.ITEM_CA.Add(_IC);
                                            logStr += _IC.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == AccessInventoryType._4.GetDescription())
                                        {
                                            var _IC = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = MessageType.already_Change.GetDescription();
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

        /// <summary>
        /// 電子憑證資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<CAViewModel> GetDetailModel(IEnumerable<ITEM_CA> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new CAViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vCA_Desc = x.CA_DESC, //電子憑證品項
                vCA_Use = x.CA_USE, //電子憑證用途
                vBank = x.BANK, //銀行/廠商
                vCA_Number = x.CA_NUMBER, //電子憑證號碼
                vMemo = x.MEMO, //備註
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}