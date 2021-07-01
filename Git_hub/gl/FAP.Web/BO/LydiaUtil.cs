using FAP.Web.AS400Models;
using FAP.Web.AS400PGM;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

/// <summary>
/// 功能說明：呼叫業務行政系統，查詢業務員相關資訊
/// 初版作者：20200925 Daiyu
/// 修改歷程：20200925 Daiyu
/// 需求單號：202008120153-01
/// 初版
/// </summary>
///


namespace FAP.Web.BO
{
    public  class LydiaUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 取得URL
        /// </summary>
        /// <param name="para_id"></param>
        /// <returns></returns>
        private string getUrl(string para_id) {
            SysParaDao sysParaDao = new SysParaDao();
            SYS_PARA lyodsAml = sysParaDao.qryByKey("AP", "Lydia", para_id);
            string lydiaUrl = StringUtil.toString(lyodsAml.PARA_VALUE);
            return lydiaUrl;
        }

        /// <summary>
        /// 001 查詢業務通路業務人員資訊_1
        /// </summary>
        /// <param name="model"></param>
        public async Task<Lydia001Model> callLydia001Async(Lydia001Model model)
        {
            string lyodsAmlUrl = getUrl("001");

            Lydia001Model responseData = new Lydia001Model();

            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 header
                client.DefaultRequestHeaders.Add("System-Name", "FAP");

                // 準備寫入的 data
                Lydia001Model postData = model;

                // 將 data 轉為 json
                string json = JsonConvert.SerializeObject(postData);

                // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // 發出 post 並取得結果
                    HttpResponseMessage response = client.PostAsync(lyodsAmlUrl, contentPost).Result;


                    // 將回應結果內容取出
                    var customerJsonString = await response.Content.ReadAsStringAsync();
                    responseData = JsonConvert.DeserializeObject<Lydia001Model>(customerJsonString);

                }
                catch (Exception e)
                {
                    logger.Error(e.ToString);

                }
            }

            return responseData;

        }

        /// <summary>
        /// 004 查詢保經代業務人員資訊_3
        /// </summary>
        /// <param name="model"></param>
        public async Task<Lydia004Model> callLydia004Async(Lydia004Model model) {
            string lyodsAmlUrl = getUrl("004");

            Lydia004Model responseData = new Lydia004Model();

            //建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                // 指定 header
                client.DefaultRequestHeaders.Add("System-Name", "FAP");

                // 準備寫入的 data
                Lydia004Model postData = model;

                // 將 data 轉為 json
                string json = JsonConvert.SerializeObject(postData);

                // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // 發出 post 並取得結果
                    HttpResponseMessage response = client.PostAsync(lyodsAmlUrl, contentPost).Result;


                    // 將回應結果內容取出
                    var customerJsonString = await response.Content.ReadAsStringAsync();
                    responseData = JsonConvert.DeserializeObject<Lydia004Model>(customerJsonString);
                    
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString);

                }
            }

            return responseData;

        }



    }
}