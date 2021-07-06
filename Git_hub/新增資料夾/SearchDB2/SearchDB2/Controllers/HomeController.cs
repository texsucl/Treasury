using SearchDB2.Interface;
using SearchDB2.Models;
using SearchDB2.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.IO;

namespace SearchDB2.Controllers
{
    public class HomeController : Controller
    {
        internal ICacheProvider Cache { get; set; }
        public HomeController() {
            this.Cache = new DefaultCacheProvider();
        }
        public ActionResult Index()
        {
            Session["Id"] = Guid.NewGuid().ToString();
            var datas = new List<SelectOption>();
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if(!string.IsNullOrWhiteSpace(c.Name) && c.Name.IndexOf("_") > -1)
                    datas.Add(new SelectOption() { Text = c.Name, Value = c.Name });
            }
            ViewBag.CST = new SelectList(datas, "Value", "Text");
            return View();
        }
        public static string Id
        {
            get
            {
                return System.Web.HttpContext.Current.Session["Id"]?.ToString();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 執行動作
        /// </summary>
        /// <param name="sqlStr">sql string</param>
        /// <param name="type"> S => ExecuteReader , E => ExecuteNonQuery</param>
        /// <param name="transaction"> true or false </param>
        /// <returns></returns>
        public JsonResult work(string sqlStr,string type, bool transaction, string connectionString)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (string.IsNullOrWhiteSpace(sqlStr))
            {
                result.DESCRIPTION = "請確認Sql是否有輸入!";
                return Json(result);
            }
            try
            {
                var _result = GetDBType(connectionString);
                result.RETURN_FLAG = _result.Item1;
                if (_result.Item1)
                {
                    var _r = _result.Item2.work(sqlStr, type, transaction, connectionString);
                    result.RETURN_FLAG = _r.RETURN_FLAG;
                    result.DESCRIPTION = _r.DESCRIPTION;
                    if (result.RETURN_FLAG && type == "S" && _r.Datas.Any())
                    {
                        var obj = _r.Datas.First();
                        Cache.Invalidate("Data"); //清除
                        Cache.Set("Data", _r.Datas); //把資料存到 Cache
                        var jqgridInfo = obj.TojqGridData();
                        result.Datas = Json(jqgridInfo);
                        result.RETURN_FLAG = true;
                    }
                }
                else
                {
                    result.DESCRIPTION = "connectionString 設定有誤";
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }
            return Json(result);
        }

        public JsonResult GetExcel()
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = true;
            var data = (List<System.Dynamic.ExpandoObject>)Cache.Get("Data");
            if (data == null)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = "無資料!";
            }
            else
            {
                result.DESCRIPTION = ((data.Count / 60000) + 1).ToString();
            }
            return Json(result);
        }

        [HttpGet]
        public ActionResult DownloadExcel(string type)
        {
            try
            {
                var str = $@"Excel_Data_{type}.xls";
                var data = (List<System.Dynamic.ExpandoObject>)Cache.Get("Data");
                return File(DataTableToExcel(ToDataTable(data.Cast<System.Dynamic.ExpandoObject>().ToList(),type), str), System.Web.MimeMapping.GetMimeMapping(str), str);         
            }
            catch (Exception ex)
            {
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
        }

        public static MemoryStream DataTableToExcel(DataTable dt, string path)
        {
            try
            {
                ISheet ws;
                string version = "2003"; //default 2003
                IWorkbook wb = null;
          
                //建立Excel 2003檔案
                if ("2003".Equals(version))
                    wb = new HSSFWorkbook();

                if ("2007".Equals(version))
                    wb = new XSSFWorkbook();

                ws = wb.CreateSheet($@"Data");
                ExcelSetValue(ws, dt);
                MemoryStream stream = new MemoryStream();
                wb.Write(stream);
                stream.Flush();
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                //關閉文件
                //oXL.Quit();
            }
        }

        private static void ExcelSetValue(ISheet ws, DataTable dt)
        {             
             ws.CreateRow(0);//第一行為欄位名稱
             for (int i = 0; i < dt.Columns.Count; i++)
             {
                 ws.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i]?.ColumnName);
             }
             for (int i = 0 ; i < dt.Rows.Count; i++)
             {
                 ws.CreateRow(i + 1);
                 for (int j = 0; j < dt.Columns.Count; j++)
                 {
                     var _data = dt.Rows[i][j]?.ToString();
                     if (_data != null)
                         ws.GetRow(i + 1).CreateCell(j).SetCellValue(_data);
                 }
             }
             for (int i = 0; i < dt.Columns.Count; i++)
             {
                 ws.AutoSizeColumn(i);
             }                      
        }

        public static DataTable ToDataTable(List<System.Dynamic.ExpandoObject> items,string type)
        {
            var data = items.ToArray();
            if (data.Count() == 0) return null;

            var dt = new DataTable();
            foreach (var key in ((IDictionary<string, object>)data[0]).Keys)
            {
                dt.Columns.Add(key);
            }
            var _type = Convert.ToInt32(type);
            foreach (var d in data.Skip(0 + (_type * 60000)).Take(60000))
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }
            return dt;
        }

        [HttpPost]
        public ActionResult GetCacheData(jqGridParam jdata, string type)
        {
            if (Cache.IsSet("Data"))
                return Content((jdata.dynToJqgridResult((List<System.Dynamic.ExpandoObject>)Cache.Get("Data"))));
            return null;
        }

        private Tuple<bool,ISql> GetDBType(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return new Tuple<bool, ISql>(false, null);
            switch (connectionString.Trim().ToLower().Split('_')[0])
            {
                case "mssql":
                    return new Tuple<bool, ISql>(true, new MsSql_Conn());
                case "oracle":
                    return new Tuple<bool, ISql>(true, new Oracle_Conn());
                case "as400":
                    return new Tuple<bool, ISql>(true, new EasyCom_Conn());
                default:
                    return new Tuple<bool, ISql>(false, null);
            }
        }

        public class SelectOption
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }
    }
}