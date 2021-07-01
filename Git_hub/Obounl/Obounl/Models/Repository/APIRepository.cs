using Obounl.Daos;
using Obounl.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Obounl.Utility;
using Newtonsoft.Json;
using Obounl.Infrastructure;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using Obounl.Models.Interface;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Configuration;

namespace Obounl.Models.Repository
{
    public class APIRepository : IAPIRepository
    {
        /// <summary>
        /// 服務人員收集完成合約資料後如需申請即時電訪，需先將資料以API方式傳送至電訪系統中建立電訪名單
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MSGReturnModel<string> Contract_Data_Insert(string data)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();            
            string Code = string.Empty;
            DateTime dtn = DateTime.Now;
            var _dtnDate = dtn.Date;

            try
            {
                var _time = (dtn.Hour * 100 + dtn.Minute);
                int _SMemo1 = 0;
                int _SMemo2 = 0;
                string sql = $@"
SELECT [Stype]
      ,[Code]
      ,[SDesc]
      ,[SMemo1]
      ,[SMemo2]
      ,[SMemo3]
      ,[RecallDay]
      ,[DeleteFlag]
  FROM [dbCTI].[dbo].[tblSysCode]
  where Stype = @Stype and Code = @Code ";
                var _tblSysCode = new MSSql().Query<tblSysCode>(sql, new { Stype = "workday", Code = "workday" }, null);
                if (!_tblSysCode.Item1.IsNullOrWhiteSpace())
                {
                    send("A0007", _tblSysCode.Item1);
                    result.DESCRIPTION = "系統錯誤,請洽系統負責人.";
                    result.Datas = "A0007";
                    return result;
                }
                var _tblSysCode_f = _tblSysCode.Item2.FirstOrDefault();
                if (_tblSysCode_f != null)
                {                   
                    Int32.TryParse(_tblSysCode_f.SMemo1, out _SMemo1);
                    Int32.TryParse(_tblSysCode_f.SMemo2, out _SMemo2);
                }
                if (_SMemo1 <= _time && _time <= _SMemo2) 
                {
                    string sql2 = $@"
SELECT [svrid]
      ,[caldtime]
      ,[daytype]
      ,[duration]
      ,[dscpt]
  FROM [dbCTI].[dbo].[cal]
  where caldtime >= @caldtime_s 
  and   caldtime < @caldtime_e ";
                    var _cal = new MSSql().Query<cal>(sql2, new { caldtime_s = _dtnDate, caldtime_e = _dtnDate.AddDays(1) }, null).Item2.FirstOrDefault();
                    if(_cal == null || _cal.daytype == "H")
                        Code = "A0006"; //非服務時段
                }
                else //非設定區段時間內
                {
                    Code = "A0006"; //非服務時段
                }
            }
            catch (Exception ex)
            {
                var _ex = ex.exceptionMessage();
                NlogSet($@"驗證時間 Error , Msg => {_ex}", null);
                NlogSet($@"驗證時間 Error , Msg => {_ex}", null, Nlog.Error);
            }
            if (Code == "A0006")
            {
                result.DESCRIPTION = "非服務時段";
                result.Datas = Code;
                return result;
            }
            try
            {
                Code = "A0000"; //Start
                #region AES解密
                var _aes = data.AESDecrypt(null, false);
                #endregion
                if (_aes.Item1) //AES 解密成功
                {
                    Code = "A0001"; //AES 解密成功
                    #region Deserialize
                    var _data = JsonConvert.DeserializeObject<Tel_Visit_InputData>(_aes.Item2);
                    Code = "A0002"; //資料 Json Deserialize 成功
                    #endregion

                    #region 資料檢核 & 新增資料
                    List<string> _ValidationMsg = new List<string>();
                    var _Tel_Visit_InputData_pros = new Tel_Visit_InputData().GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                    var _Contract_pros = new Contract().GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                    var _Customer_pros = new Customer().GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                    var _Agent_pros = new Agent().GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                    var _PolicyBenfit_pros = new PolicyBenfit().GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
                    var _Tel_Visit_InputData_context = new ValidationContext(_data, null, null);
                    var _Tel_Visit_InputData_ValidationResult = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(_data, _Tel_Visit_InputData_context, _Tel_Visit_InputData_ValidationResult, true))
                    {
                        _Tel_Visit_InputData_ValidationResult.ForEach(x =>
                        {
                            var m = x.MemberNames.FirstOrDefault()?.ToString();
                            var p = _Tel_Visit_InputData_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                            var val = (p == null) ? null : (p.GetValue(_data))?.ToString();
                            if (!val.IsNullOrWhiteSpace())
                                _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                            else
                                _ValidationMsg.Add(x.ErrorMessage);
                        });
                    }

                    if (!DateTime.TryParse(_data.UploadDate, out DateTime uploadDateTime))
                    {
                        _ValidationMsg.Add("電訪抽件日 日期轉換失敗");
                    }
                    if (_data.PolicyCases != null)
                    {
                        foreach (var _PolicyCase in _data.PolicyCases)
                        {
                            var _Contract_context = new ValidationContext(_PolicyCase, null, null);
                            var _Contract_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_PolicyCase, _Contract_context, _Contract_ValidationResult, true))
                            {
                                _Contract_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _Contract_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_PolicyCase))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                            if (!_PolicyCase.SubmitDate.IsNullOrWhiteSpace() && stringToDateTimeN(_PolicyCase.SubmitDate) == null)
                            {
                                _ValidationMsg.Add("契約始期 日期轉換失敗");
                            }
                            if (_PolicyCase.Customers != null)
                            {
                                foreach (var _Customer in _PolicyCase.Customers)
                                {
                                    var _Customer_context = new ValidationContext(_Customer, null, null);
                                    var _Customer_ValidationResult = new List<ValidationResult>();
                                    if (!Validator.TryValidateObject(_Customer, _Customer_context, _Customer_ValidationResult, true))
                                    {
                                        _Customer_ValidationResult.ForEach(x =>
                                        {
                                            var m = x.MemberNames.FirstOrDefault()?.ToString();
                                            var p = _Customer_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                            var val = (p == null) ? null : (p.GetValue(_Customer))?.ToString();
                                            if (!val.IsNullOrWhiteSpace())
                                                _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                            else
                                                _ValidationMsg.Add(x.ErrorMessage);
                                        });
                                    }
                                    if (!_Customer.CustBirthday.IsNullOrWhiteSpace() && stringToDateTimeN(_Customer.CustBirthday) == null)
                                    {
                                        _ValidationMsg.Add("客戶生日 日期轉換失敗");
                                    }
                                    List<string> _CustContGen_1_4 = new List<string>() { "1", "4" };
                                    List<string> _CustContGen_1_2_4_5 = new List<string>() { "1", "2", "4", "5" };
                                    if (!_Customer.CustContGen.IsNullOrWhiteSpace())
                                    {
                                        //if (_CustContGen_1_4.Contains(_Customer.CustContGen) && _Customer.CustEmail.IsNullOrWhiteSpace())
                                        //    _ValidationMsg.Add("要保人Mail(CustEmail) 必填");
                                        if (_CustContGen_1_2_4_5.Contains(_Customer.CustContGen) && _Customer.CustAddr.IsNullOrWhiteSpace())
                                            _ValidationMsg.Add("戶籍地址(CustAddr) 必填");
                                        if (_CustContGen_1_4.Contains(_Customer.CustContGen) && _Customer.CustRecvAddr.IsNullOrWhiteSpace())
                                            _ValidationMsg.Add("收費地址(CustRecvAddr) 必填");
                                        //if (_CustContGen_1_2_4_5.Contains(_Customer.CustContGen) && _Customer.CustMoblie.IsNullOrWhiteSpace())
                                        //    _ValidationMsg.Add("行動電話(CustMoblie) 必填");
                                        //if (_CustContGen_1_2_4_5.Contains(_Customer.CustContGen) && _Customer.CustTEL.IsNullOrWhiteSpace())
                                        //    _ValidationMsg.Add("戶籍電話(CustTEL) 必填");
                                        //if (_CustContGen_1_2_4_5.Contains(_Customer.CustContGen) && _Customer.CustRecvTEL.IsNullOrWhiteSpace())
                                        //    _ValidationMsg.Add("收費電話(CustRecvTEL) 必填");
                                        if (_CustContGen_1_2_4_5.Contains(_Customer.CustContGen) && _Customer.CustMenuValue.IsNullOrWhiteSpace())
                                            _ValidationMsg.Add("受訪者與要被保人關係(CustMenuValue) 必填");
                                    }
                                }
                            }
                            else
                            {
                                _ValidationMsg.Add($@"客戶資料 必填");
                            }

