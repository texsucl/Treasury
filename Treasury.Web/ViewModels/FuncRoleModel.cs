using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.WebViewModels
{
    public class FuncRoleModel
    {
        public String cRoleId { get; set; }

        public String cRoleName { get; set; }

        public String vMemo { get; set; }

        public String cFunctionID { get; set; }

        public String cFunctionName { get; set; }

        public int iFunctionLevel { get; set; }

        public String cParentFunctionID { get; set; }

        public String cParentFunctionName { get; set; }

        public int iParentFunctionLevel { get; set; }

        public string execAction { get; set; }

        public string roleAuthType { get; set; }


    }
}