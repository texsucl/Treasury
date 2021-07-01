using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class TelDispatchRptModel
    {
        [Display(Name = "主檔狀態")]
        public string ppaa_status { get; set; }

        [Display(Name = "派件日")]
        public string dispatch_date { get; set; }

        [Display(Name = "電訪人員ID")]
        public string tel_interview_id { get; set; }

        public string tel_interview_name { get; set; }

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "級距上限")]
        public string range_u { get; set; }

        [Display(Name = "級距下限")]
        public string range_l { get; set; }

        [Display(Name = "回存金額")]
        public Decimal main_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public Decimal check_amt { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        public string o_paid_cd_nm { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public int policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "案號")]
        public string change_id { get; set; }

        [Display(Name = "要保人 ID")]
        public string appl_id { get; set; }

        [Display(Name = "要保人姓名")]
        public string appl_name { get; set; }

        [Display(Name = "要保人生日")]
        public string appl_birth { get; set; }

        [Display(Name = "被保人 ID")]
        public string ins_id { get; set; }

        [Display(Name = "被保人姓名")]
        public string ins_name { get; set; }

        [Display(Name = "被保人生日")]
        public string ins_birth { get; set; }

        [Display(Name = "通路別")]
        public string sysmark { get; set; }

        [Display(Name = "服務人員代碼")]
        public string send_id { get; set; }

        [Display(Name = "服務人員單位")]
        public string send_unit { get; set; }

        [Display(Name = "服務人員姓名")]
        public string send_name { get; set; }

        [Display(Name = "服務人員電話")]
        public string send_tel { get; set; }

        [Display(Name = "保戶電話(手機)")]
        public string policy_mobile { get; set; }

        [Display(Name = "保戶電話(市話)")]
        public string policy_tel { get; set; }

        [Display(Name = "收費地址")]
        public string address { get; set; }

        public long cnt { get; set; }

        public long amt { get; set; }


        [Display(Name = "金額級距")]
        public string amt_range { get; set; }
        public string amt_range_desc { get; set; }


        [Display(Name = "密件戶")]
        public string sec_stat { get; set; }



        public string temp_id { get; set; }

        public string tel_appr_result { get; set; }

        public string sms_status { get; set; }

        public string data_status { get; set; }

        public string dispatch_status { get; set; }

        public string status { get; set; }

        public TelDispatchRptModel() {
            

            temp_id = "";
            ppaa_status = "";
            dispatch_date = "";
            tel_interview_id = "";
            tel_interview_name = "";
            fsc_range = "";
            system = "";
            check_no = "";
            check_acct_short = "";
            range_u = "";
            range_l = "";
            main_amt = 0;
            check_date = "";
            check_amt = 0;
            o_paid_cd = "";
            o_paid_cd_nm = "";
            paid_id = "";
            paid_name = "";
            policy_no = "";
            policy_seq = 0;
            id_dup = "";
            change_id = "";
            appl_id = "";
            appl_name = "";
            appl_birth = "";
            ins_id = "";
            ins_name = "";
            ins_birth = "";
            sysmark = "";
            send_id = "";
            send_unit = "";
            send_name = "";
            send_tel = "";
            policy_mobile = "";
            policy_tel = "";
            address = "";

            tel_appr_result = "";
            sms_status = "";
            dispatch_status = "";

            cnt = 0;
            amt = 0;
            amt_range = "";
            amt_range_desc = "";
            sec_stat = "";

            data_status = "";

            status = "";
        }
    }
}