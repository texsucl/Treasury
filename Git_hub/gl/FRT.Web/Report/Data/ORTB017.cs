using FRT.Web.BO;
using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Report.Data
{
    public class ORTB017 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            string fillrt_20 = parms.Where(x => x.key == "fillrt_20").FirstOrDefault()?.value ?? string.Empty;

            string EXPORT_BANK_NAME = "北富銀敦南";
            string EXPORT_NO = "00737102710668";
            var remit_date = string.Empty;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _FBO = db.FRT_FBO.AsNoTracking().First(x => x.remit_transfer_no == fillrt_20 && x.recovery_flag != "Y");
                remit_date = $@"{(_FBO.remit_date.Year - 1911)}/{_FBO.remit_date.Month.ToString().PadLeft(2, '0')}/{_FBO.remit_date.Day.ToString().PadLeft(2, '0')}";
            }
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = string.Empty;

//                    sql = $@"
//select 
//'{remit_date}' AS REMIT_DATE, --匯出時間
//FILLER_20, --轉檔批號
//FAST_NO, --快速付款編號
//'{EXPORT_BANK_NAME}' AS EXPORT_BANK_NAME, --匯出銀行
//'{EXPORT_NO}' AS EXPORT_NO, --匯出帳號
//RCV_NAME, --收款人戶名
//PAID_ID, --收款人ID
//(BANK_CODE || SUB_BANK) AS BANK_NO, --銀行代號
//BANK_ACT, --帳號
//CURRENCY, --幣別
//REMIT_AMT --金額
//from LRTBARM1
//where FILLER_20 = :FILLER_20 ;
//";

                    sql = $@"
select 
'{remit_date}' AS REMIT_DATE, 
TRIM(REPLACE(FILLER_20, '*', '')) FILLER_20,  
FAST_NO, 
'{EXPORT_BANK_NAME}' AS EXPORT_BANK_NAME,
'{EXPORT_NO}' AS EXPORT_NO,
RCV_NAME, 
PAID_ID, 
(BANK_CODE || SUB_BANK) AS BANK_NO,
BANK_ACT,
CURRENCY, 
REMIT_AMT 
from LRTBARM1
where FILLER_20 = :FILLER_20 ;
";
                    //com.Parameters.Add("REMIT_DATE", remit_date);
                    com.Parameters.Add("FILLER_20", fillrt_20);
                    //com.Parameters.Add("EXPORT_BANK_NAME", EXPORT_BANK_NAME);
                    //com.Parameters.Add("EXPORT_NO", EXPORT_NO);
                    com.CommandText = sql;
                    com.Prepare();
                    var adapter = new EacDataAdapter();
                    adapter.SelectCommand = com;
                    adapter.Fill(resultsTable);
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }

            return resultsTable;
        }
    }
}