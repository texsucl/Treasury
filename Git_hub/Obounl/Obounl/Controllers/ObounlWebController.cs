using Obounl.Infrastructure;
using Obounl.Models.Model;
using Obounl.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Cors;
using Obounl.Daos;
using Obounl.Models.Interface;
using Obounl.Models.Repository;

namespace Obounl.Controllers
{
    public class ObounlWebController : Controller
    {
        private IWebRepository WebRepository;
        private string domain = string.Empty;

        public ObounlWebController()
        {
            WebRepository = new WebRepository();
        }

        // GET: ObounlWeb
        public ActionResult Index()
        {

            return View();
        }


        /// <summary>
        /// 預約電訪/進度查詢 查詢畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [MVCBrowserEvent("預約電訪/進度查詢 查詢畫面", false, true)]
        public ActionResult Search(string agentID, string token)
        {
            //domain = Request.getDomain();
            var _Flag = checkToken(token);
            ViewBag.Flag = _Flag;
            ViewBag.msg = !_Flag ? "token認證無效!" : string.Empty;
            ViewBag.agentID = agentID;
            var _tokenKey = string.Empty;
            if (_Flag)
                _tokenKey = Extension.getTokenKey().Item2;
            ViewBag.tokenKey = _tokenKey;
            //ViewBag.domain = domain;
            return PartialView();
        }

        /// <summary>
        /// 預約電訪/進度查詢 結果畫面
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        [MVCBrowserEvent("預約電訪/進度查詢 結果畫面", false, true)]
        public ActionResult Data(DataSearchModel searchModel)
        {
            domain = Request.getDomain();
            DateTime dtn = DateTime.Now;
            List<DataViewModel> datas = new List<DataViewModel>();
            var _Flag = checkToken(searchModel.token);
            var _token = string.Empty;
            var _agentID = searchModel.agentID;
            if (_Flag)
            {
                searchModel.agentID = searchModel.agentID.AESDecrypt().Item2; //業務人員ID解密
                var Edate = searchModel.Edate;
                DateTime dt = DateTime.MinValue;
                if (!Edate.IsNullOrWhiteSpace() && DateTime.TryParse(Edate, out dt))
                {
                    searchModel.Edate = dt.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy-MM-dd HH:mm:ss");
                }
                datas = WebRepository.GetSearchModel(searchModel);
                _token = Extension.getTokenKey().Item2;
            }
            ViewBag.Flag = _Flag;
            ViewBag.msg = !_Flag ? "token認證無效!" : string.Empty;
            ViewBag.agentID = _agentID;
            ViewBag.tokenKey = _token;
            return PartialView(datas);
        }


