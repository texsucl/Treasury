using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SSO.Web.BO
{
    public class ReportWrapper
    {
        public ReportWrapper()
        {
            ReportDataSources = new List<ReportDataSource>();
            ReportSubDataSources = new Dictionary<string, DataTable>();
            ReportParameters = new List<ReportParameter>();
        }

        // Properties
        public string ReportPath { get; set; }

        public List<ReportDataSource> ReportDataSources { get; set; }

        public Dictionary<string, DataTable> ReportSubDataSources { get; set; }

        public List<ReportParameter> ReportParameters { get; set; }

        public bool IsDownloadDirectly { get; set; }

    }
}