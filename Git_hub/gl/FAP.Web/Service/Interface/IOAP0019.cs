using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAP.Web.Service.Interface
{
    interface IOAP0019
    {
        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        List<OAP0019ViewModel> GetSearchData(OAP0019SearchViewModel searchModel);
    }
}
