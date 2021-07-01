using System;
using System.Collections.Generic;
using System.Data;

namespace FGL.Web.BO
{
    [Serializable]
    public class ReportWrapperO
    {
        // Constructors
        public ReportWrapperO()
        {
            ReportDataSources = new Dictionary<string, DataTable>();
            ReportParameters = new Dictionary<string, string>();
        }

        // Properties
        public string ReportPath { get; set; }

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