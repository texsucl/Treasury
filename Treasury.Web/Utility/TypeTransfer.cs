using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Treasury.WebUtility
{
    public static class TypeTransfer
    {

        #region String To Int?

        /// <summary>
        /// string 轉 Int?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? stringToIntN(string value)
        {
            int i = 0;
            if (value.IsNullOrWhiteSpace())
                return null;
            if (Int32.TryParse(value, out i))
                return i;
            return null;
        }

        #endregion String To Int?

        #region String To Int

        /// <summary>
        /// string 轉 Int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int stringToInt(string value)
        {
            int i = 0;
            if (value.IsNullOrWhiteSpace())
                return i;
            Int32.TryParse(value, out i);
            return i;
        }

        #endregion String To Int

        #region String To Double

        /// <summary>
        /// string 轉 Double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double stringToDouble(string value)
        {
            double d = 0d;
            double.TryParse(value, out d);
            return d;
        }

        #endregion String To Double

        #region  Decimal? To String

        /// <summary>
        /// Decimal 轉 String
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string decimalNToString(decimal? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }

        #endregion String To Decimal

        #region String To Decimal

        /// <summary>
        /// string 轉 Decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal stringToDecimal(string value)
        {
            decimal d = 0;
            decimal.TryParse(value, out d);
            return d;
        }

        #endregion String To Decimal

        #region String To Double?

        /// <summary>
        /// string 轉 Double?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? stringToDoubleN(string value)
        {
            double d = 0d;
            if (double.TryParse(value, out d))
                return d;
            return null;
        }

        #endregion String To Double?

        #region string (XX.XX%) To Double?

        /// <summary>
        /// string (XX.XX%) To Double?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? stringToDoubleNByP(string value)
        {
            double d = 0d;
            if (!value.IsNullOrWhiteSpace() &&
                value.EndsWith("%") &&
                double.TryParse(value.Split('%')[0], out d))
                return d / 100;
            return null;
        }

        #endregion string (XX.XX%) To Double?

        #region String To DateTime?

        /// <summary>
        /// string 轉 DateTime?
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <param name="_bool"></param>
        /// <returns></returns>
        public static DateTime? stringToDateTimeN(string value, int i = 10, bool _bool = false)
        {DateTime t = new DateTime();
            if(!_bool){

            if (i == 8 && !value.IsNullOrWhiteSpace() && value.Length == i)
            {
                if (DateTime.TryParse(string.Format(
                    "{0}/{1}/{2}",
                    value.Substring(0, 4),
                    value.Substring(4, 2),
                    value.Substring(6, 2)), out t))
                    return t;
                _bool=true;
            }
                
            }
            
            if(i == 10 && !value.IsNullOrWhiteSpace() && value.Substring(4,1).Equals("/"))
            {
            if (DateTime.TryParse(string.Format(
                "{0}-{1}-{2}",
                value.Substring(0,4),
                value.Substring(5,2),
                value.Substring(8,2)), out t))
                return t;
        }
            if (DateTime.TryParse(value, out t))
                return t;
            return null;
        }

        #endregion String To DateTime?

        #region String To DateTime

        /// <summary>
        /// string 轉 DateTime
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime stringToDateTime(string value)
        {
            DateTime t = DateTime.MinValue;
            DateTime.TryParse(value, out t);
            return t;
        }

        #endregion String To DateTime

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

        #endregion 民國字串轉西元年

        #region Int? To String

        /// <summary>
        /// Int? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string intNToString(int? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }

        #endregion Int? To String

        #region Int? To Int

        /// <summary>
        /// Int? 轉 Int (null 回傳 0)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int intNToInt(int? value)
        {
            if (value.HasValue)
                return value.Value;
            return 0;
        }

        #endregion Int? To Int

        #region Double? To String

        /// <summary>
        /// Double? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string doubleNToString(double? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }

        #endregion Double? To String

        #region Double? To Double

        /// <summary>
        /// Double? 轉 Double (null 回傳 0d)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double doubleNToDouble(double? value)
        {
            if (value.HasValue)
                return value.Value;
            return 0d;
        }

        #endregion Double? To Double


        #region Double To Decimal

        /// <summary>
        /// Double 轉 Decimal (null 回傳 0d)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal doubleToDecimal(double value)
        {
            return Convert.ToDecimal(value);
        }

        #endregion Double To Decimal

        #region Double? To Decimal

        /// <summary>
        /// Double? 轉 Decimal (null 回傳 0d)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal doubleNToDecimal(double? value)
        {
            if (value.HasValue)
                return Convert.ToDecimal(value.Value);
            return 0;
        }

        #endregion Double? To Decimal

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

        #region DateTime? & TimeSpan? 轉 string
        public static string dateTimeNTimeSpanNToString(DateTime? _date, TimeSpan? _value)
        {
            return $"{_date?.Date.ToString("yyyy/MM/dd")} {_value?.timeSpanToStr()}";
        }
        #endregion

        #region DateTime To String

        public static string dateTimeToString(DateTime value,bool all = true)
        {
            if (all)
                return value.ToString("yyyy/MM/dd HH:mm:ss");
            else
                return value.ToString("yyyy/MM/dd");
        }

        #endregion

        #region obj To String

        /// <summary>
        /// object 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string objToString(object value)
        {
            if (value != null)
                return value.ToString();
            return string.Empty;
        }

        #endregion obj To String

        public static int objToInt(object value)
        {
            int i = 0;
            if (value != null)
                int.TryParse(value.ToString(), out i);
            return i;
        }

        #region obj To double

        /// <summary>
        /// object 轉 double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double objToDouble(object value)
        {
            double d = 0d;
            if (value != null)
                double.TryParse(value.ToString(), out d);
            return d;
        }

        #endregion obj To double

        #region obj To double?  (轉string包含%)

        /// <summary>
        /// object 轉 double? (轉string包含%)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? objToDoubleNByP(object value)
        {
            double d = 0d;
            if (value != null)
                if (value.ToString().EndsWith("%"))
                    if (double.TryParse(value.ToString().Split('%')[0], out d))
                        return d / 100;
            return null;
        }

        #endregion obj To double?  (轉string包含%)

        #region objDataToString(yyyy/MM/dd)

        public static string objDateToString(object value)
        {
            DateTime date = DateTime.MinValue;
            if (value != null && DateTime.TryParse(value.ToString(), out date))
                return date.ToString("yyyy/MM/dd");
            return string.Empty;
        }

        #endregion objDataToString(yyyy/MM/dd)

        #region objDataToDateTimeN(yyyy/MM/dd)

        public static DateTime? objDateToDateTimeN(object value)
        {
            DateTime? date = null;
            DateTime _date = DateTime.MinValue;
            if (value != null && DateTime.TryParse(value.ToString(), out _date))
                 date = _date;
            return date;
        }

        #endregion objDataToDateTimeN(yyyy/MM/dd)

        #region objDataToDateTime(yyyy/MM/dd)

        public static DateTime objDateToDateTime(object value)
        {
            DateTime _date = DateTime.MinValue;
            if (value != null)
                DateTime.TryParse(value.ToString(), out _date);
            return _date;
        }

        #endregion objDataToDateTime(yyyy/MM/dd)

        #region data To JsonString

        public static string dataToJson<T>(List<T> datas)
        {
            return new JavaScriptSerializer().Serialize(datas);
        }

        #endregion data To JsonString

        #region DoubleN multiplication

        public static double? DoubleNMultip(double? d1, double? d2)
        {
            if (d1.HasValue && d2.HasValue)
            {
                return d1.Value * d2.Value;
            }
            return null;
        }

        #endregion DoubleN multiplication
    }
}