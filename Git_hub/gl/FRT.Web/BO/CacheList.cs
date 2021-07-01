using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.BO
{
    /// <summary>
    /// cache命名 目的為不重複cache名稱 避免資料被覆蓋
    /// </summary>
    public static class CacheList
    {
        #region Cache資料

        /// <summary>
        ///ORTB016ViewData (ORTB016畫面資料)
        /// </summary>
        public static string ORTB016ViewData { get; private set; }

        /// <summary>
        ///ORTB021ViewData (ORTB021畫面資料)
        /// </summary>
        public static string ORTB021ViewData { get; private set; }

        /// <summary>
        /// ORT0103ViewData(ORT0103畫面資料)
        /// </summary>
        public static string ORT0103ViewData { get; private set; }

        /// <summary>
        /// 比對報表勾稽_批次定義(OPEN跨系統勾稽) (ORT0105畫面資料)
        /// </summary>
        public static string ORT0105ViewData { get; private set; }

        /// <summary>
        /// 比對報表勾稽_批次定義新增明細檔 (ORT0105畫面資料)
        /// </summary>
        public static string ORT0105ViewSubData { get; private set; }

        /// <summary>
        /// 比對報表勾稽_批次定義覆核功能(OPEN跨系統勾稽) (ORT0105A畫面資料)
        /// </summary>
        public static string ORT0105AViewData { get; private set; }

        /// <summary>
        /// 比對報表勾稽_批次定義查詢功能(OPEN跨系統勾稽) (ORT0105Q畫面資料)
        /// </summary>
        public static string ORT0105QViewData { get; private set; }

        /// <summary>
        /// 比對報表勾稽_現上執行功能 (ORT0106報表資料)
        /// </summary>
        public static string ORT0106ViewData { get; private set; }

        /// <summary>
        /// 跨系統資料庫勾稽銀存銷帳不比對帳號 (ORT0109畫面資料)
        /// </summary>
        public static string ORT0109ViewData { get; private set; }

        /// <summary>
        /// 跨系統資料庫勾稽銀存銷帳不比對帳號覆核功能 (ORT0109A畫面資料)
        /// </summary>
        public static string ORT0109AViewData { get; private set; }

        /// <summary>
        /// ORT0103SelectData(ORT0103退款原因選單資料)
        /// </summary>
        public static string ORT0103SelectData { get; private set; }


        #endregion Cache資料
        static CacheList()
        {
            #region Cache資料
            ORTB016ViewData = "ORTB016ViewData";
            ORTB021ViewData = "ORTB021ViewData";
            ORT0103ViewData = "ORT0103ViewData";
            ORT0103SelectData = "ORT0103SelectData";
            ORT0105ViewData = "ORT0105ViewData";
            ORT0105ViewSubData = "ORT0105ViewSubData";
            ORT0105AViewData = "ORT0105AViewData";
            ORT0106ViewData = "ORT0106ViewData";
            ORT0109ViewData = "ORT0109ViewData";
            ORT0109AViewData = "ORT0109AViewData";
            #endregion Cache資料

        }
    }
}