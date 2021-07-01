using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRT.Web.BO;
using FRT.Web.ViewModels;

namespace FRT.Web.Service.Interface
{
    interface IORTB021
    {
        MSGReturnModel<List<ORTB021ViewModel>> GetSearchData(string close_date_s, string close_date_e);
    }
}
