using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SSO.WebControllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
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

    }
}