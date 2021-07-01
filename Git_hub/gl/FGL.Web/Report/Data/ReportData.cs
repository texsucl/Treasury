using FGL.Web.BO;
using System.Collections.Generic;
using System.Data;

namespace FGL.Web.Report.Data
{
    public abstract class ReportData : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public ReportData()
        {
            extensionParms = new List<reportParm>();
        }
        public abstract DataSet GetData(List<reportParm> parms);
        public List<reportParm> extensionParms { get; set; }
    }
}