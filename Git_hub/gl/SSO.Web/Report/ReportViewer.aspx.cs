using System;
using SSO.Web.BO;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SSO.Report
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        private Dictionary<string, DataTable> reportSubDataSources;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string id = Request.QueryString["id"];
                GenerateReport(id);
            }
        }

        private void GenerateReport(string id)
        {

            var ReportWrapperSessionKey = id;
            var rw = (ReportWrapper)base.Session[ReportWrapperSessionKey];
            if (rw != null)
            {
                var RptViewer = this.rptViewer;

                RptViewer.LocalReport.DataSources.Clear();

                // Rdlc location
                RptViewer.LocalReport.ReportPath = rw.ReportPath;

                RptViewer.ProcessingMode = ProcessingMode.Local;

                // Set report data source
                RptViewer.LocalReport.DataSources.Clear();
                foreach (var reportDataSource in rw.ReportDataSources)
                { RptViewer.LocalReport.DataSources.Add(reportDataSource); }

                try
                {
                    // Set report parameters
                    RptViewer.LocalReport.SetParameters(rw.ReportParameters);

                    // 子報表事件
                    reportSubDataSources = rw.ReportSubDataSources;

                    if (reportSubDataSources != null) {
                        RptViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(subReportProcessing);

                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }

                // Refresh report
                RptViewer.LocalReport.Refresh();

                // Remove session
                Session[ReportWrapperSessionKey] = null;
            }

        }

        private void subReportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            string UnitId = e.Parameters["UnitId"].Values[0];

            DataTable dtDetail = reportSubDataSources["DataSetSub"];

            var query = from data in dtDetail.AsEnumerable()
                        where data.Field<String>("UnitId") == UnitId
                        select data;
            DataTable tbl = query.CopyToDataTable<DataRow>();

            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSetSub", tbl));
        }
    }
}