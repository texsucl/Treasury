using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class CodeUserRoleModel
    {
        public String aplyNo { get; set; }

        public String userId { get; set; }

        public String roleId { get; set; }

        public String roleName { get; set; }

        public String roleAuthType { get; set; }

        public String roleAuthTypeDesc { get; set; }

        public String createUid { get; set; }

        public String createDt { get; set; }

        public String execAction { get; set; }

        public String execActionDesc { get; set; }


        public CodeUserRoleModel() {
            aplyNo = "";
            userId = "";
            roleId = "";
            roleName = "";
            roleAuthType = "";
            roleAuthTypeDesc = "";
            createUid = "";
            createDt = "";
            execAction = "";
            execActionDesc = "";
        }
    }
}