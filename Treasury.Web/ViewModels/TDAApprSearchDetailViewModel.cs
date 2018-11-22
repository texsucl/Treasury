using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class TDAApprSearchDetailViewModel
    {
        /// <summary>
        /// 點選狀態
        /// </summary>
        [Description("點選狀態")]
        public bool vCheckFlag { get; set; }

        /// <summary>
        /// 定義檔維護項目
        /// </summary>
        [Description("定義檔維護項目")]
        public string vTDA_Id { get; set; }

        /// <summary>
        /// 定義檔維護項目名稱
        /// </summary>
        [Description("定義檔維護項目名稱")]
        public string vTDA_Desc { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }


        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAply_Dt { get; set; }

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

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAppr_Status { get; set; }
    }

    public class TDAApprSearchViewModel
    {
        /// <summary>
        /// 定義檔維護項目
        /// </summary>
        [Description("定義檔維護項目")]
        public string vTDA_Id { get; set; }

        /// <summary>
        /// 填表人員
        /// </summary>
        [Description("填表人員")]
        public string vCreateUid { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// 申請日期(開始)
        /// </summary>
        [Description("申請日期(開始)")]
        public string vAPLY_DT_S { get; set; }

        /// <summary>
        /// 申請日期(結束)
        /// </summary>
        [Description("申請日期(結束)")]
        public string vAPLY_DT_E { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Description("申請人")]
        public string vAPLY_ID { get; set; }
    }
}
