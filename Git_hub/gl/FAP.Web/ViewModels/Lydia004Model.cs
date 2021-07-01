using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


/// <summary>
/// 參考網址 https://km.fubonlife.com.tw/confluence/pages/viewpage.action?pageId=355045649
/// </summary>
/// 
namespace FAP.Web.ViewModels
{
    public class Lydia004Model
    {

        [Display(Name = "id")]
        public string id { get; set; }

        [Display(Name = "結果代碼")]
        public string returnCode { get; set; }

        [Display(Name = "結果訊息")]
        public string returnMessage { get; set; }

        [Display(Name = "業務人員ID")]
        public string agentId { get; set; }

        [Display(Name = "登錄證號")]
        public string registerNumber { get; set; }

        [Display(Name = "身份別")]
        public string agentType { get; set; }

        [Display(Name = "業務員姓名")]
        public string agentName { get; set; }

        [Display(Name = "單位代號")]
        public string unitCode { get; set; }

        [Display(Name = "手機")]
        public string mobilePhone { get; set; }

        [Display(Name = "電子信箱")]
        public string agentEmail { get; set; }

        [Display(Name = "通訊郵遞區號")]
        public string mailingZipCode { get; set; }

        [Display(Name = "通訊地址")]
        public string mailingAddress { get; set; }

        [Display(Name = "戶籍郵遞區號")]
        public string domiciliaryZipCode { get; set; }

        [Display(Name = "戶籍地址")]
        public string domiciliaryAddress { get; set; }

        [Display(Name = "停招狀態代碼")]
        public string stopCode { get; set; }

        [Display(Name = "停招狀態中文")]
        public string stopCodeName { get; set; }

        [Display(Name = "停招類別代碼")]
        public string stopKind { get; set; }

        [Display(Name = "停招類別中文")]
        public string stopKindName { get; set; }

        [Display(Name = "分行代號")]
        public string branchCode { get; set; }

        [Display(Name = "出生日期")]
        public string birthDate { get; set; }

        [Display(Name = "代收區號")]
        public string collectionCode { get; set; }

        [Display(Name = "星座")]
        public string constellation { get; set; }

        [Display(Name = "生肖")]
        public string zodiac { get; set; }

        [Display(Name = "員工編號")]
        public string agentNumber { get; set; }

        [Display(Name = "營品解任")]
        public string dismissal { get; set; }

        [Display(Name = "營品解任放行")]
        public string dismissalRelease { get; set; }

        [Display(Name = "優良業務人員")]
        public string excellentAgent { get; set; }

        [Display(Name = "登錄日期")]
        public string registerDate { get; set; }

        [Display(Name = "換證日期")]
        public string renewRegisterDate { get; set; }

        [Display(Name = "有效日期")]
        public string effectiveDate { get; set; }

        [Display(Name = "投資型商品登錄日期")]
        public string investmentDate { get; set; }

        [Display(Name = "外幣非投資型登錄日期")]
        public string foreignCurrencyDate { get; set; }

        [Display(Name = "傳統年金通報日期")]
        public string annuityDate { get; set; }

        [Display(Name = "利變年金通報日期")]
        public string interestAnnuityDate { get; set; }

        [Display(Name = "結構債通報日期")]
        public string structuralDebtDate { get; set; }

        [Display(Name = "資料來源")]
        public string sourceFrom { get; set; }

        [Display(Name = "停招起日")]
        public string stopStartDate { get; set; }

        [Display(Name = "停招迄日")]
        public string stopEndDate { get; set; }

        [Display(Name = "優體受訓日期")]
        public string excellentBodyDate { get; set; }

        [Display(Name = "行動投保受訓日期")]
        public string accompanyingInsuredDate { get; set; }


        public Lydia004Model() {
            id = "";
            returnCode = "";
            returnMessage = "";
            agentId = "";
            registerNumber = "";
            agentType = "";
            agentName = "";
            unitCode = "";
            mobilePhone = "";
            agentEmail = "";
            mailingZipCode = "";
            birthDate = "";
            domiciliaryAddress = "";
            mailingZipCode = "";
            mailingAddress = "";
            domiciliaryZipCode = "";
            domiciliaryAddress = "";
            stopCode = "";
            stopCodeName = "";
            stopKind = "";
            stopKindName = "";
            branchCode = "";
            birthDate = "";
            collectionCode = "";
            constellation = "";
            zodiac = "";
            agentNumber = "";
            dismissal = "";
            dismissalRelease = "";
            excellentAgent = "";
            registerDate = "";
            renewRegisterDate = "";
            effectiveDate = "";
            investmentDate = "";
            foreignCurrencyDate = "";
            annuityDate = "";
            interestAnnuityDate = "";
            structuralDebtDate = "";
            sourceFrom = "";
            stopStartDate = "";
            stopEndDate = "";
            excellentBodyDate = "";
            accompanyingInsuredDate = "";
        }
    }
}