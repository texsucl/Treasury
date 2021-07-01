using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0105AP1ReportModel : IORT0105ReportModel
    {
        public Tuple<List<ORTReportAModel>, bool, List<ORTReportADetailModel>> model { get; set; }
    }
}