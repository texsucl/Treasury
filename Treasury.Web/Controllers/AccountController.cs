using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.DirectoryServices;
using System.Security.Principal;
using Treasury.Web.Daos;
using Treasury.Web.Models;
using Treasury.WebViewModels;
using Treasury.WebDaos;
using Treasury.WebUtils;
using Treasury.WebBO;

/// <summary>
/// 功能說明：登出入作業
/// 初版作者：20170817 黃黛鈺
/// 修改歷程：20170817 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// </summary>

namespace Treasury.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 登入的 userID
        /// </summary>
        static public string CurrentUserId {
            get {
                var httpContext = System.Web.HttpContext.Current;                
                var _userId = httpContext.Session["UserID"];
                if (_userId != null)
                    return _userId.ToString();
                return null;
            }
        }

        /// <summary>
        /// 判斷是不是保管科
        /// </summary>
        static public bool CustodianFlag { get {
                var httpContext = System.Web.HttpContext.Current;
                var _Unit = httpContext.Session["UserUnit"];
                if(_Unit != null)
                   return _Unit.ToString() == Properties.Settings.Default["CustodianFlag"]?.ToString() ; //改為動態
                return false;
            }
        }

        public ActionResult Error()
        {
            return View();
        }



        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel loginModel)
        {
            logger.Info("[AccountController][Login]UserId:" + loginModel.UserId);
            bool hasuser = System.Web.HttpContext.Current.User != null;
            bool isAuthenticated = hasuser && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

 


            if (ModelState.IsValid)
            //if (isAuthenticated)
            {
                logger.Info("[AccountController][Login]IsValid" );
                this.HttpContext.Response.RemoveOutputCacheItem(Url.Action("MenuByUser", "NavigationController"));
                string ADPath = System.Configuration.ConfigurationManager.AppSettings.Get("ADPath");
                loginModel.UserId = loginModel.UserId.ToUpper();


                

                DirectoryEntry entry = new DirectoryEntry(ADPath, loginModel.UserId, loginModel.Password);
             
                try
                {
                    //string objectSid = (new SecurityIdentifier((byte[])entry.Properties["objectSid"].Value, 0).Value);

                    //AD驗證成功，檢查該user是否有系統權限
                    CodeUserDao codeUserDao = new CodeUserDao();


					CODE_USER codeUser = codeUserDao.qryUserByKey(loginModel.UserId);
                    if (codeUser != null)
                    {
                        if ("N".Equals(codeUser.IS_DISABLED)) {
                            
                            Session["UserID"] = loginModel.UserId;
                            //Session["AgentID"] = codeUser.CAGENTID;

                            Session["UserName"] = "侯蔚鑫";
                            Session["UserUnit"] = "VLX01";

                            OaEmpDao oaEmpDao = new OaEmpDao();
                            try
                            {
                                //using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                                //{
                                //    V_EMPLY2 emp = oaEmpDao.qryByUsrId(loginModel.UserId, dbIntra);
                                //    if (emp != null)
                                //    {
                                //        Session["UserName"] = StringUtil.toString(emp.EMP_NAME);
                                //        Session["UserUnit"] = StringUtil.toString(emp.DPT_CD);
                                //    }
                                //}
                            }
                            catch (Exception e)
                            {

                            }

                            writeLog("I", true, loginModel.UserId, codeUser);

                            LoginProcess(loginModel.UserId, false);

                            //System.Web.HttpContext context = System.Web.HttpContext.Current;
                            //SessionIDManager smgr = new SessionIDManager();
                            //string newId = smgr.CreateSessionID(context);
                            //string oldId = context.Session.SessionID;
                            //bool redirected = false;
                            //bool isAdded = false;
                            //smgr.SaveSessionID(context, newId, out redirected, out isAdded);


                            //string guid = Guid.NewGuid().ToString();
                            //string guid2 = Guid.NewGuid().ToString();
                            //Session["ASP.NET_SessionId"] = guid;
                            //// now create a new cookie with this guid value
                            //Response.Cookies["ASP.NET_SessionId"].Value = guid;
                            //Response.Cookies["adAuthCookie"].Value = guid2;
                            //Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", guid));
                            //Response.Cookies.Add(new HttpCookie("adAuthCookie", guid2));

                            return RedirectToAction("Index", "Home");
                        }
                    }

                    writeLog("I", false, loginModel.UserId, null);
                    ModelState.AddModelError("", "找不到這個使用者或登入帳號密碼失敗！");
                    return View(loginModel);
                    
                }
                catch(Exception e)
                {
                    logger.Error("[Login]其它錯誤：" + e.ToString());
                    writeLog("I", false, loginModel.UserId, null);
                    

                    //驗證失敗
                    ModelState.AddModelError("", "找不到這個使用者或登入帳號密碼失敗！");
                    return View(loginModel);
                }
                finally
                {
                    logger.Info("[Login]finally：" + loginModel.UserId);
                    //entry.Dispose();
                }
            }
            else {
                logger.Info("[Login](ModelState.IsValid=false)：" + loginModel.UserId);
                return View(loginModel);
            }

        }

        public ActionResult UserDashBoard()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout() {
            logger.Info("[AccountController][Logout]Session[UserID]:" + Session["UserID"]?.ToString());
            try
            {

                CodeUserDao codeUserDao = new CodeUserDao();
                CODE_USER codeUser = codeUserDao.qryUserByKey(Session["UserID"]?.ToString());

                writeLog("O", true, Session["UserID"]?.ToString(), codeUser);

                Session.Clear();
                Session.Abandon();

                //Response.Cookies["ASP.NET_SessionId"].Value = "";
                //Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-30);

                if (Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                    Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
                }

                if (Request.Cookies["adAuthCookie"] != null)
                {
                    Response.Cookies["adAuthCookie"].Value = string.Empty;
                    Response.Cookies["adAuthCookie"].Expires = DateTime.Now.AddMonths(-20);
                }



                ////建立一個同名的 Cookie 來覆蓋原本的 Cookie
                //HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                //cookie1.Expires = DateTime.Now.AddYears(-1);
                //Response.Cookies.Add(cookie1);

                ////建立 ASP.NET 的 Session Cookie 同樣是為了覆蓋
                //HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
                //cookie2.Expires = DateTime.Now.AddYears(-1);
                //Response.Cookies.Add(cookie2);
                TempData["Logout"] = "true";
                return RedirectToAction("Login");
            }
            catch (Exception e) {
                return RedirectToAction("Login");
                logger.Error("[AccountController][Logout]e:" + e.ToString());

            }


        }



        private void writeLog(String type, bool bSuccess, String userId, CODE_USER codeUser) {
            CommonUtil commonUtil = new CommonUtil();
            //logModel
            Log log = new Log();

            log.CFUNCTION = "I".Equals(type) ? "登入作業" : "登出作業";
            log.CACTION = "L";
            log.CCONTENT = "UserId：" + userId + "| UserName：" + commonUtil.GetIPAddress() + "|" + ("I".Equals(type) ? "登入成功" : "登出成功");

            //PiaLogMainModel

            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "B";
            piaLogMain.ACCESS_ACCOUNT = userId;
            piaLogMain.ACCOUNT_NAME = "";
            piaLogMain.PROGFUN_NAME = "AccountController";
            piaLogMain.EXECUTION_CONTENT = userId;
            piaLogMain.AFFECT_ROWS = 0;
            piaLogMain.PIA_TYPE = "0000000000";
            


            if (bSuccess)
            {
                CodeUserDao codeUserDao = new CodeUserDao();
                //更新login/logout日期時間
                if ("I".Equals(type))
                    codeUserDao.updateLogInOut(userId, "I");
                //codeUser.cLoginDateTime = DateTime.Now;
                else
                    codeUserDao.updateLogInOut(userId, "O");
                //codeUser.cLogoutDateTime = DateTime.Now;

               

                //寫入系統LOG

                LogDao.Insert(log, userId);

                //寫入稽核軌跡
                //piaLogMain.ACCOUNT_NAME = codeUser.CUSERNAME;
                piaLogMain.EXECUTION_TYPE = "I".Equals(type) ? "LS" : "LO";
                piaLogMain.ACCESSOBJ_NAME = "CodeUser";
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                piaLogMainDao.Insert(piaLogMain);


            }
            else {
                //寫入系統LOG
                log.CCONTENT = "UserId：" + userId + "| UserName：" + commonUtil.GetIPAddress() + "|" + "登入失敗";
                LogDao.Insert(log, userId);

                //寫入稽核軌跡
                piaLogMain.EXECUTION_TYPE = "LF";
                piaLogMain.ACCESSOBJ_NAME = "AD";
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                piaLogMainDao.Insert(piaLogMain);

            }
        }

        private void LoginProcess(string user, bool isRemeber)
        {
            var now = DateTime.Now;

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user,
                issueDate: now,
                expiration: now.AddMinutes(30),
                isPersistent: isRemeber,
                userData: user,
                cookiePath: FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);
        }

    }
}