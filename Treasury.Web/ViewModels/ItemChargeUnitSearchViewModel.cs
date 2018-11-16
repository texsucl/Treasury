using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemChargeUnitSearchViewModel: ITinItem
    {
        /// <summary>
        /// 資料庫名稱
        /// </summary>
        [Description("資料庫名稱")]
        public string vTREA_ITEM_NAME { get; set; }
        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCHARGE_DEPT { get; set; }
        /// <summary>
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCHARGE_SECT { get; set; }
        /// <summary>
        /// 經辦
        /// </summary>
        [Description("經辦")]
        public string vCHARGE_UID { get; set; }

        /// <summary>
        /// 目前使用者
        /// </summary>
        [Description("目前使用者")]
        public string vCUSER_ID { get; set; }
    }
}