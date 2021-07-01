using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// 功能說明：科目樣本對應SQL會科覆核作業
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
    public class OGL10182AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL10182A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(String aplyNo)
        {
            FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();

            List<GLAplyRecModel> rows = new List<GLAplyRecModel>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    try
                    {
                        //查出待覆核的資料
                        rows = fGLAplyRecDao.qryAplyType("B", "1", db);
                    }
                    catch (Exception e)
                    {
                        logger.Error("其它錯誤：" + e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";
                //string userId = "";

                foreach (GLAplyRecModel d in rows)
                {
                    createUid = StringUtil.toString(d.createUid);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.createUid = createUid + " " + userNameMap[createUid];
                    }

                }

            }
            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }




        /// <summary>
        /// 開啟科目修改修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
        {

            bool bHaveData = false;
            try
            {
                FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                FGL_APLY_REC fglAplyRec = new FGL_APLY_REC();
                GLAplyRecModel fGLAplyRecModel = new GLAplyRecModel();

                if (!"".Equals(StringUtil.toString(aplyNo)))
                {
                    fglAplyRec = fGLAplyRecDao.qryByKey(aplyNo);

                    if (fglAplyRec != null)
                        if (!"".Equals(StringUtil.toString(fglAplyRec.aply_no)))
                            bHaveData = true;
                }


                if (!bHaveData) {
                    ViewBag.bHaveData = "N";
                    return View(fGLAplyRecModel);
                }

                
                fGLAplyRecModel.aplyNo = aplyNo;
                fGLAplyRecModel.createUid = fglAplyRec.create_id;

                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        fGLAplyRecModel.createUid = fGLAplyRecModel.createUid == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(fGLAplyRecModel.createUid, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }
                fGLAplyRecModel.createDt = fglAplyRec.create_dt.ToString();

                


                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
                return View(fGLAplyRecModel);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View();
            }
        }



        /// <summary>
        /// 查詢申請單對應有異動的科目樣本SQL會科對應檔資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySmpaHis(String aplyNo)
        {
            FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();

            try
            {
                List<OGL10182Model> rows = new List<OGL10182Model>();
                rows = fGLSMPAHisDao.qryByAplyNo(aplyNo);

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error("其它錯誤：" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        [HttpPost]
        public JsonResult qryFormal(string aplyNo)
        {
            List<OGL10182Model> rows = new List<OGL10182Model>();

            FGLSMPADao fGLSMPADao = new FGLSMPADao();
            rows = fGLSMPADao.qryForOGL10182A(aplyNo);

            

            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string aplyNo, string apprStat)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC fgaAplyRec = fGLAplyRecDao.qryByKey(aplyNo);

                    if (StringUtil.toString(fgaAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    //異動"科目樣本SQL會科對應檔"
                    procSmpaHis(apprStat, aplyNo, conn, transaction); 



                    //異動覆核資料檔
                    FGL_APLY_REC fglAplyRec = new FGL_APLY_REC();
                    fglAplyRec.aply_no = aplyNo;
                    fglAplyRec.appr_stat = apprStat;
                    fglAplyRec.appr_id = Session["UserID"].ToString();
                    fglAplyRec.approve_datetime = DateTime.Now;
                    fglAplyRec.update_id = Session["UserID"].ToString();
                    fglAplyRec.update_datetime = DateTime.Now;

                    fGLAplyRecDao.updateStatus(fglAplyRec, conn, transaction);


                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[execReviewR]其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }

        }





        /// <summary>
        /// 處理"科目樣本SQL會科對應檔"異動
        /// </summary>
        /// <param name="apprStat"></param>
        /// <param name="aplyNo"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procSmpaHis(string apprStat, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            FGLSMPAHisDao fGLSMPAHisDao = new FGLSMPAHisDao();

            fGLSMPAHisDao.updateApprStat(aplyNo, apprStat, conn, transaction);


            List<OGL10182Model> smpaHisList = fGLSMPAHisDao.qryByAplyNo(aplyNo);
            if (smpaHisList != null)
            {
                if (smpaHisList.Count > 0)
                {
                    FGLSMPADao fGLSMPADao = new FGLSMPADao();

                    foreach (OGL10182Model d in smpaHisList)
                    {
                        

                        FGL_SMPA dsmpa = new FGL_SMPA();
                        dsmpa.smp_num = StringUtil.toString(d.smpNum);
                        dsmpa.product_type = StringUtil.toString(d.productType);
                        dsmpa.acct_type = StringUtil.toString(d.acctType);
                        dsmpa.corp_no = StringUtil.toString(d.corpNo);
                        dsmpa.sql_actnum = StringUtil.toString(d.sqlActNum);
                        dsmpa.sql_actnm = StringUtil.toString(d.sqlActNm);

                        dsmpa.data_status = "1";
                        dsmpa.update_id = StringUtil.toString(d.updateId);
                        dsmpa.update_datetime = DateUtil.stringToDatetime(d.updateDatetime);
                        dsmpa.appr_id = Session["UserID"].ToString();
                        dsmpa.approve_datetime = DateTime.Now;


                        if ("2".Equals(apprStat)) //核可
                        {
                            switch (d.execAction)
                            {
                                case "A":
                                    fGLSMPADao.insert(dsmpa, conn, transaction);
                                    break;
                                case "U":
                                    fGLSMPADao.update(dsmpa, conn, transaction);
                                    break;
                                case "D":
                                    fGLSMPADao.delete(dsmpa, conn, transaction);
                                    break;

                            }
                        }
                        else {  //退回
                            if(!"A".Equals(d.execAction))
                                fGLSMPADao.updateStatus("1", dsmpa, conn, transaction);
                        }
                        
                    }
                }
            }
        }




    }
}