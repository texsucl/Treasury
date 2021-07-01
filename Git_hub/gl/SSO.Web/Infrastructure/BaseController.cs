

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Diagnostics;

namespace SSO.Web
{
    public class BaseController : Controller
    {
        #region properties
        private DateTime _startTime;
        #endregion

        #region base
        public string AreaName
        {
            get
            {
                return ControllerContext.RouteData.DataTokens["area"].ToString();
            }
        }

        public string ControllerName
        {
            get
            {
                return ControllerContext.RouteData.Values["controller"].ToString();
            }
        }

        public string ActionName
        {
            get
            {
                return ControllerContext.RouteData.Values["action"].ToString();
            }
        }

        public BaseController()
        {
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            _startTime = DateTime.Now;
            return base.BeginExecute(requestContext, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
           

            base.Dispose(disposing);
        }
        #endregion

        #region Jquery DataTables
 

        //public JsonResult JsonTable(Fubon.Orm.OrmPagination list)
        //{
        //    return Json(new { draw = Pagination.Draw, recordsFiltered = list.Total, recordsTotal = list.Total, data = list.Data }, JsonRequestBehavior.AllowGet);
        //}

        public string GetFormValue(string name)
        {
            string result = "";
            try
            {
                result = Request.Form.GetValues(name).FirstOrDefault();
            }
            catch { }
            return result;
        }
        #endregion

        #region error
        public List<string> GetErrorListFromModelState(ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
        }

        public void AlertMessage(string message)
        {
            //throw new FBException(FBExceptionEnum.MessageOnly, message);
        }

        public void AlertMessage(List<string> messages)
        {
            //throw new FBException(FBExceptionEnum.MessageOnly, messages);
        }

        public void AlertAndLogMessage(string message)
        {
            //throw new FBException(FBExceptionEnum.MessageAndLogged, message);
        }

        public void AlertAndLogMessage(List<string> messages)
        {
            //throw new FBException(FBExceptionEnum.MessageAndLogged, messages);
        }

        /// <summary>
        /// 攔截exception判斷是否為ajax request or FBEception or 其他.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            bool isAjaxCall = string.Equals("XMLHttpRequest", filterContext.RequestContext.HttpContext.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
            var ex = filterContext.Exception;
            if (isAjaxCall)
            {
                List<string> errors = new List<string>();

                //if (ex is FBException)
                //{
                //    FBException fbException = ex as FBException;
                //    if (fbException.ReturnMessageToClient)
                //    {
                //        if (fbException.Messages != null && fbException.Messages.Count > 0)
                //            errors = fbException.Messages;
                //        else
                //            errors.Add(fbException.Message);
                //    }

                //    if (fbException.LogException)
                //    {
                //        //if (fbException.Messages != null && fbException.Messages.Count > 0)
                //        //    //Log.AddSysLogError(String.Join(";", fbException.Messages.ToArray()), statusCode: 500);
                //        //else
                //        //   // Log.AddSysLogError(fbException.Message, statusCode: 500);

                //    }
                //}
                //else
                //{
                //    errors.Add("ErrorMsg".ToI18N(MessageType.ErrorMessage));
                //   // Log.AddSysLogError(ex.Message, statusCode: 500, xml: ex.StackTrace);
                //}

                //filterContext.Result = new JsonNetResult()
                //{
                //    Data = new JsonResponseModel { Errors = errors, Status = "500" }
                //};
            }
            else
            {
                //if (ex is FBException)
                //{
                //    FBException fbException = ex as FBException;
                //    if (fbException.LogErrorPage)
                //    {
                //        filterContext.Result = RedirectToAction("Error", "Account");
                //    }
                //}
                var code = (ex is HttpException) ? (ex as HttpException).GetHttpCode() : 500;
                //Log.AddSysLogError(ex.Message, statusCode: code, xml: ex.StackTrace);
            }

            //Make sure that we mark the exception as handled
            filterContext.ExceptionHandled = true;
        }
        #endregion

        #region DataBase
        //public static DataBase Db
        //{
        //    get
        //    {
        //        return BaseService.Db;
        //    }
        //}

        //public static DataBase DbSSS
        //{
        //    get
        //    {
        //        return BaseService.DbSSS;
        //    }
        //}

        //public static DataBase DbDB2
        //{
        //    get
        //    {
        //        return BaseService.DbDB2;
        //    }
        //}
        #endregion
    }
}