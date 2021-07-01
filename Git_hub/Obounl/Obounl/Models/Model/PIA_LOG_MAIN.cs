using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class PIA_LOG_MAIN
    {
        public string TRACKING_TYPE { get; set; }
        public string ACCESS_ACCOUNT { get; set; }
        public string ACCOUNT_NAME { get; set; }
        public string FROM_IP { get; set; }
        public DateTime ACCESS_DATE { get; set; }
        public TimeSpan ACCESS_TIME { get; set; }
        public string PROGFUN_NAME { get; set; }
        public string ACCESSOBJ_NAME { get; set; }
        public string EXECUTION_TYPE { get; set; }
        public string EXECUTION_CONTENT { get; set; }
        public int AFFECT_ROWS { get; set; }
        public string PIA_OWNER1 { get; set; }
        public string PIA_OWNER2 { get; set; }
        public string PIA_TYPE { get; set; }
    }
}