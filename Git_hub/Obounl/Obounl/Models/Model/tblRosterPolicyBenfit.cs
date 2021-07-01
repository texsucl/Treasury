using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class tblRosterPolicyBenfit
    {
        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public Int64 ListNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string CampID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(15)]
        public string PolicyNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(1)]
        public string IDDup { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(2)]
        public string PolicySeq { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(3)]
        public string MemberID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(22)]
        public string RptReceNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(2)]
        public string TrustSeq { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(202)]
        public string TrustName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(12)]
        public string TrustRate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(50)]
        public string TrustRelate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(3)]
        public string TrustType { get; set; } = string.Empty;
    }
}