using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL10182Model
    {
        public string tempId { get; set; }

        [Display(Name = "查詢暫存資料")]
        public string isQryTmp { get; set; }

        [Display(Name = "科目代號")]
        public string smpNum { get; set; }

        [Display(Name = "帳本代號")]
        public string corpNo { get; set; }

        [Display(Name = "險種類別")]
        public string productType { get; set; }

        [Display(Name = "險種類別")]
        public string productTypeDesc { get; set; }

        [Display(Name = "帳務類別")]
        public string acctType { get; set; }

        [Display(Name = "帳務類別")]
        public string acctTypeDesc { get; set; }

        [Display(Name = "SQL會計科目")]
        public string sqlActNum { get; set; }

        [Display(Name = "SQL會計名稱")]
        public string sqlActNm { get; set; }


        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string updateId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "異動日期")]
        public string updateDatetime { get; set; }


        [Display(Name = "執行功能")]
        public string execAction { get; set; }

        [Display(Name = "執行功能")]
        public string execActionDesc { get; set; }

        public string aplyNo { get; set; }
        public string apprId { get; set; }
        public string apprDt { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        public OGL10182Model() {
            tempId = "";
            isQryTmp = "0";

            smpNum = "";
            corpNo = "";
            productType = "";
            productTypeDesc = "";
            acctType = "";
            acctTypeDesc = "";
            sqlActNum = "";
            sqlActNm = "";

            dataStatus = "";
            dataStatusDesc = "";
            updateId = "";
            updateUName = "";
            updateDatetime = "";
            execAction = "";
            execActionDesc = "";

            aplyNo = "";
            apprId = "";
            apprDt = "";
            apprStat = "";
        }

    }
}