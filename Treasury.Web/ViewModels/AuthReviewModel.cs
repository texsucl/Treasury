using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class AuthReviewModel
    {
        [Display(Name = "覆核單編號")]
        public String aplyNo { get; set; }

        [Display(Name = "覆核單種類")]
        public String cReviewType { get; set; }

        [Display(Name = "覆核單種類")]
        public String cReviewTypeDesc { get; set; }

        [Display(Name = "覆核狀態")]
        public String apprStatus { get; set; }

        [Display(Name = "覆核狀態")]
        public String apprStatusDesc { get; set; }


        [Display(Name = "申請人")]
        public String createUid { get; set; }

        [Display(Name = "申請時間")]
        public String createDt { get; set; }

        [Display(Name = "覆核人")]
        public String apprUid { get; set; }

        [Display(Name = "覆核時間")]
        public String apprDt { get; set; }

        [Display(Name = "覆核意見")]
        public String cReviewMemo { get; set; }

        [Display(Name = "角色群組")]
        public String roleAuthType { get; set; }

        [Display(Name = "異動資料內容")]
        public String cMappingKey { get; set; }

        public String cMappingKeyDesc { get; set; }

    }


}