using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 金庫物品項目
        /// </summary>
        public enum TreaItemType
        {
            /// <summary>
            /// 空白票據
            /// </summary>
            [Description("空白票據")]
            BILL,

            /// <summary>
            /// 不動產權狀
            /// </summary>
            [Description("不動產權狀")]
            ESTATE,

            /// <summary>
            /// 股票
            /// </summary>
            [Description("股票")]
            STOCK,

            /// <summary>
            /// 印章
            /// </summary>
            [Description("印章")]
            SEAL,

            /// <summary>
            /// 電子憑證
            /// </summary>
            [Description("電子憑證")]
            CA,

            /// <summary>
            /// 存出保證金
            /// </summary>
            [Description("存出保證金")]
            MARGING,

            /// <summary>
            /// 存入保證金
            /// </summary>
            [Description("存入保證金")]
            MARGINP,

            /// <summary>
            /// 重要物品
            /// </summary>
            [Description("重要物品")]
            ITEMIMP,

            /// <summary>
            /// 其他物品
            /// </summary>
            [Description("其他物品")]
            ITEMOTH,

            /// <summary>
            /// 定期存單
            /// </summary>
            [Description("定期存單")]
            DEPOSIT,
        }
    }

}