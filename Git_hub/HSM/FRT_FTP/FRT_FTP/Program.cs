using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.EasycomClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendMail;
using System.Transactions;
using System.Data.Common;

namespace FRT_FTP
{
    class Program
    {
        static void Main(string[] args)
        {
            bool sendFlag = false;
            string sendMsg = string.Empty;
            string _year = string.Empty;
            string _date = string.Empty;
            string _checkDate_flag = getAppSettings("TX_DATE_Check");
            int _year_i = 0;
            int _date_i = 0;
            string _checkDate = string.Empty;

            try
            {
                //Console.WriteLine("=====排程開始=====");
                WriteLog("排程開始");
                EacConnection con = new EacConnection(ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString);
                con.Open();
                var dtn = DateTime.Now;
                var checkdate = dtn.AddDays(-1);
                using (EacCommand ec = new EacCommand(con))
                {
                    string sql = @"
                        delete FRTBCOA0 
                        Where CRT_DATE = :CRT_DATE ";
                    ec.CommandText = sql;
                    ec.Prepare();
                    ec.Parameters.Add("CRT_DATE", dtn.ToString("yyyyMMdd"));
                    var insertNum = ec.ExecuteNonQuery();
                    WriteLog($"刪除 FRTBCOA0 :{insertNum}筆");
                    ec.Dispose();
                }

                if (_checkDate_flag == "Y")
                {
                    using (EacCommand com = new EacCommand(con))
                    {
                        string sql = $@"
select 
(LPAD(YEAR,3,'0')) AS YEAR , 
(LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0')) AS DATE  
from LGLCALE1 
where ((LPAD(year,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) <=  :CHECKDATE
and corp_rest <> 'Y'
order by year desc,month desc , day desc
FETCH FIRST 1 ROWS ONLY;
";
                        com.CommandText = sql;
                        com.Prepare();
                        com.Parameters.Add("CHECKDATE", $@"{checkdate.Year - 1911}{checkdate.ToString("MMdd")}");
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            _year = dbresult["YEAR"]?.ToString()?.Trim();
                            _date = dbresult["DATE"]?.ToString()?.Trim();
                        }
                        com.Dispose();
                    }
                    Int32.TryParse(_year, out _year_i);
                    Int32.TryParse(_date, out _date_i);
                    _checkDate = $@"{_year_i + 1911}{_date_i}";
                }

                var _ACNO_SA = "737102710668";
                var ACNO_SA = getAppSettings("ACNO_SA");
                if (!string.IsNullOrWhiteSpace(ACNO_SA))
                    _ACNO_SA = ACNO_SA;
                var memoStr = "金資即匯,票交即匯,金資匯退,票交匯退,匯出退還";
                var MEMO = getAppSettings("MEMO");
                if (!string.IsNullOrWhiteSpace(MEMO))
                    memoStr = MEMO;
                var _Memos = memoStr.Split(',').ToList();
                using (GLSIEXTEntities db = new GLSIEXTEntities())
                {
                    var datas = db.SCLIFEDD.AsNoTracking()
                        .Where(x => x.ACNO_SA == _ACNO_SA &&
                        x.RMK != null && x.RMK.Trim() != "").AsQueryable();
                    var alldatas = new List<SCLIFEDD>();
                    foreach (var _Memo in _Memos.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        alldatas.AddRange(datas.Where(x => x.MEMO.Contains(_Memo)).ToList());
                    }
                    if (!alldatas.Any())
                    {
                        sendFlag = true;
                        sendMsg += "當日無資料";
                    }
                    else if (_checkDate_flag == "Y" && _checkDate.Length >= 6 && !alldatas.Any(x=>x.TX_DATE == _checkDate))
                    {
                        sendFlag = true;
                        sendMsg += "中介檔資料日期中沒有上一個工作日的資料, 請抓取新版中介檔後, 再次執行";
                    }
                    else
                    {
                        foreach (var item in alldatas)
                        {
                            using (EacCommand ec = new EacCommand(con))
                            {
                                var _CORD = "0";
                                double REMIT_AMT = 0d;
                                var _WT_AMT = stringToDl(item.WT_AMT);
                                var _DEP_AMT = stringToDl(item.DEP_AMT);
                                if (_WT_AMT != 0d) //
                                {
                                    _CORD = "1";
                                    REMIT_AMT = _WT_AMT;
                                }
                                else if (_DEP_AMT != 0d) //
                                {
                                    _CORD = "2";
                                    REMIT_AMT = _DEP_AMT;
                                }
                                if (_CORD != "0")
                                {
                                    string sql = @"
                        INSERT INTO FRTBCOA0 (TRANS_DATE, FAST_NO, CORD, REMIT_AMT, CRT_DATE, CRT_TIME)
                        VALUES (:TRANS_DATE, :FAST_NO, :CORD, :REMIT_AMT, :CRT_DATE, :CRT_TIME) ;
                        ";
                                    ec.CommandText = sql;
                                    var _Tx_Date = Int32.Parse(item.TX_DATE) - 19110000;
                                    ec.Parameters.Add("TRANS_DATE", _Tx_Date);
                                    ec.Parameters.Add("FAST_NO", item.RMK);
                                    ec.Parameters.Add("CORD", _CORD);
                                    ec.Parameters.Add("REMIT_AMT", REMIT_AMT);
                                    //ec.Parameters.Add("CHK_MARK1", null);
                                    ec.Parameters.Add("CRT_DATE", DateTime.Now.ToString("yyyyMMdd"));
                                    ec.Parameters.Add("CRT_TIME", DateTime.Now.ToString("HHmmssff"));
                                    var insertNum = ec.ExecuteNonQuery();
                                    WriteLog($"新增 FRTBCOA0 :{insertNum}筆");
                                }
                            }
                        }
                    }                   
                    con.Close();
                    con.Dispose();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"排程錯誤, Exception:{ ex}",Ref.Nlog.Error);
                sendFlag = true;
                sendMsg += $"排程錯誤, Exception:{ ex}";
                Console.WriteLine($"排程錯誤, Exception:{ex}");
                Console.ReadLine();
            }
            finally
            {
                if (sendFlag)
                {
                    WriteLog("寄信開始");
                    if (sendMsg == "當日無資料")
                    {
                        WriteLog(sendMsg);
                        string Frt_Ftp = "FRT_FTP";
                        string _Frt_Ftp = getAppSettings("Frt_Group");
                        if (!string.IsNullOrWhiteSpace(_Frt_Ftp))
                            Frt_Ftp = _Frt_Ftp;
                        sendMail(sendMsg, Frt_Ftp);
                    }
                    else if(!string.IsNullOrWhiteSpace(sendMsg))
                    {
                        WriteLog(sendMsg);
                        string Sys_Error = "SYS_ERROR";
                        string _Sys_Error = getAppSettings("System_Group");
                        if (!string.IsNullOrWhiteSpace(_Sys_Error))
                            Sys_Error = _Sys_Error;
                        sendMail(sendMsg, Sys_Error);
                    }
                    WriteLog("寄信結束");
                }

                WriteLog("排程結束");
                //Console.WriteLine("=====排程結束=====");
            }

        }

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="successFlag"></param>
        /// <returns></returns>
        protected static int sendMail(string sendMsg, string groupCode = "FRT_FTP")
        {
            int result = 0;
            DateTime dn = DateTime.Now;
            string dtn = $@"{(dn.Year - 1911)}/{dn.ToString("MM/dd")}";
            var emps = new List<V_EMPLY2>();
            var depts = new List<VW_OA_DEPT>();
            var sub = $@"{dtn} 快速付款FTP通知";
            var body = $@"{sendMsg}";
            var msg = string.Empty;
            Dictionary<string, Tuple<string, string>> mailToList = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> ccToList = new Dictionary<string, Tuple<string, string>>();
            //mailToList.Add("SYS", new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"));
            bool Falg = false;
            try
            {
                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    emps = db.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null && x.EMP_NO != null && x.DPT_CD != null).ToList();
                    depts = db.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }
                using (dbGLEntities db = new dbGLEntities())
                {
                    List<FRT_MAIL_NOTIFY> _FMNs = db.FRT_MAIL_NOTIFY.AsNoTracking()
                         .Where(x => x.GROUP_CODE == groupCode).ToList();
                    _FMNs.ForEach(x =>
                    {
                        List<string> USRIDs = new List<string>();
                        if (x.EMP_TYPE == "U")
                        {
                            USRIDs.Add(x.RECEIVER_EMPNO?.Trim());
                        }
                        else if (x.EMP_TYPE == "R")
                        {
                            USRIDs.AddRange(db.CODE_ROLE.Where(y => y.ROLE_ID != null && y.ROLE_ID == x.RECEIVER_EMPNO && y.IS_DISABLED == "N")
                            .Join(db.CODE_USER_ROLE.AsNoTracking().Where(z => z.ROLE_ID != null && z.USER_ID != null),
                             i => i.ROLE_ID.Trim(),
                             j => j.ROLE_ID.Trim(),
                             (i, j) => j)
                             .Join(db.CODE_USER.AsNoTracking().Where(z => z.IS_DISABLED == "N" && z.USER_ID != null),
                             i => i.USER_ID.Trim(),
                             j => j.USER_ID.Trim(),
                             (i, j) => j).Where(y => y.USER_ID != null).Select(y => y.USER_ID.Trim()));
                        }
                        if (USRIDs.Any())
                        {
                            USRIDs = USRIDs.Distinct().ToList();
                            var mailDatas = getSendMail(USRIDs, x.IS_NOTIFY_MGR, x.IS_NOTIFY_DEPT_MGR, emps, depts);
                            foreach (var mail in mailDatas.Item1)
                            {
                                if (!mailToList.ContainsKey(mail.Key))
                                {
                                    mailToList.Add(mail.Key, mail.Value);
                                }
                            }
                            foreach (var cc in mailDatas.Item2)
                            {
                                //if (!ccToList.ContainsKey(cc.Key))
                                //{
                                //    ccToList.Add(cc.Key,cc.Value);
                                //}
                                if (!mailToList.ContainsKey(cc.Key))
                                {
                                    mailToList.Add(cc.Key, cc.Value);
                                }
                            }
                        }
                        else
                        {

                        }
                    });
                }
                if (mailToList.Any())
                {
                    result = mailToList.Values.Count;
                    var sms = new SendMail.SendMailSelf();
                    sms.smtpPort = 25;
                    sms.smtpServer = getAppSettings("smtpServer");
                    sms.mailAccount = getAppSettings("mailAccount");
                    sms.mailPwd = getAppSettings("mailPwd");
                    msg = sms.Mail_Send(
                        new Tuple<string, string>(sms.mailAccount, "FRT_FTP"),
                        mailToList.Values.AsEnumerable(),
                        ccToList.Any() ? ccToList.Values.AsEnumerable() : null,
                        sub,
                        body
                        );
                }
                else
                {
                    msg = "無查詢到寄信對象!";
                    WriteLog(msg);
                }
                if (string.IsNullOrWhiteSpace(msg))
                {
                    Falg = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog($"寄信: InnerException:{ex.InnerException},Message:{ex.Message}", Ref.Nlog.Error);
                Falg = false;
                msg = $"InnerException:{ex.InnerException},Message:{ex.Message}";
            }
            finally
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (dbGLEntities db = new dbGLEntities())
                    {
                        foreach (var mail in mailToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = mail.Key,
                                EMAIL = mail.Value.Item1,
                                MAIL_RESULT = Falg ? "S" : "F",
                                RESULT_DESC = msg.Length >= 250 ? msg.Substring(0, 250) : msg,
                                MAIL_SUB = sub
                            });
                        }
                        foreach (var cc in ccToList)
                        {
                            db.FRT_MAIL_LOG.Add(new FRT_MAIL_LOG()
                            {
                                SEQ = getMailSeq(db),
                                MAIL_DATE = dn,
                                MAIL_TIME = dn.TimeOfDay,
                                RECEIVER_EMPNO = cc.Key,
                                EMAIL = cc.Value.Item1,
                                MAIL_RESULT = Falg ? "S" : "F",
                                RESULT_DESC = msg.Length >= 250 ? msg.Substring(0, 250) : msg,
                                MAIL_SUB = sub
                            });
                        }
                        db.SaveChanges();
                    }
                    scope.Complete();
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢收信人
        /// </summary>
        /// <param name="USRIDs">收信人</param>
        /// <param name="IS_NOTIFY_MGR">IS_NOTIFY_MGR = Y時，除TABLE本身的人員要寄送，還要寄給科主管</param>
        /// <param name="IS_NOTIFY_DEPT_MGR">IS_NOTIFY_DEPT_MGR = Y時，除TABLE本身的人員要寄送，還要寄給部主管</param>
        /// <returns></returns>
        protected static Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>> getSendMail(
            List<string> USRIDs,
            string IS_NOTIFY_MGR,
            string IS_NOTIFY_DEPT_MGR,
            List<V_EMPLY2> emps,
            List<VW_OA_DEPT> depts)
        {
            var sendEmps = emps.Where(x => USRIDs.Contains(x.USR_ID.Trim())).ToList();
            Dictionary<string, Tuple<string, string>> mailTos = new Dictionary<string, Tuple<string, string>>();
            sendEmps.ForEach(x =>
            {
                mailTos.Add(x.USR_ID?.Trim(), new Tuple<string, string>(x.EMAIL?.Trim(), x.EMP_NAME?.Trim()));
            });
            Dictionary<string, Tuple<string, string>> ccs = new Dictionary<string, Tuple<string, string>>();
            if (IS_NOTIFY_MGR == "Y" || IS_NOTIFY_DEPT_MGR == "Y")
            {
                List<string> ccUSRIDs = new List<string>();
                var _sendEmps = sendEmps;
                if (IS_NOTIFY_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給科主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }
                    });
                }
                if (IS_NOTIFY_DEPT_MGR == "Y") //除TABLE本身的人員要寄送，還要寄給部主管
                {
                    _sendEmps.ForEach(x =>
                    {
                        var dept = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.DPT_CD.Trim());
                        if (dept != null && dept.Dpt_type == "03") //部
                        {
                            if (x.EMP_NO != x.DPT_HEAD) //非主管本人
                                ccUSRIDs.Add(x.DPT_HEAD.Trim());
                        }
                        if (dept != null && dept.Dpt_type == "04") //科
                        {
                            var dept2 = depts.FirstOrDefault(y => y.DPT_CD.Trim() == dept.UP_DPT_CD.Trim());
                            var emp2 = emps.FirstOrDefault(y => y.EMP_NO.Trim() == dept2?.DPT_HEAD.Trim());
                            if (emp2 != null)
                                ccUSRIDs.Add(emp2.EMP_NO.Trim());
                        }
                    });
                }
                if (ccUSRIDs.Any())
                {
                    ccUSRIDs = ccUSRIDs.Distinct().ToList();

                    foreach (var item in emps.Where(x => ccUSRIDs.Contains(x.EMP_NO.Trim())))
                    {
                        if (!ccs.ContainsKey(item.USR_ID?.Trim()))
                        {
                            ccs.Add(item.USR_ID?.Trim(), new Tuple<string, string>(item.EMAIL?.Trim(), item.EMP_NAME?.Trim()));
                        }
                    }
                }
            }
            return new Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, Tuple<string, string>>>(mailTos, ccs);
        }

        protected static long getMailSeq(dbGLEntities db)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var seq = db.SYS_SEQ.FirstOrDefault(x => x.SYS_CD == "RT" && x.SEQ_TYPE == "mail" && x.PRECODE == date);
            string seqNo = string.Empty;
            if (seq == null)
            {
                seqNo = $@"{date}{"1".PadLeft(5, '0')}";
                db.SYS_SEQ.Add(new SYS_SEQ()
                {
                    SYS_CD = "RT",
                    SEQ_TYPE = "mail",
                    PRECODE = date,
                    CURR_VALUE = 2
                });
                db.SaveChanges();
            }
            else
            {
                seqNo = $@"{date}{seq.CURR_VALUE.ToString().PadLeft(5, '0')}";
                seq.CURR_VALUE = seq.CURR_VALUE + 1;
            }
            return Convert.ToInt64(seqNo);
        }


        protected static double stringToDl(string value)
        {
            double d = 0d;
            if (string.IsNullOrWhiteSpace(value))
                return d;
            double.TryParse(value, out d);
            return d < 0 ? (d * -1) : d;
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected static void WriteLog(string log, Ref.Nlog type = Ref.Nlog.Info)
        {
            switch (type)
            {
                //追蹤
                case Ref.Nlog.Trace:
                    logger.Trace(log);
                    break;
                //開發
                case Ref.Nlog.Debug:
                    logger.Debug(log);
                    break;
                //訊息
                case Ref.Nlog.Info:
                    logger.Info(log);
                    break;
                //警告
                case Ref.Nlog.Warn:
                    logger.Warn(log);
                    break;
                //錯誤
                case Ref.Nlog.Error:
                    logger.Error(log);
                    break;
                //致命
                case Ref.Nlog.Fatal:
                    logger.Fatal(log);
                    break;
            }
        }

        public partial class Ref
        {
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

        public static string getAppSettings(string str)
        {
            string _result = string.Empty;
            try
            {
                _result = System.Configuration.ConfigurationManager.AppSettings.Get(str)?.Trim() ?? string.Empty;
            }
            catch
            {

            }
            return _result;
        }
    }
}
