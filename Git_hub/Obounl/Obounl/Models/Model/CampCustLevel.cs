using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class CampCustLevel
    {
        public int CustLevelCode { get; set; }

        public string CustLevelName { get; set; }

        public string CampID { get; set; }

        public string CampCondition { get; set; }

        public string CustContGen { get; set; }

        public string UAmtType { get; set; }

        public string CampPromo { get; set; }

        public string ProductPay { get; set; }

        public string PolicyCurType { get; set; }

        public string YAmtType { get; set; }

        public string PlanYearSuFee { get; set; }

        public string PlanSuFee { get; set; }
    }
}