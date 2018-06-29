using System.Collections.Generic;
using System.Data;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Interface
{
    public interface IReportData
    {
        DataSet GetData(List<reportParm> parms);
    }
}