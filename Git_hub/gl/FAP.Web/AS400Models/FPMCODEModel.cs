using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class FPMCODEModel
    {
        public string groupId { get; set; }

        public string refNo { get; set; }

        public string text { get; set; }

        public string srce_from { get; set; }

        public string userMark { get; set; }

        public FPMCODEModel()
        {
            groupId = "";
            refNo = "";
            text = "";
            srce_from = "";
            userMark = "";
        }
    }
}