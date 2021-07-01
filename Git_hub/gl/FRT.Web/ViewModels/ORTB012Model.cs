using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB012Model
    {

        [Display(Name = "查詢電文種類")]
        public string elecType { get; set; }

        [Display(Name = "匯款日期")]
        public string actDateB { get; set; }

        public string actDateE { get; set; }

        [Display(Name = "快速付款編號")]
        public string fastNoB { get; set; }

        public string fastNoE { get; set; }

        [Display(Name = "快速付款編號")]
        public string fastNo { get; set; }

        [Display(Name = "匯款日期")]
        public string actDate { get; set; }

        [Display(Name = "銀行代號")]
        public string bank { get; set; }

        [Display(Name = "匯款帳號")]
        public string actNo { get; set; }

        [Display(Name = "匯款金額")]
        public string rmtAmt { get; set; }

        [Display(Name = "收款人戶名")]
        public string rcvName { get; set; }

        [Display(Name = "錯誤代碼")]
        public string errorCode { get; set; }

        [Display(Name = "附言")]
        public string rmtApx { get; set; }

        [Display(Name = "電文日期/時間")]
        public string crtTime { get; set; }

        public string updTime { get; set; }

        [Display(Name = "RMT_TYPE")]
        public string rmtType { get; set; }

        [Display(Name = "FunCode")]
        public string funCode { get; set; }

        [Display(Name = "STATUS")]
        public string status { get; set; }

        public string emsgId { get; set; }

        public string emsgTxt { get; set; }

        public DateTime _crtTime { get; set; }

        public ORTB012Model() {
            elecType = "";
            actDateB = "";
            actDateE = "";
            fastNoB = "";
            fastNoE = "";

            fastNo = "";
            actDate = "";
            bank = "";
            actNo = "";
            rmtAmt = "";
            rcvName = "";
            errorCode = "";
            rmtApx = "";
            crtTime = "";
            updTime = "";
            emsgId = "";
            emsgTxt = "";
            rmtType = "";
            funCode = "";
            status = "";
        }

    }
}