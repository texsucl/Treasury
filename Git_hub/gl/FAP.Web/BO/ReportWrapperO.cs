using iTextSharp.text;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FAP.Web.BO
{
    [Serializable]
    public class ReportWrapperO
    {
        // Constructors
        public ReportWrapperO()
        {
            //ReportDataSources = new List<ReportDataSource>();
            //ReportParameters = new List<ReportParameter>();
            ReportDataSources = new Dictionary<string, DataTable>();
            ReportParameters = new Dictionary<string, string>();
        }

        // Properties
        public string ReportPath { get; set; }

        //public List<ReportDataSource> ReportDataSources { get; set; }

        //public List<ReportParameter> ReportParameters { get; set; }

        public Dictionary<string, DataTable> ReportDataSources { get; set; }

        public Dictionary<string, string> ReportParameters { get; set; }

        public bool IsDownloadDirectly { get; set; }
    }

    [Serializable]
    public class reportParm
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}