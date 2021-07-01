using System.ComponentModel;

namespace FRT.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 報表動作
        /// </summary>
        public enum ReportType
        {
            /// <summary>
            /// 批次系統寄信
            /// </summary>
            [Description("批次")]
            S,

            /// <summary>
            /// 線上執行報表
            /// </summary>
            [Description("線上")]
            R,
        }
    }
}