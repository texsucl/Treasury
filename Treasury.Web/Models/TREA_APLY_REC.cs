//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Treasury.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TREA_APLY_REC
    {
        public string APLY_NO { get; set; }
        public string APLY_FROM { get; set; }
        public string TREA_REGISTER_ID { get; set; }
        public string ITEM_ID { get; set; }
        public string ACCESS_TYPE { get; set; }
        public string ACCESS_REASON { get; set; }
        public string APLY_STATUS { get; set; }
        public string ACTUAL_ACCESS_UID { get; set; }
        public string ACTUAL_ACCESS_TYPE { get; set; }
        public Nullable<System.DateTime> EXPECTED_ACCESS_DATE { get; set; }
        public string APLY_UNIT { get; set; }
        public string APLY_UID { get; set; }
        public Nullable<System.DateTime> APLY_DT { get; set; }
        public string APLY_APPR_UID { get; set; }
        public Nullable<System.DateTime> APLY_APPR_DT { get; set; }
        public string APLY_APPR_DESC { get; set; }
        public string CUSTODY_UID { get; set; }
        public Nullable<System.DateTime> CUSTODY_DT { get; set; }
        public string CUSTODY_APPR_UID { get; set; }
        public Nullable<System.DateTime> CUSTODY_APPR_DT { get; set; }
        public string CUSTODY_APPR_DESC { get; set; }
        public string CONFIRM_UID { get; set; }
        public Nullable<System.DateTime> CONFIRM_DT { get; set; }
        public string RTN_DESC { get; set; }
        public string CREATE_UID { get; set; }
        public Nullable<System.DateTime> CREATE_DT { get; set; }
        public string LAST_UPDATE_UID { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE_DT { get; set; }
        public string CREATE_UNIT { get; set; }
    }
}
