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
using Treasury.Web.Enum;
using System.ComponentModel;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 定期存單
/// 初版作者：20180803 侯蔚鑫
/// 修改歷程：20180803 侯蔚鑫 
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
    public class Deposit : Common, IDeposit
    {
        private List<SelectOption> Currency_List { get; set; }
        private List<SelectOption> Trad_Partners_List { get; set; }

        public Deposit()
        {
            //預設幣別
            Currency_List = new List<SelectOption>()
            {
                new SelectOption() { Value = "AUD", Text = "AUD" },
                new SelectOption() { Value = "CNY", Text = "CNY" },
                new SelectOption() { Value = "EUR", Text = "EUR" },
                new SelectOption() { Value = "GBP", Text = "GBP" },
                new SelectOption() { Value = "JPY", Text = "JPY" },
                new SelectOption() { Value = "NTD", Text = "NTD" },
                new SelectOption() { Value = "USD", Text = "USD" }
            };


            //預設交易對象
            Trad_Partners_List = new List<SelectOption>()
            {
                new SelectOption() { Value = "上海三民", Text = "上海三民" },
                new SelectOption() { Value = "上海二重", Text = "上海二重" },
                new SelectOption() { Value = "上海三重", Text = "上海三重" },
                new SelectOption() { Value = "上海南港", Text = "上海南港" },
                new SelectOption() { Value = "上海營業部", Text = "上海營業部" },
                new SelectOption() { Value = "中信營業部", Text = "中信營業部" },
                new SelectOption() { Value = "王道營業部", Text = "王道營業部" },
                new SelectOption() { Value = "台企仁愛", Text = "台企仁愛" },
                new SelectOption() { Value = "永豐新湖(營業部發單)", Text = "永豐新湖(營業部發單)" },
                new SelectOption() { Value = "交銀台北", Text = "交銀台北" },
                new SelectOption() { Value = "合庫信維", Text = "合庫信維" },
                new SelectOption() { Value = "星展南京東路", Text = "星展南京東路" },
                new SelectOption() { Value = "高銀台北", Text = "高銀台北" },
                new SelectOption() { Value = "凱基營業部", Text = "凱基營業部" },
                new SelectOption() { Value = "北富銀敦南", Text = "北富銀敦南" },
                new SelectOption() { Value = "匯豐台北", Text = "匯豐台北" },
                new SelectOption() { Value = "新光南東", Text = "新光南東" }
            };

        }

        #region GetData
        /// <summary>
        /// 幣別
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetCurrency()
        {
            var result = new List<SelectOption>();

            var Currency_string = Currency_List.Select(x => x.Text).ToList();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //取出DB定期存單中幣別(不含預設幣別)
                var DB_Currency_List = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => !Currency_string.Contains(x.CURRENCY))
                    .OrderBy(x => x.CURRENCY)
                    .GroupBy(x => x.CURRENCY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.Key,
                        Text = x.Key
                    }).ToList();

                //合併預設幣別及DB定期存單幣別
                result.AddRange(Currency_List);
                result.AddRange(DB_Currency_List);
            }
            return result;
        }

        /// <summary>
        /// 交易對象
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetTrad_Partners()
        {
            var result = new List<SelectOption>();

            var Trad_Partners_string = Trad_Partners_List.Select(x => x.Text).ToList();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //取出DB定期存單中交易對象(不含預設交易對象)
                var DB_Trad_Partners_List = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x=> !Trad_Partners_string.Contains(x.TRAD_PARTNERS))
                    .OrderBy(x => x.TRAD_PARTNERS)
                    .GroupBy(x=>x.TRAD_PARTNERS)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.Key,
                        Text = x.Key
                    }).ToList();

                //合併預設交易對象及DB定期存單交易對象
                result.AddRange(Trad_Partners_List);
                result.AddRange(DB_Trad_Partners_List);
            }
            return result;
        }

        /// <summary>
        /// 計息方式
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetInterest_Rate_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "INTEREST_RATE_TYPE")
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
        /// 存單類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetDep_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DEP_TYPE")
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
        /// 是否標籤
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetYN_Flag()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "YN_FLAG")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 使用 交易對象 抓取在庫定期單明細資料
        /// </summary>
        /// <param name="vTrad_Partners">交易對象</param>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        public List<Deposit_D> GetDataByTradPartners(string vTrad_Partners, string vAplyNo = null)
        {
            var result = new List<Deposit_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //庫存狀態
                List<string> Status = new List<string> { "1", "3", "4", "8" };

                //取得符合交易對象的物品編號
                var ItemIdList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.TRAD_PARTNERS == vTrad_Partners)
                    .Where(x=>Status.Contains(x.INVENTORY_STATUS))
                    .Select(x => x.ITEM_ID).ToList();

                //有申請單時排除申請單號對應物品編號
                if(!vAplyNo.IsNullOrWhiteSpace())
                {
                    List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                    ItemIdList.RemoveAll(x => oldItemIds.Contains(x));
                }

                result.AddRange(GetDetailModel(db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x=>ItemIdList.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    ));
            }

            return result;
        }

        /// <summary>
        /// 使用 交易對象 抓取異動在庫定期單明細資料
        /// </summary>
        /// <param name="vTrad_Partners">交易對象</param>
        /// <param name="vItemId">物品單號</param>
        /// <param name="vData_Seq">明細流水號</param>
        /// <returns></returns>
        public List<CDCDeposit_D> GetCDC_DataByTradPartners(string vTrad_Partners, string vItemId, string vData_Seq)
        {
            var result = new List<CDCDeposit_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //庫存狀態
                List<string> Status = new List<string> { "1", "3", "4", "8" };

                //取得符合交易對象的物品編號
                var ItemIdList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.TRAD_PARTNERS == vTrad_Partners)
                    .Where(x => Status.Contains(x.INVENTORY_STATUS))
                    .Select(x => x.ITEM_ID).ToList();

                int DataSeq = int.Parse(vData_Seq);

                result = db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => ItemIdList.Contains(x.ITEM_ID))
                    .Where(x => x.ITEM_ID != vItemId && x.DATA_SEQ != DataSeq)
                    .AsEnumerable()
                    .Select(x => new CDCDeposit_D()
                    {
                        vItemId = x.ITEM_ID,
                        vData_Seq = x.DATA_SEQ.ToString(),
                        vDep_No_Preamble = x.DEP_NO_PREAMBLE,
                        vDep_No_Preamble_Aft = x.DEP_NO_PREAMBLE_AFT,
                        vDep_No_B = x.DEP_NO_B,
                        vDep_No_B_Aft = x.DEP_NO_B_AFT,
                        vDep_No_E = x.DEP_NO_E,
                        vDep_No_E_Aft = x.DEP_NO_E_AFT,
                        vDep_No_Tail = x.DEP_NO_TAIL,
                        vDep_No_Tail_Aft = x.DEP_NO_TAIL_AFT,
                        vDep_Cnt = x.DEP_CNT,
                        vDep_Cnt_Aft = x.DEP_CNT_AFT,
                        vDenomination = x.DENOMINATION,
                        vDenomination_Aft = x.DENOMINATION_AFT,
                        vSubtotal_Denomination = x.SUBTOTAL_DENOMINATION,
                        vSubtotal_Denomination_Aft = x.SUBTOTAL_DENOMINATION_AFT
                    }).ToList();

            }

            return result;
        }
        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <param name="vTransExpiryDateFrom">定存單到期日(起)</param>
        /// <param name="vTransExpiryDateTo">定存單到期日(迄)</param>
        /// <returns></returns>
        public DepositViewModel GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null, string vTransExpiryDateFrom = null, string vTransExpiryDateTo = null)
        {
            var result = new DepositViewModel();
            List<Deposit_M> MasterDataList = new List<Deposit_M>();
            List<Deposit_D> DetailDataList = new List<Deposit_D>();
            DateTime DateFrom, DateTo;

            DateTime.TryParse(vTransExpiryDateFrom, out DateFrom);
            DateTime.TryParse(vTransExpiryDateTo, out DateTo);

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();

                #region 取得總項資料
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    MasterDataList = GetMasterModel(
                        db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                        .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                        .Where(x => x.TRANS_EXPIRY_DATE >= DateFrom, !vTransExpiryDateFrom.IsNullOrWhiteSpace())
                        .Where(x => x.TRANS_EXPIRY_DATE <= DateTo, !vTransExpiryDateTo.IsNullOrWhiteSpace())
                        .Where(x => x.INVENTORY_STATUS == "1") //庫存
                        .AsEnumerable()
                        , false, _Inventory_types).ToList();
                }
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList();

                    MasterDataList.AddRange(
                        GetMasterModel(
                            db.ITEM_DEP_ORDER_M.AsNoTracking()
                            .Where(x => itemIds.Contains(x.ITEM_ID))
                            .AsEnumerable()
                            , true, _Inventory_types).ToList()
                        );
                }

                result.vDeposit_M = MasterDataList;
                #endregion

                #region 取得明細資料
                var MasterItemIdList = MasterDataList.Select(x => x.vItem_Id).ToList();

                DetailDataList = GetDetailModel(
                    db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => MasterItemIdList.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    ).ToList();

                result.vDeposit_D = DetailDataList;
                #endregion
            }

            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        public DepositViewModel GetDataByAplyNo(string vAplyNo)
        {
            var result = new DepositViewModel();
            List<Deposit_M> MasterDataList = new List<Deposit_M>();
            List<Deposit_D> DetailDataList = new List<Deposit_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    //使用物品編號去定期存單庫存資料明細檔抓取資料
                    var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    if (_IDOM_DataList.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        int vRowNum = 1;
                        foreach (var MasterData in _IDOM_DataList)
                        {
                            MasterDataList.AddRange(
                                GetMasterModel(
                                _IDOM_DataList.Where(x => x.ITEM_ID == MasterData.ITEM_ID).ToList()
                                , true, _Inventory_types, vRowNum.ToString()).ToList()
                                );

                            DetailDataList.AddRange(
                                GetDetailModel(
                                _IDOD_DataList.Where(x => x.ITEM_ID == MasterData.ITEM_ID).ToList()
                                , vRowNum.ToString()).ToList()
                                );

                            vRowNum++;
                        }

                        result.vDeposit_M = MasterDataList;
                        result.vDeposit_D = DetailDataList;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 依申請單號取得列印群組筆數
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        public List<DepositReportGroupData> GetReportGroupData(string vAplyNo)
        {
            List<DepositReportGroupData> result = new List<DepositReportGroupData>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    //台幣
                    var GroupDataList_NTD = _IDOM_DataList
                        .Where(x => x.CURRENCY == "NTD" && x.DEP_TYPE != null)
                        .GroupBy(x => new { x.DEP_TYPE }).ToList();

                    if (GroupDataList_NTD.Any())
                    {
                        if (GroupDataList_NTD.Count >= 2) //一般+NCD
                        {
                            result.Add(new DepositReportGroupData { isNTD = "Y", vDep_Type = "0" });
                        }
                        else if (GroupDataList_NTD.Any(x => x.Key.DEP_TYPE == "1")) //一般
                        {
                            result.Add(new DepositReportGroupData { isNTD = "Y", vDep_Type = "1" });
                        }
                        else if (GroupDataList_NTD.Any(x => x.Key.DEP_TYPE == "2")) //NCD
                        {
                            result.Add(new DepositReportGroupData { isNTD = "Y", vDep_Type = "2" });
                        }
                    }

                    //外幣
                    var GroupDataList_NNTD = _IDOM_DataList
                        .Where(x => x.CURRENCY != "NTD" && x.DEP_TYPE != null)
                        .GroupBy(x => new { x.DEP_TYPE }).ToList();

                    if (GroupDataList_NNTD.Any())
                    {
                        if (GroupDataList_NNTD.Count >= 2) //一般+NCD
                        {
                            result.Add(new DepositReportGroupData { isNTD = "N", vDep_Type = "0" });
                        }
                        else if (GroupDataList_NNTD.Any(x => x.Key.DEP_TYPE == "1")) //一般
                        {
                            result.Add(new DepositReportGroupData { isNTD = "N", vDep_Type = "1" });
                        }
                        else if (GroupDataList_NNTD.Any(x => x.Key.DEP_TYPE == "2")) //NCD
                        {
                            result.Add(new DepositReportGroupData { isNTD = "N", vDep_Type = "2" });
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查詢定期存單交易對象
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetTRAD_Partners()
        {
            //預設交易對象
            var result = new List<SelectOption>()
            {
                new SelectOption() { Text = "上海三民", Value = "上海三民" },
                new SelectOption() { Text = "上海二重", Value = "上海二重" },
                new SelectOption() { Text = "上海三重", Value = "上海三重" },
                new SelectOption() { Text = "上海南港", Value = "上海南港" },
                new SelectOption() { Text = "上海營業部", Value = "上海營業部" },
                new SelectOption() { Text = "中信營業部", Value = "中信營業部" },
                new SelectOption() { Text = "王道營業部", Value = "王道營業部" },
                new SelectOption() { Text = "台企仁愛", Value = "台企仁愛" },
                new SelectOption() { Text = "永豐新湖(營業部發單)", Value = "永豐新湖(營業部發單)" },
                new SelectOption() { Text = "交銀台北", Value = "交銀台北" },
                new SelectOption() { Text = "合庫信維", Value = "合庫信維" },
                new SelectOption() { Text = "星展南京東路", Value = "星展南京東路" },
                new SelectOption() { Text = "高銀台北", Value = "高銀台北" },
                new SelectOption() { Text = "凱基營業部", Value = "凱基營業部" },
                new SelectOption() { Text = "北富銀敦南", Value = "北富銀敦南" },
                new SelectOption() { Text = "匯豐台北", Value = "匯豐台北" },
                new SelectOption() { Text = "新光南東", Value = "新光南東" },
            };

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.TRAD_PARTNERS != null && x.TRAD_PARTNERS.Trim() != "")
                    .Select(x => x.TRAD_PARTNERS)
                    .AsEnumerable().Select(x => new SelectOption() { Text = x, Value = x }));
            }

            result = result.Distinct(new SelectOption_Comparer()).OrderBy(x => x.Value).ToList();

            return result;
        }

        /// <summary>
        /// 查詢CDC資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="aply_No">資料庫異動申請單紀錄檔  INVENTORY_CHG_APLY 單號</param>
        /// <returns></returns>
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null)
        {
            List<CDCDepositViewModel> result = new List<CDCDepositViewModel>();
            List<CDCDeposit_M> CDC_MasterDataList = new List<CDCDeposit_M>();
            List<CDCDeposit_D> CDC_DetailDataList = new List<CDCDeposit_D>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var PUT_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_From);
                    var PUT_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_To).DateToLatestTime();
                    var GET_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_From);
                    var GET_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_To).DateToLatestTime();
                    var Commit_Date = TypeTransfer.stringToDateTime(searchModel.vCommit_Date);
                    var Expiry_Date= TypeTransfer.stringToDateTime(searchModel.vExpiry_Date);

                    CDC_MasterDataList.AddRange(db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.COMMIT_DATE == Commit_Date, searchModel.vCommit_Date != null)
                        .Where(x => x.EXPIRY_DATE == Expiry_Date, searchModel.vExpiry_Date != null)
                        .Where(x => x.TRAD_PARTNERS == searchModel.vTRAD_Partners, searchModel.vTRAD_Partners != "All")
                        .AsEnumerable()
                        .Select((x) => new CDCDeposit_M()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCurrency = x.CURRENCY,
                            vCurrency_Aft = x.CURRENCY_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_Aft = x.TRAD_PARTNERS_AFT,
                            vCommit_Date = TypeTransfer.dateTimeToString(x.COMMIT_DATE, false),
                            vCommit_Date_Aft = TypeTransfer.dateTimeNToString(x.COMMIT_DATE_AFT),
                            vExpiry_Date = TypeTransfer.dateTimeToString(x.EXPIRY_DATE, false),
                            vExpiry_Date_Aft = TypeTransfer.dateTimeNToString(x.EXPIRY_DATE_AFT),
                            vInterest_Rate_Type = x.INTEREST_RATE_TYPE,
                            vInterest_Rate_Type_Aft = x.INTEREST_RATE_TYPE_AFT,
                            vInterest_Rate = x.INTEREST_RATE,
                            vInterest_Rate_Aft = x.INTEREST_RATE_AFT,
                            vDep_Type = x.DEP_TYPE,
                            vDep_Type_Aft = x.DEP_TYPE_AFT,
                            vTotal_Denomination = x.TOTAL_DENOMINATION,
                            vTotal_Denomination_Aft = x.TOTAL_DENOMINATION_AFT,
                            vDep_Set_Quality = x.DEP_SET_QUALITY,
                            vDep_Set_Quality_Aft = x.DEP_SET_QUALITY_AFT,
                            vAuto_Trans = x.AUTO_TRANS,
                            vAuto_Trans_Aft = x.AUTO_TRANS_AFT,
                            vTrans_Expiry_Date = TypeTransfer.dateTimeNToString(x.TRANS_EXPIRY_DATE),
                            vTrans_Expiry_Date_Aft = TypeTransfer.dateTimeNToString(x.TRANS_EXPIRY_DATE_AFT),
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vTrans_Tms = x.TRANS_TMS,
                            vTrans_Tms_Aft = x.TRANS_TMS_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = CDC_MasterDataList.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidName(itemIds);
                            CDC_MasterDataList.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.FirstOrDefault(y => y.itemId == x.vItemId)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    CDC_MasterDataList.AddRange(db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCDeposit_M()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCurrency = x.CURRENCY,
                            vCurrency_Aft = x.CURRENCY_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_Aft = x.TRAD_PARTNERS_AFT,
                            vCommit_Date = TypeTransfer.dateTimeToString(x.COMMIT_DATE, false),
                            vCommit_Date_Aft = TypeTransfer.dateTimeNToString(x.COMMIT_DATE_AFT),
                            vExpiry_Date = TypeTransfer.dateTimeToString(x.EXPIRY_DATE, false),
                            vExpiry_Date_Aft = TypeTransfer.dateTimeNToString(x.EXPIRY_DATE_AFT),
                            vInterest_Rate_Type = x.INTEREST_RATE_TYPE,
                            vInterest_Rate_Type_Aft = x.INTEREST_RATE_TYPE_AFT,
                            vInterest_Rate = x.INTEREST_RATE,
                            vInterest_Rate_Aft = x.INTEREST_RATE_AFT,
                            vDep_Type = x.DEP_TYPE,
                            vDep_Type_Aft = x.DEP_TYPE_AFT,
                            vTotal_Denomination = x.TOTAL_DENOMINATION,
                            vTotal_Denomination_Aft = x.TOTAL_DENOMINATION_AFT,
                            vDep_Set_Quality = x.DEP_SET_QUALITY,
                            vDep_Set_Quality_Aft = x.DEP_SET_QUALITY_AFT,
                            vAuto_Trans = x.AUTO_TRANS,
                            vAuto_Trans_Aft = x.AUTO_TRANS_AFT,
                            vTrans_Expiry_Date = TypeTransfer.dateTimeNToString(x.TRANS_EXPIRY_DATE),
                            vTrans_Expiry_Date_Aft = TypeTransfer.dateTimeNToString(x.TRANS_EXPIRY_DATE_AFT),
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vTrans_Tms = x.TRANS_TMS,
                            vTrans_Tms_Aft = x.TRANS_TMS_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                CDC_MasterDataList.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCharge_Sect_Name.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name : x.vCharge_Dept_Name;
                });

                #region 取得明細資料
                var MasterItemIdList = CDC_MasterDataList.Select(x => x.vItemId).ToList();

                CDC_DetailDataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => MasterItemIdList.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    .Select((x) => new CDCDeposit_D()
                    {
                        vItemId = x.ITEM_ID,
                        vData_Seq = x.DATA_SEQ.ToString(),
                        vDep_No_Preamble = x.DEP_NO_PREAMBLE,
                        vDep_No_Preamble_Aft = x.DEP_NO_PREAMBLE_AFT,
                        vDep_No_B = x.DEP_NO_B,
                        vDep_No_B_Aft = x.DEP_NO_B_AFT,
                        vDep_No_E = x.DEP_NO_E,
                        vDep_No_E_Aft = x.DEP_NO_E_AFT,
                        vDep_No_Tail = x.DEP_NO_TAIL,
                        vDep_No_Tail_Aft = x.DEP_NO_TAIL_AFT,
                        vDep_Cnt = x.DEP_CNT,
                        vDep_Cnt_Aft = x.DEP_CNT_AFT,
                        vDenomination = x.DENOMINATION,
                        vDenomination_Aft = x.DENOMINATION_AFT,
                        vSubtotal_Denomination = x.SUBTOTAL_DENOMINATION,
                        vSubtotal_Denomination_Aft = x.SUBTOTAL_DENOMINATION_AFT
                    }).ToList();
                #endregion

            }

            result.Add(new CDCDepositViewModel() { vDeposit_M = CDC_MasterDataList, vDeposit_D = CDC_DetailDataList });

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
                    var datas = (List<DepositViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();

                            string logStr = string.Empty; //log
                            var item_Seq = "E6"; //定期存單流水號開頭編碼    
                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請

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
                                var _first = datas.First();
                                var MasterDataList = _first.vDeposit_M;
                                foreach (var item in MasterDataList)
                                {
                                    var _IDOM_Item_Id = string.Empty;
                                    var _IDOM = new ITEM_DEP_ORDER_M();
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            _IDOM = new ITEM_DEP_ORDER_M()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}",  //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                CURRENCY = item.vCurrency,  //幣別
                                                TRAD_PARTNERS = item.vTrad_Partners,    //交易對象
                                                COMMIT_DATE = DateTime.Parse(item.vCommit_Date),    //承作日期
                                                EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date),    //到期日
                                                INTEREST_RATE_TYPE = item.vInterest_Rate_Type,  //計息方式
                                                INTEREST_RATE = item.vInterest_Rate,    //利率%
                                                DEP_TYPE = item.vDep_Type,  //存單類型
                                                TOTAL_DENOMINATION = item.vTotal_Denomination,  //總面額
                                                DEP_SET_QUALITY = item.vDep_Set_Quality,    //設質否(N/Y)
                                                AUTO_TRANS = item.vAuto_Trans,  //自動轉期(N/Y)
                                                TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date),    //轉期後到期日
                                                TRANS_TMS = item.vTrans_Tms,    //轉期次數
                                                MEMO = item.vMemo,  //備註
                                                APLY_DEPT = _dept.Item1,    //申請人部門
                                                APLY_SECT = _dept.Item2,    //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1,  //權責部門
                                                CHARGE_SECT = _dept.Item2,  //權責科別
                                                LAST_UPDATE_DT = dt //最後修改時間
                                            };
                                            _IDOM_Item_Id = _IDOM.ITEM_ID;
                                            db.ITEM_DEP_ORDER_M.Add(_IDOM);
                                            logStr += "|";
                                            logStr += _IDOM.modelToString();
                                            #endregion

                                            #region 定期存單庫存資料明細檔
                                            var DetailDataList = _first.vDeposit_D.Where(x => x.vItem_Id == item.vItem_Id).ToList();
                                            int seq = 1;
                                            foreach (var subItem in DetailDataList)
                                            {
                                                var _IDOD = new ITEM_DEP_ORDER_D();
                                                _IDOD = new ITEM_DEP_ORDER_D()
                                                {
                                                    ITEM_ID = _IDOM_Item_Id,    //物品編號
                                                    DATA_SEQ = seq, //明細流水號
                                                    DEP_NO_PREAMBLE = subItem.vDep_No_Preamble, //存單號碼前置碼
                                                    DEP_NO_B = subItem.vDep_No_B,   //存單號碼(起)
                                                    DEP_NO_E = subItem.vDep_No_E,   //存單號碼(迄)
                                                    DEP_NO_TAIL = subItem.vDep_No_Tail, //存單號碼尾碼
                                                    DEP_CNT = subItem.vDep_Cnt, //存單張數
                                                    DENOMINATION = subItem.vDenomination,   //面額
                                                    SUBTOTAL_DENOMINATION = subItem.vSubtotal_Denomination  //面額小計
                                                };
                                                db.ITEM_DEP_ORDER_D.Add(_IDOD);
                                                logStr += "|";
                                                logStr += _IDOD.modelToString();
                                                seq++;
                                            }
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        //只抓取預約取出及預約取出，計庫存
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription() || item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                            _IDOM_Item_Id = _IDOM.ITEM_ID;
                                            if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IDOM.GET_MSG = item.GetMsg;
                                            if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                            {
                                                _IDOM.INVENTORY_STATUS = "4"; //預約取出
                                            }
                                            else if(item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                            {
                                                _IDOM.INVENTORY_STATUS = "5"; //預約取出，計庫存
                                            }
                                            _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                            #endregion
                                        }
                                    }

                                    #region 其它存取項目申請資料檔
                                    if (!_IDOM_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IDOM_Item_Id
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
                                if (CustodyAppr.Contains(_TAR.APLY_STATUS))
                                {
                                    _APLY_STATUS = CustodyConfirmStatus;
                                    _TAR.CUSTODY_UID = AccountController.CurrentUserId; //保管單位直接帶使用者
                                    _TAR.CUSTODY_DT = dt;
                                }
                                else
                                {
                                    if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                        _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                }
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
                                #endregion

                                #region 儲存資料
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_DEP_ORDER_M> inserts_Master = new List<ITEM_DEP_ORDER_M>(); //新增總項資料
                                List<ITEM_DEP_ORDER_D> inserts_Detail = new List<ITEM_DEP_ORDER_D>(); //新增明細資料

                                var _first = datas.First();
                                var MasterDataList = _first.vDeposit_M;

                                foreach (var item in MasterDataList)
                                {
                                    var _IDOM_Item_Id = string.Empty;
                                    var _IDOM = new ITEM_DEP_ORDER_M();
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            if (item.vItem_Id.StartsWith(item_Seq))  //明細修改
                                            {
                                                _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                                _IDOM_Item_Id = _IDOM.ITEM_ID;
                                                if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }

                                                _IDOM.CURRENCY = item.vCurrency;   //幣別
                                                _IDOM.TRAD_PARTNERS = item.vTrad_Partners;   //交易對象
                                                _IDOM.COMMIT_DATE = DateTime.Parse(item.vCommit_Date); //承作日期
                                                _IDOM.EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date); //到期日
                                                _IDOM.INTEREST_RATE_TYPE = item.vInterest_Rate_Type;   //計息方式
                                                _IDOM.INTEREST_RATE = item.vInterest_Rate;   //利率%
                                                _IDOM.DEP_TYPE = item.vDep_Type;    //存單類型
                                                _IDOM.TOTAL_DENOMINATION = item.vTotal_Denomination;    //總面額
                                                _IDOM.DEP_SET_QUALITY = item.vDep_Set_Quality;  //設質否
                                                _IDOM.AUTO_TRANS = item.vAuto_Trans;    //自動轉期
                                                _IDOM.TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date);  //轉期後到期日
                                                _IDOM.TRANS_TMS = item.vTrans_Tms;  //轉期次數
                                                _IDOM.MEMO = item.vMemo; //備註
                                                _IDOM.LAST_UPDATE_DT = dt;   //最後修改時間

                                                updateItemIds.Add(item.vItem_Id);

                                                logStr += "|";
                                                logStr += _IDOM.modelToString();
                                            }
                                            else //明細新增
                                            {
                                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                                _IDOM = new ITEM_DEP_ORDER_M()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}",  //歸檔編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    CURRENCY = item.vCurrency,  //幣別
                                                    TRAD_PARTNERS = item.vTrad_Partners,    //交易對象
                                                    COMMIT_DATE = DateTime.Parse(item.vCommit_Date),    //承作日期
                                                    EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date),    //到期日
                                                    INTEREST_RATE_TYPE = item.vInterest_Rate_Type,  //計息方式
                                                    INTEREST_RATE = item.vInterest_Rate,    //利率%
                                                    DEP_TYPE = item.vDep_Type,  //存單類型
                                                    TOTAL_DENOMINATION = item.vTotal_Denomination,  //總面額
                                                    DEP_SET_QUALITY = item.vDep_Set_Quality,    //設質否(N/Y)
                                                    AUTO_TRANS = item.vAuto_Trans,  //自動轉期(N/Y)
                                                    TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date),    //轉期後到期日
                                                    TRANS_TMS = item.vTrans_Tms,    //轉期次數
                                                    MEMO = item.vMemo,  //備註
                                                    APLY_DEPT = _dept.Item1,    //申請人部門
                                                    APLY_SECT = _dept.Item2,    //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1,  //權責部門
                                                    CHARGE_SECT = _dept.Item2,  //權責科別
                                                    LAST_UPDATE_DT = dt //最後修改時間
                                                };
                                                _IDOM_Item_Id = _IDOM.ITEM_ID;
                                                inserts_Master.Add(_IDOM);
                                                logStr += "|";
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                            #endregion

                                            #region 定期存單庫存資料明細檔
                                            //先刪除定期存單庫存資料明細檔
                                            db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => x.ITEM_ID == item.vItem_Id));

                                            //依輸入資料新增定期存單庫存資料明細檔
                                            var DetailDataList = _first.vDeposit_D.Where(x => x.vItem_Id == item.vItem_Id).ToList();
                                            int seq = 1;
                                            foreach (var subItem in DetailDataList)
                                            {
                                                var _IDOD = new ITEM_DEP_ORDER_D();
                                                _IDOD = new ITEM_DEP_ORDER_D()
                                                {
                                                    ITEM_ID = _IDOM_Item_Id,    //物品編號
                                                    DATA_SEQ = seq, //明細流水號
                                                    DEP_NO_PREAMBLE = subItem.vDep_No_Preamble, //存單號碼前置碼
                                                    DEP_NO_B = subItem.vDep_No_B,   //存單號碼(起)
                                                    DEP_NO_E = subItem.vDep_No_E,   //存單號碼(迄)
                                                    DEP_NO_TAIL = subItem.vDep_No_Tail, //存單號碼尾碼
                                                    DEP_CNT = subItem.vDep_Cnt, //存單張數
                                                    DENOMINATION = subItem.vDenomination,   //面額
                                                    SUBTOTAL_DENOMINATION = subItem.vSubtotal_Denomination  //面額小計
                                                };
                                                inserts_Detail.Add(_IDOD);
                                                logStr += "|";
                                                logStr += _IDOD.modelToString();
                                                seq++;
                                            }
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus)) //判斷申請作業-取出
                                    {
                                        _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                        if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vTakeoutFlag)
                                        {
                                            _IDOM.GET_MSG = item.GetMsg;
                                            if (_IDOM.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                //判斷狀態
                                                if(item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                                {
                                                    _IDOM.INVENTORY_STATUS = "4"; //預約取出
                                                }
                                                else if (item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                                {
                                                    _IDOM.INVENTORY_STATUS = "5"; //預約取出，計庫存
                                                }
                                                _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IDOM.ITEM_ID);
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                            else if (_IDOM.INVENTORY_STATUS == "4"|| _IDOM.INVENTORY_STATUS == "5") //原先為預約取出或預約取出，計庫存
                                            {
                                                updateItemIds.Add(_IDOM.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IDOM.INVENTORY_STATUS == "4" || _IDOM.INVENTORY_STATUS == "5") //原先為在庫
                                            {
                                                _IDOM.INVENTORY_STATUS = "1"; //預約取出
                                                _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_ORDER_M.RemoveRange(db.ITEM_DEP_ORDER_M.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts_Master.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_DEP_ORDER_M.AddRange(inserts_Master);
                                    db.ITEM_DEP_ORDER_D.AddRange(inserts_Detail);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
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
                                    log.CFUNCTION = "申請覆核-新增定期存單";
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
            return Process(db, aply_No, logStr, dt, access_Type, true);
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
            return Process(db, aply_No, logStr, dt, access_Type, false);
        }

        /// <summary>
        /// 庫存異動資料-申請覆核
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ICDCItem>> CDCApplyAudit(IEnumerable<ICDCItem> saveData, CDCSearchViewModel searchModel)
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            string logStr = string.Empty;
            DateTime dt = DateTime.Now;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                bool changFlag = false;
                var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt);
                logStr = _data.Item2;
                foreach (CDCDepositViewModel item in saveData)
                {
                    //定期存單主檔覆核
                    foreach(CDCDeposit_M model in item.vDeposit_M)
                    {
                        var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                        if(_IDOM!=null && !changFlag)
                        {
                            if (_IDOM.LAST_UPDATE_DT > model.vLast_Update_Time || _IDOM.INVENTORY_STATUS != "1")
                            {
                                changFlag = true;
                            }

                            if (!changFlag)
                            {
                                _IDOM.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                                _IDOM.CURRENCY_AFT = model.vCurrency_Aft;
                                _IDOM.TRAD_PARTNERS_AFT = model.vTrad_Partners_Aft;
                                _IDOM.COMMIT_DATE_AFT = string.IsNullOrEmpty(model.vCommit_Date_Aft) ? null : TypeTransfer.stringToDateTimeN(model.vCommit_Date_Aft);
                                _IDOM.EXPIRY_DATE_AFT = string.IsNullOrEmpty(model.vExpiry_Date_Aft) ? null : TypeTransfer.stringToDateTimeN(model.vExpiry_Date_Aft);
                                _IDOM.INTEREST_RATE_TYPE_AFT = model.vInterest_Rate_Type_Aft;
                                _IDOM.INTEREST_RATE_AFT = model.vInterest_Rate_Aft;
                                _IDOM.DEP_TYPE_AFT = model.vDep_Type_Aft;
                                _IDOM.TOTAL_DENOMINATION_AFT = model.vTotal_Denomination_Aft;
                                _IDOM.DEP_SET_QUALITY_AFT = model.vDep_Set_Quality_Aft;
                                _IDOM.AUTO_TRANS_AFT = model.vAuto_Trans_Aft;
                                _IDOM.TRANS_EXPIRY_DATE_AFT = string.IsNullOrEmpty(model.vTrans_Expiry_Date_Aft) ? null : TypeTransfer.stringToDateTimeN(model.vTrans_Expiry_Date_Aft);
                                _IDOM.TRANS_TMS_AFT = model.vTrans_Tms_Aft;
                                _IDOM.MEMO_AFT = model.vMemo_Aft;
                                _IDOM.LAST_UPDATE_DT = dt;

                                logStr = _IDOM.modelToString(logStr);

                                //定期存單明細覆核
                                foreach (CDCDeposit_D model_D in item.vDeposit_D.Where(x => x.vItemId == model.vItemId).ToList())
                                {
                                    int DataSeq = int.Parse(model_D.vData_Seq);
                                    var _IDOD = db.ITEM_DEP_ORDER_D.FirstOrDefault(x => x.ITEM_ID == model_D.vItemId && x.DATA_SEQ == DataSeq);
                                    _IDOD.DEP_NO_PREAMBLE_AFT = model_D.vDep_No_Preamble_Aft;
                                    _IDOD.DEP_NO_B_AFT = model_D.vDep_No_B_Aft;
                                    _IDOD.DEP_NO_E_AFT = model_D.vDep_No_E_Aft;
                                    _IDOD.DEP_NO_TAIL_AFT = model_D.vDep_No_Tail_Aft;
                                    _IDOD.DEP_CNT_AFT = model_D.vDep_Cnt_Aft;
                                    _IDOD.DENOMINATION_AFT = model_D.vDenomination_Aft;
                                    _IDOD.SUBTOTAL_DENOMINATION_AFT = model_D.vSubtotal_Denomination_Aft;

                                    logStr = _IDOD.modelToString(logStr);
                                }

                                var _OIA = new OTHER_ITEM_APLY()
                                {
                                    APLY_NO = _data.Item1,
                                    ITEM_ID = _IDOM.ITEM_ID
                                };

                                db.OTHER_ITEM_APLY.Add(_OIA);

                                logStr = _OIA.modelToString(logStr);
                            }
                        }
                        else
                        {
                            changFlag = true;
                        }
                    }
                }

                if (changFlag)
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                }
                else
                {
                    db.SaveChanges();
                    #region LOG
                    //新增LOG
                    Log log = new Log();
                    log.CFUNCTION = "申請覆核-資料庫異動:定期存單";
                    log.CACTION = "A";
                    log.CCONTENT = logStr;
                    LogDao.Insert(log, searchModel.vCreate_Uid);
                    #endregion
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"申請單號:{_data.Item1}");
                    result.Datas = GetCDCSearchData(searchModel);
                }
            }

            return result;
        }

        /// <summary>
        /// 庫存異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Deposit = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Deposit != null)
                {
                    _Deposit.INVENTORY_STATUS = "1"; //在庫
                    _Deposit.CURRENCY_AFT = null;
                    _Deposit.TRAD_PARTNERS_AFT = null;
                    _Deposit.COMMIT_DATE_AFT = null;
                    _Deposit.EXPIRY_DATE_AFT = null;
                    _Deposit.INTEREST_RATE_TYPE_AFT = null;
                    _Deposit.INTEREST_RATE_AFT = null;
                    _Deposit.DEP_TYPE_AFT = null;
                    _Deposit.TOTAL_DENOMINATION_AFT = null;
                    _Deposit.DEP_SET_QUALITY_AFT = null;
                    _Deposit.AUTO_TRANS_AFT = null;
                    _Deposit.TRANS_EXPIRY_DATE_AFT = null;
                    _Deposit.TRANS_TMS_AFT = null;
                    _Deposit.MEMO_AFT = null;
                    _Deposit.LAST_UPDATE_DT = dt;
                    logStr = _Deposit.modelToString(logStr);

                    //明細
                    var _Deposit_D_List = db.ITEM_DEP_ORDER_D.Where(x => x.ITEM_ID == itemID).ToList();
                    foreach(var item_D in _Deposit_D_List)
                    {
                        var _Deposit_D = db.ITEM_DEP_ORDER_D.FirstOrDefault(x => x.ITEM_ID == item_D.ITEM_ID && x.DATA_SEQ == item_D.DATA_SEQ);
                        _Deposit_D.DEP_NO_PREAMBLE_AFT = null;
                        _Deposit_D.DEP_NO_B_AFT = null;
                        _Deposit_D.DEP_NO_E_AFT = null;
                        _Deposit_D.DEP_NO_TAIL_AFT = null;
                        _Deposit_D.DEP_CNT_AFT = null;
                        _Deposit_D.DENOMINATION_AFT = null;
                        _Deposit_D.SUBTOTAL_DENOMINATION_AFT = null;

                        logStr = _Deposit_D.modelToString(logStr);
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Deposit = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Deposit != null)
                {
                    _Deposit.INVENTORY_STATUS = "1"; //在庫
                    _Deposit.CURRENCY = GetNewValue(_Deposit.CURRENCY, _Deposit.CURRENCY_AFT);
                    _Deposit.CURRENCY_AFT = null;
                    _Deposit.TRAD_PARTNERS = GetNewValue(_Deposit.TRAD_PARTNERS, _Deposit.TRAD_PARTNERS_AFT);
                    _Deposit.TRAD_PARTNERS_AFT = null;
                    _Deposit.COMMIT_DATE = TypeTransfer.stringToDateTime(GetNewValue(TypeTransfer.dateTimeToString(_Deposit.COMMIT_DATE), TypeTransfer.dateTimeNToString(_Deposit.COMMIT_DATE_AFT)));
                    _Deposit.COMMIT_DATE_AFT = null;
                    _Deposit.EXPIRY_DATE = TypeTransfer.stringToDateTime(GetNewValue(TypeTransfer.dateTimeToString(_Deposit.EXPIRY_DATE), TypeTransfer.dateTimeNToString(_Deposit.EXPIRY_DATE_AFT)));
                    _Deposit.EXPIRY_DATE_AFT = null;
                    _Deposit.INTEREST_RATE_TYPE = GetNewValue(_Deposit.INTEREST_RATE_TYPE, _Deposit.INTEREST_RATE_TYPE_AFT);
                    _Deposit.INTEREST_RATE_TYPE_AFT = null;
                    _Deposit.INTEREST_RATE = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Deposit.INTEREST_RATE), TypeTransfer.decimalNToString(_Deposit.INTEREST_RATE_AFT)));
                    _Deposit.INTEREST_RATE_AFT = null;
                    _Deposit.DEP_TYPE = GetNewValue(_Deposit.DEP_TYPE, _Deposit.DEP_TYPE_AFT);
                    _Deposit.DEP_TYPE_AFT = null;
                    _Deposit.TOTAL_DENOMINATION = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Deposit.TOTAL_DENOMINATION), TypeTransfer.decimalNToString(_Deposit.TOTAL_DENOMINATION_AFT)));
                    _Deposit.TOTAL_DENOMINATION_AFT = null;
                    _Deposit.DEP_SET_QUALITY = GetNewValue(_Deposit.DEP_SET_QUALITY, _Deposit.DEP_SET_QUALITY_AFT);
                    _Deposit.DEP_SET_QUALITY_AFT = null;
                    _Deposit.AUTO_TRANS = GetNewValue(_Deposit.AUTO_TRANS, _Deposit.AUTO_TRANS_AFT);
                    _Deposit.AUTO_TRANS_AFT = null;
                    _Deposit.TRANS_EXPIRY_DATE = TypeTransfer.stringToDateTimeN(GetNewValue(TypeTransfer.dateTimeNToString(_Deposit.TRANS_EXPIRY_DATE), TypeTransfer.dateTimeNToString(_Deposit.TRANS_EXPIRY_DATE_AFT)));
                    _Deposit.TRANS_EXPIRY_DATE_AFT = null;
                    _Deposit.TRANS_TMS = TypeTransfer.stringToIntN(GetNewValue(TypeTransfer.intNToString(_Deposit.TRANS_TMS), TypeTransfer.intNToString(_Deposit.TRANS_TMS_AFT)));
                    _Deposit.TRANS_TMS_AFT = null;
                    _Deposit.MEMO = GetNewValue(_Deposit.MEMO, _Deposit.MEMO_AFT);
                    _Deposit.MEMO_AFT = null;
                    _Deposit.LAST_UPDATE_DT = dt;
                    logStr = _Deposit.modelToString(logStr);

                    //明細
                    var _Deposit_D_List = db.ITEM_DEP_ORDER_D.Where(x => x.ITEM_ID == itemID).ToList();
                    foreach (var item_D in _Deposit_D_List)
                    {
                        var _Deposit_D = db.ITEM_DEP_ORDER_D.FirstOrDefault(x => x.ITEM_ID == item_D.ITEM_ID && x.DATA_SEQ == item_D.DATA_SEQ);
                        _Deposit_D.DEP_NO_PREAMBLE = GetNewValue(_Deposit_D.DEP_NO_PREAMBLE, _Deposit_D.DEP_NO_PREAMBLE_AFT);
                        _Deposit_D.DEP_NO_PREAMBLE_AFT = null;
                        _Deposit_D.DEP_NO_B = GetNewValue(_Deposit_D.DEP_NO_B, _Deposit_D.DEP_NO_B_AFT);
                        _Deposit_D.DEP_NO_B_AFT = null;
                        _Deposit_D.DEP_NO_E = GetNewValue(_Deposit_D.DEP_NO_E, _Deposit_D.DEP_NO_E_AFT);
                        _Deposit_D.DEP_NO_E_AFT = null;
                        _Deposit_D.DEP_NO_TAIL = GetNewValue(_Deposit_D.DEP_NO_TAIL, _Deposit_D.DEP_NO_TAIL_AFT);
                        _Deposit_D.DEP_NO_TAIL_AFT = null;
                        _Deposit_D.DEP_CNT = TypeTransfer.stringToInt(GetNewValue(TypeTransfer.intNToString(_Deposit_D.DEP_CNT), TypeTransfer.intNToString(_Deposit_D.DEP_CNT_AFT)));
                        _Deposit_D.DEP_CNT_AFT = null;
                        _Deposit_D.DENOMINATION = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Deposit_D.DENOMINATION), TypeTransfer.decimalNToString(_Deposit_D.DENOMINATION_AFT)));
                        _Deposit_D.DENOMINATION_AFT = null;
                        _Deposit_D.SUBTOTAL_DENOMINATION = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Deposit_D.SUBTOTAL_DENOMINATION), TypeTransfer.decimalNToString(_Deposit_D.SUBTOTAL_DENOMINATION_AFT)));
                        _Deposit_D.SUBTOTAL_DENOMINATION_AFT = null;

                        logStr = _Deposit_D.modelToString(logStr);
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        /// <summary>
        /// 申請刪除 & 作廢 定期存單資料庫要處理的事件
        /// </summary>
        /// <param name="db"></param>
        /// <param name="aply_No"></param>
        /// <param name="logStr"></param>
        /// <param name="dt"></param>
        /// <param name="accessType"></param>
        /// <param name="deleFlag"></param>
        /// <returns></returns>
        public Tuple<bool, string> Process(TreasuryDBEntities db, string aply_No, string logStr, DateTime dt, string accessType, bool deleFlag)
        {
            var _changeFlag = false;

            var _TAR = db.TREA_APLY_REC.AsNoTracking()
            .FirstOrDefault(x => x.APLY_NO == aply_No);

            if (_TAR != null)
            {
                //使用單號去其他存取項目檔抓取物品編號
                var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                //使用物品編號去定期存單庫存資料檔抓取資料
                var details = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                if (details.Any())
                {
                    if (accessType == Ref.AccessProjectTradeType.G.ToString()) //取出狀態處理作業
                    {
                        foreach (var item in details)
                        {
                            var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                            _IDOM.INVENTORY_STATUS = "1"; //返回在庫
                            _IDOM.LAST_UPDATE_DT = dt;
                            logStr += _IDOM.modelToString(logStr);
                        }

                        //刪除其他存取項目檔
                        if (deleFlag)
                            db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));

                    }
                    else if (accessType == Ref.AccessProjectTradeType.P.ToString())    //存入狀態處理作業
                    {
                        //判斷申請刪除 & 作廢
                        if (deleFlag)
                        {
                            db.ITEM_DEP_ORDER_M.RemoveRange(db.ITEM_DEP_ORDER_M.Where(x => OIAs.Contains(x.ITEM_ID)));
                            db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => OIAs.Contains(x.ITEM_ID)));
                            db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));
                        }
                        else
                        {
                            foreach (var item in details)
                            {
                                var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                                _IDOM.INVENTORY_STATUS = "7"; //已取消
                                _IDOM.LAST_UPDATE_DT = dt;
                                logStr += _IDOM.modelToString(logStr);
                            }
                        }
                    }
                }
                else
                {
                    _changeFlag = true;
                }
            }
            else
            {
                _changeFlag = true;
            }

            if (_changeFlag)
            {
                return new Tuple<bool, string>(false, logStr);
            }
            else
            {
                return new Tuple<bool, string>(true, logStr);
            }
        }

        /// <summary>
        /// 定期存單資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vTakeoutFlag"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<Deposit_M> GetMasterModel(IEnumerable<ITEM_DEP_ORDER_M> data,Boolean vTakeoutFlag, List<SYS_CODE> _Inventory_types, string vRowNum = null)
        {
            return data.Select(x => new Deposit_M()
            {
                vRowNum = string.IsNullOrEmpty(vRowNum) ? 0 : int.Parse(vRowNum),  //編號
                vItem_Id = x.ITEM_ID,   //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,   //代碼.庫存狀態 
                vCurrency = x.CURRENCY, //幣別
                vTrad_Partners = x.TRAD_PARTNERS,   //交易對象
                vCommit_Date = x.COMMIT_DATE.ToString("yyyy/MM/dd"),    //承作日期
                vExpiry_Date = x.EXPIRY_DATE.ToString("yyyy/MM/dd"),    //到期日
                vInterest_Rate_Type = x.INTEREST_RATE_TYPE, //計息方式
                vInterest_Rate = x.INTEREST_RATE,   //利率%
                vDep_Type = x.DEP_TYPE, //存單類型
                vTotal_Denomination = x.TOTAL_DENOMINATION, //總面額
                vDep_Set_Quality = x.DEP_SET_QUALITY,   //設質否
                vAuto_Trans = x.AUTO_TRANS, //自動轉期
                vTrans_Expiry_Date = DateTime.Parse(x.TRANS_EXPIRY_DATE.ToString()).ToString("yyyy/MM/dd"), //轉期後到期日
                vTrans_Tms = x.TRANS_TMS,   //轉期次數
                vMemo = x.MEMO, //備註
                vTakeoutFlag = vTakeoutFlag,    //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT,    //最後修改時間
                GetMsg = x.GET_MSG, //取出原因
                MsgFlag = x.GET_MSG.IsNullOrWhiteSpace() ? null : "Y" //取出原因註記
            });
        }

        /// <summary>
        /// 在庫定期存單明細資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vRowNum"></param>
        /// <returns></returns>
        private IEnumerable<Deposit_D> GetDetailModel(IEnumerable<ITEM_DEP_ORDER_D> data, string vRowNum = null)
        {
            return data.Select(x => new Deposit_D()
            {
                vRowNum = vRowNum,  //編號
                vItem_Id = x.ITEM_ID,   //物品編號
                vData_Seq = x.DATA_SEQ.ToString(),  //明細流水號
                vDep_No_Preamble = x.DEP_NO_PREAMBLE,   //存單號碼前置碼
                vDep_No_B = x.DEP_NO_B, //存單號碼(起)
                vDep_No_E = x.DEP_NO_E, //存單號碼(迄)
                vDep_No_Tail = x.DEP_NO_TAIL,   //存單號碼尾碼
                vDep_Cnt = x.DEP_CNT,   //存單張數
                vDenomination = x.DENOMINATION, //單張面額
                vSubtotal_Denomination = x.SUBTOTAL_DENOMINATION    //面額小計
            });
        }

        #endregion
    }
}