using System.ComponentModel.DataAnnotations;

/// <summary>
///  FGLVRID0 新增人員設定檔 
/// </summary>
namespace FGL.Web.AS400Models
{
    public class FGLVRID0Model
    {
        [Display(Name = "流程別")]
        public string flowType { get; set; }

        [Display(Name = "系統別")]
        public string sysType { get; set; }

        [Display(Name = "來源別")]
        public string srceFrom { get; set; }

        [Display(Name = "承辦人員")]
        public string entryId { get; set; }

        public string entryName { get; set; }



        public FGLVRID0Model()
        {
            flowType = "";
            sysType = "";
            srceFrom = "";
            entryId = "";
            entryName = "";
        }
    }
}