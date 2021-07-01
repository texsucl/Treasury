using SSO.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SSO.Web.Daos
{
    public class CodeSysInfo
    {
        /**
        查詢出所有系統資料(for畫面下拉選單使用)
        **/
        public SelectList loadSelectList(bool bShowId)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.CODE_SYS_INFO;
            var result1 = (from sys in context.CODE_SYS_INFO
                           select new
                           {
                               SYS_CD = sys.SYS_CD.Trim(),
                               SYS_NAME = bShowId == true ? sys.SYS_CD.Trim() + sys.SYS_NAME.Trim() : sys.SYS_NAME.Trim()
                           }
                           );

            var items = new SelectList
                (
                items: result,
                dataValueField: "SYS_CD",
                dataTextField: "SYS_NAME",
                selectedValue: (object)null
                );

            return items;
        }



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