using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace FAP.Web.Utilitys
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
        /// <summary>
        /// 判斷文字是否為Null 或 空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace
                                    (this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }


        public static IQueryable<T> Where<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool flag)
        {
            if (flag)
                return source.Where(predicate);
            return source;
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool flag)
        {
            if (flag)
                return source.Where(predicate);
            return source;
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

        public static string getValidateString(this IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
                result = string.Join(" ", errors.Select(y => string.Join(",", y.ValidationErrors.Select(z => z.ErrorMessage))));
            return result;
        }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string exceptionMessage(this Exception ex)
        {
            return $"message: {ex.Message}" +
                   $", inner message {ex.InnerException}";
            //$", inner message {ex.InnerException?.InnerException?.Message}";
        }

        /// <summary>
        /// 千分位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string formateThousand(this string value)
        {

             decimal d = 0;
             if (string.IsNullOrWhiteSpace(value))
                 return string.Empty;
             try
             {
                 if (decimal.TryParse(value, out d))
                 {
                     if (value.IndexOf(".") > -1)
                     {
                         var _val = value.Substring(0, value.IndexOf("."));
                         var _valFlag = string.Empty;
                         if (_val == "-0")
                             _valFlag = "-";
                         Int64 strNumberWithoutDecimals = Convert.ToInt64(_val.Replace(",", ""));
                         string strNumberDecimals = value.Substring(value.IndexOf("."));
                         return $@"{_valFlag}{strNumberWithoutDecimals.ToString("#,##0")}{strNumberDecimals}";
                     }
                     return Convert.ToInt64(value.Replace(",", "")).ToString("#,##0");
                 }
             }
             catch (Exception ex)
             {

             }
             return value;        
        }

        public static string DPformateTWdate(this string value, string type = "400DB", char s = '/')
        {
            string result = string.Empty;

            var vals = value?.Split(s).ToList();
            int year = 0;
            int mounth = 0;
            int day = 0;
            if (vals!= null && vals.Count == 3 && Int32.TryParse(vals[0],out year) && Int32.TryParse(vals[1], out mounth) && Int32.TryParse(vals[2], out day))
            {
                if(type == "400DB")
                   result = $@"{year}{mounth.ToString().PadLeft(2,'0')}{day.ToString().PadLeft(2,'0')}";
                if (type == "OPENDB")
                    result = $@"{year + 1911}-{mounth.ToString().PadLeft(2, '0')}-{day.ToString().PadLeft(2, '0')}";
            }       
            return result;
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

        public static string strto400DB(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return string.Empty;
            return value;
        }

        public static string stringTWDateFormate(this string value)
        {
            if (!value.IsNullOrWhiteSpace() && value.Length >= 6)
            {
                string y = value.Substring(0, value.Length - 4);
                string m = value.Substring(value.Length - 4, 2);
                string d = value.Substring(value.Length - 2, 2);

                return $@"{y}/{m}/{d}";
            }
            return value;
        }

        public static string stringTimeFormate(this string value)
        {
            if (!value.IsNullOrWhiteSpace() && value.Length == 8)
            {
                return $@"{value.Substring(0,2)}:{value.Substring(2,2)}:{value.Substring(4,2)} {value.Substring(6,2)}";
            }
            return value;
        }
    }
}