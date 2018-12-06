using System;
using System.ComponentModel;
using Treasury.Web.ViewModels;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// Excel 上傳&下載統一檔名
        /// </summary>
        public enum ExcelName
        {
            /// <summary>
            /// 冊號
            /// </summary>
            [Commucation(typeof(FileItemBookEstateModel))]
            [Description("冊號")]
            BookNo,

            /// <summary>
            /// 不動產
            /// </summary>
            [Commucation(typeof(FileEstateModel))]
            [Description("不動產")]
            Estate,

            /// <summary>
            /// 存入保證金
            /// </summary>
            [Commucation(typeof(FileMarginpModel))]
            [Description("存入保證金")]
            Marginp,

            /// <summary>
            /// 存出保證金
            /// </summary>
            [Commucation(typeof(FileMargingModel))]
            [Description("存出保證金")]
            Marging,

            /// <summary>
            /// 股票
            /// </summary>
            [Commucation(typeof(FileStockModel))]
            [Description("股票")]
            Stock,

            /// <summary>
            /// 重要物品
            /// </summary>
            [Commucation(typeof(FileItemImpModel))]
            [Description("重要物品")]
            Itemimp,

        }
    }

}