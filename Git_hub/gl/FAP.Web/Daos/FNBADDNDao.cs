

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
///   FNBADDN  保單地址檔(F系統)
/// </summary>
namespace FAP.Web.Daos
{
    public class FNBADDNDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public string getTel(string tel_type, string policy_no, int policy_seq, string id_dup, EacConnection conn)
        {
            string tel = "";

            try
            {

                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select mobile_i,  mobile
  from LNBADDN1 
where policy_no = :policy_no
  and policy_seq = :policy_seq
  and id_dup = :id_dup
order by upd_yy desc, upd_mm desc, upd_dd desc";

                    com.Parameters.Add("policy_no", policy_no);
                    com.Parameters.Add("policy_seq", policy_seq);
                    com.Parameters.Add("id_dup", id_dup);

                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        if("mobile".Equals(tel_type))
                            tel = StringUtil.toString(dbresult["mobile"]?.ToString());
                        else
                            tel = StringUtil.toString(dbresult["mobile_i"]?.ToString());

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