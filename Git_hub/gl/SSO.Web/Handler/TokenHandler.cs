using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SSO.WebHandler
{
    public class TokenHandler: DelegatingHandler
    {
        string Token = ConfigurationManager.ConnectionStrings["eInvoApi"].ConnectionString;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isValid = false;
            IEnumerable<string> headerDatas;
            //嘗試取得 Header 中 "Token" 的資料
            var hasToken = request.Headers.TryGetValues("Token", out headerDatas);

            //request Header 中有 Token
            if (hasToken)
            {
                //檢查 Token 是否與設定的相同
                if (headerDatas?.Any(a => a.Equals(Token)) == true)
                {
                    isValid = true;
                }
            }

            //Token 無效，回傳 401
            if (!isValid)
                return request.CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized");

            //Token 檢查通過，繼續後續作業
            var response = await base.SendAsync(request, cancellationToken);

            //回傳執行結果
            return response;
        }
    }
}