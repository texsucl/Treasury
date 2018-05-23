//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Treasury.WebViewModels;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;

///// <summary>
///// 功能說明：付款傳票號碼回寫電子發票核銷系統
///// 初版作者：20180308 黃黛鈺
///// 修改歷程：20180308 黃黛鈺 
///// 需求單號：201801230413-00 
/////           初版
///// </summary>
///// 

//namespace Treasury.WebScheduler
//{
//    public class GLSISynJob : IJob
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        /// <summary>
//        /// 主程式段，查詢異常發票資料
//        /// </summary>
//        /// <param name="context"></param>
//        public void Execute(IJobExecutionContext context)
//        {
//            logger.Info("[Execute]執行開始!!");

//            List<string> dataList = new List<string>();
//            List<ticket> ticketList = new List<ticket>();

//            int invoCnt = 0;
//            int glsiCnt = 0;
//            int writeBatchCnt = 0;
//            int writeInvoCnt = 0;
//            int writeICanCnt = 0;

//            //查出有PAY單號，且核銷狀態為"核銷中"的資料
//            try
//            {
//                using (DbAccountEntities db = new DbAccountEntities())
//                {
//                    EInvoCancellationDao eInvoCancellationDao = new EInvoCancellationDao();
//                    dataList = eInvoCancellationDao.qryGISISynDetail(db);
//                }
//            }
//            catch (Exception e) {
//                logger.Error(e.ToString);
//            }


//            //查詢總帳系統，取得付款單號
//            if (dataList != null)
//            {
//                if (dataList.Count > 0)
//                {
//                    invoCnt = dataList.Count;

//                    vwACC02620901_2Dao glsiDao = new vwACC02620901_2Dao();

//                    using (GLSIACTEntities db = new GLSIACTEntities())
//                    {
//                        foreach (string d in dataList)
//                        {
//                            try {

//                                string ticketNo = StringUtil.toString(glsiDao.qryByPayNo(db, d));

//                                if (!"".Equals(ticketNo))
//                                {
//                                    glsiCnt++;
//                                    ticket ticket = new ticket();
//                                    ticket.payNo = d;
//                                    ticket.cticketNo = ticketNo;
//                                    ticketList.Add(ticket);
//                                }
//                            }
//                            catch (Exception e) {
//                                logger.Error(e.ToString());
//                            }
                            
//                        }
//                    }

                        
//                }
//                else
//                    logger.Info("[Execute]無需回寫付款單號的資料!!");
//            }
//            else
//                logger.Info("[Execute]無需回寫付款單號的資料!!");


//            //更新核銷檔、發票庫存檔
//            if (ticketList.Count > 0) {
//                EInvoCancellationDao eInvoCanDao = new EInvoCancellationDao();
//                EInvoStockDao eInvoStockDao = new EInvoStockDao();

//                string strConn = DbUtil.GetDBAccountConnStr();

//                foreach (ticket d in ticketList) {
//                    using (SqlConnection conn = new SqlConnection(strConn))
//                    {
//                        conn.Open();

//                        SqlTransaction transaction = conn.BeginTransaction("Transaction");
//                        try
//                        {
//                            writeInvoCnt += eInvoStockDao.updateTicketNo(d.payNo, d.cticketNo, conn, transaction);
//                            writeICanCnt += eInvoCanDao.updateTicketNo(d.payNo, d.cticketNo, conn, transaction);

//                            transaction.Commit();
//                            writeBatchCnt++;
//                        }
//                        catch (Exception e)
//                        {
//                            transaction.Rollback();

//                            logger.Error("payNo:" + d.payNo + "  cticketNo:" + d.cticketNo);
//                            logger.Error(e.ToString());
//                        }
//                    }
//                }
                

//             }



//            logger.Info("[Execute]本次應更新的核銷檔批數:" + invoCnt + "  總帳回寫批數:" + glsiCnt
//                + "  成功回寫批數:" + writeBatchCnt + "  成功回寫庫存檔筆數:" + writeInvoCnt + "  成功回寫核銷檔筆數:" + writeICanCnt);
//            logger.Info("[Execute]執行結束!!");
//        }


//        private class ticket
//        {
//            public String payNo { get; set; }

//            public String cticketNo { get; set; }

//        }
//    }
//}