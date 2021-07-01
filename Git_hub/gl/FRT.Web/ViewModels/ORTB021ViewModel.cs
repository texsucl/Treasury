using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB021ViewModel
    {
        /// <summary>
        /// 結案日
        /// </summary>
        [Description("結案日")]
        public string CLOSE_DATE { get; set; }

        /// <summary>
        /// 立帳金額
        /// </summary>
        [Description("立帳金額")]
        public string REMIT_AMT { get; set; }

        /// <summary>
        /// 沖銷金額
        /// </summary>
        [Description("沖銷金額")]
        public string WRITE_OFF_AMT { get; set; }

        /// <summary>
        /// 剩餘金額
        /// </summary>
        [Description("剩餘金額")]
        public string REMAIN_AMT { get; set; }

        /// <summary>
        /// 取消金額
        /// </summary>
        [Description("取消金額")]
        public string CANCEL_AMT { get; set; }
    }
}