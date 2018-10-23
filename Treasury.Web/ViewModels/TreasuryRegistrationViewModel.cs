using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class TreasuryRegistrationViewModel 
    {
        /// <summary>
        /// 入庫日期下拉式選單
        /// </summary>
        [Description("入庫日期下拉式選單")]
        public List<SelectOption> vActualPutTime { get; set; }

        /// <summary>
        /// 開庫模式下拉式選單
        /// </summary>
        [Description("開庫模式下拉式選單")]
        public List<SelectOption> vOpenTreaType { get; set; }

    }
}