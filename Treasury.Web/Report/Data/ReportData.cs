using System.Collections.Generic;
using System.Data;
using Treasury.Web.Report.Interface;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public abstract class ReportData : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public ReportData()
        {
            extensionParms = new List<reportParm>();
            //string efstr = System.Configuration.ConfigurationManager.
            //             ConnectionStrings["IFRS9Entities"].ConnectionString;
            //int start = efstr.IndexOf("\"", StringComparison.OrdinalIgnoreCase);
            //int end = efstr.LastIndexOf("\"", StringComparison.OrdinalIgnoreCase);
            //start++;
            //defaultConnection = efstr.Substring(start, end - start); //後期可改獨立出去
            defaultConnection = System.Configuration.ConfigurationManager.
                         ConnectionStrings["dbTreasury"].ConnectionString;
        }
        public abstract DataSet GetData(List<reportParm> parms);

        public List<reportParm> extensionParms { get; set; }
    }
}