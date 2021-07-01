using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FDCBANKAModel
    {
        public string bankNo { get; set; }
        public decimal effDaten { get; set; }
        public decimal stopDate { get; set; }
        public string remarkAr { get; set; }
        public string shrBq { get; set; }
        public string bankName { get; set; }
        public string bankAddr { get; set; }
        public string bankCity { get; set; }
        public string chgArea { get; set; }
        public decimal chgDays { get; set; }
        public string acctType { get; set; }
        public string entryId { get; set; }
        public decimal entryDate { get; set; }
        public decimal entryTime { get; set; }
        public string updId { get; set; }
        public decimal updDate { get; set; }
        public decimal updTime { get; set; }
        public string apprId { get; set; }
        public decimal apprDate { get; set; }
        public decimal apprTimeN { get; set; }
        public string filler10 { get; set; }
        public string filler20 { get; set; }
        public decimal filler08N { get; set; }


        public FDCBANKAModel()
        {
            bankNo = "";
            effDaten = 0;
            stopDate = 0;
            remarkAr = "";
            shrBq = "";
            bankName = "";
            bankAddr = "";
            bankCity = "";
            chgArea = "";
            chgDays = 0;
            acctType = "";
            entryId = "";
            entryDate = 0;
            entryTime = 0;
            updId = "";
            updDate = 0;
            updTime = 0;
            apprId = "";
            apprDate = 0;
            apprTimeN = 0;
            filler10 = "";
            filler20 = "";
            filler08N = 0;
        }
    }
}