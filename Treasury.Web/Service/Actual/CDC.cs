using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;

namespace Treasury.Web.Service.Actual
{

    public class CDC : Common, ICDC
    {
        #region Get Date
        public CDCViewModel GetItemId()
        {
            var result = new CDCViewModel();
            List<SelectOption> jobProject = new List<SelectOption>(); //作業項目
            List<SelectOption> treasuryIO = new List<SelectOption>(); //金庫內外
            List<SelectOption> dMargin_Take_Of_Type = new List<SelectOption>(); //存入保證金類別
            List<SelectOption> dMarging_Dep_Type = new List<SelectOption>(); //存出保證金類別
            List<SelectOption> Estate_Form_No = new List<SelectOption>(); //不動產權狀狀別

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var other = Ref.TreaItemType.D1019.ToString(); // 其他物品項目 用於條件判斷
                jobProject = db.TREA_ITEM.AsNoTracking() // 抓資料表的所有資料
                    .Where(x => x.ITEM_OP_TYPE == "3" && x.IS_DISABLED == "N" && x.ITEM_ID != other) //條件
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();

                var sysCode = db.SYS_CODE.AsNoTracking().ToList();

                var all = new SelectOption() { Text = "All", Value = "All" };

                treasuryIO = sysCode
                    .Where(x => x.CODE_TYPE == "YN_FLAG")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE,
                    }).ToList();

