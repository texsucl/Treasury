using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 金庫物品項目
        /// </summary>
        public enum SysCodeType
        {
            /// <summary>
            /// 庫存狀態
            /// </summary>
            [Description("庫存狀態")]
            INVENTORY_TYPE,

            /// <summary>
            /// 資料庫類別
            /// </summary>
            [Description("資料庫類別")]
            TREA_ITEM_TYPE,

        }
    }

}