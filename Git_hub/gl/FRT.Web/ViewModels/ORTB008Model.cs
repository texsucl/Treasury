using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{

    public class ORTB008Model
    {
        [Display(Name = "發送日期")]
        public string qDateB  { get; set; }
        public string qDateE  { get; set; }

        public string seq { get; set; }

        public string mailDate { get; set; }

        public string mailTime { get; set; }

        [Display(Name = "收件者帳號")]
        public string receiverEmpno { get; set; }


        public string eMail { get; set; }

        [Display(Name = "寄送結果")]
        public string mailResult { get; set; }

        public string resultDesc { get; set; }

        public string mailSub { get; set; }

        public ORTB008Model() {
            seq = "";
            mailDate = "";
            mailTime = "";
            receiverEmpno = "";
            eMail = "";
            mailResult = "";
            resultDesc = "";
            mailSub = "";
        }

    }
}