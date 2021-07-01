using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0001FileModel
    {

        [Display(Name = "在檔位置")]
        public string linePos { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "保單號碼")]
        public string policyNo { get; set; }

        [Display(Name = "保單序號")]
        public string policySeq { get; set; }

        [Display(Name = "重覆碼")]
        public string idDup { get; set; }

        [Display(Name = "案號人員別")]
        public string memberId { get; set; }

        [Display(Name = "案號")]
        public string changeId { get; set; }

        [Display(Name = "給付對象 ID ")]
        public string paidId { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paidName { get; set; }

        [Display(Name = "要保人 ID ")]
        public string applId { get; set; }

        [Display(Name = "要保人姓名")]
        public string applName { get; set; }

        [Display(Name = "被保人 ID")]
        public string insId { get; set; }

        [Display(Name = "被保人姓名")]
        public string insName { get; set; }

        [Display(Name = "原給付日")]
        public string oPaidDt { get; set; }

        [Display(Name = "幣別")]
        public string currency { get; set; }

        [Display(Name = "回存金額")]
        public string mainAmt { get; set; }

        [Display(Name = "支票金額")]
        public string checkAmt { get; set; }

        [Display(Name = "支票號碼／匯費序號")]
        public string checkNo { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string checkShrt { get; set; }

        [Display(Name = "支票到期日")]
        public string checkDate { get; set; }

        [Display(Name = "原給付性質")]
        public string oPaidCd { get; set; }

        [Display(Name = "比對到之郵遞區號")]
        public string rZipCode { get; set; }

        [Display(Name = "比對到之地址")]
        public string rAddr { get; set; }

        [Display(Name = "送件人員")]
        public string sendId { get; set; }

        [Display(Name = "送件人員姓名")]
        public string sendName { get; set; }

        [Display(Name = "送件人員單位")]
        public string sendUnit { get; set; }

        [Display(Name = "報表代碼")]
        public string report { get; set; }

        [Display(Name = "作廢代碼")]
        public string delCode { get; set; }


        [Display(Name = "AML註記")]
        public string filler14 { get; set; }


        public string dataFlag { get; set; }


        public OAP0001FileModel() {
            linePos = "";
            system = "";
            policyNo = "";
            policySeq = "";
            idDup = "";
            memberId = "";
            changeId = "";
            paidId = "";
            paidName = "";
            applId = "";
            applName = "";
            insId = "";
            insName = "";
            oPaidDt = "";
            currency = "";
            mainAmt = "";
            checkAmt = "";
            checkNo = "";
            checkShrt = "";
            checkDate = "";
            oPaidCd = "";
            rZipCode = "";
            rAddr = "";
            sendId = "";
            sendName = "";
            sendUnit = "";
            report = "";
            delCode = "";
            filler14 = "";
            dataFlag = "";
        }

    }
}