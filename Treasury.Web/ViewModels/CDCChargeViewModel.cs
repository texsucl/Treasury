using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 權責單位異動資料
    /// </summary>
    public class CDCChargeViewModel
    {
        public CDCChargeViewModel() {
            DepositData = new CDCDepositViewModel();
        }

        /// <summary>
        /// 權責單位
        /// </summary>
        public List<CDCChargeModel> ChargeData { get; set; }

        /// <summary>
        /// 空白票據資料
        /// </summary>
        public List<CDCBillViewModel> BillData { get; set; }

        /// <summary>
        /// 電子憑證資料
        /// </summary>
        public List<CDCCAViewModel> CaData { get; set; }

        /// <summary>
        /// 定期存單資料
        /// </summary>
        public CDCDepositViewModel DepositData { get; set; }

        /// <summary>
        /// 不動產權狀資料
        /// </summary>
        public List<CDCEstateViewModel> EstateData { get; set; }

        /// <summary>
        /// 重要物品資料
        /// </summary>
        public List<CDCItemImpViewModel> ItemImpData { get; set; }

        /// <summary>
        /// 存出保證金資料
        /// </summary>
        public List<CDCMargingViewModel> MargingData { get; set; }

        /// <summary>
        /// 存入保證金資料
        /// </summary>
        public List<CDCMarginpViewModel> MarginpData { get; set; }

        /// <summary>
        /// 印章資料
        /// </summary>
        public List<CDCSealViewModel> SealData { get; set; }

        /// <summary>
        /// 股票資料
        /// </summary>
        public List<CDCStockViewModel> StockData { get; set; }

    }

    public class CDCChargeModel
    {
        /// <summary>
        /// 權責部門ID
        /// </summary>
        [Description("權責部門ID")]
        public string vCharge_Dept { get; set; }

        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCharge_Dept_Name { get; set; }

        /// <summary>
        /// 權責科別ID
        /// </summary>
        [Description("權責科別ID")]
        public string vCharge_Sect { get; set; }

        /// <summary>
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCharge_Sect_Name { get; set; }

        /// <summary>
        /// 金庫物品項目
        /// </summary>
        [Description("金庫物品項目")]
        public TreaItemType type { get; set; }
    }
}