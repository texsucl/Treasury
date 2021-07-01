using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL00002Model
    {
        public string tempId { get; set; }
        public string rowType { get; set; }

        [Display(Name = "覆核單號")]
        public string aplyNo { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        [Display(Name = "查詢暫存資料")]
        public string isQryTmp { get; set; }

        [Display(Name = "保險商品編號")]
        public string productNo { get; set; }

        [Display(Name = "A險種細分類")]
        public string tranA { get; set; }

        [Display(Name = "B個人團體別")]
        public string tranB { get; set; }

        [Display(Name = "C契約別")]
        public string tranC { get; set; }

        [Display(Name = "D傳統非傳統")]
        public string tranD { get; set; }

        [Display(Name = "E主附約")]
        public string tranE { get; set; }

        [Display(Name = "F險種類型2")]
        public string tranF { get; set; }

        [Display(Name = "G危險分類")]
        public string tranG { get; set; }

        [Display(Name = "H年歲滿期")]
        public string tranH { get; set; }

        [Display(Name = "I審查/特殊商品/給付別")]
        public string tranI { get; set; }

        [Display(Name = "J速報註記")]
        public string tranJ { get; set; }

        [Display(Name = "K公司別註記")]
        public string tranK { get; set; }


        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string updateId { get; set; }

        public string updateDatetime { get; set; }

        [Display(Name = "異動日期")]
        public string lastUpdateDT { get; set; }


        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        [Display(Name = "執行功能")]
        public string execActionDesc { get; set; }

        public string apprId { get; set; }
        public string apprDt { get; set; }


        public string p01 { get; set; }
        public string p02 { get; set; }
        public string p03 { get; set; }
        public string p04 { get; set; }
        public string p05 { get; set; }
        public string p06 { get; set; }
        public string p07 { get; set; }
        public string p08 { get; set; }
        public string p09 { get; set; }
        public string p10 { get; set; }
        public string p11 { get; set; }
        public string p12 { get; set; }
        public string p13 { get; set; }
        public string p14 { get; set; }
        public string p15 { get; set; }
        public string p16 { get; set; }
        public string p17 { get; set; }
        public string p18 { get; set; }
        public string p19 { get; set; }
        public string p20 { get; set; }
        public string p21 { get; set; }
        public string p22 { get; set; }
        public string p23 { get; set; }
        public string p24 { get; set; }
        public string p25 { get; set; }
        public string p26 { get; set; }
        public string p27 { get; set; }




        public OGL00002Model() {
            tempId = "";
            aplyNo = "";
            apprStat = "";
            isQryTmp = "0";

            productNo = "";
            tranA = "";
            tranB = "";
            tranC = "";
            tranD = "";
            tranE = "";
            tranF = "";
            tranG = "";
            tranH = "";
            tranI = "";
            tranJ = "";
            tranK = "";

            dataStatus = "";
            dataStatusDesc = "";
            updateId = "";
            updateDatetime = "";
            lastUpdateDT = "";
            execAction = "";
            execActionDesc = "";

            apprId = "";
            apprDt = "";

            p01 = "";
            p02 = "";
            p03 = "";
            p04 = "";
            p05 = "";
            p06 = "";
            p07 = "";
            p08 = "";
            p09 = "";
            p10 = "";
            p11 = "";
            p12 = "";
            p13 = "";
            p14 = "";
            p15 = "";
            p16 = "";
            p17 = "";
            p18 = "";
            p19 = "";
            p20 = "";
            p21 = "";
            p22 = "";
            p23 = "";
            p24 = "";
            p25 = "";
            p26 = "";
            p27 = "";
        }

    }
}