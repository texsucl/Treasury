using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class DEPOSIT_P : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();
            string aply_No = parms.Where(x => x.key == "aply_No").FirstOrDefault()?.value ?? string.Empty;
            string isTWD = parms.Where(x => x.key == "isTWD").FirstOrDefault()?.value ?? string.Empty;
            string vDep_Type = parms.Where(x => x.key == "vDep_Type").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No, isTWD, vDep_Type);

            //報表資料
            List<DepositReportData> ReportDataList = new List<DepositReportData>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aply_No);

                var ReportData = new DepositReportData();
                int TOTAL_DEP_CNT = 0;

                //取得定存明細資料
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID))
                        .Where(x => x.CURRENCY == "TWD", isTWD == "Y")
                        .Where(x => x.CURRENCY != "TWD", isTWD == "N")
                        .Where(x => x.DEP_TYPE == vDep_Type)
                        .ToList();

                    if (_IDOM_DataList.Any())
                    {
                        foreach (var MasterData in _IDOM_DataList)
                        {
                            Decimal TOTAL_DENOMINATION = 0;

                            //使用物品編號去定期存單庫存資料明細檔抓取資料
                            var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                                .Where(x => x.ITEM_ID == MasterData.ITEM_ID).ToList();

                            foreach(var DetailData in _IDOD_DataList)
                            {
                                ReportData = new DepositReportData()
                                {
                                    TYPE = "Data",
                                    EXPIRY_DATE = TypeTransfer.dateTimeToString(MasterData.EXPIRY_DATE,false),
                                    TRAD_PARTNERS = MasterData.TRAD_PARTNERS,
                                    DEP_NO_B = DetailData.DEP_NO_B,
                                    DEP_NO_E = DetailData.DEP_NO_E,
                                    DEP_CNT = DetailData.DEP_CNT,
                                    DENOMINATION = DetailData.DENOMINATION,
                                };

                                ReportDataList.Add(ReportData);

                                TOTAL_DENOMINATION += DetailData.SUBTOTAL_DENOMINATION;
                                TOTAL_DEP_CNT += DetailData.DEP_CNT;
                            }

                            ReportData = new DepositReportData()
                            {
                                TYPE = "Data",
                                EXPIRY_DATE = TypeTransfer.dateTimeToString(MasterData.EXPIRY_DATE, false),
                                TRAD_PARTNERS = MasterData.TRAD_PARTNERS,
                                TOTAL_DENOMINATION = TOTAL_DENOMINATION
                            };

                            ReportDataList.Add(ReportData);
                        }
                    }
                }

                //取得定存交割檢核項目
                var _Dep_Chk_Item = db.DEP_CHK_ITEM.AsNoTracking()
                    .Where(x => x.ACCESS_TYPE == "P")
                    .Where(x => x.IS_DISABLED == "N")
                    .OrderBy(x => x.ISORTBY).ToList();

                foreach(var item in _Dep_Chk_Item)
                {
                    string DEP_CHK_ITEM_DESC = string.Empty;

                    //是否項次1
                    if (item.ISORTBY==1)
                    {
                        DEP_CHK_ITEM_DESC = item.DEP_CHK_ITEM_DESC.Replace("@1", TOTAL_DEP_CNT.ToString());
                    }
                    else
                    {
                        DEP_CHK_ITEM_DESC = item.DEP_CHK_ITEM_DESC;
                    }

                    ReportData = new DepositReportData()
                    {
                        TYPE = "Item",
                        ISORTBY = item.ISORTBY.ToString(),
                        DEP_CHK_ITEM_DESC = DEP_CHK_ITEM_DESC
                    };

                    ReportDataList.Add(ReportData);
                }
            }

            resultsTable.Tables.Add(ReportDataList.ToDataTable());

            SetExtensionParm();

            return resultsTable;
        }
    }
}