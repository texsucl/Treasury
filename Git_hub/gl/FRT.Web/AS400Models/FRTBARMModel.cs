using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTBARMModel
    {

        public string sysType { get; set; }

        public string srceFrom { get; set; }

        public string srceKind { get; set; }

        public string policyNo { get; set; }

        public string policySeq { get; set; }

        public string idDup { get; set; }

        public string changeId { get; set; }

        public string changeSeq { get; set; }

        public string fastNo { get; set; }

        public string bankCode { get; set; }

        public string subBank { get; set; }

        public string bankAct { get; set; }

        public string currency { get; set; }

        public string remitAmt { get; set; }

        public string rcvName { get; set; }

        public string textRcvdt { get; set; }

        public string textRcvtm { get; set; }

        public string failCode { get; set; }

        public string entryId { get; set; }

        public string closeDate { get; set; }

        public string paidId { get; set; }

        public string memberId { get; set; }

        public string errCode { get; set; }

        public string errDesc { get; set; }

        public string errBelong { get; set; }

        public string remitDate { get; set; }

        public string filler_20 { get; set; }

        public FRTBARMModel()
        {
            sysType = "";
            srceFrom = "";
            srceKind = "";
            policyNo = "";
            policySeq = "";
            idDup = "";
            changeId = "";
            changeSeq = "";
            fastNo = "";
            bankCode = "";
            subBank = "";
            bankAct = "";
            currency = "";
            remitAmt = "";
            rcvName = "";
            textRcvdt = "";
            textRcvtm = "";
            failCode = "";
            entryId = "";
            closeDate = "";
            paidId = "";
            memberId = "";
            errCode = "";
            errDesc = "";
            errBelong = "";
            remitDate = "";
            filler_20 = "";
        }
    }
}