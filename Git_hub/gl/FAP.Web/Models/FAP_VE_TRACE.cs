//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FAP.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FAP_VE_TRACE
    {
        public string system { get; set; }
        public string check_no { get; set; }
        public string check_acct_short { get; set; }
        public string paid_id { get; set; }
        public string paid_name { get; set; }
        public Nullable<decimal> check_amt { get; set; }
        public Nullable<System.DateTime> check_date { get; set; }
        public Nullable<System.DateTime> re_paid_date { get; set; }
        public string re_paid_type { get; set; }
        public string fsc_range { get; set; }
        public string status { get; set; }
        public string level_1 { get; set; }
        public string level_2 { get; set; }
        public string cert_doc_1 { get; set; }
        public string cert_doc_2 { get; set; }
        public string cert_doc_3 { get; set; }
        public string cert_doc_4 { get; set; }
        public string cert_doc_5 { get; set; }
        public Nullable<System.DateTime> exec_date_1 { get; set; }
        public Nullable<System.DateTime> exec_date_2 { get; set; }
        public Nullable<System.DateTime> exec_date_3 { get; set; }
        public Nullable<System.DateTime> exec_date_4 { get; set; }
        public Nullable<System.DateTime> exec_date_5 { get; set; }
        public string practice_1 { get; set; }
        public string practice_2 { get; set; }
        public string practice_3 { get; set; }
        public string practice_4 { get; set; }
        public string practice_5 { get; set; }
        public string proc_desc { get; set; }
        public string closed_no { get; set; }
        public Nullable<System.DateTime> closed_date { get; set; }
        public string update_id { get; set; }
        public Nullable<System.DateTime> update_datetime { get; set; }
        public string data_status { get; set; }
        public Nullable<System.DateTime> exec_date { get; set; }
        public string paid_code { get; set; }
        public string closed_desc { get; set; }
        public Nullable<System.DateTime> re_paid_date_n { get; set; }
        public string stage_status { get; set; }
        public Nullable<int> as400_send_cnt { get; set; }
    }
}