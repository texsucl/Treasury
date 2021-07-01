using Obounl.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obounl.Models.Interface
{
    interface IWebRepository
    {
        /// <summary>
        /// 目前線上等候電訪人數共計○人(類掛號人數功能)
        /// </summary>
        /// <returns></returns>
        int Waiting_Number();

        /// <summary>
        /// 取得COL70即時電訪最大可供人力
        /// </summary>
        /// <returns></returns>
        int ReturnCapacity();

        /// <summary>
        /// 是否於服務時間
        /// </summary>
        /// <returns></returns>
        bool IsServicing();

        /// <summary>
        /// 取得服務時段
        /// </summary>
        /// <returns></returns>
        tblSysCode GetServicTime();

        /// <summary>
        /// 即時電訪服務_確認送出功能
        /// </summary>
        /// <param name="caseNo">要保書編號</param>
        /// <param name="userId">業務員</param>
        /// <param name="custID">客戶ID</param>
        /// <returns></returns>
        MSGReturnModel<string> InstantCall_Confirm(string caseNo, string userId, List<string> custID);

        /// <summary>
        /// 預約電訪/進度查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<DataViewModel> GetSearchModel(DataSearchModel searchModel);

        /// <summary>
        /// 即時電訪 查詢客戶資料
        /// </summary>
        /// <param name="caseNo"></param>
        /// <param name="userId"></param>
        /// <param name="model"></param>
        void getCust(string caseNo, string userId, InstantCallViewModel model);
    }
}
