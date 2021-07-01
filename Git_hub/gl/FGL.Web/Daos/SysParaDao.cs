using FGL.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FGL.Web.Daos
{
    public class SysParaDao
    {
        public SelectList qrySmpRule56List()
        {
 
            dbFGLEntities context = new dbFGLEntities();

            var result1 = (from para in context.SYS_PARA
                           where para.SYS_CD == "GL"
                             & para.GRP_ID == "SmpRule56"
                           orderby para.PARA_ID.Length, para.PARA_ID
                           select new
                           {
                               CCODE = para.PARA_ID.Trim(),
                               CVALUE =  para.PARA_VALUE.Trim()
                           }
                           );

            var items = new SelectList
                (
                items: result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)null
                );

            return items;
        }



        public List<SYS_PARA> qryByGrpId(string sysCd, string grpId)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.SYS_PARA.Where(x => x.SYS_CD == sysCd & x.GRP_ID == grpId).OrderBy(x => x.PARA_ID).ToList();


            return result;
        }

    }
}