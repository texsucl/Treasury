using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB009Model
    {

        [Display(Name = "快速付款編號")]
        public string fastNo { get; set; }

        [Display(Name = "收款人ID")]
        public string paidId { get; set; }

        [Display(Name = "總行碼")]
        public string bankCode { get; set; }

        [Display(Name = "分行代碼")]
        public string subBank { get; set; }

        [Display(Name = "收款人戶名")]
        public string rcvName { get; set; }

        [Display(Name = "匯款帳號")]
        public string bankAct { get; set; }

        [Display(Name = "匯款金額")]
        public string remitAmt { get; set; }

        public string remitStat { get; set; }


        public string entryId { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTime { get; set; }


        [Display(Name = "申請單號")]
        public string applyNo { get; set; }

        public ORTB009Model() {
            fastNo = "";
            bankCode = "";
            subBank = "";
            rcvName = "";
            bankAct = "";
            remitAmt = "";
            remitStat = "";
            entryId = "";
            updId = "";
            applyNo = "";
            updDate = "";
            updTime = "";
        }

    }
}