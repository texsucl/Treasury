using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class AuthReviewRoleModel
    {
        [Display(Name = "覆核單編號")]
        public string aplyNo { get; set; }

        [Display(Name = "申請人")]
        public string createUid { get; set; }

        [Display(Name = "申請日期時間")]
        public string createDt { get; set; }

        [Display(Name = "角色編號")]
        public string roleId { get; set; }

        [Display(Name = "角色名稱")]
        public string roleName { get; set; }

        public string roleNameB { get; set; }

        [Display(Name = "角色群組")]
        public string roleAuthType { get; set; }

        public string roleAuthTypeDesc { get; set; }


        [Display(Name = "停用註記")]
        public string isDisabled { get; set; }

        public string isDisabledB { get; set; }

        [Display(Name = "備註")]
        public string memo { get; set; }

        public string memoB { get; set; }

        public AuthReviewRoleModel() {
            aplyNo = "";
            createUid = "";
            createDt = "";
            roleId = "";
            roleName = "";
            roleNameB = "";
            roleAuthType = "";
            roleAuthTypeDesc = "";
            isDisabled = "";
            isDisabledB = "";
            memo = "";
            memoB = "";


        }

    }
}