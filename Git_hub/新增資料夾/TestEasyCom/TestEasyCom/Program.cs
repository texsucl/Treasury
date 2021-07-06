using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEasyCom
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                #region easy
                //                EacConnection con = new EacConnection(ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString);
                //                con.Open();
                //                DateTime dtn = DateTime.Now;
                //                using (EacCommand ec = new EacCommand(con))
                //                {
                //                    string sql = string.Empty;
                //                    sql = $@"
                //select 
                //ACCT_CODE,
                //CURRENCY,
                //EFF_YY,
                //EFF_MM,
                //EFF_DD,
                //SOURCE_OP,
                //ITEM,
                //POLICY_NO,
                //POLICY_SEQ,
                //ID_DUP,
                //SYSTEM_OP,
                //FUND_CODE,
                //FEE_AMNT,
                //TOT_BONUS,
                //ACCT_YY,
                //ACCT_MM,
                //ACCT_DD,
                //DRAW_MARK,
                //TRAN_KIND
                //from FFDTXTT0
                //where 
                //(LPAD(ACCT_YY,3,'0') || LPAD(ACCT_MM,2,'0') || LPAD(ACCT_DD,2,'0'))  >= '1050101'
                //and (LPAD(ACCT_YY,3,'0') || LPAD(ACCT_MM,2,'0') || LPAD(ACCT_DD,2,'0'))  <= '1071231'
                //and EFF_YY >= '99'
                //and ACCT_CODE not in ('4611','4612')
                //and DRAW_MARK <> 'N'
                //";
                //                    ec.CommandText = sql;
                //                    ec.Prepare();
                //                    DbDataReader result = ec.ExecuteReader();
                //                    int _count = 0;
                //                    while (result.Read())
                //                    {
                //                        //string FAST_NO = result["FAST_NO"]?.ToString();
                //                        //string ACCT_CODE = result["ACCT_CODE"]?.ToString();
                //                        _count += 1;
                //                    }
                //                    ec.Dispose();
                //                    Console.WriteLine(_count);
                //                    Console.WriteLine(dtn - DateTime.Now);
                //                    Console.ReadLine();

                //                }
                #endregion

                Console.WriteLine("Start");
                string sql = string.Empty;
                #region get GLSIEXT config value
                List<CONFIG> Data_CONFIG_datas = new List<CONFIG>();
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GLSIEXT"].ConnectionString))
                {
                    conn.Open();
                    sql = $@"
select prog_id,para_id,para_value from [GLSIEXT].[dbo].[CONFIG]
where prog_id = 'XXX279350734510QC'
and para_id in ('AS400_ConnStr','AS400_Library') ;
";
                    using (SqlCommand com = new SqlCommand(sql, conn))
                    {
                        SqlDataReader reader = com.ExecuteReader();
                        while (reader.Read())
                        {

                            CONFIG config = new CONFIG()
                            {
                                prog_id = Convert.ToString(reader["prog_id"]),
                                para_id = Convert.ToString(reader["para_id"]),
                                para_value = Convert.ToString(reader["para_value"]),
                            };
                            Data_CONFIG_datas.Add(config);
                        }
                        reader.Close();
                    }
                }
                string decryptStr = Data_CONFIG_datas.FirstOrDefault(x => x.para_id == "AS400_ConnStr")?.para_value ?? string.Empty;
                var Library = Data_CONFIG_datas.FirstOrDefault(x => x.para_id == "AS400_Library")?.para_value ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(Library))
                    Library = $@"{Library}.";
                string key = "3106131061";
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key.Substring(0, (key.Length > 32) ? 32 : key.Length).PadLeft(32, '0'));
                byte[] toDecryptArray = Convert.FromBase64String(decryptStr);
                System.Security.Cryptography.RijndaelManaged rDel = new System.Security.Cryptography.RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = System.Security.Cryptography.CipherMode.ECB;
                rDel.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                System.Security.Cryptography.ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                var _EasycomConn = UTF8Encoding.UTF8.GetString(resultArray);
                Console.WriteLine($@"EasycomConn:{_EasycomConn}");
                Console.WriteLine($@"Library:{Library}");
                #endregion
                #region easycom 連線測試
                using (EacConnection conn = new EacConnection(_EasycomConn))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        Console.WriteLine("開啟400連線");
                        sql = $@"
with temp as
(select 
((ACCT_YY + 1911)  || LPAD(ACCT_MM ,2,'0')) AS VOUCHER_DATE_TYPE,
ACCT_CODE,
CURRENCY AS ORI_CURR_CODE,
(EFF_YY + 1911 ) || '-' ||  LPAD(EFF_MM,2,'0') || '-' || LPAD(EFF_DD,2,'0') AS ASSEM_NO_187,
SOURCE_OP AS ASSEM_NO_023,
ITEM AS ASSEM_NO_020,
POLICY_NO AS ASSEM_NO_118,
POLICY_SEQ AS ASSEM_NO_119,
ID_DUP AS ASSEM_NO_120,
SYSTEM_OP AS ASSEM_NO_040,
FUND_CODE AS ASSEM_NO_155,
CASE WHEN TRAN_KIND = 'A01'
           THEN CNTAMNT
           WHEN TRAN_KIND = 'A04'
           THEN CNTAMNT
           WHEN TRAN_KIND = 'A02'
           THEN TOT_BONUS
           ELSE 
           FEE_AMNT
END  AS DR_CR_AMOUNT,
0 AS DR_CR_CURR_AMOUNT,
((ACCT_YY + 1911)  || '-' || LPAD(ACCT_MM ,2,'0') || '-' ||  LPAD(ACCT_DD ,2,'0')) AS VOUCHER_DATE,
DRAW_MARK , 
TRAN_KIND
from {Library}FFDTXTT0 
where ACCT_YY = '109'
and ACCT_MM = '01'
and EFF_YY >= '99'
and ACCT_CODE not in ('4611','4612')
and ACCT_CODE not like '4641%'
and DRAW_MARK <> 'N'
and (TRAN_KIND in ('A01','A02','A04') or TRAN_KIND like 'B%' )
)
select 
VOUCHER_DATE_TYPE,
ACCT_CODE,
ORI_CURR_CODE,
ASSEM_NO_187,
ASSEM_NO_023,
ASSEM_NO_020,
ASSEM_NO_118,
ASSEM_NO_119,
ASSEM_NO_120,
ASSEM_NO_040,
ASSEM_NO_155,
SUM(DR_CR_AMOUNT) AS DR_CR_AMOUNT,
0 AS DR_CR_CURR_AMOUNT,
VOUCHER_DATE,
DRAW_MARK ,
TRAN_KIND
from temp
group by VOUCHER_DATE_TYPE, ACCT_CODE,ORI_CURR_CODE ,ASSEM_NO_187,ASSEM_NO_023, ASSEM_NO_020,ASSEM_NO_118, ASSEM_NO_119,ASSEM_NO_120,ASSEM_NO_040,ASSEM_NO_155,VOUCHER_DATE,DRAW_MARK ,TRAN_KIND ;
";


                        com.CommandText = sql;
                        com.Prepare();
                        Console.WriteLine("開啟執行400");
                        DbDataReader result = com.ExecuteReader();
                        int count = 0;
                        while (result.Read())
                        {
                            count += 1;
                        }
                        com.Dispose();
                        Console.WriteLine($@"查詢資料:{count}筆");
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("done!");
                Console.ReadLine();
            }
        }

    }

    class CONFIG
    { 
        public string prog_id { get; set; }
        public string para_id { get; set; }
        public string para_value { get; set; }
    }
}
