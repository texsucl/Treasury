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
    public partial class ORTB007PViewer : System.Web.UI.Page
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


                //處理參數
                Dictionary<string, string> para = rw.ReportParameters;

                try
                {
                    // Set report parameters
                    // RptViewer.LocalReport.SetParameters(rw.ReportParameters);


                    switch (rw.ReportId)
                    {
                        case "ORTB007P1":   //快速付款待匯款件明細表(累計)
                            RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速付款待匯款件明細表(累計)"));
                            break;
                        case "ORTB007P2":   //快速付款失敗件明細報表(累計)
                            RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速付款失敗件明細報表(累計)"));
                            break;
                        case "ORTB007P3":    //快速付款待匯款件明細表(依結案日)
                            RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速付款待匯款件明細表(依結案日)"));
                            break;
                        case "ORTB007P4":    //快速付款失敗報表-跨天通知
                            RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速付款失敗報表-跨天通知"));
                            break;
                        case "ORTB007P5":    //快速付款失敗報表-取消件
                            RptViewer.LocalReport.SetParameters(new ReportParameter("Title", "快速付款失敗報表-取消件"));
                            break;
                    }



                    RptViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", "富邦人壽"));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));
                    RptViewer.LocalReport.SetParameters(new ReportParameter("endDate", para["endDate"]));

                  

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

    }
}