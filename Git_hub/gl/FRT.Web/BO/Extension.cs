using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace FRT.Web.BO
{
    public static class Extension
    {
        public static string CustomCHECK_FORMULA(this string value)
        {
            string result = string.Empty;
            if (value.IsNullOrWhiteSpace())
                return result;
            switch (value.Trim())
            {
                case "富邦人壽紅利":
                case "富邦人壽年金":
                case "富邦人壽投資收益":
                case "富邦人壽保貸":
                case "富邦人壽解約":
                    result = "富邦人壽";
                    break;
                default:
                    result = value.Trim();
                    break;
            }
            return result;
        }

        public static string CustomPadLeft(this string value,int totalWidth, char paddingChar = ' ',bool wideFlag = false)
        {
            var _FullWidthWord = value.FullWidthWord(wideFlag);
            if (_FullWidthWord > 0)
                return string.Empty.PadLeft(((totalWidth - (_FullWidthWord * 2)) > 0 ? (totalWidth - (_FullWidthWord * 2)) : 0), paddingChar) + (wideFlag ? value.Trim().ToWide() :value.Trim());
            return value.IsNullOrWhiteSpace() ? " ".PadLeft(totalWidth, paddingChar) :  (wideFlag ? value.Trim().ToWide().PadLeft(totalWidth, paddingChar) : value.Trim().PadLeft(totalWidth, paddingChar));
        }

        public static string CustomPadRight(this string value, int totalWidth, char paddingChar = ' ', bool wideFlag = false)
        {
            var _FullWidthWord = value.FullWidthWord(wideFlag);
            if (_FullWidthWord > 0)
                return (wideFlag ? value.Trim().ToWide() : value.Trim()) + string.Empty.PadLeft(((totalWidth - (_FullWidthWord * 2)) > 0 ? (totalWidth - (_FullWidthWord * 2)) : 0), paddingChar);
            return value.IsNullOrWhiteSpace() ? " ".PadRight(totalWidth, paddingChar) : (wideFlag ? value.Trim().ToWide().PadRight(totalWidth, paddingChar) : value.Trim().PadRight(totalWidth, paddingChar));
        }

        ///字串轉全形
        ///</summary>
        ///<param name="input">任一字元串</param>
        ///<returns>全形字元串</returns>
        public static string ToWide(this string input)
        {
            //半形轉全形：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                //全形空格為12288，半形空格為32
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static int FullWidthWord(this String values,bool wideFlag )
        {
            int result = 0;
            if (values.IsNullOrWhiteSpace() || !wideFlag)
                return result;
            //string pattern = @"^[\u4E00-\u9fa5]+$";
            //foreach (char item in values)
            //{
            //    //以Regex判斷是否為中文字，中文字視為全形
            //    if (!Regex.IsMatch(item.ToString(), pattern))
            //    {
            //        //以16進位值長度判斷是否為全形字
            //        if (string.Format("{0:X}", Convert.ToInt32(item)).Length != 2)
            //        {
            //            result += 1;
            //        }
            //    }
            //    else
            //        result += 1;
            //}
            result = values.Trim().Length;
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

        public static decimal stringToDecimal(this string value)
        {
            decimal d = 0m;

            try
            {
                var _d = Convert.ToDecimal(value);
                d = _d;
            }
            catch{}
            return d;
        }

        public static Nullable<decimal> stringToDecimalN(this string value)
        {
            decimal d = 0m;

            try
            {
                var _d = Convert.ToDecimal(value?.Trim());
                d = _d;
                return d;
            }
            catch { }
            return null;
        }

        public static string formateThousand(this decimal? value, string defaultValue = "0")
        {
            return value?.ToString()?.formateThousand() ?? defaultValue;
        }

        public static string formateThousand(this int value, string curr = "")
        {
            return value.ToString().formateThousand(curr);
        }

        public static string formateThousand(this string value, string curr = "")
        {
            decimal d = 0;
            try
            {
                if (decimal.TryParse(value, out d))
                {
                    if (value.IndexOf(".") > -1)
                    {
                        Int64 strNumberWithoutDecimals = Convert.ToInt64(value.Substring(0, value.IndexOf(".")).Replace(",", ""));
                        string strNumberDecimals = value.Substring(value.IndexOf("."));
                        if (curr == "NTD")
                            return strNumberWithoutDecimals.ToString("#,##0");
                        else
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

        public static string formateThousand(this decimal value, string curr = "")
        {
            return value.ToString("F", System.Globalization.CultureInfo.InvariantCulture).formateThousand(curr);
        }

        public static int stringToInt(this string value)
        {
            int i = 0;
            if (value.IsNullOrWhiteSpace())
                return i;
            Int32.TryParse(value, out i);
            return i;
        }

        //public class SelectOption
        //{
        //    public string text { get; set; }
        //    public string value { get; set; }
        //}

        public static string getValidateString(this IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> errors)
        {
            string result = string.Empty;
            if (errors.Any())
                result = string.Join(" ", errors.Select(y => string.Join(",", y.ValidationErrors.Select(z => z.ErrorMessage))));
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

        public static string dateToString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        public static string dateFormat(this string str)
        {
            if (!str.IsNullOrWhiteSpace())
                return str.Replace("-", "/");
            return str;
        }

        public static string dateToStringT(this DateTime dt)
        {
            return $@"{dt.Year - 1911}{dt.ToString("MMdd")}";
        }

        public static string stringCheckDate(this string str)
        {
            DateTime dt = DateTime.MinValue;
            if (DateTime.TryParse(str, out dt))
            {
                return dt.dateToStringT();
            }
            else
            {
                return string.Empty;
            }
        }

        public static DateTime stringToDate(this string str)
        {
            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(str,out dt);
            return dt;
        }

        public static string TWDateToDate(this string str)
        {
            int i = 0;
            if (str.IsNullOrWhiteSpace())
                return str;
            str = str.Trim();
            if (str.Length == 6 && Int32.TryParse(str, out i))
                return $@"{1911 + Convert.ToInt32(str.Substring(0, 2))}-{str.Substring(2, 2)}-{str.Substring(4, 2)}";
            else if (str.Length == 7 && Int32.TryParse(str, out i))
                return $@"{1911 + Convert.ToInt32(str.Substring(0, 3))}-{str.Substring(3, 2)}-{str.Substring(5, 2)}";
            else
                return str;
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

        /// <summary>
        /// 勾稽報表 timeout 設定
        /// </summary>
        /// <returns></returns>
        public static int getTimeout()
        {
            int _i = 3600;
            int _t = 0;
            try
            {
                if (Int32.TryParse(ConfigurationManager.AppSettings.Get("TIMEOUT"), out _t))
                {
                    _i = _t;
                }
            }
            catch
            { }
            return _i;
        }

    }
}