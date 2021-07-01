using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTCOPL0Model
    {
        public string paidType { get; set; }

        public string field081 { get; set; }

        public string currency { get; set; }

        public string corpNo { get; set; }

        public string vhrNo1 { get; set; }

        public string resource { get; set; }

        public string policyNo { get; set; }

        public string policySeq { get; set; }

        public string idDup { get; set; }

        public string changeId { get; set; }

        public string remitAmt { get; set; }

        public string endDate { get; set; }

        public string updId { get; set; }

        public string acptId { get; set; }

        public string genId { get; set; }

        public string filler10 { get; set; }

        public string field012 { get; set; }



        public string genUnit { get; set; }

        public string stsName { get; set; }


        public FRTCOPL0Model() {
            paidType = "";
            field081 = "";
            currency = "";
            corpNo = "";
            vhrNo1 = "";
            resource = "";
            policyNo = "";
            policySeq = "";
            idDup = "";
            changeId = "";
            remitAmt = "";
            endDate = "";
            updId = "";
            acptId = "";
            genId = "";
            filler10 = "";
            field012 = "";


            genUnit = "";
            stsName = "";
        }
    }
}