using System.ComponentModel;

namespace FGL.Web.Enum
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

        }
    }
}