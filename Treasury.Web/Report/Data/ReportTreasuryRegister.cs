using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Report.Interface;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public abstract class ReportTreasuryRegister : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public List<reportParm> extensionParms { get; set; }

        public ReportTreasuryRegister()
        {
            extensionParms = new List<reportParm>();
            defaultConnection = System.Configuration.ConfigurationManager.
                         ConnectionStrings["dbTreasury"].ConnectionString;
        }

        public abstract DataSet GetData(List<reportParm> parms);

        protected REC _REC { get; private set; }

        protected void SetDetail(string vTreaRegisterId, string vUser_Id)
        {
            _REC = new REC();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //取得開庫紀錄檔
                var _TOR = db.TREA_OPEN_REC.AsNoTracking()
                    .FirstOrDefault(x => x.TREA_REGISTER_ID == vTreaRegisterId);

                //金庫管理者
                var UserData = new V_EMPLY2();
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    UserData = dbINTRA.V_EMPLY2.AsNoTracking().FirstOrDefault(x => x.USR_ID == vUser_Id);
                }

                //開庫類型
                var OpenTreaType = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE").ToList();

                if (_TOR != null)
                {
                    _REC.SYS_DATE = DateTime.Now.ToString("yyyy/MM/dd");
                    _REC.TREA_REGISTER_ID = _TOR.TREA_REGISTER_ID;
                    _REC.USER_NAME = UserData.EMP_NAME;
                    _REC.ACTUAL_PUT_TIME = string.IsNullOrEmpty(_TOR.ACTUAL_PUT_TIME.ToString()) ? null : DateTime.Parse(_TOR.ACTUAL_PUT_TIME.ToString()).ToString("HH:mm");
                    _REC.ACTUAL_GET_TIME = string.IsNullOrEmpty(_TOR.ACTUAL_GET_TIME.ToString()) ? null : DateTime.Parse(_TOR.ACTUAL_GET_TIME.ToString()).ToString("HH:mm");
                    _REC.OPEN_TREA_TYPE = OpenTreaType.FirstOrDefault(x => x.CODE == _TOR.OPEN_TREA_TYPE).CODE_VALUE;
                }
            }
        }

        protected void SetExtensionParm()
        {
            foreach (var item in _REC.GetType().GetProperties())
            {
                extensionParms.Add(new reportParm()
                {
                    key = item.Name,
                    value = item.GetValue(_REC)?.ToString(),
                });
            }
        }

        protected class REC
        {
            [Description("日期")]
            public string SYS_DATE { get; set; }

            [Description("金庫登記簿單號")]
            public string TREA_REGISTER_ID { get; set; }

            [Description("金庫管理者")]
            public string USER_NAME { get; set; }

            [Description("入庫時間")]
            public string ACTUAL_PUT_TIME { get; set; }

            [Description("出庫時間")]
            public string ACTUAL_GET_TIME { get; set; }

            [Description("開庫類型")]
            public string OPEN_TREA_TYPE { get; set; }
        }

        protected class Report_Treasury_Register
        {
            [Description("作業類型")]
            public string ITEM_OP_TYPE { get; set; }

            [Description("存取項目ID")]
            public string ITEM_ID { get; set; }

            [Description("存取項目")]
            public string ITEM_DESC { get; set; }

            [Description("印章內容")]
            public string SEAL_DESC { get; set; }

            [Description("作業別")]
            public string ACCESS_TYPE { get; set; }

            [Description("申請單號")]
            public string APLY_NO { get; set; }

            [Description("入庫原因")]
            public string ACCESS_REASON { get; set; }

            [Description("入庫人員")]
            public string ACCESS_NAME { get; set; }

            [Description("實際作業別")]
            public string ACTUAL_ACCESS_TYPE { get; set; }

            [Description("實際入庫人員")]
            public string ACTUAL_ACCESS_NAME { get; set; }
        }
    }
}