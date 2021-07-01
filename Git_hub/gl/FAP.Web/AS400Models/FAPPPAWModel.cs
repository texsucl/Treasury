using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class FAPPPAWModel
    {
        //信函類別
        public string report_tp { get; set; }

        //大系統別  
        public string system { get; set; }

        //群組  
        public string dept_group { get; set; }

        //給付對象 ID   
        public string paid_id { get; set; }

        //支票號碼    
        public string check_no { get; set; }

        //帳戶簡稱  
        public string check_shrt { get; set; }

        //DATA FLAG
        public string data_flag { get; set; }

        //信函寄出日 
        public string dept_date1 { get; set; }

        //郵遞區號 
        public string r_zip_code { get; set; }

        //地址 
        public string r_addr { get; set; }


        //登錄人員
        public string entry_id { get; set; }

        //登錄日期
        public string entry_date { get; set; }

        //登錄時間 
        public string entry_time { get; set; }
        

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public FAPPPAWModel()
        {
        }
    }
}