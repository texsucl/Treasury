using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB011Model
    {

        [Display(Name = "快速付款編號")]
        public string fastNo { get; set; }
        

        public ORTB011Model() {
            fastNo = "";
        }

    }
}