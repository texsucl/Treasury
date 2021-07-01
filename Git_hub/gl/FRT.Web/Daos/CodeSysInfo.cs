using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class CodeSysInfo
    {
        public Dictionary<string, string> qryUrlDic()
        {
            dbFGLEntities context = new dbFGLEntities();

            var result1 = (from type in context.CODE_SYS_INFO
                           select new
                           {
                               SYS_CD = type.SYS_CD.Trim(),
                               SYS_URL = type.SYS_URL.Trim()
                           }
                           ).ToDictionary(x => x.SYS_CD, x => x.SYS_URL);


            return result1;
        }
    }
}