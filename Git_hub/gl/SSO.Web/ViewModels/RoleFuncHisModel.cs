using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class RoleFuncHisModel
    {
        [Display(Name = "覆核單編號")]
        public string aplyNo { get; set; }

        public string updateDT { get; set; }

        public string updateUid { get; set; }

        public string apprStatus { get; set; }

        public string apprStatusDesc { get; set; }

        [Display(Name = "角色編號")]
        public string cRoleID { get; set; }

        [Display(Name = "功能編號")]
        public string cFunctionID { get; set; }

        [Display(Name = "功能名稱")]
        public string cFunctionName { get; set; }

        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        public string execActionDesc { get; set; }


    }
}