        /// <summary>
        /// 申請即時電訪服務
        /// </summary>
        /// <param name="CaseNo"></param>
        /// <returns></returns>
        [HttpPost]
        [MVCBrowserEvent("申請即時電訪服務", false, true)]
        //public ActionResult InstantCall([System.Web.Http.FromBody] string CaseNo = null)
        public ActionResult InstantCall(string CaseNo, string token)
        {
            domain = Request.getDomain();
            var model = new InstantCallViewModel();
            var _count = WebRepository.Waiting_Number(); //線上等候人數
            bool flag = checkToken(token);
            var _msg = !flag ? "token認證無效!" : string.Empty;
            var _Situation = "Situation_1";
            var _type = "1";
            var _CaseNo = CaseNo;
            CaseNo = CaseNo.AESDecrypt().Item2;
            switch (_type)
            {
                case "1":
                    _Situation = "Situation_1";
                    model.TopMemo = $@"目前線上等候電訪人數共計 {_count} 人，";
                    model.MemoData = new List<InstantCallSubMemoViewModel>()
                                {
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "1",
                                        Memo = @"點選【確認送出】將於10分鐘內有專人儘速與保戶聯絡，請保戶留意要保文件留存電話。若10分鐘內公司仍無法成功與保戶聯絡，將待公司受理後另行安排電訪。"
                                    },
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "2",
                                        Memo = @"點選【取消申請】或未選取的電訪對象將待公司受理後另行安排電訪。"
                                    }
                                };
                    break;
                case "2":
                    _Situation = "Situation_2";
                    model.TopMemo = "目前所有電訪人員均在忙線中";
                    model.MemoData = new List<InstantCallSubMemoViewModel>()
                                {
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "1",
                                        Memo = @"點選【預約電訪】進入電訪可預約時段(09:00-21:00)。若已屆預約時間仍聯絡失敗，將取消本次預約作業，待案件受理後公司另行安排電訪。"
                                    },
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "2",
                                        Memo = @"點選【取消申請】將待案件受理後，另行安排電訪。"
                                    },
                                };
                    break;
                case "3":
                    _Situation = "Situation_3";
                    model.TopMemo = "即時電訪服務時間：周一至周五 09:00~17:30（例假日、國定假日無提供";
                    model.MemoData = new List<InstantCallSubMemoViewModel>()
                                {
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "1",
                                        Memo = @"點選【預約電訪】進入電訪可預約時段(09:00-21:00)。若已屆預約時間仍聯絡失敗，將取消本次預約作業，待案件受理後公司另行安排電訪。"
                                    },
                                    new InstantCallSubMemoViewModel()
                                    {
                                        Num = "2",
                                        Memo = @"點選【取消申請】將待案件受理後，另行安排電訪。"
                                    },
                                };
                    break;
            }
            WebRepository.getCust(CaseNo, "", model); //加入客戶資料
            model.CaseNo = _CaseNo;
            model.Flag = flag;
            model.ErrorMsg = _msg;
            model.Situation = _Situation;
            //ViewBag.domain = domain;

            var service = WebRepository.GetServicTime();

            ViewBag.ServiceTime = String.Format("{0}:{1}~{2}:{3}",
                String.Join("", service.SMemo1.Take(2)),
                String.Join("", service.SMemo1.Skip(2).Take(2)),
                String.Join("", service.SMemo2.Take(2)),
                String.Join("", service.SMemo2.Skip(2).Take(2)));

            return PartialView(model);
        }

        [HttpPost]
        [MVCBrowserEvent("即時電訪服務_確認送出功能")]
        public JsonResult Confrim(string CaseNo, List<string> CustID)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            // 確認服務時間
            if (!WebRepository.IsServicing())
            {
                result.REASON_CODE = "01";
                return Json(result);
            }

            // 確認人力
            var count = WebRepository.Waiting_Number();     // 線上等候人數
            var capacity = WebRepository.ReturnCapacity();  // COL70即時電訪最大可供人力

            if (count >= capacity)
            {
                result.REASON_CODE = "02";
                return Json(result);
            }

            // 確認電訪對象
            if (CustID == null || !CustID.Any())
            {
                result.DESCRIPTION = "最少需要有一個電訪對象。";
            }
            else
            {
                List<string> _CustID = new List<string>();
                var _CaseNo = CaseNo.AESDecrypt().Item2; //要保書編號解密
                CustID.ForEach(x =>
                {
                    _CustID.Add(x.AESDecrypt().Item2); //客戶ID解密
                });
                result = WebRepository.InstantCall_Confirm(_CaseNo, "", _CustID);
            }
            return Json(result);
        }

        [MVCBrowserEvent("即時電訪服務_結束導頁功能")]
        public ActionResult Done()
        {
            return View();
        }

        private bool checkToken(string token)
        {
            var _Auth = false;
            DateTime _token_dtm = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var _token = token.AESDecrypt().Item2;
                DateTime.TryParseExact(_token, "yyyy/MM/dd HH:mm:ss", null,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                out _token_dtm);
                if (_token_dtm >= DateTime.Now)
                    _Auth = true;
            }
            return _Auth;
        }
    }
}