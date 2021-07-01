using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;

/// <summary>
/// 功能說明：科目樣本險種類別維護作業
/// 初版作者：20180521 Daiyu
/// 修改歷程：20180521 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL10181Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            logger.Info("Index begin");

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";
            //String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/FGL10181/");

            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL10181/");
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
            ViewBag.productTypeList = sysCodeDao.jqGridList("GL", "PRODUCT_TYPE", true);

            //帳務類別
            ViewBag.acctTypeList = sysCodeDao.jqGridList("GL", "ACCT_TYPE", true);

            //執行功能
            ViewBag.execActionList = sysCodeDao.jqGridList("GL", "EXEC_ACTION", true);

            logger.Info("Index end");
            return View();
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
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL10181/");
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
        /// 檢查暫存檔的筆數，以判斷是否可進行維護作業
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkProductStatus(OGL10181Model model)
        {
            FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();
            int tmpStatus0Cnt = fGLSMPBHisDao.qryByProductCnt("0", model.smpNum);
            

            var jsonData = new { success = true, tmpStatus0Cnt = tmpStatus0Cnt };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult delTmp(string aplyNo)
        {
            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();

                    if (!"".Equals(StringUtil.toString(aplyNo)))
                    {
                        FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                        fGLAplyRecDao.delByAplyNo(aplyNo, conn, transaction);


                        fGLSMPBHisDao.delByAplyNo(aplyNo, conn, transaction);

                        transaction.Commit();
                        return Json(new { success = true});
                    } else
                        return Json(new { success = false, err = "無暫存資料需刪除!!" }, JsonRequestBehavior.AllowGet);
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
        /// 將畫面資料寫入暫存檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="execAction"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult procHisDetail(OGL10181Model model, string execAction, string aplyNo)
        {
            FGLSMPLDao fGLSMPLDao = new FGLSMPLDao();

            //針對帳務類別第1碼 = "A"或"B"的科目代號要設控有存在LGLSMPL1
            bool bInLGLSMPL1 = true;
            if(model.acctType.StartsWith("A") || model.acctType.StartsWith("B"))
                bInLGLSMPL1 = fGLSMPLDao.chkSmpNumForOGL10181(model.smpNum);

            if (!bInLGLSMPL1)
                return Json(new { success = false, err = "科目代號不存在PGL10180，將不進行修改作業!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();

                    if ("".Equals(StringUtil.toString(aplyNo)))
                    {
                        FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                        FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                        aplyRec.aply_type = "A";
                        aplyRec.appr_stat = "0";
                        aplyRec.appr_mapping_key = model.smpNum;
                        aplyRec.create_id = Session["UserID"].ToString();

                        //新增"覆核資料檔"
                        aplyNo = fGLAplyRecDao.insert(aplyRec, conn, transaction);

                        if ("0".Equals(model.isQryTmp))
                            fGLSMPBHisDao.insertFromFormal(aplyNo, model, conn, transaction);
                    }
                  

                    FGL_SMPB_HIS his = new FGL_SMPB_HIS();
                    his.aply_no = aplyNo;
                    his.smp_num = model.smpNum;
                    his.smp_name = model.smpName;
                    his.product_type = model.productType;
                    his.acct_type = model.acctType;

                    //更改單筆資料
                    var temp = StringUtil.toString(model.tempId).Split('|');

                    //若已存在暫存檔，查詢暫存檔的資料
                    OGL10181Model hisO = new OGL10181Model();
                    if (temp.Length == 3)
                    {
                        
                        hisO.smpNum = temp[0];
                        hisO.productType = temp[1];
                        hisO.acctType = temp[2];

                        hisO = fGLSMPBHisDao.qryByKey(aplyNo, hisO);

                        if (!"".Equals(hisO.productType))
                        {
                            if ("D".Equals(execAction))
                            {
                                his.smp_num = temp[0];
                                his.product_type = temp[1];
                                his.acct_type = temp[2];
                                his.exec_action = "D";

                                if ("A".Equals(hisO.execAction))
                                    fGLSMPBHisDao.deleteByKey(his, conn, transaction);
                                else
                                    fGLSMPBHisDao.updateByKey(his, conn, transaction);
                            }
                            else {
                                //若KEY項不相同時，要記兩筆異動檔(新增、刪除)
                                if (!(temp[0].Equals(model.smpNum) & temp[1].Equals(model.productType) & temp[2].Equals(model.acctType)))
                                {

                                    his.exec_action = "A";
                                    fGLSMPBHisDao.insert(his, conn, transaction);

                                    his.exec_action = "D";
                                    his.smp_num = temp[0];
                                    his.product_type = temp[1];
                                    his.acct_type = temp[2];

                                    if ("A".Equals(hisO.execAction))
                                        fGLSMPBHisDao.deleteByKey(his, conn, transaction);
                                    else
                                        fGLSMPBHisDao.updateByKey(his, conn, transaction);
                                }
                                else {
                                    his.exec_action = hisO.execAction == "A" ? "A" : "U";
                                    fGLSMPBHisDao.updateByKey(his, conn, transaction);
                                }
                                    
                            }
                        }
                        else {
                            return Json(new { success = false, err = "此筆資料已不存在!!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else {
                        if (!"D".Equals(execAction)) {
                            his.exec_action = "A";
                            fGLSMPBHisDao.insert(his, conn, transaction);
                        }
                    }


                    transaction.Commit();

                    return Json(new { success = true, aplyNo = aplyNo });
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
        /// 以"科目代號"查詢"科目樣本險種類別檔"資料
        /// </summary>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFglsmpb(string isQryTmp, string smpNum)
        {
            List<OGL10181Model> rows = new List<OGL10181Model>();

            if ("1".Equals(isQryTmp))
            {
                FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();
                rows = fGLSMPBHisDao.qryBySmpNum(smpNum);
            }
            else {
                FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
                rows = fGLSMPBDao.qryBySmpNum(smpNum);
            }
            

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";

                foreach (OGL10181Model d in rows)
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
        /// 查詢歷史紀錄
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="productType"></param>
        /// <param name="acctType"></param>
        /// <param name="apprDateB"></param>
        /// <param name="apprDateE"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        public JsonResult qryApprHis(string smpNum, string productType, string acctType, string apprDateB, string apprDateE, string apprStat)
        {

            List<OGL10181Model> rows = new List<OGL10181Model>();


            FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();
            rows = fGLSMPBHisDao.qryApprHis(smpNum, productType, acctType, apprDateB, apprDateE, apprStat);



            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";
                string apprId = "";

                foreach (OGL10181Model d in rows)
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
            var jsonData = new { success = true,  rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="smpbData"></param>
        /// <returns></returns>
        public JsonResult execSave(string smpNum, List<OGL10181Model> smpbData, string aplyNo)
        {
            logger.Info("execSave begin");
            FGLSMPLDao fGLSMPLDao = new FGLSMPLDao();

            //針對帳務類別第1碼="A"或"B"的科目代號要設控有存在LGLSMPL1
            bool bInLGLSMPL1 = fGLSMPLDao.chkSmpNumForOGL10181(smpNum);
            //bool bPass = true;
            bool bChg = false;

            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
            List<OGL10181Model> dataList = new List<OGL10181Model>();

            //判斷畫面資料與資料庫資料是否一致
            if (smpbData != null)
            {
                foreach (OGL10181Model d in smpbData)
                {
                    if (!bInLGLSMPL1 && (d.acctType.StartsWith("A") || d.acctType.StartsWith("B"))) {

                        return Json(new { success = false, err = "科目代號不存在PGL10180，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    }

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

                    //將此覆核單號的狀態改為"1待覆核"
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                    aplyRec = fGLAplyRecDao.qryByKey(aplyNo);
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_id = "";
                    //aplyRec.APPR_DT = DBNull.Value;
                    aplyRec.create_id = Session["UserID"].ToString();
                    aplyRec.create_dt = DateTime.Now;

                    fGLAplyRecDao.updateStatus(aplyRec, conn, transaction);

                    //將"FGL_SMPB 科目樣本險種類別檔"內對應的資料修改為"2凍結中"
                    foreach (OGL10181Model d in dataList) {
                        var temp = d.tempId.Split('|');
                        if (temp.Length < 3)
                            return Json(new { success = false, err = "資料錯誤   險種類別:" + d.productType + "、帳務類別:" + d.acctType }, JsonRequestBehavior.AllowGet);

                        FGL_SMPB smpb = new FGL_SMPB();
                        smpb.smp_num = temp[0];
                        smpb.product_type = temp[1];
                        smpb.acct_type = temp[2];
                        smpb.data_status = "2";
                        smpb.update_id = Session["UserID"].ToString();
                        smpb.update_datetime = DateTime.Now;
                        smpb.appr_id = null;
                        smpb.approve_datetime = null;

                        fGLSMPBDao.updateStatus("2", smpb, conn, transaction);
                    }

                    //將暫存檔中沒有異動的資料刪除
                    FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();
                    fGLSMPBHisDao.delNoChangeByAply(aplyNo, conn, transaction);


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

        


    }
}