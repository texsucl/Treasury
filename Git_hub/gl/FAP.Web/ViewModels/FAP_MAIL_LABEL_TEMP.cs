using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class FAP_MAIL_LABEL_TEMP
    {
        /// <summary>
        /// 寄送方式
        /// </summary>
        public string send_style { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        public string zip_code { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string addr { get; set; }

        /// <summary>
        /// 收件人員
        /// </summary>
        public string rcv_id { get; set; }

        /// <summary>
        /// 應付票據變更主檔申請單號碼
        /// </summary>
        public string apply_no { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        public string check_no { get; set; }

        /// <summary>
        /// 行政單位
        /// </summary>
        public string apply_name { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        public string apply_id { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        public string rece_id { get; set; }
    }
}