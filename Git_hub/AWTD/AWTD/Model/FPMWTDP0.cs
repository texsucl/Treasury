using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWTD.Model
{
    public class FPMWTDP0
    {
        /// <summary>
        /// 系統來源
        /// </summary>
        [Description("系統來源")]
        public string SYS_TYPE { get; set; }

        /// <summary>
        /// 保單號碼
        /// </summary>
        [Description("保單號碼")]
        public string POLICY_NO { get; set; }

        /// <summary>
        /// 保單序號
        /// </summary>
        [Description("保單序號")]
        public string POLICY_SEQ { get; set; }

        /// <summary>
        /// 身份証重覆別
        /// </summary>
        [Description("身份証重覆別")]
        public string ID_DUP { get; set; }

        /// <summary>
        /// 核保部門
        /// </summary>
        [Description("核保部門")]
        public string WTR_DEPT { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string UPD_ID { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string UPD_DATE { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        [Description("異動時間")]
        public string UPD_TIME { get; set; }

        /// <summary>
        /// 異動程式
        /// </summary>
        [Description("異動程式")]
        public string UPD_PGM { get; set; }
    }
}
