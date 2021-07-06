using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using SearchDB2.Utility;
using System.Data.EasycomClient;
using System.Data.Common;
using System.Data;

namespace SearchDB2.Models
{
    public class EasyCom_Conn : ISql
    {
        public MSGReturnModel<List<ExpandoObject>> work(string sqlStr, string type, bool transaction, string connectionString)
        {
            MSGReturnModel<List<ExpandoObject>> result = new MSGReturnModel<List<ExpandoObject>>();
            try
            {
                using (EacConnection conn = new EacConnection(connectionString.getConnectionStringSetting()))
                {
                    conn.Open();
                    string sql = sqlStr;
                    EacTransaction _transaction = conn.BeginTransaction();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        if (type == "S")
                        {
                            List<ExpandoObject> dresult = new List<ExpandoObject>();
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader _result = com.ExecuteReader();
                            if (_result.Read())  // read the first one to get the columns collection
                            {
                                var cols = _result.GetSchemaTable()
                                             .Rows
                                             .OfType<DataRow>()
                                             .Select(r => r["ColumnName"]);
                                do
                                {
                                    dynamic d = new ExpandoObject();
                                    foreach (string col in cols)
                                    {
                                        ((IDictionary<System.String, System.Object>)d)[col] = _result[col];
                                    }
                                    dresult.Add(d);
                                } while (_result.Read());
                            }
                            if (dresult.Any())
                            {
                                result.RETURN_FLAG = true;
                                result.Datas = dresult;
                            }
                            else
                            {
                                result.DESCRIPTION = "查無資料!";
                            }
                        }
                        else if (type == "E")
                        {
                            com.Transaction = _transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            if (updateNum >= 1)
                            {
                                result.RETURN_FLAG = true;
                                if (transaction)
                                {
                                    _transaction.Commit();
                                    result.DESCRIPTION = $@"已異動資料筆數:{updateNum}筆.";
                                }
                                else
                                {
                                    result.DESCRIPTION = $@"欲異動資料筆數:{updateNum}筆,請確認是否異動?";
                                    _transaction.Rollback();
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