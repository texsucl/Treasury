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
        /// 金庫物品查詢畫面(資料)
        /// </summary>
        public static string TreasuryAccessSearchDetailViewData { get; private set; }

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

        #endregion Cache資料



        static CacheList()
        {
            #region Cache資料
            TreasuryAccessViewData = "TreasuryAccessViewData";
            TreasuryAccessSearchData = "TreasuryAccessSearchData";
            TreasuryAccessSearchDetailViewData = "TreasuryAccessSearchDetailViewData";
            BILLTempData = "BILLTempData";
            BILLDayData = "BILLDayData";
            ESTATEAllData = "ESTATEAllData";
            ESTATEData = "ESTATEData";
            SEALData = "SEALData";
            #endregion Cache資料

        }
    }
}