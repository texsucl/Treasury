using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Report.Data
{
    public class DEPOSIT_SEAL : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportSealData> ReportDataList = new List<DepositReportSealData>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportSealData();
            string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;
            string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;
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
                INVENTORY_STATUSs.AddRange(new List<string>() {
                    ((int)AccessInventoryType._5).ToString(),    //預約取出，計庫存
                    ((int)AccessInventoryType._6).ToString(),    //已被取出，計庫存
                    ((int)AccessInventoryType._9).ToString() }); //預約存入，計庫存
                //預約取出 , 用印  , 存入用印
                var _IS=  db.ITEM_SEAL.AsNoTracking()//判斷是否在庫
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
                    .Where(x=> x.PUT_DATE >= _APLY_ODT_From , _APLY_ODT_From != null  )
                    .Where(x=> x.PUT_DATE <= _APLY_ODT_To , _APLY_ODT_To != null  )
                    .Where(x=> x.TREA_ITEM_NAME == JobProject).ToList();

                var depts = new List<VW_OA_DEPT>();
                //var types = new List<SYS_CODE>(); 
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                   depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }

                //types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();

                foreach(var Sealdata in _IS.OrderBy(x=>x.PUT_DATE).ThenBy(x=>x.CHARGE_DEPT).ThenBy(x=>x.CHARGE_SECT).ThenBy(x=>x.ITEM_ID)) 
                {
                    TOTAL ++;
                    var _CHARGE_DEPT = getEmpName(depts, Sealdata.CHARGE_DEPT);
                    var _CHARGE_SECT = getEmpName(depts, Sealdata.CHARGE_SECT).Replace(_CHARGE_DEPT,"")?.Trim();
                    ReportData = new DepositReportSealData()
                    {
                        ROW = TOTAL,   
                        PUT_DATE = Sealdata.PUT_DATE_ACCESS.dateTimeToStr(),
                        CHARGE_DEPT = _CHARGE_DEPT,
                        CHARGE_SECT = _CHARGE_SECT,
                        SEAL_DESC = Sealdata.SEAL_DESC,
                        MEMO =Sealdata.MEMO,
                        CHARGE_DEPT_ID = Sealdata.CHARGE_DEPT,
                        CHARGE_SECT_ID = Sealdata.CHARGE_SECT
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
    }
}