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
    public abstract class ReportDepositData : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public List<reportParm> extensionParms { get; set; }

        public ReportDepositData()
        {
            extensionParms = new List<reportParm>();
            defaultConnection = System.Configuration.ConfigurationManager.
                         ConnectionStrings["dbTreasury"].ConnectionString;
        }

        public abstract DataSet GetData(List<reportParm> parms);

        protected REC _REC { get; private set; }

        protected void SetDetail(string aply_No, string isNTD, string vDep_Type)
        {
            _REC = new REC();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                _REC.APLY_NO = aply_No;
                _REC.CURRENCY = isNTD == "Y" ? "台幣" : "外幣";

                _REC.DEP_TYPE = vDep_Type;

                //_REC.SYS_TYPE = DateTime.Now.DateToTaiwanDate(9);
                _REC.SYS_TYPE = TypeTransfer.dateTimeToString(DateTime.Now,false);

                //取得承作日期
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aply_No);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    _REC.COMMIT_DATE = TypeTransfer.dateTimeToString(_IDOM_DataList
                        .Where(x => x.CURRENCY == "NTD", isNTD == "Y")
                        .Where(x => x.CURRENCY != "NTD", isNTD == "N")
                        .Where(x => x.DEP_TYPE == vDep_Type, vDep_Type != "0")
                        .Select(x => x.COMMIT_DATE).FirstOrDefault(),false);
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

        protected void SetDetail(string aply_No)
        {
            var depts = new List<VW_OA_DEPT>();
            var emps = new List<V_EMPLY2>();
            _REC = new REC();
            //var sys
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
        }
        protected class REC
        {
            public REC() {
                DEP_SET_QUALITY = "N";
            }

            [Description("申請單號")]
            public string APLY_NO { get; set; }

            [Description("台幣/外幣")]
            public string CURRENCY { get; set; }

            [Description("存單類型")]
            public string DEP_TYPE { get; set; }

            [Description("交割日期")]
            public string SYS_TYPE { get; set; }

            [Description("承作日期")]
            public string COMMIT_DATE { get; set; }

            [Description("設質否")]
            public string DEP_SET_QUALITY { get; set; }

        }

        //修正 欄位型態 by mark 20180903
        protected class DepositReportData
        {
            [Description("類型")]
            public string TYPE { get; set; }

            [Description("幣別")]
            public string CURRENCY { get; set; }

            [Description("到期日")]
            public string EXPIRY_DATE { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("存單類型")]
            public string DEP_TYPE { get; set; }

            [Description("存單類型(中文)")]
            public string DEP_TYPE_D { get; set; }

            [Description("存單號碼(起)")]
            public string DEP_NO_B { get; set; }

            [Description("存單號碼(迄)")]
            public string DEP_NO_E { get; set; }

            [Description("張數")]
            public int DEP_CNT { get; set; }

            [Description("單張面額")]
            public decimal? DENOMINATION { get; set; }

            [Description("總面額")]
            public decimal? TOTAL_DENOMINATION { get; set; }

            [Description("項次")]
            public string ISORTBY { get; set; }

            [Description("檢核項目")]
            public string DEP_CHK_ITEM_DESC { get; set; }

        }
        protected class DepositReportMarginpgData
        {
            [Description("類別")]
            public string MARGIN_DEP_TYPE { get; set; }

            [Description("歸檔編號")]
            public string ITEM_ID { get; set; }

            [Description("冊號")]
            public string BOOK_NO { get; set; }

            [Description("入庫日期")]
            public DateTime? PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }


            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("金額")]
            public decimal? AMOUNT { get; set; }

            [Description("職場代號 ")]
            public string WORKPLACE_CODE { get; set; }

            [Description("說明")]
            public string DESCRIPTION { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }
        }

    }
}