namespace Treasury.WebUtility
{
    /// <summary>
    /// cache命名 目的為不重複cache名稱 避免資料被覆蓋
    /// </summary>
    public static class CacheList
    {
        #region Cache資料

        /// <summary>
        /// 金庫物品申取畫面(新增資料欄位)
        /// </summary>
        public static string TreasuryAccessViewData { get; private set; }

        /// <summary>
        /// 金庫物品查詢畫面(查詢條件)
        /// </summary>
        public static string TreasuryAccessSearchData { get; private set; }

        /// <summary>
        /// 保管單位承辦作業(查詢條件)
        /// </summary>
        public static string TreasuryAccessCustodySearchData { get; private set; }

        /// <summary>
        /// 金庫物品查詢畫面(資料)
        /// </summary>
        public static string TreasuryAccessSearchDetailViewData { get; private set; }

        /// <summary>
        /// 保管單位承辦作業(資料)
        /// </summary>
        public static string TreasuryAccessCustodySearchDetailViewData { get; private set; }

        /// <summary>
        /// 金庫物品查詢畫面(可供修改的資料)
        /// </summary>
        public static string TreasuryAccessSearchUpdateViewData { get; private set; }

        /// <summary>
        /// 金庫物品覆核畫面(查詢條件)
        /// </summary>
        public static string TreasuryAccessApprSearchData { get; private set; }

        /// <summary>
        /// 保管單位覆核作業(查詢條件)
        /// </summary>
        public static string TreasuryAccessCustodyApprSearchData { get; private set; }

        /// <summary>
        /// 金庫物品覆核畫面(資料)
        /// </summary>
        public static string TreasuryAccessApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 保管單位覆核作業(資料)
        /// </summary>
        public static string TreasuryAccessCustodyApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 指定開庫作業(查詢條件)
        /// </summary>
        public static string SpecifiedTimeTreasurySearchData { get; private set; }

        /// <summary>
        /// 指定開庫作業(資料)
        /// </summary>
        public static string SpecifiedTimeTreasurySearchDetailViewData { get; private set; }

        /// <summary>5
        /// 指定開庫作業新增申請覆核
        /// </summary>
        public static string SpecifiedTimeTreasuryApplyData { get; private set; }

        /// <summary>
        /// 指定開庫作業覆核畫面(查詢條件)
        /// </summary>
        public static string SpecifiedTimeTreasuryApprSearchData { get; private set; }

        /// <summary>
        /// 指定開庫作業覆核畫面(資料)
        /// </summary>
        public static string SpecifiedTimeTreasuryApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 指定開庫作業覆核畫面(工作項目資料)
        /// </summary>
        public static string SpecifiedTimeTreasuryApprReasonDetailViewData { get; private set; }

        /// <summary>
        /// 金庫登記簿執行作業(開庫前)-每日例行進出未確認項目
        /// </summary>
        public static string BeforeOpenTreasuryRoutine { get; private set; }

        /// <summary>
        /// 金庫登記簿執行作業(開庫前)-已入庫確認資料
        /// </summary>
        public static string BeforeOpenTreasuryStorage { get; private set; }

        /// <summary>
        /// 金庫登記簿查詢列印作業-查詢結果
        /// </summary>
        public static string TreasuryRegisterSearchReportM { get; private set; }

        /// <summary>
        /// 金庫登記簿查詢列印作業-明細
        /// </summary>
        public static string TreasuryRegisterSearchReportD { get; private set; }

        /// <summary>
        /// 入庫確認查詢作業 - 查詢
        /// </summary>
        public static string AlreadyConfirmedSearchData { get; private set; }

        /// <summary>
        /// 入庫確認查詢作業 - 資料
        /// </summary>
        public static string AlreadyConfirmedSearchDetailViewData { get; private set; }

        /// <summary>
        /// 金庫登記簿執行作業(關庫後) - 查詢
        /// </summary>
        public static string AfterOpenTreasurySearchData { get; private set; }

        /// <summary>
        /// 金庫登記簿執行作業(關庫後) - 資料
        /// </summary>
        public static string AfterOpenTreasurySearchDetailViewData { get; private set; }

        /// <summary>
        /// 金庫登記簿執行作業(關庫後) - 未確認表單資料
        /// </summary>
        public static string AfterOpenTreasuryUnconfirmedDetailViewData { get; private set; }

        /// <summary>
        /// 金庫登記簿覆核作業(資料)
        /// </summary>
        public static string TREAReviewWorkDetailViewData { get; private set; }

