using FTPHRIS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPHRIS.model
{
    class HRIS_BGDEPT : IModel
    {
        public DateTime? EFFDT { get; set; }

        public string CHANGEMARK { get; set; }

        public string SETID { get; set; }

        public string DEPTID { get; set; }

        public string BGCODE { get; set; }

        public string TW_DEPT_LEVEL { get; set; }

        public string DEPT_LVLNAME { get; set; }

        public string DEPT_NAME { get; set; }

        public string LOCATION { get; set; }

        public string LOCNAME { get; set; }

        public string PSMGR_EMPLID { get; set; }

        public string MGR_EMPLID { get; set; }

        public string MGR_NAME { get; set; }

        public string PARENT_NODE_NAME { get; set; }

        public string UPBGCODE { get; set; }

        public string LEVEL_DETERMINER { get; set; }

        public string UPDEPT_LVLNAME { get; set; }

        public string UPDEPT_NAME { get; set; }

        public string PSUPMGR_EMPLID { get; set; }

        public string UPMGR_EMPLID { get; set; }

        public string UPMGR_NAME { get; set; }

        public string TW_BGCODE1 { get; set; }

        public string TW_BGCODE2 { get; set; }

        public string TW_BGCODE3 { get; set; }

        public string TW_BGCODE4 { get; set; }

        public string TW_BGCODE5 { get; set; }

        public string TW_BGCODE6 { get; set; }

        public string TW_BGCODE7 { get; set; }

        public string TW_BGCODE8 { get; set; }

        public string DEPT_LONGNAME { get; set; }

        public DateTime? CRT_DATETIME { get; set; }
        public string STATUS { get; set; }
    }
}
