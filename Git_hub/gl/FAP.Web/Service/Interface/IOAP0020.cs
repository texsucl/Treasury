using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    interface IOAP0020
    {
        List<OAP0020ViewModel> GetSearchData();

        List<OAP0020ViewModel> GetSearchDetail(string dep_id, string dept_Name);

        Tuple<bool, string> CheckSameData(OAP0020InsertModel model, string mod);

        MSGReturnModel<string> InsertData(OAP0020InsertModel saveData, string userId);

        MSGReturnModel<string> UpdateData(OAP0020InsertModel saveData, string userId);
        MSGReturnModel<string> DeleteData(OAP0020InsertModel saveData, string userId);
    }
}