        /// <summary>
        /// 金庫登記簿覆核作業(單號查資料)
        /// </summary>
        public static string TREAReviewWorkSearchDetailViewData { get; private set; }

        /// <summary>
        /// 入庫人員確認作業(查詢)
        /// </summary>
        public static string ConfirmStorageSearchData { get; private set; }

        /// <summary>
        /// 入庫人員確認作業(資料)
        /// </summary>
        public static string ConfirmStorageSearchDetailViewData { get; private set; }

        /// <summary>
        /// 明細資料(空白票據)
        /// </summary>
        public static string BILLTempData { get; private set; }

        /// <summary>
        /// 當日庫存明細表(空白票據)
        /// </summary>
        public static string BILLDayData { get; private set; }

        /// <summary>
        /// 分頁全部資料(不動產)
        /// </summary>
        public static string ESTATEAllData { get; private set; }

        /// <summary>
        /// 庫存資料(不動產)
        /// </summary>
        public static string ESTATEData { get; private set; }

        /// <summary>
        /// 庫存資料(印章)
        /// </summary>
        public static string SEALData { get; private set; }

        /// <summary>
        /// 股票全部資料(存取項目冊號及股票庫存)
        /// </summary>
        public static string StockData { get; private set; }
        /// <summary>
        /// 庫存資料(股票)
        /// </summary>
        public static string StockMainData { get; private set; }

        /// <summary>
        /// 明細資料(股票)
        /// </summary>
        public static string StockTempData { get; private set; }

        /// <summary>
        /// 庫存資料(電子憑證)
        /// </summary>
        public static string CAData { get; private set; }
        /// <summary>
        /// 庫存資料(存出保證金)
        /// </summary>
        public static string MargingData { get; private set; }

        /// <summary>
        /// 庫存資料(存入保證金)
        /// </summary>
        public static string MarginpData { get; private set; }

        /// <summary>
        /// 庫存資料(重要物品)
        /// </summary>
        public static string ItemImpData { get; private set; }

        /// <summary>
        /// 庫存資料(定期存單-總項)
        /// </summary>
        public static string DepositData_M { get; private set; }

        /// <summary>
        /// 庫存資料(定期存單-總項-設質否=N)
        /// </summary>
        public static string DepositData_N { get; private set; }

        /// <summary>
        /// 庫存資料(定期存單-總項-設質否=Y)
        /// </summary>
        public static string DepositData_Y { get; private set; }

        /// <summary>
        /// 庫存資料(定期存單-明細)
        /// </summary>
        public static string DepositData_D { get; private set; }

        /// <summary>
        /// 庫存資料(定期存單-明細-全)
        /// </summary>
        public static string DepositData_D_All { get; private set; }

        /// <summary>
        /// 定義檔覆核作業覆核畫面(查詢條件)
        /// </summary>
        public static string TDAApprSearchData { get; private set; }

        /// <summary>
        /// 定義檔覆核作業覆核畫面(查詢結果)
        /// </summary>
        public static string TDAApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 定義檔查詢畫面(查詢結果)
        /// </summary>
        public static string TDASearchDetailViewData { get; private set; }

        /// <summary>
        /// 資料庫異動畫面(查詢條件)
        /// </summary>
        public static string CDCSearchViewModel { get; private set; }

        /// <summary>
        /// 資料庫權限異動畫面(查詢條件)
        /// </summary>
        public static string CDCChargeSearchViewModel { get; private set; }

        /// <summary>
        /// 資料庫異動覆核畫面(查詢條件)
        /// </summary>
        public static string CDCApprSearchData { get; private set; }

        /// <summary>
        /// 資料庫權限異動覆核畫面(查詢條件)
        /// </summary>
        public static string CDCChargeApprSearchData { get; private set; }

        /// <summary>
        /// 資料庫異動覆核畫面(資料)
        /// </summary>
        public static string CDCApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 資料庫權限異動覆核畫面(資料)
        /// </summary>
        public static string CDCChargeApprSearchDetailViewData { get; private set; }

        /// <summary>
        /// 資料庫異動印章畫面
        /// </summary>
        public static string CDCSEALData { get; private set; }

        /// <summary>
        /// 資料庫異動空白票據畫面
        /// </summary>
        public static string CDCBILLData { get; private set; }

        /// <summary>
        /// 資料庫異動空白票據在庫
        /// </summary>
        public static string CDCBILLAllData { get; private set; }

        /// <summary>
        /// 資料庫異動電子憑證畫面
        /// </summary>
        public static string CDCCAData { get; private set; }

        /// <summary>
        /// 資料庫異動存出保證金畫面
        /// </summary>
        public static string CDCMargingData { get; private set; }

