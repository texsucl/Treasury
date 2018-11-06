using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class TREAReviewWorkDetailViewModel
    {
        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOPEN_TREA_TYPE { get; set; }
        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }
        /// <summary>
        /// h金庫登記簿單號
        /// </summary>
        [Description("h金庫登記簿單號")]
        public string hvTREA_REGISTER_ID { get; set; }
        /// <summary>
        /// 開庫時間
        /// </summary>
        [Description("開庫時間")]
        public string vOPEN_TREA_TIME { get; set; }
        /// <summary>
        /// 實際入庫時間
        /// </summary>
        [Description("實際入庫時間")]
        public string vACTUAL_PUT_TIME { get; set; }
        /// <summary>
        /// 實際出庫時間
        /// </summary>
        [Description("實際出庫時間")]
        public string vACTUAL_GET_TIME { get; set; }
        /// <summary>
        /// 登記簿狀態
        /// </summary>
        [Description("登記簿狀態")]
        public string vREGI_STATUS { get; set; }
        /// <summary>
        /// 是否選取
        /// </summary>
        [Description("是否選取")]
        public bool Ischecked { get; set; }
        /// <summary>
        /// 最後異動日期時間
        /// </summary>
        [Description("最後異動日期時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }

        /// <summary>
        /// 開庫日期
        /// </summary>
        [Description("開庫日期")]
        public string vOPEN_TREA_DATE { get; set; }

        /// <summary>
        /// 最後異動人員
        /// </summary>
        [Description("最後異動人員")]
        public string vLAST_UPDATE_UID { get; set; }

    }
}