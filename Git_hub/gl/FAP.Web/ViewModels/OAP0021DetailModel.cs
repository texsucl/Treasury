using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FAP.Web.ViewModels
{
    /// <summary>
    /// 應付票據變更接收明細Model
    /// </summary>
    public class OAP0021DetailModel
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 申請人員單位
        /// </summary>
        [Description("申請人員單位")]
        public string apply_unit { get; set; }

        /// <summary>
        /// 申請人員單位(中文)
        /// </summary>
        [Description("申請人員單位(中文)")]
        public string apply_unit_D { get; set; }

        /// <summary>
        /// 退件原因
        /// </summary>
        [Description("退件原因")]
        public string rej_rsn { get; set; }

        /// <summary>
        /// 實體支票
        /// </summary>
        [Description("實體支票")]
        public string check_flag { get; set; }

        ///// <summary>
        ///// 註記
        ///// </summary>
        //[Description("註記")]
        //public string mark_type2 { get; set; }

        ///// <summary>
        ///// 新抬頭
        ///// </summary>
        //[Description("新抬頭")]
        //public string new_head { get; set; }

        /// <summary>
        /// 變更原因
        /// </summary>
        [Description("變更原因")]
        public string mark_rsn2 { get; set; }

        /// <summary>
        /// 申辦方式
        /// </summary>
        [Description("申辦方式")]
        public string mark_mth2 { get; set; }

        /// <summary>
        /// 送件人員
        /// </summary>
        [Description("送件人員")]
        public string send_id { get; set; }

        /// <summary>
        /// 送件人員(中文)
        /// </summary>
        [Description("送件人員(中文)")]
        public string send_id_D { get; set; }

        /// <summary>
        /// 送件單位
        /// </summary>
        [Description("送件單位")]
        public string send_unit { get; set; }

        /// <summary>
        /// 雙掛號回執
        /// </summary>
        [Description("雙掛號回執")]
        public string reg_yn { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Description("狀態")]
        public string status { get; set; }

        /// <summary>
        /// 覆核人員
        /// </summary>
        [Description("覆核人員")]
        public string appr_id1 { get; set; }

        /// <summary>
        /// 接收日期
        /// </summary>
        [Description("接收日期")]
        public string rece_date { get; set; }

        /// <summary>
        /// 接收時間
        /// </summary>
        [Description("接收時間")]
        public string rece_time { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }

        /// <summary>
        /// 覆核人員2
        /// </summary>
        [Description("覆核人員2")]
        public string appr_id2 { get; set; }

        /// <summary>
        /// 覆核日期2
        /// </summary>
        [Description("覆核日期2")]
        public string appr_date2 { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Description("郵遞區號")]
        public string zip_code { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [Description("地址")]
        public string addr { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        [Description("收件人")]
        public string rcv_name { get; set; }

        /// <summary>
        /// 掛號號碼
        /// </summary>
        [Description("掛號號碼")]
        public string reg_no { get; set; }

        /// <summary>
        /// 刪除人員
        /// </summary>
        [Description("刪除人員")]
        public string erase_id { get; set; }

        /// <summary>
        /// 刪除日期
        /// </summary>
        [Description("刪除日期")]
        public string erase_date { get; set; }

        /// <summary>
        /// 補件原因
        /// </summary>
        [Description("補件原因")]
        public string add_rsn { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [Description("更新日期")]
        public string upd_date { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Description("更新時間")]
        public string upd_time { get; set; }

        /// <summary>
        /// 保戶電話
        /// </summary>
        [Description("保戶電話")]
        public string tel { get; set; }

        /// <summary>
        /// 支票
        /// </summary>
        [Description("支票")]
        public List<OAP0021DetailSubModel> check_nos { get; set; }
    }
}