using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class DEPOSIT_STOCK : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportSTOCKData> ReportDataList = new List<DepositReportSTOCKData>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportSTOCKData();
            string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;
            string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;
            string BOOK_NO = parms.Where(x =>x.key == "vName" ).FirstOrDefault()?.value ?? string.Empty;
            string JobProject = parms.Where(x =>x.key == "vJobProject" ).FirstOrDefault()?.value ?? string.Empty;
            string APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value ?? string.Empty; //庫存日期
            string APLY_ODT_From = parms.Where(x =>x.key == "APLY_ODT_From" ).FirstOrDefault()?.value ?? string.Empty;
            string APLY_ODT_To = parms.Where(x =>x.key == "APLY_ODT_To" ).FirstOrDefault()?.value ?? string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From).DateToLatestTime();
                var _APLY_DT_Date = _APLY_DT.Date;
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
                var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To).DateToLatestTime();
                var _IS = new List<ITEM_STOCK>();

                _IS=  db.ITEM_STOCK.AsNoTracking()//判斷是否在庫
                .Where(x => INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS), _APLY_DT_Date == dtn)
                .Where(x =>
                (INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS) && x.PUT_DATE <= _APLY_DT) // 在庫 且 存入日期 <= 庫存日期 
                ||
                (x.INVENTORY_STATUS == INVENTORY_STATUSg && 
                 x.PUT_DATE <= _APLY_DT && 
                 _APLY_DT < x.GET_DATE),  //存入日期 <= 庫存日期 且 庫存日期 < 取出日期
                _APLY_DT_Date != dtn)
                .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                .Where(x=> x.PUT_DATE >= _APLY_ODT_From , _APLY_ODT_From != null)
                .Where(x=> x.PUT_DATE <= _APLY_ODT_To , _APLY_ODT_To != null)
                .Where(x => x.GROUP_NO.ToString() == BOOK_NO, BOOK_NO != "All") //判斷為全部或單一
                .ToList();
                
                var depts = new List<VW_OA_DEPT>();
                var area = new List<SYS_CODE>();
                var types = new List<SYS_CODE>();
                var book = new List<ITEM_BOOK>(); 
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                   depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }

                area = db.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null && x.CODE_TYPE == "STOCK_AREA").ToList();
                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE != null && x.CODE_TYPE == "STOCK_TYPE").ToList();
                book = db.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO.ToString() !=null).ToList();
                
                foreach(var STOCKdata in _IS.OrderBy(x=>x.GROUP_NO).ThenBy(x=>x.PUT_DATE).ThenBy(x=>x.CHARGE_DEPT).ThenBy(x=>x.CHARGE_SECT).ThenBy(x=>x.GROUP_NO).ThenBy(x=>x.STOCK_NO_PREAMBLE).ThenBy(x=>x.STOCK_NO_B)) 
                {
                    var _CHARGE_DEPT = getEmpName(depts, STOCKdata.CHARGE_DEPT);
                    var _CHARGE_SECT = getEmpName(depts, STOCKdata.CHARGE_SECT).Replace(_CHARGE_DEPT, "")?.Trim();

                    ReportData = new DepositReportSTOCKData()
                    {
                        PUT_DATE = STOCKdata.PUT_DATE.dateTimeToStr(),
                        STOCK_NO_B = STOCKdata.STOCK_NO_B,
                        STOCK_NO_E = STOCKdata.STOCK_NO_E,
                        STOCK_CNT = STOCKdata.STOCK_CNT,
                        DENOMINATION = STOCKdata.DENOMINATION,
                        NUMBER_OF_SHARES = STOCKdata.NUMBER_OF_SHARES,
                        AREA = area.FirstOrDefault(z=>z.CODE == getArea(book, STOCKdata.GROUP_NO.ToString()))?.CODE_VALUE,
                        STOCK_TYPE = types.FirstOrDefault(z => z.CODE == STOCKdata.STOCK_TYPE)?.CODE_VALUE,
                        BATCH_NO = STOCKdata.TREA_BATCH_NO.ToString(),
                        STOCK_NO_PREAMBLE = STOCKdata.STOCK_NO_PREAMBLE,
                        AMOUNT_PER_SHARE = STOCKdata.AMOUNT_PER_SHARE,
                        SINGLE_NUMBER_OF_SHARES = STOCKdata.SINGLE_NUMBER_OF_SHARES,
                        CHARGE_DEPT = _CHARGE_DEPT,
                        CHARGE_SECT = _CHARGE_SECT,
                        MEMO =STOCKdata.MEMO,
                        BOOK_NO = STOCKdata.GROUP_NO.ToString(),
                        NAME = getName(book,STOCKdata.GROUP_NO.ToString()),
                        CHARGE_DEPT_ID = STOCKdata.CHARGE_DEPT,
                        CHARGE_SECT_ID = STOCKdata.CHARGE_SECT
                    };
                    ReportDataList.Add(ReportData);
                }
            }
            resultsTable.Tables.Add(ReportDataList.ToDataTable());
            return resultsTable;
        }

        /// <summary>
        /// 使用 部門ID 獲得 部門名稱
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        private string getEmpName(List<VW_OA_DEPT> depts, string DPT_CD)
        {
            if (!DPT_CD.IsNullOrWhiteSpace() && depts.Any())
                return depts.FirstOrDefault(x => x.DPT_CD.Trim() == DPT_CD.Trim())?.DPT_NAME?.Trim();
            return string.Empty;
        }

        /// <summary>
        /// 使用 股票GROUP_NO 獲得 區域
        /// </summary>
        /// <param name="GROUP_NO"></param>
        /// <param name="COL_VALUE"></param>
        /// <returns></returns>
        private string getArea(List<ITEM_BOOK> types, string GROUPNO)
        {
            if (!GROUPNO.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.GROUP_NO.ToString().Trim() == GROUPNO.Trim() && x.COL == "AREA" && x.ITEM_ID.Trim() == "D1015")?.COL_VALUE?.Trim();
            return string.Empty;
        }
        /// <summary>
        /// 使用 股票GROUP_NO 獲得 股票名稱
        /// </summary>
        /// <param name="GROUP_NO"></param>
        /// <param name="COL_VALUE"></param>
        /// <returns></returns>
        private string getName(List<ITEM_BOOK> types, string GROUPNO)
        {
            if (!GROUPNO.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.GROUP_NO.ToString().Trim() == GROUPNO.Trim() && x.COL == "NAME" && x.ITEM_ID.Trim() == "D1015")?.COL_VALUE?.Trim();
            return string.Empty;
        }
    }
}