        /// <summary>
        /// 資料庫異動重要物品畫面
        /// </summary>
        public static string CDCItemImpData { get; private set; }

        /// <summary>
        /// 資料庫異動存入保證金畫面
        /// </summary>
        public static string CDCMarginpData { get; private set; }

        /// <summary>
        /// 資料庫異動不動產權狀畫面
        /// </summary>
        public static string CDCEstateData { get; private set; }

        /// <summary>
        /// 資料庫異動股票畫面批號頁
        /// </summary>
        public static string CDCStockDataM { get; private set; }

        /// <summary>
        /// 資料庫異動股票畫面明細頁
        /// </summary>
        public static string CDCStockDataD { get; private set; }

        /// <summary>
        /// 資料庫異動定期存單畫面(主檔)
        /// </summary>
        public static string CDCDepositDataM { get; private set; }

        /// <summary>
        /// 資料庫異動定期存單畫面(明細)
        /// </summary>
        public static string CDCDepositDataD { get; private set; }

        /// <summary>
        /// 資料庫異動定期存單畫面(明細儲存)
        /// </summary>
        public static string CDCDepositDataD_All { get; private set; }

        /// <summary>
        /// 資料庫權限異動印章畫面
        /// </summary>
        public static string CDCChargeSEALData { get; private set; }

        /// <summary>
        /// 資料庫權限異動空白票據畫面
        /// </summary>
        public static string CDCChargeBILLData { get; private set; }

        /// <summary>
        /// 資料庫權限異動電子憑證畫面
        /// </summary>
        public static string CDCChargeCAData { get; private set; }

        /// <summary>
        /// 資料庫權限異動存出保證金畫面
        /// </summary>
        public static string CDCChargeMargingData { get; private set; }

        /// <summary>
        /// 資料庫權限異動重要物品畫面
        /// </summary>
        public static string CDCChargeItemImpData { get; private set; }

        /// <summary>
        /// 資料庫權限異動存入保證金畫面
        /// </summary>
        public static string CDCChargeMarginpData { get; private set; }

        /// <summary>
        /// 資料庫權限異動不動產權狀畫面
        /// </summary>
        public static string CDCChargeEstateData { get; private set; }

        /// <summary>
        /// 資料庫權限異動股票畫面批號頁
        /// </summary>
        public static string CDCChargeStockDataM { get; private set; }

        /// <summary>
        /// 資料庫權限異動定期存單畫面(主檔)
        /// </summary>
        public static string CDCChargeDepositDataM { get; private set; }

        /// <summary>
        /// 金庫設備維護作業查詢畫面
        /// </summary>
        public static string TreasuryMaintainSearchData { get; private set; }

        /// <summary>
        /// 金庫設備維護作業查詢結果
        /// </summary>
        public static string TreasuryMaintainSearchDataList { get; private set; }

        /// <summary>
        /// 金庫設備維護作業異動紀錄查詢結果
        /// </summary>
        public static string TreasuryMaintainChangeRecordSearchDataList { get; private set; }

        /// <summary>
        /// 定存檢核表項目查詢畫面
        /// </summary>
        public static string DepChkItemSearchData { get; private set; }

        /// <summary>
        /// 定存檢核表項目-存入查詢結果
        /// </summary>
        public static string DepChkItem_P_SearchDataList { get; private set; }

        /// <summary>
        /// 定存檢核表項目-取出查詢結果
        /// </summary>
        public static string DepChkItem_G_SearchDataList { get; private set; }


        /// <summary>
        /// 定存檢核表項目異動紀錄查詢結果
        /// </summary>
        public static string DepChkItemChangeRecordSearchDataList { get; private set; }

        /// <summary>
        /// 定存檢核表項目排序查詢結果
        /// </summary>
        public static string DepChkItemOrderSearchDataList { get; private set; }


        /// <summary>
        /// mail發送內文設定檔維護作業 主畫面查詢條件
        /// </summary>
        public static string TreasuryMailContentSearchData { get; set; }

        /// <summary>
        /// mail發送內文設定檔維護作業 主畫面資料
        /// </summary>
        public static string TreasuryMailContentData { get; set; }

        /// <summary>
        /// mail發送內文設定檔維護作業 明細資料
        /// </summary>
        public static string TreasuryMailContentDetailData { get; set; }

        /// <summary>
        /// mail發送對象設定檔 資料
        /// </summary>
        public static string TreasuryMailContentReceiveData { get; set; }

        /// <summary>
        /// mail發送內文設定檔 異動紀錄查詢結果
        /// </summary>
        public static string TreasuryMailContentChangeRecordData { get; set; }

