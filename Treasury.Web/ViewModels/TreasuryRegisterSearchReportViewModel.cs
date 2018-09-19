using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class TreasuryRegisterSearchReportViewModel
    {
        /// <summary>
        /// 查詢結果資料檔
        /// </summary>
        [Description("查詢結果資料檔")]
        public List<TreasuryRegisterSearch> vSearch { get; set; }

        /// <summary>
        /// 金庫登記簿明細檔
        /// </summary>
        [Description("查詢結果資料檔")]
        public List<TreasuryRegisterDetail> vDetail { get; set; }
    }

    public class TreasuryRegisterSearch
    {
        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOpen_Trea_Type { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTrea_Register_Id { get; set; }

        /// <summary>
        /// 入庫日期
        /// </summary>
        [Description("入庫日期")]
        public string vCreate_Dt { get; set; }

        /// <summary>
        /// 開庫時間
        /// </summary>
        [Description("開庫時間")]
        public string vOpen_Trea_Time { get; set; }

        /// <summary>
        /// 實際入庫時間
        /// </summary>
        [Description("實際入庫時間")]
        public string vActual_Put_Time { get; set; }

        /// <summary>
        /// 實際出庫時間
        /// </summary>
        [Description("實際出庫時間")]
        public string vActual_Get_Time { get; set; }

        /// <summary>
        /// 登記簿狀態
        /// </summary>
        [Description("登記簿狀態")]
        public string vRegi_Status { get; set; }
    }

    public class TreasuryRegisterDetail
    {
        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vItem_Op_Type { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vItem_Desc { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSeal_Desc { get; set; }

        /// <summary>
        /// 作業別
        /// </summary>
        [Description("作業別")]
        public string vAccess_Type { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vAccess_Reason { get; set; }

        /// <summary>
        /// 入庫人員
        /// </summary>
        [Description("入庫人員")]
        public string vConfirm { get; set; }

        /// <summary>
        /// 實際作業別
        /// </summary>
        [Description("實際作業別")]
        public string vActual_Access_Type { get; set; }

        /// <summary>
        /// 實際入庫人員
        /// </summary>
        [Description("實際入庫人員")]
        public string vActual_Access_Uid { get; set; }
    }
}