using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class CodeUserHisModel
    {
       

        [Display(Name = "使用者帳號")]
        public string cUserID { get; set; }

        [Display(Name = "使用者姓名")]
        public string cUserName { get; set; }

        [Display(Name = "啟用/停用")]
        public string isDisabled { get; set; }
        public string isDisabledDesc { get; set; }

        public string isDisabledB { get; set; }
        public string isDisabledDescB { get; set; }

        [Display(Name = "寄送MAIL")]
        public string isMail { get; set; }
        public string isMailDesc { get; set; }

        public string isMailB { get; set; }
        public string isMailDescB { get; set; }

        public string execAction { get; set; }

        public string execActionDesc { get; set; }

        [Display(Name = "覆核單編號")]
        public string aplyNo { get; set; }


        public string updateDT { get; set; }

        public string updateUid { get; set; }

        public string apprStatus { get; set; }

        public string apprStatusDesc { get; set; }



        public CodeUserHisModel()
        {
            

            cUserID = "";
            cUserName = "";
            isDisabled = "";
            isDisabledDesc = "";
            isDisabledB = "";
            isDisabledDescB = "";
            isMail = "";
            isMailDesc = "";
            isMailB = "";
            isMailDescB = "";
            execAction = "";
            execActionDesc = "";
            aplyNo = "";
            updateDT = "";
            updateUid = "";
            apprStatus = "";
            apprStatusDesc = "";
        }

    }
}