        /// <summary>
        /// mail發送時間定義檔維護作業 主畫面資料
        /// </summary>
        public static string TreasuryMailTimeData { get; set; }

        /// <summary>
        /// mail發送時間定義檔維護作業 明細資料
        /// </summary>
        public static string TreasuryMailTimeDetailData { get; set; }

        /// <summary>
        /// mail發送時間定義檔維護作業 異動紀錄查詢結果
        /// </summary>
        public static string TreasuryMailTimeChangeRecordData { get; set; }

        /// <summary>
        /// 金庫存取項目維護作業查詢條件
        /// </summary>
        public static string ItemMaintainSearchData { get; private set; }

        /// <summary>
        /// 金庫存取項目維護作業查詢結果
        /// </summary>
        public static string ItemMaintainSearchDetailViewData { get; private set; }
        /// <summary>
        /// 金庫存取項目維護作業異動查詢結果
        /// </summary>
        public static string ItemMaintainChangeRecordSearchDetailViewData { get; private set; }
        
        /// <summary>
        /// 保管資料發送維護作業查詢條件
        /// </summary>
        public static string ItemChargeUnitSearchData { get; private set; }
        /// <summary>
        /// 保管資料發送維護作業查詢結果
        /// </summary>
        public static string ItemChargeUnitSearchDetailViewData { get; private set; }

        /// <summary>
        /// 保管資料發送維護作業異動查詢結果
        /// </summary>
        public static string ItemChargeUnitChangeRecordSearchDetailViewData { get; private set; }

        /// <summary>
        /// Excel 上傳temp 資料
        /// </summary>
        public static string ExcelfileData { get; private set; }

