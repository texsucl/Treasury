using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    interface IAlreadyConfirmedSearch
    {
       Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>> GetFirstTimeData();

       List<AlreadyConfirmedSearchDetailViewModel> GetSearchDetail(AlreadyConfirmedSearchViewModel searchData);
    }
}
