using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class SRTB0010Model
    {
        
        public string id { get; set; }

        public string rtnCode { get; set; }

        public string empNo { get; set; }

        public string empId { get; set; }

        public string empAd { get; set; }

        public string empName { get; set; }

        public string empUnit { get; set; }

        public string empMgrNo { get; set; }


        public SRTB0010Model()
        {
            id = "";
            rtnCode = "";
            empNo = "";
            empId = "";
            empAd = "";
            empName = "";
            empUnit = "";
            empMgrNo = "";
        }

    }
}