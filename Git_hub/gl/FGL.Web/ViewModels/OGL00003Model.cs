
using System.ComponentModel.DataAnnotations;


namespace FGL.Web.ViewModels
{
    public class OGL00003Model
    {
        public string tempId { get; set; }

        [Display(Name = "杳詢暫存檔")]
        public string isQryTmp { get; set; }

        [Display(Name = "覆核註記")]
        public string apprMk { get; set; }
        public string apprMkDesc { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }


        [Display(Name = "險種類別")]
        public string productType { get; set; }
        public string productTypeDesc { get; set; }

        [Display(Name = "外幣註記")]
        public string fuMk { get; set; }
        public string fuMkDesc { get; set; }

        [Display(Name = "合約分類")]
        public string itemCon { get; set; }
        public string itemConDesc { get; set; }

        [Display(Name = "裁量參與特性")]
        public string discPartFeat { get; set; }

        [Display(Name = "保險商品編號版本")]
        public string prodNoVer { get; set; }
        public string prodNoVerDesc { get; set; }

        [Display(Name = "保險商品編號")]
        public string prodNo { get; set; }

        [Display(Name = "進件系統別")]
        public string sysType { get; set; }
        public string sysTypeDesc { get; set; }

        [Display(Name = "險種主險別")]
        public string itemMainType { get; set; }
        public string itemMainTypeDesc { get; set; }

        [Display(Name = "投資型商品類型")]
        public string investType { get; set; }
        public string investTypeDesc { get; set; }

        [Display(Name = "保險期間")]
        public string insTerm { get; set; }
        public string insTermDesc { get; set; }

        [Display(Name = "業務性質")]
        public string busiType { get; set; }
        public string busiTypeDesc { get; set; }

        [Display(Name = "佣金/承攬費")]
        public string comType { get; set; }
        public string comTypeDesc { get; set; }

        [Display(Name = "可展期定期保險")]
        public string extSchedulType { get; set; }

        [Display(Name = "繳費方式僅限躉繳")]
        public string pakindDmopType { get; set; }

        [Display(Name = "保費費用")]
        public string lodprmType { get; set; }

        public string lodprmTypeDesc { get; set; }

        [Display(Name = "健康管理保險商品")]
        public string healthMgrType { get; set; }

        public string healthMgrTypeDesc { get; set; }

        [Display(Name = "COI")]
        public string coiType { get; set; }

        public string coiTypeDesc { get; set; }



        [Display(Name = "商品名稱")]
        public string itemName { get; set; }

        [Display(Name = "商品簡稱")]
        public string itemNameShrt { get; set; }    //add by daiyu 20191129

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


        [Display(Name = "正式生效註記")]
        public string effMk { get; set; }

        [Display(Name = "商品資訊異動人員")]
        public string prodUpId { get; set; }

        [Display(Name = "商品資訊異動時間")]
        public string prodUpDt { get; set; }

        [Display(Name = "商品資訊覆核人員")]
        public string prodApprId { get; set; }

        [Display(Name = "商品資訊覆核時間")]
        public string prodApprDt { get; set; }

        [Display(Name = "投資資訊異動人員")]
        public string investUpId { get; set; }
        [Display(Name = "投資資訊異動時間")]
        public string investUpDt { get; set; }
        [Display(Name = "投資資訊覆核人員")]
        public string investApprId { get; set; }
        [Display(Name = "投資資訊覆核時間")]
        public string investApprDt { get; set; }

        [Display(Name = "會計資訊異動人員")]
        public string acctUpId { get; set; }
        [Display(Name = "會計資訊異動時間")]
        public string acctUpDt { get; set; }
        [Display(Name = "會計資訊覆核人員")]
        public string acctApprId { get; set; }
        [Display(Name = "會計資訊覆核時間")]
        public string acctApprDt { get; set; }

        [Display(Name = "申請人")]
        public string createId { get; set; }
        [Display(Name = "申請時間")]
        public string createDt { get; set; }

        public string status { get; set; }

        [Display(Name = "建立來源")]
        public string flag { get; set; }

        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        [Display(Name = "執行功能")]
        public string execActionDesc { get; set; }




        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        public string aplyUId { get; set; }

        public string aplyDt { get; set; }



        public OGL00003Model() {
            tempId = "";
            isQryTmp = "0";
            aplyNo = "";
            aplyUId = "";
            aplyDt = "";

            item = "";
            productType = "";
            fuMk = "";
            itemCon = "";
            discPartFeat = "";
            prodNoVer = "";
            prodNo = "";
            sysType = "";
            itemMainType = "";
            investType = "";
            insTerm = "";
            busiType = "";
            comType = "";
            extSchedulType = "";
            pakindDmopType = "";
            lodprmType = "";
            lodprmType = "";
            healthMgrType = "";
            coiType = "";

            itemName = "";
            itemNameShrt = "";  //add by daiyu 20191129
            itemAcct = "";
            separatAcct = "";
            coiAcct = "";

            effectDate = "";
            effectYY = "";
            effectMM = "";
            effectDD = "";

            effMk = "";
            prodUpId = "";
            prodUpDt = "";
            prodApprId = "";
            prodApprDt = "";
            investUpId = "";
            investUpDt = "";
            investApprId = "";
            investApprDt = "";
            acctUpId = "";
            acctUpDt = "";
            acctApprId = "";
            acctApprDt = "";

            execAction = "";
            execActionDesc = "";

            apprMk = "";
            apprMkDesc = "";
            status = "";
            flag = "";

            productTypeDesc = "";
            fuMkDesc = "";
            itemConDesc = "";
            prodNoVerDesc = "";
            sysTypeDesc = "";
            itemMainTypeDesc = "";
            investTypeDesc = "";
            insTermDesc = "";
            busiTypeDesc = "";
            comTypeDesc = "";
            lodprmTypeDesc = "";
            healthMgrTypeDesc = "";
            coiTypeDesc = "";
        }

    }
}