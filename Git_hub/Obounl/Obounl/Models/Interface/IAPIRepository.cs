using Obounl.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obounl.Models.Interface
{
    interface IAPIRepository
    {
        /// <summary>
        /// 服務人員收集完成合約資料後如需申請即時電訪，需先將資料以API方式傳送至電訪系統中建立電訪名單
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        MSGReturnModel<string> Contract_Data_Insert(string data);
    }
}
