//using Treasury.WebActionFilter;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

///// <summary>
///// 功能說明：獎勵所得-查詢異動作業
///// 初版作者：20170817 黃黛鈺
///// 修改歷程：20170817 黃黛鈺 
/////           需求單號：201707240447-01 
/////           初版
///// </summary>
///// 

//namespace Treasury.WebControllers
//{
//    [Authorize]
//    [CheckSessionFilterAttribute]

//    public class FuncRoleController : BaseController
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        public ActionResult FuncRole()
//        {


//            DbAccountEntities context = new DbAccountEntities();


//            var jsonData = new
//            {
//                item = (
//                from func in context.CODEFUNCTION
//                where 1 == 1

//                select new
//                {
//                    cFunctionID = func.CFUNCTIONID,
//                    cFunctionName = func.CFUNCTIONNAME
//                }
//                ).ToArray()
//            };
//            ViewBag.userFuncList = Json(jsonData, JsonRequestBehavior.AllowGet);
       


//            return View();
//        }
//    }
//}