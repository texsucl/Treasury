using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemMaintainChangeRecordSearchViewModel: ITinItem
    {
        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vItem_Op_Type { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vTrea_IItem { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAppr_Status { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vLast_Update_Uid { get; set; }
    }
}