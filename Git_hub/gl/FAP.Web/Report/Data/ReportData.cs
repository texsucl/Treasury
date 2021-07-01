using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FAP.Web.Report.Data
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