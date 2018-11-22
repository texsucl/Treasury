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
                case TreaItemType.D1017:
                    return new Marginp();
                case TreaItemType.D1018:
                    return new ItemImp();
            }
            return null;
        }

        /// <summary>
        /// 回傳 類型
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ICDCAction GetCDCAction(TreaItemType itemId)
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
                case TreaItemType.D1017:
                    return new Marginp();
                case TreaItemType.D1018:
                    return new ItemImp();
            }
            return null;
        }

        /// <summary>
        /// 回傳 類型
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ITinAction GetTDAAction(DefinitionType itemId)
        {
            switch (itemId)
            {
                case DefinitionType.TREA_ITEM:
                    return new ItemMaintain();
                case DefinitionType.TREA_EQUIP:
                    return new TreasuryMaintain();
                case DefinitionType.MAIL_CONTENT:
                    return new TreasuryMailContent();
                case DefinitionType.MAIL_TIME:
                    return new TreasuryMailTime();
                case DefinitionType.ITEM_CHARGE_UNIT:
                    return new ItemChargeUnit();
                case DefinitionType.DEP_CHK_ITEM:
                    return new DepChkItem();
            }
            return null;
        }
    }
}