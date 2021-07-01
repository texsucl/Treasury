using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.Infrastructure
{
    public class DeleteFileAttribute : ActionFilterAttribute
    {
        //在執行 Action Result 之後執行
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Flush();

            //convert the current filter context to file and get the file path
            string filePath = (filterContext.Result as FilePathResult).FileName;

            //delete the file after download
            System.IO.File.Delete(filePath);
        }
    }
}