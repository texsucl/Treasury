using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class EstateDetailViewModel 
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// 群組編號
        /// </summary>
        [Description("群組編號")]
        public string vGroupNo { get; set; }

        /// <summary>
        /// 狀別
        /// </summary>
        [Description("狀別")]       
        public string vEstate_From_No { get; set; }

        /// <summary>
        /// 發狀日
        /// </summary>
        [Description("發狀日")]
        public string vEstate_Date { get; set; }

        /// <summary>
        /// 字號
        /// </summary>
        [Description("字號")]
        public string vOwnership_Cert_No { get; set; }

        /// <summary>
        /// 地/建號
        /// </summary>
        [Description("地/建號")]
        public string vLand_Building_No { get; set; }

        /// <summary>
        /// 門牌號
        /// </summary>
        [Description("門牌號")]
        public string vHouse_No{ get; set; }

        /// <summary>
        /// 流水號/編號
        /// </summary>
        [Description("流水號/編號")]
        public string vEstate_Seq { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 取出註記
        /// </summary>
        [Description("取出註記")]
        public bool vtakeoutFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}