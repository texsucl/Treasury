﻿

using System;


/// ==============================================
/// 修改日期/修改人：20180123 黃黛鈺 
/// 需求單號：201801220384-00 
/// 修改內容：「年月」
///             每月15號(含15號)以前：以系統年月顯示。
///             每月16號後：以下一個年月顯示。
/// ==============================================
/// 
namespace FRT.Web.BO
{
    public static class DateUtil
    {

        public static String getCurDateTime(string strFormat) {
            if("".Equals(strFormat))
                strFormat = "yyyyMMdd HHmmss";

            string curDate = "";
            DateTime now = DateTime.Now;

            curDate = now.ToString(strFormat);
            return curDate;
        }


        public static String getCurChtDateTime(int yearLen)
        {
            String strFormat = "yyyyMMdd HHmmssff";
            DateTime now = DateTime.Now;

            String strDateTime = now.ToString(strFormat);
            strDateTime = (now.Year - 1911).ToString().PadLeft(yearLen, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

            return strDateTime;
        }

        public static DateTime getCurDateTime()
        {
            DateTime now = DateTime.Now;

            return now;
        }


        public static String getCurDate(string strFormat)
        {
            if ("".Equals(strFormat))
                strFormat = "yyyyMMdd";

            string curDate = "";
            DateTime now = DateTime.Now;

            curDate = now.ToString(strFormat);
            return curDate;
        }

       

        public static String getCurTime(string strFormat)
        {
            if ("".Equals(strFormat))
                strFormat = "HHmmss";

            string curDate = "";
            DateTime now = DateTime.Now;

            curDate = now.ToString(strFormat);
            return curDate;
        }

        public static String formatDateTimeDbToSc(string input, string dateType) {
            string result = "";
            input = StringUtil.toString(input);


            switch (dateType)
            {
                case "DT" :
                    if (input.Trim().Length == 15)
                    { 
                        result = input.Substring(1, 4);
                    result = input.Substring(12, 2);

                        result = input.Substring(0, 4) + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2) + " "
                            + input.Substring(9, 2) + ":" + input.Substring(11, 2) + ":" + input.Substring(13, 2);
                    }
                    else
                        result = input;

                    break;
                case "D":
                    if (input.Trim().Length == 8) {
                        result = input.Substring(0, 4) + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2);
                        if ("0".Equals(result.Substring(0, 1)))
                            result = result.Substring(1);
                    }
                        
                    else if(input.Trim().Length == 7)
                        result = input.Substring(0, 3) + "/" + input.Substring(3, 2) + "/" + input.Substring(5, 2);
                    else 
                    result = input;

                    break;
                case "T":
                    if (input.Trim().Length == 6)
                        result = input.Substring(0, 2) + ":" + input.Substring(2, 2) + ":" + input.Substring(4, 2);
                    else if (input.Trim().Length == 5) {
                        input = input.PadLeft(6, '0');
                        result = input.Substring(0, 2) + ":" + input.Substring(2, 2) + ":" + input.Substring(4, 2);
                    }
                        
                    else
                        result = input;

                    break;
                default:
                    break;
            }


                return result;
        }


        public static string DatetimeToString(DateTime dt, string format)
        {
            string strDt = "";

            if ("".Equals(StringUtil.toString(format)))
            {
                format = "yyyy/MM/dd HH:mm:ss";
            }

            DateTime d = dt;
            strDt = dt.ToString(format);

            return strDt;

        }


        public static DateTime stringToDatetime(string input)
        {
            DateTime tmp;
            DateTime.TryParse(input, out tmp);

            return tmp;
        }


        /// <summary>
        /// AS400的民國年轉西元年
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string As400ChtDateToADDate(string input) {
            string strDate = "";
            if (input.Trim().Length < 7)
                strDate = input;
            else {
                strDate = (Convert.ToInt16(input.Substring(0, input.Trim().Length - 4)) + 1911).ToString()
                    + input.Substring(input.Trim().Length - 4);
            }


            return strDate;
        }



        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ADDateToChtDate(string input)
        {
            input = input.Replace("-", "").Replace("/", "");
            string strDate = "";
            if (input.Trim().Length < 8)
                strDate = input;
            else
            {
                strDate = (Convert.ToInt16(input.Substring(0, 4)) - 1911).ToString()
                    + input.Substring(4);
            }


            return strDate;
        }
    }
}
