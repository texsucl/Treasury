using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0007Model
    {

        [Display(Name = "在檔位置")]
        public string linePos { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "踐行程序")]
        public string practice { get; set; }

        [Display(Name = "証明文件")]
        public string cert_doc { get; set; }

        [Display(Name = "執行日期")]
        public string exec_date { get; set; }

        [Display(Name = "過程說明")]
        public string proc_desc { get; set; }

        [Display(Name = "檢核訊息")]
        public string msg { get; set; }

        public string chkFlag { get; set; }

        public bool chkLevelChg { get; set; }

        [Display(Name = "最後執行日期")]
        public string last_exec_date { get; set; }

        public OAP0007Model() {
            linePos = "";
            paid_id = "";
            check_no = "";
            check_acct_short = "";
            level_1 = "";
            level_2 = "";
            practice = "";
            cert_doc = "";
            exec_date = "";
            proc_desc = "";
            msg = "";
            chkFlag = "";
            chkLevelChg = false;
            last_exec_date = "";
        }

    }
}