using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Obounl.Controllers;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using Obounl.Utility;
using System.Reflection;
using Newtonsoft.Json;
//using Obounl.Models;
//using Obounl.Utility;

namespace Obounl.Infrastructure
{
    public class MVCBrowserEventAttribute : ActionFilterAttribute
    {
        private string _eventName;
        private bool _encrypt;
        private bool _allowCORS;

        public MVCBrowserEventAttribute(string eventName, bool encrypt = false, bool allowCORS = false)
        {
            _eventName = eventName;
            _encrypt = encrypt;
            _allowCORS = allowCORS;
        }

        //在執行 Action 之前執行
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (_allowCORS)
                {   
                    //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                    //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type,token");
                    //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
                    //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Expose-Headers", "*");
                    //base.OnActionExecuting(filterContext);
                }
                var _controllerName = (string)filterContext.RouteData.Values["controller"];
                var _actionName = (string)filterContext.RouteData.Values["action"];
                var _ip = new Common().GetIp();
                var _Event_Name = formatEventName(filterContext, _eventName);
                NlogSet($@"ip:{_ip},controllerName:{_controllerName},actionName:{_actionName},paramter:{_Event_Name}", null, Nlog.Info);
            }
            catch (Exception ex)
            {
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }
        }


        private string formatEventName(ActionExecutingContext filterContext, string eventName)
        {
            #region default
            if (filterContext.ActionParameters.Any())
            {
                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
                foreach (var item in filterContext.ActionParameters)
                {
                    Type t = item.Value.GetType();
                    if (t.Equals(typeof(string)))
                        result.Add($"{item.Key} : {((string)item.Value)?.ToString()?.paramaterEncrypt(_encrypt)}");
                    else if (t.Equals(typeof(int)))
                        result.Add($"{item.Key} : {((int)item.Value).ToString().paramaterEncrypt(_encrypt)}");
                    else if (t.Equals(typeof(bool)))
                        result.Add($"{item.Key} : {((bool)item.Value).ToString().paramaterEncrypt(_encrypt)}");
                    else if (t.Equals(typeof(double)))
                        result.Add($"{item.Key} : {((double)item.Value).ToString().paramaterEncrypt(_encrypt)}");
                    else if (t.Equals(typeof(DateTime)))
                        result.Add($"{item.Key} : {((DateTime)item.Value).ToString("yyyy/MM/dd").paramaterEncrypt(_encrypt)}");
                    else if (t.GetProperties().Any())
                    {
                        result.Add($@"{item.Key} : {(JsonConvert.SerializeObject(item.Value))?.paramaterEncrypt(_encrypt)}");
                        //foreach (PropertyInfo _item in t.GetProperties())
                        //{
                        //    result.Add($@"{_item.Name} : {(_item.GetValue(item.Value))?.ToString()}");
                        //}
                    }
                }
                if (result.Any())
                    return $"({string.Join(",", result)}){eventName}";
            }
            #endregion
            return eventName;
        }
    }
}