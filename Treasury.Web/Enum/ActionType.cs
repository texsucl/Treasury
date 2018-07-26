using System;
using System.ComponentModel;

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 動作
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            /// 新增
            /// </summary>
            [Description("新增")]
            Add,

            /// <summary>
            /// 修改
            /// </summary>
            [Description("修改")]
            Edit,

            /// <summary>
            /// 刪除
            /// </summary>
            [Description("刪除")]
            Dele,

            /// <summary>
            /// 檢視
            /// </summary>
            [Description("檢視")]
            View,

            ///// <summary>
            ///// 作廢
            ///// </summary>
            //[Description("作廢")]
            //ObSolete,

            ///// <summary>
            ///// 取消申請
            ///// </summary>
            //[Description("取消申請")]
            //CancelApply,
        }
    }

}