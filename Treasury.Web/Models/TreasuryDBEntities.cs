using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Treasury.Web.Models
{
    public class TreasuryDBEntities : dbTreasuryEntities
    {
        public TreasuryDBEntities()
        {

        }
    }

    // 針對PIA_LOG_MAIN 單獨加入下面兩個屬性
    public partial class PIA_LOG_MAIN
    {
        public string TRACKING_TYPE { get; set; }
        public string EXECUTION_TYPE { get; set; }
    }
}