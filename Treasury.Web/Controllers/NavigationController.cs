
using Treasury.WebViewModels;
using Treasury.WebUtils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Treasury.Web;

/// <summary>
/// 功能說明：功能選單
/// 初版作者：20170817 黃黛鈺
/// 修改歷程：20170817 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// </summary>
/// 

namespace Treasury.Web.Controllers
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


                        string strConn = DbUtil.GetDBTreasuryConnStr();

                        string sql = @"select  distinct func.FUNC_ID
      ,func.PARENT_FUNC_ID
      ,func.FUNC_NAME
      ,''
      ,func.FUNC_LEVEL
      ,func.FUNC_URL
      ,func.FUNC_ORDER
      ,role.USER_ID
      ,case FUNC_LEVEL when 1 then func.FUNC_ID else func.PARENT_FUNC_ID end  menuLevel
from CODE_USER_ROLE role,CODE_ROLE_FUNC rolefunc, CODE_FUNC func
where 1=1
and role.USER_ID = @userId
and role.ROLE_ID = rolefunc.ROLE_ID
and rolefunc.FUNC_ID = func.FUNC_ID
and func.IS_DISABLED = 'N'

union 

select  distinct func.FUNC_ID
      ,func.PARENT_FUNC_ID
      ,func.FUNC_NAME
      ,''
      ,func.FUNC_LEVEL
      ,func.FUNC_URL
      ,func.FUNC_ORDER
      ,@userId
      ,case FUNC_LEVEL when 1 then func.FUNC_ID else func.PARENT_FUNC_ID end  menuLevel
from CODE_FUNC func
where 1=1
and func.FUNC_ID in (
select distinct func.PARENT_FUNC_ID
from CODE_USER_ROLE role,CODE_ROLE_FUNC rolefunc, CODE_FUNC func
where 1=1
and role.USER_ID = @userId
and role.ROLE_ID = rolefunc.ROLE_ID
and rolefunc.FUNC_ID = func.FUNC_ID
and func.IS_DISABLED = 'N'
)


order by menuLevel, func.PARENT_FUNC_ID , func.FUNC_ORDER
";

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            String menuLevelO = "";

                            conn.Open();
                            SqlCommand command = conn.CreateCommand();
                            command.CommandType = CommandType.Text;
                            command.Connection = conn;
                            command.CommandTimeout = 0;
                            command.CommandText = sql;
                            command.Parameters.AddWithValue("@userId", Session["UserID"].ToString());


                            MenuModel menuModel = new MenuModel();
                            using (SqlDataReader dr = command.ExecuteReader())
                            {

                                menuModel.SubMenu = new List<MenuModel>();

                                while (dr.Read())
                                {
                                    logger.Info("[NavigationController][MenuByUser]dr[2].ToString():" + dr[2].ToString());
                                    if (!menuLevelO.Equals(dr[8].ToString()) & !"".Equals(menuLevelO))
                                    {
                                        menuViewModel.Add(menuModel);

                                        menuModel = new MenuModel();
                                        menuModel.SubMenu = new List<MenuModel>();
                                    }

                                    if ("".Equals(dr[5].ToString()))
                                    {
                                        menuModel.MenuID = dr[0].ToString();
                                        menuModel.Title = dr[2].ToString();
                                        //menuModel.Action = "test1";
                                        // menuModel.Controller = "Home";
                                        menuModel.Link = dr[5].ToString();
                                        menuModel.IsAction = false;
                                    }
                                    else
                                    {
                                        MenuModel subMenuModel = new MenuModel();
                                        subMenuModel.MenuID = dr[0].ToString();
                                        subMenuModel.Title = dr[2].ToString();
                                        //subMenuModel.Action = "test1";
                                        // subMenuModel.Controller = "Home";
                                        subMenuModel.Link = dr[5].ToString();
                                        subMenuModel.IsAction = true;

                                        menuModel.SubMenu.Add(subMenuModel);
                                    }

                                    string test = dr[8].ToString();
                                    menuLevelO = dr[8].ToString();
                                }
                            }
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
    }
}