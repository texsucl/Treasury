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
    public class DEPOSIT_ESTATE : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportESTATEData> ReportDataList = new List<DepositReportESTATEData>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportESTATEData();
            string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;
            string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;
            string BOOK_NO = parms.Where(x =>x.key == "vBook_No" ).FirstOrDefault()?.value ?? string.Empty;
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

                int TOTAL= 0;
                var _IRE = db.ITEM_REAL_ESTATE.AsNoTracking().ToList();

                if(BOOK_NO=="All"){//判斷為全部或單一
                    _IRE=  db.ITEM_REAL_ESTATE.AsNoTracking()//判斷是否在庫
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
                    .Where(x=> x.PUT_DATE <= _APLY_ODT_To ,   _APLY_ODT_To != null).ToList();
                }
                else{
                    _IRE=  db.ITEM_REAL_ESTATE.AsNoTracking()//判斷是否在庫
                    .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
                    .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
                    .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                    .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                    .Where(x=> x.GROUP_NO.ToString()==BOOK_NO)
                    .Where(x=> x.PUT_DATE >= _APLY_ODT_From ,_APLY_ODT_From != null  )
                    .Where(x=> x.PUT_DATE <= _APLY_ODT_To ,   _APLY_ODT_To != null  ).ToList();
                }          

                var depts = new List<VW_OA_DEPT>();
                var types = new List<SYS_CODE>(); 
                var book = new List<ITEM_BOOK>(); 
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                   depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }
                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();                
                book = db.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO.ToString() !=null).ToList();
                foreach(var ESTATEdata in _IRE.OrderBy(x=>x.GROUP_NO).ThenBy(x=>x.PUT_DATE).ThenBy(x=>x.ESTATE_FORM_NO).ThenBy(x=>x.ESTATE_DATE).ThenBy(x=>x.OWNERSHIP_CERT_NO)) 
                {
                    TOTAL ++;
                    ReportData = new DepositReportESTATEData()
                    {
                        ROW = TOTAL,
                        PUT_DATE = ESTATEdata.PUT_DATE.dateTimeToStr(),
                        ESTATE_FORM_NO = ESTATEdata.ESTATE_FORM_NO,
                        ESTATE_DATE = ESTATEdata.ESTATE_DATE.dateTimeToStr(),
                        OWNERSHIP_CERT_NO = ESTATEdata.OWNERSHIP_CERT_NO,
                        LAND_BUILDING_NO = ESTATEdata.LAND_BUILDING_NO,
                        HOUSE_NO = ESTATEdata.HOUSE_NO,
                        ESTATE_SEQ = ESTATEdata.ESTATE_SEQ,
                        BOOK_NO_DETAIL = ESTATEdata.GROUP_NO.ToString(),
                        BUILDING_NAME = getBuildName(book,ESTATEdata.GROUP_NO.ToString()),
                        LOCATED = getBuildName(book,ESTATEdata.GROUP_NO.ToString()),
                        CHARGE_DEPT =getEmpName(depts,ESTATEdata.CHARGE_DEPT),
                        CHARGE_SECT= getEmpName(depts,ESTATEdata.CHARGE_SECT),
                        CHARGE_DEPT_ID = ESTATEdata.CHARGE_DEPT,
                        CHARGE_SECT_ID = ESTATEdata.CHARGE_SECT,
                        MEMO =ESTATEdata.MEMO,
                    };
                    ReportDataList.Add(ReportData);
                }
            }
            resultsTable.Tables.Add(ReportDataList.ToDataTable());
            return resultsTable;
        }
        protected ITEM_REAL_ESTATE IRE { get; private set; }

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
        /// 使用 不動產GROUP_NO 獲得 大樓名稱
        /// </summary>
        /// <param name="GROUP_NO"></param>
        /// <param name="COL_VALUE"></param>
        /// <returns></returns>
        private string getBuildName(List<ITEM_BOOK> types, string GROUPNO)
        {
            if (!GROUPNO.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.GROUP_NO.ToString().Trim() == GROUPNO.Trim() && x.COL == "BUILDING_NAME" && x.ITEM_ID.Trim() == "D1014")?.COL_VALUE?.Trim();
            return string.Empty;
        }

        /// <summary>
        /// 使用 GROUP_NO 獲得 坐落
        /// </summary>
        /// <param name="GROUP_NO"></param>
        /// <param name="COL_VALUE"></param>
        /// <returns></returns>
        private string getLocated(List<ITEM_BOOK> types, string GROUPNO)
        {
            if (!GROUPNO.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.GROUP_NO.ToString().Trim() == GROUPNO.Trim() && x.COL == "LOCATED" && x.ITEM_ID.Trim() == "D1014")?.COL_VALUE?.Trim();
            return string.Empty;
        }

    }
}