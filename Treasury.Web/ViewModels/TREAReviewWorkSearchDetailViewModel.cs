using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class TREAReviewWorkSearchDetailViewModel
    {
        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string vITeM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目編號")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM_DESC { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSEAL_ITEM { get; set; }

        /// <summary>
        /// 作業別
        /// </summary>
        [Description("作業別")]
        public string vACCESS_TYPE { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }

        /// <summary>
        /// 入庫確認人員
        /// </summary>
        [Description("入庫確認人員")]
        public string vCONFIRM_NAME { get; set; }

        /// <summary>
        /// 實際存取項目交易別
        /// </summary>
        [Description("實際存取項目交易別")]
        public string vACTUAL_ACCESS_TYPE { get; set; }

        /// <summary>
        /// 實際入庫人員
        /// </summary>
        [Description("實際入庫人員")]
        public string vACTUAL_ACCESS_NAME { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }
    }
}