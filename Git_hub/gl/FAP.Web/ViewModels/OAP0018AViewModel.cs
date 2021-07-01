using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0018AViewModel
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string aply_no { get; set; }

        /// <summary>
        /// 申請人ID
        /// </summary>
        [Description("申請人ID")]
        public string aply_id { get; set; }

        /// <summary>
        /// 申請人name
        /// </summary>
        [Description("申請人name")]
        public string aply_name { get; set; }
        
        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string aply_date { get; set; }

        /// <summary>
        /// 是否選取
        /// </summary>
        [Description("是否選取")]
        public bool Ischecked { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool review_flag { get; set; }
    }
}