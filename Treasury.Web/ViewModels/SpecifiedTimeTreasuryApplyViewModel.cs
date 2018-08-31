using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
	/// <summary>
	/// 金庫物品覆核畫面申請覆核ViewModel
	/// </summary>
	public class SpecifiedTimeTreasuryApplyViewModel
	{
		/// <summary>
		/// 開庫類型
		/// </summary>
		//[Description("開庫類型")]
		//public string vOPEN_TREA_TYPE { get; set; }

		/// <summary>
		/// 入庫日期, 提供給 系統區間(起)、系統區間(迄)、開庫時間使用
		/// </summary>
		[Description("入庫日期")]
		public string vOPEN_TREA_DATE { get; set; }

		/// <summary>
		/// 存入DB時需要: 入庫日期 + 系統區間(起)
		/// </summary>
		[Description("系統區間(起)")]
		public string vEXEC_TIME_B { get; set; }

		/// <summary>
		/// 存入DB時需要: 入庫日期 + 系統區間(迄)
		/// </summary>
		[Description("系統區間(迄)")]
		public string vEXEC_TIME_E { get; set; }

		/// <summary>
		/// 存入DB時需要: 入庫日期 + 開庫時間
		/// </summary>
		[Description("開庫時間")]
		public string vOPEN_TREA_TIME { get; set; }

		/// <summary>
		/// 備註
		/// </summary>
		[Description("備註")]
		public string vMEMO { get; set; }

		/// <summary>
		/// 開庫原因代碼
		/// </summary>
		[Description("開庫原因代碼")]
		public List<string> vOPEN_TREA_REASON_ID { get; set; }
	}
}