using FAP.Web.BO;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Mvc;
namespace FAP.Web.BO
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
                    output = policyNo.Substring(0, 4) + "***" + policyNo.Substring(7);
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
                output = id.Substring(0, 4) + "***" + id.Substring(7);
            else
                output = id;
            return output;
        }


        /// <summary>
        /// 萊斯系統的客戶編號
        /// </summary>
        /// <param name="cinNo"></param>
        /// <returns></returns>
        public static string maskAmlCinNo(string _cin_no)
        {
            string output = "";

            string[] cinNo = _cin_no.Split('-');
            try
            {
                output = cinNo[0] + "-" + cinNo[1].Substring(0, 4) + "***" + cinNo[1].Substring(7);
            }
            catch {
                output = _cin_no;
            }


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
            else if (StringUtil.toString(bankAct).Length <= 7)
                output = bankAct.Substring(0, 3) + "****";
            else
                output = bankAct.Substring(0, 3) + "****" + bankAct.Substring(7);


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
            else if (StringUtil.toString(name).Length <= 4)
                output = name.Substring(0, 1) + "*" + name.Substring(2);
            else
                output = name.Substring(0, 2) + "***" + name.Substring(5);

            return output;
        }

    }
}