using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 呼叫 PartialView 的原始畫面 (從哪個畫面呼叫)
        /// </summary>
        public enum OpenPartialViewType
        {
            /// <summary>
            /// 金庫物品存取申請作業
            /// </summary>
            [Description("金庫物品存取申請作業")]
            Index = 0,

            /// <summary>
            /// 金庫物品存取覆核作業
            /// </summary>
            [Description("金庫物品存取覆核作業")]
            Appr = 1,

        }
    }

}