using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.Utilitys
{
    public class CacheList
    {
        #region Cache資料

        public static string OAP0018SearchData { get; private set; }

        public static string OAP0018ViewData { get; private set; }

        public static string OAP0018ASearchData { get; private set; }

        public static string OAP0018AViewData { get; private set; }

        public static string OAP0018AHisViewData { get; private set; }
        
        public static string OAP0019SearchData { get; private set; }
        
        public static string OAP0019ViewData { get; private set; }

        /// <summary>
        /// OAP0020-支票簽收窗口維護作業 查詢結果
        /// </summary>
        public static string OAP0020ViewData { get; private set; }

        public static string OAP0020DetailViewData { get; private set; }

        public static string OAP0021SearchData { get; private set; }

        public static string OAP0021ViewData { get; private set; }

        public static string OAP0021DetailData { get; private set; }

        public static string OAP0021SubData { get; private set; }

        public static string OAP0021ASearchData { get; private set; }

        public static string OAP0021AViewData { get; private set; }

        public static string OAP0022SearchData { get; private set; }

        public static string OAP0022ViewData { get; private set; }

        public static string OAP0023SearchData { get; private set; }

        public static string OAP0023ViewData { get; private set; }

        public static string OAP0024SearchData { get; private set; }

        public static string OAP0024ViewData { get; private set; }

        public static string OAP0026SearchData { get; private set; }

        public static string OAP0026ViewData { get; private set; }

        public static string OAP0026ASearchData { get; private set; }

        public static string OAP0026AViewData { get; private set; }

        public static string OAP0027SearchData { get; private set; }

        public static string OAP0027ViewData { get; private set; }

        public static string OAP0027ASearchData { get; private set; }

        public static string OAP0027AViewData { get; private set; }

        public static string OAP0029SearchData { get; private set; }

        public static string OAP0029ViewData { get; private set; }

        public static string OAP0029ASearchData { get; private set; }

        public static string OAP0029AViewData { get; private set; }

        public static string OAP0030SearchData { get; private set; }

        public static string OAP0030ViewData { get; private set; }

        public static string OAP0031SearchData { get; private set; }

        public static string OAP0031ViewData { get; private set; }

        public static string OAP0032SearchData { get; private set; }

        public static string OAP0032ViewData { get; private set; }

        #endregion Cache資料

        static CacheList()
        {
            #region Cache資料
            OAP0018SearchData = "OAP0018SearchData";
            OAP0018ViewData = "OAP0018ViewData";
            OAP0018ASearchData = "OAP0018ASearchData";
            OAP0018AViewData = "OAP0018AViewData";
            OAP0018AHisViewData = "OAP0018AHisViewData";
            OAP0019SearchData = "OAP0019SearchData";
            OAP0019ViewData = "OAP0019ViewData";
            OAP0020ViewData = "OAP0020ViewData";
            OAP0020DetailViewData = "OAP0020DetailViewData";
            OAP0021SearchData = "OAP0021SearchData";
            OAP0021ViewData = "OAP0021ViewData";
            OAP0021DetailData = "OAP0021DetailData";
            OAP0021SubData = "OAP0021SubData";
            OAP0021ASearchData = "OAP0021ASearchData";
            OAP0021AViewData = "OAP0021AViewData";
            OAP0022SearchData = "OAP0022SearchData";
            OAP0022ViewData = "OAP0022ViewData";
            OAP0023SearchData = "OAP0023SearchData";
            OAP0023ViewData = "OAP0023ViewData";
            OAP0024SearchData = "OAP0024SearchData";
            OAP0024ViewData = "OAP0024ViewData";
            OAP0026SearchData = "OAP0026SearchData";
            OAP0026ViewData = "OAP0026ViewData";
            OAP0026ASearchData = "OAP0026ASearchData";
            OAP0026AViewData = "OAP0026AViewData";
            OAP0027SearchData = "OAP0027SearchData";
            OAP0027ViewData = "OAP0027ViewData";
            OAP0027ASearchData = "OAP0027ASearchData";
            OAP0027AViewData = "OAP0027AViewData";
            OAP0029SearchData = "OAP0029SearchData";
            OAP0029ViewData = "OAP0029ViewData";
            OAP0029ASearchData = "OAP0029ASearchData";
            OAP0029AViewData = "OAP0029AViewData";
            OAP0030SearchData = "OAP0030SearchData";
            OAP0030ViewData = "OAP0030ViewData";
            OAP0031SearchData = "OAP0031SearchData";
            OAP0031ViewData = "OAP0031ViewData";
            OAP0032ViewData = "OAP0032ViewData";
            #endregion Cache資料
        }
    }
}