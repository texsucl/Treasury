using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class APAplyRecModel
    {
        [Display(Name = "覆核單編號")]
        public String aply_no { get; set; }

        [Display(Name = "覆核單種類")]
        public String aply_type { get; set; }


        [Display(Name = "覆核狀態")]
        public String appr_stat { get; set; }

        [Display(Name = "覆核狀態")]
        public String appr_stat_desc { get; set; }


        [Display(Name = "申請人")]
        public String create_id { get; set; }

        [Display(Name = "申請時間")]
        public String create_dt { get; set; }

        [Display(Name = "覆核人")]
        public String appr_uid { get; set; }

        [Display(Name = "覆核時間")]
        public String appr_dt { get; set; }

        [Display(Name = "覆核意見")]
        public String appr_desc { get; set; }

        [Display(Name = "異動資料內容")]
        public String appr_mapping_key { get; set; }

        [Display(Name = "備註")]
        public String memo { get; set; }


        public APAplyRecModel() {
            aply_no = "";
            aply_type = "";
            appr_stat = "";
            appr_stat_desc = "";
            create_id = "";
            create_dt = "";
            appr_uid = "";
            appr_dt = "";
            appr_desc = "";
            appr_mapping_key = "";
            memo = "";
        }
    }


}