using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class AfterOpenTreasuryUnconfirmedDetailViewModel
    {
        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string vITEM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM_DESC { get; set; }

        /// <summary>
        /// 存取項目編號
        /// </summary>
        [Description("存取項目編號")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 印章內容編號
        /// </summary>
        [Description("印章內容編號")]
        public string vSEAL_ITEM_ID { get; set; }

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
        /// 作業別編號
        /// </summary>
        [Description("作業別編號")]
        public string vACCESS_TYPE_CODE { get; set; }

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
        /// 申請人員編號
        /// </summary>
        [Description("申請人員編號")]
        public string APLY_UID { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string APLY_NAME { get; set; }

        /// <summary>
        /// 申請日期時間
        /// </summary>
        [Description("申請日期時間")]
        public string APLY_DT { get; set; }

        /// <summary>
        /// h申請單號
        /// </summary>
        [Description("h申請單號")]
        public string hvAPLY_NO { get; set; }

        /// <summary>
        /// 是否打勾
        /// </summary>
        [Description("是否打勾")]
        public bool IsTakeout { get; set; }

        /// <summary>
        /// 實際入庫人員ID
        /// </summary>
        [Description("實際入庫人員ID")]
        public string vACTUAL_ACCESS_UID { get; set; }

        /// <summary>
        /// 實際入庫人員
        /// </summary>
        [Description("實際入庫人員")]
        public string vACTUAL_ACCESS_NAME { get; set; }

        /// <summary>
        /// 最後修改日期
        /// </summary>
        [Description("最後修改日期")]
        public DateTime? vLAST_UPDATE_DT { get; set; }
    }
}