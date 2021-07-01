using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    public interface IOAP0023
    {
        /// <summary>
        /// 查詢 應付票據–轉入支票簽收 (簽收資料個案產出)
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        List<OAP0023Model> Search_OAP0023(OAP0023SearchModel searchModel);

        /// <summary>
        /// 執行轉入新增 FAPPYSN0應付票據簽收檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId">執行人員</param>
        /// <param name="apt_id">簽收窗口</param>
        /// <param name="dep_id">簽收窗口單位</param>
        /// <returns></returns>
        MSGReturnModel InsertFAPPYSN0(List<OAP0023Model> model, string userId, string apt_id, string dep_id);

        /// <summary>
        /// 獲取 支票收件部門明細
        /// </summary>
        /// <returns></returns>
        SelectList GetDepGroup();
    }
}
