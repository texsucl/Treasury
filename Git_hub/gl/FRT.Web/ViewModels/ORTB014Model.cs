using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB014Model
    {
        [Display(Name = "輸入日期")]
        public string qDateB { get; set; }

        public string qDateE { get; set; }


        public string tempId { get; set; }

        [Display(Name = "銀行代號")]
        public string bankNo { get; set; }

        [Display(Name = "銀行帳號")]
        public string bankAct { get; set; }

        [Display(Name = "失敗代碼")]
        public string failCode { get; set; }

        public string failCodeDesc { get; set; }

        [Display(Name = "輸入日期")]
        public string entryDate { get; set; }

        [Display(Name = "匯費序號")]
        public string feeSeqn { get; set; }

        [Display(Name = "拒絕付款")]
        public string rejectPay { get; set; }

        [Display(Name = "異動日期")]
        public string updDate { get; set; }

        [Display(Name = "異動時間")]
        public string updTimeN { get; set; }

        [Display(Name = "異動人員代號")]
        public string updId { get; set; }

        [Display(Name = "覆核日期")]
        public string apprDate { get; set; }

        [Display(Name = "覆核時間 ")]
        public string apprTimeN { get; set; }

        [Display(Name = "覆核人員")]
        public string apprId { get; set; }

        [Display(Name = "備用１０")]
        public string filler10 { get; set; }

        [Display(Name = "備用２０")]
        public string filler20 { get; set; }

        [Display(Name = "備用數字０８")]
        public string filler08N { get; set; }

        [Display(Name = "資料狀態")]
        public string status { get; set; }
        public string statusDesc { get; set; }



        public ORTB014Model() {
            qDateB = "";
            qDateE = "";

            tempId = "";
            bankNo = "";
            bankAct = "";
            failCode = "";
            failCodeDesc = "";
            entryDate = "";
            feeSeqn = "";
            rejectPay = "";
            updDate = "";
            updTimeN = "";
            updId = "";
            apprDate = "";
            apprTimeN = "";
            apprId = "";
            filler10 = "";
            filler20 = "";
            filler08N = "";
            status = "";
            statusDesc = "";
        }

    }
}