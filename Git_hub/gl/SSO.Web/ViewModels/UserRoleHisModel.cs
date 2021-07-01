using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class UserRoleHisModel
    {
        [Display(Name = "覆核單編號")]
        public string aplyNo { get; set; }


        public string updateDT { get; set; }

        public string updateUid { get; set; }

        public string apprStatus { get; set; }

        public string apprStatusDesc { get; set; }

        [Display(Name = "使用者帳號")]
        public string userId { get; set; }

        [Display(Name = "角色編號")]
        public string cRoleID { get; set; }

        [Display(Name = "角色名稱")]
        public string cRoleName { get; set; }


        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        public string execActionDesc { get; set; }


        public UserRoleHisModel() {
            aplyNo = "";
            updateDT = "";
            updateUid = "";
            apprStatus = "";
            apprStatusDesc = "";
            userId = "";
            cRoleID = "";
            cRoleName = "";
            execAction = "";
            execActionDesc = "";
        }
    }
}