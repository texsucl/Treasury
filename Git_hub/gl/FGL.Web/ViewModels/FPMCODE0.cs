using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class FPMCODE0
    {
        /// <summary>
        /// 組別碼
        /// </summary>
        [Description("組別碼")]
        public string GROUP_ID { get; set; }

        /// <summary>
        /// 文字長度
        /// </summary>
        [Description("文字長度")]
        public string TEXT_LEN { get; set; }

        /// <summary>
        /// 參考號碼
        /// </summary>
        [Description("參考號碼")]
        public string REF_NO { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Description("說明")]
        public string TEXT { get; set; }

        /// <summary>
        /// 資料來源
        /// </summary>
        [Description("資料來源")]
        public string SRCE_FROM { get; set; }

        /// <summary>
        /// 使用註記
        /// </summary>
        [Description("使用註記")]
        public string USE_MARK { get; set; }

        /// <summary>
        /// 輸入日期–年(民國年)
        /// </summary>
        [Description("輸入日期–年(民國年)")]
        public string ENTRY_YY { get; set; }

        /// <summary>
        /// 輸入日期–月
        /// </summary>
        [Description("輸入日期–月")]
        public string ENTRY_MM { get; set; }

        /// <summary>
        /// 輸入日期–日
        /// </summary>
        [Description("輸入日期–日")]
        public string ENTRY_DD { get; set; }

        /// <summary>
        /// 登打日期 - 時間
        /// </summary>
        [Description("登打日期 - 時間")]
        public string ENTRY_TIME { get; set; }

        /// <summary>
        /// 輸入人員
        /// </summary>
        [Description("輸入人員")]
        public string ENTRY_ID { get; set; }

        /// <summary>
        /// 異動日期–年(民國年)
        /// </summary>
        [Description("異動日期–年(民國年)")]
        public string UPD_YY { get; set; }

        /// <summary>
        /// 異動日期–月
        /// </summary>
        [Description("異動日期–月")]
        public string UPD_MM { get; set; }

        /// <summary>
        /// 異動日期–日
        /// </summary>
        [Description("異動日期–日")]
        public string UPD_DD { get; set; }

        /// <summary>
        /// 異動人員代號
        /// </summary>
        [Description("異動人員代號")]
        public string UPD_ID { get; set; }
    }
}