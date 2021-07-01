using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FGL.Web.BO
{
    public static class StringUtil
    {

        /// <summary>
        /// 判斷字串是否為空值或NULL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool isEmpty(String input) {
            bool bEmpty = true;
            if (input != null) {
                if (!"".Equals(input.Trim()))
                    bEmpty = false;
            }
            
            return bEmpty;

        }

        public static String toString(String input) {
            string result = input;

            if (input == null)
                result = "";


            return result.TrimEnd();
        }


        //半型轉全型
        public static string halfToFull(string strInput)
        {
            if (strInput == null)
                return null;


            //var temp = "";
            char[] c = strInput.ToCharArray();

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


        ///<summary>
        ///字串轉半形
        ///</summary>
        ///<paramname="input">任一字元串</param>
        ///<returns>半形字元串</returns>
        private static string halfToNarrow(string input)
        {
            if (input == null)
                return null;

            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }
    }
}