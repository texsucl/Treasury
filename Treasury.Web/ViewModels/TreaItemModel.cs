using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
	public class TreaItemModel
	{
		/// <summary>
		/// 作業類型
		/// </summary>
		[Description("作業類型")]
		public string ITEM_DESC { get; set; }
	}
}