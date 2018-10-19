using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ConfirmStorageInsertViewModel
    {
        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string vITeM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目編號
        /// </summary>
        [Description("存取項目編號")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 印鑑物品編號
        /// </summary>
        [Description("印鑑物品編號")]
        public string vSEAL_ITEM_ID { get; set; }

        /// <summary>
        /// 作業別
        /// </summary>
        [Description("作業別")]
        public string vACCESS_TYPE { get; set; }

        /// <summary>
        /// 作業別CODE
        /// </summary>
        [Description("作業別CODE")]
        public string vACCESS_TYPE_CODE { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }

        /// <summary>
        /// 填表人員
        /// </summary>
        [Description("填表人員")]
        public string vCurrentUid { get; set; }

        /// <summary>
        /// 填表單位
        /// </summary>
        [Description("填表單位")]
        public string vCurrentUnit { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM { get; set; }

        /// <summary>
        /// 印鑑物品
        /// </summary>
        [Description("印鑑物品")]
        public string vSEAL_ITEM { get; set; }

        /// <summary>
        /// PKID
        /// </summary>
        [Description("PKID")]
        public string uuid { get; set; }

        /// <summary>
        /// 已確認
        /// </summary>
        [Description("已確認")]
        public string v_IS_CHECKED { get; set; }
    }
}