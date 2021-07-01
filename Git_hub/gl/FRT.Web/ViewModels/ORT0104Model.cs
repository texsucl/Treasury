using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0104Model
    {
        [Display(Name = "申請單號")]
        public string aply_no { get; set; }

        [Display(Name = "帳本代號")]
        public string corp_no { get; set; }

        [Display(Name = "轉帳憑證編號")]
        public string vhr_no1 { get; set; }

        [Display(Name = "處理序號")]
        public string pro_no { get; set; }

        [Display(Name = "收款人ＩＤ")]
        public string paid_id { get; set; }

        [Display(Name = "幣別")]
        public string currency { get; set; }

        [Display(Name = "退匯原因")]
        public string fail_code { get; set; }

        [Display(Name = "新處理序號")]
        public string seqn { get; set; }

        [Display(Name = "退匯原因_異動前")]
        public string fail_code_o { get; set; }

        [Display(Name = "新處理序號_異動前")]
        public string seqn_o { get; set; }



        [Display(Name = "收款人戶名")]
        public string payment { get; set; }

        [Display(Name = "匯款金額")]
        public string remit_amt { get; set; }

        [Display(Name = "銀行代號")]
        public string bank_no { get; set; }

        public string bank_name { get; set; }

        [Display(Name = "匯款帳號")]
        public string bank_act { get; set; }

        [Display(Name = "匯費序號")]
        public string fee_seqn { get; set; }

        [Display(Name = "系統別")]
        public string fbo_no { get; set; }



        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        [Display(Name = "異動人員姓名")]
        public string update_name { get; set; }

        [Display(Name = "異動日期")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期")]
        public string appr_datetime { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        public ORT0104Model()
        {

            aply_no = "";
            corp_no = "";
            vhr_no1 = "";
            pro_no = "";
            paid_id = "";
            currency = "";
            fail_code = "";
            seqn = "";
            fail_code_o = "";
            seqn_o = "";

            payment = "";
            remit_amt = "";
            bank_no = "";
            bank_name = "";
            bank_act = "";
            fee_seqn = "";
            fbo_no = "";

            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            appr_datetime = "";
            appr_stat = "";
            
        }

    }
}