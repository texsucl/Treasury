using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB001Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "系統別")]
        public string sysType { get; set; }

        [Display(Name = "資料來源")]
        public string srceFrom { get; set; }

        [Display(Name = "資料類別")]
        public string srceKind { get; set; }

        [Display(Name = "來源程式")]
        public string srcePgm { get; set; }

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

  
        


        public ORTB001Model() {
            tempId = "";
            aplyNo = "";
            sysType = "";
            srceFrom = "";
            srceKind = "";
            srcePgm = "";
            updId = "";
            updateUName = "";
            updDate = "";
            updTime = "";
            status = "";
            statusDesc = "";
        }

    }
}