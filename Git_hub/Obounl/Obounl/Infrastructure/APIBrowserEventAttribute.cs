using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using Obounl.Utility;
using System.Web.Http.Controllers;
using System.Threading;
using Newtonsoft.Json;

namespace Obounl.Infrastructure
{
    public class APIBrowserEventAttribute : ActionFilterAttribute
    {
        private string _eventName;
        private bool _encrypt;

        public APIBrowserEventAttribute(string eventName, bool encrypt = false)
        {
            _eventName = eventName;
            _encrypt = encrypt;
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                var route = actionContext.ControllerContext.RouteData.Route.RouteTemplate;
                var ip = new Common().GetIp();
                var paramter = formatEventName(actionContext, _eventName);

                NlogSet($@"開始執行: {route}, 來源IP: {ip}, 參數: {paramter}", null, Nlog.Info);
            }
            catch (Exception ex)
            {
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            try
            {
                var route = actionExecutedContext.ActionContext.ControllerContext.RouteData.Route.RouteTemplate;

                NlogSet($@"結束執行: {route}", null, Nlog.Info);
            }
            catch (Exception ex)
            {
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private string formatEventName(HttpActionContext actionContext, string eventName)
        {
            #region default
            if (actionContext.ActionArguments.Any())
            {
                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
                foreach (var item in actionContext.ActionArguments)
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