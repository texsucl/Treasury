using System.Collections.Generic;
using System.Data;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Report.Interface;
using Treasury.WebUtility;
using System.ComponentModel;
using System;
using Treasury.Web.Service.Actual;

namespace Treasury.Web.Report.Data
{
    public abstract class ReportData : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public ReportData()
        {
           
            extensionParms = new List<reportParm>();
            defaultConnection = System.Configuration.ConfigurationManager.
                         ConnectionStrings["dbTreasury"].ConnectionString;
        }
        public abstract DataSet GetData(List<reportParm> parms);

        public List<reportParm> extensionParms { get; set; }

        protected REC _REC { get; private set; }

        protected void SetDetail(string aply_No)
        {
            var depts = new List<VW_OA_DEPT>();
            var emps = new List<V_EMPLY2>();
            _REC = new REC();
            //var sys
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x=>x.DPT_CD != null).ToList();
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var sys = db.SYS_CODE.AsNoTracking().ToList();
                var data = db.TREA_APLY_REC.AsNoTracking().FirstOrDefault(x => x.APLY_NO == aply_No);
                var _dept = new INTRA().getDept(data.APLY_UNIT);
                if (_dept != null)
                {
                    if (_dept.Dpt_type != null)
                    {
                        switch (_dept.Dpt_type.Trim())
                        {
                            case "04": //科
                                _REC.APLY_DEPT = _dept.UP_DPT_CD?.Trim();
                                _REC.APLY_SECT = _dept.DPT_CD?.Trim();
                                break;
                            case "03": //部
                                _REC.APLY_DEPT = _dept.DPT_CD?.Trim();
                                break;
                        }
                    }
                }
                _REC.APLY_NO = data.APLY_NO; //申請單號
                _REC.ACCESS_TYPE = data.ACCESS_TYPE == "P" ? "存入" : data.ACCESS_TYPE == "G" ? "取出" : ""; //動作 存入/取出
                _REC.APLY_DT = data.CREATE_DT?.ToSimpleTaiwanDate(); //申請日期
                _REC.ITEM_ID = sys.FirstOrDefault(x => x.CODE_TYPE == "TREA_ITEM_TYPE" && x.CODE == data.ITEM_ID)?.CODE_VALUE; //作業項目
                _REC.APLY_UNIT = getEmpName(depts, data.APLY_UNIT); //權責部門
                _REC.ACCESS_REASON = data.ACCESS_REASON; //申請原因
                _REC.EXPECTED_ACCESS_DATE = data.EXPECTED_ACCESS_DATE?.ToSimpleTaiwanDate(); //預計存取日期
                //申請單位
                var APLY_APPR = getDeptName(emps, data.APLY_APPR_UID); //覆核人員資料
                _REC.APLY_APPR_UID_UNIT = APLY_APPR.Item1; //覆核人員單位
                _REC.APLY_APPR_UID = APLY_APPR.Item2; //覆核人員名稱
                var APLY = getDeptName(emps, data.APLY_UID); //申請人員資料
                _REC.APLY_UID_UNIT = APLY.Item1; //申請人員單位
                _REC.APLY_UID = APLY.Item2; //申請人員名稱
                //保管單位
                var CUSTODY_APPR = getDeptName(emps, data.CUSTODY_APPR_UID); //覆核人員資料
                _REC.CUSTODY_APPR_UID_UNIT = CUSTODY_APPR.Item1; //覆核人員單位
                _REC.CUSTODY_APPR_UID = CUSTODY_APPR.Item2; //覆核人員名稱
                var CUSTODY = getDeptName(emps, data.CUSTODY_UID); //承辦人員資料
                _REC.CUSTODY_UID_UNIT = CUSTODY.Item1; //承辦人員單位
                _REC.CUSTODY_UID = CUSTODY.Item2; //承辦人員名稱
            }
        }

        protected void SetExtensionParm()
        {
            foreach (var item in _REC.GetType().GetProperties()
                .Where(x => x.Name != "APLY_DEPT")
                .Where(x => x.Name != "APLY_SECT"))
            {
                extensionParms.Add(new reportParm()
                {
                    key = item.Name,
                    value = item.GetValue(_REC)?.ToString(),
                });
            }
        }

        protected class REC {

            [Description("申請單號")]
            public string APLY_NO { get; set; }

            [Description("存取項目交易別 存入(P) or 取出(G)")]
            public string ACCESS_TYPE { get; set; }

            [Description("申請日期")]
            public string APLY_DT { get; set; }

            [Description("作業項目")]
            public string ITEM_ID { get; set; }

            [Description("權責部門")]
            public string APLY_UNIT { get; set; }

            [Description("入庫原因")]
            public string ACCESS_REASON { get; set; }

            [Description("預計存取日期")]
            public string EXPECTED_ACCESS_DATE { get; set; }

            [Description("(申請單位)覆核人員單位")]
            public string APLY_APPR_UID_UNIT { get; set; }

            [Description("(申請單位)覆核人員名稱")]
            public string APLY_APPR_UID { get; set; }

            [Description("(申請單位)申請人員單位")]
            public string APLY_UID_UNIT { get; set; }

            [Description("(申請單位)申請人員名稱")]
            public string APLY_UID { get; set; }

            [Description("(保管單位)覆核人員單位")]
            public string CUSTODY_APPR_UID_UNIT { get; set; }

            [Description("(保管單位)覆核人員名稱")]
            public string CUSTODY_APPR_UID { get; set; }

            [Description("(保管單位)承辦人員單位")]
            public string CUSTODY_UID_UNIT { get; set; }

            [Description("(保管單位)承辦人員名稱")]
            public string CUSTODY_UID { get; set; }

            [Description("(庫存條件)申請部門")]
            public string APLY_DEPT { get; set; }

            [Description("(庫存條件)申請科別")]
            public string APLY_SECT { get; set; }
        }

        /// <summary>
        /// 使用 部門ID 獲得 部門名稱
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        private string getEmpName(List<VW_OA_DEPT> depts , string DPT_CD)
        {
            if (!DPT_CD.IsNullOrWhiteSpace() && depts.Any())
                return depts.FirstOrDefault(x => x.DPT_CD.Trim() == DPT_CD.Trim())?.DPT_NAME?.Trim();
            return string.Empty;
        }

        /// <summary>
        /// 使用 userId 獲得 1.部門名稱 2.使用者名稱
        /// </summary>
        /// <param name="emps"></param>
        /// <param name="USR_ID"></param>
        /// <returns></returns>
        private Tuple<string,string> getDeptName(List<V_EMPLY2> emps, string USR_ID)
        {
            if (!USR_ID.IsNullOrWhiteSpace())
            {
                var emp = emps.FirstOrDefault(x => x.USR_ID == USR_ID);
                if (emp != null)
                {
                    return new Tuple<string, string>(emp.DPT_NAME?.Trim(),emp.EMP_NAME?.Trim());
                }
            }
            return new Tuple<string, string>(string.Empty,string.Empty);

        }
    }
}