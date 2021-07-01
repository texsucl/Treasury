using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0031ViewModel : IFileDownLoadModel , IFileUpLoadModel
    {
        public OAP0031ViewModel() {
            checkFlag = true;
        }

        public bool checkFlag { get; set; }

        /// <summary>
        /// PKID
        /// </summary>
        [Description("PKID")]
        public string pkid { get; set; }

        /// <summary>
        /// 標籤號碼
        /// </summary>
        [Description("標籤號碼")]
        public string label_no { get; set; }

        /// <summary>
        /// 寄送方式
        /// </summary>
        [Description("寄送方式")]
        public string send_style { get; set; }

        /// <summary>
        /// 寄送方式(中文)
        /// </summary>
        [Description("寄送方式(中文)")]
        public string send_style_D { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Description("郵遞區號")]
        public string zip_code { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [Description("地址")]
        public string addr { get; set; }

        /// <summary>
        /// 收件人員
        /// </summary>
        [Description("收件人員")]
        public string rcv_id { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string memo { get; set; }

        /// <summary>
        /// 張數
        /// </summary>
        [Description("張數")]
        public string number { get; set; }

        /// <summary>
        /// 行政單位
        /// </summary>
        [Description("行政單位")]
        public string apply_name { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 大宗掛號號碼
        /// </summary>
        [Description("大宗掛號號碼")]
        public string bulk_no { get; set; }

        /// <summary>
        /// 新增日期
        /// </summary>
        [Description("新增日期")]
        public string create_date { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string update_date { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string update_id { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }

    }
}