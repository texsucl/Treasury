using FTPHRIS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPHRIS.model
{
    class HRIS_TITLE : IModel
    {
        public DateTime? EFFDT { get; set; }
        public string CHANGEMARK { get; set; }
        public string BUSINESS_UNIT { get; set; }
        public string TW_TITLE_CODE { get; set; }
        public string TW_TITLE { get; set; }
        public string TW_TITLE_NUM { get; set; }
        public DateTime? CRT_DATETIME { get; set; }
        public string STATUS { get; set; }
    }
}
