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
            D1012,

            /// <summary>
            /// 不動產權狀
            /// </summary>
            [Description("不動產權狀")]
            D1014,

            /// <summary>
            /// 股票
            /// </summary>
            [Description("股票")]
            D1015,

            /// <summary>
            /// 公司大章A
            /// </summary>
            [Description("公司大章A")]
            D1005,

            /// <summary>
            /// 公司小章A
            /// </summary>
            [Description("公司小章A")]
            D1006,

            /// <summary>
            /// 退管會大章A
            /// </summary>
            [Description("退管會大章A")]
            D1007,

            /// <summary>
            /// 公司大章B
            /// </summary>
            [Description("公司大章B")]
            D1008,

            /// <summary>
            /// 公司小章B
            /// </summary>
            [Description("公司小章B")]
            D1009,

            /// <summary>
            /// 退管會大章B
            /// </summary>
            [Description("退管會大章B")]
            D1010,

            /// <summary>
            /// 抽退票章
            /// </summary>
            [Description("抽退票章")]
            D1011,

            /// <summary>
            /// 電子憑證
            /// </summary>
            [Description("電子憑證")]
            D1024,

            /// <summary>
            /// 存出保證金
            /// </summary>
            [Description("存出保證金")]
            D1016,

            /// <summary>
            /// 存入保證金
            /// </summary>
            [Description("存入保證金")]
            D1017,

            /// <summary>
            /// 重要物品
            /// </summary>
            [Description("重要物品")]
            D1018,

            /// <summary>
            /// 其他物品
            /// </summary>
            [Description("其他物品")]
            D1019,

            /// <summary>
            /// 定期存單
            /// </summary>
            [Description("定期存單")]
            D1013,
        }
    }

}