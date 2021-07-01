using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class Tel_Visit_OutputData
    {
        /// <summary>
        /// 處理狀態  (success：完成、fail：失敗)
        /// </summary>
        [DisplayName("處理狀態")]
        public string Status { get; set; }

        /// <summary>
        /// 結果代碼 (提供AP判斷錯誤點的代碼)
        /// </summary>
        [DisplayName("結果代碼")]
        public string Code { get; set; }

        /// <summary>
        /// 回傳訊息
        /// </summary>
        [DisplayName("回傳訊息")]
        public string Msg { get; set; }

        /// <summary>
        /// TokenKey 
        /// </summary>
        [DisplayName("TokenKey")]
        public string TokenKey { get; set; }
    }
}