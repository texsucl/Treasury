//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FRT.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FRT_FBO
    {
        public string remit_transfer_no { get; set; }
        public System.DateTime remit_date { get; set; }
        public System.TimeSpan remit_time { get; set; }
        public Nullable<int> remit_count { get; set; }
        public Nullable<decimal> remit_amt { get; set; }
        public string recovery_flag { get; set; }
        public string transfer_id { get; set; }
    }
}