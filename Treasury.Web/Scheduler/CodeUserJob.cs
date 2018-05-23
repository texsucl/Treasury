//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;

///// <summary>
///// 功能說明：同步使用者資料
///// 初版作者：20170926 黃黛鈺
///// 修改歷程：20170926 黃黛鈺 
/////           需求單號：201707240447-01 
/////           初版
///// </summary>
///// 
//namespace Treasury.WebScheduler
//{
//    public class CodeUserJob : IJob
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


//        public void Execute(IJobExecutionContext context)
//        {
//            logger.Info("[Execute]執行開始!!");


//            //從【CodeUser使用者資料檔】中查詢所有使用者
//            CodeUserDao codeUserDao = new CodeUserDao();
//            List<CODEUSER> userList = codeUserDao.qryValidUser();
//            logger.Info("[Execute]本次作業需異動的筆數：" + userList.Count);


//            //將每個user的資料自SSSDB同步回AWTC
//            string strConn = DbUtil.GetDBAccountConnStr();
//            using (SqlConnection conn = new SqlConnection(strConn))
//            {
//                conn.Open();

//                foreach (CODEUSER user in userList)
//                {
//                    AgentInfoDao agentInfoDao = new AgentInfoDao();
//                    CODEUSER userN = agentInfoDao.qryAgentUnit(user.CAGENTID.Trim());
                    

//                    if (userN == null)
//                        logger.Info("[Execute]該使用者不存在SSSDB，未更新： " + user.CAGENTID.Trim());
//                    else {
//                        try
//                        {
//                            if (user.CUSERNAME.Trim().Equals(userN.CUSERNAME.Trim())
//                                && user.CBELONGUNITCODE.Trim().Equals(userN.CBELONGUNITCODE.Trim())
//                                && user.CBELONGUNITSEQ.Trim().Equals(userN.CBELONGUNITSEQ.Trim())
//                                && user.CBELONGUNITNAME.Trim().Equals(userN.CBELONGUNITNAME.Trim())
//                                && user.CWORKUNITCODE.Trim().Equals(userN.CWORKUNITCODE.Trim())
//                                && user.CWORKUNITSEQ.Trim().Equals(userN.CWORKUNITSEQ.Trim())
//                                && user.CWORKUNITNAME.Trim().Equals(userN.CWORKUNITNAME.Trim()))
//                                logger.Info("[Execute]使用者資料未變動，未更新： " + user.CAGENTID.Trim());
//                            else {
//                                userN.CUPDUSERID = "";
//                                userN.CUPDUSERNAME = "SYS";
//                                codeUserDao.updateUnit(userN, conn);

//                                user.CUSERNAME = userN.CUSERNAME.Trim();
//                                user.CBELONGUNITCODE = userN.CBELONGUNITCODE.Trim();
//                                user.CBELONGUNITSEQ = userN.CBELONGUNITSEQ.Trim();
//                                user.CBELONGUNITNAME = userN.CBELONGUNITNAME.Trim();
//                                user.CWORKUNITCODE = userN.CWORKUNITCODE.Trim();
//                                user.CWORKUNITSEQ = userN.CWORKUNITSEQ.Trim();
//                                user.CWORKUNITNAME = userN.CWORKUNITNAME.Trim();


//                                //新增LOG
//                                Log log = new Log();
//                                log.CFUNCTION = "使用者同步作業";
//                                log.CACTION = "A";
//                                log.CCONTENT = codeUserDao.userLogContent(user);
//                                LogDao.Insert(log, "SYS");


//                                //新增稽核軌跡
//                                PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
//                                piaLog.TRACKING_TYPE = "A";
//                                piaLog.ACCESS_ACCOUNT = "SYS";
//                                piaLog.ACCOUNT_NAME = "SYS";
//                                piaLog.FROM_IP = "";
//                                piaLog.PROGFUN_NAME = "CodeUserJob";
//                                piaLog.ACCESSOBJ_NAME = "CodeUser";
//                                piaLog.EXECUTION_TYPE = "E";
//                                piaLog.EXECUTION_CONTENT = codeUserDao.userLogContent(user);
//                                piaLog.AFFECT_ROWS = 1;
//                                piaLog.PIA_OWNER1 = user.CAGENTID;
//                                piaLog.PIA_OWNER2 = "";
//                                piaLog.PIA_TYPE = "0100000000";



//                                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
//                                piaLogMainDao.Insert(piaLog);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            logger.Error("[Execute]更新CODEUSER異常，未更新： " + user.CAGENTID.Trim());
//                            logger.Error( e.ToString());
//                        }

//                    }
//                }
//            }


//            logger.Info("[Execute]執行結束!!");
//        }
//    }
//}