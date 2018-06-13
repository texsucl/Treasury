using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class BillViewModel : ITreaItem
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// 明細流水號
        /// </summary>
        [Description("明細流水號")]
        public string vDataSeq { get; set; }

        /// <summary>
        /// 項次
        /// </summary>
        [Description("項次")]       
        public string vRowNum { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 發票行庫
        /// </summary>
        [Description("發票行庫")]
        public string vIssuingBank { get; set; }

        /// <summary>
        /// 支票類型
        /// </summary>
        [Description("支票類型")]
        public string vCheckType { get; set; }

        /// <summary>
        /// 支票號碼英文字軌
        /// </summary>
        [Description("支票號碼英文字軌")]
        public string vCheckNoTrack { get; set; }

        /// <summary>
        /// 支票號碼(起)
        /// </summary>
        [Description("支票號碼(起)")]
        public string vCheckNoB { get; set; }

        /// <summary>
        /// 支票號碼(迄)
        /// </summary>
        [Description("支票號碼(迄)")]
        public string vCheckNoE { get; set; }

        /// <summary>
        /// 總張數
        /// </summary>
        [Description("總張數")]
        public string vCheckTotalNum { get; set; }

        /// <summary>
        /// 取出號碼(迄)
        /// </summary>
        [Description("取出號碼(迄)")]
        public string vTakeOutE { get; set; }

        /// <summary>
        /// 取出總張數
        /// </summary>
        [Description("取出總張數")]
        public string vTakeOutTotalNum { get; set; }

        /// <summary>
        /// 剩餘總張數
        /// </summary>
        [Description("剩餘總張數")]
        public string vReMainTotalNum { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>

        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}