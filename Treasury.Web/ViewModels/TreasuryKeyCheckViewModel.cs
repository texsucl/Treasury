using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class TreasuryKeyCheckViewModel 
    {
        /// <summary>
        /// 管控模式
        /// </summary>
        [Description("管控模式")]
        public List<SelectOption> CONTROL_MODE { get; set; }

           /// <summary>
        /// 方式
        /// </summary>
        [Description("保管方式")]
        public List<SelectOption> CUSTODY_MODE { get; set; }

        /// <summary>
        /// 保管人
        /// </summary>
        [Description("保管人")]
        public List<SelectOption> EMP_NAME { get; set; }

        /// <summary>
        /// 代理人
        /// </summary>
        [Description("代理人")]
        public List<SelectOption> AGENT_NAME { get; set; }



    }
}