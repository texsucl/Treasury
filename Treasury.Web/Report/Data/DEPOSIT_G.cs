using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class DEPOSIT_G : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();
            string aply_No = parms.Where(x => x.key == "aply_No").FirstOrDefault()?.value ?? string.Empty;
            string isTWD = parms.Where(x => x.key == "isTWD").FirstOrDefault()?.value ?? string.Empty;
            string vDep_Type = parms.Where(x => x.key == "vDep_Type").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No, isTWD, vDep_Type);

            //報表資料
            List<ReportData> ReportDataList = new List<ReportData>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aply_No);

                var ReportData = new ReportData();
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
                        //設值否=N
                        var _IDOM_DataNList = _IDOM_DataList.Where(x => x.DEP_SET_QUALITY == "N").ToList();

                        foreach (var MasterDataN in _IDOM_DataNList)
                        {
                            //使用物品編號去定期存單庫存資料明細檔抓取資料
                            var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                                .Where(x => x.ITEM_ID == MasterDataN.ITEM_ID).ToList();

                            foreach (var DetailData in _IDOD_DataList)
                            {
                                ReportData = new ReportData()
                                {
                                    TYPE = "Data-N",
                                    EXPIRY_DATE = MasterDataN.EXPIRY_DATE.DateToTaiwanDate(9),
                                    TRAD_PARTNERS = MasterDataN.TRAD_PARTNERS,
                                    DEP_NO_B = DetailData.DEP_NO_B,
                                    DEP_NO_E = DetailData.DEP_NO_E,
                                    DEP_CNT = DetailData.DEP_CNT.ToString(),
                                    DENOMINATION = DetailData.DENOMINATION.ToString(),
                                };

                                ReportDataList.Add(ReportData);

                                TOTAL_DEP_CNT += DetailData.DEP_CNT;
                            }
                        }

                        //設值否=Y
                        var _IDOM_DataYList = _IDOM_DataList.Where(x => x.DEP_SET_QUALITY == "Y").ToList();

                        foreach (var MasterDataY in _IDOM_DataYList)
                        {
                            Decimal TOTAL_DENOMINATION = 0;

                            //使用物品編號去定期存單庫存資料明細檔抓取資料
                            var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                                .Where(x => x.ITEM_ID == MasterDataY.ITEM_ID).ToList();

                            foreach (var DetailData in _IDOD_DataList)
                            {
                                ReportData = new ReportData()
                                {
                                    TYPE = "Data-Y",
                                    CURRENCY=_REC.CURRENCY,
                                    EXPIRY_DATE=MasterDataY.EXPIRY_DATE.DateToTaiwanDate(9),
                                    TRAD_PARTNERS=MasterDataY.TRAD_PARTNERS,
                                    DEP_TYPE=_REC.DEP_TYPE,
                                    DEP_NO_B=DetailData.DEP_NO_B,
                                    DEP_NO_E=DetailData.DEP_NO_E,
                                    DEP_CNT=DetailData.DEP_CNT.ToString(),
                                    DENOMINATION=DetailData.DENOMINATION.ToString(),
                                };

                                ReportDataList.Add(ReportData);

                                TOTAL_DENOMINATION += DetailData.SUBTOTAL_DENOMINATION;
                            }

                            ReportData = new ReportData()
                            {
                                TYPE = "Data-Y",
                                CURRENCY = _REC.CURRENCY,
                                EXPIRY_DATE = MasterDataY.EXPIRY_DATE.DateToTaiwanDate(9),
                                TRAD_PARTNERS = MasterDataY.TRAD_PARTNERS,
                                DEP_TYPE = _REC.DEP_TYPE,
                                TOTAL_DENOMINATION = TOTAL_DENOMINATION.ToString()
                            };

                            ReportDataList.Add(ReportData);
                        }
                    }
                }

                //取得定存交割檢核項目
                var _Dep_Chk_Item = db.DEP_CHK_ITEM.AsNoTracking()
                .Where(x => x.ACCESS_TYPE == "G")
                .Where(x => x.IS_DISABLED == "N")
                .OrderBy(x => x.ISORTBY).ToList();

                foreach (var item in _Dep_Chk_Item)
                {
                    string DEP_CHK_ITEM_DESC = string.Empty;

                    //是否項次1
                    if (item.ISORTBY == 5)
                    {
                        DEP_CHK_ITEM_DESC = item.DEP_CHK_ITEM_DESC.Replace("@2", TOTAL_DEP_CNT.ToString());
                    }
                    else
                    {
                        DEP_CHK_ITEM_DESC = item.DEP_CHK_ITEM_DESC;
                    }

                    ReportData = new ReportData()
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