using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using SearchDB2.Utility;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using Newtonsoft.Json;

namespace SearchDB2.Models
{
    public class Oracle_Conn : ISql
    {
        public MSGReturnModel<List<ExpandoObject>> work(string sqlStr, string type, bool transaction, string connectionString)
        {
            MSGReturnModel<List<ExpandoObject>> result = new MSGReturnModel<List<ExpandoObject>>();
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString.getConnectionStringSetting()))
                {
                    conn.Open();
                    if (type == "S")
                    {
                        var _datas = conn.QueryAsync<dynamic>(sqlStr).Result.ToList();
                        if (_datas.Any())
                        {
                            List<ExpandoObject> datas = new List<ExpandoObject>();
                            foreach (dynamic item in _datas)
                            {
                                datas.Add(JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(item)));
                            }
                            result.Datas = datas;
                            result.RETURN_FLAG = true;
                        }
                        else
                        {
                            result.DESCRIPTION = "查無資料!";
                        }
                    }
                    else if (type == "E")
                    {
                        using (OracleTransaction tran = conn.BeginTransaction())
                        {
                            var updateNum = conn.ExecuteAsync(sqlStr, null, tran).Result;
                            if (updateNum >= 1)
                            {
                                result.RETURN_FLAG = true;
                                if (transaction)
                                {
                                    tran.Commit();
                                    result.DESCRIPTION = $@"已異動資料筆數:{updateNum}筆.";
                                }
                                else
                                {
                                    result.DESCRIPTION = $@"欲異動資料筆數:{updateNum}筆,請確認是否異動?";
                                    tran.Rollback();
                                }
                            }
                            else
                            {
                                result.DESCRIPTION = "無異動資料!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return result;
        }
    }
}