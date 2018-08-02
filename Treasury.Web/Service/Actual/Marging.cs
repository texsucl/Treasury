using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存出保證金
/// 初版作者：20180730 侯蔚鑫
/// 修改歷程：20180730 侯蔚鑫 
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
    public class Marging: Common,IMarging
    {
        protected INTRA intra { private set; get; }

        public Marging()
        {
            intra = new INTRA();
        }

        #region GetData
        /// <summary>
        /// 類別
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMargingType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "MARGING_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <returns></returns>
        public List<MargingViewModel> GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null)
        {
            var result = new List<MargingViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x =>
                    new MargingViewModel()
                    {
                        vItem_PK = x.ITEM_ID,
                        vItem_Id=x.ITEM_ID,
                        vStatus = AccessInventoryType._1.GetDescription(),
                        vTrad_Partners=x.TRAD_PARTNERS,
                        vMargin_Dep_Type=x.MARGIN_DEP_TYPE,
                        vAmount=x.AMOUNT,
                        vWorkplace_Code=x.WORKPLACE_CODE,
                        vDescription=x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vBook_No = x.BOOK_NO,
                        vTakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_REFUNDABLE_DEP.AsNoTracking().Where(
                    x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                    .Select(x =>
                     new MargingViewModel()
                     {
                         vItem_PK = x.ITEM_ID,
                         vItem_Id = x.ITEM_ID,
                         vStatus = AccessInventoryType._4.GetDescription(),
                         vTrad_Partners = x.TRAD_PARTNERS,
                         vMargin_Dep_Type = x.MARGIN_DEP_TYPE,
                         vAmount = x.AMOUNT,
                         vWorkplace_Code = x.WORKPLACE_CODE,
                         vDescription = x.DESCRIPTION,
                         vMemo = x.MEMO,
                         vBook_No = x.BOOK_NO,
                         vTakeoutFlag = true,
                         vLast_Update_Time = x.LAST_UPDATE_DT
                     }));
                }
            }
            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        public List<MargingViewModel> GetDataByAplyNo(string vAplyNo)
        {
            var result = new List<MargingViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去存出保證金庫存資料檔抓取資料
                    var details = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
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
        #endregion

        #region SaveData
        /// <summary>
        /// 申請覆核動作
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
                    var datas = (List<MargingViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();

                            string logStr = string.Empty; //log
                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = AccessProjectFormStatus.A01.ToString(); //表單申請
                            if (taData.vAplyNo.IsNullOrWhiteSpace()) //新增申請單
                            {
                                String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                                var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion
                                #region 儲存資料
                                foreach (var item in datas)
                                {
                                    var _IRD_Item_Id = string.Empty;
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
                                        {
                                            string item_id = string.Empty;

                                            //依類別取得對應歸檔編號
                                            switch (item.vMargin_Dep_Type)
                                            {
                                                case "1":
                                                    item_id = sysSeqDao.qrySeqNo("A", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"A{item_id}";
                                                    break;
                                                case "2":
                                                    item_id = sysSeqDao.qrySeqNo("B", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"B{item_id}";
                                                    break;
                                                case "3":
                                                    item_id = sysSeqDao.qrySeqNo("C", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"C{item_id}";
                                                    break;
                                                default:
                                                    result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                                                    return result;
                                            }

                                            #region 存出保證金庫存資料檔
                                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                            var _IRD = new ITEM_REFUNDABLE_DEP()
                                            {
                                                ITEM_ID = item_id,   //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                TRAD_PARTNERS = item.vTrad_Partners,  //交易對象
                                                MARGIN_DEP_TYPE = item.vMargin_Dep_Type,  //存出保證金類別
                                                AMOUNT = item.vAmount,    //金額
                                                WORKPLACE_CODE = item.vWorkplace_Code,    //職場代號
                                                DESCRIPTION = item.vDescription,  //說明
                                                MEMO = item.vMemo,    //備註
                                                BOOK_NO = item.vBook_No,  //冊號
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                LAST_UPDATE_DT = dt //最後修改時間
                                            };
                                            _IRD_Item_Id = _IRD.ITEM_ID;
                                            db.ITEM_REFUNDABLE_DEP.Add(_IRD);
                                            logStr += "|";
                                            logStr += _IRD.modelToString();
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == AccessInventoryType._4.GetDescription())
                                        {
                                            #region 存出保證金庫存資料檔
                                            var _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                            _IRD_Item_Id = _IRD.ITEM_ID;
                                            if (_IRD.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IRD.INVENTORY_STATUS = "4"; //預約取出
                                            _IRD.LAST_UPDATE_DT = dt;  //最後修改時間
                                            #endregion
                                        }
                                    }

                                    #region 其它存取項目申請資料檔
                                    if (!_IRD_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IRD_Item_Id
                                        });
                                    }
                                    #endregion

                                }
                                #endregion
                            }
                            else //修改申請單
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
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _TAR.APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
                                #endregion

                                #region 儲存資料
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_REFUNDABLE_DEP> inserts = new List<ITEM_REFUNDABLE_DEP>(); //新增資料
                                var _IRD = new ITEM_REFUNDABLE_DEP();

                                foreach(var item in datas)
                                {
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == AccessInventoryType._3.GetDescription())
                                        {
                                            string TypeCode = string.Empty;

                                            //依類別取得對應歸檔編號
                                            switch (item.vMargin_Dep_Type)
                                            {
                                                case "1":
                                                    TypeCode = "A";
                                                    break;
                                                case "2":
                                                    TypeCode = "B";
                                                    break;
                                                case "3":
                                                    TypeCode = "C";
                                                    break;
                                                default:
                                                    result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                                                    return result;
                                            }

                                            if (item.vItem_PK.StartsWith(TypeCode))  //明細修改
                                            {
                                                _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.vItem_PK);
                                                if (_IRD.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IRD.TRAD_PARTNERS = item.vTrad_Partners;   //交易對象
                                                _IRD.MARGIN_DEP_TYPE = item.vMargin_Dep_Type;   //存出保證金類別
                                                _IRD.AMOUNT = item.vAmount; //金額
                                                _IRD.WORKPLACE_CODE = item.vWorkplace_Code; //職場代號
                                                _IRD.DESCRIPTION = item.vDescription;   //說明
                                                _IRD.MEMO = item.vMemo; //備註
                                                _IRD.BOOK_NO = item.vBook_No;   //冊號
                                                _IRD.LAST_UPDATE_DT = dt;   //最後修改時間

                                                updateItemIds.Add(item.vItem_Id);

                                                logStr += "|";
                                                logStr += _IRD.modelToString();
                                            }
                                            else //明細新增
                                            {
                                                string item_id = string.Empty;

                                                //依類別取得對應歸檔編號
                                                switch (item.vMargin_Dep_Type)
                                                {
                                                    case "1":
                                                        item_id = sysSeqDao.qrySeqNo("A", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"A{item_id}";
                                                        break;
                                                    case "2":
                                                        item_id = sysSeqDao.qrySeqNo("B", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"B{item_id}";
                                                        break;
                                                    case "3":
                                                        item_id = sysSeqDao.qrySeqNo("C", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"C{item_id}";
                                                        break;
                                                    default:
                                                        result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                                                        return result;
                                                }

                                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                                _IRD = new ITEM_REFUNDABLE_DEP()
                                                {
                                                    ITEM_ID = item_id,   //歸檔編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    TRAD_PARTNERS = item.vTrad_Partners,  //交易對象
                                                    MARGIN_DEP_TYPE = item.vMargin_Dep_Type,  //存出保證金類別
                                                    AMOUNT = item.vAmount,    //金額
                                                    WORKPLACE_CODE = item.vWorkplace_Code,    //職場代號
                                                    DESCRIPTION = item.vDescription,  //說明
                                                    MEMO = item.vMemo,    //備註
                                                    BOOK_NO = item.vBook_No,  //冊號
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                    LAST_UPDATE_DT = dt //最後修改時間
                                                };
                                                inserts.Add(_IRD);
                                                logStr += _IRD.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_REFUNDABLE_DEP.RemoveRange(db.ITEM_REFUNDABLE_DEP.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_REFUNDABLE_DEP.AddRange(inserts);

                                }
                                else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                {

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
                                    log.CFUNCTION = "申請覆核-新增存出保證金";
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

            return new Tuple<bool, string>(true, logStr);
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

            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        /// <summary>
        /// 存出保證金資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<MargingViewModel> GetDetailModel(IEnumerable<ITEM_REFUNDABLE_DEP> data, List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new MargingViewModel()
            {
                vItem_PK = x.ITEM_ID,   //網頁PK
                vItem_Id = x.ITEM_ID, //歸檔編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vTrad_Partners = x.TRAD_PARTNERS, //交易對象
                vMargin_Dep_Type = x.MARGIN_DEP_TYPE,   //存出保證金類別
                vAmount = x.AMOUNT, //金額
                vWorkplace_Code = x.WORKPLACE_CODE, //職場代號
                vDescription = x.DESCRIPTION,   //說明
                vMemo = x.MEMO, //備註
                vBook_No = x.BOOK_NO,   //冊號
                vTakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }
        #endregion
    }
}