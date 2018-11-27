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

namespace Treasury.Web.Service.Actual
{
    public class AftereOpenTreasury : IAftereOpenTreasury
    {
        public AftereOpenTreasury()
        {

        }

        /// <summary>
        /// 初始資料
        /// </summary>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>> GetFristTimeDatas()
        {
            List<SelectOption> reisterId = new List<SelectOption>();
            List<SelectOption> itemOpType = new List<SelectOption>();
            List<SelectOption> accessType = new List<SelectOption>();
            List<SelectOption> sealItem = new List<SelectOption>();
            List<SelectOption> treaItem = new List<SelectOption>();
            List<SelectOption> actualAccessEmps = new List<SelectOption>();
            string firstItemOpType = string.Empty;
            string firstTreaItem = string.Empty;
            string status = Ref.AccessProjectFormStatus.D01.ToString(); // 金庫登記簿檢核
            string statusFromReject = Ref.AccessProjectFormStatus.D04.ToString(); // 金庫登記簿覆核退回
            List<string> _status = new List<string>() { status, statusFromReject };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _CODE_ROLE_ITEM = db.CODE_ROLE_ITEM.AsNoTracking();
                var _CODE_ROLE = db.CODE_ROLE.AsNoTracking();
                var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsNoTracking();
                var _CODE_USER = db.CODE_USER.AsNoTracking();

                reisterId = db.TREA_OPEN_REC.AsNoTracking()
                    .AsEnumerable()
                    .Where(x => !x.TREA_REGISTER_ID.IsNullOrWhiteSpace())
                    //.Where(x => x.CREATE_DT >= DateTime.Today && x.CREATE_DT < DateTime.Today.AddDays(1))
                    //.Where(x => x.REGI_STATUS == status)
                    //.Where(x => x.REGI_STATUS == statusFromReject)
                    .Where(x => _status.Contains(x.REGI_STATUS))
                    //.Where(x => x.ACTUAL_PUT_TIME == null)
                    //.OrderBy(x => x.OPEN_TREA_TIME)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.TREA_REGISTER_ID,
                        Text = x.TREA_REGISTER_ID
                    }).ToList();

