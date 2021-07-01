using FGL.Web.ActionFilter;
using FGL.Web.Service.Actual;
using FGL.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FGL.Web.Controllers
{
    // GET: Common
    [Authorize]
    [CheckSessionFilterAttribute]
    public class CommonController : BaseController
    {
        internal ICacheProvider Cache { get; set; }

        public CommonController()
        {
            Cache = new DefaultCacheProvider();
        }

        //public string GetLRTCODE(string GROUP_ID, string REF_NO)
        //{
        //    return new FAP.Web.Service.Actual.Common().GetMessage(GROUP_ID, REF_NO);
        //}

        //public string GetDeptName(string deptId)
        //{
        //    string deptName = "";
        //    deptId = deptId?.Trim();
        //    if (!deptId.IsNullOrWhiteSpace())
        //    {
        //        if (deptId.Length == 5)
        //        {
        //            var _fullDepName = new Service.Actual.Common().getFullDepName(new List<string>() { deptId });
        //            deptName = _fullDepName.First().Item2;
        //        }
        //    }
        //    deptName = (deptId == deptName ? string.Empty : deptName);
        //    return deptName;
        //}

        //public string GetUserName(string userId)
        //{
        //    string userName = string.Empty;

        //    if (!userId.IsNullOrWhiteSpace())
        //    {
        //        userName = new Service.Actual.Common().GetMemoByUserId(new List<string>() { userId.Trim() }).First().Item2;
        //    }
        //    return userName;
        //}
    }
        
}