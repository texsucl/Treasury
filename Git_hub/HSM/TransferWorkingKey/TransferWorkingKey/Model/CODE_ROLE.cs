//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TransferWorkingKey.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class CODE_ROLE
    {
        public string ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public string AUTH_UNIT { get; set; }
        public string FREE_AUTH { get; set; }
        public string IS_DISABLED { get; set; }
        public string MEMO { get; set; }
        public string DATA_STATUS { get; set; }
        public string CREATE_UID { get; set; }
        public Nullable<System.DateTime> CREATE_DT { get; set; }
        public string LAST_UPDATE_UID { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE_DT { get; set; }
        public string APPR_UID { get; set; }
        public Nullable<System.DateTime> APPR_DT { get; set; }
        public string FREEZE_UID { get; set; }
        public Nullable<System.DateTime> FREEZE_DT { get; set; }
    }
}
