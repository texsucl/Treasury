using FTPHRIS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPHRIS.model
{
    class HRIS_EMP_SEC : IModel
    {
        public DateTime? EFFDT { get; set; }
        public string ACTION { get; set; }
        public string EMPLID { get; set; }
        public string EMPL_RCD { get; set; }
        public string TW_PRER_NUM_BU { get; set; }
        public string BGSETID { get; set; }
        public string BGDEPTID { get; set; }
        public string BGCODE { get; set; }
        public string BGJOBDESCR { get; set; }
        public string BUSETID { get; set; }
        public string BUDEPTID { get; set; }
        public string BUSINESS_UNIT2 { get; set; }
        public string BUCODE { get; set; }
        public string BUHRGWCODE { get; set; }
        public string BUJOBDESCR { get; set; }
        public string TW_TITLE_NUM { get; set; }
        public string TW_TITLE { get; set;}
        public string LOCATION { get; set; }
        public string LOCNAME { get; set; }
        public string BUORGMANAGER { get; set; }
        public string BGORGMANAGER { get; set; }
        public DateTime? EXPECTED_END_DATE { get; set; }
        public DateTime? JOINDTE { get; set; }
        public string TW_TITLE_CODE { get; set; }
        public DateTime? TERDTE { get; set; }
        public DateTime? CRT_DATETIME { get; set; }
        public string STATUS { get; set; }
    }
}