                            if (_PolicyCase.PolicyBenfit != null)
                            {
                                foreach (var _PolicyBenfit in _PolicyCase.PolicyBenfit)
                                {
                                    var _PolicyBenfit_context = new ValidationContext(_PolicyBenfit, null, null);
                                    var _PolicyBenfit_ValidationResult = new List<ValidationResult>();
                                    if (!Validator.TryValidateObject(_PolicyBenfit, _PolicyBenfit_context, _PolicyBenfit_ValidationResult, true))
                                    {
                                        _PolicyBenfit_ValidationResult.ForEach(x =>
                                        {
                                            var m = x.MemberNames.FirstOrDefault()?.ToString();
                                            var p = _PolicyBenfit_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                            var val = (p == null) ? null : (p.GetValue(_PolicyBenfit))?.ToString();
                                            if (!val.IsNullOrWhiteSpace())
                                                _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                            else
                                                _ValidationMsg.Add(x.ErrorMessage);
                                        });
                                    }
                                }
                            }
                            else
                            {
                                _ValidationMsg.Add($@"受益人資料 必填");
                            }
                        }
                    }
                    else
                    {
                        _ValidationMsg.Add($@"合約要保書資料 必填");
                    }
                    if (_data.Agent != null)
                    {
                        foreach (var _Agent in _data.Agent)
                        {
                            var _Agent_context = new ValidationContext(_Agent, null, null);
                            var _Agent_ValidationResult = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(_Agent, _Agent_context, _Agent_ValidationResult, true))
                            {
                                _Agent_ValidationResult.ForEach(x =>
                                {
                                    var m = x.MemberNames.FirstOrDefault()?.ToString();
                                    var p = _Agent_pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                                    var val = (p == null) ? null : (p.GetValue(_Agent))?.ToString();
                                    if (!val.IsNullOrWhiteSpace())
                                        _ValidationMsg.Add((m + " : " + val) + " Error : " + x.ErrorMessage);
                                    else
                                        _ValidationMsg.Add(x.ErrorMessage);
                                });
                            }
                        }
                    }
                    else
                    {
                        _ValidationMsg.Add($@"業務員資料 必填");
                    }
                    if (_ValidationMsg.Any()) //資料檢核失敗
                    {
                        result.DESCRIPTION = $@"資料檢核失敗 => {string.Join(",", _ValidationMsg)}";
                    }
                    else //資料檢核成功
                    {
                        Code = "A0003"; //資料 欄位檢核 成功
                        var addData = new CampMPool();
                        addData.CaseNo = _data.CaseNo;
                        addData.CaseSeq = "";
                        addData.InsDT = TypeTransfer.stringToDateTime(_data.UploadDate);
                        addData.MData = _aes.Item2;
                        string sql = $@"
insert into  [dbo].[CampMPool]
           ([CaseNo]
           ,[CaseSeq]
           ,[InsDT]
           ,[MData])
     VALUES
           (@CaseNo, 
            @CaseSeq,
            @InsDT,
            @MData)
";
                        var _Execute = new MSSql().Execute(sql, addData, null);
                        if (_Execute.Item1.IsNullOrWhiteSpace() && _Execute.Item2 == 1)
                        {
                            Code = "A0004"; //新增CampMPool資料 成功
                            result.RETURN_FLAG = true;
                            var _pia = new PIA().AddPIA(new PIA_LOG_MAIN()
                            {
                                TRACKING_TYPE = "A", //個資存取紀錄
                                ACCESS_ACCOUNT = "", //使用者帳號或員工編號
                                ACCOUNT_NAME = "", //使用者姓名或員工姓名
                                FROM_IP = new Common().GetIp(), //記載來源IP或終端機ID
                                ACCESS_DATE = dtn.Date, //記載執行日期
                                ACCESS_TIME = dtn.TimeOfDay, //記載執行時間
                                PROGFUN_NAME = $@"Contract_Data_Insert", //執行程式名稱或交易/功能之代號/名稱
                                ACCESSOBJ_NAME = $@"CampMPool", //系統檔案名稱或存取物件對象名稱(例如TABLE / VIEW名稱）
                                EXECUTION_TYPE = "A", //新增
                                EXECUTION_CONTENT = $@"",//記載完整的資料存取動作，如執行所使用輸入條件、檔案存取紀錄或SQL Statement紀錄等
                                AFFECT_ROWS = 1, //記載執行動作之結果筆數或影響筆數
                                //PIA_OWNER1 = "??",
                                PIA_TYPE = "11111"
                            });
                            if(_pia.Item2 == 1)
                                Code = "A0005"; //加入PIA 成功 (全部成功)
                        }
                        else
                        {
                            result.DESCRIPTION = _Execute.Item1;
                        }
                    }
                    #endregion
                }
                else
                {
                    result.DESCRIPTION = "AES解密失敗";
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }
            finally
            {            
                //如果最後狀態為A0003 => 新增CampMPool資料失敗 通知IT
                //如果最後狀態為A0004 => 新增PIA資料失敗 通知IT
                if (Code == "A0003" || Code == "A0004")
                {
                    send(Code, result.DESCRIPTION);

                    result.DESCRIPTION = "系統錯誤,請洽系統負責人.";
                }
            }
            result.Datas = Code;
            return result;
        }

        private static void send(string Code, string Description)
        {
            var _body = $@"Code = {Code}, DESCRIPTION = {Description}";
            var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
            var mailAccount = ConfigurationManager.AppSettings["mailAccount"] ?? string.Empty;
            var mailSend = ConfigurationManager.AppSettings["mailSend"] ?? string.Empty;
            List<Tuple<string, string>> sendMails = new List<Tuple<string, string>>();
            foreach (var item in mailSend.Split(';'))
            {
                if (!item.IsNullOrWhiteSpace())
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
            }
            var sms = new SendMail.SendMailSelf();
            var sendStyle = ConfigurationManager.AppSettings["sendStyle"] ?? "A"; //寄送判斷

            #region M Plus
            if (sendStyle == "A" || sendStyle == "P")
            {
                try
                {
                    sms.Mplus_BaseAddress = ConfigurationManager.AppSettings["Mplus_BaseAddress"];
                    sms.Mplus_Url = ConfigurationManager.AppSettings["Mplus_Url"];
                    var _systemName = "OUTBOUND"; //(必要)於Web Service平台註冊的系統名稱
                    var _mplusId = "DCC"; //(必要)發送訊息的企業帳號代碼
                    var _sendType = "T"; //(必要)寄送類型 (ex: T => 文字,U => 圖片URL路徑,I => 圖片檔案路徑,圖片容量大小不可超過500KB)
                    var _hideSysName = "Y"; //隱藏系統名稱
                    var _policyNo = ""; //保單號碼
                    var _senderId = ""; //發訊人員5碼AD帳號
                    var _senderName = ""; //發訊人員姓名
                    var _unitId = ""; //發訊人員單位代碼
                    foreach (var item in sendMails)
                    {
                        NlogSet($@"寄送 M_Plus (A001 監控) target:{item.Item1} , body:{_body} ", null);
                        /// <returns>參數1:成功失敗 , 參數2:Api的returnCode 中文 , 參數3:Api的returnMsg , 參數4:guid</returns>
                        var mplus = sms.Mplus_Send(
                            _systemName,
                            _mplusId,
                            item.Item1,
                            _sendType,
                            _body,
                            _policyNo,
                            _hideSysName,
                            _senderId,
                            _senderName,
                            _unitId
                            );
                        NlogSet($@"寄送 M_Plus (A001 監控) request:{item.Item1} , returnCode:{mplus.Item2} , returnMsg:{mplus.Item3} , guid:{mplus.Item4} ", null);
                    }
                }
                catch (Exception ex)
                {
                    var _ex = ex.exceptionMessage();
                    NlogSet($@"Send Mplus Error , Msg => {_ex}", null);
                    NlogSet($@"Send Mplus Error , Msg => {_ex}", null, Nlog.Error);
                }
            }
            #endregion

            #region Mail
            if (sendStyle == "A" || sendStyle == "M")
            {
                try
                {
                    if (_UATmailAccount == "Y")
                    {
                        sendMails.Add(new Tuple<string, string>(mailAccount, "測試帳號-Ex2016ap"));
                    }
                    sms.smtpPort = 25;
                    sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                    sms.mailAccount = mailAccount;
                    NlogSet($@"寄送 Mail (A001 監控)  body:{_body} ", null);
                    var msg = sms.Mail_Send(
                       new Tuple<string, string>(sms.mailAccount, "即時電訪"),
                       sendMails,
                       null,
                       "A001 監控",
                       _body,
                       false
                       );
                    if (msg.IsNullOrWhiteSpace())
                        NlogSet($@"寄送 Mail (A001 監控) 成功 ", null);
                    else
                        NlogSet($@"寄送 Mail (A001 監控) 失敗 : {msg} ", null);
                }
                catch (Exception ex)
                {
                    var _ex = ex.exceptionMessage();
                    NlogSet($@"Send Mail Error , Msg => {_ex}", null);
                    NlogSet($@"Send Mail Error , Msg => {_ex}", null, Nlog.Error);
                }
            }

            #endregion
        }

        /// <summary>
        /// string 轉 datetime?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static DateTime? stringToDateTimeN(string value)
        {
            DateTime t = new DateTime();
            if (value.IsNullOrWhiteSpace())
                return null;
            if (DateTime.TryParse(value, out t))
                return t;
            return null;
        }
    }
}