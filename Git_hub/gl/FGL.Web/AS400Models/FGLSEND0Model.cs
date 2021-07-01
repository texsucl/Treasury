using System.ComponentModel.DataAnnotations;

/// <summary>
///  FGLVRID0 新增人員設定檔 
/// </summary>
namespace FGL.Web.AS400Models
{
    public class FGLSEND0Model
    {
        [Display(Name = "程式代號")]
        public string pgmId { get; set; }

        [Display(Name = "OMS 發送對象")]
        public string sendId { get; set; }

        [Display(Name = "權責部門資料來源")]
        public string depFrom { get; set; }



        public FGLSEND0Model()
        {
            pgmId = "";
            sendId = "";
            depFrom = "";
        }
    }
}