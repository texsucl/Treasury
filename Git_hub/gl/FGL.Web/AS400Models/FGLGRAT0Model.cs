using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLGRAT0Model
    {

        [Display(Name = "AS400 會計科目 ")]
        public string actNum { get; set; }

        [Display(Name = "稅務別")]
        public string taxType { get; set; }

        [Display(Name = "新增人員")]
        public string entryId { get; set; }

        [Display(Name = "新增日期")]
        public string entryDate { get; set; }

        [Display(Name = "新增時間")]
        public string entryTime { get; set; }


        public FGLGRAT0Model()
        {
            actNum = "";
            taxType = "";
            entryId = "";
            entryDate = "0";
            entryTime = "0";

        }
    }
}