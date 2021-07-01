

using System;


/// ==============================================
/// 修改日期/修改人：20180123 黃黛鈺 
/// 需求單號：201801220384-00 
/// 修改內容：「年月」
///             每月15號(含15號)以前：以系統年月顯示。
///             每月16號後：以下一個年月顯示。
/// ==============================================
/// 
namespace FAP.Web.BO
{
    public static class DateUtil
    {
        public static String getCurChtDate(int yearLen)
        {
            String strFormat = "yyyyMMdd";
            DateTime now = DateTime.Now;

            String strDateTime = now.ToString(strFormat);
            strDateTime = (now.Year - 1911).ToString().PadLeft(yearLen, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

            return strDateTime;
        }

        public static String getCurChtDateTime(int yearLen)
        {
            String strFormat = "yyyyMMdd HHmmssff";
            DateTime now = DateTime.Now;

            String strDateTime = now.ToString(strFormat);
            strDateTime = (now.Year - 1911).ToString().PadLeft(yearLen, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

            return strDateTime;
        }

        public static String getCurDateTime(string strFormat) {
            if("".Equals(strFormat))
                strFormat = "yyyyMMdd HHmmss";

            string curDate = "";
            DateTime now = DateTime.Now;

            curDate = now.ToString(strFormat);
            return curDate;
        }


        public static String getCurChtDateTime()
        {
            String strFormat = "yyyyMMdd HHmmss";
            DateTime now = DateTime.Now;

            String strDateTime = now.ToString(strFormat);
            strDateTime = (now.Year - 1911).ToString().PadLeft(3, '0') + strDateTime.Substring(4, strDateTime.Length - 4);

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

        public static String formatDateTimeDbToSc(String input, String dateType) {
            String result = "";

            switch (dateType)
            {
                case "DT" :
                    if (input.Trim().Length == 15)
                    { 
                        result = input.Substring(0, 4) + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2) + " "
                            + input.Substring(9, 2) + ":" + input.Substring(11, 2) + ":" + input.Substring(13, 2);
                    }
                    else
                        result = input;

                    break;
                case "D":
                    if (input.Trim().Length == 8)
                        result = input.Substring(0, 4) + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2);
                    else
                        result = input;

                    break;
                case "T":
                    if (input.Trim().Length == 6)
                        result = input.Substring(0, 2) + ":" + input.Substring(2, 2) + ":" + input.Substring(4, 2);
                    else
                        result = input;

                    break;
                default:
                    break;
            }


                return result;
        }


        public static string DatetimeToString(DateTime ? dt, string format)
        {
            string strDt = "";

            if ("".Equals(StringUtil.toString(format)))
            {
                format = "yyyy/MM/dd HH:mm:ss";
            }

            if (dt.HasValue)
            {
                strDt = ((DateTime)dt).ToString(format);
            }

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
        public static string As400ChtDateToADDate(string input)
        {
            string strDate = "";
            if (input.Trim().Length < 6)
                strDate = input;
            else
            {
                input = input.PadLeft(7, '0');
                strDate = formatDateTimeDbToSc((Convert.ToInt16(input.Substring(0, input.Trim().Length - 4)) + 1911).ToString()
                    + input.Substring(input.Trim().Length - 4), "D");
            }


            return strDate;
        }





        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ADDateToChtDate(string input, int yearLen, string splitC)
        {
            string strDate = "";

            if (input.Contains("-") || input.Contains("/"))
            {
                DateTime dt = Convert.ToDateTime(input);
                strDate = (dt.Year - 1911).ToString().PadLeft(yearLen, '0') + splitC
                    + dt.Month.ToString().PadLeft(2, '0') + splitC
                    + dt.Day.ToString().PadLeft(2, '0');
            }
            else {
                if (input.Trim().Length < 8)
                    strDate = input;
                else
                {
                    strDate = (Convert.ToInt16(input.Substring(0, 4)) - 1911).ToString() + splitC
                        + input.Substring(4,2) + splitC + input.Substring(6);
                }
            }

            return strDate;
        }


        public static string ADDateToChtDate(DateTime dt, int yearLen, string splitC)
        {
            string strDate = "";
            strDate = (dt.Year - 1911).ToString().PadLeft(yearLen, '0') + splitC
                    + dt.Month.ToString().PadLeft(2, '0') + splitC
                    + dt.Day.ToString().PadLeft(2, '0');
           
            return strDate;
        }



        /// <summary>
        /// 民國年轉西元年
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime? ChtDateToADDate(string input, char splitC)
        {
            try
            {
                string[] dateArr = input.Split(splitC);

                string strDate = "";
                strDate = (Convert.ToInt16(dateArr[0]) + 1911) + "-" + dateArr[1] + "-" + dateArr[2];

                DateTime dt = Convert.ToDateTime(strDate);

                return dt;
            }
            catch (Exception e) {
                return null;
            }
            
        }
    }
}
