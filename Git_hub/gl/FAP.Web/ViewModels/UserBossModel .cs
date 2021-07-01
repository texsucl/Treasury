using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class UserBossModel
    {

        public string usrId { get; set; }

        public string deptType { get; set; }

        public string empNo { get; set; }

        public string empName { get; set; }

        public string empMail { get; set; }

        public string usrIdMgr { get; set; }

        public string empNoMgr { get; set; }

        public string empNameMgr { get; set; }

        public string empMailMgr { get; set; }

        public string usrIdDeptMgr { get; set; }

        public string empNoDeptMgr { get; set; }

        public string empNameDeptMgr { get; set; }

        public string empMailDeptMgr { get; set; }

        public UserBossModel()
        {
            usrId = "";
            deptType = "";
            empNo = "";
            empName = "";
            empMail = "";

            usrIdMgr = "";
            empNoMgr = "";
            empNameMgr = "";
            empMailMgr = "";

            usrIdDeptMgr = "";
            empNoDeptMgr = "";
            empNameDeptMgr = "";
            empMailDeptMgr = "";

        }

    }
}