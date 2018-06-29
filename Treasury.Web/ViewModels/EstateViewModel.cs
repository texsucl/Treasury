using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class EstateViewModel : ITreaItem
    {
        /// <summary>
        /// 存取作業編號
        /// </summary>
        [Description("存取作業編號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 群組編號
        /// </summary>
        [Description("群組編號")]
        public string vGroupNo { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔
        /// </summary>
        [Description("存取項目冊號資料檔")]
        public EstateModel vItem_Book { get; set; }

        /// <summary>
        /// 不動產庫存資料檔
        /// </summary>
        [Description("不動產庫存資料檔")]       
        public List<EstateDetailViewModel> vDetail { get; set; }
    }
}