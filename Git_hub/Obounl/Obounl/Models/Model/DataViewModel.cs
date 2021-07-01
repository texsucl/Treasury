using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    //public class DViewModel
    //{

    //}

    public class DataViewModel
    {
        /// <summary>
        /// 要保文件上傳日期
        /// </summary>
        [Description("要保文件上傳日期")]
        public string AssignSchDate { get; set; }

        /// <summary>
        /// 受訪者身分證字號
        /// </summary>
        [Description("受訪者身分證字號")]
        public string custid { get; set; }

        /// <summary>
        /// 要保書編號
        /// </summary>
        [Description("要保書編號")]
        public string RptReceNo { get; set; }

        /// <summary>
        /// 保單號碼
        /// </summary>
        [Description("保單號碼")]
        public string MPolicyNo { get; set; }

        /// <summary>
        /// 受訪者姓名
        /// </summary>
        [Description("受訪者姓名")]
        public string CustName { get; set; }

        /// <summary>
        /// 電訪方式
        /// </summary>
        [Description("電訪方式")]
        public string Appointment { get; set; }

        /// <summary>
        /// 預約日期/時段
        /// </summary>
        [Description("預約日期/時段")]
        public string Reservation_Date { get; set; }

        /// <summary>
        /// 電訪狀態
        /// </summary>
        [Description("電訪狀態")]
        public string SMemo1 { get; set; }
    }
}