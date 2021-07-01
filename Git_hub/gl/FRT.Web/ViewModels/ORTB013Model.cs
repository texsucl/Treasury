using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB013Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }

        [Display(Name = "組別碼")]
        public string groupId { get; set; }

        [Display(Name = "文字長度")]
        public string textLen { get; set; }

        [Display(Name = "參考號碼")]
        public string refNo { get; set; }

        [Display(Name = "參考號碼_異動後")]
        public string refNoN { get; set; }

        [Display(Name = "說明")]
        public string text { get; set; }

        public string othText { get; set; }

        [Display(Name = "資料來源")]
        public string srceFrom { get; set; }

        [Display(Name = "使用註記")]
        public string useMark { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "新增人員")]
        public string entryId { get; set; }

        public string entryName { get; set; }

        public string entryDate { get; set; }

        [Display(Name = "異動日期時間")]
        public string updDateTime { get; set; }

        [Display(Name = "資料狀態")]
        public string status { get; set; }
        public string statusDesc { get; set; }

        [Display(Name = "需覆核否")]
        public string apprvFlg { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprStat { get; set; }

        [Display(Name = "覆核人員")]
        public string apprId { get; set; }

        public string apprName { get; set; }

        [Display(Name = "覆核日期")]
        public string apprDate { get; set; }

        [Display(Name = "覆核時間")]
        public string apprTimeN { get; set; }

        public string dataStatus { get; set; }

        public ORTB013Model() {
            tempId = "";
            aplyNo = "";
            groupId = "";
            textLen = "";
            refNo = "";
            refNoN = "";
            text = "";
            othText = "";
            srceFrom = "";
            useMark = "";
            entryId = "";
            entryName = "";
            entryDate = "";
            updId = "";
            updateUName = "";
            updDateTime = "";
            status = "";
            statusDesc = "";
            apprvFlg = "";
            apprStat = "";
            apprId = "";
            apprName = "";
            apprDate = "";
            apprTimeN = "";
            dataStatus = "";
        }

    }
}