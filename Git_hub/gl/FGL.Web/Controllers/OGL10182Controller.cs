using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

/// <summary>
/// 功能說明：科目樣本對應SQL會科維護作業
/// 初版作者：20180525 Daiyu
/// 修改歷程：20180525 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL10182Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL10182/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();


            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;
            ViewBag.productTypejqList = sysCodeDao.jqGridList("GL", "PRODUCT_TYPE", true);

            //帳務類別
            var acctTypeList = sysCodeDao.loadSelectList("GL", "ACCT_TYPE", true);
            ViewBag.acctTypeList = acctTypeList;
            ViewBag.acctTypejqList = sysCodeDao.jqGridList("GL", "ACCT_TYPE", true);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("GL", "EXEC_ACTION", true);

            return View();
        }



        /// <summary>
        /// 以"科目代號、險類類別、帳本、帳務類別"查詢"科目樣本險種類別檔"資料
        /// </summary>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFglsmpa(string isQryTmp, string smpNum, string productType, string corpNo, string acctType)
        {

            List<OGL10182Model> rows = new List<OGL10182Model>();

            

            if ("1".Equals(isQryTmp))
            {
                FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();
                rows = fGLSMPAHisDao.qryForFGL10182(smpNum, productType, corpNo, acctType);
            }
            else
            {
                FGLSMPADao fGLSMPADao = new FGLSMPADao();
                rows = fGLSMPADao.qryForFGL10182(smpNum, productType, corpNo, acctType);
            }



            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";

                foreach (OGL10182Model d in rows)
                {
                    uId = StringUtil.toString(d.updateId);

                    if (!"".Equals(uId))
                    {
                        if (!userNameMap.ContainsKey(uId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);
                        }
                        d.updateUName = userNameMap[uId];
                    }
                }
            }

            var jsonData = new { success = true, dataList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 將畫面資料寫入暫存檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="execAction"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult procHisDetail(OGL10182Model model, string execAction)
        {
            //20190531加檢核要異動的科目需存在於FGL_SMPB
            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
            FGL_SMPB smpb = fGLSMPBDao.qryByKey(model.smpNum, model.productType, model.acctType);
            if(smpb == null & !"D".Equals(execAction))
                return Json(new { success = false, err = "該科目不存在『科目樣本險種類別』，請至『OGL10181科目樣本險種類別維護作業』進行相關資料維護!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();


                    FGL_SMPA_HIS smpaHis = new FGL_SMPA_HIS();
                    smpaHis.aply_no = "";
                    smpaHis.exec_action = execAction;
                    smpaHis.smp_num = model.smpNum;
                    smpaHis.corp_no = model.corpNo;
                    smpaHis.product_type = model.productType;
                    smpaHis.acct_type = model.acctType;
                    smpaHis.sql_actnum = model.sqlActNum;
                    smpaHis.sql_actnm = StringUtil.toString(model.sqlActNm);
                    smpaHis.update_id = Session["UserID"].ToString();
                    smpaHis.update_datetime = DateTime.Now;
                    smpaHis.appr_stat = "0";

                    //查詢正式檔資料
                    FGLSMPADao fGLSMPADao = new FGLSMPADao();
                    List<OGL10182Model> formal = fGLSMPADao.qryForFGL10182(model.smpNum, model.productType, model.corpNo, model.acctType);


                    //更改單筆資料
                    var temp = StringUtil.toString(model.tempId).Split('|');

                    //若已存在暫存檔，查詢暫存檔的資料
                    OGL10182Model hisO = new OGL10182Model();
                    


                    if (temp.Length == 4)
                    {
                        string smpNum = temp[0];
                        string productType = temp[1];
                        string acctType = temp[2];
                        string corpNo = temp[3];

                        //判斷修改資料的原始「樣本科目代號+險種類別+帳本代號+帳務類別」是否已存在暫存檔
                        hisO = fGLSMPAHisDao.qryByKey("", smpNum, productType, corpNo, acctType);

                        if (!"".Equals(hisO.productType))
                        {
                            if ("D".Equals(execAction))
                            {
                                smpaHis.smp_num = temp[0];
                                smpaHis.product_type = temp[1];
                                smpaHis.acct_type = temp[2];
                                smpaHis.corp_no = temp[3];

                                smpaHis.exec_action = "D";

                                if ("A".Equals(hisO.execAction))
                                    fGLSMPAHisDao.deleteByKey(smpaHis, conn, transaction);
                                else
                                    fGLSMPAHisDao.updateByKey(smpaHis, conn, transaction);
                            }
                            else
                            {
                                //若KEY項不相同時，要記兩筆異動檔(新增、刪除)
                                if (hisO.smpNum != model.smpNum || hisO.productType != model.productType
                                    || hisO.corpNo != model.corpNo || hisO.acctType != model.acctType)
                                {
                                    if (formal.Count > 0)
                                        return Json(new { success = false, err = "修改後的「樣本科目代號+險種類別+帳本代號+帳務類別」已存在，不可異動!!" }, JsonRequestBehavior.AllowGet);
                                    else {

                                        smpaHis.exec_action = "A";
                                        fGLSMPAHisDao.insert(smpaHis, conn, transaction);

                                        smpaHis.exec_action = "D";
                                        smpaHis.smp_num = temp[0];
                                        smpaHis.product_type = temp[1];
                                        smpaHis.acct_type = temp[2];
                                        smpaHis.corp_no = temp[3];

                                        if ("A".Equals(hisO.execAction))
                                            fGLSMPAHisDao.deleteByKey(smpaHis, conn, transaction);
                                        else
                                            fGLSMPAHisDao.updateByKey(smpaHis, conn, transaction);
                                    }
                                    
                                }
                                else
                                {
                                    smpaHis.exec_action = hisO.execAction == "A" ? "A" :"U";
                                    fGLSMPAHisDao.updateByKey(smpaHis, conn, transaction);
                                }

                            }
                        }
                        else
                        {
                            hisO.smpNum = smpNum;
                            hisO.productType = productType;
                            hisO.acctType = acctType;
                            hisO.corpNo = corpNo;



                            if ("D".Equals(execAction))
                            {
                                smpaHis.exec_action = "D";
                                fGLSMPAHisDao.insert(smpaHis, conn, transaction);
                            }
                            else {

                                //若KEY項不相同時，要記兩筆異動檔(新增、刪除)
                                if (hisO.smpNum != model.smpNum || hisO.productType != model.productType
                                    || hisO.corpNo != model.corpNo || hisO.acctType != model.acctType)
                                {
                                   
                                    if (formal.Count > 0)
                                        return Json(new { success = false, err = "修改後的「樣本科目代號+險種類別+帳本代號+帳務類別」已存在，不可異動!!" }, JsonRequestBehavior.AllowGet);


                                    OGL10182Model hisTobe = fGLSMPAHisDao.qryByKey("", model.smpNum, model.productType, model.corpNo, model.acctType);
                                    if (!"".Equals(hisTobe.productType))
                                        return Json(new { success = false, err = "同樣的「樣本科目代號+險種類別+帳本代號+帳務類別」已存在暫存檔，不可新增!!" }, JsonRequestBehavior.AllowGet);

                                    smpaHis.exec_action = "A";
                                    fGLSMPAHisDao.insert(smpaHis, conn, transaction);

                                   

                                    smpaHis.exec_action = "D";
                                    smpaHis.smp_num = temp[0];
                                    smpaHis.product_type = temp[1];
                                    smpaHis.acct_type = temp[2];
                                    smpaHis.corp_no = temp[3];
                                    fGLSMPAHisDao.insert(smpaHis, conn, transaction);

                                }
                                else {
                                    fGLSMPAHisDao.insert(smpaHis, conn, transaction);
                                }

                            }
          
                        }
                    }
                    else
                    {
                        if (!"D".Equals(execAction))
                        {
                            if (formal.Count > 0)
                                return Json(new { success = false, err = "同樣的「樣本科目代號+險種類別+帳本代號+帳務類別」已存在，不可新增!!" }, JsonRequestBehavior.AllowGet);
                            else {
                                smpaHis.exec_action = "A";
                                fGLSMPAHisDao.insert(smpaHis, conn, transaction);
                            }
                        }
                    }


                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            /*------------------ DB處理   end------------------*/


        }


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="smpaData"></param>
        /// <returns></returns>
        public JsonResult execSave(string smpNum, string productType, string corpNo, string acctType, List<OGL10182Model> smpaData)
        {
            logger.Info("execSave begin");

            //bool bPass = true;
            bool bChg = false;

            FGLSMPADao fGLSMPADao = new FGLSMPADao();
            //List<OGL10182Model> smpaDataO = fGLSMPADao.qryForFGL10182(smpNum, itemType, corpNo, acctType);
            List<OGL10182Model> dataList = new List<OGL10182Model>();

            //判斷畫面資料與資料庫資料是否一致
            if (smpaData != null)
            {
                foreach (OGL10182Model d in smpaData)
                {
                    bChg = true;
                    dataList.Add(d);
                }
            }



            if (bChg == false)
                return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                    aplyRec.aply_type = "B";
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_mapping_key = "";
                    aplyRec.create_id = Session["UserID"].ToString();

                    //新增"覆核資料檔"
                    string aplyNo = fGLAplyRecDao.insert(aplyRec, conn, transaction);

                    FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();


                    foreach (OGL10182Model d in dataList) {
                        FGL_SMPA_HIS smpaHis = new FGL_SMPA_HIS();
                        smpaHis.aply_no = aplyNo;
                        smpaHis.exec_action = d.execAction;
                        smpaHis.smp_num = d.smpNum;
                        smpaHis.corp_no = d.corpNo;
                        smpaHis.product_type = d.productType;
                        smpaHis.acct_type = d.acctType;
                        smpaHis.update_id = Session["UserID"].ToString();
                        smpaHis.update_datetime = DateTime.Now;
                        smpaHis.appr_stat = "1";

                        //將歷史檔壓上覆核單號
                        fGLSMPAHisDao.updateAplyNo(smpaHis, conn, transaction);

                        //將正式檔狀態異動為"凍結"
                        if ("D".Equals(smpaHis.exec_action) || "U".Equals(smpaHis.exec_action)) {
                            FGL_SMPA smpa = new FGL_SMPA();

                            smpa.smp_num = d.smpNum;
                            smpa.product_type = d.productType;
                            smpa.acct_type = d.acctType;
                            smpa.corp_no = d.corpNo;
                            smpa.data_status = "2";

                            smpa.update_id = Session["UserID"].ToString();
                            smpa.update_datetime = DateTime.Now;
                            smpa.appr_id = null;
                            smpa.approve_datetime = null;

                            fGLSMPADao.updateStatus("2", smpa, conn, transaction);

                        }
                    }
                            

                    transaction.Commit();

                    return Json(new { success = true, aplyNo = aplyNo });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[updateUser]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            /*------------------ DB處理   end------------------*/

        }


        /// <summary>
        /// 畫面執行"刪除暫存資料"
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="productType"></param>
        /// <param name="corpNo"></param>
        /// <param name="acctType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult delTmp(string smpNum, string productType, string corpNo, string acctType)
        {
            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();

                    fGLSMPAHisDao.delTmpFOr10182(smpNum, productType, corpNo, acctType, conn, transaction);

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            /*------------------ DB處理   end------------------*/
        }


        /// <summary>
        /// 查詢歷史資料
        /// </summary>
        /// <returns></returns>
        public ActionResult aplyHis()
        {

            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL10182/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;



            SysCodeDao sysCodeDao = new SysCodeDao();
            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;

            //帳務類別
            var acctTypeList = sysCodeDao.loadSelectList("GL", "ACCT_TYPE", true);
            ViewBag.acctTypeList = acctTypeList;

            //覆核狀態
            var apprStatList = sysCodeDao.loadSelectList("GL", "APPR_STAT", true);
            ViewBag.apprStatList = apprStatList;


            return View();
        }



        /// <summary>
        /// 查詢歷史紀錄
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="productType"></param>
        /// <param name="corpNo"></param>
        /// <param name="acctType"></param>
        /// <param name="apprDateB"></param>
        /// <param name="apprDateE"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        public JsonResult qryApprHis(string smpNum, string productType, string corpNo, string acctType, string apprDateB, string apprDateE, string apprStat)
        {

            List<OGL10182Model> rows = new List<OGL10182Model>();


            FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();
            rows = fGLSMPAHisDao.qryApprHis(smpNum, productType, corpNo, acctType, apprDateB, apprDateE, apprStat);



            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";
                string apprId = "";

                foreach (OGL10182Model d in rows)
                {
                    uId = StringUtil.toString(d.updateId);

                    if (!"".Equals(uId))
                    {
                        if (!userNameMap.ContainsKey(uId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);
                        }
                        d.updateId = userNameMap[uId];
                    }


                    apprId = StringUtil.toString(d.apprId);

                    if (!"".Equals(apprId))
                    {
                        if (!userNameMap.ContainsKey(apprId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, apprId, dbIntra);
                        }
                        d.apprId = userNameMap[apprId];
                    }


                }
            }
            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


    }
}