using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB015Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "MAIL群組")]
        public string groupCode { get; set; }

        [Display(Name = "MAIL群組說明")]
        public string groupValue { get; set; }

        [Display(Name = "備註")]
        public string codeRemark { get; set; }

        [Display(Name = "A系統的資料類別")]
        public string codeReserve1 { get; set; }


        [Display(Name = "F系統的資料來源")]
        public string codeReserve2 { get; set; }

        [Display(Name = "角色/帳號")]
        public string receiverEmpno { get; set; }

        public string receiverEmpDesc { get; set; }

        [Display(Name = "設定類型")]
        public string empType { get; set; }

        [Display(Name = "MAIL給科主管")]
        public string isNotifyMgr { get; set; }

        [Display(Name = "MAIL給部主管")]
        public string isNotifyDeptMgr { get; set; }

        [Display(Name = "備註")]
        public string memo { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "執行功能")]
        public string status { get; set; }
        public string statusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "異動日期")]
        public string updDatetime { get; set; }

        [Display(Name = "覆核人員")]
        public string apprId { get; set; }

        public string apprName { get; set; }

        [Display(Name = "覆核日期")]
        public string apprDt { get; set; }

        public string reserve1 { get; set; }
        public string reserve2 { get; set; }
        public string reserve3 { get; set; }


        public ORTB015Model() {
            tempId = "";
            aplyNo = "";

            groupCode = "";
            groupValue = "";
            codeRemark = "";
            codeReserve1 = "";
            codeReserve2 = "";

            receiverEmpno = "";
            empType = "";
            isNotifyMgr = "";
            isNotifyDeptMgr = "";
            memo = "";

            updId = "";
            updateUName = "";
            updDatetime = "";
            apprId = "";
            apprName = "";
            apprDt = "";

            dataStatus = "";
            status = "";
            statusDesc = "";

            reserve1 = "";
            reserve2 = "";
            reserve3 = "";
        }

    }
}