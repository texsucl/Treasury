using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Google_Api_CTtoEN
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = "北市內湖區明湖里５鄰康寧路三段１６５巷２７號４樓";
            //Console.WriteLine(GoogleAPIUtilities.ConvertAddressToJsonString(address));
            //Console.ReadLine();

            //var url = String.Format("http://maps.google.com/maps/api/geocode/json?sensor=false&address={0}", address);

            var url = $@"https://translate.google.com.tw/?hl=zh-TW&tab=wT1#view=home&op=translate&sl=zh-CN&tl=en&text=%E5%8C%97%E5%B8%82%E5%85%A7%E6%B9%96%E5%8D%80%E6%98%8E%E6%B9%96%E9%87%8C%EF%BC%95%E9%84%B0%E5%BA%B7%E5%AF%A7%E8%B7%AF%E4%B8%89%E6%AE%B5%EF%BC%91%EF%BC%96%EF%BC%95%E5%B7%B7%EF%BC%92%EF%BC%97%E8%99%9F%EF%BC%94%E6%A8%93";

            string result = String.Empty;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            using (var response = request.GetResponse())
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                //Json格式: 請參考http://code.google.com/intl/zh-TW/apis/maps/documentation/geocoding/
                result = sr.ReadToEnd();
            }
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }

    public class GoogleAPIUtilities
    {
        /// <summary>  
        /// 把地址轉成Json格式，這樣回傳字串裡才有緯經度  
        /// 因為使用到Geocoding API地理編碼技術，請注意使用限制：https://developers.google.com/maps/documentation/geocoding/?hl=zh-tw#Limits  
        /// </summary>  
        /// <param name="address">地址全名(含縣市)</param>  
        /// <returns></returns>  
        public static string ConvertAddressToJsonString(string address)
        {
            //申請API Key，能夠呼叫的額度會多一些
            //string GoogleAPIKey = ConfigurationManager.AppSettings["GoogleAPIKey"];
            //string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + HttpUtility.UrlEncode(address, Encoding.UTF8) + "&key=" + GoogleAPIKey;
            string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + HttpUtility.UrlEncode(address, Encoding.UTF8);
            string result = "";//回傳結果 
            using (WebClient client = new WebClient())
            {
                //指定語言，否則Google預設回傳英文   
                client.Headers[HttpRequestHeader.AcceptLanguage] = "zh-TW";
                //不設定的話，會回傳中文亂碼
                client.Encoding = Encoding.UTF8;
                #region POST method才會用到
                /*
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                byte[] response = client.UploadValues("https://maps.googleapis.com/maps/api/geocode/json", new NameValueCollection()
                {
                        { "address", HttpUtility.UrlEncode(address, Encoding.UTF8)},
                        { "key", GoogleAPIKey }
                });
                result = Encoding.UTF8.GetString(response);
                */
                #endregion
                result = client.DownloadString(url);
            }//end using

            return result;
        }//end method

        /// <summary>  
        /// 傳入Geocoding API產生的Json字串，取得經緯度  
        /// </summary>  
        /// <param name="json"></param>  
        /// <returns></returns>  
        public static double[] 傳入GeocodingAPI產生的Json字串取得經緯度(string json)
        {

            //將Json字串轉成物件  
            GoogleGeocodingAPI.RootObject rootObj = JsonConvert.DeserializeObject<GoogleGeocodingAPI.RootObject>(json);

            //回傳結果
            double[] latLng = new double[2];
            //防呆
            if (rootObj.status == "OK")
            {
                //從results開始往下找 
                double lat = rootObj.results[0].geometry.location.lat;//緯度  
                double lng = rootObj.results[0].geometry.location.lng;//經度   
                //緯度
                latLng[0] = lat;
                //經度
                latLng[1] = lng;
            }//end if 
            else
            {//todo寫Log

            }//end else 
            return latLng;
        }//end method 
    }
}

/// <summary>
/// Json Parse資料用
/// </summary>
namespace GoogleGeocodingAPI
{
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }


    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }


    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }


    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }


    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }


    public class Geometry
    {
        public Location location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
    }


    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public bool partial_match { get; set; }
        public List<string> types { get; set; }
    }

    public class RootObject
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }

}
