﻿using System;
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
          
            var _datas = new List<ITEM_STOCK>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From);
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
               var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To);

                 var _IS = db.ITEM_STOCK.AsNoTracking().ToList();

                if(BOOK_NO=="All"){//判斷為全部或單一
                     _IS=  db.ITEM_STOCK.AsNoTracking()//判斷是否在庫
                     .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
                     .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
                     .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                     .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                     .Where(x=> x.PUT_DATE >= _APLY_ODT_From ,_APLY_ODT_From != null  )
                     .Where(x=> x.PUT_DATE <= _APLY_ODT_To ,   _APLY_ODT_To != null  ).ToList();
                }
                else{
                      _IS=  db.ITEM_STOCK.AsNoTracking()//判斷是否在庫
                     .Where(x=> x.INVENTORY_STATUS == "1" ,_APLY_DT == dtn )
                     .Where(x=> x.PUT_DATE <=_APLY_DT  && _APLY_DT < x.GET_DATE,_APLY_DT != dtn )
                     .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                     .Where(x => x.CHARGE_SECT == vsect ,  vsect !="All")
                     .Where(x=> x.GROUP_NO.ToString()==BOOK_NO).ToList();
                }

                //if( _IS.Any() &&  ( _APLY_ODT_From != null ||_APLY_ODT_To != null)){
                //    var _IS_Data = db.ITEM_STOCK.AsNoTracking()
                //    .Where(x => x.PUT_DATE<=_APLY_DT && _APLY_DT <x.GET_DATE )
                //     .Where(x=> x.GROUP_NO.ToString()==BOOK_NO).ToList();                  
                //    _datas = _IS_Data;
                //}
                //else
                //{
                //_datas = _IS;
                //}

                _datas = _IS;
                  var depts = new List<VW_OA_DEPT>();
                  var types = new List<SYS_CODE>(); 
                  var book = new List<ITEM_BOOK>(); 
                   using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                    {
                       depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                    }
                    using (dbTreasuryEntities dbt= new dbTreasuryEntities())
                    {
                       types = dbt.SYS_CODE.AsNoTracking().Where(x => x.CODE !=null).ToList();

                    }
                   using (dbTreasuryEntities dbt= new dbTreasuryEntities())
                    {
                       book = dbt.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO.ToString() !=null).ToList();
                    }
                    foreach(var STOCKdata in _datas.OrderBy(x=>x.PUT_DATE).ThenBy(x=>x.CHARGE_DEPT).ThenBy(x=>x.CHARGE_SECT).ThenBy(x=>x.GROUP_NO).ThenBy(x=>x.STOCK_NO_PREAMBLE).ThenBy(x=>x.STOCK_NO_B)) 
                    {
                                ReportData = new DepositReportSTOCKData()
                                {
                                    PUT_DATE = STOCKdata.PUT_DATE,
                                    STOCK_NO_B = STOCKdata.STOCK_NO_B,
                                    STOCK_NO_E = STOCKdata.STOCK_NO_E,
                                    STOCK_CNT = STOCKdata.STOCK_CNT,
                                    DENOMINATION = STOCKdata.DENOMINATION,
                                    NUMBER_OF_SHARES = STOCKdata.NUMBER_OF_SHARES,
                                    AREA = getArea(book,STOCKdata.GROUP_NO.ToString()),
                                    CHARGE_DEPT =getEmpName(depts,STOCKdata.CHARGE_DEPT),
                                    CHARGE_SECT= getEmpName(depts,STOCKdata.CHARGE_SECT),
                                    MEMO =STOCKdata.MEMO,
                                    BOOK_NO = STOCKdata.GROUP_NO.ToString(),
                                    NAME = getName(book,STOCKdata.GROUP_NO.ToString()),
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