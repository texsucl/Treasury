using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0003Model
    {
        public string temp_id { get; set; }

        [Display(Name = "覆核單號")]
        public string aplyNo { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        [Display(Name = "申請人")]
        public string create_id { get; set; }

        [Display(Name = "申請時間")]
        public string create_dt { get; set; }

        [Display(Name = "查詢暫存資料")]
        public string isQryTmp { get; set; }



        [Display(Name = "信函編號")]
        public string report_no { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象")]
        public string paid_name { get; set; }

        [Display(Name = "處理階段")]
        public string r_status { get; set; }




        public OAP0003Model() {

            temp_id = "";
            aplyNo = "";
            apprStat = "0";
            create_id = "";
            create_dt = "";
            isQryTmp = "";
            report_no = "";
            paid_id = "";
            paid_name = "0";
            r_status = "";

        }

    }
}