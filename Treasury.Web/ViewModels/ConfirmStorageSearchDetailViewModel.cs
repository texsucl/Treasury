using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class ConfirmStorageSearchDetailViewModel
    {
        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string vITeM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM { get; set; }

        /// <summary>
        /// h存取項目
        /// </summary>
        [Description("h存取項目")]
        public string hITEM { get; set; }

        /// <summary>
        /// 存取項目編號
        /// </summary>
        [Description("存取項目編號")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 印章內容編號
        /// </summary>
        [Description("印章內容編號")]
        public string vSEAL_ITEM_ID { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSEAL_ITEM { get; set; }

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
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
		/// 金庫登記簿單號
		/// </summary>
		[Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
        /// 印鑑下拉選單
        /// </summary>
        [Description("印鑑下拉選單")]
        public List<SelectOption> vSEAL_ITEM_OPTION { get; set; }

        /// <summary>
        /// 入庫確認人員
        /// </summary>
        [Description("入庫確認人員")]
        public string vCONFIRM_UID { get; set; }

        /// <summary>
        /// h入庫確認人員
        /// </summary>
        [Description("h入庫確認人員名稱")]
        public string hCONFIRM_Name { get; set; }

        /// <summary>
        /// h入庫確認人員
        /// </summary>
        [Description("h入庫確認人員Id")]
        public string hCONFIRM_UID { get; set; }

        /// <summary>
        /// 入庫確認時間
        /// </summary>
        [Description("入庫確認時間")]
        public string vCONFIRM_DT { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }

        /// <summary>
        /// 是否勾選
        /// </summary>
        [Description("是否勾選")]
        public bool vIsChecked { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string hvAPLY_NO { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string vAPLY_UID { get; set; }
        /// <summary>
        /// PKID
        /// </summary>
        [Description("PKID")]
        public string uuid { get; set; }
    }
}