using System;
using System.ComponentModel;	

namespace Treasury.Web.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 存取項目表單狀態
        /// </summary>
        public enum AccessProjectFormStatus
        {
            /// <summary>
            /// 表單申請
            /// </summary>
            [Description("表單申請")]
            A01,

            /// <summary>
            /// 申請單位覆核駁回
            /// </summary>
            [Description("申請單位覆核駁回")]
            A02,

            /// <summary>
            /// 保管科承辦覆核駁回
            /// </summary>
            [Description("保管科承辦覆核駁回")]
            A03,

            /// <summary>
            /// 金庫人員退回申請單位
            /// </summary>
            [Description("金庫人員退回申請單位")]
            A04,

            /// <summary>
            /// 表單重新申請
            /// </summary>
            [Description("表單重新申請")]
            A05,

            /// <summary>
            /// 保管科覆核駁回
            /// </summary>
            [Description("保管科覆核駁回")]
            A06,

            /// <summary>
            /// 申請單位完成覆核，保管科承辦確認中
            /// </summary>
            [Description("申請單位完成覆核，保管科承辦確認中")]
            B01,

            /// <summary>
            /// 保管科覆核中
            /// </summary>
            [Description("保管科覆核中")]
            B02,

            /// <summary>
            /// 金庫人員退回保管科承辦，保管科承辦確認中
            /// </summary>
            [Description("金庫人員退回保管科承辦，保管科承辦確認中")]
            B03,

            /// <summary>
            /// 保管科覆核退回，保管科確認中
            /// </summary>
            [Description("保管科覆核退回，保管科確認中")]
            B04,


            /// <summary>
            /// 保管科覆核完成覆核，待入庫人員確認中
            /// </summary>
            [Description("保管科覆核完成覆核，待入庫人員確認中")]
            C01,

            /// <summary>
            /// 已入庫確認
            /// </summary>
            [Description("已入庫確認")]
            C02,

            /// <summary>
            /// 金庫登記簿檢核
            /// </summary>
            [Description("金庫登記簿檢核")]
            D01,

            /// <summary>
            /// 金庫登記簿覆核中
            /// </summary>
            [Description("金庫登記簿覆核中")]
            D02,

            /// <summary>
            /// 金庫登記簿覆核完成
            /// </summary>
            [Description("金庫登記簿覆核完成")]
            D03,

            /// <summary>
            /// 金庫登記簿覆核退回
            /// </summary>
            [Description("金庫登記簿覆核退回")]
            D04,

            /// <summary>
            /// 已完成出入庫，通知申請人員
            /// </summary>
            [Description("已完成出入庫，通知申請人員")]
            E01,

            /// <summary>
            /// 申請人作廢
            /// </summary>
            [Description("申請人作廢")]
            E02,

            /// <summary>
            /// 申請單位退回作廢
            /// </summary>
            [Description("申請單位退回作廢")]
            E03,

            /// <summary>
            /// 金庫人員退回作業
            /// </summary>
            [Description("金庫人員退回作廢")]
            E04,
        }
    }

}