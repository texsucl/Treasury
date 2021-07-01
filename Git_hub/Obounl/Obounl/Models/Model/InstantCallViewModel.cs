using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class InstantCallViewModel
    {
        public InstantCallViewModel() {
            CallData = new List<InstantCallSubCallViewModel>();
            MemoData = new List<InstantCallSubMemoViewModel>();
        }

        /// <summary>
        /// Uid
        /// </summary>
        [Description("Uid")]
        public string CaseNo { get;  set; }

        /// <summary>
        /// 情境
        /// </summary>
        [Description("情境")]
        public string Situation { get; set; }

        /// <summary>
        /// 資料正確性
        /// </summary>
        [Description("資料正確性")]
        public bool Flag { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        [Description("錯誤訊息")]
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 上方文字
        /// </summary>
        [Description("上方文字")]
        public string TopMemo { get; set; }

        /// <summary>
        /// 文字訊息
        /// </summary>
        [Description("文字訊息")]
        public List<InstantCallSubMemoViewModel> MemoData { get; set; }

        /// <summary>
        /// 電訪對象DB
        /// </summary>
        [Description("電訪對象DB")]
        public List<InstantCallSubCallViewModel> CallData { get; set; }
    }

    public class InstantCallSubMemoViewModel
    {
        /// <summary>
        /// 編號
        /// </summary>
        [Description("編號")]
        public string Num { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        [Description("訊息")]
        public string Memo { get; set; }
    }

    public class InstantCallSubCallViewModel
    {
        /// <summary>
        /// 保戶身份
        /// </summary>
        [Description("保戶身份")]
        public string CustContGen { get; set; }

        /// <summary>
        /// 保戶姓名
        /// </summary>
        [Description("保戶姓名")]
        public string Cust_Name { get; set; }

        /// <summary>
        /// 保戶ID
        /// </summary>
        [Description("保戶ID")]
        public string CustID { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Description("排序")]
        public string Order { get; set; }
    }
}