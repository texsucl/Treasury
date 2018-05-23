//using Treasury.WebBO;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebViewModels;
//using ClosedXML.Excel;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Web;

///// <summary>
///// 功能說明：電子發票建立異常檢核報表
///// 初版作者：20180212 黃黛鈺
///// 修改歷程：20180212 黃黛鈺 
///// 需求單號：201801230413-00 
/////           初版
///// </summary>
///// 

//namespace Treasury.WebScheduler
//{
//    public class EInvoErrRptJob : IJob
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


//        /// <summary>
//        /// 主程式段，查詢異常發票資料
//        /// </summary>
//        /// <param name="context"></param>
//        public void Execute(IJobExecutionContext context)
//        {
//            logger.Info("[Execute]執行開始!!");

//            List<EInvoQryModel> errList = new List<EInvoQryModel>();
//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                EInvoStockDao eInvoStockDao = new EInvoStockDao();
//                errList = eInvoStockDao.qryManualErrRpt(7, db);

//                if (errList.Count > 0) 
//                    genRtp(errList);
//                else
//                    logger.Info("[Execute]無異常資料!!");

//            }

//                logger.Info("[Execute]執行結束!!");
//        }


//        /// <summary>
//        /// 處理異常資料產製報表
//        /// </summary>
//        /// <param name="errList"></param>
//        private void genRtp(List<EInvoQryModel> errList) {
//            string smtpServer = System.Configuration.ConfigurationManager.AppSettings.Get("smtpServer");
//            int smtpPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("smtpPort"));
//            string smtpSender = System.Configuration.ConfigurationManager.AppSettings.Get("smtpSender") ;

//            //取得本次需寄送的人員清單
//            List<EInvoQryModel> userList = errList.GroupBy(o => new { o.crtUid })
//                .Select(group => new EInvoQryModel
//                {
//                    crtUid = group.Key.crtUid
//                }).ToList<EInvoQryModel>();


//           // String guid = Guid.NewGuid().ToString();


//            OaEmpDao oaEmpDao = new OaEmpDao();
//            foreach (EInvoQryModel d in userList) {
//                VW_OA_EMP oaEmp = oaEmpDao.qryByUsrId(d.crtUid);

//                if (oaEmp == null)
//                    logger.Error("查無員工對應資訊:" + d.crtUid);
//                else {
//                    List<EInvoQryModel> rptData = errList.Where(x => x.crtUid == d.crtUid).ToList();

//                    try
//                    {
//                        CommonUtil commonUtil = new CommonUtil();
//                        DataTable dt1 = commonUtil.ConvertToDataTable(rptData.Where(x => x.rptType == "1").ToList());
//                        DataTable dt2 = commonUtil.ConvertToDataTable(rptData.Where(x => x.rptType == "2").ToList());
//                        dt1 = getHeader(dt1);
//                        dt2 = getHeader(dt2);


//                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
//                            , string.Concat("電子發票建立異常檢核報表" + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + d.crtUid, ".xlsx"));

//                        using (XLWorkbook wb = new XLWorkbook())
//                        {
//                            wb.Worksheets.Add(dt1, "人工開立異常檢核報表");
//                            wb.Worksheets.Add(dt2, "作廢或折讓異常檢核報表");

//                            //if (dt != null)
//                            //    wb.Worksheets.Add(dt.Select(), "人工開立異常檢核報表");

//                            var sheet = wb.Worksheets.Add("查詢條件");
//                            sheet.Cell(1, 1).Value = "人工開立異常檢核報表";
//                            sheet.Cell(2, 1).Value = "建立日期 < " + DateTime.Now.AddDays(-7).ToString("yyyy/MM/dd");
//                            sheet.Cell(3, 1).Value = "發票原始狀態：空白(人工開立)";

//                            sheet.Cell(5, 1).Value = "作廢或折讓異常檢核報表";
//                            sheet.Cell(6, 1).Value = "建立日期 = " + DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");

//                            wb.SaveAs(fullPath);

//                            sendMail(oaEmp, dt1.Rows.Count, dt2.Rows.Count, fullPath, smtpSender, smtpServer, smtpPort);
//                            logger.Info("產製報表成功，員工:" + d.crtUid
//                                + "、人工開立異常檢核報表:" + dt1.Rows.Count 
//                                + "筆、作廢或折讓異常檢核報表:" + dt2.Rows.Count
//                                + "筆");
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Error("產出異常報表錯誤，員工帳號：" + d.crtUid);
//                        logger.Error("其它錯誤：" + e.ToString());
//                    }
//                }
//            }
//        }



//        /// <summary>
//        /// 將異常報表寄給user
//        /// </summary>
//        /// <param name="oaEmp"></param>
//        /// <param name="err1Cnt"></param>
//        /// <param name="err2Cnt"></param>
//        /// <param name="fullPath"></param>
//        /// <param name="smtpSender"></param>
//        /// <param name="smtpServer"></param>
//        /// <param name="smtpPort"></param>
//        private void sendMail(VW_OA_EMP oaEmp, int err1Cnt, int err2Cnt, string fullPath,string smtpSender, string smtpServer, int smtpPort) {
//            MailUtil mailUtil = new MailUtil();

//            mailUtil.sendMail(smtpSender
//                , new string[] { oaEmp.EmailAddress.Trim() }
//                , "電子發票建立異常檢核報表"
//                , "人工開立異常檢核報表: " + err1Cnt + "筆、作廢或折讓異常檢核報表:" + err2Cnt + "筆"
//                , true
//                , smtpServer
//                , smtpPort
//                , ""
//                ,""
//                , new string[] { fullPath }
//                , true);

//        }

//        /// <summary>
//        /// 處理產出的excel表標題
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        private DataTable getHeader(DataTable dt) {
//            dt.Columns.Remove("tempId");
//            dt.Columns.Remove("invoOSts");
//            dt.Columns.Remove("cancellationSts");
//            dt.Columns.Remove("rptType");

//            //dt.Columns["tempId"].ColumnName = "項次";
//            dt.Columns["invoTrackNo"].ColumnName = "發票號碼";
//            dt.Columns["dataBlYm"].ColumnName = "發票年月";
//            dt.Columns["invoDate"].ColumnName = "發票日期";
//            dt.Columns["saleAmt"].ColumnName = "銷售額";
//            dt.Columns["formatCd"].ColumnName = "格式代號";
//            dt.Columns["taxType"].ColumnName = "課稅別";
//            dt.Columns["invoOStsDesc"].ColumnName = "發票原始狀態";
//            dt.Columns["crtUid"].ColumnName = "建立人員";
//            dt.Columns["crtDateTime"].ColumnName = "建立日期";
//            dt.Columns["cancellationStsDesc"].ColumnName = "核銷狀態";
//            dt.Columns["cancellationDate"].ColumnName = "核銷日期";
//            dt.Columns["cancellationUid"].ColumnName = "核銷經辦";
//            dt.Columns["batchNo"].ColumnName = "核銷批號";
//            dt.Columns["payNo"].ColumnName = "付款單號";
//            dt.Columns["cticketNo"].ColumnName = "傳票號碼";

//            return dt;
//        }
//    }
//}