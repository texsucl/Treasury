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
    public class ReportWrapper
    {
        public ReportWrapper()
        {
            //ReportDataSources = new List<ReportDataSource>();
            ReportSubDataSources = new DataTable();
            //ReportParameters = new List<ReportParameter>();
            ReportId = "";
            ReportPath = "";
            ReportDataSources = new DataTable();
            ReportParameters = new Dictionary<string, string>();
        }

        // Properties
        public string ReportId { get; set; }

        public string ReportPath { get; set; }

        public DataTable ReportDataSources { get; set; }

        public DataTable ReportSubDataSources { get; set; }

        public Dictionary<string, string> ReportParameters { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ReportId", ReportId);
            info.AddValue("ReportPath", ReportPath);
            info.AddValue("ReportDataSources", ReportDataSources);
            info.AddValue("ReportSubDataSources", ReportSubDataSources);
            info.AddValue("ReportParameters", ReportParameters);
        }
        protected ReportWrapper(SerializationInfo info, StreamingContext context)
        {
            ReportId = info.GetString("ReportId");
            ReportPath = info.GetString("ReportPath");
            ReportDataSources = (DataTable)info.GetValue("ReportDataSources", typeof(DataTable));
            ReportSubDataSources = (DataTable)info.GetValue("ReportSubDataSources", typeof(DataTable));
            ReportParameters = (Dictionary<string, string>)info.GetValue("ReportParameters", typeof(Dictionary<string, string>));
        }


        //public List<ReportDataSource> ReportDataSources { get; set; }

        //public Dictionary<string, DataTable> ReportSubDataSources { get; set; }

        //public List<ReportParameter> ReportParameters { get; set; }

        //public bool IsDownloadDirectly { get; set; }


    }
}