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
/// 功能說明：保險商品編號代碼轉換作業
/// 初版作者：20181126 Daiyu
/// 修改歷程：20181126 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00002AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00002A/");
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
                        rows = fGLAplyRecDao.qryAplyType("D", "1", db);
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
        /// 開啟明細畫面
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
        /// 查詢申請單對應FGL_ITEM_CODE_TRAN_HIS 保險商品編號代碼轉換暫存檔
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryHis(String aplyNo)
        {
            FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();

            try
            {
                List<OGL00002Model> dataList = new List<OGL00002Model>();
                List<OGL00002Model> rows = new List<OGL00002Model>();
                dataList = fGLItemCodeTranHisDao.qryByAplyNo(aplyNo);


                foreach (OGL00002Model d in dataList)
                {
                    string productNo = d.productNo.PadRight(27, ' ');

                    OGL00002Model type1 = new OGL00002Model();
                    // type1.tempId = d.tempId + "|" + "1";
                    type1.tempId = d.tempId;
                    //  type1.rowType = "1";
                    type1.p01 = productNo.Substring(0, 1);
                    type1.p02 = productNo.Substring(1, 1);
                    type1.p03 = productNo.Substring(2, 1);
                    type1.p04 = productNo.Substring(3, 1);
                    type1.p05 = productNo.Substring(4, 1);
                    type1.p06 = productNo.Substring(5, 1);
                    type1.p07 = productNo.Substring(6, 1);
                    type1.p08 = productNo.Substring(7, 1);
                    type1.p09 = productNo.Substring(8, 1);
                    type1.p10 = productNo.Substring(9, 1);
                    type1.p11 = productNo.Substring(10, 1);
                    type1.p12 = productNo.Substring(11, 1);
                    type1.p13 = productNo.Substring(12, 1);
                    type1.p14 = productNo.Substring(13, 1);
                    type1.p15 = productNo.Substring(14, 1);
                    type1.p16 = productNo.Substring(15, 1);
                    type1.p17 = productNo.Substring(16, 1);
                    type1.p18 = productNo.Substring(17, 1);
                    type1.p19 = productNo.Substring(18, 1);
                    type1.p20 = productNo.Substring(19, 1);
                    type1.p21 = productNo.Substring(20, 1);
                    type1.p22 = productNo.Substring(21, 1);
                    type1.p23 = productNo.Substring(22, 1);
                    type1.p24 = productNo.Substring(23, 1);
                    type1.p25 = productNo.Substring(24, 1);
                    type1.p26 = productNo.Substring(25, 1);
                    type1.p27 = productNo.Substring(26, 1);

                    type1.tranA = d.tranA;
                    type1.tranB = d.tranB;
                    type1.tranC = d.tranC;
                    type1.tranD = d.tranD;
                    type1.tranE = d.tranE;
                    type1.tranF = d.tranF;
                    type1.tranG = d.tranG;
                    type1.tranH = d.tranH;
                    type1.tranI = d.tranI;
                    type1.tranJ = d.tranJ;
                    type1.tranK = d.tranK;

                    type1.dataStatus = d.dataStatus;
                    type1.dataStatusDesc = d.dataStatusDesc;
                    type1.execAction = d.execAction;
                    type1.execActionDesc = d.execActionDesc;

                    type1.updateId = d.updateId;
                    type1.updateDatetime = d.updateDatetime;
                    type1.apprId = d.apprId;
                    type1.apprDt = d.apprDt;

                    rows.Add(type1);

                }


                ////將資料庫內的一筆資料拆成兩筆，呈現於畫面
                //foreach (OGL00002Model d in dataList)
                //{
                //    OGL00002Model type1 = new OGL00002Model();
                //    type1.tempId = d.tempId + "|" + "1";
                //    type1.rowType = "1";
                //    type1.p01 = d.tranA;
                //    type1.p02 = d.tranB;
                //    type1.p03 = d.tranC;
                //    type1.p04 = d.tranD;
                //    type1.p05 = d.tranE;
                //    type1.p06 = d.tranF;
                //    type1.p07 = d.tranG;
                //    type1.p08 = d.tranH;
                //    type1.p09 = d.tranI;
                //    type1.p10 = d.tranJ;
                //    type1.p11 = d.tranK;

                //    type1.tranA = d.tranA;
                //    type1.tranB = d.tranB;
                //    type1.tranC = d.tranC;
                //    type1.tranD = d.tranD;
                //    type1.tranE = d.tranE;
                //    type1.tranF = d.tranF;
                //    type1.tranG = d.tranG;
                //    type1.tranH = d.tranH;
                //    type1.tranI = d.tranI;
                //    type1.tranJ = d.tranJ;
                //    type1.tranK = d.tranK;

                //    type1.dataStatus = d.execAction;
                //    type1.dataStatusDesc = d.execActionDesc;
                //    rows.Add(type1);


                //    string productNo = d.productNo.PadRight(27, ' ');
                //    OGL00002Model type2 = new OGL00002Model();
                //    type2.tempId = d.tempId + "|" + "2";
                //    type2.rowType = "2";
                //    type2.p01 = productNo.Substring(0, 1);
                //    type2.p02 = productNo.Substring(1, 1);
                //    type2.p03 = productNo.Substring(2, 1);
                //    type2.p04 = productNo.Substring(3, 1);
                //    type2.p05 = productNo.Substring(4, 1);
                //    type2.p06 = productNo.Substring(5, 1);
                //    type2.p07 = productNo.Substring(6, 1);
                //    type2.p08 = productNo.Substring(7, 1);
                //    type2.p09 = productNo.Substring(8, 1);
                //    type2.p10 = productNo.Substring(9, 1);
                //    type2.p11 = productNo.Substring(10, 1);
                //    type2.p12 = productNo.Substring(11, 1);
                //    type2.p13 = productNo.Substring(12, 1);
                //    type2.p14 = productNo.Substring(13, 1);
                //    type2.p15 = productNo.Substring(14, 1);
                //    type2.p16 = productNo.Substring(15, 1);
                //    type2.p17 = productNo.Substring(16, 1);
                //    type2.p18 = productNo.Substring(17, 1);
                //    type2.p19 = productNo.Substring(18, 1);
                //    type2.p20 = productNo.Substring(19, 1);
                //    type2.p21 = productNo.Substring(20, 1);
                //    type2.p22 = productNo.Substring(21, 1);
                //    type2.p23 = productNo.Substring(22, 1);
                //    type2.p24 = productNo.Substring(23, 1);
                //    type2.p25 = productNo.Substring(24, 1);
                //    type2.p26 = productNo.Substring(25, 1);
                //    type2.p27 = productNo.Substring(26, 1);

                //    //type2.dataStatus = d.execAction;
                //    //type2.dataStatusDesc = d.execActionDesc;

                //    rows.Add(type2);
                //}

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
                    procHis(apprStat, aplyNo, conn, transaction); 



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
        private void procHis(string apprStat, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();


            List<OGL00002Model> hisList = fGLItemCodeTranHisDao.qryByAplyNo(aplyNo);
            if (hisList != null)
            {
                if (hisList.Count > 0)
                {

                    fGLItemCodeTranHisDao.updateApprStat(apprStat, aplyNo, conn, transaction);

                    FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();

                    foreach (OGL00002Model d in hisList)
                    {
                        

                        FGL_ITEM_CODE_TRAN item = new FGL_ITEM_CODE_TRAN();
                        item.product_no = d.productNo;
                        item.tran_a = StringUtil.toString(d.tranA);
                        item.tran_b = StringUtil.toString(d.tranB);
                        item.tran_c = StringUtil.toString(d.tranC);
                        item.tran_d = StringUtil.toString(d.tranD);
                        item.tran_e = StringUtil.toString(d.tranE);
                        item.tran_f = StringUtil.toString(d.tranF);
                        item.tran_g = StringUtil.toString(d.tranG);
                        item.tran_h = StringUtil.toString(d.tranH);
                        item.tran_i = StringUtil.toString(d.tranI);
                        item.tran_j = StringUtil.toString(d.tranJ);
                        item.tran_k = StringUtil.toString(d.tranK);

                        item.data_status = "1";
                        item.update_id = StringUtil.toString(d.updateId);
                        item.update_datetime = DateUtil.stringToDatetime(d.updateDatetime);
                        item.appr_id = Session["UserID"].ToString();
                        item.approve_datetime = DateTime.Now;


                        if ("2".Equals(apprStat)) //核可
                        {
                            switch (d.execAction)
                            {
                                case "A":
                                    fGLItemCodeTranDao.insert(item, conn, transaction);
                                    break;
                                case "U":
                                    fGLItemCodeTranDao.update(item, conn, transaction);
                                    break;
                                case "D":
                                    fGLItemCodeTranDao.delete(item, conn, transaction);
                                    break;

                            }
                        }
                        else {  //退回
                            if(!"A".Equals(d.execAction))
                                fGLItemCodeTranDao.updateStatus("1", item, conn, transaction);
                        }
                        
                    }
                }
            }
        }




    }
}