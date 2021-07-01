using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB010Model
    {
        public string tempId { get; set; }

        [Display(Name = "設定項目")]
        public string code { get; set; }

        public string codeValue { get; set; }

        public string sysCd { get; set; }

        public string grpId { get; set; }

        public string grpDesc { get; set; }

        [Display(Name = "參數代號")]
        public string paraId { get; set; }

        [Display(Name = "設定值")]
        public string paraValue { get; set; }

        [Display(Name = "參數說明")]
        public string remark { get; set; }

        public string reserve1 { get; set; }

        public string reserve2 { get; set; }

        public string reserve3 { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        public string apprStatus { get; set; }

        public string apprUid { get; set; }

        public string apprDt { get; set; }

        public string createUid { get; set; }

        public string createDt { get; set; }
        


        public ORTB010Model() {
            tempId = "";
            code = "";
            codeValue = "";
            sysCd = "";
            grpId = "";
            grpDesc = "";
            paraId = "";
            paraValue = "";
            remark = "";
            reserve1 = "";
            reserve2 = "";
            reserve3 = "";
            aplyNo = "";
            apprStatus = "";
            apprUid = "";
            apprDt = "";
            createUid = "";
            createDt = "";
            
        }

    }
}