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
            List<DepositReportDEPOSIT_DEP_ORDER_M_Data> ReportDataList2 = new List<DepositReportDEPOSIT_DEP_ORDER_M_Data>();
            var resultsTable = new DataSet();
            var ReportData = new DepositReportDEPOSIT_DEP_ORDER_M_Data();
            string vdept = parms.Where(x =>x.key == "vdept" ).FirstOrDefault()?.value ?? string.Empty;//權責部門
            string vsect = parms.Where(x =>x.key == "vsect" ).FirstOrDefault()?.value ?? string.Empty;//權責科別
            string JobProject = parms.Where(x =>x.key == "vJobProject" ).FirstOrDefault()?.value ?? string.Empty;//庫存表名稱
            string APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value ?? string.Empty; //庫存日期
            string APLY_ODT_From = parms.Where(x =>x.key == "APLY_ODT_From" ).FirstOrDefault()?.value ?? string.Empty;//入庫日期(起)
            string APLY_ODT_To = parms.Where(x =>x.key == "APLY_ODT_To" ).FirstOrDefault()?.value ?? string.Empty;//入庫日期(迄)
            string TRAD_Partners =  parms.Where(x =>x.key == "vTRAD_Partners" ).FirstOrDefault()?.value ?? string.Empty;//交易對象

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _APLY_DT = TypeTransfer.stringToDateTime(APLY_DT_From).DateToLatestTime();
                var _APLY_DT_Date = _APLY_DT.Date;
                var dtn = DateTime.Now.Date;
                var _APLY_ODT_From = TypeTransfer.stringToDateTimeN(APLY_ODT_From);
                var _APLY_ODT_To = TypeTransfer.stringToDateTimeN(APLY_ODT_To).DateToLatestTime();
                var _DEP_SET_QUALITY = db.ITEM_DEP_ORDER_M.AsNoTracking().Select(x => x.DEP_SET_QUALITY).ToList();

                List<ITEM_DEP_ORDER_M> _IDOM = new List<ITEM_DEP_ORDER_M>();

                var  _IDOM_N =db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS), _APLY_DT_Date == dtn)
                    .Where(x =>
                    (INVENTORY_STATUSs.Contains(x.INVENTORY_STATUS) && x.PUT_DATE <= _APLY_DT) // 在庫 且 存入日期 <= 庫存日期 
                    ||
                    (x.INVENTORY_STATUS == INVENTORY_STATUSg &&
                     x.PUT_DATE <= _APLY_DT && 
                     _APLY_DT < x.GET_DATE),  //存入日期 <= 庫存日期 且 庫存日期 < 取出日期
                    _APLY_DT_Date != dtn)
                    .Where(x => x.TRAD_PARTNERS ==TRAD_Partners, TRAD_Partners != "All")
                    .Where(x => x.CHARGE_DEPT == vdept , vdept != "All")
                    .Where(x => x.CHARGE_SECT == vsect , vsect !="All")
                    .Where(x => x.PUT_DATE >= _APLY_ODT_From, _APLY_ODT_From != null)
                    .Where(x => x.PUT_DATE <= _APLY_ODT_To, _APLY_ODT_To != null)
                    .Where(x=>x.DEP_SET_QUALITY == "N") //設質否等於N
                    .ToList();

                _IDOM.AddRange(_IDOM_N);

                var _IDOM_Y = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.PUT_DATE <= _APLY_DT && _APLY_DT < x.TRANS_EXPIRY_DATE)  
                     //存入日期 <= 庫存日期 且 庫存日期 < 轉期後到期日
                    .Where(x => x.TRAD_PARTNERS == TRAD_Partners, TRAD_Partners != "All")
                    .Where(x => x.CHARGE_DEPT == vdept, vdept != "All")
                    .Where(x => x.CHARGE_SECT == vsect, vsect != "All")
                    .Where(x => x.PUT_DATE >= _APLY_ODT_From, _APLY_ODT_From != null)
                    .Where(x => x.PUT_DATE <= _APLY_ODT_To, _APLY_ODT_To != null)
                    .Where(x => x.DEP_SET_QUALITY == "Y") //設質否等於Y
                    .ToList();

                _IDOM.AddRange(_IDOM_Y);

                var _IDOM_ItemIDs = _IDOM.Select(x => x.ITEM_ID).ToList();
                var _ITEM_DEP_ORDER_D = db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => _IDOM_ItemIDs.Contains(x.ITEM_ID)).ToList();
 
                var depts = new List<VW_OA_DEPT>();
                var types = new List<SYS_CODE>();
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                }

                types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE != null).ToList();
                
                

                foreach(var Stockdata in _IDOM
                    //.OrderByDescending(x => x.CURRENCY != "NTD")
                    //.ThenBy(x => x.CURRENCY)
                    //.ThenBy(x => x.DEP_TYPE)
                    //.ThenByDescending(x => x.COMMIT_DATE)
                    //.ThenBy(x => x.DEP_SET_QUALITY)
                    ) 
                {
                    ReportDataList.Add(new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                    {
                        ITEMID = Stockdata.ITEM_ID,
                        COMMIT_DATE = Stockdata.COMMIT_DATE.dateTimeToStr(),
                        EXPIRY_DATE = Stockdata.EXPIRY_DATE.dateTimeToStr(),
                        TRAD_PARTNERS = Stockdata.TRAD_PARTNERS,
                        CURRENCY = Stockdata.CURRENCY,
                        CURRENCY_Flag = Stockdata.CURRENCY == "NTD" ? "台幣" : "外幣",
                        DEP_TYPE = getDEPName(types, Stockdata.DEP_TYPE),
                        INTEREST_RATE = Stockdata.INTEREST_RATE,
                        MEMO = Stockdata.MEMO,
                        CHARGE_DEPT_ID = Stockdata.CHARGE_DEPT,
                        CHARGE_SECT_ID = Stockdata.CHARGE_SECT,
                        DEP_SET_QUALITY = Stockdata.DEP_SET_QUALITY,
                    });
                }

                //群組 : 類別 & 交易類型 群組
                foreach (var group in ReportDataList.GroupBy(x => new { x.CURRENCY_Flag, x.DEP_TYPE }))
                {
                    Dictionary<string, decimal> _data = new Dictionary<string, decimal>();

                    //排序 : 承作日期(降冪)+設質否(按N、Y順序)
                    foreach (var item in group.OrderByDescending(x => x.CURRENCY != "NTD").ThenByDescending(x => x.COMMIT_DATE).ThenBy(x => x.DEP_SET_QUALITY))
                    {

                        decimal total_DENOMINATION = 0M;

                        //明細
                        foreach (var i in _ITEM_DEP_ORDER_D.Where(x => x.ITEM_ID == item.ITEMID).OrderBy(x => x.DATA_SEQ))
                        {
                            ReportData = new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                            {
                                TYPE = "1",
                                ITEMID = item.ITEMID,
                                COMMIT_DATE = item.COMMIT_DATE,
                                EXPIRY_DATE = item.EXPIRY_DATE,
                                TRAD_PARTNERS = item.TRAD_PARTNERS,
                                CURRENCY = item.CURRENCY,
                                CURRENCY_Flag = item.CURRENCY_Flag,
                                DEP_TYPE = item.DEP_TYPE,
                                INTEREST_RATE = item.INTEREST_RATE,
                                DEP_NO_B = i.DEP_NO_B,
                                DEP_NO_E = i.DEP_NO_E,
                                DEP_CNT = i.DEP_CNT,
                                DENOMINATION = i.DENOMINATION,
                                SUBTOTAL_DENOMINATION = i.SUBTOTAL_DENOMINATION,
                                SUMTOTAL_DENOMINATION = 0,
                                MEMO = item.MEMO,
                                CHARGE_DEPT_ID = item.CHARGE_DEPT,
                                CHARGE_SECT_ID = item.CHARGE_SECT,
                                DEP_SET_QUALITY = item.DEP_SET_QUALITY,
                            };
                            total_DENOMINATION += ReportData.SUBTOTAL_DENOMINATION;
                            ReportDataList2.Add(ReportData);
                        }

                        ReportDataList2.Add(new DepositReportDEPOSIT_DEP_ORDER_M_Data() {
                            TYPE = "1",
                            ITEMID = item.ITEMID,
                            COMMIT_DATE = item.COMMIT_DATE,
                            EXPIRY_DATE = item.EXPIRY_DATE,
                            TRAD_PARTNERS = item.TRAD_PARTNERS,
                            CURRENCY = item.CURRENCY,
                            CURRENCY_Flag = item.CURRENCY_Flag,
                            DEP_TYPE = item.DEP_TYPE,
                            INTEREST_RATE = item.INTEREST_RATE,
                            DEP_CNT = 0,
                            DENOMINATION = 0,
                            SUBTOTAL_DENOMINATION = 0,
                            SUMTOTAL_DENOMINATION = total_DENOMINATION,
                            DEP_SET_QUALITY = item.DEP_SET_QUALITY
                        });

                        string key = $@"{item.CURRENCY},{item.DEP_SET_QUALITY}"; //幣別,設質否
                        decimal value = 0M;
                        if (_data.TryGetValue(key, out value))
                        {
                            value += total_DENOMINATION;
                            _data[key] = value;
                        }
                        else
                        {
                            _data.Add(key, total_DENOMINATION);
                        }
                    }

                    ReportDataList2.Add(new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                    {
                        TYPE = "2",
                        SUMTOTAL_DENOMINATION = _data.Sum(x=>x.Value),
                        CURRENCY_Flag = group.Key.CURRENCY_Flag,
                        DEP_TYPE = group.Key.DEP_TYPE,
                    });

                    ReportDataList2.Add(new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                    {
                        TYPE = "3",
                        COMMIT_DATE = "幣別",
                        EXPIRY_DATE = "設質否",
                        TRAD_PARTNERS = "面額合計",
                        CURRENCY_Flag = group.Key.CURRENCY_Flag,
                        DEP_TYPE = group.Key.DEP_TYPE,
                    });

                    foreach (var d in _data.OrderBy(x=>x.Key))
                    {
                        ReportDataList2.Add(new DepositReportDEPOSIT_DEP_ORDER_M_Data()
                        {
                            TYPE = "3",
                            COMMIT_DATE = d.Key.Split(',')[0],
                            EXPIRY_DATE = d.Key.Split(',')[1],
                            TRAD_PARTNERS = d.Value.ToString().formateThousand(),
                            CURRENCY_Flag = group.Key.CURRENCY_Flag,
                            DEP_TYPE = group.Key.DEP_TYPE,
                        });
                    }
                }
            }
       
            resultsTable.Tables.Add(ReportDataList2.ToDataTable());
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