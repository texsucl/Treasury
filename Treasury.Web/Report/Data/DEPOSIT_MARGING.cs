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
    public class DEPOSIT_MARGING : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportMarginpgData> ReportDataList = new List<DepositReportMarginpgData>();
            var resultsTable = new DataSet();
             var ReportData = new DepositReportMarginpgData();
             string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;
             string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;
             string JobProject = parms.Where(x =>x.key == "vJobProject" ).FirstOrDefault()?.value ?? string.Empty;
            string APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value ?? string.Empty; //庫存日期
            string APLY_ODT_From = parms.Where(x =>x.key == "APLY_ODT_From" ).FirstOrDefault()?.value ?? string.Empty;
            string APLY_ODT_To = parms.Where(x =>x.key == "APLY_ODT_To" ).FirstOrDefault()?.value ?? string.Empty;
            var _datas = new List<ITEM_REFUNDABLE_DEP>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From);
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
                var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To);
                
                    var _IRD=  db.ITEM_REFUNDABLE_DEP.AsNoTracking()//判斷是否在庫
                     .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
                     .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
                     .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                     .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                    .Where(x=> x.PUT_DATE >= _APLY_ODT_From ,_APLY_ODT_From != null  )
                    .Where(x=> x.PUT_DATE <= _APLY_ODT_To ,   _APLY_ODT_To != null  )
                    .ToList()
                     ;
                _datas = _IRD;
                  var depts = new List<VW_OA_DEPT>();
                  var types = new List<SYS_CODE>(); 
                   using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                    {
                       depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                    }
                    using (dbTreasuryEntities dbt= new dbTreasuryEntities())
                    {
                       types = dbt.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();

                    }
                   foreach(var Stockdata in _datas.OrderBy(x => x.MARGIN_DEP_TYPE).ThenBy(x => x.ITEM_ID).ThenBy(x => x.BOOK_NO).ThenBy(x => x.PUT_DATE).ThenBy(x => x.CHARGE_DEPT).ThenBy(x => x.CHARGE_SECT)) 
                    {
                                ReportData = new DepositReportMarginpgData()   
                                {
                                    MARGIN_DEP_TYPE = getMDTtype( types,Stockdata.MARGIN_DEP_TYPE),
                                    ITEM_ID =Stockdata.ITEM_ID,
                                    BOOK_NO =Stockdata.BOOK_NO,
                                    PUT_DATE =Stockdata.PUT_DATE,
                                    CHARGE_DEPT =getEmpName(depts,Stockdata.CHARGE_DEPT),
                                    CHARGE_SECT= getEmpName(depts,Stockdata.CHARGE_SECT),
                                    TRAD_PARTNERS = Stockdata.TRAD_PARTNERS,
                                    AMOUNT = Stockdata.AMOUNT,
                                    WORKPLACE_CODE =Stockdata.WORKPLACE_CODE,
                                    DESCRIPTION =Stockdata.DESCRIPTION,
                                    MEMO =Stockdata.MEMO,
                                };
                                ReportDataList.Add(ReportData);
                          }
              }
              resultsTable.Tables.Add(ReportDataList.ToDataTable());
              return resultsTable;
        }
        protected ITEM_REFUNDABLE_DEP IRD { get; private set; }
        protected void SetDetail(string aply_No)
        {
            var depts = new List<VW_OA_DEPT>();
            var types = new List<SYS_CODE>();
            IRD = new ITEM_REFUNDABLE_DEP();

            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
            }
            using (dbTreasuryEntities db = new dbTreasuryEntities())
            {
                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE != null).ToList();
            }

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
        /// 使用 類別ID 獲得 類別名稱
        /// </summary>
        /// <param name="MARGIN_DEP_TYPE"></param>
        /// <param name="CODE_VALUE"></param>
        /// <returns></returns>
        private string getMDTtype(List<SYS_CODE> types, string CODE)
        {
            if (!CODE.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.CODE.Trim() == CODE.Trim() && x.CODE_TYPE == "MARGING_TYPE")?.CODE_VALUE?.Trim();
            return string.Empty;
        }

    }
}