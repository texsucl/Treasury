using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Service.Interface;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{
    /// <summary>
    /// 簡單工廠模式
    /// </summary>
    public class SampleFactory
    {
        /// <summary>
        /// 回傳 類型
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public IAgency GetAgenct(TreaItemType itemId)
        {
            switch (itemId)
            {
                case TreaItemType.D1012:
                    return new Bill();
                
            }
            return null;
        }


    }
}