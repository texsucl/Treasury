using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class ItemWanpieModel
    {
        [Display(Name = "覆核單號")]
        public String aplyNo { get; set; }

        [Display(Name = "標註")]
        public String flag { get; set; }

        [Display(Name = "公司代碼")]
        public String corpNo { get; set; }

        [Display(Name = "表單編號")]
        public String voucherNo { get; set; }

        [Display(Name = "表單日期")]
        public String voucherDate { get; set; }

        [Display(Name = "生效日")]
        public String startDate { get; set; }

   
        public String entryTime { get; set; }

        [Display(Name = "系統別")]
        public String aSysType { get; set; }

        [Display(Name = "會科後六碼")]
        public String aActnumLastsix { get; set; }

        [Display(Name = "險種")]
        public String aInsurPolicyItem { get; set; }

        [Display(Name = "產品編號")]
        public String aInsurPolicyObjectNo { get; set; }

        [Display(Name = "險種中文名稱")]
        public String aInsurPolicyObjectName { get; set; }

        [Display(Name = "保發險別大分類")]
        public String aInsurPolicyMajorCategories { get; set; }

        [Display(Name = "險種細分類")]
        public String aInsurPolicySubdivisions { get; set; }

        [Display(Name = "保發個人/團體別")]
        public String aInsurPolicyPersonGroup { get; set; }

        [Display(Name = "保發契約別")]
        public String aContractType { get; set; }

        [Display(Name = "保發傳統/非傳統")]
        public String aInsurPolicyTradition { get; set; }

        [Display(Name = "保發業務性質")]
        public String aInsurPolicyBusinessObject { get; set; }

        [Display(Name = "保發主/附約")]
        public String aInsurPolicyMainOrRider { get; set; }

        [Display(Name = "保發保險商品類別1")]
        public String aInsurPolicyObjectType1 { get; set; }

        [Display(Name = "保發保險商品類別2")]
        public String aInsurPolicyObjectType2 { get; set; }

        [Display(Name = "險種主險別")]
        public String aInsurPolicyMainInsurType { get; set; }

        [Display(Name = "保發危險分類")]
        public String aInsurPolicyDangerType { get; set; }

        [Display(Name = "分紅商品")]
        public String aInsurPolicyIsPar { get; set; }

        [Display(Name = "明細備註")]
        public String detailRemark { get; set; }

        [Display(Name = "截止日期")]
        public String endDate { get; set; }

        [Display(Name = "保留欄位(生存滿期金)")]
        public String fieldCha1 { get; set; }

        [Display(Name = "保留欄位")]
        public String fieldCha2 { get; set; }

        [Display(Name = "保留欄位")]
        public String fieldCha3 { get; set; }

        [Display(Name = "保留欄位")]
        public String fieldCha4 { get; set; }

        [Display(Name = "保留欄位")]
        public String fieldCha5 { get; set; }

        [Display(Name = "Index編碼")]
        public String aInsurPolicyIndex { get; set; }

        [Display(Name = "策略型商品")]
        public String aInsurPolicyStrategyObject { get; set; }

        [Display(Name = "速報類別1")]
        public String aInsurPolicyRapidReportCategory1 { get; set; }

        [Display(Name = "投資合約/保險合約")]
        public String aInvestmentInsuranceContract { get; set; }

        [Display(Name = "險種年滿期/歲滿期代號")]
        public String aYmiCode { get; set; }

        [Display(Name = "審查/特殊商品/給付別")]
        public String aExamineSpecialPayment { get; set; }


        public ItemWanpieModel() {
            aplyNo = "";
            flag = "";
            corpNo = "";
            voucherNo = "";
            voucherDate = "";
            startDate = "";
            entryTime = "";
            aSysType = "";
            aActnumLastsix = "";
            aInsurPolicyItem = "";
            aInsurPolicyObjectNo = "";
            aInsurPolicyObjectName = "";
            aInsurPolicyMajorCategories = "";
            aInsurPolicySubdivisions = "";
            aInsurPolicyPersonGroup = "";
            aContractType = "";
            aInsurPolicyTradition = "";
            aInsurPolicyBusinessObject = "";
            aInsurPolicyMainOrRider = "";
            aInsurPolicyObjectType1 = "";
            aInsurPolicyObjectType2 = "";
            aInsurPolicyMainInsurType = "";
            aInsurPolicyDangerType = "";
            aInsurPolicyIsPar = "";
            detailRemark = "";
            endDate = "";
            fieldCha1 = "";
            fieldCha2 = "";
            fieldCha3 = "";
            fieldCha4 = "";
            fieldCha5 = "";
            aInsurPolicyIndex = "";
            aInsurPolicyStrategyObject = "";
            aInsurPolicyRapidReportCategory1 = "";
            aInvestmentInsuranceContract = "";
            aYmiCode = "";
            aExamineSpecialPayment = "";
        }
    }


}