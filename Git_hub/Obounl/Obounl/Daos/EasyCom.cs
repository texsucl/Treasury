using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Obounl.Daos
{
    public class EasyCom
    {
        /// <summary>
        /// 取得easycom連線資訊
        /// </summary>
        /// <returns></returns>
        public static String GetEasycomConn()
        {
            var easycom = new EasyCom();
            var _Execute = new MSSql().Query<dynamic>(
                "select DB_LID, DB_PWD From CC_DB (nolock) Where DB_ID=@DB_ID",
                new { DB_ID = ConfigurationManager.AppSettings["as400-db-id"] });
            var data = _Execute.Item2.FirstOrDefault();
            var id = easycom.Decrypt(data?.DB_LID);
            var password = easycom.Decrypt(data?.DB_PWD);
            var connString = String.Format(ConfigurationManager.ConnectionStrings["Easycom"]?.ConnectionString, id, password);

            return connString;
        }


        private string Decrypt(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                return data;
            }

            var key = ConfigurationManager.AppSettings["as400-db-key"];
            var keyData = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(key)));
            var IVData = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(key)));
            return Aes256DecryptString(data, keyData, IVData);
        }

        private string Aes256DecryptString(string data, string key, string iv)
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                byte[] cipherTextData = Convert.FromBase64String(data);
                byte[] keyData = Convert.FromBase64String(key);
                byte[] IVData = Convert.FromBase64String(iv);
                ICryptoTransform transform = AES.CreateDecryptor(keyData, IVData);
                byte[] outputData = transform.TransformFinalBlock(cipherTextData, 0, cipherTextData.Length);
                return Encoding.Unicode.GetString(outputData);
            }
        }
    }
}