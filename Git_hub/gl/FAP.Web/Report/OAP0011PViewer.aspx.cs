using System;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Collections.Generic;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System.Linq;
using System.Drawing.Printing;
using System.IO;

namespace FAP.Report
{
    public partial class OAP0011PViewer : System.Web.UI.Page
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


                    RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票清理結案報表"));

                    //RptViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", "富邦人壽"));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));


                    RptViewer.LocalReport.SetParameters(new ReportParameter("paid_id", para["paid_id"]));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("paid_name", para["paid_name"]));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("level_1", para["level_1"]));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("level_2", para["level_2"]));
                   // RptViewer.LocalReport.SetParameters(new ReportParameter("closed_no", para["closed_no"]));

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

                //Microsoft.Reporting.WebForms.Warning[] tWarnings;
                //string[] tStreamids;
                //string tMimeType;
                //string tEncoding;
                //string tExtension;
                //byte[] tBytes = RptViewer.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
                //string fileName = "OAP0011P_" + DateUtil.getCurDateTime("") + ".pdf";
                //using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
                //{
                //    fs.Write(tBytes, 0, tBytes.Length);
                //}

                // Remove session
                Session[ReportWrapperSessionKey] = null;
            }

        }

        private void subReportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            
        }
    }
}