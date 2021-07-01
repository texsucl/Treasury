using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ObounlTest.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            //AESEncrypt("2020/11/30 23:59:59");
            //AESEncrypt("V288890308");
            return View();
        }

        public JsonResult AESEncrypt(string str)
        {
            string _key = "fbim";
            str = str ?? "test";
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(_key.Substring(0, (_key.Length > 32) ? 32 : _key.Length).PadLeft(32, '0'));
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(str);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            var encryptStr = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            encryptStr = Uri.UnescapeDataString(encryptStr);
            return Json(encryptStr);
        }
    }
}