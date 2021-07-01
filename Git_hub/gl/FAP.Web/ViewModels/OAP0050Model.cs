using Org.BouncyCastle.Crypto.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0050Model
    {
        [Display(Name = "在檔位置")]
        public string linePos { get; set; }

        public string chkResult { get; set; }

        public bool bPyck { get; set; }

        public string msg { get; set; }


        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼／匯費序號")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "案號人員別")]
        public string member_id { get; set; }

        [Display(Name = "案號")]
        public string change_id { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "回存金額")]
        public string main_amt { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "帳務日期")]
        public string re_paid_date { get; set; }

        [Display(Name = "再給付方式")]
        public string re_paid_type { get; set; }

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "証明文件(一)")]
        public string cert_doc_1 { get; set; }

        [Display(Name = "証明文件(二)")]
        public string cert_doc_2 { get; set; }

        [Display(Name = "証明文件(三)")]
        public string cert_doc_3 { get; set; }

        [Display(Name = "証明文件(四)")]
        public string cert_doc_4 { get; set; }

        [Display(Name = "証明文件(五)")]
        public string cert_doc_5 { get; set; }

        [Display(Name = "執行日期(一)")]
        public string exec_date_1 { get; set; }

        [Display(Name = "執行日期(二)")]
        public string exec_date_2 { get; set; }

        [Display(Name = "執行日期(三)")]
        public string exec_date_3 { get; set; }

        [Display(Name = "執行日期(四)")]
        public string exec_date_4 { get; set; }

        [Display(Name = "執行日期(五)")]
        public string exec_date_5 { get; set; }

        [Display(Name = "執行日期")]
        public string exec_date { get; set; }

        [Display(Name = "踐行程序(一)")]
        public string practice_1 { get; set; }

        [Display(Name = "踐行程序(二)")]
        public string practice_2 { get; set; }

        [Display(Name = "踐行程序(三)")]
        public string practice_3 { get; set; }

        [Display(Name = "踐行程序(四)")]
        public string practice_4 { get; set; }

        [Display(Name = "踐行程序(五)")]
        public string practice_5 { get; set; }

        [Display(Name = "過程說明")]
        public string proc_desc { get; set; }

        [Display(Name = "匯入說明")]
        public string imp_desc { get; set; }

        [Display(Name = "結案編號")]
        public string closed_no { get; set; }

        [Display(Name = "結案日期")]
        public string closed_date { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }
        public string update_name { get; set; }

        [Display(Name = "異動日期time")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public DateTime approve_datetime { get; set; }

        [Display(Name = "再給付日期")]
        public string re_paid_date_n { get; set; }

        public int cnt { get; set; }

        [Display(Name = "給付對象姓名_應付票據檔")]
        public string paid_name_pyck { get; set; }

        [Display(Name = "支票金額_應付票據檔")]
        public string check_amt_pyck { get; set; }

        [Display(Name = "支票到期日_應付票據檔")] 
        public string check_date_pyck { get; set; }



        public OAP0050Model() {
            
            linePos = "";
            chkResult = "";
            bPyck = true;
            msg = "";

            aply_no = "";
            system = "";
            check_no = "";
            check_acct_short = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            member_id = "";
            change_id = "";
            paid_id = "";
            paid_name = "";
            main_amt = "";
            check_amt = "";
            check_date = "";
            o_paid_cd = "";
            re_paid_date = "";
            re_paid_type = "";
            fsc_range = "";
            status = "";
            level_1 = "";
            level_2 = "";
            cert_doc_1 = "";
            cert_doc_2 = "";
            cert_doc_3 = "";
            cert_doc_4 = "";
            cert_doc_5 = "";
            exec_date_1 = "";
            exec_date_2 = "";
            exec_date_3 = "";
            exec_date_4 = "";
            exec_date_5 = "";
            exec_date = "";
            practice_1 = "";
            practice_2 = "";
            practice_3 = "";
            practice_4 = "";
            practice_5 = "";
            proc_desc = "";
            imp_desc = "";
            closed_no = "";
            closed_date = "";
            appr_stat = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            //approve_datetime = "";
            re_paid_date_n = "";

            cnt = 0;

            paid_name_pyck = "";
            check_amt_pyck = "";
            check_date_pyck = "";
        }

    }
}