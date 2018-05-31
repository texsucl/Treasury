using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class RoleMgrHisModel
    {
        [Display(Name = "角色代碼")]
        public string cRoleID { get; set; }

        [Display(Name = "角色名稱")]
        public string cRoleName { get; set; }

        [Display(Name = "角色群組")]
        public string roleAuthType { get; set; }


        [Display(Name = "異動日期/時間")]
        public string cUpdDateTime { get; set; }

        public string updDateB { get; set; }

        public string updDateE { get; set; }


        [Display(Name = "覆核狀態")]
        public string apprtatus { get; set; }



        public RoleMgrHisModel() {
            cRoleID = "";
            cRoleName = "";
            roleAuthType = "";
            cUpdDateTime = "";
            apprtatus = "";
        }

    }
    
}