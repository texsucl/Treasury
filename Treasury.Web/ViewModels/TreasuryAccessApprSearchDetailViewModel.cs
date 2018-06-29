using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 覆核表單查詢顯示區塊ViewModel
    /// </summary>
    public class TreasuryAccessApprSearchDetailViewModel
    {
        /// <summary>
        /// 覆核回復意見
        /// </summary>
        [Description("覆核回復意見")]
        public string vAPPR_DESC { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        [Description("存取項目")]
        public string vItem { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAPLY_DT { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 申請單位
        /// </summary>
        [Description("申請單位")]
        public string vAPLY_UNIT { get; set; }

        /// <summary>
        /// 申請人ID
        /// </summary>
        [Description("申請人ID")]
        public string vAPLY_UID { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Description("申請人")]
        public string vAPLY_UID_NAME { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }
  
        /// <summary>
        /// 最後修改時間
        /// </summary>
        [Description("最後修改時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}