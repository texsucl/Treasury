using System;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Collections.Generic;
using FRT.Web.BO;
using FRT.Web.ViewModels;
using System.Linq;
using System.Drawing.Printing;

namespace FRT.Report
{
    public partial class ORTB005PViewer : System.Web.UI.Page
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

                //foreach (var reportDataSource in rw.ReportDataSources)
                //{ RptViewer.LocalReport.DataSources.Add(reportDataSource); }

                try
                {
                    // Set report parameters
                    // RptViewer.LocalReport.SetParameters(rw.ReportParameters);

                    if("ORTB005P1".Equals(rw.ReportId))
                        RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速及櫃檯匯款付款資料彙總表"));
                    else
                        RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速及櫃檯匯款付款資料明細表"));

                    RptViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", "富邦人壽"));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));

                    // 子報表事件
                    reportSubDataSources = rw.ReportDataSources;

                    if (reportSubDataSources != null)
                    {
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
            string field081 = e.Parameters["field081"].Values.First();
            string paidType = e.Parameters["paidType"].Values.First();
            string currency = e.Parameters["currency"].Values.First();

            DataTable dtDetail = reportSubDataSources;

            var query = from data in dtDetail.AsEnumerable()
                        where data.Field<String>("field081") == field081
                        & data.Field<String>("paidType") == paidType
                         & data.Field<String>("currency") == currency
                        select data;
            DataTable tbl = query.CopyToDataTable<DataRow>();

            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", tbl));
        }
    }
}