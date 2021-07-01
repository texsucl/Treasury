using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB004Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "錯誤代碼")]
        public string errCode { get; set; }

        [Display(Name = "錯誤代碼說明")]
        public string errDesc { get; set; }

        [Display(Name = "錯誤歸屬")]
        public string errBelong { get; set; }

        [Display(Name = "轉碼代號")]
        public string transCode { get; set; }

        public string transCodeDesc { get; set; }

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



        public ORTB004Model() {
            tempId = "";
            aplyNo = "";
            errCode = "";
            errDesc = "";
            errBelong = "";
            transCode = "";
            transCodeDesc = "";
            updId = "";
            updateUName = "";
            updDate = "";
            updTime = "";
            status = "";
            statusDesc = "";
        }

    }
}