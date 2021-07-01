using FGL.Web.BO;
using System.Collections.Generic;
using System.Data;

namespace FGL.Web.Report.Data
{
    public interface IReportData
    {
        DataSet GetData(List<reportParm> parms);
    }
}
