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
    public class ConfirmStorage : IConfirmStorage
    {
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, string, List<string>> GetFirstTimeData(string cUserId)
        {
            List<SelectOption> storageType = new List<SelectOption>();
            List<SelectOption> accessType = new List<SelectOption>();
            List<SelectOption> item = new List<SelectOption>();
            List<SelectOption> itemOpType = new List<SelectOption>();
            List<SelectOption> sealItem = new List<SelectOption>();
            List<string> itemId = new List<string>();
            string registerId = string.Empty;
            var empty = new SelectOption() { Text = string.Empty, Value = string.Empty };

            BaseUserInfoModel user = GetUserInfo(cUserId); //填表人 資料
            
            try
            {
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
                        var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsNoTracking();
                        var _CODE_ROLE_ITEM = db.CODE_ROLE_ITEM.AsNoTracking();
                        var _CODE_ROLE = db.CODE_ROLE.AsNoTracking();
                        var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                        var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking();
                        var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                        var today = DateTime.Today;
                        var now = DateTime.Now;

                        //itemId = _CODE_USER_ROLE.AsNoTracking()
                        //           .Where(x => x.USER_ID == cUserId) //登入者所擁有的角色
                        //           .Join(_CODE_ROLE_ITEM.AsNoTracking()
                        //           .Where(x => x.AUTH_TYPE == "1"),//表單申請權限=Y
                        //           x => x.ROLE_ID,
                        //           y => y.ROLE_ID,
                        //           (x, y) => new { _CODE_ROLE_ITEM = y }
                        //           ).Join(_CODE_ROLE.AsNoTracking()
                        //           .Where(x => x.IS_DISABLED == "N"),
                        //           x => x._CODE_ROLE_ITEM.ROLE_ID,
                        //           z => z.ROLE_ID,
                        //           (x, z) => new { _CODE_ROLE_ITEM = x }
                        //           ).Join(_TREA_ITEM.AsNoTracking(),
                        //           x => x._CODE_ROLE_ITEM._CODE_ROLE_ITEM.ITEM_ID,
                        //           y => y.ITEM_ID,
                        //           (x, y) => y
                        //           ).Distinct()
                        //           .AsEnumerable()
                        //           .Select(x => x.ITEM_ID).ToList();

                        //itemOpType = _CODE_USER_ROLE.AsNoTracking()
                        //           .Where(x => x.USER_ID == cUserId) //登入者所擁有的角色
                        //           .Join(_CODE_ROLE_ITEM.AsNoTracking()
                        //           .Where(x => x.AUTH_TYPE == "1"),//表單申請權限=Y
                        //           x => x.ROLE_ID,
                        //           y => y.ROLE_ID,
                        //           (x, y) => new { _CODE_ROLE_ITEM = y }
                        //           ).Join(_CODE_ROLE.AsNoTracking()
                        //           .Where(x => x.IS_DISABLED == "N"),
                        //           x => x._CODE_ROLE_ITEM.ROLE_ID,
                        //           z => z.ROLE_ID,
                        //           (x, z) => new { _CODE_ROLE_ITEM = x }
                        //           ).Join(_TREA_ITEM.AsNoTracking(),
                        //           x => x._CODE_ROLE_ITEM._CODE_ROLE_ITEM.ITEM_ID,
                        //           y => y.ITEM_ID,
                        //           (x, y) => y
                        //           ).Distinct()
                        //           .AsEnumerable()
                        //           .Select(x => new SelectOption()
                        //           {
                        //               Value = x.ITEM_OP_TYPE,
                        //               Text = x.ITEM_OP_TYPE
                        //           }).ToList();

                        itemId = (from T1 in _CODE_USER_ROLE
                                  join T2 in _CODE_ROLE_ITEM
                                  on T1.ROLE_ID equals T2.ROLE_ID
                                  join T3 in _CODE_ROLE
                                  on T2.ROLE_ID equals T3.ROLE_ID
                                  join T4 in _TREA_ITEM
                                  on T2.ITEM_ID equals T4.ITEM_ID
                                  group T4 by T4.ITEM_ID into G
                                  orderby G.Key
                                  select G.Key).ToList();

                        itemOpType = (from T1 in _CODE_USER_ROLE
                                  join T2 in _CODE_ROLE_ITEM
                                  on T1.ROLE_ID equals T2.ROLE_ID
                                  join T3 in _CODE_ROLE
                                  on T2.ROLE_ID equals T3.ROLE_ID
                                  join T4 in _TREA_ITEM
                                  on T2.ITEM_ID equals T4.ITEM_ID
                                  orderby T4.ITEM_ID
                                  select new SelectOption()
                                  {
                                      Value = T4.ITEM_OP_TYPE,
                                      Text = T4.ITEM_OP_TYPE
                                  }).ToList();

                        itemOpType = itemOpType.Distinct(new SelectOption_Comparer()).OrderBy(x => x.Value).ToList();

                        var whichType = itemOpType.FirstOrDefault().Value;

                        item = _TREA_ITEM
                            .Where(x => x.ITEM_OP_TYPE == whichType)
                            .Where(x => itemId.Contains(x.ITEM_ID))                          
                            .Select(x => new SelectOption()
                            {
                                Value = x.ITEM_ID,
                                Text = x.ITEM_DESC
                            }).ToList();

                        var whichItem = item.FirstOrDefault().Value;
                        var itemName = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == whichItem).TREA_ITEM_NAME;
                        
                        if (itemName != null)
                        {
                            var whichitemName = _TREA_ITEM
                                .Where(x => x.TREA_ITEM_NAME == itemName)
                                .Select(x => x.ITEM_ID).ToList();

                            sealItem = _ITEM_SEAL
                               .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                               .Where(x => x.INVENTORY_STATUS == "6")
                               .Select(x => new SelectOption()
                               {
                                   Value = x.ITEM_ID,
                                   Text = x.SEAL_DESC
                               }).ToList();
                        }

                        //item = _CODE_USER_ROLE.AsNoTracking()
                        //           .Where(x => x.USER_ID == cUserId) //登入者所擁有的角色
                        //           .Join(_CODE_ROLE_ITEM.AsNoTracking()
                        //           .Where(x => x.AUTH_TYPE == "1"),//表單申請權限=Y
                        //           x => x.ROLE_ID,
                        //           y => y.ROLE_ID,
                        //           (x, y) => new { _CODE_ROLE_ITEM = y}
                        //           ).Join(_CODE_ROLE.AsNoTracking()
                        //           .Where(x => x.IS_DISABLED == "N"),
                        //           x => x._CODE_ROLE_ITEM.ROLE_ID,
                        //           z => z.ROLE_ID,
                        //           (x, z) => new { _CODE_ROLE_ITEM = x}
                        //           ).Join(db.TREA_ITEM.AsNoTracking(),
                        //           x => x._CODE_ROLE_ITEM._CODE_ROLE_ITEM.ITEM_ID,
                        //           y => y.ITEM_ID,
                        //           (x, y) => y
                        //           ).AsEnumerable()
                        //           .Select(x => new SelectOption() {
                        //               Value = x.ITEM_ID,
                        //               Text = x.ITEM_DESC
                        //           }).ToList();

                        //storageType = db.SYS_CODE.AsNoTracking()
                        //.Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE")
                        //.AsEnumerable()
                        //.Select(x => new SelectOption()
                        //{
                        //    Value = x.CODE,
                        //    Text = x.CODE_VALUE
                        //}).ToList();

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

                        registerId = _TREA_OPEN_REC.AsEnumerable()
                            .Where(x => x.OPEN_TREA_DATE == today)
                            .Where(x => x.REGI_STATUS == "C02")
                            .Where(x => DateTime.Parse(x.EXEC_TIME_B + ":00") <= now)
                            .Where(x => DateTime.Parse(x.EXEC_TIME_E + ":00") >= now)
                            .FirstOrDefault()?.TREA_REGISTER_ID;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.exceptionMessage();
                throw ex;
            }

            //try
            //{
            //    using (TreasuryDBEntities db = new TreasuryDBEntities())
            //    {
            //        storageType = db.SYS_CODE.AsNoTracking()
            //            .Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE")
            //            .AsEnumerable()
            //            .Select(x => new SelectOption()
            //            {
            //                Value = x.CODE,
            //                Text = x.CODE_VALUE
            //            }).ToList();

            //        accessType = db.SYS_CODE.AsNoTracking()
            //            .Where(x => x.CODE_TYPE == "ACCESS_TYPE")
            //            .AsEnumerable()
            //            .Select(x => new SelectOption()
            //            {
            //                Value = x.CODE,
            //                Text = x.CODE_VALUE
            //            }).ToList();

            //        //item = db.TREA_ITEM.AsNoTracking()
            //        //    .Select(x => new SelectOption()
            //        //    {
            //        //        Value = x.ITEM_ID,
            //        //        Text = x.ITEM_DESC
            //        //    }).ToList();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var message = ex.exceptionMessage();
            //    throw ex;
            //}

            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, string, List<string>>(sealItem, accessType, item, itemOpType, registerId, itemId);
        }

        public Tuple<List<SelectOption>, List<SelectOption>> ItemOpTypeChange(string data, List<string> ItemIdList, string AccessType, List<string>SealIdList,string ItemId = null)
        {
            List<SelectOption> vItem = new List<SelectOption>();
            List<SelectOption> vSealItem = new List<SelectOption>();
            List<string> ItemIds = new List<string>();
            if (!data.IsNullOrWhiteSpace())
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    vItem = db.TREA_ITEM.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == data)
                        .Where(x => ItemIdList.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select(x => new SelectOption()
                        {
                            Value = x.ITEM_ID,
                            Text = x.ITEM_DESC
                        }).ToList();

                    if (!ItemId.IsNullOrWhiteSpace())
                    {
                        var _TREA_ITEM_NAME = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_ID == ItemId).FirstOrDefault()?.TREA_ITEM_NAME;
                        ItemIds.AddRange(
                             db.TREA_ITEM.AsNoTracking().Where(x => x.TREA_ITEM_NAME == _TREA_ITEM_NAME).Select(x => x.ITEM_ID));
                    }


                    var whitchItem = vItem.FirstOrDefault().Value;
                    var itemName = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == whitchItem).TREA_ITEM_NAME;
                    var whichitemName = _TREA_ITEM
                                .Where(x => x.TREA_ITEM_NAME == itemName)
                                .Select(x => x.ITEM_ID).ToList();
                    if (AccessType == "P")
                    {
                        vSealItem = db.ITEM_SEAL.AsNoTracking()
                       .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                       .Where(x => !SealIdList.Contains(x.ITEM_ID))
                       .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                       .Where(x => x.INVENTORY_STATUS == "6")
                       .AsEnumerable()
                       .Select(x => new SelectOption()
                       {
                           Value = x.ITEM_ID,
                           Text = x.SEAL_DESC
                       }).ToList();
                    }
                    else if (AccessType == "G")
                    {
                        vSealItem = db.ITEM_SEAL.AsNoTracking()
                      .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                      .Where(x => !SealIdList.Contains(x.ITEM_ID))
                      .Where(x => ItemIds.Contains(x.TREA_ITEM_NAME))
                      .Where(x => x.INVENTORY_STATUS == "1")
                      .AsEnumerable()
                      .Select(x => new SelectOption()
                      {
                          Value = x.ITEM_ID,
                          Text = x.SEAL_DESC
                      }).ToList();
                    }
                    else if(AccessType == "S")
                    {
                        vSealItem = db.ITEM_SEAL.AsNoTracking()
                     .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                     .Where(x => !SealIdList.Contains(x.ITEM_ID))
                     .Where(x => ItemIds.Contains(x.TREA_ITEM_NAME))
                     .Where(x => x.INVENTORY_STATUS == "1")
                     .AsEnumerable()
                     .Select(x => new SelectOption()
                     {
                         Value = x.ITEM_ID,
                         Text = x.SEAL_DESC
                     }).ToList();
                    }
                    else if (AccessType == "A")
                    {
                        vSealItem = db.ITEM_SEAL.AsNoTracking()
                     .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                     .Where(x => !SealIdList.Contains(x.ITEM_ID))
                     .Where(x => ItemIds.Contains(x.TREA_ITEM_NAME))
                     .Where(x => x.INVENTORY_STATUS == "2")
                     .AsEnumerable()
                     .Select(x => new SelectOption()
                     {
                         Value = x.ITEM_ID,
                         Text = x.SEAL_DESC
                     }).ToList();
                    }
                    else if (AccessType == "B")
                    {
                        vSealItem = db.ITEM_SEAL.AsNoTracking()
                     .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                     .Where(x => !SealIdList.Contains(x.ITEM_ID))
                     .Where(x => ItemIds.Contains(x.TREA_ITEM_NAME))
                     .Where(x => x.INVENTORY_STATUS == "1")
                     .AsEnumerable()
                     .Select(x => new SelectOption()
                     {
                         Value = x.ITEM_ID,
                         Text = x.SEAL_DESC
                     }).ToList();
                    }

                }
                return new Tuple<List<SelectOption>, List<SelectOption>>(vItem, vSealItem);
            }
            return null;
        }

        public Tuple<string, List<SelectOption>> GetItemOpType(string data, string AccessType, List<string> SealIdList,string ItemId = null)
        {
            string vOpType = string.Empty;
            List<SelectOption> vitem = new List<SelectOption>();
            if (!data.IsNullOrWhiteSpace())
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    vOpType = db.TREA_ITEM.AsNoTracking()
                        .FirstOrDefault(x => x.ITEM_ID == data)?.ITEM_OP_TYPE;

                    var itemName = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == data).TREA_ITEM_NAME;
                    var whichitemName = _TREA_ITEM
                                .Where(x => x.TREA_ITEM_NAME == itemName)
                                .Select(x => x.ITEM_ID).ToList();

                    if (AccessType == "P")
                    {
                        vitem = db.ITEM_SEAL.AsNoTracking()
                       .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                       .Where(x => !SealIdList.Contains(x.ITEM_ID))
                       .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                       .Where(x => x.INVENTORY_STATUS == "6")
                       .AsEnumerable()
                       .Select(x => new SelectOption()
                       {
                           Value = x.ITEM_ID,
                           Text = x.SEAL_DESC
                       }).ToList();
                    }
                    else if(AccessType == "G")
                    {
                        vitem = db.ITEM_SEAL.AsNoTracking()
                      .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                      .Where(x => !SealIdList.Contains(x.ITEM_ID))
                      .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                      .Where(x => x.INVENTORY_STATUS == "1")
                      .AsEnumerable()
                      .Select(x => new SelectOption()
                      {
                          Value = x.ITEM_ID,
                          Text = x.SEAL_DESC
                      }).ToList();
                    }
                    else if(AccessType == "S")
                    {
                        vitem = db.ITEM_SEAL.AsNoTracking()
                      .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                      .Where(x => !SealIdList.Contains(x.ITEM_ID))
                      .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                      .Where(x => x.INVENTORY_STATUS == "1")
                      .AsEnumerable()
                      .Select(x => new SelectOption()
                      {
                          Value = x.ITEM_ID,
                          Text = x.SEAL_DESC
                      }).ToList();
                    }
                    else if (AccessType == "A")
                    {
                        vitem = db.ITEM_SEAL.AsNoTracking()
                     .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                     .Where(x => !SealIdList.Contains(x.ITEM_ID))
                     .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                     .Where(x => x.INVENTORY_STATUS == "2")
                     .AsEnumerable()
                     .Select(x => new SelectOption()
                     {
                         Value = x.ITEM_ID,
                         Text = x.SEAL_DESC
                     }).ToList();
                    }
                    else if (AccessType == "B")
                    {
                        vitem = db.ITEM_SEAL.AsNoTracking()
                     .Where(x => whichitemName.Contains(x.TREA_ITEM_NAME))
                     .Where(x => !SealIdList.Contains(x.ITEM_ID))
                     .Where(x => x.TREA_ITEM_NAME == ItemId, !ItemId.IsNullOrWhiteSpace())
                     .Where(x => x.INVENTORY_STATUS == "1")
                     .AsEnumerable()
                     .Select(x => new SelectOption()
                     {
                         Value = x.ITEM_ID,
                         Text = x.SEAL_DESC
                     }).ToList();
                    }
                }
                return new Tuple<string, List<SelectOption>>(vOpType, vitem);
            }

            return null;
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<ConfirmStorageSearchDetailViewModel> GetSearchDetail(ConfirmStorageSearchViewModel data)
        {
            List<ConfirmStorageSearchDetailViewModel> result = new List<ConfirmStorageSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vCREATE_DT);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vCREATE_DT).DateToLatestTime();
                DateTime? now = DateTime.Now;
                var _ITEM_ITEM = db.TREA_ITEM.AsNoTracking();
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking().AsQueryable();
                var _TREA_APLY_TEMP = db.TREA_APLY_TEMP.AsNoTracking().AsQueryable();
                var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();

                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking().Where(x =>x.ITEM_OP_TYPE == "3").Select(x => x.ITEM_ID).ToList();
                if (data.v_IS_CHECKED == null)  //是否已確認
                {
                    _TREA_APLY_REC = _TREA_APLY_REC
                   .Where(x => x.APLY_STATUS == "C01")
                   .Where(x => x.EXPECTED_ACCESS_DATE <= _vAPLY_DT_E, _vAPLY_DT_E != null)
                   .Where(x => data.vITEM_ID_List.Contains(x.ITEM_ID))
                   .Where(x => _TREA_ITEM.Contains(x.ITEM_ID))
                   .Where(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID, !data.vTREA_REGISTER_ID.IsNullOrWhiteSpace());

                    _TREA_APLY_TEMP = _TREA_APLY_TEMP
                    .Where(x => data.vITEM_ID_List.Contains(x.ITEM_ID));

                    _TREA_APLY_TEMP.ToList().ForEach(x => {
                        var _ITeM_OP_TYPE = _ITEM_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE;
                        var _SEAL_ITEM = _ITEM_SEAL.Where(y => y.TREA_ITEM_NAME == x.ITEM_ID).ToList();
                        result.Add(new ConfirmStorageSearchDetailViewModel()
                        {
                            vITeM_OP_TYPE = _ITeM_OP_TYPE,
                            vITEM_ID = x.ITEM_ID,
                            vITEM = _ITEM_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        });
                    });
                }
                else
                {
                    _TREA_APLY_REC = _TREA_APLY_REC
                   .Where(x => x.APLY_STATUS == "C02")
                   .Where(x => data.vITEM_ID_List.Contains(x.ITEM_ID))
                   .Where(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID, !data.vTREA_REGISTER_ID.IsNullOrWhiteSpace());
                }


                _TREA_APLY_REC.ToList().ForEach(x =>
                {
                    var _ITeM_OP_TYPE = _ITEM_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE;
                    var _SEAL_ITEM = _ITEM_SEAL.Where(y => y.TREA_ITEM_NAME == x.ITEM_ID).ToList();
                    result.Add(new ConfirmStorageSearchDetailViewModel()
                    {
                        vITeM_OP_TYPE = _ITeM_OP_TYPE,
                        vITEM_ID = x.ITEM_ID,
                        vITEM = _ITEM_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,
                        //vAPLY_NO = _ITeM_OP_TYPE == "3" ? x.APLY_NO : null,
                        vAPLY_NO = x.APLY_NO,
                        vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        vACCESS_TYPE = x.APLY_STATUS == "C02" && _ITeM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE : null,
                        vACCESS_TYPE_CODE = x.APLY_STATUS == "C02" && _ITeM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE : null,
                        vSEAL_ITEM_ID = x.APLY_STATUS == "C02" && _ITeM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.ITEM_ID : null,
                        vSEAL_ITEM = x.APLY_STATUS == "C02" && _ITeM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.SEAL_DESC : null,
                        //vSEAL_ITEM_OPTION = _ITeM_OP_TYPE == "2" ? _SEAL_ITEM.Select(y => new SelectOption() { Value = y.SEAL_DESC, Text = y.APLY_DEPT }).ToList() : null,
                        vCONFIRM_UID = GetUserInfo(x.CONFIRM_UID).EMP_Name,
                        vCONFIRM_DT = x.CONFIRM_DT?.ToString("yyyy/MM/dd HH:mm"),
                        vACCESS_REASON = x.APLY_STATUS == "C02" ? x.ACCESS_REASON : null,
                        hvAPLY_NO = x.APLY_NO,
                    });
                });
                result.ForEach(x => { x.uuid = Guid.NewGuid().ToString(); });
                return result;
            }
        }

        public MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> InsertData(ConfirmStorageInsertViewModel data, ConfirmStorageSearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            if (data.vITeM_OP_TYPE != "3")
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    DateTime now = DateTime.Now;
                    //SysSeqDao sysSeqDao = new SysSeqDao();
                    //string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                    //var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');
                    //string applyNO = $@"G6{qPreCode}{cId}";
                    //string applyStatus = "C01";
                    string logStr = string.Empty;

                    #region 申請單暫存檔
                    var _ItemId = db.TREA_APLY_TEMP.AsNoTracking().Select(x => x.ITEM_ID).ToList();
                    if (!_ItemId.Contains(data.vITEM_ID))
                    {
                        var _TREA_APLY_TEMP = db.TREA_APLY_TEMP
                        .Add(new TREA_APLY_TEMP()
                        {
                            ITEM_ID = data.vITEM_ID,
                            ACCESS_TYPE = data.vITeM_OP_TYPE == "2" ? data.vACCESS_TYPE : null,
                            ACCESS_REASON = data.vACCESS_REASON,

                        });
                        logStr += _TREA_APLY_TEMP.modelToString(logStr);
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
                                log.CFUNCTION = "新增申請-入庫人員確認作業";
                                log.CACTION = "A";
                                log.CCONTENT = logStr;
                                LogDao.Insert(log, data.vCurrentUid);
                                #endregion

                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
                            }
                            catch (DbUpdateException ex)
                            {
                                result.DESCRIPTION = ex.exceptionMessage();
                            }
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = "存取項目重覆無法新增";
                    }

                    //var _TREA_APLY_REC = db.TREA_APLY_REC
                    //    .Add(new TREA_APLY_REC()
                    //    {
                    //        APLY_NO = applyNO, ////申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                    //        APLY_FROM = "M",                   //人工
                    //        ITEM_ID = data.vITEM_ID,
                    //        ACCESS_TYPE = data.vITeM_OP_TYPE == "2" ? data.vACCESS_TYPE : null,
                    //        ACCESS_REASON = data.vACCESS_REASON,
                    //        APLY_STATUS = applyStatus,
                    //        APLY_UID = data.vCurrentUid,
                    //        APLY_DT = now
                    //    });
                    //logStr += _TREA_APLY_REC.modelToString(logStr);
                    //if (data.vITeM_OP_TYPE == "2")
                    //{
                    //    #region 其它存取項目申請資料檔
                    //    var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY
                    //        .Add(new OTHER_ITEM_APLY()
                    //        {
                    //            APLY_NO = applyNO,
                    //            ITEM_ID = data.vSEAL_ITEM_ID
                    //        });
                    //    logStr += _OTHER_ITEM_APLY.modelToString(logStr);
                    //    #endregion
                    //}
                    //#region 申請單歷程檔
                    //var _APLY_REC_HIS = db.APLY_REC_HIS
                    //    .Add(new APLY_REC_HIS()
                    //    {
                    //        APLY_NO = applyNO,
                    //        PROC_DT = now,
                    //        APLY_STATUS = applyStatus,
                    //        PROC_UID = data.vCurrentUid
                    //    });
                    //logStr += _APLY_REC_HIS.modelToString(logStr);
                    //#endregion
   
                }
                if (result.RETURN_FLAG)
                {
                    result.Datas = GetSearchDetail(searchData);
                }
            }
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> UpdateData(ConfirmStorageInsertViewModel data, ConfirmStorageSearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            if (data.vITeM_OP_TYPE != "3")
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    string logStr = string.Empty;
                    #region 申請單暫存檔
                    var _TREA_APLY_TEMP = db.TREA_APLY_TEMP.FirstOrDefault(x => x.ITEM_ID == data.vITEM_ID);
                    _TREA_APLY_TEMP.ACCESS_TYPE = data.vACCESS_TYPE;
                    _TREA_APLY_TEMP.ACCESS_REASON = data.vACCESS_REASON;
                    #endregion

                    logStr += _TREA_APLY_TEMP.modelToString(logStr);

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
                            log.CFUNCTION = "修改-入庫人員確認作業";
                            log.CACTION = "U";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, data.vCurrentUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
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
        /// 刪除
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> DeteleData(ConfirmStorageDeleteViewModel data, ConfirmStorageSearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
            string logStr = string.Empty;
            if (searchData.v_IS_CHECKED == null)
            {
                //if (data.vITeM_OP_TYPE != "3")
                //{

                //    using (TreasuryDBEntities db = new TreasuryDBEntities())
                //    {
                //        var _TREA_APLY_TEMP = db.TREA_APLY_TEMP.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == data.vITEM_ID);

                //        var DeleteData = db.TREA_APLY_TEMP.Remove(_TREA_APLY_TEMP);
                //        logStr += DeleteData.modelToString(logStr);

                //        var validateMessage = db.GetValidationErrors().getValidateString();
                //        if (validateMessage.Any())
                //        {
                //            result.DESCRIPTION = validateMessage;
                //        }
                //        else
                //        {
                //            try
                //            {
                //                db.SaveChanges();

                //                #region LOG
                //                //新增LOG
                //                Log log = new Log();
                //                log.CFUNCTION = "刪除-入庫人員確認作業";
                //                log.CACTION = "D";
                //                log.CCONTENT = logStr;
                //                LogDao.Insert(log, data.vCurrentUid);
                //                #endregion

                //                result.RETURN_FLAG = true;
                //                result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();

                //            }
                //            catch (DbUpdateException ex)
                //            {
                //                result.DESCRIPTION = ex.exceptionMessage();
                //            }
                //        }
                //    }
                //}
            }
            else
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == data.vAPLY_NO);
                    if (data.vITeM_OP_TYPE == "2")
                    {
                        var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.FirstOrDefault(x => x.APLY_NO == TREA_APLY_REC.APLY_NO);
                        var _INVENTORY_STATUS = string.Empty;
                        var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.ITEM_ID);
                        switch (TREA_APLY_REC.ACCESS_TYPE)
                        {
                            //存入
                            case "P":
                                _INVENTORY_STATUS = "6";
                                break;
                            //取出,用印
                            case "G":
                            case "S":
                                _INVENTORY_STATUS = "1";
                                break;
                            //存入用印
                            case "A":
                                _INVENTORY_STATUS = "2";
                                break;
                            //取出存入
                            case "B":
                                _INVENTORY_STATUS = "1";
                                break;
                        }
                        _ITEM_SEAL.INVENTORY_STATUS = _INVENTORY_STATUS;
                    }
                    db.APLY_REC_HIS.RemoveRange(db.APLY_REC_HIS.Where(x => x.APLY_NO == TREA_APLY_REC.APLY_NO));                
                    var DeleteData = db.TREA_APLY_REC.Remove(TREA_APLY_REC);
                    logStr += DeleteData.modelToString(logStr);            
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
                            log.CFUNCTION = "刪除-入庫人員確認作業";
                            log.CACTION = "D";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, data.vCurrentUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();

                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
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
        /// 確認入庫
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> ConfirmData(List<string> data, ConfirmStorageSearchViewModel searchData, List<ConfirmStorageSearchDetailViewModel> viewData, string cUserId,string register_ID)
        {
            var result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.Check_Fail.GetDescription();

            if (data != null)
            {
                DateTime now = DateTime.Now;
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(searchData.vCREATE_DT).DateToLatestTime();
                string logStr = string.Empty;
                string applyStatus = "C02";
                bool _flag = true;
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    data.ForEach(x => {
                        var vItemId = x.Split(';')[0];
                        var vOpType = x.Split(';')[1];
                        var vAply = x.Split(';')[2];
                        var rowdata = viewData.Where(y => y.vITEM_ID == vItemId)
                        .Where(y => y.vITeM_OP_TYPE == vOpType)
                        .Where(y => y.vAPLY_NO == vAply, !vAply.IsNullOrWhiteSpace())
                        .FirstOrDefault();

                        if (_flag && vOpType == "2" && (
                        rowdata.vSEAL_ITEM_ID.IsNullOrWhiteSpace() || 
                        rowdata.vACCESS_TYPE_CODE.IsNullOrWhiteSpace()))
                        {
                            result.DESCRIPTION = "請輸入印章內容、作業別!!";
                            _flag = false;
                        }
                        else
                        {
                            if(_flag && vOpType == "4" && rowdata.vACCESS_REASON.IsNullOrWhiteSpace())
                            {
                                result.DESCRIPTION = "請輸入入庫原因!!";
                                _flag = false;
                            }
                            else
                            {
                                if (_flag && vOpType == "3")
                                {
                                    var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(y => y.APLY_NO == vAply);
                                    _TREA_APLY_REC.APLY_STATUS = applyStatus;
                                    _TREA_APLY_REC.CONFIRM_UID = cUserId;
                                    _TREA_APLY_REC.CONFIRM_DT = now;
                                    _TREA_APLY_REC.LAST_UPDATE_DT = now;
                                    _TREA_APLY_REC.LAST_UPDATE_UID = cUserId;
                                    _TREA_APLY_REC.TREA_REGISTER_ID = register_ID;
                                    logStr += _TREA_APLY_REC.modelToString(logStr);

                                    var _APLY_REC_HIS = db.APLY_REC_HIS
                                        .Add(new APLY_REC_HIS()
                                        {
                                            APLY_NO = vAply,
                                            PROC_DT = now,
                                            APLY_STATUS = applyStatus,
                                            PROC_UID = cUserId
                                        });
                                    logStr += _APLY_REC_HIS.modelToString(logStr);
                                }
                                else if(_flag)
                                {
                                    SysSeqDao sysSeqDao = new SysSeqDao();
                                    string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                                    var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');
                                    string applyNO = $@"G6{qPreCode}{cId}";
                                    //var _code = db.SYS_CODE.AsNoTracking().Where(y => y.CODE_TYPE == "ACCESS_TYPE").Where(y => y.CODE == rowdata.vACCESS_TYPE_CODE).FirstOrDefault()?.CODE;
                                    var _TREA_APLY_REC = db.TREA_APLY_REC
                                       .Add(new TREA_APLY_REC()
                                       {
                                           APLY_NO = applyNO,
                                           APLY_FROM = "M",
                                           ITEM_ID =  vItemId,
                                           //ITEM_ID = vOpType == "2" ? vItemId + rowdata.vSEAL_ITEM_ID : vItemId,
                                           ACCESS_TYPE = vOpType == "2" ? rowdata.vACCESS_TYPE_CODE : null,
                                           ACCESS_REASON = vOpType == "4" ? rowdata.vACCESS_REASON : null,
                                           APLY_STATUS = applyStatus,
                                           APLY_UID = cUserId,
                                           APLY_DT = now,
                                           CREATE_UID = cUserId,
                                           CREATE_DT = now,
                                           TREA_REGISTER_ID = register_ID
                                       });
                                    logStr += _TREA_APLY_REC.modelToString(logStr);

                                    var _APLY_REC_HIS = db.APLY_REC_HIS
                                      .Add(new APLY_REC_HIS()
                                      {
                                          APLY_NO = applyNO,
                                          PROC_DT = now,
                                          APLY_STATUS = applyStatus,
                                          PROC_UID = cUserId
                                      });
                                    logStr += _APLY_REC_HIS.modelToString(logStr);

                                    if (vOpType == "2")
                                    {
                                        #region 其它存取項目申請資料檔
                                        var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY
                                            .Add(new OTHER_ITEM_APLY()
                                            {
                                                APLY_NO = applyNO,
                                                ITEM_ID = rowdata.vSEAL_ITEM_ID
                                            });
                                        logStr += _OTHER_ITEM_APLY.modelToString(logStr);
                                        #endregion
                                        var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == rowdata.vSEAL_ITEM_ID);
                                        var _INVENTORY_STATUS = string.Empty;
                                       
                                         switch (rowdata.vACCESS_TYPE)
                                        {
                                                 //存入
                                            case "P":
                                                _INVENTORY_STATUS = "9";
                                                break;
                                                //取出,用印
                                            case "G":
                                            case "S":
                                                _INVENTORY_STATUS = "5";
                                                break;
                                                //存入用印
                                            case "A":
                                                _INVENTORY_STATUS = "3";
                                                break;
                                                //取出存入
                                            case "B":
                                                _INVENTORY_STATUS = "1";
                                                break;
                                        }
                                        _ITEM_SEAL.INVENTORY_STATUS = _INVENTORY_STATUS;
                                        logStr += _ITEM_SEAL.modelToString(logStr);
                                    }

                                    var _deleteData = db.TREA_APLY_TEMP.FirstOrDefault(y => y.ITEM_ID == rowdata.vITEM_ID);
                                    if (_deleteData != null)
                                    {
                                        db.TREA_APLY_TEMP.Remove(_deleteData);
                                        logStr += _deleteData.modelToString(logStr);
                                    }
                                }
                            }
                        }

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
                            if (_flag)
                            {
                                db.SaveChanges();

                                #region LOG
                                //新增LOG
                                Log log = new Log();
                                log.CFUNCTION = "確認入庫-入庫人員確認作業";
                                log.CACTION = "A";
                                log.CCONTENT = logStr;
                                LogDao.Insert(log, cUserId);
                                #endregion

                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = "入庫確認成功!";
                            }
                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
                if (result.RETURN_FLAG)
                {
                    result.Datas  = GetSearchDetail(searchData);
                }
                return result;
            }
            return null;
        }
    }
}