using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0042Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_status { get; set; }

        public string execActionDesc { get; set; }

        [Display(Name = "申請人")]
        public string create_id { get; set; }

        [Display(Name = "申請時間")]
        public string create_dt { get; set; }

        [Display(Name = "設定項目")]
        public string type { get; set; }

        public string type_desc { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }

        [Display(Name = "計算條件")]
        public string rpt_cnt_tp { get; set; }

        [Display(Name = "歸戶金額")]
        public string stat_amt { get; set; }
        public string stat_amt_b { get; set; }
        public string stat_amt_e { get; set; }

        [Display(Name = "尚未派件月份")]
        public string assign_month { get; set; }

        [Display(Name = "清理狀態")]
        public string clr_status { get; set; }

        [Display(Name = "主檔狀態")]
        public string ppaa_status { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "覆核結果")]
        public string appr_code { get; set; }

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "簡訊清除月份")]
        public string sms_clear_month { get; set; }

        [Display(Name = "簡訊狀態")]
        public string sms_status { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }
        public string check_date_b { get; set; }
        public string check_date_e { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }


        public OAP0042Model() {

            tempId = "";
            aply_no = "";
            appr_status = "";
            execActionDesc = "";
            create_id = "";
            create_dt = "";

            type = "";
            type_desc = "";
            data_status = "";
            rpt_cnt_tp = "";
            stat_amt_b = "";
            stat_amt_e = "";
            assign_month = "";
            clr_status = "";
            ppaa_status = "";
            o_paid_cd = "";
            appr_code = "";
            fsc_range = "";
            sms_clear_month = "";
            sms_status = "";
            check_date_b = "";
            check_date_e = "";

            check_no = "";
            paid_id = "";

        }

    }
}