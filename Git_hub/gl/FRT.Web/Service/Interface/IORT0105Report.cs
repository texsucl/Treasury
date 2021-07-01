using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

namespace FRT.Web.Service.Interface
{
    interface IORT0105Report
    {
        MSGReturnModel<Tuple<IORT0105ReportModel,bool>> check(FRT_CROSS_SYSTEM_CHECK schedulerModel, string UserId, ReportType type = ReportType.S , string date_s = null , string date_e = null, string deadline = null);
    }
}
