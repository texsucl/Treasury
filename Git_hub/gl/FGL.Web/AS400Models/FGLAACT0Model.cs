using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLAACT0Model
    {
        [Display(Name = "帳本代號")]
        public string corpNo { get; set; }

        [Display(Name = "科目代號")]
        public string actNum { get; set; }

        [Display(Name = "科目類別")]
        public string actType { get; set; }

        [Display(Name = "科目屬性")]
        public string actAtrb { get; set; }

        [Display(Name = "借貸別")]
        public string dbSign { get; set; }

        [Display(Name = "科目中文名稱")]
        public string actName { get; set; }

        [Display(Name = "科目中文簡稱")]
        public string actShort { get; set; }

        [Display(Name = "科目英文名稱")]
        public string actEngl { get; set; }

        [Display(Name = "預記註記")]
        public string preRemark { get; set; }

        [Display(Name = "銷帳註記")]
        public string rmRemark { get; set; }

        [Display(Name = "單位註記")]
        public string unitRemrk { get; set; }

        [Display(Name = "險種註記")]
        public string itemRemrk { get; set; }

        [Display(Name = "外幣金額註記")]
        public string famtRemrk { get; set; }

        [Display(Name = "基金單位數註記")]
        public string fnoRemrk { get; set; }

        [Display(Name = "分紅註記")]
        public string bonuRemrk { get; set; }

        [Display(Name = "性質")]
        public string atrCode { get; set; }

        [Display(Name = "借貸別")]
        public string cord { get; set; }

        [Display(Name = "貸方科目")]
        public string crCode { get; set; }

        [Display(Name = "契約始期–年")]
        public string effYy { get; set; }

        [Display(Name = "契約始期–月")]
        public string effMm { get; set; }

        [Display(Name = "契約始期–日")]
        public string effDd { get; set; }

        [Display(Name = "失效日期–年")]
        public string invalidYy { get; set; }

        [Display(Name = "失效日期–月")]
        public string invalidMm { get; set; }

        [Display(Name = "失效日期–日")]
        public string invalidDd { get; set; }

        [Display(Name = "異動人員代號")]
        public string updId { get; set; }

        [Display(Name = "異動日期–年")]
        public string updYy { get; set; }

        [Display(Name = "異動日期–月")]
        public string updMm { get; set; }

        [Display(Name = "異動日期–日")]
        public string updDd { get; set; }

        [Display(Name = "備用")]
        public string filler1 { get; set; }

        [Display(Name = "備用")]
        public string filler8 { get; set; }

        [Display(Name = "備用")]
        public string filler10 { get; set; }


        public FGLAACT0Model()
        {
            corpNo = "";
            actNum = "";
            actType = "";
            actAtrb = "";
            dbSign = "";
            actName = "";
            actShort = "";
            actEngl = "";
            preRemark = "";
            rmRemark = "";
            unitRemrk = "";
            itemRemrk = "";
            famtRemrk = "";
            fnoRemrk = "";
            bonuRemrk = "";
            atrCode = "";
            cord = "";
            crCode = "";
            effYy = "0";
            effMm = "0";
            effDd = "0";
            invalidYy = "0";
            invalidMm = "0";
            invalidDd = "0";
            updId = "";
            updYy = "0";
            updMm = "0";
            updDd = "0";
            filler1 = "";
            filler8 = "";
            filler10 = "";
        }
    }
}