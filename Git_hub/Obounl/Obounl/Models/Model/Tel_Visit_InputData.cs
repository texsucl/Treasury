using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class Tel_Visit_InputData
    {
        /// <summary>
        /// 要保書編號
        /// </summary>
        [Required(ErrorMessage = "要保書編號(CaseNo) 必填")]
        [DisplayName("要保書編號")]
        public string CaseNo { get; set; }

        /// <summary>
        /// 電訪抽件日 要保書上傳日Date Format：yyyy/MM/dd
        /// </summary>
        [Required(ErrorMessage = "電訪抽件日(UploadDate) 必填")]
        //[RegularExpression(@"([12]\d{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01]))", ErrorMessage = "電訪抽件日需符合日期格式yyyy/MM/dd")]
        [DisplayName("電訪抽件日")]
        public string UploadDate { get; set; }

        /// <summary>
        /// 系統別 通路來源代碼 1：業務服展 2：整銷 3：保經代
        /// </summary>
        [Required(ErrorMessage = "系統別(SysID) 必填")]
        [DisplayName("系統別")]
        public string SysID { get; set; }

        /// <summary>
        /// 合約 要保書資料多筆
        /// </summary>
        [DisplayName("合約")]
        public List<Contract> PolicyCases { get; set; } = new List<Contract>();

        /// <summary>
        /// 業務員 業務員資料多筆
        /// </summary>
        [DisplayName("業務員")]
        public List<Agent> Agent { get; set; } = new List<Agent>();
    }

    /// <summary>
    /// 合約內容
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// 商品名稱 ProductName
        /// </summary>
        [Required(ErrorMessage = "商品名稱(Context_C) 必填")]
        [DisplayName("商品名稱")]
        public string Context_C { get; set; }

        /// <summary>
        /// 險種 險種代碼
        /// </summary>
        [Required(ErrorMessage = "險種(PlanDesc) 必填")]
        [DisplayName("險種")]
        public string PlanDesc { get; set; }

        /// <summary>
        /// 年金險/壽險 A：意外險、D：死亡險、E：養老險、H：健康險、P：生存險、Y：年金險
        /// </summary>
        [Required(ErrorMessage = "年金險/壽險(YAmtType) 必填")]
        [DisplayName("年金險/壽險")]
        public string YAmtType { get; set; }

        /// <summary>
        /// 投資型/非投資型 U：投資型、其它則為非投資型
        /// </summary>
        [Required(ErrorMessage = "投資型/非投資型(UAmtType) 必填")]
        [DisplayName("投資型/非投資型")]
        public string UAmtType { get; set; }

        /// <summary>
        /// 繳費年期 年期
        /// </summary>
        [Required(ErrorMessage = "繳費年期(PolicyYear) 必填")]
        [DisplayName("繳費年期")]
        public int PolicyYear { get; set; }

        /// <summary>
        /// 幣別 幣別代碼
        /// </summary>
        [Required(ErrorMessage = "幣別(PolicyCurType) 必填")]
        [DisplayName("幣別")]
        public string PolicyCurType { get; set; }

        /// <summary>
        /// 繳別 繳別代碼
        /// </summary>
        [Required(ErrorMessage = "繳別(ProductPay) 必填")]
        [DisplayName("繳別")]
        public string ProductPay { get; set; }

        /// <summary>
        /// 總保額
        /// </summary>
        [Required(ErrorMessage = "總保額(PolicyAmt) 必填")]
        [DisplayName("總保額")]
        public string PolicyAmt { get; set; }

        /// <summary>
        /// 應繳保費
        /// </summary>
        [Required(ErrorMessage = "應繳保費(PolicyCost) 必填")]
        [DisplayName("應繳保費")]
        public string PolicyCost { get; set; }

        /// <summary>
        /// 繳款方式
        /// </summary>
        [Required(ErrorMessage = "繳款方式(PayMethod) 必填")]
        [DisplayName("繳款方式")]
        public string PayMethod { get; set; }

        /// <summary> 
        /// 契約始期 Date Format：yyyy/MM/dd
        /// </summary>
        [RegularExpression(@"([12]\d{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01]))", ErrorMessage = "契約始期需符合日期格式yyyy/MM/dd")]
        [DisplayName("契約始期")]
        public string SubmitDate { get; set; }

        /// <summary>
        /// 電訪特殊註記 業報書上業務員的特殊註記
        /// </summary>
        [DisplayName("電訪特殊註記")]
        public string ContactMemo { get; set; }

        /// <summary>
        /// 電訪條件 1(貸款/借款: True, 解約:  False), 2(貸款/借款: False, 解約: True), 3(貸款/借款: True, 解約: True)
        /// </summary>
        [Required(ErrorMessage = "電訪條件(CampCondition) 必填")]
        [DisplayName("電訪條件")]
        public string CampCondition { get; set; }

        /// <summary>
        /// 貸款/借款 
        /// </summary>
        [Required(ErrorMessage = "貸款/借款(CustLevelLoan) 必填")]
        [DisplayName("貸款/借款")]
        public bool CustLevelLoan { get; set; }

        /// <summary>
        /// 解約
        /// </summary>
        [Required(ErrorMessage = "解約(CustLevelChangeItem) 必填")]
        [DisplayName("解約")]
        public bool CustLevelChangeItem { get; set; }

        /// <summary>
        /// 保費來源題目
        /// </summary>
        [Required(ErrorMessage = "保費來源題目(PayAmountSourceQ) 必填")]
        [DisplayName("保費來源題目")]
        public string PayAmountSourceQ { get; set; }

        /// <summary>
        /// 保費來源回應
        /// </summary>
        [Required(ErrorMessage = "保費來源回應(PayAmountSourceA) 必填")]
        [DisplayName("保費來源回應")]
        public string PayAmountSourceA { get; set; }

        /// <summary>
        /// 投保目的題目
        /// </summary>
        [Required(ErrorMessage = "投保目的題目(InsurancePolicyQ) 必填")]
        [DisplayName("投保目的題目")]
        public string InsurancePolicyQ { get; set; }

        /// <summary>
        /// 投保目的回應
        /// </summary>
        [Required(ErrorMessage = "投保目的回應(InsurancePolicyA) 必填")]
        [DisplayName("投保目的回應")]
        public string InsurancePolicyA { get; set; }

        /// <summary>
        /// 客戶資料
        /// </summary>
        [DisplayName("客戶資料")]
        public List<Customer> Customers { get; set; } = new List<Customer>();

        /// <summary>
        /// 受益人 
        /// </summary>
        [DisplayName("受益人")]
        public List<PolicyBenfit> PolicyBenfit { get; set; }
    }

    /// <summary>
    /// 客戶資料
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// 客戶ID
        /// </summary>
        [Required(ErrorMessage = "客戶ID(CustID) 必填")]
        [DisplayName("客戶ID")]
        public string CustID { get; set; }

        /// <summary>
        /// 客戶姓名
        /// </summary>
        [Required(ErrorMessage = "客戶姓名(CustName) 必填")]
        [DisplayName("客戶姓名")]
        public string CustName { get; set; }

        /// <summary>
        /// 客戶姓別
        /// </summary>
        [Required(ErrorMessage = "客戶姓別(CustGen) 必填")]
        [DisplayName("客戶姓別")]
        public string CustGen { get; set; }

        /// <summary>
        /// 客戶生日 Date Format：yyyy/MM/dd
        /// </summary>
        //[Required(ErrorMessage = "客戶生日(CustBirthday) 必填")]
        //[RegularExpression(@"([12]\d{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01]))", ErrorMessage = "客戶生日需符合日期格式yyyy/MM/dd")]
        [DisplayName("客戶生日")]
        public string CustBirthday { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [DisplayName("Email")]
        public string CustEmail { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [DisplayName("戶籍地址")]
        public string CustAddr { get; set; }

        /// <summary>
        /// 收費地址
        /// </summary>
        [DisplayName("收費地址")]
        public string CustRecvAddr { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [DisplayName("行動電話")]
        public string CustMoblie { get; set; }

        /// <summary>
        /// 戶籍電話(住家電話)
        /// </summary>
        [DisplayName("戶籍電話(住家電話)")]
        public string CustTEL { get; set; }

        /// <summary>
        /// 戶籍電話(住家電話)分機
        /// </summary>
        [DisplayName("戶籍電話(住家電話)分機")]
        public string CustTELext { get; set; }

        /// <summary>
        /// 收費電話
        /// </summary>
        [DisplayName("收費電話")]
        public string CustRecvTEL { get; set; }

        /// <summary>
        /// 收費電話分機
        /// </summary>
        [DisplayName("收費電話分機")]
        public string CustRecvTELext { get; set; }

        /// <summary>
        /// 角色代碼  CodeDesc角色名稱（要保人、被保險人、實際繳款人、要保人法代、被保險人法代、實際繳款人法代）
        /// </summary>
        [Required(ErrorMessage = "角色代碼(CustContGen) 必填")]
        [DisplayName("角色代碼")]
        public string CustContGen { get; set; }

        /// <summary>
        /// 受訪者與要被保人關係 MenuText關係名稱
        /// </summary>
        [DisplayName("受訪者與要被保人關係")]
        [MaxLength(2)]
        public string CustMenuValue { get; set; }

        /// <summary>
        /// 電訪時間(起)
        /// </summary>
        //[Required(ErrorMessage = "電訪時間_起(CustVisitTimeS) 必填")]
        [DisplayName("電訪時間(起)")]
        public int CustVisitTimeS { get; set; }

        /// <summary>
        /// 電訪時間(迄)
        /// </summary>
        //[Required(ErrorMessage = "電訪時間_迄(CustVisitTimeE) 必填")]
        [DisplayName("電訪時間(迄)")]
        public int CustVisitTimeE { get; set; }

        /// <summary>
        /// 有善服務
        /// </summary>
        [DisplayName("有善服務")]
        public bool CustIsfriendly { get; set; }
    }

    /// <summary>
    /// 業務員內容
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// 業務員ID
        /// </summary>
        [Required(ErrorMessage = "業務員ID(AgentID) 必填")]
        [DisplayName("業務員ID")]
        public string AgentID { get; set; }

        /// <summary>
        /// 業務員姓名
        /// </summary>
        [Required(ErrorMessage = "業務員姓名(AgentName) 必填")]
        [DisplayName("業務員姓名")]
        public string AgentName { get; set; }

        /// <summary>
        /// 單位代號
        /// </summary>
        [Required(ErrorMessage = "單位代號(AgentUnitCode) 必填")]
        [DisplayName("單位代號")]
        public string AgentUnitCode { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Required(ErrorMessage = "單位名稱(AgentUnitName) 必填")]
        [DisplayName("單位名稱")]
        public string AgentUnitName { get; set; }

        /// <summary>
        /// 業務員O365的Email
        /// </summary>
        [DisplayName("業務員O365的Email")]
        public string AgentEmail { get; set; }
    }

    /// <summary>
    /// 受益人資料
    /// </summary>
    public class PolicyBenfit
    {
        /// <summary>
        /// 受益人順位
        /// </summary>
        [Required(ErrorMessage = "受益人順位(TrustSeq) 必填")]
        [DisplayName("受益人順位")]
        public string TrustSeq { get; set; }

        /// <summary>
        /// 受益人姓名
        /// </summary>
        [Required(ErrorMessage = "受益人姓名(TrustName) 必填")]
        [DisplayName("受益人姓名")]
        public string TrustName { get; set; }

        /// <summary>
        /// 受益人比例
        /// </summary>
        [Required(ErrorMessage = "受益人比例(TrustRate) 必填")]
        [DisplayName("受益人比例")]
        public string TrustRate { get; set; }

        /// <summary>
        /// 受益人關係
        /// </summary>
        [Required(ErrorMessage = "受益人關係(TrustRelate) 必填")]
        [DisplayName("受益人關係")]
        public string TrustRelate { get; set; }

        /// <summary>
        /// 受益人類型
        /// </summary>
        [Required(ErrorMessage = "受益人類型(TrustType) 必填")]
        [DisplayName("受益人類型")]
        public string TrustType { get; set; }
    }
}