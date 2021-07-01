using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAP.Web.Service.Interface
{
    interface IBAP0004
    {
        /// <summary>
        /// 比對出已達追蹤條件的資料，寄送追蹤報表給相關追蹤人員 
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <param name="type">執行類別 A:全部執行,M:僅執行寄送mail,E:僅執行產生Excel</param>
        /// <returns></returns>
        Tuple<bool, string, string> VE_Clear_Scheduler(string userId = null, string type = "A");

        /// <summary>
        /// 比對資料，查已達N個月需重新派件的資料
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        Tuple<bool, string> VE_Clear_ReDispatch(string userId = null);

        /// <summary>
        ///  比對資料，查已達N個月未聯繫的資料
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        Tuple<bool, string> VE_Clear_Cust(string userId = null);

        /// <summary>
        /// 比對資料，將符合設定期間的簡訊資訊清空
        /// </summary>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        Tuple<bool, string> SMS_Clear(string userId = null);

        /// <summary>
        /// 產生"清理暨逾期處理清單"
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type">執行類別 A:全部執行,M:僅執行寄送mail,E:僅執行產生Excel</param>
        /// <returns></returns>
        Tuple<bool, string, string> VE_Level_Detail(string userId = null, string type = "A");
    }
}
