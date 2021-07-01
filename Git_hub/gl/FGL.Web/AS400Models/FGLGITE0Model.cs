using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLGITE0Model
    {
        [Display(Name = "系統別")]
        public string sysType { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }

        [Display(Name = "保險商品編號版本")]
        public string numVrsn { get; set; }

        [Display(Name = "保險商品編號")]
        public string num { get; set; }

        [Display(Name = "新增人員")]
        public string entryId { get; set; }

        [Display(Name = "新增日期")]
        public string entryDate { get; set; }

        [Display(Name = "新增時間")]
        public string entryTime { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTime { get; set; }

      


        public FGLGITE0Model()
        {
            sysType = "";
            item = "";
            numVrsn = "";
            num = "";
            entryId = "";
            entryDate = "";
            entryTime = "";
            updId = "";
            updDate = "";
            updTime = "";
            
        }
    }
}