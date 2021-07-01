

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
///  FPMADDR 保單地址檔(F系統)
/// </summary>
namespace FAP.Web.Daos
{
    public class FPMADDRDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public string getTel(string addr_code, string policy_no, int policy_seq, string id_dup, EacConnection conn)
        {
            string tel = "";

            try
            {

                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select tel, tel_no1
  from LPMADDR1 
where policy_no = :policy_no
  and policy_seq = :policy_seq
  and id_dup = :id_dup
  and addr_code = :addr_code
order by upd_yy desc, upd_mm desc, upd_dd desc";

                    com.Parameters.Add("policy_no", policy_no);
                    com.Parameters.Add("policy_seq", policy_seq);
                    com.Parameters.Add("id_dup", id_dup);
                    com.Parameters.Add("addr_code", addr_code);

                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        tel = StringUtil.toString(dbresult["tel"]?.ToString());

                        if (tel.StartsWith("09") & tel.Length == 10)
                            return tel;
                        else {
                            tel = StringUtil.toString(dbresult["tel_no1"]?.ToString());

                            if (tel.StartsWith("09") & tel.Length == 10)
                                return tel;
                        }
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