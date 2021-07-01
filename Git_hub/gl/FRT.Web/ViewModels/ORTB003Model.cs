using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB003Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "銀行類型")]
        public string bankType { get; set; }

        [Display(Name = "電文類型")]
        public string textType { get; set; }

        public string textTypeDesc { get; set; }

        [Display(Name = "起時")]
        public string strTime { get; set; }

        [Display(Name = "迄時")]
        public string endTime { get; set; }

        [Display(Name = "逾時天數")]
        public string timeoutD { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTime { get; set; }

        [Display(Name = "資料狀態")]
        public string status { get; set; }
        public string statusDesc { get; set; }



        public ORTB003Model() {
            tempId = "";
            aplyNo = "";
            bankType = "";
            textType = "";
            textTypeDesc = "";
            strTime = "";
            endTime = "";
            timeoutD = "";
            updId = "";
            updateUName = "";
            updDate = "";
            updTime = "";
            status = "";
            statusDesc = "";
        }

    }
}