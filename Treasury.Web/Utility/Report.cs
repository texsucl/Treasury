using Microsoft.Reporting.WebForms;
using System.Collections.Generic;

namespace Treasury.WebUtility
{
    public class ReportWrapper
    {
        // Constructors
        public ReportWrapper()
        {
            ReportDataSources = new List<ReportDataSource>();
            ReportParameters = new List<ReportParameter>();
        }

        // Properties
        public string ReportPath { get; set; }

        public List<ReportDataSource> ReportDataSources { get; set; }

        public List<ReportParameter> ReportParameters { get; set; }

        public bool IsDownloadDirectly { get; set; }
    }

    public class reportParm
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}