using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq.Expressions;
using Treasury.Web.Enum;
using System.Reflection.Emit;
using System.Data.Entity;
using NLog;
using System.Globalization;

namespace Treasury.WebUtility
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static class Extension
    {
        public static StringBuilder CheckBoxString(string name, List<CheckBoxListInfo> listInfo,
            IDictionary<string, object> htmlAttributes, int number)
        {
            StringBuilder sb = new StringBuilder();
            int lineNumber = 0;
            sb.Append("<table style='width:100%;'><tr>");
            foreach (CheckBoxListInfo info in listInfo)
            {
                lineNumber++;
                TagBuilder builder = new TagBuilder("input");
                if (info.IsChecked)
                {
                    builder.MergeAttribute("checked", "checked");
                }
                builder.MergeAttributes<string, object>(htmlAttributes);
                builder.MergeAttribute("type", "checkbox");
                builder.MergeAttribute("value", info.Value);
                builder.MergeAttribute("name", name);
                builder.MergeAttribute("class", "styled");
                builder.InnerHtml = string.Format(" <label>{0}</label> ", info.DisplayText);
                sb.Append(
                    string.Format("<td><div class='checkbox checkbox-info'>{0}</div></td>",
                    builder.ToString(TagRenderMode.Normal)));
                if (number == 0)
                {
                    //sb.Append("<br />");
                    sb.Append("</tr><tr>");
                }
                else if (lineNumber % number == 0)
                {
                    //sb.Append("<br />");
                    sb.Append("</tr><tr>");
                }
            }
            if (number == 0 || lineNumber % number == 0)
                sb.Remove(sb.Length - 4, 4);
            else
                sb.Append("</tr>");
            sb.Append("</table>");
            return sb;
        }

        public static IEnumerable<T> Filter<T>
                    (this IEnumerable<T> data, Func<T, bool> fun)
        {
            foreach (T item in data)
            {
                if (fun(item))
                    yield return item;
            }
        }

        public static List<FormateTitle> GetFormateTitles<T>(this T cls)
            where T : class
        {
            var result = new List<FormateTitle>();
            var obj = cls.GetType();
            if (!obj.IsClass)
                return result;
            obj.GetProperties()
               .ToList().ForEach(x =>
               {
                   var _des = x.GetCustomAttributes(typeof(DescriptionAttribute), false);
                   result.Add(new FormateTitle()
                   {
                       OldTitle = x.Name,
                       NewTitle = _des.Any() ? ((DescriptionAttribute)_des[0]).Description : x.Name
                   });
               });
            return result;
        }

        public static string GetDescription<T>(this T enumerationValue, string title = null, string body = null)
          where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
            }
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    if (!title.IsNullOrWhiteSpace() && !body.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1} => {2}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    if (!title.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description
                            );
                    if (!body.IsNullOrWhiteSpace())
                        return string.Format("{0} => {1}",
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        public static string GetExelName(this string str)
        {
            if (str.IsNullOrWhiteSpace())
                return string.Empty;
            string version = "2003"; //default 2003
            string configVersion = ConfigurationManager.AppSettings["ExcelVersion"];
            if (!configVersion.IsNullOrWhiteSpace())
                version = configVersion;
            return "2003".Equals(version) ? str + ".xls" : str + ".xlsx";
        }

        public static int GetMonths(this TimeSpan timespan)
        {
            return (int)(timespan.Days / 30.436875);
        }

        public static double GetYears(this TimeSpan timespan)
        {
            return (double)(timespan.Days / 365.2425);
        }

        public static bool IsNullOrEmpty
             (this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static bool IsNullOrWhiteSpace
                                    (this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static DateTime DateToLatestTime(this DateTime datetime)
        {
            return datetime.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        public static DateTime? DateToLatestTime(this DateTime? datetime)
        {
            if (datetime == null)
                return null;
            datetime = (datetime.Value.Date).DateToLatestTime();
            return datetime;
        }

        /// <summary>
        /// To the simple taiwan date.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        /// <returns></returns>
        public static string ToSimpleTaiwanDate(this DateTime datetime)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
            return string.Format("{0}/{1}/{2}",
                taiwanCalendar.GetYear(datetime),
                datetime.Month,
                datetime.Day);
        }

        /// <summary>
        /// 西元時間轉台灣文字時間
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string DateToTaiwanDate(this DateTime datetime, int length = 7,bool _bool=false)
        {
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
            if(datetime.Year <= 1911)
                return string.Empty;
           if (length == 9)
           {
              if(!_bool)
                  return string.Format("{0}/{1}/{2}", taiwanCalendar.GetYear(datetime), datetime.Month.ToString().PadLeft(2, '0'), datetime.Day.ToString().PadLeft(2, '0'));
              //$@"{taiwanCalendar.GetYear(datetime)}/{datetime.Month.ToString().PadLeft(2, '0')}/{datetime.Day.ToString().PadLeft(2, '0')}";
              else
                  return string.Format("{0}-{1}-{2}", taiwanCalendar.GetYear(datetime), datetime.Month.ToString().PadLeft(2, '0'), datetime.Day.ToString().PadLeft(2, '0'));
              //$@"{taiwanCalendar.GetYear(datetime)}/{datetime.Month.ToString().PadLeft(2, '0')}/{datetime.Day.ToString().PadLeft(2, '0')}";
           }
           else if (length == 17)
                return string.Format("{0}/{1}/{2} {3}", taiwanCalendar.GetYear(datetime), datetime.Month.ToString().PadLeft(2, '0'), datetime.Day.ToString().PadLeft(2, '0'), datetime.ToString("HH:mm:ss"));
                //$@"{taiwanCalendar.GetYear(datetime)}/{datetime.Month.ToString().PadLeft(2, '0')}/{datetime.Day.ToString().PadLeft(2, '0')} {datetime.ToString("HH:mm:ss")}";
           else
                return string.Format("{0}{1}{2}", taiwanCalendar.GetYear(datetime), datetime.Month.ToString().PadLeft(2, '0'), datetime.Day.ToString().PadLeft(2, '0'));
                //$@"{taiwanCalendar.GetYear(datetime)}{datetime.Month.ToString().PadLeft(2, '0')}{datetime.Day.ToString().PadLeft(2, '0')}";         
        }

        /// <summary>
        /// 台灣文字時間轉西元時間
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime TaiwanDateToDate(this string datetime)
        {
            DateTime result = DateTime.MinValue;
            if (!datetime.IsNullOrWhiteSpace() && 
                DateTime.TryParseExact(datetime.PadLeft(7,'0'),"yyyMMdd", null,DateTimeStyles.AllowWhiteSpaces ,out result))
            {
                result = result.AddYears(1911);
            }
            return result;
        }

        #region 時間相減 取年

        /// <summary>
        /// 時間相減 取年
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static double? dateSubtractToYear(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetYears();
        }

        #endregion 時間相減 取年

        #region 時間相減 取月

        /// <summary>
        /// 時間相減 取月
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int? dateSubtractToMonths(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetMonths();
        }

        #endregion 時間相減 取月

        #region CheckBoxList

        /// <summary>
        /// CheckBoxList.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="name">The name.</param>
        /// <param name="listInfo">CheckBoxListInfo.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns></returns>
        public static IHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo, object htmlAttributes)
        {
            return htmlHelper.CheckBoxList
            (
                name,
                listInfo,
                (IDictionary<string, object>)new RouteValueDictionary(htmlAttributes),
                0
            );
        }

        /// <summary>
        /// CheckBoxList.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="name">The name.</param>
        /// <param name="listInfo">The list info.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="number">每個Row的顯示個數.</param>
        /// <returns></returns>
        public static IHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo,
            IDictionary<string, object> htmlAttributes, int number)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("必須給這些CheckBoxList一個Tag Name", "name");
            }
            if (listInfo == null)
            {
                //return htmlHelper.Raw(string.Empty);
                throw new ArgumentNullException("必須要給List<CheckBoxListInfo> listInfo");
            }
            if (listInfo.Count < 1)
            {
                throw new ArgumentException("List<CheckBoxListInfo> listInfo 至少要有一組資料", "listInfo");
            }
            StringBuilder sb = CheckBoxString(name, listInfo, htmlAttributes, number);
            return htmlHelper.Raw(sb.ToString());
        }

        #endregion CheckBoxList

        #region List to DataTable

        public static DataTable ToDataTable(this List<System.Dynamic.ExpandoObject> items)
        {
            var data = items.ToArray();
            if (data.Count() == 0) return null;

            var dt = new DataTable();
            foreach (var key in ((IDictionary<string, object>)data[0]).Keys)
            {
                dt.Columns.Add(key);
            }
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }
            return dt;
        }

        public static DataTable ToDataTable<T>(this List<T> items)
            where T : class
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        #endregion List to DataTable

        /// <summary>
        /// class 轉 jqGridData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cls">class</param>
        /// <param name="widths">自定義寬度</param>
        /// <param name="aligns">自訂一顯示位置</param>
        /// <param name="act">是否加入</param>
        /// <param name="titles">客製化Title</param>
        /// <returns></returns>
        public static jqGridData<T> TojqGridData<T>(this T cls, int[] widths = null, string[] aligns = null, bool act = false, List<FormateTitle> titles = null)
            where T : class
        {
            var obj = cls.GetType();
            if (!obj.IsClass)
                return new jqGridData<T>();
            var jqgridParams = new jqGridData<T>();

            bool flag = false;
            int len = 0;
            if (widths != null && widths.Length > 0)
            {
                len = widths.Length;
                flag = true;
            }
            int widthIndex = 0;
            int? widthParam = null;

            bool alignFlag = false;
            int alignLen = 0;
            if (aligns != null && aligns.Length > 0)
            {
                alignLen = aligns.Length;
                alignFlag = true;
            }
            int alignIndex = 0;
            string alignParam = "left";

            if (act)
            {
                //jqgridParams.colNames.Add("act".formateTitle(titles));
                jqgridParams.colNames.Add("執行動作");
                jqgridParams.colModel.Add(new jqGridColModel()
                {
                    name = "act",
                    index = "act",
                    width = flag ? (len > widthIndex ? widths[widthIndex] : 100) : 100,
                    sortable = false
                });
                widthIndex += 1;
            }
            obj.GetProperties()
                .ToList().ForEach(x =>
                {
                    var str = x.Name;
                    jqgridParams.colModel.Add(new jqGridColModel()
                    {
                        name = str,
                        index = str,
                        width = flag ? (len > widthIndex ? widths[widthIndex] : widthParam) : widthParam,
                        align = alignFlag ? (alignLen > alignIndex ? aligns[alignIndex] : alignParam) : alignParam,
                    });
                    var des = x.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    str = des.Length > 0 ? ((DescriptionAttribute)des[0]).Description : x.Name;
                    jqgridParams.colNames.Add(str.formateTitle(titles));
                    widthIndex += 1;
                    alignIndex += 1;
                });
            return jqgridParams;
        }

        public static jqGridData TojqGridData(this System.Dynamic.ExpandoObject obj, int[] widths = null, string[] aligns = null, bool act = false, List<FormateTitle> titles = null)
        {
            var jqgridParams = new jqGridData();

            var tTarget = obj as System.Dynamic.IDynamicMetaObjectProvider;
            if (tTarget != null)
            {
                bool flag = false;
                int len = 0;
                if (widths != null && widths.Length > 0)
                {
                    len = widths.Length;
                    flag = true;
                }
                int widthIndex = 0;
                int? widthParam = 200;

                bool alignFlag = false;
                int alignLen = 0;
                if (aligns != null && aligns.Length > 0)
                {
                    alignLen = aligns.Length;
                    alignFlag = true;
                }
                int alignIndex = 0;
                string alignParam = "left";

                if (act)
                {
                    //jqgridParams.colNames.Add("act".formateTitle(titles));
                    jqgridParams.colNames.Add("執行動作");
                    jqgridParams.colModel.Add(new jqGridColModel()
                    {
                        name = "act",
                        index = "act",
                        width = flag ? (len > widthIndex ? widths[widthIndex] : 100) : 100,
                        sortable = false
                    });
                    widthIndex += 1;
                }
                var names = tTarget.GetMetaObject(System.Linq.Expressions.Expression.Constant(tTarget)).GetDynamicMemberNames();
                foreach (string name in names)
                {
                    var str = name;
                    jqgridParams.colModel.Add(new jqGridColModel()
                    {
                        name = str,
                        index = str,
                        width = flag ? (len > widthIndex ? widths[widthIndex] : widthParam) : widthParam,
                        align = alignFlag ? (alignLen > alignIndex ? aligns[alignIndex] : alignParam) : alignParam,
                    });
                    jqgridParams.colNames.Add(str.formateTitle(titles));
                    widthIndex += 1;
                    alignIndex += 1;
                }            
            }
            return jqgridParams;
        }

        public static void hideColModel(this List<jqGridColModel> colModel, List<string> hideTitles = null, bool userFlag = true)
        {
            var _titles = new List<string>();
            if(userFlag)
                _titles.AddRange(new List<string>()
            {
                "Create_User",
                "Create_Date",
                "Create_Time",
                "LastUpdate_User",
                "LastUpdate_Date",
                "LastUpdate_Time"
            });
            if (hideTitles != null)
            {
                _titles.AddRange(hideTitles);
            }
            colModel.ForEach(x =>
            {
                if (_titles.Any(y => y == x.index))
                    x.hidden = true;
            });
        }

        public static string getValidateString
            (this IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
                result = string.Join(" ", errors.Select(y=>string.Join(",",y.ValidationErrors.Select(z=>z.ErrorMessage))));
            return result;
        }

        public static string formateTitle(this string value, List<FormateTitle> titles)
        {
            if (titles != null && titles.Any() && !value.IsNullOrWhiteSpace())
            {
                var data = titles.FirstOrDefault(x => x.OldTitle == value);
                if (data != null)
                    return data.NewTitle;
            }
            return value;
        }

        public static Tuple<string,bool> CheckAFT(this string value, string oldpar)
        {
            if (oldpar.IsNullOrWhiteSpace() && value.IsNullOrWhiteSpace())
                return new Tuple<string, bool>(null, false) ;
            if (oldpar.IsNullOrWhiteSpace() && !value.IsNullOrWhiteSpace())
                return new Tuple<string, bool>(value,true);
            if (!oldpar.IsNullOrWhiteSpace() && !value.IsNullOrWhiteSpace() && (oldpar == value))
                return new Tuple<string, bool>(null, false); 
            return new Tuple<string, bool>(value, true);
        }

        public static Tuple<Decimal?, bool> CheckAFT(this Decimal value, decimal oldpar)
        {
            if (oldpar == value)
                return new Tuple<Decimal?, bool>(null, false);
            return new Tuple<Decimal?, bool>(value, true);
        }

        public static Tuple<int?, bool> CheckAFT(this int? value, int? oldpar)
        {
            if (oldpar == value)
                return new Tuple<int?, bool>(null, false);
            return new Tuple<int?, bool>(value, true);
        }

        public static string formateThousand(this string value)
        {
            decimal d = 0;
            try
            {
                if (decimal.TryParse(value, out d))
                {
                    if (value.IndexOf(".") > -1)
                    {
                        Int64 strNumberWithoutDecimals = Convert.ToInt64( value.Substring(0, value.IndexOf(".")).Replace(",",""));
                        string strNumberDecimals = value.Substring(value.IndexOf("."));
                        return strNumberWithoutDecimals.ToString("#,##0") + strNumberDecimals;
                    }
                    return Convert.ToInt64(value.Replace(",", "")).ToString("#,##0");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return value;
        }

        public static string stringToDateTimeStr(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return null;
            DateTime? rateDate = null; //RateDate
            rateDate = TypeTransfer.stringToDateTimeN(value);
            return rateDate.HasValue ? rateDate.Value.ToString("yyyy/MM/dd") : null;
        }

        public static string exceptionMessage(this Exception ex)
        {
            return $"message: {ex.Message}" +
                   $", inner message {ex.InnerException}";
            //$", inner message {ex.InnerException?.InnerException?.Message}";
        }

        public static string stringToStrSql(this string par)
        {
            if (!par.IsNullOrWhiteSpace())
                return $" '{par.Replace("'","''")}' ";
            return " null ";
        }

        public static string intToStrSql(this int par)
        {
            return $" {par.ToString()} ";
        }

        public static string intNToStrSql(this int? par)
        {
            if (par.HasValue)
                return $" {par.Value} ";
            return " null ";
        }

        public static string doubleNToStrSql(this double? par)
        {
            if (par.HasValue)
                return $" {par.Value} ";
            return " null ";
        }

        public static string dateTimeNToStrSql(this DateTime? par)
        {
            if (par.HasValue)
                return par.Value.ToString("yyyy/MM/dd").stringToStrSql();
            return " null ";
        }

        public static string dateTimeToStrSql(this DateTime par)
        {
            return par.ToString("yyyy/MM/dd").stringToStrSql();
        }

        public static string timeSpanToStrSql(this TimeSpan ts)
        {
            return ts.ToString(@"hh\:mm\:ss").stringToStrSql();
        }

        public static string dateTimeToStr(this DateTime par, bool allFlag = false)
        {
            if (allFlag)
                return par.ToString("yyyy/MM/dd HH:mm:ss");
            return par.ToString("yyyy/MM/dd");
        }

        public static string dateTimeToStr(this DateTime? par, bool allFlag = false)
        {
            if(par.HasValue)
            {
                if(allFlag)
                    return par.Value.ToString("yyyy/MM/dd HH:mm:ss"); 
                else
                    return par.Value.ToString("yyyy/MM/dd");
            }
            return string.Empty;
        }

        public static string timeSpanToStr(this TimeSpan ts)
        {
            return ts.ToString(@"hh\:mm\:ss");
        }

        public static string timeSpanNToStr(this TimeSpan? ts)
        {
            if(ts.HasValue)
            return ts.Value.ToString(@"hh\:mm\:ss");
            return string.Empty;
        }

        public static string stringToDtSql(this string par, int i = 10)
        {
            if (!par.IsNullOrWhiteSpace())
            {
                var dt = TypeTransfer.stringToDateTimeN(par, i);
                if(dt!=null)
                    return $" '{dt.Value.ToString("yyyy/MM/dd")}' ";
            }
            return " null ";
        }

        public static string stringToDblSql(this string par)
        {
            if (!par.IsNullOrWhiteSpace())
            {
                var dl = TypeTransfer.stringToDoubleN(par);
                if(dl != null)
                    return $" {dl.Value.ToString()} ";
            }
            return " null ";
        }

        public static string stringListToInSql(this List<string> datas)
        {
            if (!datas.Any())
                return string.Empty.stringToStrSql();
            List<string> pars = new List<string>();
            string sql = string.Empty;
            datas.ForEach(x => {
                if (!x.IsNullOrWhiteSpace())
                {
                    pars.Add(string.Format("'{0}'", x.Replace("'", "''")));
                }
            });
            return string.Join(",", pars);
        }

        public static string stringToSHA512(this string value)
        {
            string resultSha512 = string.Empty;
            if (!value.IsNullOrWhiteSpace())
            {
                SHA512 sha512 = new SHA512CryptoServiceProvider();
                resultSha512 = Convert.ToBase64String(sha512.ComputeHash(Encoding.Default.GetBytes(value)));
            }
            return resultSha512;
        }

        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        /// Model 和 Model 轉換
        /// <summary>
        /// Model 和 Model 轉換
        /// </summary>
        /// <typeparam name="T1">來源型別</typeparam>
        /// <typeparam name="T2">目的型別</typeparam>
        /// <param name="model">來源資料</param>
        /// <returns></returns>
        public static T2 ModelConvert<T1, T2>(this T1 model) where T2 : new()
        {
            T2 newModel = new T2();
            if (model != null)
            {
                foreach (PropertyInfo itemInfo in model.GetType().GetProperties())
                {
                    PropertyInfo propInfoT2 = typeof(T2).GetProperty(itemInfo.Name);
                    if (propInfoT2 != null)
                    {
                        // 型別相同才可轉換
                        if (propInfoT2.PropertyType == itemInfo.PropertyType)
                        {
                            propInfoT2.SetValue(newModel, itemInfo.GetValue(model, null), null);
                        }
                    }
                }
            }
            return newModel;
        }

        /// <summary>
        /// Convert List T1 type to List T2 type
        /// </summary>
        /// <typeparam name="T1">T1 type</typeparam>
        /// <typeparam name="T2">T2 type</typeparam>
        /// <param name="model">T1 type model</param>
        /// <returns>T2 type</returns>
        public static List<T2> ModelConvert<T1, T2>(this List<T1> model) where T2 : new()
        {
            List<T2> lstT2 = new List<T2>();
            if (model.Any())
            {
                foreach (T1 item in model)
                {
                    T2 newModel = ModelConvert<T1, T2>(item);
                    lstT2.Add(newModel);
                }
            }
            return lstT2;
        }

        public static IQueryable<T> Where<T>
            (this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool flag)
        {
            if(flag)
                return source.Where(predicate) ;
            return source;
        }

        public static IEnumerable<T> Where<T>
            (this IEnumerable<T> source, Func<T, bool> predicate, bool flag)
        {
            if (flag)
                return source.Where(predicate);
            return source;
        }

        public static List<dynamic> DynamicSqlQuery(this Database database, string sql, System.Data.Common.DbTransaction transaction = null , params object[] parameters)
        {
            TypeBuilder builder = createTypeBuilder(
                    "MyDynamicAssembly", "MyDynamicModule", "MyDynamicType");

            using (System.Data.IDbCommand command = database.Connection.CreateCommand())
            {
                try
                {
                    if (database.Connection.State != ConnectionState.Open)
                    database.Connection.Open();
                    if(transaction != null)
                    command.Transaction = transaction;
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        var schema = reader.GetSchemaTable();

                        foreach (System.Data.DataRow row in schema.Rows)
                        {
                            string name = (string)row["ColumnName"];
                            //var a=row.ItemArray.Select(d=>d.)
                            Type type = (Type)row["DataType"];
                            if (type != typeof(string) && (bool)row.ItemArray[schema.Columns.IndexOf("AllowDbNull")])
                            {
                                type = typeof(Nullable<>).MakeGenericType(type);
                            }
                            createAutoImplementedProperty(builder, name, type);
                        }
                    }
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            Type resultType = builder.CreateType();

            return database.SqlQuery(resultType, sql, parameters).Cast<dynamic>().ToList();
        }

        private static TypeBuilder createTypeBuilder(
            string assemblyName, string moduleName, string typeName)
        {
            TypeBuilder typeBuilder = AppDomain
                .CurrentDomain
                .DefineDynamicAssembly(new AssemblyName(assemblyName),
                                       AssemblyBuilderAccess.Run)
                .DefineDynamicModule(moduleName)
                .DefineType(typeName, TypeAttributes.Public);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder;
        }

        private static void createAutoImplementedProperty(
            TypeBuilder builder, string propertyName, Type propertyType)
        {
            const string PrivateFieldPrefix = "m_";
            const string GetterPrefix = "get_";
            const string SetterPrefix = "set_";

            // Generate the field.
            FieldBuilder fieldBuilder = builder.DefineField(
                string.Concat(PrivateFieldPrefix, propertyName),
                              propertyType, FieldAttributes.Private);

            // Generate the property
            PropertyBuilder propertyBuilder = builder.DefineProperty(
                propertyName, System.Reflection.PropertyAttributes.HasDefault, propertyType, null);

            // Property getter and setter attributes.
            MethodAttributes propertyMethodAttributes =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the getter method.
            MethodBuilder getterMethod = builder.DefineMethod(
                string.Concat(GetterPrefix, propertyName),
                propertyMethodAttributes, propertyType, Type.EmptyTypes);

            // Emit the IL code.
            // ldarg.0
            // ldfld,_field
            // ret
            ILGenerator getterILCode = getterMethod.GetILGenerator();
            getterILCode.Emit(OpCodes.Ldarg_0);
            getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
            getterILCode.Emit(OpCodes.Ret);

            // Define the setter method.
            MethodBuilder setterMethod = builder.DefineMethod(
                string.Concat(SetterPrefix, propertyName),
                propertyMethodAttributes, null, new Type[] { propertyType });

            // Emit the IL code.
            // ldarg.0
            // ldarg.1
            // stfld,_field
            // ret
            ILGenerator setterILCode = setterMethod.GetILGenerator();
            setterILCode.Emit(OpCodes.Ldarg_0);
            setterILCode.Emit(OpCodes.Ldarg_1);
            setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
            setterILCode.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            propertyBuilder.SetSetMethod(setterMethod);
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string modelToString<T>(this T model,string log = null) 
        {
            var result = string.Empty;
            if (model != null)
            {
                if (!log.IsNullOrWhiteSpace())
                    result += "|";
                StringBuilder sb = new StringBuilder();
                var Type = model.GetType();
                sb.Append($@"TableName:{Type.Name}|");
                var Pros = Type.GetProperties();
                Pros.ToList().ForEach(x =>
                {
                    sb.Append($@"{x.Name}:{TypeTransfer.objToString(x.GetValue(model))}|");
                });
                if (sb.Length > 0)
                {
                    result = sb.ToString();
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        /// <summary>
        /// Nlog 寫入
        /// </summary>
        /// <param name="message">訊息</param>
        /// <param name="nlog">類型 預設為訊息</param>
        public static void NlogSet(string message, Ref.Nlog nlog = Ref.Nlog.Info)
        {
            switch (nlog)
            {
                // 用於追蹤，可以在程式裡需要追蹤的地方將訊息以Trace傳出。
                case Ref.Nlog.Trace:
                    logger.Trace(message);
                    break;
                // 用於開發，於開發時將一些需要特別關注的訊息以Debug傳出。
                case Ref.Nlog.Debug:
                    logger.Debug(message);
                    break;
                // 訊息，記錄不影響系統執行的訊息，通常會記錄登入登出或是資料的建立刪除、傳輸等。
                case Ref.Nlog.Info:
                    logger.Info(message);
                    break;
                // 警告，用於需要提示的訊息，例如庫存不足、貨物超賣、餘額即將不足等。
                case Ref.Nlog.Warn:
                    logger.Warn(message);
                    break;
                // 錯誤，記錄系統實行所發生的錯誤，例如資料庫錯誤、遠端連線錯誤、發生例外等。
                case Ref.Nlog.Error:
                    logger.Error(message);
                    break;
                // 致命，用來記錄會讓系統無法執行的錯誤，例如資料庫無法連線、重要資料損毀等。
                case Ref.Nlog.Fatal:
                    logger.Fatal(message);
                    break;
            }            
        }
    }
}