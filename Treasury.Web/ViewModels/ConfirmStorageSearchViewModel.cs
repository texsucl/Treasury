
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ConfirmStorageSearchViewModel
    {
        /// <summary>
		/// 申請日期
		/// </summary>
		[Description("申請日期")]
        public string vCREATE_DT { get; set; }

        /// <summary>
		/// 金庫登記簿單號
		/// </summary>
		[Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOPEN_TREA_TYPE { get; set; }

        /// <summary>
        /// 已確認
        /// </summary>
        [Description("已確認")]
        public string v_IS_CHECKED { get; set; }

        /// <summary>
        /// 工作項目
        /// </summary>
        [Description("工作項目")]
        public List<string> vITEM_ID_List { get; set; }
    }
}