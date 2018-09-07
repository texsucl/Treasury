using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class CDCViewModel 
    {
        /// <summary>
        /// 金庫內外下拉式選單
        /// </summary>
        [Description("金庫內外下拉式選單")]
        public List<SelectOption> vTreasuryIO { get; set; }

        /// <summary>
        /// 作業項目下拉式選單
        /// </summary>
        [Description("作業項目下拉式選單")]
        public List<SelectOption> vJobProject { get; set; }

        /// <summary>
        /// 存入保證金類別下拉式選單
        /// </summary>
        [Description("存入保證金類別下拉式選單")]
        public List<SelectOption> vMarginp { get; set; }

        /// <summary>
        /// 存出保證金類別下拉式選單
        /// </summary>
        [Description("存出保證金類別下拉式選單")]
        public List<SelectOption> vMarging { get; set; }

        /// <summary>
        /// 不動產冊號下拉式選單
        /// </summary>
        [Description("不動產冊號下拉式選單")]
        public List<SelectOption> vBook_No { get; set; }

        /// <summary>
        /// 不動產狀別下拉式選單
        /// </summary>
        [Description("不動產狀別下拉式選單")]
        public List<SelectOption> vEstate_From_No  { get; set; }

        /// <summary>
        /// 股票編號下拉式選單
        /// </summary>
        [Description("股票編號下拉式選單")]
        public List<SelectOption> vName { get; set; }

        /// <summary>
        /// 定期存單交易對象下拉式選單
        /// </summary>
        [Description("定期存單交易對象下拉式選單")]
        public List<SelectOption> vTRAD_Partners { get; set; }


    }
}