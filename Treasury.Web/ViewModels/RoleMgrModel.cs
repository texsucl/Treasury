using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class RoleMgrModel
    {
        [Display(Name = "角色代碼")]
        public string cRoleID { get; set; }


        [Required]
        [MaxLength(20)]
        [Display(Name = "角色名稱")]
        public string cRoleName { get; set; }

        [Display(Name = "停用註記")]
        public string isDisabled { get; set; }

        [Display(Name = "角色群組")]
        public string roleAuthType { get; set; }
        public string roleAuthTypeDesc { get; set; }

        [MaxLength(250)]
        [Display(Name = "備註")]
        public string vMemo { get; set; }


        [Display(Name = "資料凍結人員")]
        public string freezeUid { get; set; }

        [Display(Name = "異動人員")]
        public string cUpdUserID { get; set; }

        [Display(Name = "異動日期/時間")]
        public string cUpdDateTime { get; set; }

        [Display(Name = "新增人員")]
        public string cCrtUserID { get; set; }

        [Display(Name = "新增日期/時間")]
        public string cCrtDateTime { get; set; }


        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "分類")]
        public List<FuncRoleModel> Categories { get; set; }


        public RoleMgrModel() {
            cRoleID = "";
            cRoleName = "";
            roleAuthType = "";
            roleAuthTypeDesc = "";
            isDisabled = "";
            vMemo = "";
            freezeUid = "";
            cUpdUserID = "";
            cUpdDateTime = "";
            cCrtUserID = "";
            cCrtDateTime = "";
            dataStatus = "";


        }

    }
}