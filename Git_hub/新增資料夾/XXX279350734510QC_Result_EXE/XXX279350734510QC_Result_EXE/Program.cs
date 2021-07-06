using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XXX279350734510QC_Result_EXE
{
    class Program
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string _conn = ConfigurationManager.ConnectionStrings["GLSIEXT"].ConnectionString;

        static void Main(string[] args)
        {
            if (!string.IsNullOrWhiteSpace(args[0]))
            {
                logger.Info($@"value:{args[0]}");
                List<string> VDs = args[0].Split(',').ToList();

                var successFlag = false;
                try
                {
                    string sql = string.Empty;
                    string strparm = string.Empty;
                    string _p = string.Empty;

                    List<CONFIG> Data_CONFIG_datas = new List<CONFIG>();
                    using (SqlConnection conn = new SqlConnection(_conn))
                    {
                        using (SqlCommand com = new SqlCommand())
                        {
                            for (var i = 0; i < VDs.Count; i++)
                            {
                                strparm += $@" {_p} @VOUCHER_DATE_TYPE{i} ";
                                _p = ",";
                                com.Parameters.AddWithValue($"VOUCHER_DATE_TYPE{i}", VDs[i]);
                            }
                            sql = $@"
delete [GLSIEXT].[dbo].[XXX279350734511_Detail]
where VOUCHER_DATE_TYPE in ({strparm}) 
and DB_TYPE = 'A' ;
";
                            com.Connection = conn;
                            com.CommandText = sql;
                            com.CommandTimeout = 36000;
                            var _delete = com.ExecuteNonQuery();
                            logger.Info($@"delete XXX279350734511_Detail count :{_delete}");
                        }

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

                    successFlag = true;

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
                    logger.Info("連線字串解密OK");

                    foreach (var dt in VDs)
                    {
                        bool _ch = false;
                        try
                        {
                            List<XXX279350734511_Detail> datas = new List<XXX279350734511_Detail>();
                            using (EacConnection conn = new EacConnection(_EasycomConn))
                            {
                                conn.Open();
                                using (EacCommand com = new EacCommand(conn))
                                {
                                    logger.Info("開啟400連線");
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
where ACCT_YY = :ACCT_YY
and ACCT_MM = :ACCT_MM
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


                                    com.Parameters.Add("ACCT_YY", (Convert.ToInt32(dt.Substring(0, 4)) - 1911).ToString());
                                    com.Parameters.Add("ACCT_MM", dt.Substring(4, 2));

                                    com.CommandText = sql;
                                    com.Prepare();
                                    logger.Info("開啟執行400");
                                    DbDataReader result = com.ExecuteReader();
                                    logger.Info("執行400中");
                                    while (result.Read())
                                    {
                                        var _ACCT_CODE_TYPE = "B";
                                        switch (result["TRAN_KIND"]?.ToString()?.Trim())
                                        {
                                            case "A01":
                                            case "A02":
                                            case "A04":
                                                _ACCT_CODE_TYPE = "A";
                                                break;
                                        }
                                        var data = new XXX279350734511_Detail()
                                        {
                                            DB_TYPE = "A",
                                            ACCT_CODE_TYPE = _ACCT_CODE_TYPE,
                                            VOUCHER_DATE_TYPE = result["VOUCHER_DATE_TYPE"]?.ToString()?.Trim(),
                                            ASSEM_NO_008 = result["ACCT_CODE"]?.ToString()?.Trim(),
                                            ORI_CURR_CODE = result["ORI_CURR_CODE"]?.ToString()?.Trim(),
                                            ASSEM_NO_187 = stringToDataTimeN(result["ASSEM_NO_187"]?.ToString()?.Trim()),
                                            ASSEM_NO_023 = result["ASSEM_NO_023"]?.ToString()?.Trim(),
                                            ASSEM_NO_020 = result["ASSEM_NO_020"]?.ToString()?.Trim(),
                                            ASSEM_NO_118 = result["ASSEM_NO_118"]?.ToString()?.Trim(),
                                            ASSEM_NO_119 = result["ASSEM_NO_119"]?.ToString()?.Trim(),
                                            ASSEM_NO_120 = result["ASSEM_NO_120"]?.ToString()?.Trim(),
                                            ASSEM_NO_040 = result["ASSEM_NO_040"]?.ToString()?.Trim(),
                                            ASSEM_NO_155 = result["ASSEM_NO_155"]?.ToString()?.Trim(),
                                            DR_CR_AMOUNT = stringToDecimal(result["DR_CR_AMOUNT"]?.ToString()?.Trim()),
                                            DR_CR_CURR_AMOUNT = stringToDecimal(result["DR_CR_CURR_AMOUNT"]?.ToString()?.Trim()),
                                            VOUCHER_DATE = stringToDataTimeN(result["VOUCHER_DATE"]?.ToString()?.Trim()),
                                            DRAW_MARK = result["DRAW_MARK"]?.ToString()?.Trim(),
                                            TRAN_KIND = result["TRAN_KIND"]?.ToString()?.Trim(),
                                        };
                                        datas.Add(data);
                                    }
                                    com.Dispose();
                                    logger.Info($@"待新增資料:{datas.Count}筆");
                                }
                            }

                            if (datas.Any())
                            {
                                insertXXX279350734511_Detail(datas, 100);
                                updateStatus(new List<string>() { dt }, "A", STATUS.G);
                                _ch = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info(ex.ToString());
                        }
                        finally
                        {
                            if (!_ch)
                            {
                                updateStatus(new List<string>() { dt }, "A", STATUS.E);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!successFlag)
                    {
                         updateStatus(VDs, "W", STATUS.E);
                    }
                    logger.Info(ex.ToString());
                }

            }
        }

        static void updateStatus(List<string> VDs, string source_type, STATUS status, bool updateFlag = true)
        {
            string _strMSG = GetDescription(status);
            string sql = string.Empty;
            string str = " and VOUCHER_DATE_TYPE in ( ";
            string s = string.Empty;
            var dtn = DateTime.Now;
            List<XXX279350734510Status> XXX279350734510Status = new List<XXX279350734510Status>();
            using (SqlConnection conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (SqlCommand com = new SqlCommand())
                {                 
                    com.Parameters.AddWithValue("SOURCE_TYPE", source_type);
                    for (int i = 0; i < VDs.Count; i++)
                    {
                        str += $@" {s} @VOUCHER_DATE_TYPE{i} ";
                        s = ",";
                        com.Parameters.AddWithValue($"VOUCHER_DATE_TYPE{i}", VDs[i]);
                    }
                    str += " ) ";
                    sql = $@"
select 
VOUCHER_DATE_TYPE,
SOURCE_TYPE,
STATUS
from [GLSIEXT].[dbo].[XXX279350734510Status]
where SOURCE_TYPE = @SOURCE_TYPE
{str}
";
                    com.Connection = conn;
                    com.CommandText = sql;
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {

                        XXX279350734510Status _status = new XXX279350734510Status()
                        {
                            VOUCHER_DATE_TYPE = Convert.ToString(reader["VOUCHER_DATE_TYPE"]),
                            SOURCE_TYPE = Convert.ToString(reader["SOURCE_TYPE"]),
                            STATUS = Convert.ToString(reader["STATUS"]),
                        };
                        XXX279350734510Status.Add(_status);
                    }
                    reader.Close();
                }

                List<XXX279350734510Status> inserts = new List<XXX279350734510Status>();
                List<XXX279350734510Status> updates = new List<XXX279350734510Status>();
                foreach (var item in VDs)
                {
                    var single = XXX279350734510Status.FirstOrDefault(x => x.VOUCHER_DATE_TYPE == item);
                    if (single != null)
                    {
                        updates.Add(new XXX279350734510Status()
                        {
                            SOURCE_TYPE = source_type,
                            STATUS = _strMSG,
                            VOUCHER_DATE_TYPE = item
                        });
                    }
                    else
                    {
                        inserts.Add(new XXX279350734510Status()
                        {
                            SOURCE_TYPE = source_type,
                            STATUS = _strMSG,
                            VOUCHER_DATE_TYPE = item,
                            UPDATE_DATETIME = dtn
                        });
                    }
                }
                insertXXX279350734510Status(inserts);
                if (updateFlag)
                {
                    updates.ForEach(x =>
                    {
                        using (SqlCommand com = new SqlCommand())
                        {
                            sql = $@"
update [GLSIEXT].[dbo].[XXX279350734510Status]
SET STATUS = @STATUS,
LAST_TIME = UPDATE_DATETIME,
UPDATE_DATETIME = @UPDATE_DATETIME
where VOUCHER_DATE_TYPE = @VOUCHER_DATE_TYPE
and SOURCE_TYPE = @SOURCE_TYPE
";
                            com.Connection = conn;
                            com.CommandText = sql;
                            com.Parameters.AddWithValue($@"STATUS", x.STATUS);
                            com.Parameters.AddWithValue($@"VOUCHER_DATE_TYPE", x.VOUCHER_DATE_TYPE);
                            com.Parameters.AddWithValue($@"SOURCE_TYPE", x.SOURCE_TYPE);
                            com.Parameters.AddWithValue($@"UPDATE_DATETIME", dtn);
                        }
                    });
                }
            }
        }

        static void insertXXX279350734510Status(List<XXX279350734510Status> modelsA, int size = 100)
        {
            var pros = new XXX279350734510Status().GetType().GetProperties();
            using (SqlConnection conn = new SqlConnection(_conn))
            {
                conn.Open();
                for (int q = 0; (modelsA.Count() / size) >= q; q += 1)
                {
                    string insertStr = string.Empty;
                    StringBuilder sql = new StringBuilder();
                    var _datas = getInsertStr(modelsA.Skip((q) * size).Take(size).ToList(), pros, "[GLSIEXT].[dbo].[XXX279350734510Status]");
                    if (_datas.Item1.Length > 0)
                    {
                        using (SqlCommand com = new SqlCommand(_datas.Item1.ToString(), conn))
                        {
                            com.Parameters.AddRange(_datas.Item2.ToArray());
                            com.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        static void insertXXX279350734511_Detail (List<XXX279350734511_Detail> modelsA, int size = 100)
        {
            var pros = new XXX279350734511_Detail().GetType().GetProperties();
            using (SqlConnection conn = new SqlConnection(_conn))
            {
                conn.Open();
                for (int q = 0; (modelsA.Count() / size) >= q; q += 1)
                {
                    string insertStr = string.Empty;
                    StringBuilder sql = new StringBuilder();
                    var _datas = getInsertStr(modelsA.Skip((q) * size).Take(size).ToList(), pros, "[GLSIEXT].[dbo].[XXX279350734511_Detail]");
                    if (_datas.Item1.Length > 0)
                    {
                        using (SqlCommand com = new SqlCommand(_datas.Item1.ToString(), conn))
                        {
                            com.Parameters.AddRange(_datas.Item2.ToArray());
                            com.ExecuteNonQuery();
                        }
                    }
                }
            }

        }


        private static Tuple<StringBuilder, List<SqlParameter>> getInsertStr<T>(List<T> datas, System.Reflection.PropertyInfo[] infos = null, string tableRename = null)
        {
            StringBuilder sb = new StringBuilder();
            List<SqlParameter> parms = new List<SqlParameter>();
            if (!datas.Any())
                return new Tuple<StringBuilder, List<SqlParameter>>(new StringBuilder(), new List<SqlParameter>());
            var pros = infos;
            if (pros == null)
                pros = datas.First().GetType().GetProperties();
            var _name = tableRename;
            if (string.IsNullOrWhiteSpace(_name))
                _name = typeof(T).Name;
            for (var i = 0; i < datas.Count; i++)
            {
                var strs = $@"
INSERT INTO {_name} ";
                string strp = " ( ";
                string strv = " ( ";
                foreach (var pro in pros)
                {
                    if (pro.GetValue(datas[i]) != null)
                    {
                        strp += $@" [{pro.Name}] ,";
                        strv += $@" @{pro.Name}{i} ,";
                        //SqlDbType _type = SqlDbType.NVarChar;
                        //if (pro.PropertyType == typeof(decimal) || pro.PropertyType == typeof(decimal?))
                        //    _type = SqlDbType.Decimal;
                        //if (pro.PropertyType == typeof(int) || pro.PropertyType == typeof(int?))
                        //    _type = SqlDbType.Int;
                        //if (pro.PropertyType == typeof(DateTime) || pro.PropertyType == typeof(DateTime?))
                        //    _type = SqlDbType.DateTime;
                        parms.Add(new SqlParameter($@"{pro.Name}{i}", pro.GetValue(datas[i])));
                    }
                }
                if (strp.Length > 3)
                {
                    strp = strp.Substring(0, strp.Length - 1);
                    strv = strv.Substring(0, strv.Length - 1);
                    strp += " ) VALUES ";
                    strv += " ) ; ";
                    strs += strp;
                    strs += strv;
                    sb.Append(strs);
                }
            }
            return new Tuple<StringBuilder, List<SqlParameter>>(sb, parms);
        }

        private enum STATUS
        {
            [Description("尚未執行匯入作業。")]
            A,
            [Description("會科核對中，請等待。")]
            B,
            [Description("會科核對有誤，請詳會科核對報表。")]
            C,
            [Description("匯入作業執行中，請等待。")]
            D,
            [Description("匯入失敗，請重新執行。")]
            E,
            [Description("匯入成功，請詳會科核對報表。")]
            F,
            [Description("匯入成功。")]
            G,
        }

        public static string GetDescription<T>(T enumerationValue)
where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
            }
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        private static decimal stringToDecimal(string value)
        {
            var d = 0M;
            decimal.TryParse(value, out d);
            return d;
        }

        private static DateTime? stringToDataTimeN(string value)
        {
            DateTime? result = null;
            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(value, out dt);
            if (dt != DateTime.MinValue)
                result = dt;
            return result;
        }
    }

    class CONFIG
    {
        public string prog_id { get; set; }
        public string para_id { get; set; }
        public string para_value { get; set; }
    }

    public class XXX279350734510Status
    {
        /// <summary>
        /// 入帳日期年月
        /// </summary>
        [Description("入帳日期年月")]
        public string VOUCHER_DATE_TYPE { get; set; }

        /// <summary>
        /// 系統來源
        /// </summary>
        [Description("系統來源")]
        public string SOURCE_TYPE { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Description("狀態")]
        public string STATUS { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Description("更新時間")]
        public DateTime? UPDATE_DATETIME { get; set; }

        /// <summary>
        /// 上個狀態更新時間
        /// </summary>
        [Description("上個狀態更新時間")]
        public DateTime? LAST_TIME { get; set; }

    }

    public class XXX279350734511_Detail
    {

        /// <summary>
        /// 資料庫類別 W/A
        /// </summary>
        [Description("資料庫類別")]
        public string DB_TYPE { get; set; }

        /// <summary>
        /// -會計類別 A/B
        /// </summary>
        [Description("-會計類別")]
        public string ACCT_CODE_TYPE { get; set; }

        /// <summary>
        /// 入帳日期年月
        /// </summary>
        [Description("入帳日期年月")]
        public string VOUCHER_DATE_TYPE { get; set; }

        /// <summary>
        /// 會計科目
        /// </summary>
        [Description("會計科目")]
        public string ACCT_CODE { get; set; }

        /// <summary>
        /// AS400會計科目
        /// </summary>
        [Description("AS400會計科目")]
        public string ASSEM_NO_008 { get; set; }

        /// <summary>
        /// 幣別(原外幣帳本)
        /// </summary>
        [Description("幣別(原外幣帳本)")]
        public string ORI_CURR_CODE { get; set; }

        /// <summary>
        /// 帳本別
        /// </summary>
        [Description("帳本別")]
        public string ASSEM_NO_013 { get; set; }

        /// <summary>
        /// 契約始期
        /// </summary>
        [Description("契約始期")]
        public DateTime? ASSEM_NO_187 { get; set; }

        /// <summary>
        /// 資料來源
        /// </summary>
        [Description("資料來源")]
        public string ASSEM_NO_023 { get; set; }

        /// <summary>
        /// 險種
        /// </summary>
        [Description("險種")]
        public string ASSEM_NO_020 { get; set; }

        /// <summary>
        /// 保單號碼
        /// </summary>
        [Description("保單號碼")]
        public string ASSEM_NO_118 { get; set; }

        /// <summary>
        /// 保單序號
        /// </summary>
        [Description("保單序號")]
        public string ASSEM_NO_119 { get; set; }

        /// <summary>
        /// 身份證重覆別
        /// </summary>
        [Description("身份證重覆別")]
        public string ASSEM_NO_120 { get; set; }

        /// <summary>
        /// 系統來源別
        /// </summary>
        [Description("系統來源別")]
        public string ASSEM_NO_040 { get; set; }

        /// <summary>
        /// 標的代碼
        /// </summary>
        [Description("標的代碼")]
        public string ASSEM_NO_155 { get; set; }

        /// <summary>
        /// 分錄金額(借-貸)
        /// </summary>
        [Description("分錄金額(借-貸)")]
        public decimal? DR_CR_AMOUNT { get; set; }

        /// <summary>
        /// 分錄金額(原幣)(借-貸)
        /// </summary>
        [Description("分錄金額(原幣)(借-貸)")]
        public decimal? DR_CR_CURR_AMOUNT { get; set; }

        /// <summary>
        /// 傳票日期
        /// </summary>
        [Description("傳票日期")]
        public DateTime? VOUCHER_DATE { get; set; }

        /// <summary>
        /// 抽件註記
        /// </summary>
        [Description("抽件註記")]
        public string DRAW_MARK { get; set; }

        /// <summary>
        /// 交易種類
        /// </summary>
        [Description("交易種類")]
        public string TRAN_KIND { get; set; }

    }
}
