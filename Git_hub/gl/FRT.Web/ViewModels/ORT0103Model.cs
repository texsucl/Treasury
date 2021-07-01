using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0103Model
    {
        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "退匯代碼")]
        public string rejected_Code { get; set; }

        [Display(Name = "退匯原因")]
        public string rejected_Reason { get; set; }

        [Display(Name = "收款人ID")]
        public string received_Id { get; set; }

        [Display(Name = "收款人戶名")]
        public string received_Account { get; set; }

        [Display(Name = "銀行代號")]
        public string bank_Code { get; set; }

        [Display(Name = "銀行帳號")]
        public string bank_Account { get; set; }

        [Display(Name = "異動人員")]
        public string update_Id { get; set; }

        [Display(Name = "異動人員姓名")]
        public string update_Name { get; set; }

        [Display(Name = "異動日期")]
        public string update_Datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_Id { get; set; }

        [Display(Name = "覆核日期")]
        public string appr_Datetime { get; set; }

        [Display(Name = "資料狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "執行類型")]
        public string status { get; set; }

        [Display(Name = "執行類型畫面")]
        public string status_Show { get; set; }
        

    }
}