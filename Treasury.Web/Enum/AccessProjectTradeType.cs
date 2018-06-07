using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 存取項目交易別
        /// </summary>
        public enum AccessProjectTradeType
        {
            /// <summary>
            /// 存入
            /// </summary>
            [Description("存入")]
            P,

            /// <summary>
            /// 取出
            /// </summary>
            [Description("取出")]
            G,

            /// <summary>
            /// 用印
            /// </summary>
            [Description("用印")]
            S

        }
    }

}