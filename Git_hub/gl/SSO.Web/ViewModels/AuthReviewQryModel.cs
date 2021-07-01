using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class AuthReviewQryModel
    {
        [Display(Name = "覆核單編號")]
        public String cReviewSeq { get; set; }

        [Display(Name = "覆核單種類")]
        public String cReviewType { get; set; }

        [Display(Name = "覆核單種類")]
        public String cReviewTypeDesc { get; set; }

        [Display(Name = "覆核狀態")]
        public String cReviewFlag { get; set; }

        [Display(Name = "覆核狀態")]
        public String cReviewFlagDesc { get; set; }


        [Display(Name = "申請人")]
        public String cCrtUserID { get; set; }

        [Display(Name = "申請日期")]
        public String cCrtDateB { get; set; }

        [Display(Name = "申請日期")]
        public String cCrtDateE { get; set; }

        [Display(Name = "覆核人")]
        public String cReviewUserID { get; set; }

        [Display(Name = "覆核日期")]
        public String cReviewDateB { get; set; }

        public String cReviewDateE { get; set; }

        [Display(Name = "覆核意見")]
        public String cReviewMemo { get; set; }

        [Display(Name = "異動資料內容")]
        public String cMappingKey { get; set; }

        public String cMappingKeyDesc { get; set; }

        [Display(Name = "角色")]
        public String cRoleId { get; set; }

        [Display(Name = "使用者")]
        public String cUserId { get; set; }
    }
}