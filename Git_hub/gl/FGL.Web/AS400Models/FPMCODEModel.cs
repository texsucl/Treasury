using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FPMCODEModel
    {
        public string groupId { get; set; }

        public string textLen { get; set; }

        public string refNo { get; set; }

        public string text { get; set; }

        public string srce_from { get; set; }

        public string userMark { get; set; }

        public string entryYy { get; set; }

        public string entryMm { get; set; }

        public string entryDd { get; set; }

        public string entryTime { get; set; }

        public string entryId { get; set; }

        public string updYy { get; set; }

        public string updMm { get; set; }

        public string updDd { get; set; }

        public string updId { get; set; }



        public FPMCODEModel()
        {
            groupId = "";
            textLen = "";
            refNo = "";
            text = "";
            srce_from = "";
            userMark = "";

            entryYy = "";
            entryMm = "";
            entryDd = "";
            entryTime = "";
            entryId = "";
            updYy = "";
            updMm = "";
            updDd = "";
            updId = "";
        }
    }
}