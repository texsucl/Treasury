using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class FAPPPASModel
    {
        //客戶編號
        public string cin_no { get; set; }

        //查詢單位   
        public string unit { get; set; }

        //查詢來源  
        public string source_id { get; set; }

        //給付對象 ID   
        public string paid_id { get; set; }

        //給付對象 ID   
        public string appl_id { get; set; }

        //給付對象姓名     
        public string paid_name { get; set; }

        //查詢者 ID  
        public string query_id { get; set; }

        //執行結果代碼
        public string rtn_code { get; set; }

        //疑似黑名單分類  
        public string is_san { get; set; }

        //案件狀態 
        public string status { get; set; }

        //登錄日期 
        public string entry_dt { get; set; }


        //登錄時間 
        public string entry_tm { get; set; }

        //註銷註記
        public string cancel_mk { get; set; }

        //註銷日期 
        public string cancel_dt { get; set; }

        //註銷時間 
        public string cancel_tm { get; set; }


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public FAPPPASModel()
        {
            cin_no = "";
            unit = "";
            source_id = "";
            paid_id = "";
            paid_name = "";
            appl_id = "";
            query_id = "";
            rtn_code = "";
            is_san = "";
            status = "";
            entry_dt = "";
            entry_tm = "";
            cancel_mk = "";
            cancel_dt = "";
            cancel_tm = "";
        }
    }
}