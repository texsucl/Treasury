using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using Obounl.Daos;

namespace Obounl.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            //NlogSet("123", null, Nlog.Info);
            //var result =  new MSSql().Query<A>(
            //    "select * from SYS_CODE Where SYS_CD = @SYS_CD",
            //    new { SYS_CD = "AP" }
            //    );
            //var q = result.ToList();
     //       var result = new MSSql().Execute(
     //          @"INSERT INTO [dbo].[SYS_CODE]
     //      ([SYS_CD]
     //      ,[CODE_TYPE]
     //      ,[CODE]
     //      ,[CODE_VALUE]
     //      ,[ISORTBY]
     //      ,[REMARK])
     //VALUES
     //      (@SYS_CD,
     //       @CODE_TYPE,
     //       @CODE,
     //       @CODE_VALUE,
     //       @ISORTBY,
     //       @REMARK) ", new List<A>() { 
     //              new A() {
     //              SYS_CD = "AA",
     //              CODE_TYPE = "BB",
     //              CODE = "AB",
     //              CODE_VALUE = "AABB",
     //              ISORTBY = 1,
     //              REMARK = null
     //          },
     //          new A(){
     //              SYS_CD = "AA",
     //              CODE_TYPE = "BB",
     //              CODE = "BC",
     //              CODE_VALUE = "BBCC",
     //              ISORTBY = 2,
     //              REMARK = "123"
     //          }});
            return View();
        }

        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error401()
        {
            ViewBag.status = "401 無此操作權限";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error404()
        {
            ViewBag.status = "404 找不到此頁面";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error403()
        {
            ViewBag.status = "403 禁止: 拒絕存取";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error500()
        {
            ViewBag.status = "500 伺服器錯誤";
            return View("~/Views/Shared/Error.cshtml");
        }

        public class A {
            public string SYS_CD { get; set; }
            public string CODE_TYPE { get; set; }
            public string CODE { get; set; }
            public string CODE_VALUE { get; set; }
            public int ISORTBY { get; set; }
            public string REMARK { get; set; }
        }
    }
}
