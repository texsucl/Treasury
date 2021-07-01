

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Web.Mvc;

/// <summary>
///  保單電話傳真資料檔 (A系統)
/// </summary>
namespace FAP.Web.Daos
{
    public class FECPTELDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public string getTel(string tel_type, string ins_id, int ins_seq, EacConnection conn)
        {
            string tel = "";

            try
            {

                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select tel 
  from LECPTELP 
where ins_id = :ins_id
  and ins_seq = :ins_seq
  and tel_type = :tel_type
order by upd_date desc, upd_time desc";

                    com.Parameters.Add("ins_id", ins_id);
                    com.Parameters.Add("ins_seq", ins_seq);
                    com.Parameters.Add("tel_type", tel_type);

                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        tel = StringUtil.toString(dbresult["tel"]?.ToString());

                        if (tel.StartsWith("09") & tel.Length == 10)
                            return tel;
                        else
                            tel = "";
                    }
                    com.Dispose();
                }

            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return tel;
        }

  



    }
}