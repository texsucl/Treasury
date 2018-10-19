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
    public class  DEPOSIT_DEP_ORDER_M : ReportDepositData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            //報表資料
            List<DepositReportDEPOSIT_DEP_ORDER_M_Data> ReportDataList = new List<DepositReportDEPOSIT_DEP_ORDER_M_Data>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportDEPOSIT_DEP_ORDER_M_Data();
            string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;//權責部門
            string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;//權責科別
            string JobProject = parms.Where(x =>x.key == "vJobProject" ).FirstOrDefault()?.value ?? string.Empty;//庫存表名稱
            string APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value ?? string.Empty; //庫存日期
            string APLY_ODT_From = parms.Where(x =>x.key == "APLY_ODT_From" ).FirstOrDefault()?.value ?? string.Empty;//入庫日期(起)
            string APLY_ODT_To = parms.Where(x =>x.key == "APLY_ODT_To" ).FirstOrDefault()?.value ?? string.Empty;//入庫日期(迄)
            string TRAD_Partners =  parms.Where(x =>x.key == "vTRAD_Partners" ).FirstOrDefault()?.value ?? string.Empty;
            var  IDOMdata = new List<ITEM_DEP_ORDER_M>();
            var  IDODdata = new List<ITEM_DEP_ORDER_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
               
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From);
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
                var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To);
                var _DEP_SET_QUALITY = db.ITEM_DEP_ORDER_M.AsNoTracking().Select(x => x.DEP_SET_QUALITY).ToList();
               

                var  _IDOM =db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.PUT_DATE >=_APLY_ODT_From && x.PUT_DATE <=_APLY_ODT_To  )
                    .Where(x => x.TRAD_PARTNERS ==TRAD_Partners)
                    .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                    .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                    //.Where(x=>x.DEP_SET_QUALITY == "Y")
                    //.Where(x => x.PUT_DATE<=_APLY_DT && _APLY_DT < x.TRANS_EXPIRY_DATE )
                    .ToList();
                    ;

                //foreach(var x in _IDOM)
                //{
                
                //}
                _IDOM.ForEach(x=>{
                        if(true &&_APLY_DT !=null )
                        {
                            if(x.DEP_SET_QUALITY == "Y" &&( x.PUT_DATE<=_APLY_DT && _APLY_DT < x.TRANS_EXPIRY_DATE) )
                            {
                                IDOMdata.Add(x);
                           
                            }
                            else if(x.DEP_SET_QUALITY == "N" &&(x.INVENTORY_STATUS == "1"&& _APLY_DT == dtn )||(x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE&&(_APLY_DT != dtn)))
                            {
                                IDOMdata.Add(x);
                            }
                        }

                });
                var _IDOMs = _IDOM.Select(x => x.ITEM_ID).ToList();
                var _ITEM_DEP_ORDER_D = db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => _IDOMs.Contains(x.ITEM_ID)).ToList();


               //   var  _IDOM_item = db.ITEM_DEP_ORDER_D.AsNoTracking()
               //     .Where(x =>_IDOM_ITEMID.Contains(x.ITEM_ID))
               //     .Select(x=> x.ITEM_ID).ToList();
               // if(_IDOM.Any() &&  _APLY_DT != null)
               // {
               //var _IDOMs =_IDOM .Where(x=> x.DEP_SET_QUALITY == "N")
               //      .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
               //      .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
               //      .ToList();
               //     IDOMdata = _IDOMs;
               // }
               // else
               // {
               //    IDOMdata =_IDOM;
               // }  
                var depts = new List<VW_OA_DEPT>();
                var types = new List<SYS_CODE>();
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }
                using (dbTreasuryEntities dbt = new dbTreasuryEntities())
                {
                         types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE != null).ToList();
                }

                   foreach(var Stockdata in IDOMdata) 
                    {
                                ReportData = new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                                {
                                    COMMIT_DATE = Stockdata.COMMIT_DATE,
                                    EXPIRY_DATE =Stockdata.EXPIRY_DATE,
                                    APLY_DEPT =getEmpName(depts,Stockdata.APLY_DEPT),
                                    APLY_SECT= getEmpName(depts,Stockdata.APLY_SECT),
                                    TRAD_PARTNERS =Stockdata.TRAD_PARTNERS,
                                    CURRENCY =Stockdata.CURRENCY,
                                    DEP_TYPE=getDEPName(types , Stockdata.DEP_TYPE),
                                    INTEREST_RATE = Stockdata.INTEREST_RATE,
                                    MEMO = Stockdata.MEMO,
                                    DENOMINATION = _ITEM_DEP_ORDER_D.FirstOrDefault(x => x.ITEM_ID == Stockdata.ITEM_ID)?.DENOMINATION.ToString(),
                                    DEP_NO_B = _ITEM_DEP_ORDER_D.FirstOrDefault(x =>x.ITEM_ID ==Stockdata.ITEM_ID)?.DEP_NO_B.ToString(),
                                    //DEP_NO_B = _ITEM_DEP_ORDER_D.FirstOrDefault(x =>x.ITEM_ID ==Stockdata.ITEM_ID)?.DEP_NO_B.ToString(),
                                    DEP_NO_E = _ITEM_DEP_ORDER_D.FirstOrDefault(x =>x.ITEM_ID ==Stockdata.ITEM_ID)?.DEP_NO_E.ToString(),
                                };
                                    ReportDataList.Add(ReportData);
                    }
              }
              resultsTable.Tables.Add(ReportDataList.ToDataTable());
              return resultsTable;
        }
        protected ITEM_DEP_ORDER_M IDOM { get; private set; }
        protected void SetDetail(string aply_No)
        {
            var depts = new List<VW_OA_DEPT>();
            var types = new List<SYS_CODE>();
            IDOM = new ITEM_DEP_ORDER_M();

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
        /// 使用 存單類型 獲得 存單類型名稱
        /// </summary>
        /// <param name="types"></param>
        /// <param name="DEP_TYPE"></param>
        /// <returns></returns>
        private string getDEPName(List<SYS_CODE> types, string CODE)
        {
           if (!CODE.IsNullOrWhiteSpace() && types.Any())
                return types.FirstOrDefault(x => x.CODE.Trim() == CODE.Trim() && x.CODE_TYPE == "DEP_TYPE")?.CODE_VALUE?.Trim();
            return string.Empty;
        }
    }
}