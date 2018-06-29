using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class EstateModel 
    {
        /// <summary>
        /// 冊號
        /// </summary>
        [Description("冊號")]
        public string BOOK_NO { get; set; }

        /// <summary>
        /// 大樓名稱
        /// </summary>
        [Description("大樓名稱")]
        public string BUILDING_NAME { get; set; }

        /// <summary>
        /// 坐落
        /// </summary>
        [Description("坐落")]
        public string LOCATED { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]       
        public string MEMO { get; set; }

    }
}