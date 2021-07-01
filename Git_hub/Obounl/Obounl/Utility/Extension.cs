using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static Obounl.ObounlEnum.Ref;
using static Obounl.Utility.Log;


namespace Obounl.Utility
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

        /// <summary>
        /// ASE 加密 (Encrypt)
        /// </summary>
        /// <param name="str">待加密字串</param>
        /// <param name="Logger"></param>
        /// <param name="key">加密的key,如果沒有帶,預設為config參數</param>
        /// <returns>參數1(成功或失敗) 參數2(成功回傳加密後文字,失敗回傳失敗訊息)</returns>
        public static Tuple<bool, string> AESEncrypt(this string str, string Logger = null, bool log = true, string key = null)
        {
            bool flag = false;
            string encryptStr = string.Empty; //加密後字串
            if (str.IsNullOrWhiteSpace())
                return new Tuple<bool, string>(flag, encryptStr);
            try
            {
                string _key = key ?? System.Configuration.ConfigurationManager.AppSettings.Get("AESKey");
                if (log)
                {
                    NlogSet($@"待加密文字:{str}", Logger);
                    NlogSet($@"加密Key:{_key}", Logger);
                }
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(_key.Substring(0, (_key.Length > 32) ? 32 : _key.Length).PadLeft(32, '0'));
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(str);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                encryptStr = Convert.ToBase64String(resultArray, 0, resultArray.Length);
                flag = true;
                if (log)
                    NlogSet($@"AES加密成功:加密後文字:{encryptStr}", Logger);
            }
            catch (Exception ex)
            {
                var msg = ex.exceptionMessage();
                NlogSet($@"AES加密失敗:{msg}", Logger, Nlog.Error);
                encryptStr = msg;
            }
            return new Tuple<bool, string>(flag, encryptStr);
        }

        /// <summary>
        /// ASE 解密 (Decrypt)
        /// </summary>
        /// <param name="str">待解密字串</param>
        /// <param name="Logger"></param>
        /// <param name="key">解密的key,如果沒有帶,預設為config參數</param>
        /// <returns>參數1(成功或失敗) 參數2(成功回傳解密後文字,失敗回傳失敗訊息)</returns>
        public static Tuple<bool, string> AESDecrypt(this string str, string Logger = null, bool log = true, string key = null)
        {
            bool flag = false;
            string decryptStr = string.Empty; //解密後字串
            if (str.IsNullOrWhiteSpace())
                return new Tuple<bool, string>(flag, decryptStr);
            try
            {
                string _key = key ?? System.Configuration.ConfigurationManager.AppSettings.Get("AESKey");
                if (log)
                {
                    NlogSet($@"待解密文字:{str}", Logger);
                    NlogSet($@"解密Key:{_key}", Logger);
                }
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(_key.Substring(0, (_key.Length > 32) ? 32 : _key.Length).PadLeft(32, '0'));
                byte[] toDecryptArray = Convert.FromBase64String(str);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                decryptStr = UTF8Encoding.UTF8.GetString(resultArray);
                flag = true;
                if (log)
                    NlogSet($@"AES解密成功:解密後文字:{decryptStr}", Logger);
            }
            catch (Exception ex)
            {
                var msg = ex.exceptionMessage();
                NlogSet($@"AES解密失敗:{msg}", Logger, Nlog.Error);
                decryptStr = msg;
            }
            return new Tuple<bool, string>(flag, decryptStr);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="AStr"></param>
        /// <returns></returns>

        public static string Base64Encode(this string AStr)
        {
            if (AStr.IsNullOrWhiteSpace())
                return AStr;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(AStr));
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="ABase64"></param>
        /// <returns></returns>
        public static string Base64Decode(this string ABase64)
        {
            if (ABase64.IsNullOrWhiteSpace())
                return ABase64;
            return Encoding.UTF8.GetString(Convert.FromBase64String(ABase64));
        }

        /// <summary>
        /// 參數AES加密
        /// </summary>
        /// <param name="str">參數</param>
        /// <param name="EncryptFlag">加密判斷</param>
        /// <param name="Logger"></param>
        /// <returns></returns>
        public static string paramaterEncrypt(this string str, bool EncryptFlag, string Logger = null)
        {
            if (EncryptFlag)
            {
                var _result = str.AESEncrypt(Logger, false);
                return _result.Item1 ? _result.Item2 : str;
            }
            else
                return str;
        }

        public static string stringSetRequired(this string str)
        {
            return str ?? string.Empty;
        }

        public static Tuple<bool, string> getTokenKey()
        {
            string Token = ConfigurationManager.ConnectionStrings["TokenKey"]?.ConnectionString;
            DateTime dtn = DateTime.Now.AddMinutes(10);
            return dtn.ToString("yyyy/MM/dd HH:mm:dd").AESEncrypt(null, false);
        }

        public static string getDomain(this HttpRequestBase Request)
        {
            return Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
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
    }
}