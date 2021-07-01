using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class tblRosterPolicy_DTL
    {
        /// <summary>
        /// 名單序號
        /// </summary>
        [DisplayName("名單序號")]
        public Int64 ListNo { get; set; }

        /// <summary>
        /// 保單號碼
        /// </summary>
        [DisplayName("保單號碼")]
        //[Required(ErrorMessage = "PolicyNo 必填")]
        [MaxLength(15)]
        public string PolicyNo { get; set; } = string.Empty;

        /// <summary>
        /// 重複碼
        /// </summary>
        [DisplayName("重複碼")]
        //[Required(ErrorMessage = "IDDup 必填")]
        [MaxLength(1)]
        public string IDDup { get; set; } = string.Empty;

        /// <summary>
        /// 保單序號
        /// </summary>
        [DisplayName("保單序號")]
        //[Required(ErrorMessage = "PolicySeq 必填")]
        [MaxLength(2)]
        public string PolicySeq { get; set; } = string.Empty;

        /// <summary>
        /// 人員別
        /// </summary>
        [DisplayName("人員別")]
        //[Required(ErrorMessage = "MemberID 必填")]
        [MaxLength(3)]
        public string MemberID { get; set; } = string.Empty;

        /// <summary>
        /// 變更/歸檔代號
        /// </summary>
        [DisplayName("變更/歸檔代號")]
        //[Required(ErrorMessage = "RptReceNo 必填")]
        [MaxLength(20)]
        public string RptReceNo { get; set; } = string.Empty;

        /// <summary>
        /// 繳款人ID
        /// </summary>
        [DisplayName("繳款人ID")]
        [MaxLength(10)]
        public string PAYER_Id { get; set; } 

        /// <summary>
        /// 繳款人姓名
        /// </summary>
        [DisplayName("繳款人姓名")]
        //[Required(ErrorMessage = "PAYER_Name 必填")]
        [MaxLength(202)]
        public string PAYER_Name { get; set; } = string.Empty;

        /// <summary>
        /// 繳款人性別
        /// </summary>
        [DisplayName("繳款人性別")]
        [MaxLength(20)]
        public string PAYER_Moblie { get; set; }

        /// <summary>
        /// 繳款人電話
        /// </summary>
        [DisplayName("繳款人電話")]
        [MaxLength(20)]
        public string PAYER_Tel { get; set; }

        /// <summary>
        /// 繳款人生日
        /// </summary>
        [DisplayName("繳款人生日")]
        [MaxLength(8)]
        public string PAYER_BirthDate { get; set; }

        /// <summary>
        /// 繳款人與要被保人關係
        /// </summary>
        [DisplayName("繳款人與要被保人關係")]
        [MaxLength(20)]
        public string PAYER_Relate { get; set; }

        /// <summary>
        /// 繳款人法代ID
        /// </summary>
        [DisplayName("繳款人法代ID")]
        [MaxLength(10)]
        public string PAYER_LR_Id { get; set; }

        /// <summary>
        /// 繳款人法代姓名
        /// </summary>
        [DisplayName("繳款人法代姓名")]
        //[Required(ErrorMessage = "PAYER_LR_Name 必填")]
        [MaxLength(202)]
        public string PAYER_LR_Name { get; set; } = string.Empty;

        /// <summary>
        /// 繳款人法代生日
        /// </summary>
        [DisplayName("繳款人法代生日")]
        [MaxLength(8)]
        public string PAYER_LR_BirthDate { get; set; }

        /// <summary>
        /// 首次授權銀行
        /// </summary>
        [DisplayName("首次授權銀行")]
        [MaxLength(15)]
        public string FPrem_Auth_BankName { get; set; }

        /// <summary>
        /// 首次授權方式
        /// </summary>
        [DisplayName("首次授權方式")]
        [MaxLength(8)]
        public string FPrem_Auth_Way { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(15)]
        public string LetterNM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(5)]
        public string SendType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string SendDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string ReturnDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(20)]
        public string ReturnRS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string RecentPayDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(5)]
        public string Pay_Kind { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string Pay_Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(15)]
        public string Pay_AMT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string RepayDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string RepayType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(15)]
        public string RepayAMT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(12)]
        public string ContractType { get; set; }

        /// <summary>
        /// 要保人Email
        /// </summary>
        [DisplayName("要保人Email")]
        //[Required(ErrorMessage = "CustEmailAddr 必填")]
        [MaxLength(60)]
        public string CustEmailAddr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(10)]
        public string campid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LK_GRACE_DATE { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LK_GEN_DATE { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LK_STOP_DATE { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PAYER_TelO 必填")]
        [MaxLength(20)]
        public string PAYER_TelO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(1)]
        public string Appointment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime CreateTime { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(100)]
        public string PayAmountSourceQ { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(200)]
        public string PayAmountSourceA { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(100)]
        public string InsurancePolicyQ { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(200)]
        public string InsurancePolicyA { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(15)]
        public string MPolicyNo { get; set; }
    }
}