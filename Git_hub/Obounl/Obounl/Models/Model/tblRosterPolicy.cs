using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class tblRosterPolicy
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
        //[Required(ErrorMessage = "CampID 必填")]
        [MaxLength(10)]
        public string CampID { get; set; } = string.Empty;

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
        //[Required(ErrorMessage = "PolicyNo 必填")]
        [MaxLength(15)]
        public string PolicyNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "IDDup 必填")]
        [MaxLength(1)]
        public string IDDup { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicySeq 必填")]
        [MaxLength(2)]
        public string PolicySeq { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "MemberID 必填")]
        [MaxLength(3)]
        public string MemberID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RptReceNo 必填")]
        [MaxLength(20)]
        public string RptReceNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustLevel 必填")]
        [MaxLength(50)]
        public string CustLevel { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyGoods 必填")]
        [MaxLength(10)]
        public string PolicyGoods { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustID 必填")]
        [MaxLength(11)]
        public string CustID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPID 必填")]
        [MaxLength(15)]
        public string APPID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPName 必填")]
        [MaxLength(202)]
        public string APPName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPBirth 必填")]
        [MaxLength(10)]
        public string APPBirth { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime AssignSchDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "VisitDate 必填")]
        [MaxLength(2)]
        public string VisitDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "VisitTimeScale 必填")]
        [MaxLength(1)]
        public string VisitTimeScale { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "VisitTime 必填")]
        [MaxLength(5)]
        public string VisitTime { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RecvZip 必填")]
        [MaxLength(6)]
        public string RecvZip { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RecvAddr 必填")]
        [MaxLength(300)]
        public string RecvAddr { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RecvTEL 必填")]
        [MaxLength(30)]
        public string RecvTEL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime ApplyDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChangeCloseDate 必填")]
        [MaxLength(10)]
        public string ChangeCloseDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FileNO 必填")]
        [MaxLength(25)]
        public string FileNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChangeItem 必填")]
        [MaxLength(20)]
        public string ChangeItem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentID 必填")]
        [MaxLength(10)]
        public string sAgentID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentName 必填")]
        [MaxLength(202)]
        public string sAgentName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string sAgentPhone { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentMobile 必填")]
        [MaxLength(20)]
        public string sAgentMobile { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentEmail 必填")]
        [MaxLength(60)]
        public string sAgentEmail { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentUnitName 必填")]
        [MaxLength(50)]
        public string sAgentUnitName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentUnitNo 必填")]
        [MaxLength(10)]
        public string sAgentUnitNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AccAgent 必填")]
        [MaxLength(50)]
        public string AccAgent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ProceUnitNo 必填")]
        [MaxLength(10)]
        public string ProceUnitNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ProceID 必填")]
        [MaxLength(15)]
        public string ProceID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ProceName 必填")]
        [MaxLength(202)]
        public string ProceName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CCustID 必填")]
        [MaxLength(15)]
        public string CCustID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CCustName 必填")]
        [MaxLength(202)]
        public string CCustName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CZipCode 必填")]
        [MaxLength(5)]
        public string CZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CADDR 必填")]
        [MaxLength(300)]
        public string CADDR { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CTEL 必填")]
        [MaxLength(30)]
        public string CTEL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AcceptCNTime 必填")]
        [MaxLength(20)]
        public string AcceptCNTime { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AcceptCNTEL 必填")]
        [MaxLength(30)]
        public string AcceptCNTEL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ContactMemo 必填")]
        [MaxLength(500)]
        public string ContactMemo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AreaCode 必填")]
        [MaxLength(2)]
        public string AreaCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CardType 必填")]
        [MaxLength(2)]
        public string CardType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustAddr 必填")]
        [MaxLength(300)]
        public string CustAddr { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "Payment 必填")]
        [MaxLength(10)]
        public string Payment { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "DCRNO 必填")]
        [MaxLength(20)]
        public string DCRNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CTNO 必填")]
        [MaxLength(20)]
        public string CTNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "BookValue 必填")]
        [MaxLength(20)]
        public string BookValue { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SharesValue 必填")]
        [MaxLength(20)]
        public string SharesValue { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChkNO 必填")]
        [MaxLength(20)]
        public string ChkNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChkAccount 必填")]
        [MaxLength(20)]
        public string ChkAccount { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PlanDesc 必填")]
        [MaxLength(20)]
        public string PlanDesc { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AssetsNM 必填")]
        [MaxLength(20)]
        public string AssetsNM { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayItem 必填")]
        [MaxLength(10)]
        public string PayItem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string OTel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "VerifyAgent 必填")]
        [MaxLength(202)]
        public string VerifyAgent { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CNTFaxNO 必填")]
        [MaxLength(25)]
        public string CNTFaxNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CNTOP 必填")]
        [MaxLength(202)]
        public string CNTOP { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CNTDP 必填")]
        [MaxLength(50)]
        public string CNTDP { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CNTFL 必填")]
        [MaxLength(50)]
        public string CNTFL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskControlFactor 必填")]
        [MaxLength(500)]
        public string RiskControlFactor { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SubmitDate 必填")]
        [MaxLength(10)]
        public string SubmitDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyNum 必填")]
        [MaxLength(10)]
        public string PolicyNum { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyYear 必填")]
        [MaxLength(10)]
        public string PolicyYear { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyAmt 必填")]
        [MaxLength(20)]
        public string PolicyAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SourceCamp 必填")]
        [MaxLength(50)]
        public string SourceCamp { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyCurType 必填")]
        [MaxLength(10)]
        public string PolicyCurType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(12)]
        public string PayType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(12)]
        public string PayMethod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyCost 必填")]
        [MaxLength(20)]
        public string PolicyCost { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfSigned 必填")]
        [MaxLength(20)]
        public string AmtOfSigned { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfAppro 必填")]
        [MaxLength(20)]
        public string AmtOfAppro { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfNTChk 必填")]
        [MaxLength(20)]
        public string AmtOfNTChk { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfAct 必填")]
        [MaxLength(20)]
        public string AmtOfAct { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfEQNT 必填")]
        [MaxLength(22)]
        public string AmtOfEQNT { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CntOfSelf 必填")]
        [MaxLength(10)]
        public string CntOfSelf { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AmtOfKind 必填")]
        [MaxLength(10)]
        public string AmtOfKind { get; set; } = string.Empty;

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
        public DateTime CreateDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime BringDate { get; set; } = DateTime.Parse("1900/01/01");

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
        //[Required(ErrorMessage = "LAgentID 必填")]
        [MaxLength(15)]
        public string LAgentID { get; set; } = string.Empty;

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
        public DateTime LAGContactDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime LAGFollowDate { get; set; } = DateTime.Parse("1900/01/01");

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
        public int ProcDay { get; set; }

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
        //[Required(ErrorMessage = "CMoblie 必填")]
        [MaxLength(25)]
        public string CMoblie { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FZipCode 必填")]
        [MaxLength(5)]
        public string FZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FADDR 必填")]
        [MaxLength(300)]
        public string FADDR { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FTEL 必填")]
        [MaxLength(30)]
        public string FTEL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FMoblie 必填")]
        [MaxLength(30)]
        public string FMoblie { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FRecvZip 必填")]
        [MaxLength(6)]
        public string FRecvZip { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FRecvAddr 必填")]
        [MaxLength(300)]
        public string FRecvAddr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FRecvTEL 必填")]
        [MaxLength(30)]
        public string FRecvTEL { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPhone1 必填")]
        [MaxLength(25)]
        public string RiskDispPhone1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPNo1 必填")]
        [MaxLength(15)]
        public string RiskDispPPNo1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSO1 必填")]
        [MaxLength(2)]
        public string RiskDispPPSO1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPIDDUP1 必填")]
        [MaxLength(1)]
        public string RiskDispPIDDUP1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSREC1 必填")]
        [MaxLength(8)]
        public string RiskDispPPSREC1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMob1 必填")]
        [MaxLength(25)]
        public string RiskDispMob1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPNo1 必填")]
        [MaxLength(15)]
        public string RiskDispMPNo1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSO1 必填")]
        [MaxLength(2)]
        public string RiskDispMPSO1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMIDDUP1 必填")]
        [MaxLength(1)]
        public string RiskDispMIDDUP1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSREC1 必填")]
        [MaxLength(8)]
        public string RiskDispMPSREC1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPhone2 必填")]
        [MaxLength(25)]
        public string RiskDispPhone2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPNo2 必填")]
        [MaxLength(15)]
        public string RiskDispPPNo2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSO2 必填")]
        [MaxLength(2)]
        public string RiskDispPPSO2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPIDDUP2 必填")]
        [MaxLength(1)]
        public string RiskDispPIDDUP2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSREC2 必填")]
        [MaxLength(8)]
        public string RiskDispPPSREC2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMob2 必填")]
        [MaxLength(25)]
        public string RiskDispMob2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPNo2 必填")]
        [MaxLength(15)]
        public string RiskDispMPNo2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSO2 必填")]
        [MaxLength(2)]
        public string RiskDispMPSO2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMIDDUP2 必填")]
        [MaxLength(1)]
        public string RiskDispMIDDUP2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSREC2 必填")]
        [MaxLength(8)]
        public string RiskDispMPSREC2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPhone3 必填")]
        [MaxLength(25)]
        public string RiskDispPhone3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPNo3 必填")]
        [MaxLength(15)]
        public string RiskDispPPNo3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSO3 必填")]
        [MaxLength(2)]
        public string RiskDispPPSO3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPIDDUP3 必填")]
        [MaxLength(1)]
        public string RiskDispPIDDUP3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispPPSREC3 必填")]
        [MaxLength(8)]
        public string RiskDispPPSREC3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMob3 必填")]
        [MaxLength(25)]
        public string RiskDispMob3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPNo3 必填")]
        [MaxLength(15)]
        public string RiskDispMPNo3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSO3 必填")]
        [MaxLength(2)]
        public string RiskDispMPSO3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMIDDUP3 必填")]
        [MaxLength(1)]
        public string RiskDispMIDDUP3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RiskDispMPSREC3 必填")]
        [MaxLength(8)]
        public string RiskDispMPSREC3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentID2 必填")]
        [MaxLength(10)]
        public string sAgentID2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentName2 必填")]
        [MaxLength(202)]
        public string sAgentName2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicySdType 必填")]
        [MaxLength(25)]
        public string PolicySdType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "BnkCampCond 必填")]
        [MaxLength(30)]
        public string BnkCampCond { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SecretGrp 必填")]
        [MaxLength(1)]
        public string SecretGrp { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CONTEXT_C 必填")]
        [MaxLength(500)]
        public string CONTEXT_C { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChgDateReason 必填")]
        [MaxLength(100)]
        public string ChgDateReason { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        public DateTime OldAssignSchDate { get; set; } = DateTime.Parse("1900/01/01");

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = " 必填")]
        [MaxLength(10)]
        public string LegalPID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LegalPName 必填")]
        [MaxLength(202)]
        public string LegalPName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LegalPRel 必填")]
        [MaxLength(15)]
        public string LegalPRel { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentUnitNo2 必填")]
        [MaxLength(10)]
        public string sAgentUnitNo2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "sAgentUnitName2 必填")]
        [MaxLength(50)]
        public string sAgentUnitName2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ContactSF 必填")]
        [MaxLength(1)]

        public string ContactSF { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "SourceType 必填")]
        [MaxLength(1)]
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayType_Aft 必填")]
        [MaxLength(12)]
        public string PayType_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayTypeN_Org 必填")]
        [MaxLength(12)]
        public string PayTypeN_Org { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayTypeN_Aft 必填")]
        [MaxLength(12)]
        public string PayTypeN_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayMethod_Aft 必填")]
        [MaxLength(12)]
        public string PayMethod_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayMethodN_Org 必填")]
        [MaxLength(12)]
        public string PayMethodN_Org { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayMethodN_Aft 必填")]
        [MaxLength(12)]
        public string PayMethodN_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChangeItem_rest 必填")]
        [MaxLength(1000)]
        public string ChangeItem_rest { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AppGen 必填")]
        [MaxLength(10)]
        public string AppGen { get; set; } = string.Empty;

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
        //[Required(ErrorMessage = "CustBirth_Aft 必填")]
        [MaxLength(10)]
        public string CustBirth_Aft { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FRecvTEL2 必填")]
        [MaxLength(30)]
        public string FRecvTEL2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RecvTEL2 必填")]
        [MaxLength(30)]
        public string RecvTEL2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "FTEL2 必填")]
        [MaxLength(30)]
        public string FTEL2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CTEL2 必填")]
        [MaxLength(30)]
        public string CTEL2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = " 必填")]
        [MaxLength(10)]
        public string AutoBatchNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TotallPolicyCost 必填")]
        [MaxLength(20)]
        public string TotallPolicyCost { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TrustInfo 必填")]
        [MaxLength(10)]
        public string TrustInfo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PrePolicyCost 必填")]
        [MaxLength(20)]
        public string PrePolicyCost { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PolicyCostDate 必填")]
        [MaxLength(10)]
        public string PolicyCostDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "AuthFlag 必填")]
        [MaxLength(1)]
        public string AuthFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "OCustFingPrt 必填")]
        [MaxLength(1)]
        public string OCustFingPrt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NCustFingPrt 必填")]
        [MaxLength(1)]
        public string NCustFingPrt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPFingPrt 必填")]
        [MaxLength(1)]
        public string APPFingPrt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustSERR 必填")]
        [MaxLength(1)]
        public string CustSERR { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPSERR 必填")]
        [MaxLength(1)]
        public string APPSERR { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPZipCode 必填")]
        [MaxLength(5)]
        public string APPZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(300)]
        public string APPAddr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPTel1 必填")]
        [MaxLength(30)]
        public string APPTel1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPTel2 必填")]
        [MaxLength(30)]
        public string APPTel2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string APPMobile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "APPEmailAddr 必填")]
        [MaxLength(50)]
        public string APPEmailAddr { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NAPPZipCode 必填")]
        [MaxLength(5)]
        public string NAPPZipCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(300)]
        public string NAPPAddr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string NAPPTel1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string NAPPTel2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(30)]
        public string NAPPMobile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NAPPEmailAddr 必填")]
        [MaxLength(50)]
        public string NAPPEmailAddr { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "OriCustName 必填")]
        [MaxLength(202)]
        public string OriCustName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NTrustInfo 必填")]
        [MaxLength(10)]
        public string NTrustInfo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LawRefer 必填")]
        [MaxLength(150)]
        public string LawRefer { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAEffet_Date 必填")]
        [MaxLength(10)]
        public string TAEffet_Date { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAEffet_Time 必填")]
        [MaxLength(6)]
        public string TAEffet_Time { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAEnd_Date 必填")]
        [MaxLength(10)]
        public string TAEnd_Date { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TATravelDays 必填")]
        [MaxLength(10)]
        public string TATravelDays { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TATravelPlac1 必填")]
        [MaxLength(20)]
        public string TATravelPlac1 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TATravelPlac2 必填")]
        [MaxLength(20)]
        public string TATravelPlac2 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TATravelPlac3 必填")]
        [MaxLength(20)]
        public string TATravelPlac3 { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAAddUnit 必填")]
        [MaxLength(25)]
        public string TAAddUnit { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAMrUnit 必填")]
        [MaxLength(25)]
        public string TAMrUnit { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAOhsUnit 必填")]
        [MaxLength(25)]
        public string TAOhsUnit { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAOhs1Unit 必填")]
        [MaxLength(25)]
        public string TAOhs1Unit { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAAddPrem 必填")]
        [MaxLength(20)]
        public string TAAddPrem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAMrPrem 必填")]
        [MaxLength(20)]
        public string TAMrPrem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAOhsPrem 必填")]
        [MaxLength(20)]
        public string TAOhsPrem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TAOhs1Prem 必填")]
        [MaxLength(20)]
        public string TAOhs1Prem { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TABankCode 必填")]
        [MaxLength(12)]
        public string TABankCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TACardNo 必填")]
        [MaxLength(20)]
        public string TACardNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TACardEndYM 必填")]
        [MaxLength(6)]
        public string TACardEndYM { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CPlanDesc 必填")]
        [MaxLength(20)]
        public string CPlanDesc { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CPolicyYear 必填")]
        [MaxLength(10)]
        public string CPolicyYear { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CCONTEXT_C 必填")]
        [MaxLength(500)]
        public string CCONTEXT_C { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CAPPID 必填")]
        [MaxLength(15)]
        public string CAPPID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CAPPNAME 必填")]
        [MaxLength(202)]
        public string CAPPNAME { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CAPPBirth 必填")]
        [MaxLength(10)]
        public string CAPPBirth { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EffcntFlag 必填")]
        [MaxLength(1)]
        public string EffcntFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RemitTotal 必填")]
        [MaxLength(15)]
        public string RemitTotal { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RemitAmt 必填")]
        [MaxLength(15)]
        public string RemitAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayMode 必填")]
        [MaxLength(2)]
        public string PayMode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PayRelation 必填")]
        [MaxLength(2)]
        public string PayRelation { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RemitDate 必填")]
        [MaxLength(10)]
        public string RemitDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkFrom 必填")]
        [MaxLength(2)]
        public string RchkFrom { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkPNO 必填")]
        [MaxLength(15)]
        public string RchkPNO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkIDDUP 必填")]
        [MaxLength(1)]
        public string RchkIDDUP { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkPSO 必填")]
        [MaxLength(5)]
        public string RchkPSO { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkCustName 必填")]
        [MaxLength(202)]
        public string RchkCustName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkAppName 必填")]
        [MaxLength(202)]
        public string RchkAppName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkAppMop 必填")]
        [MaxLength(10)]
        public string RchkAppMop { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkAppMopName 必填")]
        [MaxLength(12)]
        public string RchkAppMopName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkAppPayK 必填")]
        [MaxLength(10)]
        public string RchkAppPayK { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RchkAppPayKName 必填")]
        [MaxLength(12)]
        public string RchkAppPayKName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RepyPType 必填")]
        [MaxLength(1)]
        public string RepyPType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RepyChgAmt 必填")]
        [MaxLength(20)]
        public string RepyChgAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RepyNotyAmt 必填")]
        [MaxLength(20)]
        public string RepyNotyAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RepyTolPayAmt 必填")]
        [MaxLength(20)]
        public string RepyTolPayAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "RepyInfo 必填")]
        [MaxLength(1)]
        public string RepyInfo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyClmArea 必填")]
        [MaxLength(10)]
        public string EasyClmArea { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyRevID 必填")]
        [MaxLength(15)]
        public string EasyRevID { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyRevName 必填")]
        [MaxLength(202)]
        public string EasyRevName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyPayType 必填")]
        [MaxLength(10)]
        public string EasyPayType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyPrClmMemo 必填")]
        [MaxLength(200)]
        public string EasyPrClmMemo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyPrClmAmt 必填")]
        [MaxLength(20)]
        public string EasyPrClmAmt { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyClmillDate 必填")]
        [MaxLength(10)]
        public string EasyClmillDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyClmAcpDate 必填")]
        [MaxLength(10)]
        public string EasyClmAcpDate { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyClmTime 必填")]
        [MaxLength(20)]
        public string EasyClmTime { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyBankName 必填")]
        [MaxLength(30)]
        public string EasyBankName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyBankCode 必填")]
        [MaxLength(15)]
        public string EasyBankCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "EasyBankNo 必填")]
        [MaxLength(20)]
        public string EasyBankNo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NBSysType 必填")]
        [MaxLength(1)]
        public string NBSysType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NBPayAmtperY 必填")]
        [MaxLength(15)]
        public string NBPayAmtperY { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "PSChgType 必填")]
        [MaxLength(1)]
        public string PSChgType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustTrustPayFlag 必填")]
        [MaxLength(1)]
        public string CustTrustPayFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CustTrustPayInfo 必填")]
        [MaxLength(10)]
        public string CustTrustPayInfo { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "ChangeItemR_Code 必填")]
        [MaxLength(500)]
        public string ChangeItemR_Code { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "NBCPType 必填")]
        [MaxLength(30)]
        public string NBCPType { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "IGTFlag 必填")]
        [MaxLength(1)]
        public string IGTFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "TRSFlag 必填")]
        [MaxLength(1)]
        public string TRSFlag { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "CoverState 必填")]
        [MaxLength(1)]
        public string CoverState { get; set; } = string.Empty;

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
        //[Required(ErrorMessage = "TrustFlagYN 必填")]
        [MaxLength(1)]
        public string TrustFlagYN { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "HearingYN 必填")]
        [MaxLength(1)]
        public string HearingYN { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        //[Required(ErrorMessage = "LanguageYN 必填")]
        [MaxLength(1)]
        public string LanguageYN { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(1)]
        public string YAmtType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(1)]
        public string LegalCustodian { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("")]
        [MaxLength(20)]
        public string RITDID { get; set; }

        /// <summary>
        /// 通用欄位
        /// </summary>
        [DisplayName("通用欄位")]
        [MaxLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// 投資型保單(Y:投資型 / N:非投資型)
        /// </summary>
        [DisplayName("投資型保單(Y:投資型 / N:非投資型)")]
        [MaxLength(20)]
        public string investment { get; set; }
    }
}