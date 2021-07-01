using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    /// <summary>
    /// 預約電訪/進度查詢 查詢Model
    /// </summary>
    [Description("預約電訪/進度查詢 查詢Model")]
    public class DataSearchModel
    {
        /// <summary>
        /// 業務人員ID
        /// </summary>
        [Description("業務人員ID")]
        public string agentID { get; set; }

        /// <summary>
        /// 要保文件上傳起日
        /// </summary>
        [Description("要保文件上傳起日")]
        public string Sdate { get; set; }

        /// <summary>
        /// 要保文件上傳迄日
        /// </summary>
        [Description("要保文件上傳迄日")]
        public string Edate { get; set; }

        /// <summary>
        /// 要保人身份證字號
        /// </summary>
        [Description("要保人身份證字號")]
        public string custid { get; set; }

        /// <summary>
        /// 同意書編號
        /// </summary>
        [Description("同意書編號")]
        public string RptReceNo { get; set; }

        /// <summary>
        /// token
        /// </summary>
        [Description("token")]
        public string token { get; set; }
    }
}