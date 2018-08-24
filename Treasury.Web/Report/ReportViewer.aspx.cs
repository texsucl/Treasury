using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Treasury.WebUtility;

namespace Treasury.Web.Report
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
                var rw = (ReportWrapper)base.Session[ReportWrapperSessionKey];
                if (rw != null)
                {
                    var RptViewer = this.rptViewer;

                    RptViewer.LocalReport.DataSources.Clear();

                    // Rdlc location
                    RptViewer.LocalReport.ReportPath = rw.ReportPath;

                    // Set report data source
                    RptViewer.LocalReport.DataSources.Clear();
                    foreach (var reportDataSource in rw.ReportDataSources)
                    { RptViewer.LocalReport.DataSources.Add(reportDataSource); }


                    // Set DownLoad Name
                    var _name = rw.ReportParameters.FirstOrDefault(x => x.Name == "Title");
                    if (_name != null)
                    {
                        string _DisplayName = _name.Values[0].ToString();
                        _DisplayName = _DisplayName.Replace("(", "-").Replace(")", "");
                        RptViewer.LocalReport.DisplayName = _DisplayName;
                    }

                    // Set report parameters
                    RptViewer.LocalReport.SetParameters(rw.ReportParameters);


                    // Refresh report
                    RptViewer.LocalReport.Refresh();

                    // Remove session
                    Session[ReportWrapperSessionKey] = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}