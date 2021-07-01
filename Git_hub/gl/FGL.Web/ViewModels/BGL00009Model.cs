
using System.ComponentModel.DataAnnotations;


namespace FGL.Web.ViewModels
{
    public class BGL00009Model
    {

        [Display(Name = "系統別")]
        public string SYS_TYPE { get; set; }

        [Display(Name = "來源")]
        public string RESOURCE { get; set; }

        public string RESOURCE_DESC { get; set; }

        [Display(Name = "區部")]
        public string AREA { get; set; }

        [Display(Name = "憑證編號")]
        public string VHR_NO { get; set; }

        [Display(Name = "繳款單號/處理編號")]
        public string PRO_NO { get; set; }

        [Display(Name = "給付方式")]
        public string PAID_TYPE { get; set; }

        [Display(Name = "給付金額")]
        public string AMT { get; set; }

        [Display(Name = "公司別")]
        public string COMP_TYPE { get; set; }

        [Display(Name = "登打人員")]
        public string ENTRY_ID { get; set; }

        [Display(Name = "登打人員")]
        public string ENTRY_NAME { get; set; }

        [Display(Name = "承辦人員")]
        public string PROC_ID { get; set; }

        [Display(Name = "承辦人員")]
        public string PROC_NAME { get; set; }

        public string SEND_GRP { get; set; }

        public BGL00009Model() {
            SYS_TYPE = "";
            RESOURCE = "";
            RESOURCE_DESC = "";
            AREA = "";
            VHR_NO = "";
            PRO_NO = "";
            PAID_TYPE = "";
            AMT = "";
            COMP_TYPE = "";
            ENTRY_ID = "";
            ENTRY_NAME = "";
            PROC_ID = "";
            PROC_NAME = "";
            SEND_GRP = "";
        }
    }
}