using Dapper;
using DFHRIS.Interface;
using DFHRIS.model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DFHRIS
{
    class Program
    {
        static void Main(string[] args)
        {
            string DefaultConn = string.Empty;
            "DFHRIS START!".setMessage();
            StringBuilder sb = new StringBuilder();
            try
            {
                DateTime dtn = DateTime.Now;
                string strSql = string.Empty;
                List<IModel> DSDs = new List<IModel>();
                List<IModel> DSMs = new List<IModel>();
                List<V_EMPLY2> V_EMPs = new List<V_EMPLY2>();
                var conEFPDB = ConfigurationManager.ConnectionStrings["EFPDB"].ConnectionString;
                var conDFDBUT = ConfigurationManager.ConnectionStrings["DFDBUT"].ConnectionString;
                #region 抓取資料並轉換
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB_INTRA"].ConnectionString))
                {
                    conn.Open();
                    strSql = @"
select LTRIM(RTRIM(EMP_NO)) AS EMP_NO, LTRIM(RTRIM(USR_ID)) AS USR_ID from V_EMPLY2 ;
";
                    V_EMPs = conn.Query<V_EMPLY2>(strSql).ToList();
                }
                using (SqlConnection conn = new SqlConnection(conEFPDB))
                {
                    conn.Open();
                    strSql = @"
select 
UP_BU_DEPT_PRER_CD,
TW_DEPT_LEVEL,
TW_BU_DEPT_PRER_CD,
DEPT_NAME,
TW_DEPT_LEVEL,
MGR_EMPLID,
MGR_NAME,
DEPT_LVLNAME
From HRIS_BUDEPT
Where STATUS = @STATUS
and BU = @BU
and CHANGEMARK <> @CHANGEMARK
and not
( 
   TW_DEPT_LEVEL = @TW_DEPT_LEVEL 
   or
   TW_DEPT_LEVEL is null 
   or
   TW_DEPT_LEVEL = ''
 ) ;
";
                    //查詢
                    var resultHRIS_BUDEPTs = conn.Query<HRIS_BUDEPT>(strSql,
                        new {
                            STATUS = "Y",
                            BU = "BULTW",
                            CHANGEMARK = "D",
                            TW_DEPT_LEVEL = "00"
                        }).ToList();

                    DSDs.AddRange(resultHRIS_BUDEPTs.Select(x => new DF_SCH_DEP()
                    {
                        SYS_TYPE = "A",
                        PAR_DEP = x.TW_DEPT_LEVEL == "01" //董事長
                                  ? "0" : x.UP_BU_DEPT_PRER_CD?.Trim(),
                        DEP_ID = x.TW_BU_DEPT_PRER_CD?.Trim(),
                        DEP_NAME = x.DEPT_NAME?.Trim(),
                        DEP_LEVEL = x.TW_DEPT_LEVEL?.Trim()
                    }));

                    strSql = @"
select 
BUCODE,
NATIONAL_ID,
NAME_FORMAL,
EMAIL_ADDR,
BUJOBDESCR,
TW_PRER_NUM_BU,
HR_STATUS,
BUORGMANAGER
from LFlow_HRIS_EMP
where STATUS = @STATUS 
and CHANGEMARK <> @CHANGEMARK --指查詢非刪除註記 ;
";
                    //查詢
                    var resultLFlow_HRIS_EMPs = conn.Query<LFlow_HRIS_EMP>(strSql,
                        new
                        {
                            STATUS = "Y",
                            CHANGEMARK = "D"
                        }).ToList();

                    //主管
                    DSMs.AddRange(resultHRIS_BUDEPTs.GroupBy(x=>x.MGR_EMPLID).Aggregate(new List<DF_SCH_MEM>(), (x,y) =>
                    {
                        var _LFlow_HRIS_EMP = resultLFlow_HRIS_EMPs.FirstOrDefault(z => z.TW_PRER_NUM_BU == y.Key);
                        var _V_EMP = V_EMPs.FirstOrDefault(j => j.EMP_NO == y.Key)?.USR_ID?.Trim();
                        bool MJOBFlag = true;
                        foreach (var item in y.OrderBy(j => j.DEPT_LVLNAME.OrderByLVLName()))
                        {
                            x.Add(new DF_SCH_MEM()
                            {
                                SYS_TYPE = "A",
                                DEP_ID = item.TW_BU_DEPT_PRER_CD?.Trim(),
                                MEM_ID = _LFlow_HRIS_EMP?.NATIONAL_ID?.Trim(),
                                MEM_NAME = item.MGR_NAME?.Trim(),
                                MEM_EMAIL = _LFlow_HRIS_EMP?.EMAIL_ADDR?.Trim(),
                                MEM_JOBTITLE = item.DEPT_LVLNAME.FormateLVLName(),
                                IS_MANA = "Y",
                                IS_MJOB = MJOBFlag ? "Y" : "N",
                                MEM_MEMO1 = _V_EMP,
                                MEM_MEMO2 = JsonConvert.SerializeObject(new { PNM = y.Key }),
                                ID_MEMO = "內勤",
                                AGENT_MEMO = "內勤"
                            });
                            MJOBFlag = false;
                        }
                        return x;
                    }));

                    //非主管
                    DSMs.AddRange(resultLFlow_HRIS_EMPs
                        .Where(x =>
                        x.HR_STATUS == "A" && //在職
                        x.BUORGMANAGER == "N" //非主管
                    ).Select(x => new DF_SCH_MEM()
                    {
                        SYS_TYPE = "A",
                        DEP_ID = x.BUCODE?.Trim(),
                        MEM_ID = x.NATIONAL_ID?.Trim(),
                        MEM_NAME = x.NAME_FORMAL?.Trim(),
                        MEM_EMAIL = x.EMAIL_ADDR?.Trim(),
                        MEM_JOBTITLE = x.BUJOBDESCR?.Trim(),
                        IS_MANA = "N",
                        IS_MJOB = "Y",
                        MEM_MEMO1 = V_EMPs.FirstOrDefault(y=>y.EMP_NO == x.TW_PRER_NUM_BU)?.USR_ID?.Trim(),
                        MEM_MEMO2 = JsonConvert.SerializeObject(new { PNM = x.TW_PRER_NUM_BU }),
                        ID_MEMO = "內勤",
                        AGENT_MEMO = "內勤"
                    }));
                }
                #endregion

                #region 寫入檔案資料至DB
                using (SqlConnection conn = new SqlConnection(conDFDBUT))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            bool tranCheck = true;
                            int DSD_del = DeleteDatas(new DF_SCH_DEP(), conn, tran);
                            int DSM_del = DeleteDatas(new DF_SCH_MEM(), conn, tran);
                            var DSD_Insert = InsertDatas(DSDs, conn, tran);
                            var DSM_Insert = InsertDatas(DSMs, conn, tran);
                            var _DF_SCH_MEM = UpdateDF_SCH_MEM(conn, tran).Item2;
                            if (DSD_Insert.Item1 == 0 || DSM_Insert.Item1 == 0 || !_DF_SCH_MEM.IsNullOrWhiteSpace())
                                tranCheck = false;
                            if (tranCheck)
                            {
                                tran.Commit();
                                UpdateDF_SCH_AP_ROLE(dtn, DSMs.Cast<DF_SCH_MEM>().ToList());
                            }
                            else
                                tran.Rollback();
                            if (tranCheck)
                            {
                                sb.AppendLine("執行成功!");
                                $@"執行成功!".setMessage();
                            }
                            else
                            {
                                sb.AppendLine("執行失敗!");
                                sb.AppendLine(DSD_Insert.Item2);
                                sb.AppendLine(DSM_Insert.Item2);
                                $@"執行失敗!".setMessage();
                                DSD_Insert.Item2.setMessage();
                                DSM_Insert.Item2.setMessage();
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            sb.Append(ex);
                            $@"執行失敗:{ex}!".setMessage();
                        }
                    }
                }
                #endregion

                #region 加入約聘人員
                sb.AppendLine("開始執行加入約聘人員");
                $@"開始執行加入約聘人員".setMessage();
                List<IModel> DSM_2s = new List<IModel>();
                using (SqlConnection conn = new SqlConnection(conEFPDB))
                {
                    conn.Open();
                    strSql = $@"
SELECT [SYS_TYPE]
      ,[IS_MANA]
      ,[IS_MJOB]
      ,[ID_MEMO]
      ,[AGENT_MEMO]
      ,[DEP_ID]
      ,[MEM_ID]
      ,[MEM_NAME]
      ,[MEM_EMAIL]
      ,[MEM_JOBTITLE]
      ,[MEM_MEMO1]
      ,[MEM_MEMO2]
      ,[UPDATE_DATE]
      ,[CREATE_UID]
      ,[UPDATE_UID]
  FROM [EFPDB].[dbo].[DF_CE_MEM] ; 
";
                    //查詢
                    var resultDF_CE_MEMs = conn.Query<DF_CE_MEM>(strSql).ToList();
                    sb.AppendLine($@"查詢到約聘人員有:{resultDF_CE_MEMs.Count}筆!");
                    $@"查詢到約聘人員有:{resultDF_CE_MEMs.Count}筆!".setMessage();
                    if (resultDF_CE_MEMs.Any())
                    {
                        DSM_2s = new List<IModel>();
                        DSM_2s.AddRange(resultDF_CE_MEMs.Select(x => new DF_SCH_MEM()
                        {
                            SYS_TYPE = x.SYS_TYPE?.Trim(),
                            DEP_ID = x.DEP_ID?.Trim(),
                            MEM_ID = x.MEM_ID?.Trim(),
                            MEM_NAME = x.MEM_NAME?.Trim(),
                            MEM_EMAIL = x.MEM_EMAIL?.Trim(),
                            MEM_JOBTITLE = x.MEM_JOBTITLE?.Trim().FormateLVLName(),
                            IS_MANA = x.IS_MANA?.Trim(),
                            IS_MJOB = x.IS_MJOB?.Trim(),
                            MEM_MEMO1 = x.MEM_MEMO1?.Trim(),
                            MEM_MEMO2 = JsonConvert.SerializeObject(new { PNM = x.MEM_MEMO2 }),
                            ID_MEMO = x.ID_MEMO?.Trim(),
                            AGENT_MEMO =x.AGENT_MEMO?.Trim()
                        }));                      
                    }              
                }
                if (DSM_2s.Any())
                {
                    using (SqlConnection conn = new SqlConnection(conDFDBUT))
                    {
                        conn.Open();
                        var DSM_Insert = InsertDatas(DSM_2s, conn);
                        if (DSM_Insert.Item1 != 0)
                        {
                            sb.AppendLine("加入約聘人員執行成功!");
                            $@"加入約聘人員執行成功!".setMessage();
                        }
                        else
                        {
                            sb.AppendLine("加入約聘人員執行失敗!");
                            sb.AppendLine(DSM_Insert.Item2);
                            $@"加入約聘人員執行失敗!".setMessage();
                            DSM_Insert.Item2.setMessage();
                        }
                    }
                }
                sb.AppendLine("執行加入約聘人員結束!");
                $@"執行加入約聘人員結束!".setMessage();
                #endregion
            }
            catch (Exception ex)
            {
                sb.Append(ex);
                WriteLog(ex.ToString());
                $@"執行失敗:{ex}!".setMessage();
            }
            finally
            {
                if(sb.Length > 0)
                    sendMail(sb.ToString());
                "DFHRIS END!".setMessage();
            }
        }

        protected static void sendMail(string body)
        {
            var sms = new SendMail.SendMailSelf();
            sms.smtpPort = 25;
            sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
            sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
            sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
            string sendMailstr = Properties.Settings.Default["mailSend"]?.ToString();
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
                new Tuple<string, string>(sms.mailAccount, "DFHRIS"),
                sendMails,
                null,
                "DFHRIS訊息",
                body
                );
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

        private static Tuple<int,string> InsertDatas(List<IModel> datas, SqlConnection conn, SqlTransaction tran = null)
        {
            if (!datas.Any())
                return new Tuple<int, string>(0,"無資料");
            bool _errorflag = false;
            Type p = datas.First().GetType();
            string typeName = p.Name;
            List<string> names = new List<string>() { "DEP_GUID", "MEM_GUID" };
            var pros = p.GetProperties().Where(x=> !names.Contains(x.Name)).ToList();
            $@"欲新增資料:{typeName},筆數:{datas.Count}筆.".setMessage();
            var _pros = p.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            StringBuilder _sb = new StringBuilder();
            datas.ForEach(item =>
            {
                var context = new ValidationContext(item, null, null);
                var val = new List<ValidationResult>();
                if (!Validator.TryValidateObject(item, context, val, true))
                {
                    _errorflag = true;
                    List<string> errors = new List<string>();
                    val.ForEach(x =>
                    {
                        var m = x.MemberNames.FirstOrDefault()?.ToString();
                        var _p = _pros.FirstOrDefault(y => y.Name.ToUpper() == m?.ToUpper());
                        var _val = (_p == null) ? null : (_p.GetValue(item))?.ToString();
                        if (!_val.IsNullOrWhiteSpace())
                            errors.Add((m + " : " + _val) + " Error : " + x.ErrorMessage);
                        else
                            errors.Add(x.ErrorMessage);
                    });
                    var _msg = $@"檢核失敗資料:{item.modelToString()}, 檢核失敗原因:{string.Join(",", errors)}";
                    _sb.AppendLine(_msg);
                    _msg.setMessage();
                }
            });
            if (_errorflag)
                return new Tuple<int, string>(0, _sb.ToString());
            var str = string.Empty;
            str += $@"Insert into {typeName} ( {string.Join(",", pros.Select(x => "[" + x.Name + "]"))} ) VALUES ({string.Join(",", pros.Select(x => "@" + x.Name))} ) ;";
            var _result = conn.Execute(str, datas, tran);
            return new Tuple<int, string>(_result,string.Empty);
        }

        private static int DeleteDatas(IModel model, SqlConnection conn, SqlTransaction tran)
        {
            int result = 0;
            Type p = model.GetType();
            string typeName = p.Name;
            var str = string.Empty;
            str = $@"Delete {typeName}  where SYS_TYPE = @SYS_TYPE ; ";
            result = conn.Execute(str, new { SYS_TYPE = "A" }, tran);
            return result;
        }

        private static Tuple<int, string> UpdateDF_SCH_MEM(SqlConnection conn, SqlTransaction tran)
        {
            int _result = 0;

            string _DEP_LEVEL = Properties.Settings.Default["DEP_LEVEL"]?.ToString();
            string _PAR_DEP = Properties.Settings.Default["PAR_DEP"]?.ToString();

            string str = $@"
update DF_SCH_MEM
set  MEM_JOBTITLE = '人事行政'
where DEP_ID in (
select DEP_ID from DF_SCH_DEP where DEP_LEVEL = @DEP_LEVEL and PAR_DEP in
(select DEP_ID from DF_SCH_DEP where PAR_DEP = @PAR_DEP))
and MEM_JOBTITLE = '行政'
";
            try
            {
                _result = conn.Execute(str, new { DEP_LEVEL = _DEP_LEVEL, PAR_DEP = _PAR_DEP }, tran);
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(_result, ex.ToString());
            }
            return new Tuple<int, string>(_result, string.Empty);
        }

        private static void UpdateDF_SCH_AP_ROLE(DateTime dtn, List<DF_SCH_MEM> DSMs)
        {
            string message = string.Empty;
            List<IModel> DSARs = new List<IModel>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["intraware"].ConnectionString))
            {
                conn.Open();
                string sql = $@"
                select budget_dept,budget_admin,admin_name,admin_mail from vw_tempACC
";
                foreach (var item in conn.Query<vw_tempACC>(sql))
                {
                    DSARs.Add(new DF_SCH_AP_ROLE()
                    {
                        AP_TYPE = "EFP",
                        MEM_ID = DSMs.FirstOrDefault(z=>z.MEM_MEMO2.IndexOf(item.budget_admin) > -1)?.MEM_ID,
                        DEP_ID = item.budget_dept,
                        ROLE_ID = "A",
                        CREATE_USER = "DFHRIS",
                        CREATE_DATE = dtn
                    });
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DFDBUT"].ConnectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {    
                    string sql = $@"
                    delete from DF_SCH_AP_ROLE where AP_TYPE = 'EFP' ; ";                  
                    try
                    {
                        if (DSARs.Any())
                        {
                            var _deleCount =  conn.Execute(sql,null, tran);
                            var _DSARs = InsertDatas(DSARs, conn, tran);
                            if (_DSARs.Item1 == DSARs.Count)
                            {
                                tran.Commit();
                                //刪除成功筆數
                                //新增成功筆數
                                $@"預算委員刪除資料數:{_deleCount}".setMessage();
                                $@"預算委員新增成功筆數:{_DSARs.Item1}".setMessage();
                                $@"預算委員更新成功".setMessage();
                            }
                            else
                            {
                                tran.Rollback();
                                //新增資料筆數有誤
                                $@"預算委員,新增資料筆數有誤! 已復原!".setMessage();
                            }
                        }
                        else
                        {
                            //沒有新增資料
                             $@"vw_tempACC 無資料! 跳過更新預算委員".setMessage();
                        }                 
                    }
                    catch (Exception ex)
                    {
                        //失敗 錯誤訊息
                        $@"更新預算委員失敗:{ex.ToString()}".setMessage();
                    }
                }
            }
        }
    }

    public static class Extension
    {
        public static void setMessage(this string message)
        {
            Console.WriteLine(message);
            Program.WriteLog(message);
        }
        public static string objToString(object value)
        {
            if (value != null)
                return value.ToString();
            return string.Empty;
        }

        public static string modelToString<T>(this T model, string log = null)
        {
            var result = string.Empty;
            if (model != null)
            {
                if (!log.IsNullOrWhiteSpace())
                    result += "|";
                StringBuilder sb = new StringBuilder();
                var Type = model.GetType();
                sb.Append($@"TableName:{Type.Name}|");
                var Pros = Type.GetProperties();
                Pros.ToList().ForEach(x =>
                {
                    sb.Append($@"{x.Name}:{objToString(x.GetValue(model))}|");
                });
                if (sb.Length > 0)
                {
                    result = sb.ToString();
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string GetDescription<T>(this T enumerationValue, string title = null, string body = null)
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
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(body))
                        return string.Format("{0} : {1} => {2}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    if (!string.IsNullOrWhiteSpace(title))
                        return string.Format("{0} : {1}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description
                            );
                    if (!string.IsNullOrWhiteSpace(body))
                        return string.Format("{0} => {1}",
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        public static string FormateLVLName(this string val)
        {
            if (val.IsNullOrWhiteSpace())
                return val?.Trim();
            val = val?.Trim();
            switch (val)
            {
                case "直屬科":
                    val = "科長";
                    break;
                case "處":
                case "部":
                case "科":
                    val += "長";
                    break;               
            }
            return val;
        }

        public static int OrderByLVLName(this string val)
        {
            int result = 9;
            switch (val?.Trim())
            {
                case "董事長":
                    result = 0;
                    break;
                case "副董事長":
                    result = 1;
                    break;
                case "總經理":
                    result = 2;
                    break;
                case "處":
                    result = 3;
                    break;
                case "處副":
                    result = 4;
                    break;
                case "部":
                    result = 5;
                    break;
                case "部副":
                    result = 6;
                    break;
                case "直屬科":
                    result = 7;
                    break;
                case "科":
                    result = 8;
                    break;
                default:
                    result = 9;
                    break;
            }
            return result;
        }
    }
}
