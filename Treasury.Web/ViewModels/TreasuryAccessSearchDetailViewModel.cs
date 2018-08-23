using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 申請表單查詢顯示區塊ViewModel
    /// </summary>
    public class TreasuryAccessSearchDetailViewModel
    {
        /// <summary>
        /// 取消申請
        /// </summary>
        [Description("取消申請")]
        public string vCancleFlag { get; set; }

        /// <summary>
        /// 作廢
        /// </summary>
        [Description("作廢")]
        public string vInvalidFlag { get; set; }

        /// <summary>
        /// 列印
        /// </summary>
        [Description("列印")]
        public string vPrintFlag { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        [Description("存取申請項目")]
        public string vItem { get; set; }

        /// <summary>
        /// 申請項目中文
        /// </summary>
        [Description("存取申請項目中文")]
        public string vItemDec { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAPLY_DT { get; set; }

        /// <summary>
        /// 實際存取日期
        /// </summary>
        [Description("實際存取日期")]
        public string vREGI_APPR_DT { get; set; }

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
        /// 表單狀態
        /// </summary>
        [Description("表單狀態")]
        public string vAPLY_STATUS { get; set; }

        /// <summary>
        /// 表單狀態(中文)
        /// </summary>
        [Description("表單狀態(中文)")]
        public string vAPLY_STATUS_D { get; set; }

        /// <summary>
        /// 覆核意見/退回原因
        /// </summary>
        [Description("覆核意見/退回原因")]
        public string vDESC { get; set; }

        /// <summary>
        /// 存入(P) or 取出(G)
        /// </summary>
        [Description("存入/取出")]
        public string vACCESS_TYPE { get; set; }

        /// <summary>
        /// 最後修改時間
        /// </summary>
        [Description("最後修改時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}