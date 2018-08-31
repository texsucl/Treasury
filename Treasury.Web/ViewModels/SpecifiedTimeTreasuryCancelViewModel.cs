using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
	public class SpecifiedTimeTreasuryCancelViewModel
	{
		/// <summary>
		/// 金庫登記簿單號
		/// </summary>
		[Description("金庫登記簿單號")]
		public string vTREA_REGISTER_ID { get; set; }

		/// <summary>
		/// 新增人員
		/// </summary>
		[Description("新增人員")]
		public string vCREATE_UID { get; set; }

		/// <summary>
		/// 表單狀態代碼
		/// </summary>
		[Description("表單狀態代碼")]
		public string vAPLY_STATUS_ID { get; set; }

		/// <summary>
		/// 覆核人員
		/// </summary>
		[Description("覆核人員")]
		public string vAPPR_UID { get; set; }
	}
}