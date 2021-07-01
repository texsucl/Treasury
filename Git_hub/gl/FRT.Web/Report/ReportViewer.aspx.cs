using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace FRT.Web.Report
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GenerateReport();
            }
        }

        private void GenerateReport()
        {
            try
            {
                var ReportWrapperSessionKey = Request.QueryString["ReportId"];
                var rw = (ReportWrapperO)base.Session[ReportWrapperSessionKey];
                if (rw != null)
                {
                    var RptViewer = this.rptViewer;

                    RptViewer.LocalReport.DataSources.Clear();

                    // Rdlc location
                    RptViewer.LocalReport.ReportPath = rw.ReportPath;

                    // Set report data source
                    RptViewer.LocalReport.DataSources.Clear();
                    foreach (var reportDataSource in rw.ReportDataSources)
                    { RptViewer.LocalReport.DataSources.Add(new ReportDataSource(reportDataSource.Key, reportDataSource.Value)); }

                    // Set DownLoad Name
                    var _name = rw.ReportParameters.FirstOrDefault(x => x.Key == "Title");
                    if (rw.ReportParameters.ContainsKey("Title"))
                    {
                        string _DisplayName = rw.ReportParameters["Title"];
                        _DisplayName = _DisplayName.Replace("(", "-").Replace(")", "");
                        RptViewer.LocalReport.DisplayName = _DisplayName;
                    }

                    // Set report parameters
                    rw.ReportParameters.ToList().ForEach(x =>
                    {
                        RptViewer.LocalReport.SetParameters(new ReportParameter(x.Key, x.Value));
                    });


                    // Refresh report
                    RptViewer.LocalReport.Refresh();

                    // Remove session
                    Session[ReportWrapperSessionKey] = null;
                }
                else
                {
                    Response.Redirect(Page.ResolveClientUrl("~/Home/Error403"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}