using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace FAP.Web.Controllers
{
    public class ReportController : BaseController
    {
        /// <summary>
        /// 報表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parms"></param>
        /// <param name="extensionParms"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReport(
            reportModel data,
            List<reportParm> parms,
            List<reportParm> extensionParms = null
            )
        {
            Utility.MSGReturnModel<string> result = new Utility.MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            try
            {
                string title = "報表名稱";
                if (string.IsNullOrWhiteSpace(data.className))
                {
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
                    //result.DESCRIPTION = MessageType.parameter_Error.GetDescription(null, "無呼叫的className");
                    return Json(result);
                }
                if (!string.IsNullOrWhiteSpace(data.title))
                    title = data.title;
                object obj = Activator.CreateInstance(Assembly.Load("FAP.Web").GetType($"FAP.Web.Report.Data.{data.className}"));
                MethodInfo[] methods = obj.GetType().GetMethods();
                MethodInfo mi = methods.FirstOrDefault(x => x.Name == "GetData");
                if (mi == null)
                {
                    //檢查是否有實作資料獲取
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
                    return Json(result);
                }
                if (parms == null)
                    parms = new List<reportParm>();
                DataSet ds = (DataSet)mi.Invoke(obj, new object[] { parms });
                List<reportParm> eparm = (List<reportParm>)(obj.GetType().GetProperty("extensionParms").GetValue(obj));
                ReportWrapperO rw = new ReportWrapperO();
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{data.className}.rdlc");
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    rw.ReportDataSources.Add("DataSet" + (i + 1).ToString(), ds.Tables[i]);
                }
                rw.ReportParameters.Add("Title", title);
                rw.ReportParameters.Add("UserId", Session["UserID"]?.ToString() + Session["UserName"]?.ToString());
                if (extensionParms != null)
                    extensionParms.ForEach(x => {
                        rw.ReportParameters.Add(x.key, x.value);
                    });
                if (eparm.Any())
                    eparm.ForEach(x =>
                    {
                        rw.ReportParameters.Add(x.key, x.value);
                    });
                rw.IsDownloadDirectly = false;
                var g = Guid.NewGuid().ToString();
                Session[g] = rw;
                result.RETURN_FLAG = true;
                result.Datas = g;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.ToString();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        public class reportModel
        {
            public string title { get; set; }
            public string className { get; set; }
        }
    }
}