using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class AuthRptModel
    {
        [Display(Name = "報表種類")]
        public String cRptType { get; set; }

        [Display(Name = "網路帳號")]
        public String cUserId { get; set; }

        [Display(Name = "中文姓名")]
        public String cUserName { get; set; }

        [Display(Name = "上班單位")]
        public String cWorkUnit { get; set; }

        public String cWorkUnitName { get; set; }

        [Display(Name = "角色")]
        public String cRoleName { get; set; }

        [Display(Name = "指派單位及區間")]
        public String cMaintainUnit { get; set; }

        public String cEnableDate { get; set; }

        public String cDisableDate { get; set; }

        public String dptType { get; set; }

        public String upDptCd { get; set; }

        public String upDptName { get; set; }
    }
}