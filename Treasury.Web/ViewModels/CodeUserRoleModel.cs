using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class CodeUserRoleModel
    {

        public String userId { get; set; }

        public String roleId { get; set; }

        public String roleName { get; set; }

        public String createUid { get; set; }

        public String createDt { get; set; }

        public String execAction { get; set; }

        public String execActionDesc { get; set; }


        public CodeUserRoleModel() {
            userId = "";
            roleId = "";
            roleName = "";
            createUid = "";
            createDt = "";
            execAction = "";
            execActionDesc = "";
        }
    }
}