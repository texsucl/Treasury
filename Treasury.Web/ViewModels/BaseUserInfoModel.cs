using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class BaseUserInfoModel
    {
        [Description("員工ID")]
        public string EMP_ID { get; set; }
        [Description("員工")]
        public string EMP_Name { get; set; }
        [Description("部門ID")]
        public string DPT_ID { get; set; }
        [Description("部門")]
        public string DPT_Name { get; set; }     
    }
}