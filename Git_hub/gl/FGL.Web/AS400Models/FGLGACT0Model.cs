using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLGACT0Model
    {
        [Display(Name = "AS400 會計科目")]
        public string actNum { get; set; }

        [Display(Name = "AS400 會計名稱")]
        public string actName { get; set; }

        [Display(Name = "ＳＱＬ會計科目")]
        public string sqlActnum { get; set; }

        [Display(Name = "ＳＱＬ會科名稱")]
        public string sqlActnm { get; set; }

        [Display(Name = "保留文字 10(1)")]
        public string field101 { get; set; }

        [Display(Name = "保留文字10(2)")]
        public string field102 { get; set; }

        [Display(Name = "保留文字10(3)")]
        public string field103 { get; set; }

        [Display(Name = "保留日期08(1)")]
        public string field081 { get; set; }

        [Display(Name = "保留日期08(2)")]
        public string field082 { get; set; }

        [Display(Name = "保留日期08(3)")]
        public string field083 { get; set; }

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

       


        public FGLGACT0Model()
        {
            actNum = "";
            actName = "";
            sqlActnum = "";
            sqlActnm = "";
            field101 = "";
            field102 = "";
            field103 = "";
            field081 = "";
            field082 = "";
            field083 = "";
            entryId = "";
            entryDate = "";
            entryTime = "";
            updId = "";
            updDate = "";
            updTime = "";
 
        }
    }
}