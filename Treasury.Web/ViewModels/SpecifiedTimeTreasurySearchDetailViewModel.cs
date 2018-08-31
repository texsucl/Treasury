using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
	/// <summary>
	/// 指定時間開庫顯示區塊ViewModel
	/// </summary>
	public class SpecifiedTimeTreasurySearchDetailViewModel
	{
		/// <summary>
		/// 項次
		/// </summary>
		[Description("項次")]
		public string vINDEX { get; set; }

		/// <summary>
		/// 申請日期
		/// </summary>
		[Description("申請日期")]
		public string vCREATE_DT { get; set; }

		/// <summary>
		/// 金庫登記簿單號
		/// </summary>
		[Description("金庫登記簿單號")]
		public string vTREA_REGISTER_ID { get; set; }

		/// <summary>
		/// 表單狀態代碼
		/// </summary>
		[Description("表單狀態代碼")]
		public string vAPLY_STATUS_ID { get; set; }

		/// <summary>
		/// 表單狀態
		/// </summary>
		[Description("表單狀態")]
		public string vAPLY_STATUS { get; set; }

		/// <summary>
		/// 開庫原因
		/// </summary>
		[Description("開庫原因")]
		public string vOPEN_TREA_REASON { get; set; }

		/// <summary>
		/// 開庫時間
		/// </summary>
		[Description("預計開庫時間")]
		public string vOPEN_TREA_TIME { get; set; }

		/// <summary>
		/// 備註
		/// </summary>
		[Description("備註")]
		public string vMEMO { get; set; }

		/// <summary>
		/// 開庫日期
		/// </summary>
		[Description("開庫日期")]
		public string vOPEN_TREA_DATE { get; set; }

		/// <summary>
		/// 系統區間(起)
		/// </summary>
		[Description("系統區間(起)")]
		public string vEXEC_TIME_B { get; set; }

		/// <summary>
		/// 系統區間(迄)
		/// </summary>
		[Description("系統區間(迄)")]
		public string vEXEC_TIME_E { get; set; }

		/// <summary>
		/// 開庫原因代碼
		/// </summary>
		[Description("開庫原因代碼")]
		public string vOPEN_TREA_REASON_ID { get; set; }

		/// <summary>
		/// 新增人員
		/// </summary>
		[Description("新增人員")]
		public string vCREATE_UID { get; set; }

		/// <summary>
		/// 覆核人員
		/// </summary>
		[Description("覆核人員")]
		public string vAPPR_UID { get; set; }
		
	}
}