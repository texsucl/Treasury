using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ConfirmStorageDeleteViewModel
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
        /// 欄位ID
        /// </summary>
        [Description("欄位ID")]
        public string vrowid { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAPLY_NO { get; set; }

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