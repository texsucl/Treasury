using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 庫存狀態
        /// </summary>
        public enum AccessInventoryType
        {
            /// <summary>
            /// 在庫
            /// </summary>
            [Description("在庫")]
            _1 = 1,

            /// <summary>
            /// 已被取出
            /// </summary>
            [Description("已被取出")]
            _2 = 2,

            /// <summary>
            /// 預約存入
            /// </summary>
            [Description("預約存入")]
            _3 = 3,

            /// <summary>
            /// 預約取出
            /// </summary>
            [Description("預約取出")]
            _4 = 4,

            /// <summary>
            /// 預約取出，計庫存
            /// </summary>
            [Description("預約取出，計庫存")]
            _5 = 5,

            /// <summary>
            /// 已被取出，計庫存
            /// </summary>
            [Description("已被取出，計庫存")]
            _6 = 6,

            /// <summary>
            /// 已取消
            /// </summary>
            [Description("已取消")]
            _7 = 7,

            /// <summary>
            /// 資料庫異動中
            /// </summary>
            [Description("資料庫異動中")]
            _8 = 8,

            /// <summary>
            /// 預約存入，計庫存
            /// </summary>
            [Description("預約存入，計庫存")]
            _9 = 9,
        }
    }

}