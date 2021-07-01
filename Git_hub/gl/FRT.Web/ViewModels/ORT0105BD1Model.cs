using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 已銷帳/未銷帳 檢核報表
    /// </summary>
    public class ORT0105BD1Model : IEquatable<ORT0105BD1Model>
    {
        /// <summary>
        /// 交易序號 / 代收編號 / 表單編號(存摺收支)
        /// </summary>
        [Description("交易序號 / 代收編號 / 表單編號(存摺收支)")]
        public string NO { get; set; }

        /// <summary>
        /// 表單編號
        /// </summary>
        [Description("表單編號")]
        public string VOUCHER_NO { get; set; }

        /// <summary>
        /// 沖銷檔編號
        /// </summary>
        [Description("沖銷檔編號")]
        public string VOUCHER_NO_Y_N2 { get; set; }

        /// <summary>
        /// 檔案代碼/名稱
        /// </summary>
        [Description("檔案代碼/名稱")]
        public string VAR_CODE_NAME { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        [Description("交易日期")]
        public string DATE { get; set; }

        /// <summary>
        /// 沖銷日期
        /// </summary>
        [Description("沖銷日期")]
        public DateTime? VOUCHER_DATE { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURRENCY { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public decimal? AMT { get; set; }

        public bool Equals(ORT0105BD1Model other)
        {

            if (Object.ReferenceEquals(other, null)) return false;

            if (Object.ReferenceEquals(this, other)) return true;

            return NO.Equals(other.NO) && DATE.Equals(other.DATE) && CURRENCY.Equals(other.CURRENCY);
        }


        public override int GetHashCode()
        {

            int hashProductNO = NO == null ? 0 : NO.GetHashCode();

            int hashProductDATE = DATE == null ? 0 : DATE.GetHashCode();

            int hashProductCURRENCY = CURRENCY == null ? 0 : CURRENCY.GetHashCode();

            return hashProductNO ^ hashProductDATE ^ hashProductCURRENCY;
        }
    }
}