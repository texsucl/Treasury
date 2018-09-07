using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class SpecifiedTimeTreasuryApprReasonDetailViewModel
    {
        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM_DESC { get; set; }

        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vITEM_OP_TYPE { get; set; }
    }
}