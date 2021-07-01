using System;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Collections.Generic;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System.Linq;
using System.Drawing.Printing;


namespace FGL.Report
{
    public partial class OGL00005PViewer : System.Web.UI.Page
    {
        private DataTable reportSubDataSources = new DataTable();


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


                // Set report data source
                RptViewer.LocalReport.DataSources.Clear();
                
                RptViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", rw.ReportDataSources));
                RptViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", rw.ReportSubDataSources));

                //foreach (var reportDataSource in rw.ReportDataSources)
                //{ RptViewer.LocalReport.DataSources.Add(reportDataSource); }

                try
                {
                    //處理參數
                    Dictionary<string, string> para = rw.ReportParameters;


                    RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "商品資料接收報表"));

                    //RptViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", "富邦人壽"));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));

                    
                    RptViewer.LocalReport.SetParameters(new ReportParameter("acctUpDt", para["acctUpDt"]));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("acctUpId", para["acctUpId"]));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("acctApprId", para["acctApprId"]));

                    //// 子報表事件
                    //reportSubDataSources = rw.ReportDataSources;

                    //if (reportSubDataSources != null)
                    //{
                    //    RptViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(subReportProcessing);

                    //}

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
            
        }
    }
}