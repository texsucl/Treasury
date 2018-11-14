using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 設定檔項目
        /// </summary>
        public enum DefinitionType
        {
            /// <summary>
            /// 金庫存取作業設定檔
            /// </summary>
            [Description("金庫存取作業設定檔")]
            TREA_ITEM,

            /// <summary>
            /// 金庫設備設定檔
            /// </summary>
            [Description("金庫設備設定檔")]
            TREA_EQUIP,

            /// <summary>
            /// mail發送內文設定檔
            /// </summary>
            [Description("mail發送內文設定檔")]
            MAIL_CONTENT,

            /// <summary>
            /// 發送時間定義檔
            /// </summary>
            [Description("發送時間定義檔")]
            MAIL_TIME,

            /// <summary>
            /// 保管單位設定檔
            /// </summary>
            [Description("保管單位設定檔")]
            ITEM_CHARGE_UNIT,

            /// <summary>
            /// 定存檢核表項目設定檔
            /// </summary>
            [Description("定存檢核表項目設定檔")]
            DEP_CHK_ITEM,
        }
    }

}