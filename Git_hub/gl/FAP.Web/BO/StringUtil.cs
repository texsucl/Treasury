using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FAP.Web.BO
{
    public static class StringUtil
    {
        public static string transToChtNumber(Int64 number, bool bUnit) {

            string[] chineseNumber = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] unit = { "", "十", "百", "千", "萬", "十萬", "百萬", "千萬", "億", "十億", "百億", "千億", "兆", "十兆", "百兆", "千兆" };
            StringBuilder ret = new StringBuilder();
            string inputNumber = number.ToString();
            int idx = inputNumber.Length;
            bool needAppendZero = false;
            foreach (char c in inputNumber)
            {
                idx--;
                if (c > '0')
                {
                    if (needAppendZero)
                    {
                        ret.Append(chineseNumber[0]);
                        needAppendZero = false;
                    }

                    if(bUnit)
                        ret.Append(chineseNumber[(int)(c - '0')] + unit[idx]);
                    else
                        ret.Append(chineseNumber[(int)(c - '0')]);
                }
                else
                    needAppendZero = true;
            }
            return ret.Length == 0 ? chineseNumber[0] : ret.ToString();

        }



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

    }
}