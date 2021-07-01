using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class CodeRoleItemModel
    {
        public string id { get; set; }
        public string roleId { get; set; }

        public string itemId { get; set; }

        public string authType { get; set; }

        public string itemOpType { get; set; }

        public string itemDesc { get; set; }

        public string execAction { get; set; }

        public string execActionDesc { get; set; }

        public string aplyNo { get; set; }

        public string updateDT { get; set; }

        public string updateUid { get; set; }

        public string apprStatus { get; set; }

        public string apprStatusDesc { get; set; }

    }
}