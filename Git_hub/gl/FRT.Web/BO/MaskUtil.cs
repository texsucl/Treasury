using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Mvc;
namespace FRT.Web.BO
{
    public static class MaskUtil
    {
        /// <summary>
        /// 保單號碼
        /// </summary>
        /// <param name="policyNo"></param>
        /// <returns></returns>
        public static string maskPolicyNo(string policyNo) {
            string output = "";

            if (StringUtil.toString(policyNo).Length == 10) {
                ValidateUtil validateUtil = new ValidateUtil();
                if(validateUtil.IsNum(policyNo))
                    output = policyNo;
                else
                    output = policyNo.Substring(0, 2) + "****" + policyNo.Substring(7);
            }
            else
                output = policyNo;


            return output;
        }

        /// <summary>
        /// 身份證字號
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string maskId(string id)
        {
            string output = "";

            if (StringUtil.toString(id).Length == 10)
                output = id.Substring(0, 2) + "****" + id.Substring(7);
            else
                output = id;
            return output;
        }


        /// <summary>
        /// 銀行帳號
        /// </summary>
        /// <param name="bankAct"></param>
        /// <returns></returns>
        public static string maskBankAct(string bankAct)
        {
            string output = "";

            if (StringUtil.toString(bankAct).Length < 4)
                output = bankAct;
            else if (StringUtil.toString(bankAct).Length < 7)
                output = bankAct.Substring(0, bankAct.Length - 4) + "****";
            else
                output = bankAct.Substring(0, bankAct.Length - 7) + "****" + bankAct.Substring(bankAct.Length - 2);


            return output;
        }


        /// <summary>
        /// 姓名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string maskName(string name)
        {
            string output = "";

            if (StringUtil.toString(name).Length < 2)
                output = name;
            else
                output = name.Substring(0, 1) + "*" + name.Substring(2);


            return output;
        }

    }
}