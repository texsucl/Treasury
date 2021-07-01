using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Data.SqlClient;

/// <summary>
/// 功能說明：快速付款寄送EMAIL對象的TABLE檔維護
/// 初版作者：20181204 Daiyu
/// 修改歷程：20181204 Daiyu
///           需求單號：201811300566
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB015Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB015/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            
            SysCodeDao sysCodeDao = new SysCodeDao();

            //DATA_STATUS
            ViewBag.dataStatusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", false);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", false);

            //可設定的MAIL群組
            var mailGroupList = sysCodeDao.loadSelectList("RT", "MAIL_GROUP", true);
            ViewBag.mailGroupList = mailGroupList;
            ViewBag.mailGroupjqList = sysCodeDao.jqGridList("RT", "MAIL_GROUP", false);

            //設定類型
            ViewBag.empTypejqList = sysCodeDao.jqGridList("RT", "MAIL_EMP_TYPE", false);

            //Y/N
            ViewBag.ynjqList = sysCodeDao.jqGridList("SSO", "YN_FLAG", false);

            return View();
        }



        /// <summary>
        /// 查詢"FRT_MAIL_NOTIFY"資料
        /// </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryMailNotify(string groupCode)
        {
            logger.Info("qryMailNotify begin!!");
            try
            {
                FRTMailNotifyDao fRTMailNotifyDao = new FRTMailNotifyDao();
                List<ORTB015Model> rows = fRTMailNotifyDao.qryForORTB015(groupCode, "");

                if (rows.Count > 0) {
                    //查DB_INTRA取得mail人員姓名
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string uId = "";

                        string updId = "";
                        string apprId = "";

                        foreach (ORTB015Model d in rows.Where(x => x.empType == "U"))
                        {
                            uId = StringUtil.toString(d.receiverEmpno);

                            if (!"".Equals(uId))
                            {
                                if (!userNameMap.ContainsKey(uId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);
                                }
                                d.receiverEmpDesc = userNameMap[uId];
                            }
                        }

                        foreach (ORTB015Model d in rows)
                        {
                            updId = StringUtil.toString(d.updId);
                            apprId = StringUtil.toString(d.apprId);

                            if (!"".Equals(updId))
                            {
                                if (!userNameMap.ContainsKey(updId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, updId, dbIntra);
                                }
                                d.updateUName = userNameMap[updId];
                            }

                            if (!"".Equals(apprId))
                            {
                                if (!userNameMap.ContainsKey(apprId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprId, dbIntra);
                                }
                                d.apprName = userNameMap[apprId];
                            }

                        }

                    }
                }


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
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string groupCode, List<ORTB015Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try {
                bool bChg = false;

                List<FRT_MAIL_NOTIFY> dataFormalList = new List<FRT_MAIL_NOTIFY>();
                List<FRT_MAIL_NOTIFY_HIS> dataHisList = new List<FRT_MAIL_NOTIFY_HIS>();

                foreach (ORTB015Model d in gridData)
                {
                    if (!"".Equals(StringUtil.toString(d.status))) {

                        errModel errModel = chkAplyData(d.status, groupCode, d.receiverEmpno);
                        if (errModel.chkResult)
                        {
                            bChg = true;
                            d.updId = Session["UserID"].ToString();
                            d.updDatetime = DateUtil.DatetimeToString(DateTime.Now, "");

                            if (!"A".Equals(d.status)) {
                                FRT_MAIL_NOTIFY formal = new FRT_MAIL_NOTIFY();
                                formal.GROUP_CODE = groupCode;
                                formal.RECEIVER_EMPNO = d.receiverEmpno;
                                formal.DATA_STATUS = "2";
                                formal.UPDATE_ID = Session["UserID"].ToString();
                                formal.UPDATE_DATETIME = DateTime.Now;
                                dataFormalList.Add(formal);
                            }

                            FRT_MAIL_NOTIFY_HIS his = new FRT_MAIL_NOTIFY_HIS();
                            his.EXEC_ACTION = d.status;
                            his.GROUP_CODE = groupCode;
                            his.RECEIVER_EMPNO = d.receiverEmpno;
                            his.EMP_TYPE = d.empType;
                            his.IS_NOTIFY_MGR = d.isNotifyMgr;
                            his.IS_NOTIFY_DEPT_MGR = d.isNotifyDeptMgr;
                            his.UPDATE_ID = Session["UserID"].ToString();
                            his.UPDATE_DATETIME = DateTime.Now;
                            his.APPR_STATUS = "1";
                            dataHisList.Add(his);
                        }
                        else {
                            errStr += "角色/帳號：" + d.receiverEmpno + ":" + errModel.msg +"<br/>";
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
                FRTMailNotifyDao fRTMailNotifyDao = new FRTMailNotifyDao();
                FRTMailNotifyHisDao fRTMailNotifyHisDao = new FRTMailNotifyHisDao();

                string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("RT", "B015", qPreCode).ToString();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        fRTMailNotifyDao.updateStatus("2", dataFormalList, conn, transaction);

                        fRTMailNotifyHisDao.insert(qPreCode + cId.ToString().PadLeft(3, '0'), dataHisList, conn, transaction);

                        transaction.Commit();
                        return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        logger.Error(e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

                    }
                }

                     

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
        /// <param name="groupCode"></param>
        /// <param name="receiverEmpno"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string status, string groupCode, string receiverEmpno)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(status, groupCode, receiverEmpno);

            if(errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

        }


        private errModel chkAplyData(string status, string groupCode, string receiverEmpno)
        {
            FRTMailNotifyDao fRTMailNotifyDao = new FRTMailNotifyDao();
            FRTMailNotifyHisDao fRTMailNotifyHisDao = new FRTMailNotifyHisDao();
            List<ORTB015Model> dataO = new List<ORTB015Model>();

            errModel errModel = new errModel();

            if ("A".Equals(status))
            {
                dataO = fRTMailNotifyDao.qryForORTB015(groupCode, receiverEmpno);

                if (dataO.Count > 0) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「寄送EMAIL對象資料檔」不可新增!!";
                    return errModel;
                }
            }

            dataO = fRTMailNotifyHisDao.qryForSTAT(groupCode, receiverEmpno, "1");
            if (dataO.Count > 0)
            {
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


        /// <summary>
        /// 查詢該MAIL群組的資料來源、資料類別
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryGroupCode(string code)
        {
            try
            {
                string codeReserve1 = "";
                string codeReserve2 = "";
                string codeRemark = "";
                SysCodeDao SysCodeDao = new SysCodeDao();
                SYS_CODE data = SysCodeDao.qryByKey("RT", "MAIL_GROUP", code);
                if (data != null)
                {
                    codeReserve1 = StringUtil.toString(data.RESERVE1);
                    codeReserve2 = StringUtil.toString(data.RESERVE2);
                    codeRemark = StringUtil.toString(data.REMARK);
                }

                return Json(new { success = true, codeReserve1 = codeReserve1, codeReserve2 = codeReserve2, codeRemark = codeRemark }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public JsonResult qryEmpDesc(string empType, string receiverEmpno)
        {
            try
            {
                string receiverEmpDesc = "";

                if ("U".Equals(empType))
                {
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        OaEmpDao oaEmpDao = new OaEmpDao();

                        V_EMPLY2 emp = new V_EMPLY2();
                        emp = oaEmpDao.qryByUsrId(receiverEmpno, dbIntra);

                        if (emp != null)
                            receiverEmpDesc = StringUtil.toString(emp.EMP_NAME);
                    }
                }
                else {
                    CodeRoleDao codeRoleDao = new CodeRoleDao();
                    CODE_ROLE role = codeRoleDao.qryRoleByKey(receiverEmpno);

                    if(role != null)
                        receiverEmpDesc = StringUtil.toString(role.ROLE_NAME);
                }


                return Json(new { success = true, receiverEmpDesc = receiverEmpDesc }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}