                itemOpType = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.ITEM_OP_TYPE != "3")
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.ITEM_OP_TYPE,
                        Text = x.ITEM_OP_TYPE
                    }).Distinct(new SelectOption_Comparer()).OrderBy(x => x.Value)
                    .ToList();

                firstItemOpType = itemOpType.FirstOrDefault().Value;

                treaItem = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.ITEM_OP_TYPE == firstItemOpType)
                    .Select(x => new SelectOption()
                    {
                        Value = x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();

                firstTreaItem = treaItem.FirstOrDefault()?.Value;

                actualAccessEmps = GetActualUserOption(firstTreaItem, null);

                accessType = db.SYS_CODE.AsNoTracking()
                .Where(x => x.CODE_TYPE == "ACCESS_TYPE")
                .AsEnumerable()
                .ToList()
                .OrderBy(x => x.ISORTBY)
                .Select(x => new SelectOption()
                {
                    Value = x.CODE,
                    Text = x.CODE_VALUE
                }).ToList();

                string firstAccessType = accessType.FirstOrDefault()?.Value;


                switch (firstAccessType)
                {
                    case "P":
                        sealItem = _ITEM_SEAL
                            .Where(x => x.INVENTORY_STATUS == "6")
                            .AsEnumerable()
                            .Select(x => new SelectOption()
                            {
                                Value = x.ITEM_ID,
                                Text = x.SEAL_DESC
                            }).ToList();
                        break;
                    case "G":
                    case "S":
                        sealItem = _ITEM_SEAL
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .AsEnumerable()
                           .Select(x => new SelectOption()
                           {
                               Value = x.ITEM_ID,
                               Text = x.SEAL_DESC
                           }).ToList();
                        break;
                }
            }

            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>>(reisterId, itemOpType, accessType, sealItem, treaItem, actualAccessEmps);
        }

        /// <summary>
        /// Dialog Selected Change 事件
        /// </summary>
        /// <param name="ItemOpType"></param>
        /// <param name="TreaItem"></param>
        /// <param name="AccessType"></param>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>> DialogSelectedChange(string ItemOpType, string TreaItem, string AccessType, List<AfterOpenTreasurySearchDetailViewModel> ViewDatas)
        {
            List<SelectOption> vTreaItem = new List<SelectOption>();
            List<SelectOption> vSealItem = new List<SelectOption>();
            List<SelectOption> actualAccessEmps = new List<SelectOption>();
            List<SelectOption> actualAccessType = new List<SelectOption>(); 
            List<string> removeItemList = new List<string>();
            List<string> removeSealList = new List<string>();
            string tItemId = string.Empty;
            string accessType = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _CODE_ROLE_ITEM = db.CODE_ROLE_ITEM.AsNoTracking();
                var _CODE_ROLE = db.CODE_ROLE.AsNoTracking();
                var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsNoTracking();
                var _CODE_USER = db.CODE_USER.AsNoTracking();

                if (ItemOpType == "2")
                {
                    //已在畫面上不需再出現於下拉選單的印章內容
                    removeSealList.AddRange(ViewDatas.Where(x => x.vITEM_OP_TYPE == ItemOpType).Select(x => x.vSEAL_ITEM_ID));
                }
                else if (ItemOpType == "4")
                {
                    //已在畫面上不需再出現於下拉選單的存取項目(其他業務可以新增多筆，所已排除)
                    var _ViewDatas_ITEM_ID = ViewDatas.Where(x => x.vITEM_OP_TYPE == ItemOpType).Select(x => x.vITEM_ID);
                    removeItemList.AddRange(_ViewDatas_ITEM_ID.Where(x => x != "D1023"));
                }
                else
                {
                    //已在畫面上不需再出現於下拉選單的存取項目
                    removeItemList.AddRange(ViewDatas.Where(x => x.vITEM_OP_TYPE == ItemOpType).Select(x => x.vITEM_ID));
                }

                vTreaItem = _TREA_ITEM
                    .Where(x => x.ITEM_OP_TYPE == ItemOpType)
                    .Where(x => !removeItemList.Contains(x.ITEM_ID))   //排除畫面已有的
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();

                if (!TreaItem.IsNullOrWhiteSpace())
                {
                    tItemId = TreaItem;
                }
                else
                {
                    tItemId = vTreaItem.FirstOrDefault()?.Value;
                }

                actualAccessEmps = GetActualUserOption(tItemId, ViewDatas);
                actualAccessType = GetActualAccessTypeOption();
                if (ItemOpType == "2" || _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == tItemId)?.ITEM_OP_TYPE == "2")
                {
                    var _TREA_ITEM_NAME = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == tItemId)?.TREA_ITEM_NAME;
                    var _neededItemId = _TREA_ITEM.Where(x => x.TREA_ITEM_NAME == _TREA_ITEM_NAME).Select(x => x.ITEM_ID).ToList();

                    vSealItem = _ITEM_SEAL
                                .Where(x => _neededItemId.Contains(x.TREA_ITEM_NAME))
                                .Where(x => !removeSealList.Contains(x.ITEM_ID))
                                .Where(x => x.INVENTORY_STATUS != "8")
                                .AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = x.ITEM_ID,
                                    Text = x.SEAL_DESC
                                }).ToList();
                    //根據存取項目顯示印章內容
                    //if (!AccessType.IsNullOrWhiteSpace())
                    //{
                    //    accessType = AccessType;
                    //}
                    //else
                    //{
                    //    accessType = "P";
                    //}
                    //switch (accessType)
                    //{
                    //    case "P":
                    //        vSealItem = _ITEM_SEAL
                    //            .Where(x => !removeSealList.Contains(x.ITEM_ID))
                    //            .Where(x => x.INVENTORY_STATUS == "6")
                    //            .AsEnumerable()
                    //            .Select(x => new SelectOption()
                    //            {
                    //                Value = x.ITEM_ID,
                    //                Text = x.SEAL_DESC
                    //            }).ToList();
                    //        break;
                    //    case "G":
                    //    case "S":
                    //    case "B":
                    //        vSealItem = _ITEM_SEAL
                    //           .Where(x => !removeSealList.Contains(x.ITEM_ID))
                    //           .Where(x => x.INVENTORY_STATUS == "1")
                    //           .AsEnumerable()
                    //           .Select(x => new SelectOption()
                    //           {
                    //               Value = x.ITEM_ID,
                    //               Text = x.SEAL_DESC
                    //           }).ToList();
                    //        break;
                    //    case "A":
                    //        vSealItem = _ITEM_SEAL
                    //           .Where(x => !removeSealList.Contains(x.ITEM_ID))
                    //           .Where(x => x.INVENTORY_STATUS == "6")
                    //           .AsEnumerable()
                    //           .Select(x => new SelectOption()
                    //           {
                    //               Value = x.ITEM_ID,
                    //               Text = x.SEAL_DESC
                    //           }).ToList();
                    //        break;
                    //}
                }
            }
            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>>(vTreaItem, vSealItem, actualAccessEmps, actualAccessType);
        }

        public List<SelectOption> GetActualUserOption(string treaItemId, List<AfterOpenTreasurySearchDetailViewModel> ViewDatas)
        {
            List<SelectOption> result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _CODE_ROLE_ITEM = db.CODE_ROLE_ITEM.AsNoTracking();
                var _CODE_ROLE = db.CODE_ROLE.AsNoTracking();
                var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsNoTracking();
                var _CODE_USER = db.CODE_USER.AsNoTracking();

                var UserIdList = (from T1 in _CODE_ROLE_ITEM
                                  where T1.ITEM_ID == treaItemId
                                  where T1.AUTH_TYPE == "1"
                                  join T2 in _CODE_ROLE
                                  on T1.ROLE_ID equals T2.ROLE_ID
                                  where T2.IS_DISABLED != "Y"
                                  join T3 in _CODE_USER_ROLE
                                  on T1.ROLE_ID equals T3.ROLE_ID
                                  join T4 in _CODE_USER
                                  on T3.USER_ID equals T4.USER_ID
                                  where T4.IS_DISABLED != "Y"
                                  select T3.USER_ID).ToList();

                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    result = dbINTRA.V_EMPLY2.AsNoTracking()
                        .Where(x => UserIdList.Contains(x.USR_ID))
                        .Select(x => new SelectOption()
                        {
                            Value = x.USR_ID,
                            Text = x.EMP_NAME
                        }).ToList();
                }
            }
            if (ViewDatas != null)
            {
                ViewDatas.Where(x => x.vITEM_ID == "D1023").ToList().ForEach(x =>
                {
                    result.Remove(result.FirstOrDefault(y => y.Value == x.vCONFIRM_UID));
                });
            }
            return result;
        }

        public List<SelectOption> GetActualAccessTypeOption(string SEAL_ID = null)
        {
            List<SelectOption> result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == SEAL_ID);
                if(_ITEM_SEAL != null)
                {
                    result = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.CODE_TYPE == "ACCESS_TYPE")
                            .AsEnumerable()
                            .Select(x => new SelectOption()
                            {
                                Value = x.CODE,
                                Text = x.CODE_VALUE
                            }).ToList();

                    switch (_ITEM_SEAL.INVENTORY_STATUS)
                    {
                        case "1":   //在庫
                            result = result.Where(x => x.Value == "B").ToList();
                            break;
                        case "2":   //已被取出
                            break;
                        case "3":   //預約存入
                            break;
                        case "4":   //預約取出
                            break;
                        case "5":   //預約取出，計庫存
                            result = result.Where(x => x.Value == "G" || x.Value == "S").ToList();
                            break;
                        case "6":   //已被取出，計庫存
                            break;
                        case "7":   //已取消
                            break;
                        case "8":
                            break;
                        case "9":   //預約存入，計庫存
                            result = result.Where(x => x.Value == "P" || x.Value == "A").ToList();
                            break;
                        case "10":  //全部取出
                            break;      
                    }
                }
                else
                {
                    result = db.SYS_CODE.AsNoTracking()
                            .Where(x => x.CODE_TYPE == "ACCESS_TYPE")
                            .AsEnumerable()
                            .Select(x => new SelectOption()
                            {
                                Value = x.CODE,
                                Text = x.CODE_VALUE
                            }).ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// 取得 人員基本資料
        /// </summary>
        /// <param name="cUserID"></param>
        /// <returns></returns>
        public BaseUserInfoModel GetUserInfo(string cUserID)
        {
            BaseUserInfoModel user = new BaseUserInfoModel();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                var _emply = dbINTRA.V_EMPLY2.AsNoTracking().FirstOrDefault(x => x.USR_ID == cUserID);
                if (_emply != null)
                {
                    user.EMP_ID = cUserID;
                    user.EMP_Name = _emply.EMP_NAME?.Trim();
                    user.DPT_ID = _emply.DPT_CD?.Trim();
                    user.DPT_Name = _emply.DPT_NAME?.Trim();
                }
            }
            return user;
        }

        /// <summary>
        /// 查詢未確認表單資料
        /// </summary>
        /// <returns></returns>
        public List<AfterOpenTreasuryUnconfirmedDetailViewModel> GetUnconfirmedDetail()
        {
            List<AfterOpenTreasuryUnconfirmedDetailViewModel> result = new List<AfterOpenTreasuryUnconfirmedDetailViewModel>();
            string status = Ref.AccessProjectFormStatus.C01.ToString(); // 保管科覆核完成覆核，待入庫人員確認中
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();

                result = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.APLY_STATUS == status)
                    .Where(x => x.CONFIRM_UID == null)
                    .Where(x => _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID).ITEM_OP_TYPE == "3")
                    //.Where(x => x.APLY_APPR_UID != null)
                    .AsEnumerable()
                    .Select(x => new AfterOpenTreasuryUnconfirmedDetailViewModel()
                    {
                        vITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE,
                        vITEM_ID = x.ITEM_ID.Trim(),
                        vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID).ITEM_DESC,
                        vSEAL_ITEM_ID = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.ITEM_ID : null,
                        vSEAL_ITEM = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.SEAL_DESC : null,
                        vACCESS_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE : null,
                        vACCESS_TYPE_CODE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? x.ACCESS_TYPE : null,
                        vACCESS_REASON = x.ACCESS_REASON,
                        APLY_DT = x.APLY_DT?.ToString("yyyy/MM/dd HH:mm"),
                        APLY_UID = x.APLY_UID,
                        APLY_NAME = !x.APLY_UID.IsNullOrWhiteSpace() ? GetUserInfo(x.APLY_UID)?.EMP_Name : null,
                        vAPLY_NO = x.APLY_NO,
                        hvAPLY_NO = x.APLY_NO,
                        IsTakeout = false,
                        vACTUAL_ACCESS_UID = x.ACTUAL_ACCESS_UID,
                        vACTUAL_ACCESS_NAME = !x.ACTUAL_ACCESS_UID.IsNullOrWhiteSpace() ? GetUserInfo(x.ACTUAL_ACCESS_UID)?.EMP_Name : null,
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<AfterOpenTreasurySearchDetailViewModel> GetSearchDetail(AfterOpenTreasurySearchViewModel searchData)
        {
            List<AfterOpenTreasurySearchDetailViewModel> result = new List<AfterOpenTreasurySearchDetailViewModel>();
            string status = Ref.AccessProjectFormStatus.D01.ToString(); // 金庫登記簿檢核
            string statusFromReject = Ref.AccessProjectFormStatus.D04.ToString(); // 金庫登記簿覆核退回
            if (searchData.vTREA_REGISTER_ID == null) // 單號為必輸
                return result;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();
                var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking().FirstOrDefault(x => x.TREA_REGISTER_ID == searchData.vTREA_REGISTER_ID);

                result = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.TREA_REGISTER_ID == searchData.vTREA_REGISTER_ID, !searchData.vTREA_REGISTER_ID.IsNullOrWhiteSpace()) //金庫登記簿單號
                    .Where(x => x.APLY_STATUS == status || x.APLY_STATUS == statusFromReject)
                    //.Where(x => x.APLY_STATUS == "C02")     //測試
                    .Join(db.TREA_OPEN_REC.AsNoTracking(),
                    x => x.TREA_REGISTER_ID,
                    y => y.TREA_REGISTER_ID,
                    (x, y) => new { _APLY_REC = x, _OPEN_REC = y })
                    .AsEnumerable()
                    .Select(x => new AfterOpenTreasurySearchDetailViewModel()
                    {
                        vACTUAL_PUT_TIME = x._OPEN_REC.ACTUAL_PUT_TIME?.ToString(),
                        vACTUAL_GET_TIME = x._OPEN_REC.ACTUAL_GET_TIME?.ToString(),
                        vTREA_REGISTER_ID = x._APLY_REC.TREA_REGISTER_ID,
                        vAPLY_NO = x._APLY_REC.APLY_NO,
                        hvAPLY_NO = x._APLY_REC.APLY_NO,
                        vACCESS_REASON = x._APLY_REC.ACCESS_REASON,
                        vITEM_ID = x._APLY_REC.ITEM_ID.Trim(),
                        vCONFIRM_NAME = !x._APLY_REC.CONFIRM_UID.IsNullOrWhiteSpace() ? GetUserInfo(x._APLY_REC.CONFIRM_UID).EMP_Name : null,
                        vCONFIRM_UID = x._APLY_REC.CONFIRM_UID,
                        vITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_OP_TYPE,
                        vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_DESC,
                        vSEAL_ITEM_ID = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x._APLY_REC.APLY_NO).ITEM_ID)?.ITEM_ID : null,
                        vSEAL_ITEM = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x._APLY_REC.APLY_NO).ITEM_ID)?.SEAL_DESC : null,
                        vACCESS_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x._APLY_REC.ACCESS_TYPE)?.CODE_VALUE : null,
                        vACCESS_TYPE_CODE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x._APLY_REC.ITEM_ID)?.ITEM_OP_TYPE == "2" ? x._APLY_REC.ACCESS_TYPE : null,
                        ACTUAL_ACCESS_TYPE = _SYS_CODE.FirstOrDefault(y => y.CODE == x._APLY_REC.ACTUAL_ACCESS_TYPE)?.CODE_VALUE,
                        ACTUAL_ACCESS_TYPE_CODE = x._APLY_REC.ACTUAL_ACCESS_TYPE,
                        ACTUAL_ACCESS_UID = x._APLY_REC.ACTUAL_ACCESS_UID,
                        ACTUAL_ACCESS_NAME = x._APLY_REC.ACTUAL_ACCESS_UID != null ? GetUserInfo(x._APLY_REC.ACTUAL_ACCESS_UID)?.EMP_Name : null,
                        vLAST_UPDATE_DT = x._APLY_REC.LAST_UPDATE_DT
                    }).ToList();

                if (_TREA_OPEN_REC.OPEN_TREA_TYPE == "1")
                {
                    List<string> ItemIDList = new List<string>();
                    ItemIDList.AddRange(result.Select(x => x.vITEM_ID).ToList());

                    var _TREA_APLY_TEMP = db.TREA_APLY_TEMP
                        .Where(x => !ItemIDList.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select(x => new AfterOpenTreasurySearchDetailViewModel()
                        {
                            vITEM_ID = x.ITEM_ID,
                            vTREA_REGISTER_ID = searchData.vTREA_REGISTER_ID,
                            vITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE,
                            vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        }).ToList();
                    result.AddRange(_TREA_APLY_TEMP);
                }

                return result;
            }
        }

        /// <summary>
        /// 確定存檔
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> ConfrimedData(AfterOpenTreasurySearchViewModel searchData, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            if (searchData.vTREA_REGISTER_ID == null) // 單號為必輸
                return result;
            if(searchData.vACTUAL_PUT_TIME == null || searchData.vACTUAL_GET_TIME == null)
            {
                if (searchData.vACTUAL_PUT_TIME == null)
                    result.DESCRIPTION = "請輸入入庫時間";
                if (searchData.vACTUAL_GET_TIME == null)
                    result.DESCRIPTION = "請輸入出庫時間";
                return result;
            }
            DateTime _now = DateTime.Now;
            var p_split = searchData.vACTUAL_PUT_TIME.Split(':');
            var p_hh = p_split[0];
            var p_mm = p_split.Length > 1 ? p_split[1] : string.Empty;
            var g_split = searchData.vACTUAL_GET_TIME.Split(':');
            var g_hh = g_split[0];
            var g_mm = g_split.Length > 1 ? g_split[1] : string.Empty;
            var putTime = new DateTime(_now.Year, _now.Month, _now.Day, Convert.ToInt32(p_hh), Convert.ToInt32(p_mm), 0);
            var getTime = new DateTime(_now.Year, _now.Month, _now.Day, Convert.ToInt32(g_hh), Convert.ToInt32(g_mm), 0);
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_OPEN_REC = db.TREA_OPEN_REC
                    .FirstOrDefault(x => x.TREA_REGISTER_ID == searchData.vTREA_REGISTER_ID);

                _TREA_OPEN_REC.ACTUAL_PUT_TIME = putTime;
                _TREA_OPEN_REC.ACTUAL_GET_TIME = getTime;
                _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                _TREA_OPEN_REC.LAST_UPDATE_DT = _now;

                logStr += _TREA_OPEN_REC.modelToString(logStr);

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
                        log.CFUNCTION = "確定存檔-金庫登記簿(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Ref.MessageType.save_Success.GetDescription();

                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetSearchDetail(searchData);
            }
            return result;
        }

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="registerID"></param>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> ApplyData(string registerID, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!viewModels.Any())
            {
                return result;
            }           

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string status = Ref.AccessProjectFormStatus.D02.ToString(); // 金庫登記簿覆核中
            var aplynos = new List<string>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == registerID);

                _TREA_OPEN_REC.REGI_STATUS = status;
                _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                _TREA_OPEN_REC.LAST_UPDATE_DT = dt;
                logStr += _TREA_OPEN_REC.modelToString(logStr);

                var _TREA_APLY_REC = db.TREA_APLY_REC.Where(x => x.TREA_REGISTER_ID == registerID)
                    .Where(x => x.APLY_STATUS == "D01" || x.APLY_STATUS == "D04")
                    //.Where(x => x.APLY_STATUS == "C02")     //測試
                    .ToList();

                _TREA_APLY_REC.ForEach(x =>
                {
                    aplynos.Add(x.APLY_NO);
                    if (x.ACTUAL_ACCESS_TYPE.IsNullOrWhiteSpace())
                    {
                        x.ACTUAL_ACCESS_TYPE = x.ACCESS_TYPE;
                    }
                    if (x.ACTUAL_ACCESS_UID.IsNullOrWhiteSpace())
                    {
                        x.ACTUAL_ACCESS_UID = x.CONFIRM_UID;
                    }
                    x.APLY_STATUS = status;
                    x.LAST_UPDATE_UID = cUserId;
                    x.LAST_UPDATE_DT = dt;
                    logStr += x.modelToString(logStr);

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = x.APLY_NO,
                        APLY_STATUS = status,
                        PROC_UID = cUserId,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);
                    #endregion
                });

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
                        log.CFUNCTION = "申請覆核-金庫登記簿(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 申請成功!";
                        result.Datas = GetSearchDetail(searchData);
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
        /// 新增
        /// </summary>
        /// <param name="InsertModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> InsertData(AfterOpenTreasuryInsertViewModel InsertModel, AfterOpenTreasurySearchViewModel searchData, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string status = Ref.AccessProjectFormStatus.D01.ToString(); // 金庫登記簿檢核

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                SysSeqDao sysSeqDao = new SysSeqDao();
                string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');
                string applyNO = $@"G6{qPreCode}{cId}";

                //var _APLY_UID = db.TREA_APLY_REC.AsNoTracking().AsEnumerable().FirstOrDefault(x => x.TREA_REGISTER_ID == InsertModel.vTREA_REGISTER_ID)?.APLY_UID;

                var TREA_APLY_REC = db.TREA_APLY_REC;
                TREA_APLY_REC.Add(new TREA_APLY_REC()
                {
                    APLY_NO = applyNO,
                    APLY_FROM = "M",
                    TREA_REGISTER_ID = InsertModel.vTREA_REGISTER_ID,
                    ITEM_ID = InsertModel.vITEM_ID,
                    //ACCESS_TYPE = InsertModel.vACCESS_TYPE_CODE,
                    ACCESS_REASON = InsertModel.vACCESS_REASON,
                    APLY_STATUS = status,
                    ACTUAL_ACCESS_UID = !InsertModel.ACTUAL_ACCESS_UID.IsNullOrWhiteSpace() ? InsertModel.ACTUAL_ACCESS_UID : null,
                    ACTUAL_ACCESS_TYPE = !InsertModel.ACTUAL_ACCESS_TYPE.IsNullOrWhiteSpace() ? InsertModel.ACTUAL_ACCESS_TYPE : null,
                    //APLY_UID = _APLY_UID,
                    //APLY_DT = dt,
                    //APLY_APPR_UID = cUserId,
                    //APLY_APPR_DT = dt,
                    CREATE_UID = cUserId,
                    CREATE_DT = dt,
                    //CONFIRM_UID = cUserId,
                    //CONFIRM_DT = dt,
                    CREATE_UNIT = GetUserInfo(cUserId)?.DPT_ID,            
                });
                logStr += TREA_APLY_REC.modelToString(logStr);

                #region 申請單歷程檔
                var ARH = new APLY_REC_HIS()
                {
                    APLY_NO = applyNO,
                    APLY_STATUS = status,
                    PROC_UID = cUserId,
                    PROC_DT = dt
                };
                logStr += ARH.modelToString(logStr);
                #endregion

                #region 其它存取項目申請資料檔
                if (InsertModel.vITEM_OP_TYPE == "2")
                {
                    var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY
                        .Add(new OTHER_ITEM_APLY()
                        {
                            APLY_NO = applyNO,
                            ITEM_ID = InsertModel.vSEAL_ITEM_ID
                        });
                    logStr += _OTHER_ITEM_APLY.modelToString(logStr);

                    var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == InsertModel.vSEAL_ITEM_ID);
                    var _INVENTORY_STATUS = string.Empty;

                    switch (InsertModel.ACTUAL_ACCESS_TYPE)
                    {
                        //存入
                        case "P":
                            _INVENTORY_STATUS = "9";
                            break;
                        //取出
                        case "G":
                            _INVENTORY_STATUS = "5";
                            break;
                        //用印
                        case "S":
                            _INVENTORY_STATUS = "5";
                            break;
                        //存入用印
                        case "A":
                            _INVENTORY_STATUS = "9";
                            break;
                        //取出存入
                        case "B":
                            _INVENTORY_STATUS = "1";
                            break;
                    }
                    _ITEM_SEAL.INVENTORY_STATUS = _INVENTORY_STATUS;
                    logStr += _ITEM_SEAL.modelToString(logStr);
                }
                #endregion

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
                        log.CFUNCTION = "新增-金庫登記簿(關庫後)";
                        log.CACTION = "A";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = "新增成功!";

                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetSearchDetail(searchData);
            }
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="searchData"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
       // public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> UpdateData(string APLYNO, string ActualAccEmp, string ActualAccType, string InsertReason, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId, string ItemId, string OpType)
        public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> UpdateData(string APLYNO, AfterOpenTreasuryInsertViewModel InsertModel, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
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
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == APLYNO);

                if(_TREA_APLY_REC == null)
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == searchData.vTREA_REGISTER_ID);
                    if(_TREA_OPEN_REC.OPEN_TREA_TYPE == "1")
                    {
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                        var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');
                        string applyNO = $@"G6{qPreCode}{cId}";
                        string status = Ref.AccessProjectFormStatus.D01.ToString(); // 金庫登記簿檢核

                        var TREA_APLY_REC = db.TREA_APLY_REC;
                        TREA_APLY_REC.Add(new TREA_APLY_REC()
                        {
                            APLY_NO = applyNO,
                            APLY_FROM = "M",
                            TREA_REGISTER_ID = InsertModel.vTREA_REGISTER_ID,
                            ITEM_ID = InsertModel.vITEM_ID,
                            ACCESS_REASON = InsertModel.vACCESS_REASON,
                            APLY_STATUS = status,
                            ACTUAL_ACCESS_UID = InsertModel.ACTUAL_ACCESS_UID,
                            ACTUAL_ACCESS_TYPE = InsertModel.ACTUAL_ACCESS_TYPE,
                            CREATE_UID = cUserId,
                            CREATE_DT = dt,
                            CREATE_UNIT = GetUserInfo(cUserId)?.DPT_ID

                        });
                        logStr += TREA_APLY_REC.modelToString(logStr);

                        #region 申請單歷程檔
                        var ARH = new APLY_REC_HIS()
                        {
                            APLY_NO = applyNO,
                            APLY_STATUS = status,
                            PROC_UID = cUserId,
                            PROC_DT = dt
                        };
                        logStr += ARH.modelToString(logStr);
                        #endregion

                        #region 其它存取項目申請資料檔
                        if (InsertModel.vITEM_OP_TYPE == "2")
                        {
                            var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY
                                .Add(new OTHER_ITEM_APLY()
                                {
                                    APLY_NO = applyNO,
                                    ITEM_ID = InsertModel.vSEAL_ITEM_ID
                                });
                            logStr += _OTHER_ITEM_APLY.modelToString(logStr);

                            var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == InsertModel.vSEAL_ITEM_ID);
                            var _INVENTORY_STATUS = string.Empty;

                            switch (InsertModel.ACTUAL_ACCESS_TYPE)
                            {
                                //存入
                                case "P":
                                    _INVENTORY_STATUS = "9";
                                    break;
                                //取出
                                case "G":
                                    _INVENTORY_STATUS = "5";
                                    break;
                                //用印
                                case "S":
                                    _INVENTORY_STATUS = "5";
                                    break;
                                //存入用印
                                case "A":
                                    _INVENTORY_STATUS = "9";
                                    break;
                                //取出存入
                                case "B":
                                    _INVENTORY_STATUS = "1";
                                    break;
                            }
                            _ITEM_SEAL.INVENTORY_STATUS = _INVENTORY_STATUS;
                            logStr += _ITEM_SEAL.modelToString(logStr);
                        }
                        #endregion
                    }
                    else
                    {
                        result.DESCRIPTION = "查無單號";
                        return result;
                    }
                }
                else
                {
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > viewModels.FirstOrDefault(x => x.hvAPLY_NO == APLYNO)?.vLAST_UPDATE_DT) //資料已被更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{_TREA_APLY_REC.TREA_REGISTER_ID}");
                        return result;
                    }

                    if (!InsertModel.ACTUAL_ACCESS_UID.IsNullOrWhiteSpace())
                        _TREA_APLY_REC.ACTUAL_ACCESS_UID = InsertModel.ACTUAL_ACCESS_UID;
                    if (!InsertModel.vACCESS_TYPE_CODE.IsNullOrWhiteSpace())
                        _TREA_APLY_REC.ACTUAL_ACCESS_TYPE = InsertModel.vACCESS_TYPE_CODE;
                    if(!InsertModel.vACCESS_REASON.IsNullOrWhiteSpace())
                        _TREA_APLY_REC.ACCESS_REASON = InsertModel.vACCESS_REASON;
                    _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    logStr += _TREA_APLY_REC.modelToString(logStr);
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
                        log.CFUNCTION = "修改-金庫登記簿(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {APLYNO} 修改成功!";
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetSearchDetail(searchData);
            }
            return result;
        }

        public MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> UnConfirmedUpdateDatas(string APLYNO, string ActualAccEmp, List<AfterOpenTreasuryUnconfirmedDetailViewModel> viewModels, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
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
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == APLYNO);

                if (_TREA_APLY_REC.LAST_UPDATE_DT > viewModels.FirstOrDefault(x => x.hvAPLY_NO == APLYNO)?.vLAST_UPDATE_DT) //資料已被更新
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{_TREA_APLY_REC.TREA_REGISTER_ID}");
                    return result;
                }

                if (!ActualAccEmp.IsNullOrWhiteSpace())
                    _TREA_APLY_REC.ACTUAL_ACCESS_UID = ActualAccEmp;
                _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                logStr += _TREA_APLY_REC.modelToString(logStr);

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
                        log.CFUNCTION = "未確認-(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {APLYNO} 修改成功!";
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetUnconfirmedDetail();
            }
            return result;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> DeleteData(string APLYNO, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId, bool custodyFlag)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!viewModels.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string tempStatus = "";
            string nonCustodyStatus = Ref.AccessProjectFormStatus.B03.ToString(); // 金庫人員退回保管科承辦人員
            string custodyStatus = Ref.AccessProjectFormStatus.A04.ToString(); // 金庫人員退回保管科申請人
            string rejectStatue = Ref.AccessProjectFormStatus.E04.ToString(); // 金庫人員退回作業

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == APLYNO);
                if (_TREA_APLY_REC == null)
                {
                    result.DESCRIPTION = "此項目尚未入庫確認，請先選擇【修改】執行確認";
                    return result;
                }
                var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.FirstOrDefault(x => x.APLY_NO == _TREA_APLY_REC.APLY_NO);
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == _TREA_APLY_REC.ITEM_ID);

                if (_TREA_APLY_REC.LAST_UPDATE_DT > viewModels.FirstOrDefault(x => x.hvAPLY_NO == APLYNO)?.vLAST_UPDATE_DT) //資料已被更新
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{_TREA_APLY_REC.TREA_REGISTER_ID}");
                    return result;
                }

                switch (_TREA_ITEM.ITEM_OP_TYPE)
                {
                    case "1":
                    case "4":
                        _TREA_APLY_REC.APLY_STATUS = rejectStatue;
                        tempStatus = rejectStatue;
                        break;
                    case "2":
                        if(_OTHER_ITEM_APLY != null)
                        {
                            var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY.ITEM_ID);

                            switch (_TREA_APLY_REC.ACCESS_TYPE)
                            {
                                case "P":
                                case "A":
                                    _ITEM_SEAL.INVENTORY_STATUS = "6";
                                    break;
                                case "G":
                                case "S":
                                case "B":
                                    _ITEM_SEAL.INVENTORY_STATUS = "1";
                                    break;
                            }
                            _ITEM_SEAL.LAST_UPDATE_DT = dt;
                            logStr += _ITEM_SEAL.modelToString(logStr);
                            _TREA_APLY_REC.APLY_STATUS = rejectStatue;
                            tempStatus = rejectStatue;
                        }
                        break;
                    case "3":
                        if (Properties.Settings.Default["CustodianFlag"]?.ToString() == _TREA_APLY_REC.CREATE_UNIT)
                        //新增人員等於保管科人員 狀態 => 入庫確認中
                        {
                            _TREA_APLY_REC.APLY_STATUS = custodyStatus;
                            tempStatus = custodyStatus;
                        }
                        else
                        {
                            _TREA_APLY_REC.APLY_STATUS = nonCustodyStatus;
                            tempStatus = nonCustodyStatus;
                        }

                        break;
                }

                _TREA_APLY_REC.TREA_REGISTER_ID = string.Empty;
                _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                logStr += _TREA_APLY_REC.modelToString(logStr);

                #region 申請單歷程檔
                var ARH = new APLY_REC_HIS()
                {
                    APLY_NO = APLYNO,
                    APLY_STATUS = tempStatus,
                    PROC_UID = cUserId,
                    PROC_DT = dt
                };
                logStr += ARH.modelToString(logStr);

                db.APLY_REC_HIS.Add(ARH);
                #endregion

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
                        log.CFUNCTION = "刪除-金庫登記簿(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {APLYNO} 刪除成功!";
                        result.Datas = GetSearchDetail(searchData);
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
        /// 未確認表單刪除
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> UnconfirmedDeleteData(string APLYNO, List<AfterOpenTreasuryUnconfirmedDetailViewModel> viewModels, string cUserId)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!viewModels.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string nonCustodyStatus = Ref.AccessProjectFormStatus.B03.ToString(); // 金庫人員退回保管科承辦人員
            string custodyStatus = Ref.AccessProjectFormStatus.A04.ToString(); // 金庫人員退回保官科申請人

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == APLYNO);

                if (_TREA_APLY_REC.LAST_UPDATE_DT > viewModels.FirstOrDefault(x => x.hvAPLY_NO == APLYNO)?.vLAST_UPDATE_DT) //資料已被更新
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{_TREA_APLY_REC.TREA_REGISTER_ID}");
                    return result;
                }
                if (Properties.Settings.Default["CustodianFlag"]?.ToString() == _TREA_APLY_REC.CREATE_UNIT)
                //新增人員等於保管科人員 狀態 => 入庫確認中
                {
                    _TREA_APLY_REC.APLY_STATUS = custodyStatus;
                }
                else
                {
                    _TREA_APLY_REC.APLY_STATUS = nonCustodyStatus;
                }
                _TREA_APLY_REC.TREA_REGISTER_ID = string.Empty;
                _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                logStr += _TREA_APLY_REC.modelToString(logStr);

                #region 申請單歷程檔
                var ARH = new APLY_REC_HIS()
                {
                    APLY_NO = APLYNO,
                    APLY_STATUS = Properties.Settings.Default["CustodianFlag"]?.ToString() == _TREA_APLY_REC.CREATE_UNIT? custodyStatus : nonCustodyStatus,
                    PROC_UID = cUserId,
                    PROC_DT = dt
                };
                logStr += ARH.modelToString(logStr);
                db.APLY_REC_HIS.Add(ARH);
                #endregion
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
                        log.CFUNCTION = "未確認刪除-(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {APLYNO} 刪除成功!";
                        result.Datas = GetUnconfirmedDetail();
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }

        public MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> InsertUnconfirmedDetail(string RegisterID, List<AfterOpenTreasuryUnconfirmedDetailViewModel> InsertModel, string cUserId, AfterOpenTreasurySearchViewModel SearchData)
        {
            var result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string status = Ref.AccessProjectFormStatus.D01.ToString(); // 金庫登記簿檢核
            List<string> aplyNO_List = new List<string>();

            if (!InsertModel.Any())
            {
                return result;
            }
            if(InsertModel.Where(x => x.IsTakeout).Any(x => x.vACTUAL_ACCESS_NAME == null))
            {
                result.DESCRIPTION = "有單號未輸入實際入庫人員";
                return result;
            }

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                InsertModel.Where(x => x.IsTakeout == true).ToList()
                    .ForEach(x =>
                    {
                        var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(y => y.APLY_NO == x.hvAPLY_NO);
                        _TREA_APLY_REC.APLY_STATUS = status;
                        _TREA_APLY_REC.TREA_REGISTER_ID = RegisterID;
                        //_TREA_APLY_REC.CONFIRM_UID = cUserId;
                        //_TREA_APLY_REC.CONFIRM_DT = dt;
                        _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                        _TREA_APLY_REC.LAST_UPDATE_DT = dt;

                        #region 申請單歷程檔
                        var ARH = new APLY_REC_HIS()
                        {
                            APLY_NO = x.vAPLY_NO,
                            APLY_STATUS = status,
                            PROC_UID = cUserId,
                            PROC_DT = dt
                        };
                        logStr += ARH.modelToString(logStr);

                        db.APLY_REC_HIS.Add(ARH);
                        #endregion

                        aplyNO_List.Add(x.vAPLY_NO);
                    });

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
                        log.CFUNCTION = "新增登記簿(關庫後)";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplyNO_List)} 新增成功!";

                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
                if (result.RETURN_FLAG)
                {
                    result.Datas = GetUnconfirmedDetail();
                    GetSearchDetail(SearchData);
                }
            }
            return result;
        }

        public Tuple<string, string> GetConfrimedTime(string RegisterNo)
        {
            if (!RegisterNo.IsNullOrWhiteSpace())
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking().First(x => x.TREA_REGISTER_ID == RegisterNo);

                    return new Tuple<string, string>(_TREA_OPEN_REC.ACTUAL_PUT_TIME?.ToString("hh:mm"), _TREA_OPEN_REC.ACTUAL_GET_TIME?.ToString("hh:mm"));
                }
            }
            return null;
        }
    }
}