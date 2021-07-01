using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;

namespace FRT.Web.Controllers
{
    [RoutePrefix("FastError")]
    public class FastErrorController : ApiController
    {
        /// <summary>
        /// 處理快速付款失敗案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("RemittanceFailureNotice")]
        [ValidateModel]
        public IHttpActionResult PostRemittanceFailureNotice(ErrorModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            FastErrUtil fastErrUtil = new FastErrUtil();
            FastErrModel fastErrModel = fastErrUtil.procFailNotify("S", model.ExecType, model.Fast_No, model.TextType, model.ErrorCode, model.EMSGTXT);

           
            return StatusCode(HttpStatusCode.OK);
        }


        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ErrorModel
        {
            /// <summary>
            ///S-->若要回寫FRTBARM0的匯款狀態=3-->這時…RemittanceFailureNotice只會去多呼叫SRTB0008…其它事情…還是要主程式做!!(如update FRTBARM REMIT_STAT)
            ///F-->若不回寫FRTBARM0的匯款狀態=3(回F時，一定要有ErrorCode)
            /// </summary>
            [Required]
            public string ExecType { get; set; }

            /// <summary>
            /// 快速付款編號
            /// </summary>
            [Required]
            public string Fast_No { get; set; }

            /// <summary>
            /// 錯誤代碼
            /// </summary>
            public string ErrorCode { get; set; }

            /// <summary>
            /// 若ErrorCode帶的是EMSGID，將對應中文帶回
            /// </summary>
            public string EMSGTXT { get; set; }

            /// <summary>
            /// 電文類型
            /// </summary>
            public string TextType { get; set; }


            /// <summary>
            /// 錯誤訊息
            /// </summary>
            public string ErrorMsg { get; set; }
        }
    }
}
