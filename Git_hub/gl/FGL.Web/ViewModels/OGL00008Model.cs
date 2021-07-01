
using System.ComponentModel.DataAnnotations;


namespace FGL.Web.ViewModels
{
    public class OGL00008Model
    {
        public string tempId { get; set; }

        [Display(Name = "杳詢暫存檔")]
        public string isQryTmp { get; set; }

        [Display(Name = "資料來源")]
        public string srceFrom { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }


        [Display(Name = "險種類別")]
        public string productType { get; set; }
        public string productTypeDesc { get; set; }

        [Display(Name = "外幣註記")]
        public string fuMk { get; set; }
        public string fuMkDesc { get; set; }

        [Display(Name = "進件系統別")]
        public string sysType { get; set; }
        public string sysTypeDesc { get; set; }

        [Display(Name = "商品名稱")]
        public string itemName { get; set; }

        [Display(Name = "險種科目")]
        public string itemAcct { get; set; }

        [Display(Name = "分離帳科目")]
        public string separatAcct { get; set; }

        [Display(Name = "COI科目")]
        public string coiAcct { get; set; }

        [Display(Name = "生效日期")]
        public string effectDate { get; set; }

        public string effectYY { get; set; }
        public string effectMM { get; set; }
        public string effectDD { get; set; }

        [Display(Name = "覆核註記")]
        public string apprMk { get; set; }
        public string apprMkDesc { get; set; }

        [Display(Name = "會科編碼類別")]
        public string rule56 { get; set; }
        public string rule56Desc { get; set; }

        public OGL00008Model() {
            tempId = "";
            isQryTmp = "0";
            srceFrom = "";

            item = "";
            productType = "";
            fuMk = "";

            itemName = "";
            itemAcct = "";
            separatAcct = "";
            coiAcct = "";

            effectDate = "";
            effectYY = "";
            effectMM = "";
            effectDD = "";

            apprMk = "";
            apprMkDesc = "";
            productTypeDesc = "";
            fuMkDesc = "";
            sysTypeDesc = "";
            rule56 = "";
            rule56Desc = "";

        }

    }
}