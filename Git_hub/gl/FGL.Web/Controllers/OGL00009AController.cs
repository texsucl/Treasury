using FGL.Web.ActionFilter;
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;


/// <summary>
/// 功能說明：商品年期及躉繳商品設定作業
/// 初版作者：20191220 Daiyu
/// 修改歷程：20191220 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00009AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00009A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //商品類別
            var itemTermList = sysCodeDao.loadSelectList("GL", "ITEM_TERM", true);
            ViewBag.itemTermList = itemTermList;
            ViewBag.itemTermjsList = sysCodeDao.jqGridList("GL", "ITEM_TERM", true);

            //系統別
            ViewBag.sysCodejsList = sysCodeDao.jqGridList("GL", "SYS_TYPE_2", false);

            //繳費年期類別
            ViewBag.premYearjsList = sysCodeDao.jqGridList("GL", "PREM_YEAR", true);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("GL", "EXEC_ACTION", true);

            //資料狀態
            ViewBag.dataStatusjqList = sysCodeDao.jqGridList("GL", "DATA_STATUS", true);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="item_type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string item_type)
        {
            FGLGitmHisDao fGLGitmHisDao = new FGLGitmHisDao();

            try {
                List<OGL00009Model> dataList = new List<OGL00009Model>();

                dataList = fGLGitmHisDao.qryForOGL00009("", "1", item_type, "", "", "");


                //查DB_INTRA取得異動人員、覆核人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (OGL00009Model d in dataList)
                    {
                        d.sys_type = d.sys_type;
                        d.sys_type_n =d.sys_type_n;
                        d.tempId = d.item_type + d.sys_type + d.item;

                        update_id = StringUtil.toString(d.update_id);
                        d.update_name = update_id;

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);

                            d.update_name = userNameMap[update_id];
                        }
                    }
                }

                var jsonData = new { success = true, rows = dataList };
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
        public JsonResult execSave(List<OGL00009Model> recData, List<OGL00009Model> rtnData)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            List<ErrorModel> errList = new List<ErrorModel>();

            using (SqlConnection conn = new SqlConnection(strConn)) { 
                conn.Open();

                FGLGitmHisDao fGLGitmHisDao = new FGLGitmHisDao();

                //處理駁回資料
                if (rtnData.Count > 0)
                {
                    List<FGL_GITM_HIS> rtnList = new List<FGL_GITM_HIS>();

                    foreach (OGL00009Model d in rtnData) {
                        FGL_GITM_HIS model = new FGL_GITM_HIS();
                        
                        model.aply_no = d.aply_no;
                        model.item_type = d.item_type;
                        model.sys_type = d.sys_type;
                        model.item = d.item;
                        model.update_id = d.update_id;

                        rtnList.Add(model);
                    }


                    foreach (FGL_GITM_HIS d in rtnList)
                    {
                        SqlTransaction transaction = conn.BeginTransaction("Transaction");
                        try
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                            {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.tempId = "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n;
                                errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                errList.Add(errorModel);
                                transaction.Rollback();
                            }
                            else
                            {
                                fGLGitmHisDao.updateHis(Session["UserID"].ToString(), "3", d, conn, transaction);
                                transaction.Commit();
                            }
                        }
                        catch (Exception e) {
                            ErrorModel errorModel = new ErrorModel();
                            errorModel.tempId = "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n;
                            errorModel.errorMsg = StringUtil.toString(e.Message);

                            errList.Add(errorModel);
                            transaction.Rollback();
                        }
                    }
                }

                //處理核可資料
                if (recData.Count > 0)
                {
                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();

                        FGLGITM0Dao fGLGITM0Dao = new FGLGITM0Dao();

                        List<FGL_GITM_HIS> recList = new List<FGL_GITM_HIS>();
                        foreach (OGL00009Model d in recData)
                        {
                            try {
                                FGL_GITM_HIS model = new FGL_GITM_HIS();
                                OGL00009Model model_o = fGLGitmHisDao.qryForOGL00009(d.aply_no, "1", d.item_type, d.sys_type, d.item, "")[0];

                                if ("".Equals(StringUtil.toString(model_o.year)))
                                    model_o.year = "0";

                                if ("".Equals(StringUtil.toString(model_o.age)))
                                    model_o.age = "0";

                                model_o.appr_id = Session["UserID"].ToString();
                                model_o.appr_datetime = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");
                                ObjectUtil.CopyPropertiesTo(model_o, model);

                                recList.Add(model);
                            } catch (Exception e) {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.tempId = "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n;
                                errorModel.errorMsg = StringUtil.toString(e.Message);

                                errList.Add(errorModel);
                            }
                            
                        }

                        foreach (FGL_GITM_HIS d in recList)
                        {
                            SqlTransaction transaction = conn.BeginTransaction("Transaction");
                            EacTransaction transaction400 = conn400.BeginTransaction();

                            try
                            {
                                if (d.update_id.Equals(Session["UserID"].ToString()))
                                {
                                    ErrorModel errorModel = new ErrorModel();
                                    errorModel.tempId = "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n;
                                    errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                    errList.Add(errorModel);
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                }
                                else {

                                    fGLGitmHisDao.updateHis(Session["UserID"].ToString(), "2", d, conn, transaction);
                                    fGLGITM0Dao.procAppr(d, conn400, transaction400);

                                    transaction.Commit();
                                    transaction400.Commit();
                                }
                            }
                            catch (Exception e) {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.tempId = "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n;
                                errorModel.errorMsg = StringUtil.toString(e.Message);

                                errList.Add(errorModel);
                                transaction.Rollback();
                                transaction400.Rollback();
                            }
                        }
                    }    
                }
            }


            if (errList.Count > 0)
            {
                return Json(new { success = false, err = errList }, JsonRequestBehavior.AllowGet);
            }
            else {
                return Json(new { success = true });
            }
            
        }


        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ErrorModel
        {

            public string tempId { get; set; }

            public string errorMsg { get; set; }
        }
    }
}