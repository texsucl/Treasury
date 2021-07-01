using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class SAAEMPYModel
    {
        public string errCode { get; set; }

        public string empyId { get; set; }

        public string empyName { get; set; }

        public string unitCode { get; set; }

        public string updCode { get; set; }

        public string sysMark { get; set; }


        public SAAEMPYModel()
        {
            errCode = "";
            empyId = "";
            empyName = "";
            unitCode = "";
            updCode = "";
            sysMark = "";

        }

        public SAAEMPYModel(string input) {

            input = StringUtil.toString(input).PadRight(30, ' ');

            this.errCode = input.Substring(0, 1);
            this.empyId = input.Substring(1, 10);
            this.empyName = input.Substring(11, 5);
            this.unitCode = input.Substring(16, 10);
            this.updCode = input.Substring(26, 2);
            this.sysMark = input.Substring(28, 1);
        }
    }
}