using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0001Model
    {

        [Display(Name = "在檔位置")]
        public string linePos { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼")]
        public string checkNo { get; set; }

        [Display(Name = "帳戶簡稱")]
        public string checkShrt { get; set; }

        [Display(Name = "檢核訊息")]
        public string msg { get; set; }



        public OAP0001Model() {
            linePos = "";
            system = "";
            checkNo = "";
            checkShrt = "";
            msg = "";
            
        }

    }
}