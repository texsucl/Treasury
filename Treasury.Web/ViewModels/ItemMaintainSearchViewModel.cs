using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemMaintainSearchViewModel: ITinItem
    {
        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vITEM_OP_TYPE { get; set; }
        /// <summary>
        /// 每日進出
        /// </summary>
        [Description("每日進出")]
        public string vISDO_PERDAY { get; set; }
        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 目前使用者
        /// </summary>
        [Description("目前使用者")]
        public string vCUSER_ID { get; set; }
    }
}