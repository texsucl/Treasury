using Newtonsoft.Json;
using Obounl.Infrastructure;
using Obounl.Models.Interface;
using Obounl.Models.Model;
using Obounl.Models.Repository;
using Obounl.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;


namespace Obounl.Controllers
{
   
    public class ObounlApiController : ApiController
    {
        private IAPIRepository APIRepository;

        public ObounlApiController() 
        {
            APIRepository = new APIRepository();
        }

        /// <summary>
        ///  服務人員收集完成合約資料後如需申請即時電訪，需先將資料以API方式傳送至電訪系統中建立電訪名單
        /// </summary>
        /// <param name="data">加密後字串</param>
        /// <returns></returns>
        //[APIAuth]
        [IPAuth]
        [Route("api/ObounlApi/Contract")]
        [APIBrowserEvent("Contract")]
        public IHttpActionResult Post([FromBody] string data)
        {
            var _result = new Tel_Visit_OutputData();
            DateTime dtn = DateTime.Now.AddMinutes(10);
            var _dtnAES = Extension.getTokenKey();
            if (!data.IsNullOrWhiteSpace() && data.ToLower().Trim() == "tokenkey")
            {
                if (_dtnAES.Item1)
                {
                    _result.Status = "success";
                    _result.TokenKey = _dtnAES.Item2;
                }
                else
                {
                    _result.Status = "fail";
                    _result.TokenKey = string.Empty;
                }
                return Json(_result);
            }
            var _Insert_result = APIRepository.Contract_Data_Insert(data);
      
            if (_Insert_result.RETURN_FLAG)
            {
                _result.Status = "success";
                _result.Code = _Insert_result.Datas;
                _result.Msg = $@"上傳完成";
                _result.TokenKey = _dtnAES.Item2;
            }
            else
            {
                _result.Status = "fail";
                _result.Code = _Insert_result.Datas;
                _result.Msg = $@"上傳失敗 : {_Insert_result.DESCRIPTION}";
            }
            return Json(_result);
        }

        /// <summary>
        /// 取token值的Api 使用白名單防擋
        /// </summary>
        /// <param name="data">加密後字串</param>
        /// <returns></returns>
        //[IPAuth]
        //[Route("api/ObounlApi/GetT")]
        //[APIBrowserEvent("GetT")]
        //public IHttpActionResult post()
        //{
        //    string Token = ConfigurationManager.ConnectionStrings["TokenKey"]?.ConnectionString;
        //    DateTime dtn = DateTime.Now.AddMinutes(10);
        //    var _dtnAES = dtn.ToString("yyyy/MM/dd HH:mm:dd").AESEncrypt(null, false);
        //    var _result = new Token_OutputData();
        //    if (_dtnAES.Item1)
        //    {
        //        _result.Status = "success";
        //        _result.TokenKey = _dtnAES.Item2;
        //    }
        //    else
        //    {
        //        _result.Status = "fail";
        //        _result.TokenKey = string.Empty;
        //    }
        //    return Json(_result);
        //}
    }
}
