using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB007Model
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

        public string remitDate { get; set; }

        public string closeDate { get; set; }

        public string paidId { get; set; }

        public string memberId { get; set; }

        public string entryId { get; set; }

        public string entryName { get; set; }

        public string entryUnit { get; set; }

        public ORTB007Model() {
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
            remitDate = "";
            rcvName = "";
            textRcvdt = "";
            textRcvtm = "";
            failCode = "";
            closeDate = "";
            paidId = "";
            memberId = "";
            entryId = "";
            entryName = "";
            entryUnit = "";
        }

    }
}