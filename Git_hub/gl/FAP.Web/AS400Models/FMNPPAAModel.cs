using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class FMNPPAAModel
    {
        //大系統別
        public string system { get; set; }

        //小系統別  
        public string source_op { get; set; }

        //保單號碼  
        public string policy_no { get; set; }

        //保單序號   
        public string policy_seq { get; set; }

        //重覆碼    
        public string id_dup { get; set; }

        //案號人員別  
        public string member_id { get; set; }

        //案號
        public string change_id { get; set; }

        //給付對象 ID 
        public string paid_id { get; set; }

        //給付對象姓名
        public string paid_name { get; set; }

        //要保人 ID 
        public string appl_id { get; set; }

        //要保人姓名 
        public string appl_name { get; set; }

        //被保人 ID  
        public string ins_id { get; set; }

        //被保人姓名   
        public string ins_name { get; set; }

        //作業性質   
        public string pgm_type { get; set; }

        //原給付方式   
        public string o_paid_tp { get; set; }

        //原給付日  
        public string o_paid_dt { get; set; }

        //幣別  
        public string currency { get; set; }

        //回存金額    
        public string main_amt { get; set; }

        //支票金額
        public string check_amt { get; set; }

        //回存日期 
        public string main_date { get; set; }

        //狀態
        public string status { get; set; }

        //支票號碼／匯費序號
        public string check_no { get; set; }

        //支票帳號簡稱
        public string check_acct_short { get; set; }

        //支票到期日
        public string check_date { get; set; }

        //支票狀態
        public string check_stat { get; set; }

        //作廢代碼
        public string delc_code { get; set; }

        //重開票原因 
        public string re_ck_f { get; set; }

        //原給付性質 
        public string o_paid_cd { get; set; }

        //原支票開出日
        public string o_check_dt { get; set; }

        //區部別
        public string area { get; set; }

        //資料來源
        public string srce_from { get; set; }

        //資料類別
        public string source_kind { get; set; }

        //給付編號 
        public string pay_no { get; set; }

        //給付序號
        public string pay_seq { get; set; }

        //給付序次
        public string paid_seq { get; set; }

        //處理序號
        public string pro_no { get; set; }

        //承辦人員 ID  
        public string entry_id { get; set; }

        //承辦人員姓名
        public string entry_name { get; set; }

        //承辦人員單位代碼
        public string enter_ut1 { get; set; }

        //承辦人員單位   
        public string enter_ut2 { get; set; }

        //再給付日期 
        public string re_paid_date { get; set; }

        public string re_paid_date_n { get; set; }

        //再給付方式
        public string re_paid_type { get; set; }

        //再給付對象 ID 
        public string re_paid_id { get; set; }

        //再給付對象  
        public string r_paid_na { get; set; }

        //再給付人員
        public string r_paid_usr { get; set; }

        //付款申請編號 
        public string re_paid_no { get; set; }

        //付款申請序號   
        public string re_paid_seq { get; set; }

        //再給付支票號碼  
        public string re_paid_check_no { get; set; }

        //再給付匯款銀行代碼
        public string re_bank_code { get; set; }

        //再給付匯款分行代碼
        public string re_sub_bank { get; set; }

        //再給付匯款帳號  
        public string re_bank_account { get; set; }

        //再給付 SWIFT CODE 
        public string r_swift_cd { get; set; }

        //轉繳之保單系統別  
        public string rt_system { get; set; }

        //轉繳之保單號碼 
        public string rt_policy_no { get; set; }

        //轉繳之保單序號   
        public string rt_policy_seq { get; set; }

        //轉繳之保單重覆碼  
        public string rt_id_dup { get; set; }

        //異動人員
        public string upd_id { get; set; }

        //異動日期   
        public string upd_date { get; set; }

        //備註 10     
        public string filler_10 { get; set; }

        //備註 11     
        public string filler_11 { get; set; }

        //備註 12     
        public string filler_12 { get; set; }

        //備註 13     
        public string filler_13 { get; set; }

        //備註 14     
        public string filler_14 { get; set; }

        //備註 15     
        public string filler_15 { get; set; }

        //備註 16     
        public string filler_16 { get; set; }

        //備註 17     
        public string filler_17 { get; set; }

        //備註 18     
        public string filler_18 { get; set; }

        //備註 19     
        public string filler_19 { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public FMNPPAAModel()
        {
        }
    }
}