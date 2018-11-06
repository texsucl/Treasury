using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class AlreadyConfirmedSearchDetailViewModel
    {
        /// <summary>
        /// 入庫日期
        /// </summary>
        [Description("入庫日期")]
        public string vACTUAL_PUT_TIME { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOPEN_TREA_TYPE { get; set; }

        /// <summary>
        /// 確認人員
        /// </summary>
        [Description("確認人員")]
        public string vConfirm_Id { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM_DESC { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSEAL_DESC { get; set; }

        /// <summary>
        /// 作業別
        /// </summary>
        [Description("作業別")]
        public string vACCESS_TYPE { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vACCESS_REASON { get; set; }

        /// <summary>
        /// 存取項目ID
        /// </summary>
        [Description("存取項目ID")]
        public string hITEM_ID { get; set; }

        /// <summary>
        /// 作業類型
        /// </summary>
        [Description("作業類型")]
        public string hITEM_OP_TYPE { get; set; }
    }
}