        #endregion Cache資料
        static CacheList()
        {
            #region Cache資料
            TREAReviewWorkDetailViewData = "TREAReviewWorkDetailViewData";
            TREAReviewWorkSearchDetailViewData = "TREAReviewWorkSearchDetailViewData";
            TreasuryAccessViewData = "TreasuryAccessViewData";
            TreasuryAccessSearchData = "TreasuryAccessSearchData";
            TreasuryAccessCustodySearchData = "TreasuryAccessCustodySearchData";
            TreasuryAccessSearchDetailViewData = "TreasuryAccessSearchDetailViewData";
            TreasuryAccessCustodySearchDetailViewData = "TreasuryAccessCustodySearchDetailViewData";
            TreasuryAccessSearchUpdateViewData = "TreasuryAccessSearchUpdateViewData";
            TreasuryAccessApprSearchData = "TreasuryAccessApprSearchData";
            TreasuryAccessCustodyApprSearchData = "TreasuryAccessCustodyApprSearchData";
            TreasuryAccessApprSearchDetailViewData = "TreasuryAccessApprSearchDetailViewData";
            TreasuryAccessCustodyApprSearchDetailViewData = "TreasuryAccessCustodyApprSearchDetailViewData";
            SpecifiedTimeTreasurySearchData = "SpecifiedTimeTreasurySearchData";
            SpecifiedTimeTreasurySearchDetailViewData = "SpecifiedTimeTreasurySearchDetailViewData";
            SpecifiedTimeTreasuryApplyData = "SpecifiedTimeTreasuryApplyData";
            SpecifiedTimeTreasuryApprSearchData = "SpecifiedTimeTreasuryApprSearchData";
            SpecifiedTimeTreasuryApprSearchDetailViewData = "SpecifiedTimeTreasuryApprSearchDetailViewData";
            SpecifiedTimeTreasuryApprReasonDetailViewData = "SpecifiedTimeTreasuryApprReasonDetailViewData";
            AlreadyConfirmedSearchData = "AlreadyConfirmedSearchData";
            AlreadyConfirmedSearchDetailViewData = "AlreadyConfirmedSearchDetailViewData";
            AfterOpenTreasurySearchData = "AfterOpenTreasurySearchData";
            AfterOpenTreasurySearchDetailViewData = "AfterOpenTreasurySearchDetailViewData";
            AfterOpenTreasuryUnconfirmedDetailViewData = "AfterOpenTreasuryUnconfirmedDetailViewData";
            BeforeOpenTreasuryRoutine = "BeforeOpenTreasuryRoutine";
            BeforeOpenTreasuryStorage = "BeforeOpenTreasuryStorage";
            ConfirmStorageSearchData = "ConfirmStorageSearchData";
            ConfirmStorageSearchDetailViewData = "ConfirmStorageSearchDetailViewData";
            BILLTempData = "BILLTempData";
            BILLDayData = "BILLDayData";
            ESTATEAllData = "ESTATEAllData";
            ESTATEData = "ESTATEData";
            SEALData = "SEALData";
            CAData = "CAData";
            ItemImpData = "ItemImpData";
            StockData = "StockData";
            StockMainData = "StockMainData";
            StockTempData = "StockTempData";
            MargingData = "MargingData";
            MarginpData = "MarginpData";
            DepositData_M = "DepositData_M";
            DepositData_N = "DepositData_N";
            DepositData_Y = "DepositData_Y";
            DepositData_D = "DepositData_D";
            DepositData_D_All = "DepositData_D_All";
            TDAApprSearchData = "TDAApprSearchData";
            TDAApprSearchDetailViewData = "TDAApprSearchDetailViewData";
            TDASearchDetailViewData = "TDASearchDetailViewData";
            CDCSearchViewModel = "CDCSearchViewModel";
            CDCChargeSearchViewModel = "CDCChargeSearchViewModel";
            CDCApprSearchData = "CDCApprSearchData";
            CDCChargeApprSearchData = "CDCChargeApprSearchData";
            CDCApprSearchDetailViewData = "CDCApprSearchDetailViewData";
            CDCChargeApprSearchDetailViewData = "CDCChargeApprSearchDetailViewData";
            CDCSEALData = "CDCSEALData";
            CDCBILLData = "CDCBILLData";
            CDCBILLAllData = "CDCBILLAllData";
            CDCCAData = "CDCCAData";
            CDCMargingData = "CDCMargingData";
            CDCItemImpData = "CDCItemImpData";
            CDCMarginpData = "CDCMarginpData";
            CDCEstateData = "CDCEstateData";
            CDCStockDataM = "CDCStockDataM";
            CDCStockDataD = "CDCStockDataD";
            CDCDepositDataM = "CDCDepositDataM";
            CDCDepositDataD = "CDCDepositDataD";
            CDCDepositDataD_All = "CDCDepositDataD_All";
            CDCChargeSEALData = "CDCSEALData";
            CDCChargeBILLData = "CDCBILLData";
            CDCChargeCAData = "CDCCAData";
            CDCChargeMargingData = "CDCMargingData";
            CDCChargeItemImpData = "CDCItemImpData";
            CDCChargeMarginpData = "CDCMarginpData";
            CDCChargeEstateData = "CDCEstateData";
            CDCChargeStockDataM = "CDCStockDataM";
            CDCChargeDepositDataM = "CDCDepositDataM";
            TreasuryMaintainSearchData = "TreasuryMaintainSearchData";
            TreasuryMaintainSearchDataList = "TreasuryMaintainSearchDataList";
            TreasuryMaintainChangeRecordSearchDataList = "TreasuryMaintainChangeRecordSearchDataList";
            DepChkItemSearchData = "DepChkItemSearchData";
            DepChkItem_P_SearchDataList = "DepChkItem_P_SearchDataList";
            DepChkItem_G_SearchDataList = "DepChkItem_G_SearchDataList";
            DepChkItemChangeRecordSearchDataList = "DepChkItemChangeRecordSearchDataList";
            DepChkItemOrderSearchDataList = "DepChkItemOrderSearchDataList";
            ItemMaintainSearchData = "ItemMaintainSearchData";
            ItemMaintainSearchDetailViewData = "ItemMaintainSearchDetailViewData";
            ItemMaintainChangeRecordSearchDetailViewData = "ItemMaintainChangeRecordSearchDetailViewData";
            ItemChargeUnitSearchData = "ItemChargeUnitSearchData";
            ItemChargeUnitSearchDetailViewData = "ItemChargeUnitSearchDetailViewData";
            ItemChargeUnitChangeRecordSearchDetailViewData = "ItemChargeUnitChangeRecordSearchDetailViewData";
            TreasuryRegisterSearchReportM = "TreasuryRegisterSearchReportM";
            TreasuryRegisterSearchReportD = "TreasuryRegisterSearchReportD";
            TreasuryMailContentSearchData = "TreasuryMailContentSearchData";
            TreasuryMailContentData = "TreasuryMailContentData";
            TreasuryMailContentDetailData = "TreasuryMailContentDetailData";
            TreasuryMailContentReceiveData = "TreasuryMailContentReceiveData";
            TreasuryMailTimeData = "TreasuryMailTimeData";
            TreasuryMailTimeDetailData = "TreasuryMailTimeDetailData";
            TreasuryMailTimeChangeRecordData = "TreasuryMailTimeChangeRecordData";
            ExcelfileData = "ExcelfileData";
            #endregion Cache資料

        }
    }
}