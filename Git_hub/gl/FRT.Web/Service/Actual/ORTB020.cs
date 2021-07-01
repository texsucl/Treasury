using FRT.Web.BO;
using FRT.Web.Models;
using Ionic.Zip;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FRT.Web.Service.Actual
{
    public class ORTB020
    {

        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void sendData(string VHR_NO1_DT)
        {
            try
            {

                string title = "快速付款銀行出款彙總表";
                List<reportParm> parms = new List<reportParm>();
                parms.Add(new reportParm() { key = "VHR_NO1_DT", value = VHR_NO1_DT });

                object obj = Activator.CreateInstance(Assembly.Load("FRT.Web").GetType($"FRT.Web.Report.Data.ORTB020"));
                MethodInfo[] methods = obj.GetType().GetMethods();
                MethodInfo mi = methods.FirstOrDefault(x => x.Name == "GetData");
                DataSet ds = (DataSet)mi.Invoke(obj, new object[] { parms });

                var lr = new LocalReport();
                lr.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"Report\\Rdlc\\ORTB020.rdlc");
                lr.DataSources.Clear();
                List<ReportParameter> _parms = new List<ReportParameter>();
                _parms.Add(new ReportParameter("Title", title));
                _parms.Add(new ReportParameter("UserId", "System"));
                lr.SetParameters(_parms);
                lr.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables[0]));

                string mimeType, encoding, extension;
                Warning[] warnings;
                string[] streams;
                var renderedBytes = lr.Render
                    (
                        "PDF",
                        null,
                        out mimeType,
                        out encoding,
                        out extension,
                        out streams,
                        out warnings
                    );

                var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { };
                if (_UATmailAccount == "Y")
                {
                    _mailTo.Add(new Tuple<string, string>(ConfigurationManager.AppSettings["testMail"], "測試帳號"));
                }
                getSendUser("ORTB020").ForEach(x =>
                {
                    _mailTo.Add(x);
                });
               
                Dictionary<string, Stream> attachment = new Dictionary<string, Stream>();
                using (ZipFile zip = new ZipFile(System.Text.Encoding.Default))
                {
                    var memSteam = new MemoryStream();
                    var streamWriter = new StreamWriter(memSteam);

                    zip.AddEntry($@"{title}.pdf", new MemoryStream(renderedBytes));

                    //ZipEntry e = zip.AddEntry(string.Format("{0}.pdf", _DisplayName), new MemoryStream(renderedBytes));
                    //e.Password = passwordZip;
                    //e.Encryption = EncryptionAlgorithm.WinZipAes256;

                    var ms = new MemoryStream();
                    ms.Seek(0, SeekOrigin.Begin);

                    zip.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Flush();

                    attachment.Add(string.Format("{0}.zip", title), ms);
                }

                var sms = new SendMail.SendMailSelf();
                sms.smtpPort = 25;
                sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                sms.mailAccount = ConfigurationManager.AppSettings["smtpSender"];
                var msg = sms.Mail_Send(
                   new Tuple<string, string>(sms.mailAccount, $@"{title} 通知"),
                   _mailTo,
                   null,
                   "報表通知",
                   $@"結果詳附件",
                   false,
                   attachment
                   );
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private List<Tuple<string, string>> getSendUser(string PGM_ID)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            try 
            {
                List<string> userIds = new List<string>();

                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = string.Empty;
                        sql = $@"
select SEND_ID from FGLSEND0 
where PGM_ID = :PGM_ID 
WITH UR;
";
                        com.Parameters.Add("PGM_ID", PGM_ID);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            userIds.Add(dbresult["SEND_ID"]?.ToString()?.Trim());
                        }
                        com.Dispose();
                    }
                }
                if (userIds.Any())
                {
                    using (DB_INTRAEntities db_intra = new DB_INTRAEntities())
                    {
                        var emplys = db_intra.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
                        userIds.ForEach(x =>
                        {
                            var _emply = emplys.FirstOrDefault(y => y.USR_ID == x);
                            if (_emply != null)
                            {
                                result.Add(new Tuple<string, string>(_emply.EMAIL, _emply.EMP_NAME));
                            }
                        });                       
                    }
                }
            }
            catch 
            { 
                
            }
            return result;

        }
    }
}