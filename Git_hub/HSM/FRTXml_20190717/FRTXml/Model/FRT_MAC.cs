//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FRTXml.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class FRT_MAC
    {
        public int Id { get; set; }
        public string WorkingKeyLabel { get; set; }
        public string Data { get; set; }
        public string IV { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string MAC { get; set; }
        public System.DateTime CreateDate { get; set; }
        public System.TimeSpan CreateTime { get; set; }
    }
}
