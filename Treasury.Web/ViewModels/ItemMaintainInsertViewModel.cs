using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemMaintainInsertViewModel
    {
        /// <summary>
        /// 存取項目編號
        /// </summary>
        [Description("存取項目編號")]
        public string vTrea_Item_Id { get; set; }

        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vOp_Type { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vItem_Desc { get; set; }

        /// <summary>
        /// 連結資料庫
        /// </summary>
        [Description("連結資料庫")]
        public string vIs_Item { get; set; }

        /// <summary>
        /// 資料庫類別
        /// </summary>
        [Description("資料庫類別")]
        public string vType { get; set; }

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        [Description("資料庫名稱")]
        public string vName { get; set; }

        /// <summary>
        /// 每日進出
        /// </summary>
        [Description("每日進出")]
        public string vIsDo_Perday { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }
    }
}