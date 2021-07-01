using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class LyodsAmlFSKWSModel
    {

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "查詢者 ID")]
        public string query_id { get; set; }

        [Display(Name = "查詢單位")]
        public string unit { get; set; }

        [Display(Name = "查詢來源")]
        public string source_id { get; set; }

        [Display(Name = "客戶編號")]
        public string cin_no { get; set; }

        [Display(Name = "要保人客戶編號")]
        public string appl_id { get; set; }

        [Display(Name = "rtn_code")]
        public string rtn_code { get; set; }

        [Display(Name = "DecState 疑似黑名單分類")]
        public string is_san { get; set; }

        [Display(Name = "狀態")]
        public string status { get; set; }

        [Display(Name = "承辦人姓名")]
        public string query_name { get; set; }

        [Display(Name = "科別代碼")]
        public string dpt_cd { get; set; }

        [Display(Name = "科別名稱")]
        public string dpt_name { get; set; }

        [Display(Name = "callType")]
        public string callType { get; set; }

        public string calc { get; set; }

        public string errMsg { get; set; }

        public string name { get; set; }

        public string enName { get; set; }

        public string hasPPAS { get; set; }

        public LyodsAmlFSKWSModel() {
            paid_id = "";
            paid_name = "";
            o_paid_cd = "";
            query_id = "";
            unit = "";
            source_id = "";
            cin_no = "";
            appl_id = "";
            rtn_code = "";
            is_san = "";
            status = "";
            query_name = "";
            dpt_cd = "";
            dpt_name = "";
            callType = "";
            calc = "";
            errMsg = "";

            name = "";
            enName = "";
            hasPPAS = "";
        }

    }
}