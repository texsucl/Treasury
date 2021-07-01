using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0105AP2ReportModel : IORT0105ReportModel
    {
        public Tuple<List<ORTReportBModel>, bool, List<ORTReportBDetailModel>> model { get; set; }
    }
}