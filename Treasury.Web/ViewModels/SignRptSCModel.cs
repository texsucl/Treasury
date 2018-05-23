using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class SignRptSCModel
    {
        [Display(Name = "批次號碼")]
        public String cBatchNo { get; set; }

        [Display(Name = "狀態")]
        public String cStatus { get; set; }

        public bool bPrint { get; set; }
    }


}