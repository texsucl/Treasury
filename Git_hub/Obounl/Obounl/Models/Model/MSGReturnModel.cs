﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Obounl.Models.Model
{
    /// <summary>
    /// 傳到前端的Model
    /// </summary>
    public class MSGReturnModel<T>
    {
        /// <summary>
        /// 回傳資料
        /// </summary>
        [DataMember]
        public T Datas { get; set; }

        /// <summary>
        /// 回傳訊息
        /// </summary>
        [DataMember]
        [DisplayName("Message Description")]
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// ReasonCode
        /// </summary>
        [DataMember]
        [DisplayName("Message Reason Code")]
        public string REASON_CODE { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [DataMember]
        [DisplayName("Message Return Flag")]
        public bool RETURN_FLAG { get; set; }
    }
}