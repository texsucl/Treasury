using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0021Model
    {
        public OAP0021Model() {
            checkFlag = true;
        }
        public bool checkFlag { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 申請日
        /// </summary>
        [Description("申請日")]
        public string apply_date { get; set; }

        /// <summary>
        /// 接收日
        /// </summary>
        [Description("接收日")]
        public string rece_date { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 申請人員(中文)
        /// </summary>
        [Description("申請人員(中文)")]
        public string apply_id_D { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }

        /// <summary>
        /// 接收人員(中文)
        /// </summary>
        [Description("接收人員(中文)")]
        public string rece_id_D { get; set; }

        /// <summary>
        /// 申請部門
        /// </summary>
        [Description("申請部門")]
        public string apply_dep { get; set; }

        /// <summary>
        /// 申請部門(中文)
        /// </summary>
        [Description("申請部門(中文)")]
        public string apply_dep_D { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool apprFlag { get; set; }

    }
}