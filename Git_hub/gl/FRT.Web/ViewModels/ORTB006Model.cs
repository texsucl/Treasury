using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB006Model
    {

        [Display(Name = "快速付款編號")]
        public string fastNo { get; set; }

        [Display(Name = "保單號碼")]
        public string policyNo { get; set; }

        [Display(Name = "保單序號")]
        public string policySeq { get; set; }

        [Display(Name = "身份証重覆別")]
        public string idDup { get; set; }

        [Display(Name = "匯款日期")]
        public string remitDate { get; set; }

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

        [Display(Name = "匯款失敗原因")]
        public string failCode { get; set; }

        public string failCodeDesc { get; set; }

        public string entryId { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTime { get; set; }


        [Display(Name = "申請單號")]
        public string applyNo { get; set; }

        /// add by mark
        /// <summary>
        ///  匯款轉檔批號
        /// </summary>
        public string filler_20 { get; set; }

        public ORTB006Model() {
            fastNo = "";
            policyNo = "";
            policySeq = "";
            idDup = "";
            remitDate = "";
            bankCode = "";
            subBank = "";
            rcvName = "";
            bankAct = "";
            remitAmt = "";
            failCode = "";
            failCodeDesc = "";
            entryId = "";
            updId = "";
            applyNo = "";
            updDate = "";
            updTime = "";
            filler_20 = "";
        }

    }
}