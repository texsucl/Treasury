using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：保局範圍TABLE維護作業
/// 初版作者：20190612 Daiyu
/// 修改歷程：20190612 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0005Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0005/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            //執行功能
            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);

            return View();
        }



        /// <summary>
        /// 查詢"FAP_VE_CODE 逾期未兌領代碼設定檔"資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryVeCode()
        {
            logger.Info("qryVeCode begin!!");
            try
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rows = fAPVeCodeDao.qryByGrp("FSC_RANGE");

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }



        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<VeTraceModel> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                bool bChg = false;
                List<VeTraceModel> dataList = new List<VeTraceModel>();


                foreach (VeTraceModel d in gridData)
                {
                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {

                        errModel errModel = chkAplyData(d.exec_action, d.code_id, d.code_value);
                        if (errModel.chkResult)
                        {
                            bChg = true;
                            d.code_type = "FSC_RANGE";
                            d.appr_stat = "1"; 
                            d.update_id = Session["UserID"].ToString();
                            dataList.Add(d);
                        }
                        else
                        {
                            errStr += "代碼：" + d.code_id + " 保局範圍：" + d.code_value + " 錯誤原因：" + errModel.msg + "<br/>";
                        }
                    }
                }


                if (bChg == false)
                {
                    if ("".Equals(errStr))
                        return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = true, err = errStr });
                }


                /*------------------ DB處理   begin------------------*/
                

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "VE" + curDateTime[0].Substring(0, 5);
                var cId = sysSeqDao.qrySeqNo("AP", "VE", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(5, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;

                        //新增覆核資料至【FAP_VE_CODE_HIS 逾期未兌領代碼設定暫存檔】
                        FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
                        fAPVeCodeHisDao.insert(aply_no, dt,  dataList, conn, transaction);

                        //將已存在【FAP_VE_CODE 逾期未兌領代碼設定檔】的資料設為"凍結中"
                        FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                        foreach (VeTraceModel d in dataList) {
                            if (!"A".Equals(d.exec_action))
                                fAPVeCodeDao.updateStatus("1", dt,  d, conn, transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                        transaction.Rollback();

                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }

                return Json(new { success = true, aply_no = aply_no, err = errStr });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }



        /// <summary>
        /// 畫面GRID在儲存前，需先檢查可存檔
        /// </summary>
        /// <param name="status"></param>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(exec_action, code_id, code_value);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

        }


        private errModel chkAplyData(string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();

            if ("A".Equals(exec_action))
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                FAP_VE_CODE formal = fAPVeCodeDao.qryByKey("FSC_RANGE", code_id);

                if (!"".Equals(StringUtil.toString(formal.code_id))) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「逾期未兌領代碼設定檔」不可新增!!";
                    return errModel;
                }
            }


            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
            FAP_VE_CODE_HIS his = fAPVeCodeHisDao.qryInProssById("FSC_RANGE", code_id, "");
            if (!"".Equals(StringUtil.toString(his.code_id))) {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }

            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }


        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}