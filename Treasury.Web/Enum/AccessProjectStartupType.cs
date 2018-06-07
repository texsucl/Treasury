using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 存取項目啟動方式
        /// </summary>
        public enum AccessProjectStartupType
        {
            /// <summary>
            /// 系統
            /// </summary>
            [Description("系統")]
            S,

            /// <summary>
            /// 人工
            /// </summary>
            [Description("人工")]
            M,

        }
    }

}