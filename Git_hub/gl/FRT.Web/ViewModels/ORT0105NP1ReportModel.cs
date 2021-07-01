using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0105NP1ReportModel : IORT0105ReportModel
    {
        public Tuple<List<ORTReportDModel>, bool, List<ORTReportDDetailModel>> model { get; set; }
    }
}