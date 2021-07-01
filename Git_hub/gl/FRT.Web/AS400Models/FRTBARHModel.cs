using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTBARHModel
    {

        public string applyNo { get; set; }

        public string fastNo { get; set; }

        public string aplyType { get; set; }

        public string status { get; set; }

        public string failCode { get; set; }

        public string paidId { get; set; }

        public string bankCode { get; set; }

        public string subBank { get; set; }

        public string bankAct { get; set; }

        public string rcvName { get; set; }

        public string dupYn { get; set; }

        public string apprStat { get; set; }

        public string updId { get; set; }

        public string updDate { get; set; }

        public string updTime { get; set; }

        public string apprId { get; set; }

        public string apprDate { get; set; }

        public string apprTime { get; set; }
        


        public FRTBARHModel()
        {
            applyNo = "";
            fastNo = "";
            aplyType = "";
            status = "";
            failCode = "";
            paidId = "";
            bankCode = "";
            subBank = "";
            bankAct = "";
            rcvName = "";
            dupYn = "";
            apprStat = "";
            updId = "";
            updDate = "";
            updTime = "";
            apprId = "";
            apprDate = "";
            apprTime = "";
        }
    }
}