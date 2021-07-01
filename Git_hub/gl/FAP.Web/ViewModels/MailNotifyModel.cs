using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class MailNotifyModel
    {
        public string receiverEmpno { get; set; }

        public string empType { get; set; }

        public string isNotifyMgr { get; set; }

        public string isNotifyDeptMgr { get; set; }

        public MailNotifyModel()
        {
            receiverEmpno = "";
            empType = "";
            isNotifyMgr = "";
            isNotifyDeptMgr = "";
        }
    }
}