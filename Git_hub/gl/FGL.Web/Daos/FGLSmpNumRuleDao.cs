
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;

namespace FGL.Web.Daos
{
    public class FGLSmpNumRuleDao
    {
        public List<FGL_SMP_NUM_RULE> qryForOGL00007(string fuMk)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FGL_SMP_NUM_RULE> dataList = db.FGL_SMP_NUM_RULE
                    .Where(x => x.fu_mk == fuMk || x.fu_mk == "" || x.fu_mk == null
                    ).ToList();

                return dataList;
            }
        }
    }
}