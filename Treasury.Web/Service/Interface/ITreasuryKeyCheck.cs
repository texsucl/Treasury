using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.Enum;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryKeyCheck
    {
        #region Get
        /// <summary>
        /// 
        /// </summary>
        TreasuryKeyCheckViewModel GetItemId();
        #endregion

        #region Save

        #endregion

    }
}
