using Dapper;
using FTPHRIS.Interface;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FTPHRIS
{
    class Program
    {
        static void Main(string[] args)
        {
            string DefaultConn = string.Empty;

            Status _status = Status.S1;
            StringBuilder sb = new StringBuilder();
            string _s = string.Empty;
            string deleteDir = string.Empty;
            try
            {
                DateTime dtn = DateTime.Now;
                #region FTP抓取資料
                string dtnStr = dtn.ToString("yyyyMMdd");
                string startupPath = System.Windows.Forms.Application.StartupPath;
                string batDir = Path.Combine(startupPath, ConfigurationManager.AppSettings["batDir"]);
                string fileDir = Path.Combine(startupPath, ConfigurationManager.AppSettings["fileDir"]);
                string cdfileDir = Path.Combine(startupPath, ConfigurationManager.AppSettings["cdfileDir"]);
                string ftpExe = ConfigurationManager.AppSettings["ftpExe"];
                createDir(batDir);
                createDir(fileDir);
                string batDirDay = Path.Combine(batDir, dtnStr);
                string fileDirDay = Path.Combine(fileDir, dtnStr);
                createDir(fileDirDay);
                deleteDir = fileDirDay;
                WriteLog("開始建立抓取 FTP bat & txt");
                string batfileName = $@"bat-{dtnStr}_FTPHRIS.bat";
                string txtfileName = $@"ftp-{dtnStr}_FTPHRIS.txt";
                createFile(batDirDay, batfileName,
                    new List<string>()
                    {
                        $@"{ftpExe} -cpa:5 -cpw:5 -e -s:{Path.Combine(batDirDay, txtfileName)} open"
                    });
                createFile(batDirDay, txtfileName,
                    new List<string>()
                    {
                        $@"binary",
                        $@"prompt",
                        $@"cd {cdfileDir} ",
                        $@"lcd {fileDirDay} ",
                        $@"mget *.*",
                        $@"mdelete *.*",
                        $@"quit "
                    });
                WriteLog($@"執行批次，fileName={batfileName}, 執行目錄：{batDirDay}");
                Process p = new Process();
                p.StartInfo.FileName = batfileName;
                p.StartInfo.WorkingDirectory = batDirDay;
                p.Start();
                p.WaitForExit();
                p.Close();

                #endregion
                _status = Status.S2;
                #region 檔案資料讀取至DB
                List<string> fileNames = fileSearch(fileDirDay);
                var a = DirSearch(fileDir);
                List<string> pathTypes = new List<string>();  //檔案類型
                List<string> savefileNames = ConfigurationManager.AppSettings["fileNames"].Split(';').ToList(); //可匯入檔案
                List<string> checkfileNames = ConfigurationManager.AppSettings["checkfileNames"].Split(';').ToList(); //需檢核檔案
                int checkCount = 0;
                if (fileNames.Any())
                {
                    //如果有多天 抓取最後一天(一版)
                    string pathType = fileNames.Max(y => Path.GetExtension(y).Substring(1));
                    //依照副檔名 只抓最後一版
                    fileNames = fileNames.Where(x => pathType == (Path.GetExtension(x).Substring(1))).ToList();
                    if (checkfileNames.Count(x => fileNames.Any(y => y.IndexOf(x) > -1)) == checkfileNames.Count)
                    {
                        List<List<IModel>> insertModels = new List<List<IModel>>();
                        fileNames.ForEach(x =>
                        {
                            List<IModel> fileDatas = new List<IModel>();
                            var savefileName = savefileNames.FirstOrDefault(y => x.IndexOf(y) > -1);
                            if (savefileName != null)
                            {
                                var file = Path.Combine(fileDirDay, x);
                                WriteLog(file);
                                IModel type = GetInstance(savefileName);
                                var typePros = type.GetType().GetProperties();
                                bool firstFlag = true;
                                List<string> filePros = new List<string>(); 
                                using (StreamReader sr = new StreamReader(file))
                                {
                                    string line = string.Empty;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (firstFlag)
                                        {
                                            filePros.AddRange(line.Split(','));
                                            firstFlag = false;
                                        }
                                        else
                                        {
                                            var data = GetInstance(savefileName);
                                            var datas = line.Split(',');
                                            if (filePros.Count == datas.Count())
                                            {
                                                fileDatas.Add(filePros.Aggregate(data, (result, y) =>
                                                {
                                                    int i = 0;
                                                    foreach (var pro in typePros)
                                                    {
                                                        if (!y.IsNullOrWhiteSpace() && y.ToUpper() == pro.Name.ToUpper())
                                                        {
                                                            if (pro.PropertyType == typeof(Nullable<DateTime>))
                                                            {
                                                                Nullable<DateTime> dt = null;
                                                                DateTime d = DateTime.MinValue;
                                                                if (DateTime.TryParse(datas[i]?.Replace("\"", string.Empty), out d))
                                                                    dt = d;
                                                                pro.SetValue(data,dt);
                                                            }
                                                            else
                                                                pro.SetValue(data, datas[i]?.Replace("\"", string.Empty));
                                                        }
                                                        if (pro.Name.ToUpper() == "STATUS")
                                                            pro.SetValue(data, "Y");
                                                        if (pro.Name.ToUpper() == "CRT_DATETIME")
                                                            pro.SetValue(data, dtn);
                                                        i++;
                                                    }
                                                    return data;
                                                }));
                                            }
                                        }
                                        Console.WriteLine(line);
                                    }
                                }
                                insertModels.Add(fileDatas);
                            }
                        });
                        if (insertModels.Any())
                        {
                            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GLSIEXT"].ConnectionString))
                            {
                                conn.Open();
                                using (SqlTransaction tran = conn.BeginTransaction())
                                {
                                    bool tranCheck = true;
                                    foreach (var item in insertModels.Where(x => x.Any() && checkfileNames.Contains(x.First().GetType().Name)))
                                    {
                                        var t = item.First();
                                        var result_del = DeleteDatas(t, conn, tran);
                                        var result_upd = UpdateDatas(t, conn, tran);
                                        int result = InsertDatas(item, conn, tran);
                                        if (result == 0)
                                            tranCheck = false;
                                    }
                                    if(tranCheck)
                                        tran.Commit();
                                    else
                                        tran.Rollback();
                                }
                                foreach (var item in insertModels.Where(x => x.Any() && !checkfileNames.Contains(x.First().GetType().Name)))
                                {
                                    int result = 0;
                                    using (SqlTransaction tran = conn.BeginTransaction())
                                    {
                                        if (item.Any())
                                        {                                           
                                            var t = item.First();
                                            var result_del = DeleteDatas(t, conn, tran);
                                            var result_upd = UpdateDatas(t, conn, tran);
                                            result = InsertDatas(item, conn, tran);
                                        }
                                        if (result > 0)
                                            tran.Commit();
                                        else
                                            tran.Rollback();
                                    }                              
                                }
                            }
                        }
                        _status = Status.SS;
                    }
                    else
                    {
                        WriteLog($@"檔案夾內無所有必要檔案! {string.Join(",", checkfileNames)}.");
                    }                   
                }
                else
                {
                    WriteLog("檔案夾內無資料!");
                }
                WriteLog(_status.GetDescription());
                #endregion
            }
            catch (Exception ex)
            {
                WriteLog(_status.GetDescription());
                _s = ex.ToString();
                WriteLog(ex.ToString());
            }
            finally
            {
                sb.AppendLine(_status.GetDescription());
                if (!_s.IsNullOrWhiteSpace())
                    sb.AppendLine(_s);
                var deleteFile = ConfigurationManager.AppSettings["deleteFile"]?.ToString();
                if (deleteFile == "Y")
                    deleFileDir(deleteDir);       
                sendMail(sb.ToString());
                Console.WriteLine("FTPHRIS END!");
            }
            Console.ReadLine();
        }

        protected static void sendMail(string body)
        {
            try
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
                new Tuple<string, string>(sms.mailAccount, "FTRHRIS"),
                sendMails,
                null,
                "FTPHRIS訊息",
                body
                );
            }
            catch(Exception ex)
            {
                WriteLog(ex.ToString(), Nlog.Error);
            }
        }

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void WriteLog(string log,Nlog type = Nlog.Info)
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

        public enum Status
        {
            /// <summary>
            /// FTP抓取資料失敗
            /// </summary>
            [Description("FTP抓取資料失敗")]
            S1,
            /// <summary>
            /// 資料匯入DB失敗
            /// </summary>
            [Description("資料匯入DB失敗")]
            S2,
            /// <summary>
            /// FTPHRIS 執行成功
            /// </summary>
            [Description("FTPHRIS 執行成功")]
            SS
        }

        #region Create 資料夾

        /// <summary>
        /// Create 資料夾(判斷如果沒有的話就新增)
        /// </summary>
        /// <param name="projectFile">資料夾位置</param>
        public static void createDir(string projectFile)
        {
            bool exists = Directory.Exists(projectFile);
            if (!exists) Directory.CreateDirectory(projectFile);
        }

        #endregion Create 資料夾       

        public static void createFile(string filePath, string fileName, List<string> data)
        {
            if (data.Any() && !string.IsNullOrWhiteSpace(filePath) && !string.IsNullOrWhiteSpace(fileName))
            {
                createDir(filePath);
                using (StreamWriter sw = new StreamWriter
                    (Path.Combine(filePath, fileName), false)) //false 複寫 true 附加
                {
                    data.ForEach(x => sw.WriteLine(x));
                    sw.Close();
                }
            }
        }

        public static void deleFileDir(string sDir)
        {
            foreach (string f in Directory.GetFiles(sDir))
            {
                System.IO.File.Delete(f);
            }
        } 

        public static List<string> fileSearch(string sDir)
        {
            List<string> fileNames = new List<string>();
            foreach (string f in Directory.GetFiles(sDir))
            {
                fileNames.Add(Path.GetFileName(f));
            }
            return fileNames;
        }

        public static List<string> DirSearch(string sDir)
        {
            List<string> fileNames = new List<string>();
            foreach (string d in Directory.GetDirectories(sDir))
            {
                fileNames.AddRange(fileSearch(d));
                fileNames.AddRange(DirSearch(d));
            }
            return fileNames;
        }

        public static IModel GetInstance(string type)
        {
            return (IModel)Activator.CreateInstance(Assembly.Load("FTPHRIS").GetType($"FTPHRIS.model.{type}"));
        }

        private static int InsertDatas(List<IModel> datas, SqlConnection conn, SqlTransaction tran)
        {
            int result = 0;
            if (!datas.Any())
                return result;
            Type p = datas.First().GetType();
            string typeName = p.Name;
            var pros = p.GetProperties();
            Console.WriteLine($@"欲新增資料:{typeName},筆數:{datas.Count}筆.");
            WriteLog($@"欲新增資料:{typeName},筆數:{datas.Count}筆.");
            var str = string.Empty;
            str = $@"Insert into {typeName} ( {string.Join(",",pros.Select(x=>"[" + x.Name + "]"))} ) VALUES ({string.Join(",", pros.Select(x => "@" + x.Name))} ) ;";
            result = conn.Execute(str, datas, tran);
            return result;
        }

        private static int UpdateDatas(IModel model, SqlConnection conn, SqlTransaction tran)
        {
            int result = 0;
            Type p = model.GetType();
            string typeName = p.Name;
            var str = string.Empty;
            str = $@"Update {typeName} set STATUS = @STATUS_N where STATUS = @STATUS_Y ; ";
            result = conn.Execute(str, new { STATUS_Y = "Y" , STATUS_N  = "N"}, tran);
            return result;
        }

        private static int DeleteDatas(IModel model, SqlConnection conn, SqlTransaction tran)
        {
            int result = 0;
            Type p = model.GetType();
            string typeName = p.Name;
            var str = string.Empty;
            str = $@"Delete {typeName}  where STATUS = @STATUS_N ; ";
            result = conn.Execute(str, new { STATUS_N = "N" }, tran);
            return result;
        }
    }

    public static class Extension
    {
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
    }
}
