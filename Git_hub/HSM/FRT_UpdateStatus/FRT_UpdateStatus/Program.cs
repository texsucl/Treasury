using FRT_UpdateStatus.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FRT_UpdateStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("程式開始!");
            WriteLog("程式開始!");
            string msg = string.Empty;
            string _fastNo = string.Empty;
            try
            {
                EacConnection con = new EacConnection(ConfigurationManager.ConnectionStrings["Easycom"].ConnectionString);
                con.Open();
                int _stat_update_min_int = 0;
                using (dbGLEntities db = new dbGLEntities())
                {
                    var _stat_update_min = db.SYS_PARA.AsNoTracking()
                        .Where(x => x.SYS_CD == "RT" && x.GRP_ID == "FRQxml" && x.PARA_ID == "stat_update_min")
                        .FirstOrDefault()?.PARA_VALUE;
                    if (_stat_update_min != null)
                        Int32.TryParse(_stat_update_min, out _stat_update_min_int);
                }
                var dtn = DateTime.Now.AddMinutes(_stat_update_min_int * -1);
                var _dtn = (dtn.Year - 1911).ToString() + dtn.ToString("MMddHHmmssff");

                List<string> FAST_NOs = new List<string>();

                using (EacCommand ec = new EacCommand(con))
                {
                    string sql = string.Empty;
                    sql = $@"
select FAST_NO from FRTBARM0
where REMIT_STAT = '2'
and 
(
    (
        TRIM(IFNULL(FILLER_20,'')) = ''
        and
        TEXT_TYPE = '3' 
        and
        TEXT_RCVTM <> '0'
        and
        (LPAD(TEXT_RCVDT,7,'0') || LPAD(TEXT_RCVTM,8,'0')) <= :Stat_Update_Min
    )
    OR
    (
        TRIM(IFNULL(FILLER_20,'')) <> ''
        and
        (LPAD(TEXT_SNDDT,7,'0') || LPAD(TEXT_SNDTM,8,'0')) <= :Stat_Update_Min
    )
)
";
                    ec.CommandText = sql;
                    ec.Parameters.Add("Stat_Update_Min", _dtn);
                    ec.Prepare();
                    DbDataReader result = ec.ExecuteReader();

                    while (result.Read())
                    {
                        //string FAST_NO = result["FAST_NO"]?.ToString();
                        string FAST_NO = result[0]?.ToString();
                        WriteLog($@"FAST_NO:{FAST_NO}");
                        FAST_NOs.Add(FAST_NO);
                        //Console.WriteLine($@"FAST_NO:{FAST_NO}");//DEBUG LINE
                        //new Program().callApi(new ErrorModel()
                        //{
                        //    ExecType = "S",
                        //    Fast_No = FAST_NO,
                        //    ErrorCode = "0000",
                        //    EMSGTXT = null,
                        //    TextType = "3"
                        //});
                    }
                    ec.Dispose();
                }
                //con.Dispose();
                //con.Close();

                var updateNum = 0;

                using (EacCommand ec = new EacCommand(con))
                {

                    string sql = string.Empty;
                    sql = $@"
update FRTBARM0
set REMIT_STAT = '3'
where REMIT_STAT = '2'
and 
(
    (
        TRIM(IFNULL(FILLER_20,'')) = ''
        and
        TEXT_TYPE = '3' 
        and
        TEXT_RCVTM <> '0'
        and
        (LPAD(TEXT_RCVDT,7,'0') || LPAD(TEXT_RCVTM,8,'0')) <= :Stat_Update_Min
    )
    OR
    (
        TRIM(IFNULL(FILLER_20,'')) <> ''
        and
        (LPAD(TEXT_SNDDT,7,'0') || LPAD(TEXT_SNDTM,8,'0')) <= :Stat_Update_Min
    )
)
";
                    ec.CommandText = sql;
                    ec.Parameters.Add("Stat_Update_Min", _dtn);
                    ec.Prepare();
                    updateNum = ec.ExecuteNonQuery();
                    //Console.WriteLine(string.Empty);
                    WriteLog($"FRTBARM0 資料異動筆數:{updateNum}");
                    //Extension.writeTxtLog($"{DateTime.Now} 資料異動筆數:{updateNum}", Path.Combine(Directory.GetCurrentDirectory(), "log.txt"));
                    //Console.WriteLine($"資料異動筆數:{updateNum}");
                    ec.Dispose();
                }

                //if (updateNum == FAST_NOs.Count)
                //{
                    FAST_NOs.ForEach(x =>
                    {
                        var _result = new Program().callApi(new ErrorModel()
                        {
                            ExecType = "S",
                            Fast_No = x,
                            ErrorCode = "0000",
                            EMSGTXT = null,
                            TextType = "3"
                        });
                        _fastNo = x;
                        if (!_result.IsNullOrWhiteSpace())
                        {
                            msg += System.Environment.NewLine;
                            msg += $"FASTNO:{_fastNo}";
                            msg += System.Environment.NewLine;
                            msg += _result;
                        }
                    });
                //}
                con.Dispose();
                con.Close();

            }
            catch (Exception ex)
            {
                msg += System.Environment.NewLine;
                msg += $"FASTNO:{_fastNo}";
                msg += System.Environment.NewLine;
                msg += $"程式錯誤:{ex.Message}";
                WriteLog($"程式錯誤:{ex.Message}", Ref.Nlog.Error);
                //Extension.writeTxtLog($"{DateTime.Now} 程式錯誤:{ex.Message}", Path.Combine(Directory.GetCurrentDirectory(), "log.txt"));
                //Console.WriteLine($"程式錯誤:{ex.Message}");
            }
            finally
            {
                if (!msg.IsNullOrWhiteSpace())
                {
                    string Sys_Error = "SYS_ERROR";
                    string _Sys_Error = Properties.Settings.Default["System_Group"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(_Sys_Error))
                        Sys_Error = _Sys_Error;
                    sendMail(_Sys_Error, msg);
                }
                WriteLog("程式結束!");
                //Console.WriteLine("程式結束!");
            }       
        }



        /// <summary>
        /// 錯誤時呼叫 Api 
        /// </summary>
        /// <param name="model">傳送的Data</param>
        protected string callApi(ErrorModel model)
        {
            string _result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //Console.WriteLine($"開始傳送API");
                    // 指定 authorization header
                    client.DefaultRequestHeaders.Add("authorization", "token = 6D9310E55EB72CA5D7BBC8F98DD517BC");
                    client.DefaultRequestHeaders.Add("token", "6D9310E55EB72CA5D7BBC8F98DD517BC");
                    // 將 data 轉為 json
                    string json = JsonConvert.SerializeObject(model);
                    // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                    // 發出 post 並取得結果
                    string uri = "http://10.240.68.38:8096/FastError/RemittanceFailureNotice/";
                    string _uri = Properties.Settings.Default["http_Client"]?.ToString();
                    if (!_uri.IsNullOrWhiteSpace())
                        uri = _uri;
                    HttpResponseMessage response = client.PostAsync(uri, contentPost).Result;
                    // 將回應結果內容取出並轉為 string 再透過 linqpad 輸出
                    var result = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                _result = $"程式錯誤:{ex.Message}";
                WriteLog(_result, Ref.Nlog.Error);
                //Console.WriteLine($"傳送API失敗 : InnerException:{ex.InnerException},Message{ex.Message}");
            }
            return _result;
        }

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        protected partial class ErrorModel
        {
            /// <summary>
            /// 回寫匯款狀態
            /// </summary>
            public string ExecType { get; set; }

            /// <summary>
            /// 快速付款編號
            /// </summary>
            public string Fast_No { get; set; }

            /// <summary>
            /// 錯誤代碼
            /// </summary>
            public string ErrorCode { get; set; }

            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string EMSGTXT { get; set; }

            /// <summary>
            /// 電文類型
            /// </summary>
            public string TextType { get; set; }
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void WriteLog(string log, Ref.Nlog type = Ref.Nlog.Info)
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

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="successFlag"></param>
        /// <returns></returns>
        protected static int sendMail(string groupCode = "SYS_ERROR", string msg = null)
        {
            int result = 0;
            DateTime dn = DateTime.Now;
            string dtn = $@"{(dn.Year - 1911)}/{dn.ToString("MM/dd")}";
            var emps = new List<V_EMPLY2>();
            var depts = new List<VW_OA_DEPT>();
            var sub = string.Empty;
            var body = string.Empty;
            sub = $@"{dtn} 快速付款 FRT_UpdateStatus 失敗通知";
            body = $@"錯誤訊息:{msg}";

            var _msg = string.Empty;
            Dictionary<string, Tuple<string, string>> mailToList = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> ccToList = new Dictionary<string, Tuple<string, string>>();
            bool Flag = false;
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
                if (mailToList.Values.Any())
                {
                    var sms = new SendMail.SendMailSelf();
                    sms.smtpPort = 25;
                    sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                    sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                    sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                    _msg = sms.Mail_Send(
                        new Tuple<string, string>(sms.mailAccount, "FRT_ReTrXml"),
                        mailToList.Values.AsEnumerable(),
                        ccToList.Any() ? ccToList.Values.AsEnumerable() : null,
                        sub,
                        body
                        );
                    if (string.IsNullOrWhiteSpace(_msg))
                    {
                        Flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"寄信: InnerException:{ex.InnerException},Message:{ex.Message}", Ref.Nlog.Error);
                Flag = false;
                _msg = $"InnerException:{ex.InnerException},Message:{ex.Message}";
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
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0, 250) : _msg,
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
                                MAIL_RESULT = Flag ? "S" : "F",
                                RESULT_DESC = _msg.Length >= 250 ? _msg.Substring(0, 250) : _msg,
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
    }

    

    internal static class Extension
    {

        /// <summary>
        /// 寫入 寄信txt (量化評估使用)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="folderPath"></param>
        public static void writeTxtLog(string msg, string folderPath)
        {
            try
            {
                string txtData = string.Empty;
                try //試著抓取舊資料
                {
                    txtData = File.ReadAllText(folderPath, System.Text.Encoding.Default);
                }
                catch { }
                writeTxt(folderPath, msg, txtData, true);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 寫入 txt
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="newtTxt"></param>
        /// <param name="oldTxt"></param>
        private static void writeTxt(string folderPath, string newtTxt, string oldTxt = null, bool flag = false)
        {
            if (!folderPath.IsNullOrWhiteSpace() && !newtTxt.IsNullOrWhiteSpace())
                using (FileStream fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write))
                {
                    var txtData = string.Empty;
                    if (!oldTxt.IsNullOrWhiteSpace())
                    {
                        txtData = oldTxt;
                        if (!flag)
                            txtData += string.Format("\r\n{0}", newtTxt);
                        else
                            txtData = (string.Format("{0}\r\n{1}", newtTxt, txtData));
                    }
                    else
                        txtData = newtTxt;
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.Write(txtData); //存檔
                    sw.Close();
                }
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }


}
