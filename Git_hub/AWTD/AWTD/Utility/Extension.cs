using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWTD.Utility
{
    public static class Extension
    {
        /// <summary>
        /// 判斷文字是否為Null 或 空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// GetDescription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerationValue"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <returns></returns>
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


        public static string stringSetRequired(this string str)
        {
            return str ?? string.Empty;
        }

        public static string getAppSettings(this string str)
        {
            string _result = string.Empty;
            try
            {
                _result =  System.Configuration.ConfigurationManager.AppSettings.Get(str)?.Trim() ?? string.Empty;
            }
            catch
            { 
            
            }
            return _result;
        }
    }
}
