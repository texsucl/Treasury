using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailReceivelViewModel 
    {
        /// <summary>
        /// 功能編號
        /// </summary>
        [Description("功能編號")]
        public string FUNC_ID { get; set; }

        /// <summary>
        /// 功能編號名稱
        /// </summary>
        [Description("功能編號名稱")]
        public string FUNC_ID_Name { get; set; }

        /// <summary>
        /// 異動狀態
        /// </summary>
        [Description("異動狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 異動狀態(中文)
        /// </summary>
        [Description("異動狀態(中文)")]
        public string vStatus_D { get; set; }

    }
}