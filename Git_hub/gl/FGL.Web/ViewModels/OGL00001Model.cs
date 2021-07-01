using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL00001Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aplyNo { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        [Display(Name = "查詢暫存資料")]
        public string isQryTmp { get; set; }

        [Display(Name = "險種類別")]
        public string productType { get; set; }

        [Display(Name = "險種類別")]
        public string productTypeDesc { get; set; }

        [Display(Name = "外幣註記")]
        public string fuMk { get; set; }

        [Display(Name = "合約分類")]
        public string itemCon { get; set; }

        [Display(Name = "裁量參與特性")]
        public string discPartFeat { get; set; }

        [Display(Name = "是否適用 IFRS4")]
        public string isIfrs4 { get; set; }

        [Display(Name = "投資型商品類型維護")]
        public string investTypeMk { get; set; }

        [Display(Name = "投資型商品交易維護")]
        public string investTradMk { get; set; }

        [Display(Name = "保費費用維護")]
        public string lodprmMk { get; set; }

        [Display(Name = "可展期定期保險維護")]
        public string extSchedulMk { get; set; }

        [Display(Name = "繳費方式僅限躉繳維護")]
        public string pakindDmopMk { get; set; }


        [Display(Name = "帳務類別")]
        public string acctType { get; set; }

        [Display(Name = "帳務類別")]
        public string acctTypeDesc { get; set; }

        [Display(Name = "科目代號")]
        public string acctItem { get; set; }

        [Display(Name = "帳本")]
        public string corpNo { get; set; }

        [Display(Name = "限躉繳註記")]
        public string dmopMk { get; set; }

        [Display(Name = "COI維護")]
        public string coiType { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string lastUpdateUid { get; set; }

        public string lastUpdateUName { get; set; }

        [Display(Name = "異動日期")]
        public string lastUpdateDT { get; set; }


        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        [Display(Name = "執行功能")]
        public string execActionDesc { get; set; }


        [Display(Name = "健康管理保險商品")]
        public string healthMgrMk { get; set; }


        [Display(Name = "投資型商品類型維護")]
        public string investTypeMkF { get; set; }

        [Display(Name = "投資型商品交易維護")]
        public string investTradMkF { get; set; }

        [Display(Name = "保費費用維護")]
        public string lodprmMkF { get; set; }

        [Display(Name = "可展期定期保險維護")]
        public string extSchedulMkF { get; set; }

        [Display(Name = "繳費方式僅限躉繳維護")]
        public string pakindDmopMkF { get; set; }

        [Display(Name = "限躉繳註記")]
        public string dmopMkF { get; set; }

        [Display(Name = "COI維護")]
        public string coiTypeF { get; set; }

        public string updateMk { get; set; }


        public OGL00001Model() {
            tempId = "";
            aplyNo = "";
            apprStat = "";
            isQryTmp = "0";

            productType = "";
            productTypeDesc = "";
            fuMk = "";
            itemCon = "";
            discPartFeat = "";
            isIfrs4 = "";
            investTypeMk = "";
            investTradMk = "";
            lodprmMk = "";
            extSchedulMk = "";
            pakindDmopMk = "";
            coiType = "";

            healthMgrMk = "";

            acctType = "";
            acctTypeDesc = "";
            acctItem = "";
            corpNo = "";
            dmopMk = "";
            dataStatus = "";
            dataStatusDesc = "";
            lastUpdateUid = "";
            lastUpdateUName = "";
            lastUpdateDT = "";
            execAction = "";
            execActionDesc = "";


            investTypeMkF = "";
            investTradMkF = "";
            lodprmMkF = "";
            extSchedulMkF = "";
            pakindDmopMkF = "";
            dmopMkF = "";
            coiTypeF = "";

            updateMk = "";
        }

    }
}