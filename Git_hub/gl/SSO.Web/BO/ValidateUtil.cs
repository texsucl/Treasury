using SSO.Web.Utils;
using System;
using System.Text.RegularExpressions;


/// <summary>
/// 功能說明：檢核程式
/// 初版作者：20170831 黃黛鈺
/// 修改歷程：20170831 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// ====================================================================
/// 修改日期/修改人：20180110 黃黛鈺 
/// 需求單號：201801100244-00
/// 修改內容：修改檢核單位邏輯錯誤問題(若單位第3碼為D時...要特別轉換)
/// ====================================================================
/// 修改日期/修改人：20180123 黃黛鈺 
/// 需求單號：201801220384-00
/// 修改內容：1.「身分證字號」不可有有小寫情形
///           2.「活動名稱」、「備註」不可有半形字元
/// ====================================================================
/// 修改歷程：20180128 黃黛鈺 
///           需求單號： 201801230413-00
///           加入電子發票相關檢核
/// </summary>
/// 
namespace SSO.Web.BO
{
    public class ValidateUtil
    {
        /// <summary>
        /// 判斷大寫英文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsBEG(String input)
        {
            Regex BEG = new Regex("[^A-Z]");
            return !BEG.IsMatch(input);
        }

        /// <summary>
        /// 判斷數字(正負及小數)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsDecimal(String input)
        {
            // Regex Num = new Regex("^[-?0-9]+(\.[0-9]{0,4})?$");
            Regex Num = new Regex("^[-?0-9] + (\\.[0-9])?$");
            //Regex Num = new Regex("^-?\\d+$");
            bool test = Num.IsMatch(input);

            try
            {
                Convert.ToDecimal(input);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            //return !Num.IsMatch(input);
        }

      


        public string chkDate(string input, string inputCol)
        {
            bool bPass = true;
            String err = "";

            if (StringUtil.isEmpty(input))
            {
                err += inputCol + "欄位-請務必輸入！";
                bPass = false;
            }


            if (input.Length != 8 || !IsNum(input))
            {
                err += inputCol + "欄位-需為8碼數字！";
                bPass = false;
            }

            if (bPass)
            {

                try
                {

                    Convert.ToDateTime(input.Substring(0, 4) + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2));
                }
                catch (Exception e)
                {
                    err += inputCol + "欄位- 需為日期格式！";
                    bPass = false;
                }
            }

            return err;
        }



        /// <summary>
        /// 檢核 agent 姓名
        /// </summary>
        /// <param name="cAgentName"></param>
        /// <returns></returns>
        public String chkAgentName(String cAgentName) {
            bool bPass = true;
            String err = "";

            if (StringUtil.isEmpty(cAgentName))
            {
                err += "“姓名”欄位-請務必輸入！";
                bPass = false;
            }

            if (bPass)
            {
                if (cAgentName.Length > 7) {
                    err += "“姓名”欄位-不能超過7個字符！";
                    bPass = false;
                }
            }


            return err;
        }

   


        /// <summary>
        /// 檢核年月(民國年)
        /// </summary>
        /// <param name="cYearMonth"></param>
        /// <param name="fileYearMonth"></param>
        /// <returns></returns>
        public String chkChtYM(String cYearMonth, String fileYearMonth)
        {
            bool bPass = true;
            String err = "";

            if (StringUtil.isEmpty(cYearMonth))
            {
                err += "”年月”欄位-請務必輸入！";
                bPass = false;
            }

            if (bPass) {
                if (cYearMonth.Length != 6)
                {
                    err += "“年月”欄位-請輸入正確的格式：民國年4碼+月份2碼(ex:0096年10月請輸入009610)！";
                    bPass = false;
                }
            }

            if (bPass)
            {
                if (!IsNum(cYearMonth)) {
                    err += "“年月”欄位-該年月不存在，請重新輸入正確年月！";
                    bPass = false;
                }
            }

            if (bPass)
            {
                int iMM = Convert.ToInt32(cYearMonth.Substring(cYearMonth.Trim().Length - 2, 2));
                if (!(iMM >= 1 && iMM <= 12)) {
                    err += "“年月”欄位-該年月不存在，請重新輸入正確年月！";
                    bPass = false;
                }
            }


            if (bPass)
            {
                if (!cYearMonth.Equals(fileYearMonth)) {
                    err += "“年月”欄位-與匯入檔案名稱不相符，請檢查資料並重新匯入！";
                    bPass = false;
                }
            }


                return err;
        }


        public bool chkChtYM(String cYearMonth)
        {
            bool bPass = true;

            if (StringUtil.isEmpty(cYearMonth))
                bPass = false;

            if (bPass)
            {
                if (cYearMonth.Length != 6)
                    bPass = false;
            }

            if (bPass)
            {
                if (!IsNum(cYearMonth))
                    bPass = false;
            }

            if (bPass)
            {
                int iMM = Convert.ToInt32(cYearMonth.Substring(cYearMonth.Trim().Length - 2, 2));
                if (!(iMM >= 1 && iMM <= 12))
                    bPass = false;
            }


            return bPass;
        }

        


        /// <summary>
        /// 判斷英數字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsNumandEG(String input) {
            Regex NumandEG = new Regex("[^A-Za-z0-9]");
            return !NumandEG.IsMatch(input);
        }


        /// <summary>
        /// 判斷數字或大寫英文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsNumandUEG(String input)
        {
            Regex NumandEG = new Regex("[^A-Z0-9]");
            return !NumandEG.IsMatch(input);
        }


        /// <summary>
        /// 判斷數字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsNum(String input)
        {
            Regex Num= new Regex("[^-?0-9]");
            //Regex Num = new Regex("^-?\\d+$");
            
            return !Num.IsMatch(input);
        }


        /// <summary>
        /// 判斷全型字
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool FullWidthWord(String values)
        {
            bool result = false;
            //modify by daiyu 20180123 
            if (!"".Equals(StringUtil.toString(values))) {
                string temp = halfToFull(values);
                if (!temp.Equals(values))
                    result = false;
                else
                    result = true;
            } else
                result = true;

            return result;

            //bool result = false;
            //string pattern = @"^[\u4E00-\u9fa5]+$";
            //foreach (char item in values)
            //{
            //    //以Regex判斷是否為中文字，中文字視為全形
            //    if (!Regex.IsMatch(item.ToString(), pattern))
            //    {
            //        //以16進位值長度判斷是否為全形字
            //        if (string.Format("{0:X}", Convert.ToInt32(item)).Length != 2)
            //        {
            //            result = true;
            //            break;
            //        }
            //        else {  //add by daiyu 20180123
            //            result = false;
            //            break;
            //        }
            //    }else
            //        result = true;
            //}
            //return result;
        }


        //半型轉全型
        public string halfToFull(string strInput)
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