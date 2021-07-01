using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using SSO.Web;
using System.Reflection;
using System.Linq;
using SSO.Web.Daos;
using SSO.Web.ViewModels;
using SSO.Web.BO;
using SSO.Web.Utils;

/// <summary>
/// 功能說明：功能選單
/// 初版作者：20170817 黃黛鈺
/// 修改歷程：20170817 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// </summary>
/// 

namespace SSO.WebControllers
{


    public class NavigationController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult MenuDefault()
        {
            logger.Info("[NavigationController][MenuDefault]");
            List<MenuModel> menuViewModel = new List<MenuModel>();

            
            
            return PartialView("_Navigation", menuViewModel);
        }


        public ActionResult MenuByUser()
        {
            CodeSysInfo codeSysInfo = new CodeSysInfo();
            Dictionary<string, string> sysUrl = new Dictionary<string, string>();
            sysUrl = codeSysInfo.qryUrlDic();
            string bDev = System.Configuration.ConfigurationManager.AppSettings.Get("bDev");
            string devSSOUrl = System.Configuration.ConfigurationManager.AppSettings.Get("DevSSOUrl");
            string devGLUrl = System.Configuration.ConfigurationManager.AppSettings.Get("DevGLUrl");
            string devRTUrl = System.Configuration.ConfigurationManager.AppSettings.Get("DevRTUrl");
            string devAPUrl = System.Configuration.ConfigurationManager.AppSettings.Get("DevAPUrl");
            if ("Y".Equals(bDev)) {
                sysUrl["SSO"] = devSSOUrl;
                sysUrl["GL"] = devGLUrl;
                sysUrl["RT"] = devRTUrl;
                sysUrl["AP"] = devAPUrl;
            }


            List<MenuModel> menuViewModel = new List<MenuModel>();
            logger.Info("[NavigationController][MenuByUser]" );
            try
            {

                logger.Info("[NavigationController][MenuByUser]UserID:" + Session["UserID"]);
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Account");
                    //return PartialView("_Navigation", menuViewModel);
                }
                else
                {
                    if ("".Equals(Session["UserID"])) {
                        return RedirectToAction("Login", "Account");
                    }
                   // logger.Info("[NavigationController][MenuByUser]UserID" + Session["UserID"]);
                    if (Session["menu"] == null)
                    {

                        Dictionary<string, MenuModel> pMenu = new Dictionary<string, MenuModel>();

                        string strConn = DbUtil.GetDBFglConnStr();

                        string sql = @"select  distinct func.FUNC_ID MenuID
                              ,func.PARENT_FUNC_ID
                              ,func.FUNC_NAME Title
                              ,func.SYS_CD
                              ,func.FUNC_LEVEL  FUNC_LEVEL
                              ,func.FUNC_URL Link
                              ,func.FUNC_ORDER FUNC_ORDER
                              ,role.USER_ID
                              ,case FUNC_LEVEL when 1 then func.FUNC_ID else func.PARENT_FUNC_ID end  menuLevel
                        from CODE_USER_ROLE role,CODE_ROLE_FUNC rolefunc, CODE_FUNC func, CODE_ROLE codeR
                        where 1=1
                        and role.USER_ID = @userId
                        and role.ROLE_ID = rolefunc.ROLE_ID
                        and rolefunc.FUNC_ID = func.FUNC_ID
                        and func.IS_DISABLED = 'N'
                        and role.ROLE_ID = codeR.ROLE_ID
                        and codeR.IS_DISABLED = 'N'
                        ";

                        string sqlP = @"select  distinct func.FUNC_ID MenuID
                              ,func.PARENT_FUNC_ID
                              ,func.FUNC_NAME Title
                              ,func.SYS_CD
                              ,func.FUNC_LEVEL  FUNC_LEVEL
                              ,func.FUNC_URL Link
                              ,func.FUNC_ORDER FUNC_ORDER
                              ,@userId
                              ,case FUNC_LEVEL when 1 then func.FUNC_ID else func.PARENT_FUNC_ID end  menuLevel
                        from CODE_FUNC func
                        where 1=1
                        and func.FUNC_URL = ''
                        order by menuLevel, func.PARENT_FUNC_ID , func.FUNC_ORDER
                        ";

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            //String menuLevelO = "";

                            conn.Open();
                            SqlCommand command = conn.CreateCommand();
                            command.CommandType = CommandType.Text;
                            command.Connection = conn;
                            command.CommandTimeout = 0;
                            command.CommandText = sql;
                            command.Parameters.AddWithValue("@userId", Session["UserID"].ToString());

                            MenuModel menuModel = new MenuModel();

                            List<MenuModel> rows = new List<MenuModel>();

                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    MenuModel t = new MenuModel();

                                    for (int inc = 0; inc < dr.FieldCount; inc++)
                                    {
                                        Type type = t.GetType();
                                        PropertyInfo prop = type.GetProperty(dr.GetName(inc));

                                        try
                                        {
                                            prop.SetValue(t, dr.GetValue(inc), null);
                                        }
                                        catch (Exception e) {

                                        }

                                    }

                                    rows.Add(t);
                                }
                            }

                            //查詢父節點
                            command.CommandText = sqlP;
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    MenuModel t = new MenuModel();

                                    for (int inc = 0; inc < dr.FieldCount; inc++)
                                    {
                                        Type type = t.GetType();
                                        PropertyInfo prop = type.GetProperty(dr.GetName(inc));

                                        try
                                        {
                                            prop.SetValue(t, dr.GetValue(inc), null);
                                        }
                                        catch (Exception e)
                                        {

                                        }

                                    }

                                    pMenu.Add(t.MenuID, t);
                                }
                            }

                            if (rows.Count > 0) {
                                foreach (MenuModel child in rows.GroupBy(x => new { x.PARENT_FUNC_ID }).Select(group => new MenuModel { PARENT_FUNC_ID = group.Key.PARENT_FUNC_ID }).ToList<MenuModel>())
                                    rows = qryParent(rows, pMenu, child.PARENT_FUNC_ID);
                            }
                            



                            menuModel.MenuID = "0";
                            menuModel.FUNC_LEVEL = 0;
 

                            menuModel = ChildrenOf(rows, menuModel, sysUrl);

                            menuViewModel.Add(menuModel);


                            Session["menu"] = menuViewModel;

                        }
                    }
                    else
                    {
                        
                        menuViewModel = (List<MenuModel>)Session["menu"];
                        logger.Info("[NavigationController][MenuByUser]menu" + Session["menu"]);
                    }



                    return PartialView("_Navigation", menuViewModel);
                }
            }
            catch (Exception e)
            {

                logger.Error("[NavigationController][MenuByUser]e:" + e.ToString());

                ViewBag.status = "其它錯誤，請洽系統管理人員";
                return View("~/Views/Shared/Error.cshtml");
            }
            
                //return Content("cache invalidated, you could now go back to the index action");
            
        }

        public static MenuModel ChildrenOf(List<MenuModel> rows, MenuModel menu, Dictionary<string, string> sysUrl)
        {
            foreach (MenuModel child in rows.Where(x => x.PARENT_FUNC_ID.Trim() == menu.MenuID).OrderBy(x => x.FUNC_ORDER))
            {
                MenuModel item = new MenuModel();
                item.MenuID = child.MenuID;
                item.Title = child.Title;
                item.PARENT_FUNC_ID = child.PARENT_FUNC_ID;

                if (sysUrl.ContainsKey(child.SYS_CD)) {
                    item.Link = child.Link.Trim().Replace("~", StringUtil.toString(sysUrl[child.SYS_CD]));
                }
                    
                else
                    item.Link = child.Link.Trim();

                menu.SubMenu.Add(ChildrenOf(rows, item, sysUrl));

            }

            return menu;
        }

        public static List<MenuModel> qryParent(List<MenuModel> rows, Dictionary<string, MenuModel> pMenu, string nowPFuncId)
        {
            if (!rows.Exists(x => x.MenuID == nowPFuncId)) {
                if (pMenu.ContainsKey(nowPFuncId))
                {
                    rows.Add(pMenu[nowPFuncId]);
                    rows = qryParent(rows, pMenu, pMenu[nowPFuncId].PARENT_FUNC_ID);
                }

            }
           

            return rows;
        }

    }
}