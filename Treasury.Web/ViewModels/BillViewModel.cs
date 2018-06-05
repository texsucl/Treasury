using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class BillViewModel
    {
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
    }
}