                dMargin_Take_Of_Type = sysCode
                    .Where(x => x.CODE_TYPE == "MARGIN_TAKE_OF_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE,
                    }).ToList();
                dMargin_Take_Of_Type.Insert(0, all);

                dMarging_Dep_Type = sysCode
                   .Where(x => x.CODE_TYPE == "MARGING_TYPE")
                   .OrderBy(x => x.ISORTBY)
                   .AsEnumerable().Select(x => new SelectOption()
                   {
                       Value = x.CODE,
                       Text = x.CODE_VALUE,
                   }).ToList();
                dMarging_Dep_Type.Insert(0, all);

                Estate_Form_No = sysCode
                 .Where(x => x.CODE_TYPE == "ESTATE_TYPE")
                 .OrderBy(x => x.ISORTBY)
                 .AsEnumerable().Select(x => new SelectOption()
                 {
                     Value = x.CODE,
                     Text = x.CODE_VALUE,
                 }).ToList();
                Estate_Form_No.Insert(0, all);
            }

            result.vTreasuryIO = treasuryIO;
            result.vJobProject = jobProject;
            result.vEstate_From_No = Estate_Form_No;
            result.vMarging = dMarging_Dep_Type;
            result.vMarginp = dMargin_Take_Of_Type;
            result.vBook_No = new Estate().GetBuildName();
            result.vName = new Stock().GetStockName();
            var TRAD_PartnersList = new List<SelectOption>()
            {
                new SelectOption() { Text = "All", Value = "All" }
            };
            TRAD_PartnersList.AddRange(new Deposit().GetTRAD_Partners());
            result.vTRAD_Partners = TRAD_PartnersList;

            return result;
        }

        /// <summary>
        /// 資料查詢異動作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<CDCApprSearchDetailViewModel> GetApprSearchDetail(CDCApprSearchViewModel data)
        {
            List<CDCApprSearchDetailViewModel> result = new List<CDCApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var treaItems = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "3").ToList();
                DateTime? _vAply_Dt = TypeTransfer.stringToDateTimeN(data.vAply_Dt);
                var aplyStatus = ((int)Ref.ApplyStatus._1).ToString(); // 狀態 => 表單申請
                result = db.INVENTORY_CHG_APLY.AsNoTracking()
                    .Where(x => x.CREATE_Date == _vAply_Dt, _vAply_Dt != null)
                    .Where(x => x.APLY_NO == data.vAply_No, !data.vAply_No.IsNullOrWhiteSpace())
                    .Where(x => x.CREATE_UID == data.vAply_Uid, !data.vAply_Uid.IsNullOrWhiteSpace())
                    .Where(x => x.APPR_STATUS == aplyStatus)
                    .AsEnumerable()
                    .Select(x => GetCDCApprSearchDetailViewModel(data.vCreateUid, x, treaItems, emps)).ToList();
            }

            return result;
        }
        #endregion

        #region Save Data
        /// <summary>
        /// 覆核畫面覆核
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <returns></returns>
        public MSGReturnModel<List<CDCApprSearchDetailViewModel>> Approved(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels)
        {
            var result = new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!viewModels.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    #region 資料庫異動申請單紀錄檔
                    var _INVENTORY_CHG_APLY = db.INVENTORY_CHG_APLY
                        .FirstOrDefault(x => x.APLY_NO == item.vAply_No);
                    if (_INVENTORY_CHG_APLY == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vAply_No}");
                        return result;
                    }
                    aplynos.Add(item.vAply_No);
                    var aplyStatus = ((int)Ref.ApplyStatus._2).ToString(); // 狀態 => 覆核完成
                    _INVENTORY_CHG_APLY.APPR_STATUS = aplyStatus;
                    _INVENTORY_CHG_APLY.APPR_UID = searchData.vCreateUid;
                    _INVENTORY_CHG_APLY.APPR_Date = dt.Date;
                    _INVENTORY_CHG_APLY.APPR_Time = dt.TimeOfDay;

                    logStr += _INVENTORY_CHG_APLY.modelToString(logStr);
                    #endregion

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _INVENTORY_CHG_APLY.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);
                    #endregion

                    #region 對應資料檔-核可
                    //其它存取項目申請資料檔找對應物品編號(ITEM_ID)
                    List<string> itemIds = new List<string>();
                    if (_INVENTORY_CHG_APLY.ITEM_ID != Ref.TreaItemType.D1012.ToString())
                    {
                        itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _INVENTORY_CHG_APLY.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    }
                    else
                    {
                        itemIds = db.BLANK_NOTE_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _INVENTORY_CHG_APLY.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    }                   
                    var sampleFactory = new SampleFactory();
                    var getCDCAction = sampleFactory.GetCDCAction(EnumUtil.GetValues<Ref.TreaItemType>().First(x => x.ToString() == _INVENTORY_CHG_APLY.ITEM_ID));
                    if (getCDCAction != null)
                    {
                        var _recover = getCDCAction.CDCApproved(db, itemIds, logStr, dt);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                    #endregion

                }

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
                        log.CFUNCTION = "覆核-資料庫異動覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 覆核成功!";
                        result.Datas = GetApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 覆核畫面駁回
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        public MSGReturnModel<List<CDCApprSearchDetailViewModel>> Reject(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels, string apprDesc)
        {
            var result = new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    #region 資料庫異動申請單紀錄檔
                    var _INVENTORY_CHG_APLY = db.INVENTORY_CHG_APLY
                        .FirstOrDefault(x => x.APLY_NO == item.vAply_No);
                    if (_INVENTORY_CHG_APLY == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vAply_No}");
                        return result;
                    }
                    aplynos.Add(item.vAply_No);
                    var aplyStatus = ((int)Ref.ApplyStatus._3).ToString(); // 狀態 => 退回
                    _INVENTORY_CHG_APLY.APPR_STATUS = aplyStatus;
                    _INVENTORY_CHG_APLY.APPR_UID = searchData.vCreateUid;
                    _INVENTORY_CHG_APLY.APPR_Date = dt.Date;
                    _INVENTORY_CHG_APLY.APPR_Time = dt.TimeOfDay;

                    logStr += _INVENTORY_CHG_APLY.modelToString(logStr);
                    #endregion

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _INVENTORY_CHG_APLY.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);
                    #endregion

                    #region 對應資料檔-駁回
                    //其它存取項目申請資料檔找對應物品編號(ITEM_ID)
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _INVENTORY_CHG_APLY.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    var sampleFactory = new SampleFactory();
                    var getCDCAction = sampleFactory.GetCDCAction(EnumUtil.GetValues<Ref.TreaItemType>().First(x => x.ToString() == _INVENTORY_CHG_APLY.ITEM_ID));
                    if (getCDCAction != null)
                    {
                        var _recover = getCDCAction.CDCReject(db, itemIds, logStr, dt);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                    #endregion
                }

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
                        log.CFUNCTION = "駁回-資料庫異動覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 已駁回!";
                        result.Datas = GetApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }
        #endregion

        #region private function
        private CDCApprSearchDetailViewModel GetCDCApprSearchDetailViewModel(string userId, INVENTORY_CHG_APLY data, List<TREA_ITEM> treaItems, List<V_EMPLY2> emps)
        {
            return new CDCApprSearchDetailViewModel()
            {
                vItem_Id = data.ITEM_ID,
                vItem_Desc = treaItems.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC,
                vAply_Dt = data.CREATE_Date?.ToString("yyyy/MM/dd"),
                vAply_No = data.APLY_NO,
                vAply_Uid = data.CREATE_UID,
                vAply_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == data.CREATE_UID)?.EMP_NAME,
                vApprFlag = data.CREATE_UID != userId
            };
        }
        #endregion
    }
}