using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using FAP.Web.Models;
using System.Threading.Tasks;
using System.Data.EasycomClient;
using FAP.Web.AS400PGM;
using FAP.Web.AS400Models;

/// <summary>
/// 功能說明：OAP0043A 電訪派件標準設定覆核作業
/// 初版作者：20200831 Daiyu
/// 修改歷程：20200831 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：
/// 需求單號：
/// 修改內容：
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0043AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static string funcName = "";

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            //string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0043A/");
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
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            try
            {
                FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();

                SysCodeDao sysCodeDao = new SysCodeDao();
                Dictionary<string, string> typeMap = sysCodeDao.qryByTypeDic("AP", "OAP0042_TYPE");


                List<OAP0043Model> rows = fAPTelCodeHisDao.qryByOAP0043A(typeMap.Keys.ToArray());
                


                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (OAP0043Model d in rows)
                {

                    //取得申請人姓名
                    usr_id = StringUtil.toString(d.create_id);
                    if (!"".Equals(usr_id))
                    {
                        if (!userNameMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }
                        d.create_id = d.create_id + " " + userNameMap[usr_id].name;
                    }

                    if (typeMap.ContainsKey(d.type))
                        d.type_desc = typeMap[d.type];

                }


                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString() };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aply_no, string type)
        {
            ViewBag.funcName = funcName;

            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();

            List<OAP0043Model> rows = fAPTelCodeHisDao.qryByAplyNo(type, aply_no, "1")
                .OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).ThenByDescending(x => x.amt_range).ToList();


            //依保局範圍
            List<OAP0043Model> rowsByFscRange = rows.GroupBy(x => new { x.fsc_range, x.temp_id })
                .Select(group => new OAP0043Model
                {
                    fsc_range = group.Key.fsc_range,
                    temp_id = group.Key.temp_id,
                    check_amt = group.Sum(o => o.check_amt)
                }).ToList();


            OAP0043Model model = new OAP0043Model();

            try
            {
                if (rows.Count > 0) {
                    SysCodeDao sysCodeDao = new SysCodeDao();
                    Dictionary<string, string> typeMap = sysCodeDao.qryByTypeDic("AP", "OAP0042_TYPE");
                    model.type = rows[0].type;
                    if (typeMap.ContainsKey(model.type))
                        model.type_desc = typeMap[model.type];

                    CommonUtil commonUtil = new CommonUtil();
                    ADModel adModel = new ADModel();
                    adModel = commonUtil.qryEmp(rows[0].create_id);

                    model.create_id = rows[0].create_id + StringUtil.toString(adModel.name);
                    model.create_dt = rows[0].create_dt;

                    ViewBag.aply_no = aply_no;

                    ViewBag.bHaveData = "Y";
                    return View(model);
                }
            }
            catch (Exception e)
            {
                
            }

            ViewBag.bHaveData = "N";
            return View(model);
        }


        public List<OAP0043Model> getDispatchList(string type, string aply_no, string appr_stat) {
            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();

            List<OAP0043Model> dataList = new List<OAP0043Model>();

            List<OAP0043Model> rows = fAPTelCodeHisDao.qryByAplyNo(type, aply_no, appr_stat)
                .OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).ThenByDescending(x => x.amt_range).ToList();


            //dataList.Select(x => { x.amt_range = Convert.ToDecimal(x.amt_range_desc); return x; }).ToList();

            //依保局範圍
            List<OAP0043Model> rowsByStat = rows.GroupBy(x => new { x.fsc_range, x.amt_range })
                .Select(group => new OAP0043Model
                {
                    fsc_range = group.Key.fsc_range,
                    amt_range = group.Key.amt_range,
                    std_2 = group.Sum(o => Convert.ToDecimal(o.std_2)).ToString()
                }).ToList();


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            CommonUtil commonUtil = new CommonUtil();
            Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();


            List<string> amt_range_list = new List<string>();
            //查詢級距
            VeTelUtil veTelUtil = new VeTelUtil();
            rows = veTelUtil.getAmtRangeDesc(rows).OrderByDescending(x => x.amt_range.Length).ThenByDescending(x => x.amt_range).ThenByDescending(x => x.std_1).ToList();


            string fsc_range = "";
            foreach (OAP0043Model range in rowsByStat.OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).ThenByDescending(x => x.amt_range.Length).ThenByDescending(x => x.amt_range))
            {
                //保局範圍的total資料
                if (!fsc_range.Equals(range.fsc_range))
                {
                    OAP0043Model fsc_tot = new OAP0043Model();
                    fsc_tot.key = range.fsc_range;
                    fsc_tot.fsc_range = range.fsc_range;
                    fsc_tot.cnt = rows.Where(x => x.fsc_range == range.fsc_range).Sum(o => Convert.ToInt32(o.std_2));

                    if (fscRangeMap.ContainsKey(range.fsc_range))
                        fsc_tot.fsc_range_desc = fscRangeMap[range.fsc_range];
                    else
                        fsc_tot.fsc_range_desc = range.fsc_range;

                    dataList.Add(fsc_tot);
                    fsc_range = range.fsc_range;
                }

                int range_stat_cnt = rows.Where(x => x.fsc_range == range.fsc_range && x.amt_range == range.amt_range).Sum(o => Convert.ToInt32(o.std_2));

                foreach (OAP0043Model d in rows.Where(x => x.fsc_range == range.fsc_range && x.amt_range == range.amt_range))
                {
                    if (!empMap.ContainsKey(d.proc_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.proc_id);
                        empMap.Add(d.proc_id, adModel);
                    }
                    d.proc_name = empMap[d.proc_id].name;
                    d.cnt = range_stat_cnt;

                    dataList.Add(d);
                }

            }

            return dataList;
        }


        [HttpPost]
        public JsonResult detailRow(string type, string aply_no)
        {
            ViewBag.funcName = funcName;

            
            List<OAP0043Model> dataList = new List<OAP0043Model>();
            dataList = getDispatchList(type, aply_no, "1");


            var jsonData = new { success = true, rows = dataList };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OAP0043Model model, string apprStat)
        {

            if (StringUtil.toString(model.create_id).StartsWith(Session["UserID"].ToString()))
                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);



            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
                FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                DateTime now = DateTime.Now;

                try
                {
                    //處理駁回資料
                    if ("3".Equals(StringUtil.toString(apprStat)))
                    {
                        fAPTelCodeHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, model.aply_no, model.type, "", now, conn, transaction);
                        fAPTelCheckHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, model.aply_no, model.type, now, conn, transaction);

                    }



                    //處理核可資料
                    if ("2".Equals(StringUtil.toString(apprStat)))
                    {
                        //異動歷史檔
                        fAPTelCodeHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, model.aply_no, model.type, "", now, conn, transaction);
                        fAPTelCheckHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, model.aply_no, model.type, now, conn, transaction);

                        //異動正式檔
                        fAPTelCodeDao.delForOAP0043(model.type, conn, transaction);
                        fAPTelCodeDao.insertForOAP0043A(model.aply_no, model.type, conn, transaction);
                        fAPTelCheckDao.updForOAP0043A(model.aply_no, model.type, conn, transaction);

                        //若屬一年以上簡訊通知，多加寫踐行程序
                        if ("sms_assign_case".Equals(model.type)) {
                            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                            fAPVeTrackProcDao.insertTelCheck(model.type, model.aply_no, "G16", "F5 ", "email/簡訊通知", now, conn, transaction);

                            FAPVeTraceDao faPVeTraceDao = new FAPVeTraceDao();
                            faPVeTraceDao.updateForTelCheck(model.type, model.aply_no, "email/簡訊通知", now, conn, transaction);
                        }

                    }


                    transaction.Commit();




                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

                }
            }
        }





    }
}