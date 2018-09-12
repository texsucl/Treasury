using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 執行排程模式
        /// </summary>
        public enum DailyRoutineType
        {
            /// <summary>
            /// 每日第一次開庫
            /// </summary>
            [Description("每日第一次開庫")]
            Routine_Fisrt,

            /// <summary>
            /// 每日第二次開庫
            /// </summary>
            [Description("每日第二次開庫")]
            Routine_Second,

        }
    }

}