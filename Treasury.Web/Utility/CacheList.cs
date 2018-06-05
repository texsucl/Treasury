namespace Treasury.WebUtility
{
    /// <summary>
    /// cache命名 目的為不重複cache名稱 避免資料被覆蓋
    /// </summary>
    public static class CacheList
    {
        #region 資料庫資料
        public static string TempData { get; private set; }

        #endregion 資料庫資料



        static CacheList()
        {
            #region 資料庫資料

            TempData = "TempData";

            #endregion 資料庫資料

        }
    }
}