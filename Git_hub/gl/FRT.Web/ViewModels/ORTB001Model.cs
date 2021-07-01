using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB002Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "銀行代碼")]
        public string bankCode { get; set; }

        [Display(Name = "銀行名稱")]
        public string bankName { get; set; }

        [Display(Name = "快速付款銀行類型")]
        public string bankType { get; set; }

        [Display(Name = "資料狀態")]
        public string status { get; set; }
        public string statusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTime { get; set; }

  
        


        public ORTB002Model() {
            tempId = "";
            aplyNo = "";
            bankCode = "";
            bankName = "";
            bankType = "";
            updId = "";
            updateUName = "";
            updDate = "";
            updTime = "";
            status = "";
            statusDesc = "";
        }

    }
}