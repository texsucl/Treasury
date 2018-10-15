using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class AfterOpenTreasurySearchViewModel
    {
        /// <summary>
        /// 建立日期
        /// </summary>
        [Description("建立日期")]
        public string vCREATE_DT { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

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
    }
}