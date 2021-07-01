using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Web.Mvc;
using FRT.Web.AS400Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FRT.Web.Models;
using System.Linq;

/// <summary>
/// 功能說明：重新發送電文覆核作業
/// 初版作者：20180830 Daiyu
/// 修改歷程：20180830 Daiyu
///           需求單號：201807190487
///           初版
/// 修改歷程：20190514 Mark
///           需求單號：201904100470
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB009AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB009A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            return View();

        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="paidId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string fastNo)
        {
            FRTBARHDao fRTBARHDao = new FRTBARHDao();

            try {
                List<ORTB009Model> rows = fRTBARHDao.qryForORTB009(fastNo);

                //確認目前該筆快速付款資料的狀態是否可以重送
                if (rows.Count > 0) {
                    FRTBARMDao fRTBARMDao = new FRTBARMDao();
                    ORTB009Model oRTB009Model = fRTBARMDao.qryForORTB009(fastNo);
                    if ("".Equals(oRTB009Model.fastNo))
                        return Json(new { success = true, rows, err = "該筆快速付款的資料狀態已變動，不可執行重送，請執行【駁回】!!" }, JsonRequestBehavior.AllowGet);
                    else if("1".Equals(StringUtil.toString(oRTB009Model.remitStat)))
                        return Json(new { success = true, rows, err = "是否已跟IT確認未成功產生電文，如已確認請執行【核可】!!" }, JsonRequestBehavior.AllowGet);

                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "ORTB009AController";
                    piaLogMain.EXECUTION_CONTENT = $@"fastNo:{fastNo}";
                    piaLogMain.AFFECT_ROWS = rows.Count;
                    piaLogMain.PIA_TYPE = "0000000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                    piaLogMainDao.Insert(piaLogMain);

                }
                var jsonData = new { success = true, rows, err = "" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString()};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB009Model> recData, List<ORTB009Model> rtnData)
        {
            List<FRTBARMModel> sendList = new List<FRTBARMModel>();
            List<FRTBARHModel> rtnDataList = new List<FRTBARHModel>();
            List<FRTBARHModel> recDataList = new List<FRTBARHModel>();

            //取得呼叫電文相關設定
            SysParaDao sysParaDao = new SysParaDao();
            List<SYS_PARA> sysPara = sysParaDao.qryForGrpId("RT", "FAST_API");
            string fastApiIp = "";
            int fastApiPort = 0;

            if (sysPara != null) {
                fastApiIp = sysPara.Where(x => x.PARA_ID == "ip").First().PARA_VALUE;

                try
                {
                    fastApiPort = Convert.ToInt32(sysPara.Where(x => x.PARA_ID == "port").First().PARA_VALUE);
                }
                catch (Exception e) {
                }
            }


            if("".Equals(fastApiIp) || fastApiPort == 0)
                return Json(new { success = false, err = "快速付款呼叫電文API設定錯誤，請洽IT!!" }, JsonRequestBehavior.AllowGet);



            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn())) {
                conn.Open();

                EacTransaction transaction = conn.BeginTransaction();
                //EacTransaction transaction = null;

                try
                {
                    FRTBARHDao fRTBARHDao = new FRTBARHDao();

                    //處理駁回資料
                    if (rtnData.Count > 0) {
                        foreach (ORTB009Model d in rtnData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            FRTBARHModel fRTBARHModel = new FRTBARHModel();
                            fRTBARHModel.applyNo = d.applyNo;
                            fRTBARHModel.fastNo = d.fastNo;

                            rtnDataList.Add(fRTBARHModel);
                        }
                        fRTBARHDao.updateFRTBARH0(Session["UserID"].ToString(), "3", rtnDataList, conn, transaction);
                    }
                        


                    //處理核可資料
                    if (recData.Count > 0) {
                        FRTBARMDao fRTBARMDao = new FRTBARMDao();

                        foreach (ORTB009Model d in recData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                            else {

                                //確認目前該筆快速付款資料的狀態是否可以重送
                                ORTB009Model oRTB009Model = fRTBARMDao.qryForORTB009(d.fastNo);
                                if("".Equals(oRTB009Model.fastNo))
                                    return Json(new { success = false, err = "該筆快速付款的資料狀態已變動，不可執行重送，請駁回!!" }, JsonRequestBehavior.AllowGet);

                                if (!callFastApi(d.fastNo, fastApiIp, fastApiPort))
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, err = "電文重送失敗，請洽IT!!" }, JsonRequestBehavior.AllowGet);
                                }

                                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                piaLogMain.TRACKING_TYPE = "A";
                                piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                                piaLogMain.ACCOUNT_NAME = "";
                                piaLogMain.PROGFUN_NAME = "ORTB009AController";
                                piaLogMain.EXECUTION_CONTENT = $@"fastNo:{d.fastNo}";
                                piaLogMain.AFFECT_ROWS = 1;
                                piaLogMain.PIA_TYPE = "0000000000";
                                piaLogMain.EXECUTION_TYPE = "E";
                                piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                                piaLogMainDao.Insert(piaLogMain);

                                FRTBARHModel fRTBARHModel = new FRTBARHModel();
                                fRTBARHModel.applyNo = d.applyNo;
                                fRTBARHModel.fastNo = d.fastNo;

                                recDataList.Add(fRTBARHModel);
                            }
                        }
                        fRTBARHDao.updateFRTBARH0(Session["UserID"].ToString(), "2", recDataList, conn, transaction);
                    }

                    transaction.Commit();

  

                    return Json(new { success = true });
                }
                catch (Exception e) {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }
        }


        /// <summary>
        /// 呼叫電文重送
        /// </summary>
        /// <param name="fastNo"></param>
        /// <returns></returns>
        private bool callFastApi(string fastNo, string ip, int port) {
            bool bSuccess = false;

            logger.Info("callFastApi fastNo:" + fastNo);
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn())) //20190514 加入修改電文類型為空白
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
update LRTBARM1
set TEXT_TYPE = '',
TEXT_RCVDT = '0',
TEXT_RCVTM = '0'
where FAST_NO = :FAST_NO
";
                        com.Parameters.Add("FAST_NO", fastNo?.Trim());
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }

                IPEndPoint ipont = new IPEndPoint(IPAddress.Parse(ip), port);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Connect(ipont);

                Encoding ascii = Encoding.ASCII;
                Encoding ebcdic = Encoding.GetEncoding("IBM037");   //送到AS400需以"IBM037"編碼

                byte[] dataS = Encoding.Convert(ascii, ebcdic, Encoding.ASCII.GetBytes(fastNo));
                server.Send(dataS);

                byte[] dataR = new byte[1];
                int lenR = server.Receive(dataR);
                byte[] asciiR = Encoding.Convert(ebcdic, ascii, dataR);
                string renCode = Encoding.ASCII.GetString(asciiR);

                server.Shutdown(SocketShutdown.Both);
                server.Close();

                if ("0".Equals(renCode))
                    bSuccess = true;
                else
                    bSuccess = false;
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                bSuccess = false;
            }



            return bSuccess;

        }
    }
}