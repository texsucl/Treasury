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
                case TreaItemType.D1008:
                case TreaItemType.D1009:
                case TreaItemType.D1010:
                case TreaItemType.D1011:
                    return new Seal();
                case TreaItemType.D1014:
                    return new Estate();
                case TreaItemType.D1024:
                    return new CA();
                case TreaItemType.D1015:
                    return new Stock();
                case TreaItemType.D1016:
                    return new Marging();
                case TreaItemType.D1013:
                    return new Deposit();
            }
            return null;
        }


    }
}