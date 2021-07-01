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
/// 功能說明：商品科目設定作業
/// 初版作者：20181109 Daiyu
/// 修改歷程：20181109 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00001Controller : BaseController
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


            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00001/");
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
            ViewBag.productTypejqList = sysCodeDao.jqGridList("GL", "PRODUCT_TYPE", true);

            //Y/N
            var ynList = sysCodeDao.loadSelectList("GL", "YN_FLAG", true);
            ViewBag.ynList = ynList;
            ViewBag.ynjqList = sysCodeDao.jqGridList("GL", "YN_FLAG", true);

            //合約分類
            var itemConList = sysCodeDao.loadSelectList("GL", "ITEM_CON", true);
            ViewBag.itemConList = itemConList;
            ViewBag.itemConjqList = sysCodeDao.jqGridList("GL", "ITEM_CON", true);

            //是否適用 IFRS4
            var isIfrs4List = sysCodeDao.loadSelectList("GL", "IS_IFRS4", true);
            ViewBag.isIfrs4List = isIfrs4List;
            ViewBag.isIfrs4jqList = sysCodeDao.jqGridList("GL", "IS_IFRS4", true);


            logger.Info("Index end");
            return View();
        }


        public ActionResult detail(string execType, string tempId, string isQryTmp, string dataStatus)
        {
            logger.Info("Index begin");

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00001/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }



            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;
            ViewBag.execType = execType;
            ViewBag.isQryTmp = isQryTmp;
            ViewBag.dataStatus = dataStatus;


            SysCodeDao sysCodeDao = new SysCodeDao();
            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;
            ViewBag.productTypejqList = sysCodeDao.jqGridList("GL", "PRODUCT_TYPE", true);

            //帳務類別
            ViewBag.acctTypejqList = sysCodeDao.jqGridList("GL", "ACCT_TYPE", true);

            //Y/N
            var ynList = sysCodeDao.loadSelectList("GL", "YN_FLAG", true);
            ViewBag.ynList = ynList;
            ViewBag.ynjqList = sysCodeDao.jqGridList("GL", "YN_FLAG", true);

            //合約分類
            var itemConList = sysCodeDao.loadSelectList("GL", "ITEM_CON", true);
            ViewBag.itemConList = itemConList;
            ViewBag.itemConjqList = sysCodeDao.jqGridList("GL", "ITEM_CON", true);

            //是否適用 IFRS4
            var isIfrs4List = sysCodeDao.loadSelectList("GL", "IS_IFRS4", true);
            ViewBag.isIfrs4List = isIfrs4List;
            ViewBag.isIfrs4jqList = sysCodeDao.jqGridList("GL", "IS_IFRS4", true);

            //保費費用
            var lodprmTypeList = sysCodeDao.loadSelectList("GL", "LODPRM_TYPE", true);
            ViewBag.lodprmTypeList = lodprmTypeList;

            OGL00001Model model = new OGL00001Model();
            List<OGL00001Model> rows = new List<OGL00001Model>();

            if ("U".Equals(execType)) {
                //model.isQryTmp = isQryTmp;

                var temp = StringUtil.toString(tempId).Split('|');
                if (temp.Length == 5)
                {
                    model.productType = temp[0];
                    model.fuMk = temp[1];
                    model.itemCon = temp[2];
                    model.discPartFeat = temp[3];
                    model.isIfrs4 = temp[4];

                    //查詢暫存檔
                    if (isQryTmp == "1")
                    {
                        FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
                        rows = fGLItemAcctHisDao.qryHead(model.productType, model.fuMk
                            , model.itemCon, model.discPartFeat, model.isIfrs4);

                    }
                    else
                    {  //查詢正式檔

                        FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                        rows = fGLItemAcctDao.qryHead(model.productType, model.fuMk
                            , model.itemCon, model.discPartFeat, model.isIfrs4);
                    }
                }

                 


                if (rows.Count > 0)
                {
                    model.productType = rows[0].productType;
                    model.fuMk = rows[0].fuMk;
                    model.itemCon = rows[0].itemCon;
                    model.discPartFeat = rows[0].discPartFeat;
                    model.isIfrs4 = rows[0].isIfrs4;
                    model.investTypeMk = rows[0].investTypeMk;
                    model.investTradMk = rows[0].investTradMk;
                    model.lodprmMk = rows[0].lodprmMk;
                    //model.extSchedulMk = rows[0].extSchedulMk;
                    model.pakindDmopMk = rows[0].pakindDmopMk;
                    model.coiType = rows[0].coiType;
                }
            }



            logger.Info("Index end");
            return View(model);
        }



        /// <summary>
        /// 將畫面資料寫入暫存檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="execAction"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult procHisDetail(OGL00001Model model, string execAction)
        {
            string aplyNo = "";
            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();

                    if ("".Equals(StringUtil.toString(model.aplyNo)))
                    {
                        FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                        FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                        aplyRec.aply_type = "C";
                        aplyRec.appr_stat = "0";
                        aplyRec.appr_mapping_key = model.productType + "|" + model.fuMk + "|"
                            + model.itemCon + "|" + model.discPartFeat + "|" + model.isIfrs4;
                        aplyRec.create_id = Session["UserID"].ToString();

                        //新增"覆核資料檔"
                        aplyNo = fGLAplyRecDao.insert(aplyRec, conn, transaction);

                        model.aplyNo = aplyNo;


                        if ("0".Equals(model.isQryTmp))
                            fGLItemAcctHisDao.insertFromFormal(model, conn, transaction);
                    }
                    else
                        aplyNo = model.aplyNo;



                    FGL_ITEM_ACCT_HIS his = new FGL_ITEM_ACCT_HIS();
                    his.aply_no = aplyNo;
                    //his.EXEC_ACTION = d.execAction;
                    his.product_type = model.productType;
                    his.fu_mk = model.fuMk;
                    his.item_con = model.itemCon;
                    his.disc_part_feat = model.discPartFeat;
                    his.is_ifrs4 = model.isIfrs4;
                    his.invest_type_mk = model.investTypeMk;
                    his.invest_trad_mk = model.investTradMk;
                    his.lodprm_mk = model.lodprmMk;
                    //his.ext_schedul_mk = model.extSchedulMk;
                    his.pakind_dmop_mk = model.pakindDmopMk;
                    his.coi_type = model.coiType;
                    his.acct_type = model.acctType;
                    his.smp_num = model.acctItem;
                    his.corp_no = model.corpNo;

                    if ("UA".Equals(execAction))
                    {
                        //更改商品屬性
                        fGLItemAcctHisDao.updateProdAppr(his, conn, transaction);
                    }
                    else {
                        //更改單筆資料
                        var temp = StringUtil.toString(model.tempId).Split('|');


                        //若已存在暫存檔，查詢暫存檔的資料
                        OGL00001Model hisO = new OGL00001Model();

                        //hisO = fGLItemAcctHisDao.qryByKey(his);

                        //if (!"".Equals(hisO.productType))
                        //{
                        //    fGLItemAcctHisDao.delete(his, conn, transaction);
                        //}



                        if (temp.Length == 3)
                        {
                            his.acct_type = temp[0];
                            his.smp_num = temp[1];
                            his.corp_no = temp[2];
                        }

                        hisO = fGLItemAcctHisDao.qryByKey(his);

                        if (!"".Equals(hisO.productType))
                        {
                            fGLItemAcctHisDao.delete(his, conn, transaction);
                        }


                        if (!"D".Equals(execAction))
                        {
                            his.acct_type = model.acctType;
                            his.smp_num = model.acctItem;
                            his.corp_no = model.corpNo;
                            his.dmop_mk = model.dmopMk;

                            fGLItemAcctHisDao.insert(his, conn, transaction);
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
        /// 刪除暫存資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult delTmpData(string aplyNo)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    fGLAplyRecDao.delByAplyNo(aplyNo, conn, transaction);

                    FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
                    fGLItemAcctHisDao.delByAplyNo(aplyNo, conn, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            var jsonData = new { success = true };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 執行"申請覆核"
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execAply(OGL00001Model model, string aplyNo)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                    aplyRec = fGLAplyRecDao.qryByKey(aplyNo);
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_id = "";
                    //aplyRec.APPR_DT = DBNull.Value;
                    aplyRec.create_id = Session["UserID"].ToString();
                    aplyRec.create_dt = DateTime.Now;


                    fGLAplyRecDao.updateStatus(aplyRec, conn, transaction);

                    FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                    model.dataStatus = "2";
                    fGLItemAcctDao.updateDataStatus(model, aplyRec, conn, transaction);

                    transaction.Commit();

             //       return Json(new { success = true, aplyNo = aplyNo });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }


            var jsonData = new { success = true, aplyNo = aplyNo };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 檢查正式檔及暫存檔的筆數，以判斷是否可進行維護作業
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkProductStatus(OGL00001Model model)
        {
            FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
            int tmpStatus0Cnt = fGLItemAcctHisDao.qryByProductCnt("0", model.productType, model.fuMk, model.itemCon, model.discPartFeat, model.isIfrs4);
            int tmpStatus1Cnt = fGLItemAcctHisDao.qryByProductCnt("1", model.productType, model.fuMk, model.itemCon, model.discPartFeat, model.isIfrs4);

            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            int formalCnt = fGLItemAcctDao.qryByProductCnt(model.productType, model.fuMk, model.itemCon, model.discPartFeat, model.isIfrs4);


            var jsonData = new { success = true, formalCnt = formalCnt, tmpStatus0Cnt = tmpStatus0Cnt, tmpStatus1Cnt= tmpStatus1Cnt };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult qryProductHead(OGL00001Model model)
        {
            //查詢暫存檔
            if (model.isQryTmp == "1")
            {
                FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
                List<OGL00001Model> rows = fGLItemAcctHisDao.qryHead(model.productType, model.fuMk
                    , model.itemCon, model.discPartFeat, model.isIfrs4);


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            else {  //查詢正式檔

                FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                List<OGL00001Model> rows = fGLItemAcctDao.qryHead(model.productType, model.fuMk
                    , model.itemCon, model.discPartFeat, model.isIfrs4);

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }
        

        [HttpPost]
        public JsonResult qryDetail(OGL00001Model model)
        {
            List<OGL00001Model> rows = new List<OGL00001Model>();

            //查詢暫存檔
            if (model.isQryTmp == "1")
            {
                FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
                rows = fGLItemAcctHisDao.qryByProduct(model.productType, model.fuMk
                    , model.itemCon, model.discPartFeat, model.isIfrs4);
            }
            else {
                FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                rows = fGLItemAcctDao.qryByProduct(model.productType, model.fuMk
                    , model.itemCon, model.discPartFeat, model.isIfrs4);
            }
              


            var jsonData = new { success = true, dataList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public partial class AcctModel
        {
            public string tempId { get; set; }

            public string execAction { get; set; }
            /// <summary>
            /// 帳務類別
            /// </summary>
            public string acctType { get; set; }

            /// <summary>
            /// 科目代號
            /// </summary>
            public string acctItem { get; set; }

            /// <summary>
            /// 帳本
            /// </summary>
            public string corpNo { get; set; }

            /// <summary>
            /// 限躉繳註記
            /// </summary>
            public string dmopMk { get; set; }

        }

        
    }
}