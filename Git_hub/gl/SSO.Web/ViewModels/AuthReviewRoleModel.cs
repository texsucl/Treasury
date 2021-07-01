using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
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

        [Display(Name = "程式OWNER單位")]
        public string authUnit { get; set; }
        public string authUnitNm { get; set; }

        [Display(Name = "允許其他單位授權")]
        public string freeAuth { get; set; }
        public string freeAuthB { get; set; }


        [Display(Name = "角色名稱")]
        public string roleName { get; set; }

        public string roleNameB { get; set; }


        [Display(Name = "停用註記")]
        public string isDisabled { get; set; }

        public string isDisabledB { get; set; }

        [Display(Name = "備註")]
        public string memo { get; set; }

        public string memoB { get; set; }

        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        public string execActionDesc { get; set; }

        public AuthReviewRoleModel() {
            aplyNo = "";
            createUid = "";
            createDt = "";
            roleId = "";
            authUnit = "";
            authUnitNm = "";
            freeAuth = "";
            freeAuthB = "";
            roleName = "";
            roleNameB = "";
            isDisabled = "";
            isDisabledB = "";
            memo = "";
            memoB = "";
            execAction = "";
            execActionDesc = "";

        }

    }
}