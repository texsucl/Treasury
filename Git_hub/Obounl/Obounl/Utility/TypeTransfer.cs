using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obounl.Utility
{
    public class TypeTransfer
    {
        #region String To DateTime?

        /// <summary>
        /// string 轉 DateTime?
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <param name="_bool"></param>
        /// <returns></returns>
        public static DateTime? stringToDateTimeN(string value, int i = 10, bool _bool = false)
        {
            DateTime t = new DateTime();
            if (!_bool)
            {

                if (i == 8 && !value.IsNullOrWhiteSpace() && value.Length == i)
                {
                    if (DateTime.TryParse(string.Format(
                        "{0}/{1}/{2}",
                        value.Substring(0, 4),
                        value.Substring(4, 2),
                        value.Substring(6, 2)), out t))
                        return t;
                    _bool = true;
                }

            }

            if (i == 10 && !value.IsNullOrWhiteSpace() && value.Length >= 5 && value.Substring(4, 1).Equals("/"))
            {
                if (DateTime.TryParse(string.Format(
                    "{0}-{1}-{2}",
                    value.Substring(0, 4),
                    value.Substring(5, 2),
                    value.Substring(8, 2)), out t))
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

        #region String To Int

        /// <summary>
        /// string 轉 int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int stringToInt(string value)
        {
            int result = 0;
            if (value.IsNullOrWhiteSpace())
                return result;
            Int32.TryParse(value, out result);
            return result;
        }

        #endregion
    }
}