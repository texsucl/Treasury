using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0002PoliModel
    {
        public string temp_id { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "案號人員別")]
        public string member_id { get; set; }

        [Display(Name = "案號")]
        public string change_id { get; set; }

        [Display(Name = "回存金額")]
        public string main_amt { get; set; }


        public OAP0002PoliModel() {

            temp_id = "";
            policy_no = "";
            policy_seq = "0";
            id_dup = "";
            member_id = "";
            change_id = "";
            main_amt = "";
            
        }

    }
}