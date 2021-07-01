
using System;
using System.ComponentModel.DataAnnotations;


namespace FGL.Web.ViewModels
{
    public class OGL00003DModel : ICloneable
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aplyNo { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }

        [Display(Name = "險種類別")]
        public string productType { get; set; }

        [Display(Name = "外幣註記")]
        public string fuMk { get; set; }

        [Display(Name = "合約分類")]
        public string itemCon { get; set; }

        [Display(Name = "裁量參與特性")]
        public string discPartFeat { get; set; }

        [Display(Name = "是否適用 IFRS4")]
        public string isIfrs4 { get; set; }

        [Display(Name = "科目代號")]
        public string smpNum { get; set; }

        [Display(Name = "帳務類別")]
        public string acctType { get; set; }

        [Display(Name = "資料類別")]
        public string dataType { get; set; }

        [Display(Name = "標註")]
        public string flag { get; set; }

        [Display(Name = "科目名稱")]
        public string smpName { get; set; }

        [Display(Name = "科目名稱簡稱")]
        public string smpNameShort { get; set; }

        public string smpNameCnt { get; set; }

        [Display(Name = "帳本別")]
        public string corpNo { get; set; }

        [Display(Name = "AS400科目")]
        public string acctNum { get; set; }

        [Display(Name = "SQL科目")]
        public string sqlSmpNum { get; set; }

        [Display(Name = "SQL科目名稱")]
        public string sqlSmpName { get; set; }

        [Display(Name = "生效日")]
        public string effectDate { get; set; }

        [Display(Name = "失效日")]
        public string expireDate { get; set; }

        public string smpNumFrom { get; set; }

        public bool bExist { get; set; }

        public bool bChg { get; set; }


        public object Clone() {
            return this.MemberwiseClone();
        }



        public OGL00003DModel() {
            tempId = "";
            aplyNo = "0";

            item = "";
            productType = "";
            fuMk = "";
            itemCon = "";
            discPartFeat = "";
            isIfrs4 = "";

            smpNum = "";
            acctType = "";
            dataType = "";
            flag = "";

            acctNum = "";
            smpName = "";
            corpNo = "";
            smpNameShort = "";
            sqlSmpNum = "";
            sqlSmpName = "";

            smpNameCnt = "0";
            expireDate = "";
            expireDate = "";

            bExist = false;
            bChg = false;
            smpNumFrom = "";
        }
    }
}