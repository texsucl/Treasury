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
            string isNTD = parms.Where(x => x.key == "isNTD").FirstOrDefault()?.value ?? string.Empty;
            string vDep_Type = parms.Where(x => x.key == "vDep_Type").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No, isNTD, vDep_Type);

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
                        .Where(x => x.CURRENCY == "NTD", isNTD == "Y")
                        .Where(x => x.CURRENCY != "NTD", isNTD == "N")
                        .Where(x => x.DEP_TYPE != null)
                        .Where(x => x.DEP_TYPE == vDep_Type , vDep_Type != "0")
                        .OrderBy(x=>x.DEP_TYPE)
                        .ThenBy(x=> x.ITEM_ID)
                        .ToList();

                    var DEP_TYPEs = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DEP_TYPE").ToList() ;

                    if (_IDOM_DataList.Any())
                    {
                        //設值否=N
                        var _IDOM_DataNList = _IDOM_DataList.Where(x => x.DEP_SET_QUALITY == "N").ToList();

                        foreach (var MasterDataN in _IDOM_DataNList)
                        {
                            //使用物品編號去定期存單庫存資料明細檔抓取資料
                            var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                                .Where(x => x.ITEM_ID == MasterDataN.ITEM_ID).ToList();

                            List<DepositReportData> addDatas = new List<DepositReportData>();

                            foreach (var DetailData in _IDOD_DataList)
                            {
                                ReportData = new DepositReportData()
                                {
                                    TYPE = "Data-N",
                                    ITEM_ID = DetailData.ITEM_ID,
                                    EXPIRY_DATE = TypeTransfer.dateTimeToString(MasterDataN.EXPIRY_DATE,false),
                                    TRAD_PARTNERS = MasterDataN.TRAD_PARTNERS,
                                    DEP_TYPE = MasterDataN.DEP_TYPE,
                                    DEP_NO_B = DetailData.DEP_NO_B,
                                    DEP_NO_E = DetailData.DEP_NO_E,
                                    DEP_CNT = DetailData.DEP_CNT,
                                    DENOMINATION = DetailData.DENOMINATION,
                                    MSG = MasterDataN.GET_MSG
                                };

                                //ReportDataList.Add(ReportData);

                                addDatas.Add(ReportData);

                                TOTAL_DEP_CNT += DetailData.DEP_CNT;
                            }

                            ReportDataList.AddRange(addDatas);
                        }

                        //設值否=Y
                        var _IDOM_DataYList = _IDOM_DataList.Where(x => x.DEP_SET_QUALITY == "Y").ToList();

                        if (_IDOM_DataYList.Any()) //設質否
                            _REC.DEP_SET_QUALITY = "Y";

                        foreach (var MasterDataY in _IDOM_DataYList)
                        {
                            Decimal TOTAL_DENOMINATION = 0;

                            //使用物品編號去定期存單庫存資料明細檔抓取資料
                            var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                                .Where(x => x.ITEM_ID == MasterDataY.ITEM_ID).ToList();

                            List<DepositReportData> addDatas = new List<DepositReportData>();

                            foreach (var DetailData in _IDOD_DataList)
                            {
                                ReportData = new DepositReportData()
                                {
                                    TYPE = "Data-Y",
                                    CURRENCY=_REC.CURRENCY,
                                    EXPIRY_DATE=TypeTransfer.dateTimeToString(MasterDataY.EXPIRY_DATE,false),
                                    TRAD_PARTNERS=MasterDataY.TRAD_PARTNERS,
                                    DEP_TYPE_D = DEP_TYPEs.FirstOrDefault(x=>x.CODE == MasterDataY.DEP_TYPE)?.CODE_VALUE,
                                    DEP_TYPE　= MasterDataY.DEP_TYPE,
                                    DEP_NO_B=DetailData.DEP_NO_B,
                                    DEP_NO_E=DetailData.DEP_NO_E,
                                    DEP_CNT=DetailData.DEP_CNT,
                                    DENOMINATION=DetailData.DENOMINATION,
                                };

                                addDatas.Add(ReportData);

                                //ReportDataList.Add(ReportData);

                                TOTAL_DENOMINATION += DetailData.SUBTOTAL_DENOMINATION;
                            }

                            addDatas.ForEach(x =>
                            {
                                x.TOTAL_DENOMINATION = TOTAL_DENOMINATION;
                            });

                            ReportDataList.AddRange(addDatas);

                            //ReportData = new DepositReportData()
                            //{
                            //    TYPE = "Data-Y",
                            //    CURRENCY = _REC.CURRENCY,
                            //    EXPIRY_DATE = TypeTransfer.dateTimeToString(MasterDataY.EXPIRY_DATE,false),
                            //    TRAD_PARTNERS = MasterDataY.TRAD_PARTNERS,
                            //    DEP_TYPE = MasterDataY.DEP_TYPE,
                            //    TOTAL_DENOMINATION = TOTAL_DENOMINATION
                            //};

                            //ReportDataList.Add(ReportData);
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

                    //是否為項次5 確認當日交易已全數交割完成，到期存單共@2張
                    if (item.ISORTBY == 5)
                    {
                        DEP_CHK_ITEM_DESC = item.DEP_CHK_ITEM_DESC.Replace("@2", TOTAL_DEP_CNT.ToString().formateThousand());
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