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
            var _datas = new List<ITEM_SEAL>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From);
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
              var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To);
              

                int TOTAL= 0;
               var _IS=  db.ITEM_SEAL.AsNoTracking()//判斷是否在庫
                     .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
                     .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
                     .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                     .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                     .Where(x=> x.PUT_DATE >= _APLY_ODT_From ,_APLY_ODT_From != null  )
                     .Where(x=> x.PUT_DATE <= _APLY_ODT_To ,   _APLY_ODT_To != null  )
                     .Where(x=>x.TREA_ITEM_NAME ==JobProject).ToList()
                     ;

               // if( _IS.Any() &&  ( _APLY_ODT_From != null ||_APLY_ODT_To != null)){
               //       var _IS_Data = db.ITEM_SEAL.AsNoTracking()
               //     .Where(x => x.PUT_DATE<=_APLY_DT && _APLY_DT <x.GET_DATE )
               //     .Where(x=>x.TREA_ITEM_NAME ==JobProject).ToList();                  
               //     _datas = _IS_Data;
                
               //}
               // else
               // {
               // _datas = _IS;
               // }
                _datas = _IS;
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
                    foreach(var Sealdata in _datas.OrderBy(x=>x.PUT_DATE).ThenBy(x=>x.CHARGE_DEPT).ThenBy(x=>x.CHARGE_SECT).ThenBy(x=>x.SEAL_DESC)) 
                    {
                                TOTAL ++;
                                ReportData = new DepositReportSealData()
                                {
                                    ROW = TOTAL,   
                                    PUT_DATE = Sealdata.PUT_DATE,
                                    CHARGE_DEPT =getEmpName(depts,Sealdata.CHARGE_DEPT),
                                    CHARGE_SECT= getEmpName(depts,Sealdata.CHARGE_SECT),
                                    SEAL_DESC = Sealdata.SEAL_DESC,
                                    MEMO =Sealdata.MEMO,
                                };
                            
                        ReportDataList.Add(ReportData);
                    }
              }
              resultsTable.Tables.Add(ReportDataList.ToDataTable());
              return resultsTable;
        }
        protected ITEM_SEAL IRD { get; private set; }
        
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