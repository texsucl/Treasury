using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class AuthRptModel
    {
        [Display(Name = "報表種類")]
        public String rptType { get; set; }

        //200319 Bianco Add
        [Display(Name = "報表格式")]
        public String rptFormat { get; set; }
        //200319 Bianco Add

        [Display(Name = "網路帳號")]
        public String user_id { get; set; }

        [Display(Name = "中文姓名")]
        public String user_name { get; set; }

        [Display(Name = "上班單位")]
        public String dpt_cd { get; set; }

        public String dpt_name { get; set; }

        [Display(Name = "角色")]
        public String role_id { get; set; }

        public String role_name { get; set; }

        public String user_unit { get; set; }

        public String grp_id { get; set; }

        public String grp_name { get; set; }

        public String owner_unit { get; set; }

        public String owner_unit_name { get; set; }

        public String pgm_id { get; set; }

        public String pgm_name { get; set; }

     　 public string sys_owner_role { get; set; }

        public String parent_func_id { get; set; }     //200319 Bianco add 作業功能id

        public String parent_func_name { get; set; }    //200319 Bianco add 作業功能name
    }
}