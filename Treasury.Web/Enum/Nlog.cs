using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        public enum Nlog
        {
            /// <summary>
            /// 用於追蹤，可以在程式裡需要追蹤的地方將訊息以Trace傳出。
            /// </summary>
            [Description("追蹤")]
            Trace = 1,

            /// <summary>
            /// 用於開發，於開發時將一些需要特別關注的訊息以Debug傳出。
            /// </summary>
            [Description("開發")]
            Debug = 2,

            /// <summary>
            /// 訊息，記錄不影響系統執行的訊息，通常會記錄登入登出或是資料的建立刪除、傳輸等。
            /// </summary>
            [Description("訊息")]
            Info = 0,

            /// <summary>
            /// 警告，用於需要提示的訊息，例如庫存不足、貨物超賣、餘額即將不足等。
            /// </summary>
            [Description("警告")]
            Warn = 3,

            /// <summary>
            /// 錯誤，記錄系統實行所發生的錯誤，例如資料庫錯誤、遠端連線錯誤、發生例外等。
            /// </summary>
            [Description("錯誤")]
            Error = 4,

            /// <summary>
            /// 致命，用來記錄會讓系統無法執行的錯誤，例如資料庫無法連線、重要資料損毀等。
            /// </summary>
            [Description("致命")]
            Fatal = 5,
        }
    }

}