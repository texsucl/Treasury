using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRT.Web.Report.Data
{
    public interface IReportData
    {
        DataSet GetData(List<reportParm> parms);
    }
}
