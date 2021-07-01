using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0105BD1ReportModel : IORT0105ReportModel
    {
        public Tuple<ORTReportCModel, List<ORTReportCDetailModel>> model { get; set; }
    }
}