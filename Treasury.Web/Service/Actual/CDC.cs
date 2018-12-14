using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebDaos;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

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
        /// 查詢資料異動作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<CDCApprSearchDetailViewModel> GetApprSearchDetail(CDCApprSearchViewModel data)
        {           
            return getApprData(data, "N");
        }

        /// <summary>
        /// 查詢權責異動作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<CDCApprSearchDetailViewModel> GetChargeApprSearchDetail(CDCApprSearchViewModel data)
        {
            return getApprData(data, "Y");
        }

        /// <summary>
        /// 權責調整查詢
        /// </summary>
        /// <returns></returns>
        public CDCChargeViewModel GetChargeData() {
            CDCChargeViewModel result = new CDCChargeViewModel();
            List<CDCChargeModel> ChargeData = new List<CDCChargeModel>();
            var searchModel = new CDCSearchViewModel() { vTreasuryIO = "Y" , vJobProject = "All", vTRAD_Partners = "All" , vEstate_Form_No = "All" , vMargin_Dep_Type = "All"};
            #region 空白票據
            List<CDCBillViewModel> _Bill = (List<CDCBillViewModel>)(new Bill().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Bill
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1012,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 電子憑證
            List<CDCCAViewModel> _CA = (List<CDCCAViewModel>)(new CA().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _CA
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1024,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 定期存單
            CDCDepositViewModel _Deposit = ((List<CDCDepositViewModel>)(new Deposit().GetCDCSearchData(searchModel))).First();
            ChargeData.AddRange(
                _Deposit.vDeposit_M
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1013,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 不動產權狀
            List<CDCEstateViewModel> _Estate = (List<CDCEstateViewModel>)(new Estate().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Estate
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1014,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 重要物品
            List<CDCItemImpViewModel> _ItemImp = (List<CDCItemImpViewModel>)(new ItemImp().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _ItemImp
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1018,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 存出保證金
            List<CDCMargingViewModel> _Marging = (List<CDCMargingViewModel>)(new Marging().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Marging
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1016,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 存入保證金
            List<CDCMarginpViewModel> _Marginp = (List<CDCMarginpViewModel>)(new Marginp().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Marginp
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1017,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 印章
            List<CDCSealViewModel> _Seal = (List<CDCSealViewModel>)(new Seal().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Seal
                .GroupBy(x => new { x.vTrea_Item_Name, x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == x.Key.vTrea_Item_Name),
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion
            #region 股票
            List<CDCStockViewModel> _Stock = (List<CDCStockViewModel>)(new Stock().GetCDCSearchData(searchModel));
            ChargeData.AddRange(
                _Stock
                .GroupBy(x => new { x.vCharge_Dept, x.vCharge_Dept_Name, x.vCharge_Sect, x.vCharge_Sect_Name })
                .Select(x => new CDCChargeModel()
                {
                    type = TreaItemType.D1015,
                    vCharge_Dept = x.Key.vCharge_Dept,
                    vCharge_Dept_Name = x.Key.vCharge_Dept_Name,
                    vCharge_Sect = x.Key.vCharge_Sect,
                    vCharge_Sect_Name = x.Key.vCharge_Sect_Name
                }));
            #endregion

            #region 放資料
            result.ChargeData = ChargeData;
            //result.BillData = _Bill;
            //result.CaData = _CA;
            //result.DepositData = _Deposit;
            //result.EstateData = _Estate;
            //result.ItemImpData = _ItemImp;
            //result.MargingData = _Marging;
            //result.MarginpData = _Marginp;
            //result.SealData = _Seal;
            //result.StockData = _Stock;
            #endregion
            return result;
        }

        /// <summary>
        /// 權責單位查詢細項資料
        /// </summary>
        /// <param name="type"></param>
        /// <param name="charge_Dept"></param>
        /// <param name="charge_Sect"></param>
        /// <returns></returns>
        public CDCChargeViewModel GetChargeDetailData(TreaItemType type, string charge_Dept, string charge_Sect )
        {
            CDCChargeViewModel result = new CDCChargeViewModel();
            var searchModel = new CDCSearchViewModel() { vTreasuryIO = "Y",vTRAD_Partners = "All",vEstate_Form_No = "All",vMargin_Dep_Type = "All"};
            switch (type)
            {
                #region 空白票據
                case TreaItemType.D1012:
                    result.BillData = (List<CDCBillViewModel>)(new Bill().GetCDCSearchData(searchModel,null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 電子憑證
                case TreaItemType.D1024:
                    result.CaData = (List<CDCCAViewModel>)(new CA().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 定期存單
                case TreaItemType.D1013:
                    result.DepositData = ((List<CDCDepositViewModel>)(new Deposit().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect))).First();
                    break;
                #endregion
                #region 不動產權狀
                case TreaItemType.D1014:
                    result.EstateData = (List<CDCEstateViewModel>)(new Estate().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 重要物品
                case TreaItemType.D1018:
                    result.ItemImpData = (List<CDCItemImpViewModel>)(new ItemImp().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 存出保證金
                case TreaItemType.D1016:
                    result.MargingData = (List<CDCMargingViewModel>)(new Marging().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 存入保證金
                case TreaItemType.D1017:
                    result.MarginpData = (List<CDCMarginpViewModel>)(new Marginp().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 印章
                case TreaItemType.D1008:
                case TreaItemType.D1009:
                case TreaItemType.D1010:
                case TreaItemType.D1011:
                    searchModel.vJobProject = type.ToString();
                    result.SealData = (List<CDCSealViewModel>)(new Seal().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
                #region 股票
                case TreaItemType.D1015:
                    result.StockData = (List<CDCStockViewModel>)(new Stock().GetCDCSearchData(searchModel, null, charge_Dept, charge_Sect));
                    break;
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 保管單位設定檔 查詢部門
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<SelectOption> GetChargeDept(string type)
        {
            List<SelectOption> result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                result.AddRange(db.ITEM_CHARGE_UNIT.AsNoTracking()
                    .Where(x => x.ITEM_ID == type)
                    .Where(x => x.IS_DISABLED == "N")
                    .Select(x => x.CHARGE_DEPT)
                    .Distinct()
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x,
                        Text = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x)?.DPT_NAME
                    }));
            }
            return result;
        }

        /// <summary>
        /// 保管單位設定檔 查詢科別
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dept"></param>
        /// <returns></returns>
        public List<SelectOption> GetChargeSect(string type, string dept)
        {
            List<SelectOption> result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                result.AddRange(db.ITEM_CHARGE_UNIT.AsNoTracking()
                    .Where(x => x.ITEM_ID == type)
                    .Where(x => x.CHARGE_DEPT == dept,!dept.IsNullOrWhiteSpace())
                    .Where(x => x.IS_DISABLED == "N")
                    .Select(x => x.CHARGE_SECT)
                    .Distinct()
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x,
                        Text = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x)?.DPT_NAME
                    }));
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 權責單位調整申請
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<CDCChargeViewModel> ChargeAppr(CDCChargeViewModel data, CDCSearchViewModel searchModel)
        {
            MSGReturnModel<CDCChargeViewModel> result = new MSGReturnModel<CDCChargeViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();

            string aplyNo = string.Empty; //aplyNo
            string logStr = string.Empty; //log
            DateTime dt = DateTime.Now;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                bool chargeFlag = false; //資料有修改
                bool changFlag = false; //資料已被異動
                try
                {
                    #region 空白票據
                    if (searchModel.vJobProject == TreaItemType.D1012.ToString() && data.BillData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.BillData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
                        {
                            var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_Bill != null && !changFlag)
                            {
                                if (_Bill.LAST_UPDATE_DT > model.vLast_Update_Time || _Bill.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _Bill.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _Bill.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _Bill.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _Bill.LAST_UPDATE_DT = dt;
                                    logStr = _Bill.modelToString(logStr);
                                    var _BNA = new BLANK_NOTE_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _Bill.ITEM_ID,
                                        CHECK_TYPE = string.Empty,
                                        CHECK_NO_TRACK = string.Empty,
                                        CHECK_NO_B = string.Empty,
                                        CHECK_NO_E = string.Empty,
                                    };
                                    db.BLANK_NOTE_APLY.Add(_BNA);
                                    logStr = _BNA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 電子憑證
                    if (searchModel.vJobProject == TreaItemType.D1024.ToString() && data.CaData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.CaData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
                        {
                            var _CA = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_CA != null && !changFlag)
                            {
                                if (_CA.LAST_UPDATE_DT > model.vLast_Update_Time || _CA.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _CA.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _CA.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _CA.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _CA.LAST_UPDATE_DT = dt;

                                    logStr = _CA.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _CA.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 定期存單
                    if (searchModel.vJobProject == TreaItemType.D1013.ToString() && data.DepositData.vDeposit_M.Any(x => x.vAftFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.DepositData.vDeposit_M.Where(x => x.vAftFlag).ToList().ForEach(model =>
                        {
                            var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_IDOM != null && !changFlag)
                            {
                                if (_IDOM.LAST_UPDATE_DT > model.vLast_Update_Time || _IDOM.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _IDOM.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _IDOM.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _IDOM.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _IDOM.LAST_UPDATE_DT = dt;

                                    logStr = _IDOM.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _IDOM.ITEM_ID,
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 不動產權狀
                    if (searchModel.vJobProject == TreaItemType.D1014.ToString() && data.EstateData.Any(x => x.vAftFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.EstateData.Where(x => x.vAftFlag).ToList().ForEach(model =>
                        {
                            var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_Estate != null && !changFlag)
                            {
                                if (_Estate.LAST_UPDATE_DT > model.vLast_Update_Time || _Estate.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _Estate.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _Estate.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _Estate.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _Estate.LAST_UPDATE_DT = dt;

                                    logStr = _Estate.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _Estate.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 重要物品
                    if (searchModel.vJobProject == TreaItemType.D1018.ToString() && data.ItemImpData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.ItemImpData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
                        {
                            var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_ItemImp != null && !changFlag)
                            {
                                if (_ItemImp.LAST_UPDATE_DT > model.vLast_Update_Time || _ItemImp.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _ItemImp.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _ItemImp.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _ItemImp.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _ItemImp.LAST_UPDATE_DT = dt;

                                    logStr = _ItemImp.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _ItemImp.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 存出保證金
                    if (searchModel.vJobProject == TreaItemType.D1016.ToString() && data.MargingData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.MargingData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
                        {
                            var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == model.vItem_PK);
                            if (_Marging != null && !changFlag)
                            {
                                if (_Marging.LAST_UPDATE_DT > model.vLast_Update_Time || _Marging.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _Marging.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _Marging.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _Marging.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _Marging.LAST_UPDATE_DT = dt;

                                    logStr = _Marging.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _Marging.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 存入保證金
                    if (searchModel.vJobProject == TreaItemType.D1017.ToString() && data.MarginpData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.MarginpData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
                        {
                            var _Marginp = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == model.vItem_Id);
                            if (_Marginp != null && !changFlag)
                            {
                                if (_Marginp.LAST_UPDATE_DT > model.vLast_Update_Time || _Marginp.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _Marginp.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _Marginp.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _Marginp.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _Marginp.LAST_UPDATE_DT = dt;

                                    logStr = _Marginp.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _Marginp.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    #region 印章
                    var seals = new List<string>() {
                        TreaItemType.D1008.ToString(),
                        TreaItemType.D1009.ToString(),
                        TreaItemType.D1010.ToString(),
                        TreaItemType.D1011.ToString()
                    };
                    if (seals.Contains(searchModel.vJobProject) && data.SealData.Any(x => x.vAFTFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.SealData.Where(x => x.vAFTFlag).ToList().ForEach(model =>
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
                                    _seal.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _seal.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
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
                        });
                    }
                    #endregion
                    #region 股票
                    if (searchModel.vJobProject == TreaItemType.D1015.ToString() && data.StockData.Any(x => x.vAftFlag))
                    {
                        var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt, "Y");
                        aplyNo = _data.Item1;
                        chargeFlag = true;
                        data.StockData.Where(x => x.vAftFlag).ToList().ForEach(model =>
                        {
                            var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                            if (_Stock != null && !changFlag)
                            {
                                if (_Stock.LAST_UPDATE_DT > model.vLast_Update_Time || _Stock.INVENTORY_STATUS != "1")
                                {
                                    changFlag = true;
                                }
                                if (!changFlag)
                                {
                                    _Stock.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                    _Stock.CHARGE_DEPT_AFT = searchModel.CHARGE_DEPT_AFT;
                                    _Stock.CHARGE_SECT_AFT = searchModel.CHARGE_SECT_AFT;
                                    _Stock.LAST_UPDATE_DT = dt;

                                    logStr = _Stock.modelToString(logStr);

                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _data.Item1,
                                        ITEM_ID = _Stock.ITEM_ID
                                    };

                                    db.OTHER_ITEM_APLY.Add(_OIA);

                                    logStr = _OIA.modelToString(logStr);
                                }
                            }
                            else
                            {
                                changFlag = true;
                            }
                        });
                    }
                    #endregion
                    if (changFlag)
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                    }
                    else if(chargeFlag)
                    {
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
                                log.CFUNCTION = "申請覆核-資料庫權責異動";
                                log.CACTION = "A";
                                log.CCONTENT = logStr;
                                LogDao.Insert(log, searchModel.vCreate_Uid);
                                #endregion
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"申請單號:{aplyNo}");
                            }
                            catch (DbUpdateException ex)
                            {
                                result.DESCRIPTION = ex.exceptionMessage();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = ex.exceptionMessage();
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// 權責覆核畫面覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        public MSGReturnModel<List<CDCApprSearchDetailViewModel>> ChargeApproved(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels)
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
                        var _recover = getCDCAction.CDCChargeApproved(db, itemIds, logStr, dt);
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
                        result.Datas = GetChargeApprSearchDetail(searchData);
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
        /// 權責覆核畫面駁回
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        public MSGReturnModel<List<CDCApprSearchDetailViewModel>> ChargeReject(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels, string apprDesc)
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
                        var _recover = getCDCAction.CDCChargeReject(db, itemIds, logStr, dt);
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

        private List<CDCApprSearchDetailViewModel> getApprData(CDCApprSearchViewModel data, string CHG_AUTH_UNIT)
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
                    .Where(x => x.CHG_AUTH_UNIT == CHG_AUTH_UNIT)
                    .Where(x => x.APLY_NO == data.vAply_No, !data.vAply_No.IsNullOrWhiteSpace())
                    .Where(x => x.CREATE_UID == data.vAply_Uid, !data.vAply_Uid.IsNullOrWhiteSpace())
                    .Where(x => x.APPR_STATUS == aplyStatus)
                    .AsEnumerable()
                    .Select(x => GetCDCApprSearchDetailViewModel(data.vCreateUid, x, treaItems, emps)).ToList();
            }
            return result;
        }

        #endregion
    }
}