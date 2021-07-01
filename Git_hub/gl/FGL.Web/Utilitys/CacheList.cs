using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FGL.Web.Utilitys
{
    public class CacheList
    {
        #region Cache資料

        public static string OGL00010SearchData { get; private set; }

        public static string OGL00010ViewData { get; private set; }

        public static string OGL00010ViewSubData { get; private set; }

        public static string OGL00010ASearchData { get; private set; }

        public static string OGL00010AViewData { get; private set; }

        public static string OGL00010AViewSubData { get; private set; }

        #endregion Cache資料

        static CacheList()
        {
            #region Cache資料
            OGL00010SearchData = "OGL00010SearchData";
            OGL00010ViewData = "OGL00010ViewData";
            OGL00010ViewSubData = "OGL00010ViewSubData";
            OGL00010ASearchData = "OGL00010ASearchData";
            OGL00010AViewData = "OGL00010AViewData";
            OGL00010AViewSubData = "OGL00010AViewSubData";
            #endregion Cache資料
        }
    }
}