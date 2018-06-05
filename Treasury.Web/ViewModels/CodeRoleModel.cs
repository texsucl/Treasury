using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class CodeRoleModel
    {
        public string aplyNo { get; set; }

        public string updateDT { get; set; }

        public string updateUid { get; set; }

        public string execAction { get; set; }
        public string execActionDesc { get; set; }

        public string roleName { get; set; }

        public string roleNameB { get; set; }

        public string isDisabled { get; set; }

        public string isDisabledDesc { get; set; }

        public string isDisabledB { get; set; }

        public string isDisabledDescB { get; set; }

        public string memo { get; set; }

        public string memoB { get; set; }

        public string apprUid { get; set; }

        public string apprStatus { get; set; }

        public string apprStatusDesc { get; set; }


        public CodeRoleModel() {
            aplyNo = "";
            updateDT = "";
            updateUid = "";
            execAction = "";
            execActionDesc = "";
            roleName = "";
            roleNameB = "";
            isDisabled = "";
            isDisabledB = "";
            memo = "";
            memoB = "";
            apprUid = "";
            apprStatus = "";
            apprStatusDesc = "";
        }


    }
}