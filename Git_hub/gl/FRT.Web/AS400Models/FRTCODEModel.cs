using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTCODEModel
    {
        public string groupId { get; set; }

        public string refNo { get; set; }

        public string text { get; set; }

        public string srce_from { get; set; }

        public string userMark { get; set; }

        public string entryDate { get; set; }
        public string entryYY { get; set; }
        public string entryMM { get; set; }
        public string entryDD { get; set; }
        public string entryTime { get; set; }
        public string entryId { get; set; }

        public string updDate { get; set; }
        public string updYY { get; set; }
        public string updMM { get; set; }
        public string updDD { get; set; }
        public string updId { get; set; }

        public string apprvFlg { get; set; }
        public string filler1 { get; set; }
        public string filler2 { get; set; }
        public string filler3 { get; set; }

        public string apprId { get; set; }
        public string apprDate { get; set; }


        public FRTCODEModel()
        {
            groupId = "";
            refNo = "";
            text = "";
            srce_from = "";
            userMark = "";

            entryDate = "";
            entryYY = "";
            entryMM = "";
            entryDD = "";
            entryTime = "";
            entryId = "";
            updDate = "";
            updYY = "";
            updMM = "";
            updDD = "";
            updId = "";
            apprvFlg = "";
            filler1 = "";
            filler2 = "";
            filler3 = "";

            apprId = "";
            apprDate = "";
        }
    }
}