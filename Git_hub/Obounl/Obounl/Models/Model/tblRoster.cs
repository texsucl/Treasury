using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class tblRoster
    {
        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ListNo 必填")]
        public Int64 ListNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CampID 必填")]
        [MaxLength(10)]
        public string CampID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CampType 必填")]
        [MaxLength(2)]
        public string CampType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public Int64 CustKey { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustID 必填")]
        [MaxLength(20)]
        public string CustID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = " 必填")]
        [MaxLength(202)]
        public string CustName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ContactName 必填")]
        [MaxLength(202)]
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Gender 必填")]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Status 必填")]
        [MaxLength(1)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustType 必填")]
        [MaxLength(10)]
        public string CustType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustBirth 必填")]
        [MaxLength(10)]
        public string CustBirth { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Marry 必填")]
        [MaxLength(1)]
        public string Marry { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EmailAddr 必填")]
        [MaxLength(100)]
        public string EmailAddr { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Email2 必填")]
        [MaxLength(100)]
        public string Email2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ZipCode 必填")]
        [MaxLength(6)]
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(300)]
        public string ContactAddr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelM_Org { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelM_Aft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelO_Org { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelO_Aft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(6)]
        public string TelO_Ext { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelH_Org { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string TelH_Aft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TelH_Ext 必填")]
        [MaxLength(6)]
        public string TelH_Ext { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "OfficeZipCode 必填")]
        [MaxLength(6)]
        public string OfficeZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(300)]
        public string OfficeAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string FaxNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(500)]
        public string CustMemo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime AssignSchDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ListSource 必填")]
        [MaxLength(10)]
        public string ListSource { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SaleAgentID 必填")]
        [MaxLength(10)]
        public string SaleAgentID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Urgent 必填")]
        [MaxLength(1)]
        public string Urgent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Trf_File 必填")]
        [MaxLength(100)]
        public string Trf_File { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "DialTM 必填")]
        [MaxLength(5)]
        public string DialTM { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LCallBackDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime FContactDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LContactDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LTermCode 必填")]
        [MaxLength(10)]
        public string LTermCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LCloseCode 必填")]
        [MaxLength(10)]
        public string LCloseCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LCloseDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SYSID 必填")]
        [MaxLength(10)]
        public string SYSID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LAGTermCode 必填")]
        [MaxLength(10)]
        public string LAGTermCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LAGFollowDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LAGContactDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AGID 必填")]
        [MaxLength(10)]
        public string AGID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AGName 必填")]
        [MaxLength(202)]
        public string AGName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LTarget 必填")]
        [MaxLength(10)]
        public string LTarget { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LAGContent 必填")]
        [MaxLength(2000)]
        public string LAGContent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NullifyReason 必填")]
        [MaxLength(100)]
        public string NullifyReason { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LAgentID 必填")]
        [MaxLength(15)]
        public string LAgentID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LContent 必填")]
        [MaxLength(2000)]
        public string LContent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LTelNo 必填")]
        [MaxLength(20)]
        public string LTelNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime CreateDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "BatchNo 必填")]
        [MaxLength(9)]
        public string BatchNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime AssignDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AgentID 必填")]
        [MaxLength(15)]
        public string AgentID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AgentName 必填")]
        [MaxLength(202)]
        public string AgentName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "GroupID 必填")]
        [MaxLength(10)]
        public string GroupID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "GroupName 必填")]
        [MaxLength(50)]
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "DialFlag 必填")]
        [MaxLength(1)]
        public string DialFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "DataCatchFlag 必填")]
        [MaxLength(1)]
        public string DataCatchFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "DialTMFlag 必填")]
        [MaxLength(1)]
        public string DialTMFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TicketNo 必填")]
        [MaxLength(100)]
        public string TicketNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "isContact 必填")]
        [MaxLength(1)]
        public string isContact { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TranAgent 必填")]
        [MaxLength(15)]
        public string TranAgent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime TranDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime ReclaimDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CampName 必填")]
        [MaxLength(50)]
        public string CampName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TelM_State 必填")]
        [MaxLength(10)]
        public string TelM_State { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TelH_State 必填")]
        [MaxLength(10)]
        public string TelH_State { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TelO_State 必填")]
        [MaxLength(10)]
        public string TelO_State { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "InsertFlag 必填")]
        [MaxLength(1)]
        public string InsertFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime Deadline { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Dept_ID 必填")]
        [MaxLength(10)]
        public string Dept_ID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Dept_Name 必填")]
        [MaxLength(60)]
        public string Dept_Name { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "InsID 必填")]
        [MaxLength(15)]
        public string InsID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime InsDT { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "UpdID 必填")]
        [MaxLength(15)]
        public string UpdID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime UpdDT { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustType_Aft 必填")]
        [MaxLength(10)]
        public string CustType_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustTypeN_Org 必填")]
        [MaxLength(20)]
        public string CustTypeN_Org { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustTypeN_Aft 必填")]
        [MaxLength(20)]
        public string CustTypeN_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AutoBatchNo 必填")]
        [MaxLength(10)]
        public string AutoBatchNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public int QueueTime { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public int AbnCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustGroup 必填")]
        [MaxLength(15)]
        public string CustGroup { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AppFlag 必填")]
        [MaxLength(1)]
        public string AppFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PSContCustID 必填")]
        [MaxLength(20)]
        public string PSContCustID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CCSFlag 必填")]
        [MaxLength(1)]
        public string CCSFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime CCSFDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EVaiFlag 必填")]
        [MaxLength(1)]
        public string EVaiFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AS400Nflg 必填")]
        [MaxLength(1)]
        public string AS400Nflg { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AssiReason 必填")]
        [MaxLength(50)]
        public string AssiReason { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ClsAgentID 必填")]
        [MaxLength(15)]
        public string ClsAgentID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public int CustAge { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public float UseTime { get; set; } 

        /// <summary>
        /// 建議時段1
        /// </summary>
        [DisplayName("建議時段1")]
        [MaxLength(5)]
        public string Suggesttime1 { get; set; }

        /// <summary>
        /// 建議時段2
        /// </summary>
        [DisplayName("建議時段2")]
        [MaxLength(5)]
        public string Suggesttime2 { get; set; }

        /// <summary>
        /// 建議時段3
        /// </summary>
        [DisplayName("建議時段3")]
        [MaxLength(5)]
        public string Suggesttime3 { get; set; }
    }
}