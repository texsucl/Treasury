using FGL.Web.BO;
using Microsoft.Reporting.WebForms;
using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FGL.Web.Report
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    var title = ((ReportWrapperO)base.Session[Request.QueryString["ReportId"]])
                        .ReportParameters.FirstOrDefault(x => x.Key == "Title").Value; 
                    DisableExportButtons(title, new string[] { "WORD", "PDF", "WORDOPENXML" }, false );
                    GenerateReport();
                }
                catch (Exception ex)
                {

                }

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

        private void DisableExportButtons(string title, string[] strFormatName, bool showPrintFlag = true)
        {
            if (title == "退費類別明細")
            {
                try
                {
                    FieldInfo FInfo;
                    var RptViewer = this.rptViewer;
                    foreach (RenderingExtension RenExt in
                    RptViewer.LocalReport.ListRenderingExtensions())
                    {
                        foreach (string s in strFormatName)
                        {
                            if (RenExt.Name.Equals(s))
                            {
                                FInfo = RenExt.GetType().GetField("m_isVisible",
                                     BindingFlags.Instance | BindingFlags.NonPublic);
                                FInfo.SetValue(RenExt, false);
                            }
                        }
                    }
                    if (!showPrintFlag)
                        RptViewer.ShowPrintButton = false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}