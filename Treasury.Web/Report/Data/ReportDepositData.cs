﻿using System;
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

        protected void SetDetail(string aply_No, string isTWD, string vDep_Type)
        {
            _REC = new REC();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                _REC.CURRENCY = isTWD == "Y" ? "台幣" : "外幣";

                _REC.DEP_TYPE = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DEP_TYPE" && x.CODE == vDep_Type)
                    .Select(x => x.CODE_VALUE).FirstOrDefault();

                _REC.SYS_TYPE = DateTime.Now.DateToTaiwanDate(9);

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

                    _REC.COMMIT_DATE = _IDOM_DataList
                        .Where(x => x.CURRENCY == "TWD", isTWD == "Y")
                        .Where(x => x.CURRENCY != "TWD", isTWD == "N")
                        .Where(x => x.DEP_TYPE == vDep_Type)
                        .Select(x => x.COMMIT_DATE).FirstOrDefault().DateToTaiwanDate(9);
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
            [Description("台幣/外幣")]
            public string CURRENCY { get; set; }

            [Description("存單類型")]
            public string DEP_TYPE { get; set; }

            [Description("交割日期")]
            public string SYS_TYPE { get; set; }

            [Description("承作日期")]
            public string COMMIT_DATE { get; set; }

        }

        protected class ReportData
        {
            [Description("到期日")]
            public string EXPIRY_DATE { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("存單號碼(起)")]
            public string DEP_NO_B { get; set; }

            [Description("存單號碼(迄)")]
            public string DEP_NO_E { get; set; }

            [Description("張數")]
            public int DEP_CNT { get; set; }

            [Description("單張面額")]
            public Decimal DENOMINATION { get; set; }

            [Description("總面額")]
            public Decimal TOTAL_DENOMINATION { get; set; }

        }
    }
}