using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class CampMPool
    {
        /// <summary>
        /// 要保書編號
        /// </summary>
        [DisplayName("要保書編號")]
        public string CaseNo { get; set; }

        /// <summary>
        /// 主約順序
        /// </summary>
        [DisplayName("主約順序")]
        public string CaseSeq { get; set; }

        /// <summary>
        /// 電訪抽件日
        /// </summary>
        [DisplayName("電訪抽件日")]
        public DateTime InsDT { get; set; }

        /// <summary>
        /// Input 資料
        /// </summary>
        [DisplayName("Input 資料")]
        public string MData { get; set; }
    }
}