using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;
using Ionic.Zip;
using Microsoft.Reporting.WebForms;
using System.Data.EasycomClient;
using System.Data.Common;

namespace FRT.Web.Service.Actual
{
    public class GoujiReport
    {
        public MSGReturnModel sendReport(IORT0105ReportModel model, FRT_CROSS_SYSTEM_CHECK check ,string userId ,string date_s = null , string date_e = null)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                List<Tuple<string, byte[]>> sendDatas = new List<Tuple<string, byte[]>>();
                var _title = "報表Title";
                var _reportType = getReportType(check);
                var _dates = getReportDate(check, date_s, date_e);
                var _mailGroup = check.mail_group;
                var _Cross_Systems = new ORT0106().getCross_System();
                _title = _Cross_Systems.FirstOrDefault(x => x.Value.IndexOf($@"{check.type}_{check.kind}") > -1)?.Text;
                switch (_reportType)
                {
                    case "ORT0105AP1":
                        ORT0105AP1ReportModel _AP1reportModel = (ORT0105AP1ReportModel)model;
                        List<reportParm> _parm_AP1_main = new List<reportParm>();
                        _parm_AP1_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_AP1_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                        _parm_AP1_main.Add(new reportParm() { key = "UserId", value = userId });
                        DataSet ds_AP1main = new DataSet();
                        ds_AP1main.Tables.Add(_AP1reportModel.model.Item1.ToDataTable());
                        var _report_AP1_main = SetPdfReport($@"{_reportType}A", ds_AP1main, _parm_AP1_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_AP1_main.Item2));
                        if (_AP1reportModel.model.Item2)
                        {
                            List<reportParm> _parm_AP1_sub = new List<reportParm>();
                            var _t_AP1_sub = $@"{_title}差異明細表";
                            _parm_AP1_sub.Add(new reportParm() { key = "Title", value = _t_AP1_sub });
                            _parm_AP1_sub.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                            _parm_AP1_sub.Add(new reportParm() { key = "UserId", value = userId });
                            DataSet ds_AP1_sub = new DataSet();
                            ds_AP1_sub.Tables.Add(_AP1reportModel.model.Item3.ToDataTable());
                            var _report_AP1_sub = SetPdfReport($@"{_reportType}B", ds_AP1_sub, _parm_AP1_sub);
                            sendDatas.Add(new Tuple<string, byte[]>(_t_AP1_sub, _report_AP1_sub.Item2));
                        }
                        break;
                    case "ORT0105AP2":
                        ORT0105AP2ReportModel _AP2reportModel = (ORT0105AP2ReportModel)model;
                        List<reportParm> _parm_AP2_main = new List<reportParm>();
                        _parm_AP2_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_AP2_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                        _parm_AP2_main.Add(new reportParm() { key = "UserId", value = userId });
                        DataSet ds_AP2main = new DataSet();
                        ds_AP2main.Tables.Add(_AP2reportModel.model.Item1.ToDataTable());
                        var _report_AP2_main = SetPdfReport($@"{_reportType}A", ds_AP2main, _parm_AP2_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_AP2_main.Item2));
                        if (_AP2reportModel.model.Item2)
                        {
                            List<reportParm> _parm_AP2_sub = new List<reportParm>();
                            var _t_AP2_sub = $@"{_title}差異明細表";
                            _parm_AP2_sub.Add(new reportParm() { key = "Title", value = _t_AP2_sub });
                            _parm_AP2_sub.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                            _parm_AP2_sub.Add(new reportParm() { key = "UserId", value = userId });
                            DataSet ds_AP2_sub = new DataSet();
                            ds_AP2_sub.Tables.Add(_AP2reportModel.model.Item3.ToDataTable());
                            var _report_AP2_sub = SetPdfReport($@"{_reportType}B", ds_AP2_sub, _parm_AP2_sub);
                            sendDatas.Add(new Tuple<string, byte[]>(_t_AP2_sub, _report_AP2_sub.Item2));
                        }
                        break;
                    case "ORT0105AP3":
                        ORT0105AP2ReportModel _AP3reportModel = (ORT0105AP2ReportModel)model;
                        List<reportParm> _parm_AP3_main = new List<reportParm>();
                        _parm_AP3_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_AP3_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                        _parm_AP3_main.Add(new reportParm() { key = "UserId", value = userId });
                        DataSet ds_AP3_main = new DataSet();
                        ds_AP3_main.Tables.Add(_AP3reportModel.model.Item1.ToDataTable());
                        var _report_AP3_main = SetPdfReport($@"{_reportType}A", ds_AP3_main, _parm_AP3_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_AP3_main.Item2));
                        if (_AP3reportModel.model.Item2)
                        {
                            List<reportParm> _parm_AP3_sub = new List<reportParm>();
                            var _t_AP3_sub = $@"{_title}差異明細表";
                            _parm_AP3_sub.Add(new reportParm() { key = "Title", value = _t_AP3_sub });
                            _parm_AP3_sub.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                            _parm_AP3_sub.Add(new reportParm() { key = "UserId", value = userId });
                            DataSet ds_AP3_sub = new DataSet();
                            ds_AP3_sub.Tables.Add(_AP3reportModel.model.Item3.ToDataTable());
                            var _report_AP3_sub = SetPdfReport($@"{_reportType}B", ds_AP3_sub, _parm_AP3_sub);
                            sendDatas.Add(new Tuple<string, byte[]>(_t_AP3_sub, _report_AP3_sub.Item2));
                        }
                        break;
                    case "ORT0105BD1":
                        _title = @"AS400 及 Wanpie 比對結果差異報表-銀存銷帳-已銷帳";
                        ORT0105BD1ReportModel _BD1reportModel = (ORT0105BD1ReportModel)model;
                        var _BD1reportModel_item1 = _BD1reportModel.model.Item1;
                        List<reportParm> _parm_BD1_main = new List<reportParm>();
                        _parm_BD1_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_BD1_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                        _parm_BD1_main.Add(new reportParm() { key = "UserId", value = userId });
                        _parm_BD1_main.Add(new reportParm() { key = "AS400_AMT", value = _BD1reportModel_item1.AS400_AMT });
                        _parm_BD1_main.Add(new reportParm() { key = "Wanpie_AMT", value = _BD1reportModel_item1.Wanpie_AMT });
                        _parm_BD1_main.Add(new reportParm() { key = "Diff_AMT", value = _BD1reportModel_item1.Diff_AMT });
                        _parm_BD1_main.Add(new reportParm() { key = "Compare_Result", value = _BD1reportModel_item1.Compare_Result });
                        DataSet ds_BD1main = new DataSet();
                        ds_BD1main.Tables.Add(_BD1reportModel.model.Item2.ToDataTable());
                        var _report_BD1_main = SetPdfReport($@"{_reportType}", ds_BD1main, _parm_BD1_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_BD1_main.Item2));
                        break;
                    case "ORT0105BD2":
                        _title = @"AS400 及 Wanpie 比對結果差異報表-銀存銷帳-未銷帳";
                        ORT0105BD1ReportModel _BD2reportModel = (ORT0105BD1ReportModel)model;
                        var _BD2reportModel_item1 = _BD2reportModel.model.Item1;
                        List<reportParm> _parm_BD2_main = new List<reportParm>();
                        _parm_BD2_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_BD2_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item2}~{_dates.Item3}").dateFormat() });
                        _parm_BD2_main.Add(new reportParm() { key = "UserId", value = userId });
                        _parm_BD2_main.Add(new reportParm() { key = "AS400_AMT", value = _BD2reportModel_item1.AS400_AMT });
                        _parm_BD2_main.Add(new reportParm() { key = "AS400_Count", value = _BD2reportModel_item1.AS400_Count });
                        _parm_BD2_main.Add(new reportParm() { key = "Wanpie_AMT", value = _BD2reportModel_item1.Wanpie_AMT });
                        _parm_BD2_main.Add(new reportParm() { key = "Wanpie_Count", value = _BD2reportModel_item1.Wanpie_Count });
                        _parm_BD2_main.Add(new reportParm() { key = "Diff_AMT", value = _BD2reportModel_item1.Diff_AMT });
                        _parm_BD2_main.Add(new reportParm() { key = "Diff_Count", value = _BD2reportModel_item1.Diff_Count });
                        _parm_BD2_main.Add(new reportParm() { key = "Compare_Result", value = _BD2reportModel_item1.Compare_Result });
                        _parm_BD2_main.Add(new reportParm() { key = "Deadline", value = _BD2reportModel_item1.Deadline });
                        DataSet ds_BD2main = new DataSet();
                        ds_BD2main.Tables.Add(_BD2reportModel.model.Item2.ToDataTable());
                        var _report_BD2_main = SetPdfReport($@"{_reportType}", ds_BD2main, _parm_BD2_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_BD2_main.Item2));
                        break;
                    case "ORT0105NP3":
                        ORT0105NP1ReportModel _NP3reportModel = (ORT0105NP1ReportModel)model;
                        var _NP3reportModel_item1 = _NP3reportModel.model.Item1.First();
                        List<reportParm> _parm_NP3_main = new List<reportParm>();
                        _parm_NP3_main.Add(new reportParm() { key = "Title", value = _title });
                        _parm_NP3_main.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item3}").dateFormat() });
                        _parm_NP3_main.Add(new reportParm() { key = "UserId", value = userId });
                        _parm_NP3_main.Add(new reportParm() { key = "NO", value = _NP3reportModel_item1.NO});
                        _parm_NP3_main.Add(new reportParm() { key = "Kind", value = _NP3reportModel_item1.Kind });
                        DataSet ds_NP3_main = new DataSet();
                        ds_NP3_main.Tables.Add(_NP3reportModel.model.Item1.ToDataTable());
                        var _report_NP3_main = SetPdfReport($@"{_reportType}A", ds_NP3_main, _parm_NP3_main);
                        sendDatas.Add(new Tuple<string, byte[]>($@"{_title}", _report_NP3_main.Item2));
                        if (_NP3reportModel.model.Item2)
                        {
                            List<reportParm> _parm_NP3_sub = new List<reportParm>();
                            var _NP3reportModel_item3 = _NP3reportModel.model.Item3.First();
                            var _t_NP3_sub = $@"{_title}差異明細表";
                            _parm_NP3_sub.Add(new reportParm() { key = "Title", value = _t_NP3_sub });
                            _parm_NP3_sub.Add(new reportParm() { key = "Date", value = ($@"{_dates.Item3}").dateFormat() });
                            _parm_NP3_sub.Add(new reportParm() { key = "UserId", value = userId });
                            _parm_NP3_sub.Add(new reportParm() { key = "NO", value = _NP3reportModel_item3.NO });
                            _parm_NP3_sub.Add(new reportParm() { key = "Kind", value = _NP3reportModel_item3.Kind });
                            DataSet ds_NP3_sub = new DataSet();
                            ds_NP3_sub.Tables.Add(_NP3reportModel.model.Item3.ToDataTable());
                            var _report_NP3_sub = SetPdfReport($@"{_reportType}B", ds_NP3_sub, _parm_NP3_sub);
                            sendDatas.Add(new Tuple<string, byte[]>(_t_NP3_sub, _report_NP3_sub.Item2));
                        }
                        break;

                }

                var mail_key = check.mail_key?.Trim() ?? string.Empty;
                var _dtn = DateTime.Now.ToString("yyyyMMdd");
                Dictionary<string, Stream> attachment = new Dictionary<string, Stream>();
                using (ZipFile zip = new ZipFile(System.Text.Encoding.Default))
                {
                    var memSteam = new MemoryStream();
                    var streamWriter = new StreamWriter(memSteam);

                    sendDatas.ForEach(x =>
                    {
                        ZipEntry e = zip.AddEntry($@"{x.Item1}.pdf", new MemoryStream(x.Item2));
                        e.Password = $@"{mail_key}{_dtn}";
                        e.Encryption = EncryptionAlgorithm.WinZipAes256;
                    });

                    //ZipEntry e = zip.AddEntry(string.Format("{0}.pdf", _DisplayName), new MemoryStream(renderedBytes));
                    //e.Password = DateTime.Now.ToString("yyyyMMdd");
                    //e.Encryption = EncryptionAlgorithm.WinZipAes256;

                    var ms = new MemoryStream();
                    ms.Seek(0, SeekOrigin.Begin);

                    zip.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Flush();
                    //NLog.LogManager.GetCurrentClassLogger().Info(ms.Length);
                    attachment.Add(string.Format("{0}.zip", _title), ms);
                }

                List<UserBossModel> _mails = new MailUtil().getMailGrpId(_mailGroup);
                var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { };
                if (_UATmailAccount == "Y")
                {
                    _mailTo.Add(new Tuple<string, string>(ConfigurationManager.AppSettings["testMail"], "測試帳號"));
                }
                _mails.ForEach(x =>
                {
                    _mailTo.Add(new Tuple<string, string>(x.empMail, x.usrId));
                });
                var sms = new SendMail.SendMailSelf();
                sms.smtpPort = 25;
                sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                sms.mailAccount = ConfigurationManager.AppSettings["smtpSender"];
                var msg = sms.Mail_Send(
                   new Tuple<string, string>(sms.mailAccount, $@"{_title} 通知"),
                   _mailTo,
                   null,
                   $@"跨系統勾稽檢核報表＿「{_title}」",
                   $@"密碼＿科別代碼及系統寄件日期（西元年月日）",
                   false,
                   attachment
                   );
                if (!msg.IsNullOrWhiteSpace())
                { 
                    NLog.LogManager.GetCurrentClassLogger().Error(msg);
                    result.DESCRIPTION = $@"寄信失敗 : {msg}";
                }
                else
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($@"跨系統勾稽檢核報表＿「{_title}」寄信成功!");
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = "寄信成功";
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
            }
            return result;
        }

        public Tuple<string, byte[]> SetPdfReport(string rdlcName, DataSet ds, List<reportParm> rps)
        {
            var lr = new LocalReport();
            lr.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"Report\\Rdlc\\{rdlcName}.rdlc");
            lr.DataSources.Clear();
            List<ReportParameter> _parm = new List<ReportParameter>();
            if (rps.Any())
                _parm.AddRange(rps.Select(x => new ReportParameter(x.key, x.value)));
            if (_parm.Any())
                lr.SetParameters(_parm);
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                lr.DataSources.Add(new ReportDataSource("DataSet" + (i + 1).ToString(), ds.Tables[i]));
            }
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
            return new Tuple<string, byte[]>(rdlcName, renderedBytes);
        }

        /// <summary>
        /// 報表日期 (1)執行日 (2)起日_西元年 (3)迄日_西元年 (4)起日_民國年 (5)迄日_民國年
        /// </summary>
        /// <param name="check"></param>
        /// <returns>(1)執行日 (2)起日_西元年 (3)迄日_西元年 (4)起日_民國年 (5)迄日_民國年</returns>
        public Tuple<DateTime, string, string , string ,string> getReportDate (FRT_CROSS_SYSTEM_CHECK check, string date_s = null, string date_e = null)
        {
            DateTime runDate = DateTime.MinValue;
            DateTime dtn = DateTime.Now;
            string sDate = string.Empty;
            string eDate = string.Empty;
            string sDate_t = string.Empty;
            string eDate_t = string.Empty;

            DateTime _date_s = DateTime.MinValue;
            DateTime _date_e = DateTime.MinValue;
            DateTime _date_c = DateTime.MinValue;

            if (date_s != null && date_e != null)
            {
                DateTime.TryParse(date_s, out _date_s);
                DateTime.TryParse(date_e, out _date_e);
            }
            if (_date_s != DateTime.MinValue && _date_e != DateTime.MinValue)
            {
                if (_date_s > _date_e)
                {
                    _date_c = _date_s;
                    _date_s = _date_e;
                    _date_e = _date_c;
                }
                runDate = DateTime.Now.Date;
                sDate = _date_s.dateToString();
                sDate_t = _date_s.dateToStringT();
                eDate = _date_e.dateToString();
                eDate_t = _date_e.dateToStringT();
            }
            else
            {
                switch (check.frequency)
                {
                    case "m": //選擇為月 即為每個月執行一次
                              //執行工作天為0時，程式執行日會是109.10.31，資料迄日109.10.31
                              //執行工作天為1時，程式執行日會是109.11.01，資料迄日109.10.31
                              //執行工作天為2時，程式執行日會是109.11.02，資料迄日109.10.31
                        var _eDate_m = new DateTime(dtn.Year, dtn.Month, 1).AddDays(-1); //上個月最後一天
                        if (check.frequency_value == 0) //執行工作天為0
                        {
                            runDate = new DateTime(dtn.Year, dtn.Month, 1)
                                .AddMonths(1).AddDays(-1); //當月最後一天 
                            eDate = runDate.dateToString();
                            eDate_t = runDate.dateToStringT();

                            //runDate = GetSTDDate(_eDate_m.AddDays(1).dateToStringT(), 1, "<"); //上個月最後一天工作天
                            //eDate = _eDate_m.dateToString();
                            //eDate_t = _eDate_m.dateToStringT();
                        }
                        else //執行工作天不為0
                        {
                            //runDate = new DateTime(dtn.Year, dtn.Month, 1)
                            //    .AddDays(-1) //上個月最後一天
                            //    .AddDays(check.frequency_value); //加上設定參數日
                            runDate = GetSTDDate(_eDate_m.dateToStringT(), check.frequency_value);
                            eDate = _eDate_m.dateToString();
                            eDate_t = _eDate_m.dateToStringT();
                        }
                        break;
                    case "d": //選擇為日 即為每天執行
                              //系統基準日 0時，程式執行日會是109.11.02，資料迄日109.11.02
                              //系統基準日 -1時，程式執行日會是109.11.02，資料迄日109.11.01
                              //系統基準日 -2時，程式執行日會是109.11.02，資料迄日109.10.31
                        runDate = dtn.Date;  //系統日當日
                        if (check.frequency_value == 0) //系統基準日為0
                        {
                            eDate = runDate.dateToString();
                            eDate_t = runDate.dateToStringT();
                        }
                        else
                        {
                            var _eDate = runDate.AddDays(check.frequency_value); //系統日當日 加上設定參數日
                            eDate = _eDate.dateToString();
                            eDate_t = _eDate.dateToStringT();
                        }
                        break;
                }
                switch (check.start_date_type)
                {
                    case "1": //固定日期
                        sDate = check.start_date_value; //選擇固定日期者 即固定資料起日
                        sDate_t = sDate.stringToDate().dateToStringT();
                        break;
                    case "2": //執行前(月)
                              //例如：109.10.31執行時
                              //執行前 0月，資料區間起始日會為109.10.01
                              //執行前 1月，資料區間起始日會為109.09.01
                              //執行前23月，資料區間起始日會為107.11.01
                              //選擇執行前N月者 即固定資料起日為設定月1號
                        var _sDate_2 = new DateTime(runDate.Year, runDate.Month, 1)
                            .AddMonths(-1 * check.start_date_value.stringToInt());
                        sDate = _sDate_2.dateToString();
                        sDate_t = _sDate_2.dateToStringT();
                        break;
                    case "3": //同系統基準日
                        sDate = eDate; //選擇同基準日者 即每天執行 且資料起訖日為同一天
                        sDate_t = eDate_t;
                        break;
                    case "4": //執行前(日)
                              //例如：109.11.02執行時
                              //執行前1日，資料區間起始日會為109.11.01
                              //執行前5日，資料區間起始日會為109.10.28
                              //選擇執行前N日者 即每天執行 且頻率:日的系統基準日數字需 <= 執行前N日數字設定
                        var _sDate_4 = runDate.AddDays(-1 * check.start_date_value.stringToInt());
                        sDate = _sDate_4.dateToString();
                        sDate_t = _sDate_4.dateToStringT();
                        break;
                }
            }
            return new Tuple<DateTime, string, string, string, string>(runDate, sDate, eDate, sDate_t, eDate_t);
        }

        /// <summary>
        /// 取得指定日加設定工作日的日期(不包含指定日)
        /// </summary>
        /// <param name="TWDate">指定日(民國年月日)</param>
        /// <param name="std">工作日</param>
        /// <returns></returns>
        private DateTime GetSTDDate(string TWDate, int std, string oper = ">")
        {
            DateTime result = DateTime.Now;
            if (TWDate.IsNullOrWhiteSpace() || std < 1)
                return result;
            try
            {
                var _result = string.Empty;
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;              
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                    select ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) STDDate from (
                    select YEAR,MONTH,DAY from LGLCALE1 
                    where ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) {oper} :CDate
                    and corp_rest <> 'Y'
                    order by year,month,day
                    FETCH FIRST :STD ROWS ONLY)
                    order by year desc,month desc , day desc
                    FETCH FIRST 1 ROWS ONLY; ";
                        com.Parameters.Add($@"CDate", TWDate);
                        com.Parameters.Add($@"STD", std);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            _result = dbresult["STDDate"]?.ToString()?.Trim();
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
                result = _result.TWDateToDate().stringToDate();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return result;
        }

        public string getReportType(FRT_CROSS_SYSTEM_CHECK check)
        {
            string reportType = string.Empty;

            if (check.check_id != null && (check.type == null || check.kind == null))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var _check = db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking().FirstOrDefault(x => x.check_id == check.check_id);
                    if (_check != null)
                    {
                        check.type = _check.type;
                        check.kind = _check.kind;
                        check.mail_group = _check.mail_group;
                        check.mail_key = _check.mail_key;
                    }
                }
            }

            //if (check.type == "AP") //應付匯款
            //{
            //    if (check.kind == "1") //AS400_匯款檢核
            //        reportType = "ORT0105AP1";
            //    else if (check.kind == "2") //Wanpie_應付匯款檢核
            //        reportType = "ORT0105AP2";
            //    else if (check.kind == "3") //Wanpie_退匯檢核
            //        reportType = "ORT0105AP3";
            //}
            //else if (check.type == "BD") //銀存銷帳
            //{ 

            //}
            //else if (check.type == "NP") //應付票據
            //{

            //}
            reportType = $@"ORT0105{check.type}{check.kind}";
            return reportType;
        }
    }
}