using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0002Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aplyNo { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        public string execActionDesc { get; set; }

        [Display(Name = "申請人")]
        public string create_id { get; set; }

        [Display(Name = "申請時間")]
        public string create_dt { get; set; }

        [Display(Name = "查詢暫存資料")]
        public string isQryTmp { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "帳戶簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "系統別")]
        public string system { get; set; }

        [Display(Name = "小系統別")]
        public string source_op { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "處理狀態")]
        public string status { get; set; }

        public string status_desc { get; set; }

        [Display(Name = "簡述說明(4個中文字)")]
        public string filler_10 { get; set; }

        [Display(Name = "備用欄位-14 (AML註記) ")]
        public string filler_14 { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "再給付方式")]
        public string re_paid_type { get; set; }

        [Display(Name = "區部")]
        public string area { get; set; }

        [Display(Name = "來源")]
        public string srce_from { get; set; }

        [Display(Name = "類別")]
        public string source_kind { get; set; }

        [Display(Name = "給付編號")]
        public string pay_no { get; set; }

        [Display(Name = "給付序號")]
        public string pay_seq { get; set; }

        [Display(Name = "付款申請編號")]
        public string re_paid_no { get; set; }

        [Display(Name = "付款申請序號")]
        public string re_paid_seq { get; set; }

        [Display(Name = "再給付票號")]
        public string re_paid_check_no { get; set; }

        [Display(Name = "轉繳之保單系統別")]
        public string rt_system { get; set; }

        [Display(Name = "轉繳之保單號碼")]
        public string rt_policy_no { get; set; }

        [Display(Name = "轉繳之保單序號")]
        public string rt_policy_seq { get; set; }

        [Display(Name = "轉繳之保單重覆碼")]
        public string rt_id_dup { get; set; }

        [Display(Name = "再給付匯款銀行代碼")]
        public string re_bank_code { get; set; }

        [Display(Name = "再給付匯款分行代碼")]
        public string re_sub_bank { get; set; }

        [Display(Name = "再給付匯款帳號")]
        public string re_bank_account { get; set; }

        [Display(Name = "再給付對象 ID")]
        public string re_paid_id { get; set; }

        [Display(Name = "給付帳務日")]
        public string re_paid_date { get; set; }

        [Display(Name = "再給付日期")]
        public string re_paid_date_n { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        public OAP0002Model() {

            tempId = "";
            aplyNo = "";
            apprStat = "";
            execActionDesc = "";
            isQryTmp = "0";
            exec_action = "";
            create_id = "";
            create_dt = "";

            check_no = "";
            check_acct_short = "";
            system = "";
            source_op = "";
            check_date = "";
            check_amt = "";
            status = "";
            status_desc = "";
            filler_10 = "";
            filler_14 = "";
            paid_id = "";
            paid_name = "";
            re_paid_type = "";
            area = "";
            srce_from = "";
            source_kind = "";
            pay_no = "";
            pay_seq = "";
            re_paid_no = "";
            re_paid_seq = "";
            re_paid_check_no = "";
            rt_system = "";
            rt_policy_no = "";
            rt_policy_seq = "";
            rt_id_dup = "";
            re_bank_code = "";
            re_sub_bank = "";
            re_bank_account = "";
            re_paid_id = "";
            re_paid_date = "";
            re_paid_date_n = "";
            o_paid_cd = "";
        }

    }
}