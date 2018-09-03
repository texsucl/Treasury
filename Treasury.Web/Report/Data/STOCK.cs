using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class STOCK : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            string aply_No = parms.Where(x => x.key == "aply_No").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No);

            //報表資料
            List<StockReportData> ReportDataList = new List<StockReportData>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aply_No);

                var ReportData = new StockReportData();
                decimal? NUMBER_OF_SHARES_TOTAL = 0;
                
                //取得股票明細資料
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去股票庫存資料檔抓取資料
                    var _IS_DataList = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    var IS_Group_No_DaatList = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID))
                        .Select(x => x.GROUP_NO).ToList();
                    //使用群組編號去存取項目冊號資料檔抓取資料
                    var _IB_DataList = _IS_DataList.GroupBy(x => x.TREA_BATCH_NO);
                    var _IB_Data = db.ITEM_BOOK.AsNoTracking()
                        .Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString())
                        .Where(x => IS_Group_No_DaatList.Contains(x.GROUP_NO))
                        .Where(x => x.COL == "NAME").FirstOrDefault();
                    //類型清單
                    var _IS_StockTypeList = db.SYS_CODE.AsNoTracking()
                        .Where(x => x.CODE_TYPE == "STOCK_TYPE")
                        .OrderBy(x => x.ISORTBY).ToList();

                    if (_IB_DataList.Any())
                    {
                        foreach (var ItemBook in _IB_DataList)
                        {
                            #region 存取項目冊號資料
                            //計算總股數
                            NUMBER_OF_SHARES_TOTAL = _IS_DataList.Where(x => x.TREA_BATCH_NO == ItemBook.Key).Sum(x => x.NUMBER_OF_SHARES);

                            ReportData = new StockReportData()
                            {
                                TYPE = "B",
                                GROUP_NO = _IB_Data.GROUP_NO.ToString(),
                                NAME = _IB_Data.COL_VALUE,
                                TREA_BATCH_NO = ItemBook.Key.ToString(),
                                NUMBER_OF_SHARES_TOTAL = NUMBER_OF_SHARES_TOTAL
                            };

                            ReportDataList.Add(ReportData);
                            #endregion

                            #region 股票庫存資料
                            var _IS_TREA_BATCH_NO_DataList = _IS_DataList.Where(x => x.TREA_BATCH_NO == ItemBook.Key).ToList();
                            int ROW_NUMBER = 1;

                            foreach (var ItemStock in _IS_TREA_BATCH_NO_DataList)
                            {
                                //計算面額小計
                                decimal DENOMINATION_TOTAL = 0M;
                                if(ItemStock.STOCK_CNT != null && ItemStock.DENOMINATION != null)
                                {
                                    DENOMINATION_TOTAL = (decimal.Parse(ItemStock.STOCK_CNT.ToString()) * decimal.Parse(ItemStock.DENOMINATION.ToString()));
                                }

                                ReportData = new StockReportData()
                                {
                                    TYPE = "S",
                                    ROW_NUMBER = ROW_NUMBER.ToString(),
                                    TREA_BATCH_NO = ItemStock.TREA_BATCH_NO.ToString(),
                                    STOCK_TYPE = _IS_StockTypeList.Where(x => x.CODE == ItemStock.STOCK_TYPE).Select(x => x.CODE_VALUE).FirstOrDefault(),
                                    STOCK_NO_PREAMBLE = ItemStock.STOCK_NO_PREAMBLE,
                                    STOCK_NO_B = ItemStock.STOCK_NO_B,
                                    STOCK_NO_E = ItemStock.STOCK_NO_E,
                                    STOCK_CNT = ItemStock.STOCK_CNT,
                                    AMOUNT_PER_SHARE = ItemStock.AMOUNT_PER_SHARE,
                                    SINGLE_NUMBER_OF_SHARES = ItemStock.SINGLE_NUMBER_OF_SHARES,
                                    DENOMINATION = ItemStock.DENOMINATION,
                                    DENOMINATION_TOTAL = DENOMINATION_TOTAL,
                                    NUMBER_OF_SHARES = ItemStock.NUMBER_OF_SHARES,
                                    MEMO = ItemStock.MEMO
                                };

                                ReportDataList.Add(ReportData);

                                ROW_NUMBER++;
                            }
                            #endregion
                        }
                    }
                }
            }

            resultsTable.Tables.Add(ReportDataList.ToDataTable());

            SetExtensionParm();

            return resultsTable;
        }

    }

    //修正 欄位型態 by mark 20180903
    public class StockReportData
    {
        [Description("類型")]
        public string TYPE { get; set; }

        [Description("編號")]
        public string GROUP_NO { get; set; }

        [Description("股票名稱")]
        public string NAME { get; set; }

        [Description("入庫批號")]
        public string TREA_BATCH_NO { get; set; }

        [Description("總股數")]
        public decimal? NUMBER_OF_SHARES_TOTAL { get; set; }

        [Description("項次")]
        public string ROW_NUMBER { get; set; }

        [Description("類型")]
        public string STOCK_TYPE { get; set; }

        [Description("序號前置碼")]
        public string STOCK_NO_PREAMBLE { get; set; }

        [Description("序號(起)")]
        public string STOCK_NO_B { get; set; }

        [Description("序號(迄)")]
        public string STOCK_NO_E { get; set; }

        [Description("張數")]
        public int? STOCK_CNT { get; set; }

        [Description("每股金額")]
        public decimal? AMOUNT_PER_SHARE { get; set; }

        [Description("單張股數")]
        public decimal? SINGLE_NUMBER_OF_SHARES { get; set; }

        [Description("單張面額")]
        public decimal? DENOMINATION { get; set; }

        [Description("面額小計")]
        public decimal? DENOMINATION_TOTAL { get; set; }

        [Description("股數小計")]
        public decimal? NUMBER_OF_SHARES { get; set; }

        [Description("備註")]
        public string MEMO { get; set; }
    }
}