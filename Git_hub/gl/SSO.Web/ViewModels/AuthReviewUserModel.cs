using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class AuthReviewUserModel
    {
        [Display(Name = "覆核單編號")]
        public string aplyNo { get; set; }

        [Display(Name = "申請人")]
        public string createUid { get; set; }

        [Display(Name = "申請日期時間")]
        public string createDt { get; set; }

        [Display(Name = "使用者帳號")]
        public string userId { get; set; }

        [Display(Name = "姓名")]
        public string userName { get; set; }

        [Display(Name = "停用註記")]
        public string isDisabled { get; set; }

        public string isDisabledDesc { get; set; }

        public string isDisabledB { get; set; }
        public string isDisabledDescB { get; set; }

        [Display(Name = "是否寄送MAIL")]
        public string isMail { get; set; }

        public string isMailDesc { get; set; }

        public string isMailB { get; set; }

        public string isMailDescB { get; set; }

        [Display(Name = "覆核結果")]
        public String apprStatus { get; set; }

        public String apprStatusDesc { get; set; }


        [Display(Name = "執行功能")]
        public String execAction { get; set; }

        public String execActionDesc { get; set; }

        [Display(Name = "覆核人")]
        public String apprUid { get; set; }

        [Display(Name = "覆核日期")]
        public String apprDt { get; set; }


        public AuthReviewUserModel() {
            aplyNo = "";
            createUid = "";
            createDt = "";
            userId = "";
            userName = "";
            isDisabled = "";
            isDisabledDesc = "";
            isDisabledB = "";
            isDisabledDescB = "";
            isMail = "";
            isMailDesc = "";
            isMailB = "";
            isMailDescB = "";
            apprStatus = "";
            apprStatusDesc = "";
            execAction = "";
            execActionDesc = "";
            apprUid = "";
            apprDt = "";

        }

    }
}