using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeCleanPracModel
    {
        public string seq { get; set; }
        public string practice { get; set; }
        public string exec_date { get; set; }
        public string cert_doc { get; set; }

        public string practice_desc { get; set; }

        public string cert_doc_desc { get; set; }
        public string proc_desc { get; set; }


        public VeCleanPracModel()
        {
            seq = "";
            practice = "";
            exec_date = "";
            cert_doc = "";
            practice_desc = "";
            cert_doc_desc = "";
            proc_desc = "";
        }

    }
}