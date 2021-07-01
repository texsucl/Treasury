using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSO.Web.Utils
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

    }
}