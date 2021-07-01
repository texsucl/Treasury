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
    interface IOAP0028
    {

        /// <summary>
        /// 執行明細表產出
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <param name="entry_date">登打日期(不輸入預設為系統日)</param>
        /// <returns></returns>
        Tuple<bool, string> changStatus(string userId, string entry_date = null);
    }
}
