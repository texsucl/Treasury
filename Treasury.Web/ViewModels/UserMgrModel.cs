using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class UserMgrModel
    {
       

        [Display(Name = "使用者帳號")]
        public string cUserID { get; set; }

        [Display(Name = "使用者姓名")]
        public string cUserName { get; set; }

        [Display(Name = "啟用/停用")]
        public string isDisabled { get; set; }
        public string isDisabledDesc { get; set; }

        public string isDisabledB { get; set; }
        public string isDisabledDescB { get; set; }

        [Display(Name = "寄送MAIL")]
        public string isMail { get; set; }
        public string isMailDesc { get; set; }

        public string isMailB { get; set; }
        public string isMailDescB { get; set; }

        [Display(Name = "單位")]
        public string cWorkUnitCode { get; set; }


        public string cWorkUnitDesc { get; set; }

        public string upDptCd { get; set; }

        public string dptType { get; set; }


        [Display(Name = "新增人員")]
        public string cCrtUserID { get; set; }

        [Display(Name = "新增日期")]
        public string cCrtDate { get; set; }

        [Display(Name = "異動人員")]
        public string cUpdUserID { get; set; }

        [Display(Name = "異動日期")]
        public string cUpdDate { get; set; }

        [Display(Name = "異動區間")]
        public string cUpdDateB { get; set; }

        public string cUpdDateE { get; set; }

        [Display(Name = "角色群組")]
        public string roleAuthType { get; set; }
        public string roleAuthTypeDesc { get; set; }

        [Display(Name = "角色名稱")]
        public string codeRole { get; set; }

        [Display(Name = "覆核狀態")]
        public string apprtatus { get; set; }
        

        [Display(Name = "覆核人員")]
        public string apprUid { get; set; }

        [Display(Name = "覆核日期")]
        public string apprDt { get; set; }

        [Display(Name = "資料凍結人員")]
        public string frezzeUid { get; set; }


        [Display(Name = "資料凍結日期")]
        public string frezzeDt { get; set; }


        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        public string dataStatusDesc { get; set; }

        public string roleName { get; set; }

        [MaxLength(250)]
        [Display(Name = "備註")]
        public string vMemo { get; set; }

        public string vMemoB { get; set; }


        public UserMgrModel()
        {
            

            cUserID = "";
            cUserName = "";
            isDisabled = "";
            isDisabledDesc = "";
            isDisabledB = "";
            isDisabledDescB = "";
            isMail = "";
            isMailDesc = "";
            isMailB = "";
            isMailDescB = "";
            vMemo = "";
            vMemoB = "";
            cWorkUnitCode = "";
            cWorkUnitDesc = "";
            codeRole = "";
            cCrtUserID = "";
            cCrtDate = "";
            cUpdUserID = "";
            cUpdDate = "";
            apprUid = "";
            apprDt = "";
            frezzeUid = "";
            frezzeDt = "";
            dataStatus = "";
            dataStatusDesc = "";
            roleAuthType = "";
            roleAuthTypeDesc = "";
            roleName = "";
            apprtatus = "";
        }

    }
}