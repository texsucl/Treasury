using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class OAP0001PYCK
    {
        //案號
        public string changeId { get; set; }

        //支票金額
        public string checkAmt { get; set; }

        //支票號碼/匯費序號
        public string checkNo { get; set; }

        //支票帳號簡稱
        public string checkShrt { get; set; }

        //支票到期日
        public string checkDate { get; set; }

        //支票狀態
        public string checkStat { get; set; }

        //重開票原因
        public string reCkF { get; set; }

        //資料作廢原因
        public string delCode { get; set; }

        //給付帳務日
        public string sqlVhrdt { get; set; }

        //給付對象姓名
        public string receiver { get; set; }

        public OAP0001PYCK()
        {
            changeId = "";
            checkAmt = "";
            checkNo = "";
            checkShrt = "";
            checkDate = "";
            checkStat = "";
            checkStat = "";
            reCkF = "";
            delCode = "";
            sqlVhrdt = "";
            receiver = "";
        }
    }
}