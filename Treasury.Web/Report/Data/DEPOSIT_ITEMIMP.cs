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
    public class DEPOSIT_ITEMIMP : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportITEMIMPData> ReportDataList = new List<DepositReportITEMIMPData>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportITEMIMPData();
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
                var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To).DateToLatestTime(); ;
              
                int TOTAL= 0;
                INVENTORY_STATUSs.AddRange(new List<string>() {
                    ((int)AccessInventoryType._5).ToString(),    //預約取出，計庫存
                    ((int)AccessInventoryType._6).ToString(),    //已被取出，計庫存
                    ((int)AccessInventoryType._9).ToString() }); //預約存入，計庫存
                var _II=  db.ITEM_IMPO.AsNoTracking()//判斷是否在庫
                    .Where(x => INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS), _APLY_DT_Date == dtn)
                    .Where(x =>
                    (INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS) && x.PUT_DATE <= _APLY_DT) // 在庫 且 存入日期 <= 庫存日期 
                    ||
                    (x.INVENTORY_STATUS == INVENTORY_STATUSg && 
                     x.PUT_DATE <= _APLY_DT && 
                     _APLY_DT < x.GET_DATE),  //存入日期 <= 庫存日期 且 庫存日期 < 取出日期
                    _APLY_DT_Date != dtn)
                    .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                    .Where(x => x.CHARGE_SECT == vsect , vsect !="All")
                    .Where(x=> x.PUT_DATE >= _APLY_ODT_From , _APLY_ODT_From != null  )
                    .Where(x=> x.PUT_DATE <= _APLY_ODT_To , _APLY_ODT_To != null  )
                    .ToList();

                var depts = new List<VW_OA_DEPT>();
                var types = new List<SYS_CODE>(); 
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                   depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }

                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();

                if (_APLY_DT_Date != dtn)
                {
                    List<string> Item_IDs = _II.Select(x => x.ITEM_ID_FROM == null ? x.ITEM_ID : x.ITEM_ID_FROM).Distinct().ToList();

                    List<ITEM_IMPO> ITEM_IMPOs = db.ITEM_IMPO.AsNoTracking().Where(x => Item_IDs.Contains(x.ITEM_ID)).ToList();

                    List<ITEM_IMPO> GetITEM_IMPOs = db.ITEM_IMPO.AsNoTracking()
                        .Where(x => x.ITEM_ID_FROM != null && Item_IDs.Contains(x.ITEM_ID_FROM) && _APLY_DT > x.GET_DATE).ToList();
                         //取出日 < 庫存日,之前取出

                    foreach (var item in ITEM_IMPOs)
                    {
                        var _CHARGE_DEPT = getEmpName(depts, item.CHARGE_DEPT);
                        var _CHARGE_SECT = getEmpName(depts, item.CHARGE_SECT).Replace(_CHARGE_DEPT, "")?.Trim();
                        ReportData = new DepositReportITEMIMPData()
                        {
                            ITEM_ID = item.ITEM_ID,
                            PUT_DATE = item.PUT_DATE.dateTimeToStr(),
                            CHARGE_DEPT = _CHARGE_DEPT,
                            CHARGE_SECT = _CHARGE_SECT,
                            ITEM_NAME = item.ITEM_NAME,
                            QUANTITY = GetQUANTITY(item, GetITEM_IMPOs),
                            AMOUNT = item.AMOUNT,
                            EXPECTED_ACCESS_DATE = item.EXPECTED_ACCESS_DATE.dateTimeToStr(),
                            DESCRIPTION = item.DESCRIPTION,
                            MEMO = item.MEMO,
                            CHARGE_DEPT_ID = item.CHARGE_DEPT,
                            CHARGE_SECT_ID = item.CHARGE_SECT
                        };
                        ReportDataList.Add(ReportData);
                    }
                }
                else
                {
                    foreach (var item in _II)
                    {
                        ReportData = new DepositReportITEMIMPData()
                        {
                            ITEM_ID = item.ITEM_ID,
                            PUT_DATE = item.PUT_DATE.dateTimeToStr(),
                            CHARGE_DEPT = getEmpName(depts, item.CHARGE_DEPT),
                            CHARGE_SECT = getEmpName(depts, item.CHARGE_SECT),
                            ITEM_NAME = item.ITEM_NAME,
                            QUANTITY = item.REMAINING,
                            AMOUNT = item.AMOUNT,
                            EXPECTED_ACCESS_DATE = item.EXPECTED_ACCESS_DATE.dateTimeToStr(),
                            DESCRIPTION = item.DESCRIPTION,
                            MEMO = item.MEMO,
                            CHARGE_DEPT_ID = item.CHARGE_DEPT,
                            CHARGE_SECT_ID = item.CHARGE_SECT
                        };
                        ReportDataList.Add(ReportData);
                    }
                }
              }
              resultsTable.Tables.Add(ReportDataList
                  .OrderBy(x=>x.ITEM_ID)
                  .ThenBy(x=>x.PUT_DATE)
                  .ThenBy(x=>x.CHARGE_DEPT_ID)
                  .ThenBy(x=>x.CHARGE_SECT_ID)
                  .ToList().ToDataTable());
              return resultsTable;
        }

        protected int GetQUANTITY(ITEM_IMPO data, List<ITEM_IMPO> getDatas)
        {
            return data.QUANTITY - getDatas.Where(x => x.ITEM_ID_FROM == data.ITEM_ID).Sum(x => x.QUANTITY);
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