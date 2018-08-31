using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.Enum
{
	public partial class Ref
	{
		/// <summary>
		/// 申請狀態
		/// </summary>
		public enum ApplyStatus
		{
			/// <summary>
			/// 表單申請
			/// </summary>
			[Description("表單申請")]
			_1 = 1,

			/// <summary>
			/// 覆核完成
			/// </summary>
			[Description("覆核完成")]
			_2 = 2,

			/// <summary>
			/// 退回
			/// </summary>
			[Description("退回")]
			_3 = 3,

			/// <summary>
			/// 申請人刪除
			/// </summary>
			[Description("申請人刪除")]
			_4 = 4,
		}
	}
}