using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Obounl.Models.Model
{
    public class Token_OutputData
    {
        /// <summary>
        /// 處理狀態  (success：完成、fail：失敗)
        /// </summary>
        [DisplayName("處理狀態")]
        public string Status { get; set; }

        /// <summary>
        /// TokenKey 
        /// </summary>
        [DisplayName("TokenKey")]
        public string TokenKey { get; set; }
    }
}