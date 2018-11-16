using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class CDCSearchViewModel 
    {
        /// <summary>
        /// 金庫內
        /// </summary>
        [Description("金庫內")]
        public string vTreasuryIO { get; set; }

        /// <summary>
        /// 作業項目
        /// </summary>
        [Description("作業項目")]
        public string vJobProject { get; set; }

        /// <summary>
        /// 入庫日期(起)
        /// </summary>
        [Description("入庫日期(起)")]
        public string vAPLY_DT_From { get; set; }

        /// <summary>
        /// 入庫日期(迄)
        /// </summary>
        [Description("入庫日期(迄)")]
        public string vAPLY_DT_To { get; set; }

        /// <summary>
        /// 入庫日期(起)
        /// </summary>
        [Description("出庫日期(起)")]
        public string vAPLY_ODT_From { get; set; }

        /// <summary>
        /// 入庫日期(迄)
        /// </summary>
        [Description("出庫日期(迄)")]
        public string vAPLY_ODT_To { get; set; }

        /// <summary>
        /// 冊號(不動產)
        /// </summary>
        [Description("冊號(不動產)")]       
        public string vBookNo { get; set; }

        /// <summary>
        /// 狀別(不動產)
        /// </summary>
        [Description("狀別(不動產)")]
        public string vEstate_Form_No { get; set; }

        /// <summary>
        /// 編號(股票)
        /// </summary>
        [Description("編號(股票)")]
        public string vName { get; set; }

        /// <summary>
        /// 承作日期(定期存單)
        /// </summary>
        [Description("承作日期(定期存單)")]
        public string vCommit_Date { get; set; }

        /// <summary>
        /// 到期日(定期存單)
        /// </summary>
        [Description("到期日(定期存單)")]
        public string vExpiry_Date  { get; set; }

        /// <summary>
        /// 交易對象(定期存單)
        /// </summary>
        [Description("交易對象(定期存單)")]
        public string vTRAD_Partners { get; set; }

        /// <summary>
        /// 歸檔編號(存入保證金,存出保證金,重要物品)
        /// </summary>
        [Description("歸檔編號(存入保證金,存出保證金,重要物品)")]
        public string vItem_No { get; set; }

        /// <summary>
        /// 冊號(存入保證金,存出保證金)
        /// </summary>
        [Description("冊號(存入保證金,存出保證金)")]
        public string vItem_Book_No { get; set; }

        /// <summary>
        /// 類別(存入保證金,存出保證金)
        /// </summary>
        [Description("類別(存入保證金,存出保證金)")]
        public string vMargin_Dep_Type { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        [Description("新增人員")]
        public string vCreate_Uid { get; set; }

        public string CHARGE_DEPT_AFT { get; set; }

        public string CHARGE_SECT_AFT { get; set; }
    }
}