using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailContentUpdateViewModel : ITinItem
    {
        public TreasuryMailContentUpdateViewModel()
        {
            vMAIL_CONTENT_ID = string.Empty;
            vIS_DISABLED = string.Empty;
            vMAIL_SUBJECT = string.Empty;
            vMAIL_CONTENT = string.Empty;
            subData = new List<TreasuryMailReceivelViewModel>();
        }

        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 發送主旨
        /// </summary>
        [Description("發送主旨")]
        public string vMAIL_SUBJECT { get; set; }

        /// <summary>
        /// 發送內文
        /// </summary>
        [Description("發送內文")]
        public string vMAIL_CONTENT { get; set; }

        /// <summary>
        /// 對應功能明細
        /// </summary>
        [Description("對應功能明細")]
        public List<TreasuryMailReceivelViewModel> subData { get; set; }

        /// <summary>
        /// 最後異動時間 
        /// </summary>
        [Description("最後異動時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }

        /// <summary>
        /// 使用者ID 
        /// </summary>
        [Description("使用者ID")]
        public string UserID { get; set; }
    }
}