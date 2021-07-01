using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.BO
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


            return result.Trim();
        }

        /// <summary>
        /// 判斷全型字
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool FullWidthWord(String values)
        {
            bool result = false;

            if (!"".Equals(StringUtil.toString(values)))
            {
                string temp = halfToFull(values);
                if (!temp.Equals(values))
                    result = false;
                else
                    result = true;
            }
            else
                result = true;

            return result;


        }



        //半型轉全型
        public static string halfToFull(string strInput)
        {
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

    }
}