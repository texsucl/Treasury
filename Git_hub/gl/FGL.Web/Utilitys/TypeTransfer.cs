using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FGL.Web.Utilitys
{
    public static class TypeTransfer
    {
        #region DateTime? To String
        /// <summary>
        /// DateTime? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string dateTimeNToString(DateTime? value, int number = 10)
        {
            if (value.HasValue)
                return 8.Equals(number) ?
                    value.Value.ToString("yyyyMMdd") :
                    value.Value.ToString("yyyy/MM/dd");
            return string.Empty;
        }

        #endregion DateTime? To String

        #region DateTime? To String
        /// <summary>
        /// DateTime? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string dateTimeNToStringNT(DateTime? value)
        {
            var twCalendar = new System.Globalization.TaiwanCalendar();

            DateTime dt = value ?? DateTime.Now;

            if (value.HasValue)
                return string.Format("{0}/{1}/{2}", twCalendar.GetYear(dt), dt.Month.ToString().PadLeft(2, '0'), dt.Day.ToString().PadLeft(2, '0'));

            return string.Empty;
        }
        #endregion DateTime? To String

        #region DateTime? To String
        /// <summary>
        /// DateTime? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string dateTimeNToStringNT(DateTime? value, string time)
        {
            var twCalendar = new System.Globalization.TaiwanCalendar();

            DateTime dt = value ?? DateTime.Now;

            time = dt.ToString("HH:mm:ss");

            if (value.HasValue)
                return string.Format("{0}/{1}/{2} {3}", twCalendar.GetYear(dt), dt.Month.ToString().PadLeft(2, '0'), dt.Day.ToString().PadLeft(2, '0'), time);

            return string.Empty;
        }
        #endregion DateTime? To String

        #region 民國字串轉西元年
        /// <summary>
        /// 民國字串轉西元年
        /// </summary>
        /// <param name="value">1051119 to 2016/11/19</param>
        /// <returns></returns>
        public static DateTime? stringToADDateTimeN(string value)
        {
            if (!value.IsNullOrWhiteSpace() && value.Length >= 6)
            {
                value = value?.Replace("/", string.Empty);
                string y = value.Substring(0, value.Length - 4);
                string m = value.Substring(value.Length - 4, 2);
                string d = value.Substring(value.Length - 2, 2);

                int ady = 0;
                Int32.TryParse(y, out ady);
                ady += 1911;
                y = ady.ToString();

                DateTime t = new DateTime();
                if (DateTime.TryParse($"{y}/{m}/{d}", out t))
                    return t;
                return null;
            }
            return null;
        }
        #endregion DateTime? To String
    }
}