using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Controllers;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class BILL : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {  
            var resultsTable = new DataSet();

            List<BillReportModel> reportDatas = new List<BillReportModel>();

            var _Parameters = new List<SqlParameter>(); 
            string aply_No = parms.Where(x => x.key == "aply_No").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No);

            var Bill = new Bill();
            var billController = new BillController();

            var _data =  (List<BillViewModel>)Bill.GetTempData(aply_No);
            var _data2 = (List<BillViewModel>)Bill.GetDayData(null, null, aply_No);

            var tempData = billController.SetBillViewRowNum(_data);
            var dayData = billController.SetBillTakeOutViewModelGroup(_data2);

            var checkTypes = new List<SYS_CODE>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                checkTypes = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "CHECK_TYPE").ToList();
            }
          
            reportDatas.AddRange(tempData.Select(x => new BillReportModel()
            {
                ROW = x.vRowNum,
                TYPE = "1",
                ISSUING_BANK = x.vIssuingBank,
                CHECK_TYPE = checkTypes.FirstOrDefault(y=>y.CODE == x.vCheckType)?.CODE_VALUE ?? x.vCheckType,
                CHECK_NO_TRACK = x.vCheckNoTrack,
                CHECK_NO_B = x.vCheckNoB,
                CHECK_NO_E = x.vCheckNoE,
                Total = TypeTransfer.stringToInt(x.vCheckTotalNum) == 0 ? TypeTransfer.stringToInt(x.vTakeOutTotalNum) : TypeTransfer.stringToInt(x.vCheckTotalNum),
                ReMainTotalNum = TypeTransfer.stringToInt(x.vReMainTotalNum)
            }));
            reportDatas.AddRange(dayData.Select(x => new BillReportModel()
            {
                ROW = x.vRowNum,
                TYPE = "2",
                Status = x.vStatus,
                ISSUING_BANK = x.vIssuingBank,
                CHECK_TYPE = checkTypes.FirstOrDefault(y => y.CODE == x.vCheckType)?.CODE_VALUE ?? x.vCheckType,
                CHECK_NO_TRACK = x.vCheckNoTrack,
                CHECK_NO_B = x.vCheckNoB,
                CHECK_NO_E = x.vCheckNoE,
                Total = TypeTransfer.stringToInt(x.vCheckTotalNum),
                ReMainTotalNum = TypeTransfer.stringToInt(x.vReMainTotalNum)
            }));

            resultsTable.Tables.Add(reportDatas.ToDataTable());

            SetExtensionParm();
            return resultsTable;
        }

        public class BillReportModel {

            [Description("項次")]
            public string ROW { get; set; }

            [Description("顯示類別")]
            public string TYPE { get; set; }

            [Description("狀態")]
            public string Status {get;set;}

            [Description("發票行庫")]
            public string ISSUING_BANK { get; set; }

            [Description("類別")]
            public string CHECK_TYPE { get; set; }

            [Description("字軌")]
            public string CHECK_NO_TRACK { get; set; }

            [Description("號碼(起)")]
            public string CHECK_NO_B { get; set;}

            [Description("號碼(迄)")]
            public string CHECK_NO_E { get; set; }

            [Description("總張數")]
            public int Total { get; set; }

            [Description("剩餘總張數")]
            public int ReMainTotalNum { get; set; }
        }
    }
}