using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：商品科目設定覆核作業
/// 初版作者：20181113 Daiyu
/// 修改歷程：20181113 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00001AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00001A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            SysCodeDao sysCodeDao = new SysCodeDao();
            //險種類別
            ViewBag.productTypejqList = sysCodeDao.jqGridList("GL", "PRODUCT_TYPE", true);

            //Y/N
            ViewBag.ynjqList = sysCodeDao.jqGridList("GL", "YN_FLAG", true);

            //合約分類
            ViewBag.itemConjqList = sysCodeDao.jqGridList("GL", "ITEM_CON", true);

            //是否適用 IFRS4
            ViewBag.isIfrs4jqList = sysCodeDao.jqGridList("GL", "IS_IFRS4", true);


            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();

            List<GLAplyRecModel> rowsAply = new List<GLAplyRecModel>();
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
                        rowsAply = fGLAplyRecDao.qryAplyType("C", "1", db);
                    }
                    catch (Exception e)
                    {
                        logger.Error("其它錯誤：" + e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }

            List<OGL00001Model> rows = new List<OGL00001Model>();

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (GLAplyRecModel aply in rowsAply)
                {
                    OGL00001Model d = new OGL00001Model();
                    d.aplyNo = aply.aplyNo;
                    d.lastUpdateUid = aply.createUid;
                    d.lastUpdateDT = aply.createDt;

                    createUid = StringUtil.toString(d.lastUpdateUid);

                    var temp = StringUtil.toString(aply.cMappingKey).Split('|');
                    if (temp.Length == 5)
                    {
                        d.productType = temp[0];
                        d.fuMk = temp[1];
                        d.itemCon = temp[2];
                        d.discPartFeat = temp[3];
                        d.isIfrs4 = temp[4];
                    }

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.lastUpdateUid = createUid + " " + userNameMap[createUid];
                    }

                    rows.Add(d);
                }
            }

              


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult qryHis(String aplyNo)
        {
            FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
            
            try
            {
                List<OGL00001Model> rows = fGLItemAcctHisDao.qryByAplyNo(aplyNo);

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
        public JsonResult qryFormal(string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4)
        {
            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();

            try
            {
                List<OGL00001Model> rows = fGLItemAcctDao.qryByProduct(productType, fuMk, itemCon, discPartFeat, isIfrs4);

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error("其它錯誤：" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }


        }


        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
        {
            SysCodeDao sysCodeDao = new SysCodeDao();
            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;

            //帳務類別
            ViewBag.acctTypejqList = sysCodeDao.jqGridList("GL", "ACCT_TYPE", true);

            //Y/N
            var ynList = sysCodeDao.loadSelectList("GL", "YN_FLAG", true);
            ViewBag.ynList = ynList;
            ViewBag.ynjqList = sysCodeDao.jqGridList("GL", "YN_FLAG", true);

            //合約分類
            var itemConList = sysCodeDao.loadSelectList("GL", "ITEM_CON", true);
            ViewBag.itemConList = itemConList;

            //保費費用
            var lodprmTypeList = sysCodeDao.loadSelectList("GL", "LODPRM_TYPE", true);
            ViewBag.lodprmTypeList = lodprmTypeList;

            //是否適用 IFRS4
            var isIfrs4List = sysCodeDao.loadSelectList("GL", "IS_IFRS4", true);
            ViewBag.isIfrs4List = isIfrs4List;



            OGL00001Model oGL00001Model = new OGL00001Model();

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
                    return View(oGL00001Model);
                }


                oGL00001Model.aplyNo = aplyNo;
                oGL00001Model.lastUpdateUid = fglAplyRec.create_id;

                var temp = StringUtil.toString(fglAplyRec.appr_mapping_key).Split('|');
                if (temp.Length == 5)
                {
                    ViewBag.productType = temp[0];
                    ViewBag.fuMk = temp[1];
                    ViewBag.itemCon = temp[2];
                    ViewBag.discPartFeat = temp[3];
                    ViewBag.isIfrs4 = temp[4];
                }

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
                oGL00001Model.lastUpdateDT = fglAplyRec.create_dt.ToString();

               
                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
    
                return View(oGL00001Model);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(oGL00001Model);
            }
        }

        


        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OGL00001Model model, string apprStat)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC fglAplyRec = fGLAplyRecDao.qryByKey(model.aplyNo);

                    if(StringUtil.toString(fglAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    //異動覆核資料檔
                    fglAplyRec.aply_no = model.aplyNo;
                    fglAplyRec.appr_stat = apprStat;
                    fglAplyRec.appr_id = Session["UserID"].ToString();
                    fglAplyRec.approve_datetime = DateTime.Now;
                    fglAplyRec.update_id = Session["UserID"].ToString();
                    fglAplyRec.update_datetime = DateTime.Now;

                    fGLAplyRecDao.updateStatus(fglAplyRec, conn, transaction);

                    //異動"商品科目設定檔"
                    FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                    if ("2".Equals(apprStat))
                    {
                        //核可
                        fGLItemAcctDao.delByProd(model, conn, transaction);
                        fGLItemAcctDao.insertFromHis(model, fglAplyRec, conn, transaction);
                    }
                    else {
                        //駁回
                        model.dataStatus = "1";
                        fGLItemAcctDao.updateDataStatus(model, fglAplyRec, conn, transaction);
                    }


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
        /// 處理"科目樣本險種類別檔"異動
        /// </summary>
        /// <param name="apprStat"></param>
        /// <param name="aplyNo"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procSmpbHis(string apprStat, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            FGLSMPBHisDao fGLSMPBHisDao = new FGLSMPBHisDao();


            List<OGL10181Model> smpbHisList = fGLSMPBHisDao.qryByAplyNo(aplyNo);
            if (smpbHisList != null)
            {
                if (smpbHisList.Count > 0)
                {
                    FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();

                    foreach (OGL10181Model d in smpbHisList)
                    {
                        FGL_SMPB dsmpb = new FGL_SMPB();
                        dsmpb.smp_num = StringUtil.toString(d.smpNum);
                        dsmpb.smp_name = StringUtil.toString(d.smpName);
                        dsmpb.product_type = StringUtil.toString(d.productType);
                        dsmpb.acct_type = StringUtil.toString(d.acctType);
                        dsmpb.data_status = "1";
                        dsmpb.update_id = StringUtil.toString(d.updateId);
                        dsmpb.update_datetime = DateUtil.stringToDatetime(d.updateDatetime);
                        dsmpb.appr_id = Session["UserID"].ToString();
                        dsmpb.approve_datetime = DateTime.Now;


                        if ("2".Equals(apprStat)) //核可
                        {
                            switch (d.execAction)
                            {
                                case "A":
                                    fGLSMPBDao.insert(dsmpb, conn, transaction);
                                    break;
                                case "U":
                                    fGLSMPBDao.update(dsmpb, conn, transaction);
                                    break;
                                case "D":
                                    fGLSMPBDao.delete(dsmpb, conn, transaction);
                                    break;

                            }
                        }
                        else {  //退回
                            if(!"A".Equals(d.execAction))
                                fGLSMPBDao.updateStatus("1", dsmpb, conn, transaction);
                        }
                        
                    }
                }
            }
        }




    }
}