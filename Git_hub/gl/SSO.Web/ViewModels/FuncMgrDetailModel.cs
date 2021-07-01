using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class FuncMgrDetailModel
    {
        [Display(Name = "系統別")]
        public string sysCd { get; set; }

        [Display(Name = "授權單位")]
        public string authUnit { get; set; }

        [Display(Name = "功能編號")]
        [MaxLength(10), MinLength(1)]
        public string cFunctionID { get; set; }

        [Display(Name = "上層功能")]
        [MaxLength(10), MinLength(1)]
        public string cParentFunctionID { get; set; }

        [Display(Name = "上層功能名稱")]
        [MaxLength(25), MinLength(1)]
        public string cParentFunctionName { get; set; }

        [Display(Name = "功能名稱")]
        [MaxLength(25), MinLength(1)]
        public string cFunctionName { get; set; }

        [Display(Name = "功能類別")]
        [MaxLength(10), MinLength(1)]
        public string cFunctionType { get; set; }

        [Display(Name = "功能層級")]
        public int iFunctionLevel { get; set; }

        [Display(Name = "功能備註")]
        [MaxLength(250), MinLength(0)]
        public string vFunctionMemo { get; set; }

        [Display(Name = "功能連結")]
        [MaxLength(1), MinLength(1)]
        public String vFunctionUrl { get; set; }

        [Display(Name = "排序")]
        public int iSortBy { get; set; }

        [Display(Name = "啟用狀態")]
        [MaxLength(1), MinLength(1)]
        public String cFlag { get; set; }


        [Display(Name = "新增人ID")]
        [MaxLength(5), MinLength(5)]
        public String cCrtUserID { get; set; }

        [Display(Name = "創建人名稱")]
        [MaxLength(20), MinLength(20)]
        public String cCrtUserName { get; set; }

        [Display(Name = "新增日期")]
        public string cCrtDate { get; set; }


        [Display(Name = "異動人ID")]
        [MaxLength(5), MinLength(5)]
        public String cUpdUserID { get; set; }

        [Display(Name = "最后異動人名稱")]
        [MaxLength(20), MinLength(20)]
        public String cUpdUserName { get; set; }

        [Display(Name = "最後異動日期")]
        public string cUpdDate { get; set; }


        public FuncMgrDetailModel()
        {

            sysCd = "";
            authUnit = "";
            cFunctionID = "";
            cParentFunctionID = "";
            cParentFunctionName = "";
            cFunctionName = "";
            cFunctionType = "";
            iFunctionLevel = 0;
            vFunctionMemo = "";
            vFunctionUrl = "";
            iSortBy = 0;
            cFlag = "";
            cCrtUserID = "";
            cCrtUserName = "";
            cCrtDate = "";
            cUpdUserID = "";
            cUpdUserName = "";
            cUpdDate = "";
            
        }

    }



}