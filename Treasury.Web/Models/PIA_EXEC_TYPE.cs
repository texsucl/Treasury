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
    
    public partial class PIA_EXEC_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PIA_EXEC_TYPE()
        {
            this.PIA_LOG_MAIN = new HashSet<PIA_LOG_MAIN>();
        }
    
        public string STATUS_ID { get; set; }
        public string IN_NAME { get; set; }
        public string OUT_NAME { get; set; }
        public string CRT_UID { get; set; }
        public string CRT_NAME { get; set; }
        public Nullable<System.DateTime> CRT_DATE { get; set; }
        public Nullable<System.TimeSpan> CRT_TIME { get; set; }
        public string UPD_UID { get; set; }
        public string UPD_NAME { get; set; }
        public Nullable<System.DateTime> UPD_DATE { get; set; }
        public Nullable<System.TimeSpan> UPD_TIME { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PIA_LOG_MAIN> PIA_LOG_MAIN { get; set; }
    }
}
