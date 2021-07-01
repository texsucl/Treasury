using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


/// <summary>
/// ==============================================
/// 修改日期/修改人：20191230 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：1.修改"程式OWNER單位"
///           2.增加"管理者單位"
/// ==============================================
/// </summary>



namespace SSO.Web.ViewModels
{
    public class RoleMgrModel
    {
        [Display(Name = "角色代碼")]
        public string cRoleID { get; set; }


        [Required]
        [MaxLength(20)]
        [Display(Name = "角色名稱")]
        public string cRoleName { get; set; }

        [Display(Name = "程式OWNER單位")]
        public string authUnit { get; set; }

        public string authUnitNm { get; set; }

        [Display(Name = "允許其他單位授權")]
        public string freeAuth { get; set; }

        [Display(Name = "停用註記")]
        public string isDisabled { get; set; }


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


        [Display(Name = "管理者單位")]
        public string mgrUnit { get; set; }

        public string mgrUnitNm { get; set; }


        public RoleMgrModel() {
            cRoleID = "";
            cRoleName = "";
            authUnit = "";
            authUnitNm = "";
            freeAuth = "";
            isDisabled = "";
            vMemo = "";
            freezeUid = "";
            cUpdUserID = "";
            cUpdDateTime = "";
            cCrtUserID = "";
            cCrtDateTime = "";
            dataStatus = "";
            mgrUnit = "";
            mgrUnitNm = "";
        }

    }
}