using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    public interface IOAP0021
    {
        /// <summary>
        /// 查詢 應付票據變更接收作業
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        List<OAP0021Model> Search_OAP0021(OAP0021SearchModel searchData);

        /// <summary>
        /// 查詢 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        List<OAP0021Model> Search_OAP0021A(OAP0021ASearchModel searchData, string userId);

        /// <summary>
        /// 應付票據接收檔 明細 
        /// </summary>
        /// <param name="apply_no">申請單號</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel<OAP0021DetailModel> GetDetailData(string apply_no, string userId = null);

        /// <summary>
        /// 應付票據接收檔 之票明細 by 支票號碼
        /// </summary>
        /// <param name="resultModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel getSubData(List<OAP0021DetailSubModel> resultModel);

        /// <summary>
        /// 接收應付票據變更申請檔
        /// </summary>
        /// <param name="updateModel">接收資料</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel RECEFAPPYCH0(OAP0021DetailModel updateModel, string userId);

        /// <summary>
        /// 退回應付票據變更申請檔
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel REJFAPPYCH0(OAP0021DetailModel updateModel, string userId);

        /// <summary>
        /// 應付票據變更申請檔 補件中案件
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel ADFAPPYCH0(OAP0021DetailModel updateModel, string userId);

        /// <summary>
        /// 執行 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="apprModel">待處理資料</param>
        /// <param name="userId"></param>
        /// <param name="flag">true = 核准 false = 駁回</param>
        /// <returns></returns>
        MSGReturnModel UpdateOAP0021A(List<OAP0021Model> apprModel, string userId, bool flag);

        /// <summary>
        /// 查詢業務人員資料 id , 姓名 , unit_id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Tuple<string, string, string> callEBXGXFK(string userId);
    }
}
