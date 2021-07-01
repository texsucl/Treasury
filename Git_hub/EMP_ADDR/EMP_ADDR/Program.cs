using Dapper;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMP_ADDR
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLog("程式 Start!");
            DateTime dtn = DateTime.Now;
            var DF_Str = System.Configuration.ConfigurationManager.
                    ConnectionStrings["DF"].ConnectionString;
            var DBINTRA_Str = System.Configuration.ConfigurationManager.
                    ConnectionStrings["DBINTRA"].ConnectionString;
            var GLSIEXT_Str = System.Configuration.ConfigurationManager.
                    ConnectionStrings["GLSIEXT"].ConnectionString;

            string mailMsg = string.Empty; //mail 的訊息 (錯誤訊息 & 資料無統編訊息)
            string strSql = string.Empty; //sql string 預設
            List<A19_TEMP> temps = new List<A19_TEMP>(); //A19_TEMP 資料
            List<A19_TEMP> missData = new List<A19_TEMP>(); //無統一編號的資料
            try
            {
                List<dept_empaddr_vw> empaddrs = new List<dept_empaddr_vw>();
                List<A19_COMPARES> comspares = new List<A19_COMPARES>();
                List<DF_TEMP> DFs = new List<DF_TEMP>();
                #region 外勤
                using (SqlConnection conn = new SqlConnection(DF_Str))
                {
                    conn.Open();
                    strSql = $@"
select
 D.DEP_ID,D.DEP_NAME,M.MEM_NAME
from [dbo].[DF_SCH_DEP] AS D
left join [dbo].[DF_SCH_MEM] M
on D.DEP_ID = M.DEP_ID
and M.IS_MANA = 'Y'
and M.SYS_TYPE = 'B'
where D.SYS_TYPE = 'B'
group by D.DEP_ID,D.DEP_NAME,M.MEM_NAME ;
";
                    DFs = conn.Query<DF_TEMP>(strSql, null).ToList();
                    WriteLog($@"查詢 外勤 筆數 : {DFs.Count}");
                    foreach (var item in DFs) 
                    {
                        temps.Add(new A19_TEMP()
                        {
                            EMP_NAME = item.MEM_NAME,
                            DPT_CD = item.DEP_ID?.TransferDep(),
                            DPT_NAME = item.DEP_NAME,
                            SDATE = dtn,
                            TAX_ID = "27935073" //公司統編
                        }) ;
                    }
                }
                #endregion

                #region 內勤
                using (SqlConnection conn = new SqlConnection(DBINTRA_Str))
                {
                    conn.Open();
                    strSql = $@"
select emp_name
      ,usr_id
      ,dpt_cd
      ,dpt_name
      ,zip
      ,address
      from dept_empaddr_vw ;";
                    empaddrs = conn.Query<dept_empaddr_vw>(strSql, null).ToList();
                    WriteLog($@"查詢 內勤 筆數 : {empaddrs.Count}");
                }

                using (SqlConnection conn = new SqlConnection(GLSIEXT_Str))
                {
                    conn.Open();

                    strSql = $@"select ADDRESS,TAX_ID from [dbo].A19_COMPARES ;";

                    comspares = conn.Query<A19_COMPARES>(strSql, null).ToList();

                    foreach (var item in empaddrs)
                    {
                        var _ADDRESS = halfToFull(item.address?.CheckAddress()); //職場地址
                        temps.Add(new A19_TEMP()
                        {
                            EMP_NAME = item.emp_name, //員工姓名
                            EMP_NO = item.usr_id, //員工網域帳號
                            DPT_CD = item.dpt_cd, //部門代碼
                            DPT_NAME = item.dpt_name, //部門名稱
                            ZIP = item.zip, //職場郵遞區號
                            ADDRESS = _ADDRESS, //職場地址
                            SDATE = dtn, //執行時間
                            TAX_ID = comspares.FirstOrDefault(x => x.ADDRESS == _ADDRESS)?.TAX_ID //分公司代碼(統編)
                        });
                    }

                    missData = temps.Where(x => x.TAX_ID.IsNullOrWhiteSpace()).ToList();
                    WriteLog($@"無比對到 筆數 : {missData.Count}");
                    missData.ForEach(x => temps.Remove(x));

                    if (temps.Any())
                    {
                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                            strSql = $@" Truncate Table [GLSIEXT].[dbo].[A19_TEMP]; ";
                            int tru_result = conn.Execute(strSql, null, tran);
                            strSql = $@" INSERT INTO [GLSIEXT].[dbo].[A19_TEMP] 
(ZIP,ADDRESS,DPT_NAME,DPT_CD,EMP_NAME,EMP_NO,SDATE,TAX_ID)
VALUES
(@ZIP,@ADDRESS,@DPT_NAME,@DPT_CD,@EMP_NAME,@EMP_NO,@SDATE,@TAX_ID) ;
";
                            int ins_result_1 = conn.Execute(strSql, temps, tran);

                            strSql = $@" 
Delete GLSIEXT..FGLGCTL0S 
where FLOW_TYPE = 'A19'
and CRT_DT between (convert(varchar, getdate(), 111) + ' 00:00:00.000') and (convert(varchar, getdate(), 111) + ' 23:59:59.999')
and TRANS_STS <> 'IMS'
";
                            int del_result_1 = conn.Execute(strSql, null, tran);

                            strSql = $@" INSERT INTO GLSIEXT..FGLGCTL0S 
(TRANS_NO,FLOW_TYPE,TRANS_STS,CRT_DT)
VALUES
(@TRANS_NO,@FLOW_TYPE,@TRANS_STS,@CRT_DT)
";
                            FGLGCTL0S insert_2 = new FGLGCTL0S()
                            {
                                TRANS_NO = $@"A19{dtn.ToString("yyyyMMddHHmmssf")}",
                                FLOW_TYPE = "A19",
                                TRANS_STS = "TRS",
                                CRT_DT = dtn
                            };
                            int ins_result_2 = conn.Execute(strSql, insert_2, tran);

                            tran.Commit();
                            WriteLog($@"Truncate A19_TEMP 筆數 : {tru_result}");
                            WriteLog($@"Insert A19_TEMP 筆數 : {ins_result_1}");
                            WriteLog($@"Delete FGLGCTL0S 筆數 : {del_result_1} ");
                            WriteLog($@"Insert FGLGCTL0S 筆數 : {ins_result_2}");
                        }
                    }
                    else if (!missData.Any())
                    {
                        mailMsg = "組織樹無查詢到資料!";
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {
                mailMsg = ex.ToString();
                WriteLog($@"Exception : {ex.ToString()}");
            }
            finally
            {
                WriteLog("寄信 Start!");
                try
                {
                    #region 錯誤訊息 or 有缺漏統編
                    mailMsg = createMissTable(missData, mailMsg);
                    if (!mailMsg.IsNullOrWhiteSpace())
                    {
                        sendMissMail(mailMsg);
                    }
                    #endregion

                    #region 部門代碼及分公司對照報表
                    string tableStr = string.Empty;
                    if (temps.Any())
                    {
                        tableStr = createMailTable(temps.OrderBy(x => x.DPT_CD).ToList(), "依部門代碼排序",true);
                        tableStr += "</br></br></br>";
                        tableStr += createMailTable(temps.OrderBy(x => x.TAX_ID).ToList(), "依分公司代碼排序");
                        sendTableMail(tableStr);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    WriteLog("寄信失敗 " + ex.ToString());
                }
                WriteLog("寄信 End!");

                WriteLog("程式 End!");
            }
        }

        protected static void sendMissMail(string body)
        {
            try
            {
                var sms = new SendMail.SendMailSelf();
                sms.smtpPort = 25;
                sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"]?.ToString();
                sms.mailAccount = ConfigurationManager.AppSettings["mailAccount"]?.ToString();
                string sendMailstr = ConfigurationManager.AppSettings["mailSend_msg"]?.ToString();
                List<Tuple<string, string>> sendMails = new List<Tuple<string, string>>();
                foreach (var item in sendMailstr.Split(';'))
                {
                    string mail = string.Empty;
                    string name = string.Empty;
                    var items = item.Split(':');
                    mail = items[0];
                    if (items.Length == 2)
                        name = items[1];
                    else
                        name = items[0];
                    sendMails.Add(new Tuple<string, string>(mail, name));
                }
                string _msg = sms.Mail_Send(
                new Tuple<string, string>(sms.mailAccount, "A19_Temp"),
                sendMails,
                null,
                "分公司對照檔異常預警通知報表",
                body,
                true
                );
                if (!_msg.IsNullOrWhiteSpace())
                    WriteLog(_msg, Nlog.Error);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString(), Nlog.Error);
            }
        }

        protected static void sendTableMail(string body)
        {
            try
            {
                var sms = new SendMail.SendMailSelf();
                sms.smtpPort = 25;
                sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"]?.ToString();
                sms.mailAccount = ConfigurationManager.AppSettings["mailAccount"]?.ToString();
                string sendMailstr = ConfigurationManager.AppSettings["mailSend_table"]?.ToString();
                List<Tuple<string, string>> sendMails = new List<Tuple<string, string>>();
                foreach (var item in sendMailstr.Split(';'))
                {
                    string mail = string.Empty;
                    string name = string.Empty;
                    var items = item.Split(':');
                    mail = items[0];
                    if (items.Length == 2)
                        name = items[1];
                    else
                        name = items[0];
                    sendMails.Add(new Tuple<string, string>(mail, name));
                }
                string _msg = sms.Mail_Send(
                new Tuple<string, string>(sms.mailAccount, "A19_Temp"),
                sendMails,
                null,
                "部門代碼及分公司對照報表",
                body,
                true
                );
                if(!_msg.IsNullOrWhiteSpace())
                    WriteLog(_msg, Nlog.Error);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString(), Nlog.Error);
            }
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void WriteLog(string log, Nlog type = Nlog.Info)
        {
            switch (type)
            {
                //追蹤
                case Nlog.Trace:
                    logger.Trace(log);
                    break;
                //開發
                case Nlog.Debug:
                    logger.Debug(log);
                    break;
                //訊息
                case Nlog.Info:
                    logger.Info(log);
                    break;
                //警告
                case Nlog.Warn:
                    logger.Warn(log);
                    break;
                //錯誤
                case Nlog.Error:
                    logger.Error(log);
                    break;
                //致命
                case Nlog.Fatal:
                    logger.Fatal(log);
                    break;
            }
        }

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

        public static string createMissTable(List<A19_TEMP> temps, string mailMsg)
        {
            string result = string.Empty;          
            if (mailMsg.IsNullOrWhiteSpace() && temps.Any())
            {
                WriteLog($@"Miss 資料!");
                result += @"<table style='border: 3px #cccccc solid;' cellpadding='10' border='1'>";
                result += @"<thead>";
                result += $@"<tr><td colspan='5'>分公司對照檔異常預警通知報表</td></tr>";
                result += @"</thead>";
                result += @"<tr><td>部門代碼</td><td>部門名稱</td><td>員工姓名</td><td>營業地址</td><td>分公司代碼</td></tr>";
                int i = 0;
                temps.ForEach(x =>
                {
                    i += 1;
                    WriteLog($@"MissData_{i} => 部門代碼:{x.DPT_CD},部門名稱:{x.DPT_NAME},員工姓名:{x.EMP_NAME},營業地址:{x.ADDRESS},分公司代碼:{x.TAX_ID}");
                    result += $@"<tr><td>{x.DPT_CD}</td><td>{x.DPT_NAME}</td><td>{x.EMP_NAME}</td><td>{x.ADDRESS}</td><td>{x.TAX_ID}</td></tr>";
                });
                result += @"</table>";
            }
            if (!mailMsg.IsNullOrWhiteSpace())
                result = mailMsg;

            return result;
        }

        public static string createMailTable(List<A19_TEMP> temps, string head,bool log = false)
        {
            if(log)
                WriteLog($@"符合 資料!");
            string result = string.Empty;
            result += @"<table style='border: 3px #cccccc solid;' cellpadding='10' border='1'>";
            result += @"<thead>";
            result += $@"<tr><td colspan='5'>{head}</td></tr>";
            result += @"</thead>";
            result += @"<tr><td>部門代碼</td><td>部門名稱</td><td>員工姓名</td><td>營業地址</td><td>分公司代碼</td></tr>";
            int i = 0;
            temps.ForEach(x =>
            {
                i += 1;
                if (log)
                    WriteLog($@"CompareData_{i} => 部門代碼:{x.DPT_CD},部門名稱:{x.DPT_NAME},員工姓名:{x.EMP_NAME},營業地址:{x.ADDRESS},分公司代碼:{x.TAX_ID}");
                result += $@"<tr><td>{x.DPT_CD}</td><td>{x.DPT_NAME}</td><td>{x.EMP_NAME}</td><td>{x.ADDRESS}</td><td>{x.TAX_ID}</td></tr>";
            });
            result += @"</table>";
            return result;
        }

        public enum Nlog
        {
            [Description("追蹤")]
            Trace,
            [Description("開發")]
            Debug,
            [Description("訊息")]
            Info,
            [Description("警告")]
            Warn,
            [Description("錯誤")]
            Error,
            [Description("致命")]
            Fatal
        }
    }

    public class dept_empaddr_vw
    {
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string emp_name { get; set; }

        /// <summary>
        /// 員工網域帳號
        /// </summary>
        public string usr_id { get; set; }

        /// <summary>
        /// 部門代碼
        /// </summary>
        public string dpt_cd { get; set; }

        /// <summary>
        /// 部門名稱
        /// </summary>
        public string dpt_name { get; set; }

        /// <summary>
        /// 職場郵遞區號
        /// </summary>
        public string zip { get; set; }

        /// <summary>
        /// 職場地址
        /// </summary>
        public string address { get; set; }
    }

    public class A19_TEMP
    {
        /// <summary>
        /// 地址號碼
        /// </summary>
        public string ZIP { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string ADDRESS { get; set; }

        /// <summary>
        /// 部門中文
        /// </summary>
        public string DPT_NAME { get; set; }

        /// <summary>
        /// 部門ID
        /// </summary>
        public string DPT_CD { get; set; }

        /// <summary>
        /// 員工(主管)名
        /// </summary>
        public string EMP_NAME { get; set; }

        /// <summary>
        /// 員工(主管)5碼ID
        /// </summary>
        public string EMP_NO { get; set; }

        /// <summary>
        /// 生效日
        /// </summary>
        public DateTime SDATE { get; set; }

        /// <summary>
        /// 分公司代碼(統編)
        /// </summary>
        public string TAX_ID { get; set; }
    }

    public class A19_COMPARES
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string ADDRESS { get; set; }

        /// <summary>
        /// 分公司代碼(統編)
        /// </summary>
        public string TAX_ID { get; set; }
    }

    public class DF_TEMP
    {
        /// <summary>
        /// 部門ID
        /// </summary>
        public string DEP_ID { get; set; }

        /// <summary>
        /// 部門中文
        /// </summary>
        public string DEP_NAME { get; set; }

        /// <summary>
        /// 員工(主管)名
        /// </summary>
        public string MEM_NAME { get; set; }
    }

    public class FGLGCTL0S
    { 
        public string TRANS_NO { get; set; }

        public string FLOW_TYPE { get; set; }

        public string TRANS_STS { get; set; }

        public DateTime CRT_DT { get; set; }
    }

    public static class Extension
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 地址 只抓取號(含)以前的文字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CheckAddress(this string str)
        {
            string result = str;
            if (!str.IsNullOrWhiteSpace() && str.IndexOf("號") > -1)
                result = str.Substring(0, str.IndexOf("號") + 1);         
            return result;
        }

        /// <summary>
        /// 外勤依規則轉換 部門代碼
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TransferDep(this string str)
        {
            if (str.IsNullOrWhiteSpace() || str.Length <= 5)
                return str;
            if (str.Length < 7 || str.Substring(6, 1) == "0")
                return str.Substring(0, 5);
            else
                return $@"{str.Substring(0, 4)}{str.Substring(6, 1)}"; 
                
        }
    }
}
