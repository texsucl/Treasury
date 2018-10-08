using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class AfterOpenTreasuryInsertViewModel
    {
        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string vITEM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目編號
        /// </summary>
        [Description("存取項目編號")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }

        /// <summary>
        /// 實際存取項目交易別
        /// </summary>
        [Description("實際存取項目交易別")]
        public string ACTUAL_ACCESS_TYPE { get; set; }

        /// <summary>
        /// 印章內容編號
        /// </summary>
        [Description("印章內容編號")]
        public string vSEAL_ITEM_ID { get; set; }

        /// <summary>
        /// 作業別編號
        /// </summary>
        [Description("作業別編號")]
        public string vACCESS_TYPE_CODE { get; set; }

        /// <summary>
        /// 實際入庫人員編號
        /// </summary>
        [Description("實際入庫人員編號")]
        public string ACTUAL_ACCESS_UID { get; set; }
    }
}