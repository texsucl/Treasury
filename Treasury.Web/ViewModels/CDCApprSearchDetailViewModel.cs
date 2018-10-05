using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class CDCApprSearchDetailViewModel
    {
        /// <summary>
        /// 點選狀態
        /// </summary>
        [Description("點選狀態")]
        public bool vCheckFlag { get; set; }

        /// <summary>
        /// 存取申請項目
        /// </summary>
        [Description("存取申請項目")]
        public string vItem_Id { get; set; }

        /// <summary>
        /// 存取申請項目中文
        /// </summary>
        [Description("存取申請項目中文")]
        public string vItem_Desc { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAply_Dt { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 申請人ID
        /// </summary>
        [Description("申請人ID")]
        public string vAply_Uid { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Description("申請人")]
        public string vAply_Uid_Name { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool vApprFlag { get; set; }

        /// <summary>
        /// 覆核回復意見
        /// </summary>
        [Description("覆核回復意見")]
        public string vAppr_Desc { get; set; }
    }

    public class CDCApprSearchViewModel
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 申請人ID
        /// </summary>
        [Description("申請人ID")]
        public string vAply_Uid { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAply_Dt { get; set; }

        /// <summary>
        /// 填表人員
        /// </summary>
        [Description("填表人員")]
        public string vCreateUid { get; set; }
    }
}