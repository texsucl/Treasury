using Treasury.WebBO;
using System;
using Treasury.Web;

/// <summary>
/// 功能說明：共用 controller
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.WebControllers
{

    public class CommonController : BaseController
    {

        protected string GetopScope(string funcId)
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            String opScope = "";
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), funcId);
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = roleInfo[0];
            }
            return opScope;
        }

    }
}