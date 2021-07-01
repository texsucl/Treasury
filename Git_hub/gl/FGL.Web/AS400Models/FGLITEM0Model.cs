using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLITEM0Model
    {
        [Display(Name = "帳本代號")]
        public string corpNo { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }

        [Display(Name = "繳費年期")]
        public string premYear { get; set; }

        [Display(Name = "保費收入首年首期")]
        public string acctCode { get; set; }

        [Display(Name = "保費收入首年續期")]
        public string acctCodef { get; set; }

        [Display(Name = "保費收入續期")]
        public string acctCoder { get; set; }

        [Display(Name = "增額保費首年首期")]
        public string acctCodes { get; set; }

        [Display(Name = "增額保費首年續期")]
        public string acctCodeg { get; set; }

        [Display(Name = "增額保費續年度")]
        public string acctCodet { get; set; }

        [Display(Name = "ＣＯＩ首年首期")]
        public string coiCode { get; set; }

        [Display(Name = "ＣＯＩ首年續期")]
        public string coiCodef { get; set; }

        [Display(Name = "ＣＯＩ續年")]
        public string coiCoder { get; set; }

        [Display(Name = "佣金支出首年首期")]
        public string acctCodei { get; set; }

        [Display(Name = "佣金支出首年續期")]
        public string comuCodef { get; set; }

        [Display(Name = "佣金支出續期")]
        public string comuCoder { get; set; }

        [Display(Name = "應付佣金首年首期")]
        public string acctCodeo { get; set; }

        [Display(Name = "應付佣金首年續期")]
        public string comuPayf { get; set; }

        [Display(Name = "應付佣金續期")]
        public string comuPayr { get; set; }

        [Display(Name = "增額前置初年首次")]
        public string acct4570 { get; set; }

        [Display(Name = "增額前置初年續次")]
        public string acct4571 { get; set; }

        [Display(Name = "增額前置續年度")]
        public string acct4572 { get; set; }

        [Display(Name = "保費費用初年首次")]
        public string acct4573 { get; set; }

        [Display(Name = "保費費用初年續次")]
        public string acct4574 { get; set; }

        [Display(Name = "保費費用續年度")]
        public string acct4575 { get; set; }

        [Display(Name = "異動人員代號")]
        public string updId { get; set; }

        [Display(Name = "異動日期–年")]
        public string updYy { get; set; }

        [Display(Name = "異動日期–月")]
        public string updMm { get; set; }

        [Display(Name = "異動日期–日")]
        public string updDd { get; set; }


        [Display(Name = "保留欄位")]
        public string filler10 { get; set; }

        [Display(Name = "保留欄位1")]
        public string filler101 { get; set; }

        [Display(Name = "保留欄位2")]
        public string filler102 { get; set; }

        [Display(Name = "保留欄位3")]
        public string filler103 { get; set; }

        [Display(Name = "保留欄位4")]
        public string filler104 { get; set; }

        [Display(Name = "保留欄位5")]
        public string filler105 { get; set; }

        [Display(Name = "保留欄位6")]
        public string filler106 { get; set; }

        [Display(Name = "保留欄位7")]
        public string filler107 { get; set; }

        [Display(Name = "保留欄位8")]
        public string filler108 { get; set; }


        public FGLITEM0Model()
        {
            corpNo = "";
            item = "";
            premYear = "";
            acctCode = "";
            acctCodef = "";
            acctCoder = "";
            acctCodes = "";
            acctCodeg = "";
            acctCodet = "";
            coiCode = "";
            coiCodef = "";
            coiCoder = "";
            acctCodei = "";
            comuCodef = "";
            comuCoder = "";
            acctCodeo = "";
            comuPayf = "";
            comuPayr = "";
            acct4570 = "";
            acct4571 = "";
            acct4572 = "";
            acct4573 = "";
            acct4574 = "";
            acct4575 = "";
            updId = "";
            updYy = "";
            updMm = "";
            updDd = "";
            filler10 = "";
            filler101 = "";
            filler102 = "";
            filler103 = "";
            filler104 = "";
            filler105 = "";
            filler106 = "";
            filler107 = "";
            filler108 = "";

        }
    }
}