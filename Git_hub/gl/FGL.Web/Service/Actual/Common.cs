using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FGL.Web.Utilitys;
using FGL.Web.Models;
using System.Data.EasycomClient;
using FGL.Web.BO;
using System.Data.Common;
using FGL.Web.Controllers;
using FGL.Web.ActionFilter;

namespace FGL.Web.Service.Actual
{
    public class Common
    {


        public string GetUser()
        {
            return AccountController.CurrentUserId;
        }

        /// <summary>
        ///  get SysCode by CodeType
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="codeType"></param>
        /// <param name="isAll">起始資料新增 All</param>
        /// <param name="sdata">新增起始資料</param>
        /// <param name="edate">新增結束資料</param>
        /// <returns></returns>
        public List<SelectOption> GetSysCode(string sysCd,
            string codeType, 
            bool isAll = false, 
            IEnumerable<SelectOption> sdata = null,
            IEnumerable<SelectOption> edate = null)
        {
            var result = new List<SelectOption>();
            if (sysCd.IsNullOrWhiteSpace() || codeType.IsNullOrWhiteSpace())
                return result;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                if (isAll)
                    result.Add(new SelectOption() { Text = "All", Value = "All" });
                if (sdata != null)
                    result.AddRange(sdata);
                result.AddRange(db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == sysCd)
                    .Where(x => x.CODE_TYPE == codeType)
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList());
                if (edate != null)
                    result.AddRange(edate);
            }
            return result;
        }

        /// <summary>
        /// tuple to selectoption
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="showValue"></param>
        /// <returns></returns>
        public List<SelectOption> tupleToSelectOption(List<Tuple<string, string>> datas,bool showValue = false)
        {
            var result = new List<SelectOption>();
            if (datas.Any())
                result.AddRange(datas.Select(x => new SelectOption()
                {
                    Value = x.Item1,
                    Text = (showValue ? (x.Item1 + " : ") : string.Empty) + x.Item2
                }));
            return result;
        }


        /// <summary>
        /// 獲取 員工資料
        /// </summary>
        /// <returns></returns>
        //public List<DF_SCH_MEM> GetEmps()
        //{
        //    var emps = new List<DF_SCH_MEM>();
        //    using (DFDBUTEntities dbDFDBUTE = new DFDBUTEntities())
        //    {
        //        emps = dbDFDBUTE.DF_SCH_MEM.AsNoTracking().Where(x => x.MEM_MEMO1 != null && x.MEM_MEMO1.Length == 5).ToList();
        //    }

        //    return emps;
        //}

        /// <summary>
        /// 獲取 員工名子
        /// </summary>
        /// <param name="userIds">5碼AD</param>
        /// <returns></returns>
        //public Dictionary<string, string> GetEmpNames(List<string> userIds)
        //{
        //    Dictionary<string, string> result = new Dictionary<string, string>();
        //    var emps = GetEmps();
        //    foreach (var item in emps.Where(x => userIds.Contains(x.MEM_MEMO1)))
        //    {
        //        result.Add(item.MEM_MEMO1, item.MEM_NAME);
        //    }
        //    return result;
        //}

        /// <summary>
        /// 獲取 部門科別資料
        /// </summary>
        /// <returns></returns>
        //public List<DF_SCH_DEP> GetDepts()
        //{
        //    var depts = new List<DF_SCH_DEP>();
        //    using (DFDBUTEntities dbDFDBUTE = new DFDBUTEntities())
        //    {
        //        depts = dbDFDBUTE.DF_SCH_DEP.AsNoTracking().ToList();
        //    }

        //    return depts;
        //}

        /// <summary>
        /// 查詢結果 欄位1(5碼ID or 身份證號碼) 欄位2(中文名子) 欄位3(部門ID) 欄位4(部門中文) 欄位5(Email)
        /// </summary>
        /// <param name="userIds">5ID or 身份證字號</param>
        /// <param name="flag">true => 要多串部門名稱</param>
        /// <returns>欄位1(5碼ID or 身份證號碼) 欄位2(中文名子) 欄位3(部門ID) 欄位4(部門中文) 欄位5(Email)</returns>
        //public List<Tuple<string, string, string, string, string>> GetMemoByUserId(IEnumerable<string> userIds,bool flag = false)
        //{
        //    var depts = new List<DF_SCH_DEP>();
        //    var mems = new List<DF_SCH_MEM>();
        //    var emps = GetEmps(); //DF_SCH_MEM
        //    var deps = GetDepts(); //DF_SCH_DEP
        //    List<Tuple<string, string, string, string, string>> results = 
        //        new List<Tuple<string, string, string, string, string>>();
        //    foreach (var item in userIds)
        //    {
        //        var str1 = item?.Trim();
        //        var str2 = string.Empty;
        //        var str3 = string.Empty;
        //        var str4 = string.Empty;
        //        var str5 = string.Empty;
        //        if (!str1.IsNullOrWhiteSpace())
        //        {
        //            DF_SCH_MEM mem = null;
        //            if (str1.Length == 5)
        //                mem = emps.FirstOrDefault(x => x.MEM_MEMO1 == str1);
        //            else
        //                mem = emps.FirstOrDefault(x => x.MEM_ID == str1);
        //            //if (mem != null)
        //            //{
        //            //    str2 = mem.MEM_NAME;
        //            //    str5 = mem.MEM_EMAIL;
        //            //    str3 = mem.DEP_ID;
        //            //    var dep = deps.FirstOrDefault(x => x.DEP_ID == mem.DEP_ID);
        //            //    if (dep != null)
        //            //    {
        //            //        if (flag)
        //            //        {
        //            //            var _dep_level = 0;
        //            //            Int32.TryParse(dep.DEP_LEVEL, out _dep_level);
        //            //            if (_dep_level > 11)
        //            //            {
        //            //                var _DSD_P = deps.FirstOrDefault(x => x.DEP_ID == dep.PAR_DEP);
        //            //                var _DSD_P_D = string.Empty;
        //            //                if (_DSD_P != null && _DSD_P.DEP_NAME.IndexOf("(") > -1)
        //            //                {
        //            //                    _DSD_P_D = _DSD_P.DEP_NAME.Substring(0, _DSD_P.DEP_NAME.IndexOf("("))?.Trim();
        //            //                }
        //            //                str4 = $@"{_DSD_P_D} {dep?.DEP_NAME}";
        //            //            }
        //            //            else
        //            //                str4 = $@"{dep?.DEP_NAME}";
        //            //        }
        //            //        else
        //            //        {
        //            //            str4 = dep.DEP_NAME;
        //            //        }
        //            //    }                   
        //            //}
        //            if (mem != null)
        //            {
        //                str2 = mem.MEM_NAME;
        //                str5 = mem.MEM_EMAIL;
        //                str3 = mem.DEP_ID;

        //                var dep = deps.FirstOrDefault(x => x.DEP_ID == mem.DEP_ID);
        //                if (dep != null)
        //                {
        //                    if (flag)
        //                    {
        //                        var _dep_level = 0;
        //                        Int32.TryParse(dep.DEP_LEVEL, out _dep_level);
        //                        if (_dep_level > 11)
        //                        {
        //                            var _DSD_P = deps.FirstOrDefault(x => x.DEP_ID == dep.PAR_DEP);
        //                            var _DSD_P_D = string.Empty;
        //                            if (_DSD_P != null && _DSD_P.DEP_NAME.IndexOf("(") > -1)
        //                            {
        //                                _DSD_P_D = _DSD_P.DEP_NAME.Substring(0, _DSD_P.DEP_NAME.IndexOf("("))?.Trim();
        //                            }
        //                            str4 = $@"{_DSD_P_D} {dep?.DEP_NAME}";
        //                        }
        //                        else
        //                            str4 = $@"{dep?.DEP_NAME}";
        //                    }
        //                    else
        //                    {
        //                        str4 = dep.DEP_NAME;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                V_EMPLY _emp = null;
        //                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
        //                {
        //                    if (str1.Length == 5)
        //                    {
        //                        _emp = dbIntra.V_EMPLY.FirstOrDefault(x => x.USR_ID == str1);
        //                    }
        //                    else
        //                    {
        //                        _emp = dbIntra.V_EMPLY.FirstOrDefault(x => x.ID_NO == str1);
        //                    }
        //                    if(_emp != null)
        //                    {
        //                        str2 = _emp.EMP_NAME?.Trim();
        //                        str5 = _emp.EMAIL?.Trim();
        //                        str3 = _emp.DPT_CD?.Trim();
        //                        str4 = _emp.DPT_NAME?.Trim();
        //                    }   
        //                }
        //            }
        //        }
        //        results.Add(new Tuple<string, string, string, string, string>(str1, str2, str3, str4, str5));
        //    }
        //    return results;
        //}

        /// <summary>
        /// 抓400設定訊息
        /// </summary>
        /// <param name="GROUP_ID"></param>
        /// <param name="REF_NO"></param>
        /// <returns></returns>
        public string GetMessage(string GROUP_ID, string REF_NO)
        {
            string result = string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select TEXT from LRTCODE1
where REF_NO = :REF_NO and GROUP_ID = :GROUP_ID
";
                    com.Parameters.Add("REF_NO", REF_NO);
                    com.Parameters.Add("GROUP_ID", GROUP_ID);
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        result = dbresult["TEXT"]?.ToString()?.Trim();
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }
            return result;
        }

        /// <summary>
        /// 傳入單位名稱 抓取至部門
        /// </summary>
        /// <param name="deps"></param>
        /// <returns></returns>
        //public List<Tuple<string, string>> getFullDepName(IEnumerable<string> deps)
        //{
        //    var _deps = GetDepts();
        //    List<Tuple<string, string>> result = new List<Tuple<string, string>>();
        //    foreach (var item in deps)
        //    {
        //        var _DSD = _deps.FirstOrDefault(x => x.DEP_ID == item);
        //        var D = string.Empty;
        //        if (_DSD != null)
        //        {
        //            var _dep_level = 0;
        //            Int32.TryParse(_DSD.DEP_LEVEL, out _dep_level);
        //            if (_dep_level > 11)
        //            {
        //                var _DSD_P = _deps.FirstOrDefault(x => x.DEP_ID == _DSD.PAR_DEP);
        //                var _DSD_P_D = string.Empty;
        //                if (_DSD_P != null && _DSD_P.DEP_NAME.IndexOf("(") > -1)
        //                {
        //                    _DSD_P_D = _DSD_P.DEP_NAME.Substring(0, _DSD_P.DEP_NAME.IndexOf("("))?.Trim();
        //                }

        //                D = $@"{_DSD_P_D} {_DSD?.DEP_NAME}";
        //            }
        //            else
        //                D = $@"{_DSD?.DEP_NAME}";
        //        }
        //        else
        //            D = item;
        //        result.Add(new Tuple<string, string>(item, D));
        //    }
        //    return result;
        //}

        /// <summary>
        /// 組合 支票，票號後3碼，每張支票用「、」區分，最長放12張支票加11個、符號
        /// </summary>
        /// <param name="values">支票號碼</param>
        /// <returns></returns>
        //internal string GetCheckmemo(IEnumerable<string> values)
        //{
        //    //對應到同「地址、收件人」的支票，票號後3碼，每張支票用「、」區分，
        //    //例如：票號KG9876543、KG9876542、KG9976012對應到同一「地址、收件人」
        //    //543、542、012 (長度50，最長放12張支票加11個、符號)
        //    return string.Join("、", values.Where(x => !x.IsNullOrWhiteSpace() && x.Length >= 3)
        //        .Take(12)
        //        .Select(x => x.Substring(x.Length - 3, 3)));
        //}


        /// <summary>
        /// GET codeValue
        /// </summary>
        /// <param name="sys_codes">資料</param>
        /// <param name="code_type">code_type</param>
        /// <param name="code">code</param>
        /// <returns></returns>
        internal string getCodeValue(List<SYS_CODE> sys_codes, string code_type, string code)
        {
            return sys_codes.FirstOrDefault(x => x.CODE_TYPE == code_type && x.CODE == code)?.CODE_VALUE ?? string.Empty;
        }
    }
}