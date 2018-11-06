using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.WebBO;

namespace Treasury.Web.Models
{
    public class TreasuryDBEntities : dbTreasuryEntities
    {
        public TreasuryDBEntities()
        {
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 300;
            //Database.Log = str => Treasury.WebUtility.Extension.NlogSet(str, Enum.Ref.Nlog.Info);
        }
    }

    // 針對PIA_LOG_MAIN 單獨加入下面兩個屬性
    public partial class PIA_LOG_MAIN
    {
        public string TRACKING_TYPE { get; set; }
        public string EXECUTION_TYPE { get; set; }
    }
}