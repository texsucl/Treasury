using AWTD.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWTD.Utility.Log;
using static AWTD.Enum.Ref;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Newtonsoft.Json;

namespace AWTD.Daos
{
    public class MSSql
    {
        #region MyRegion
        /// <summary>
        /// 連線字串
        /// </summary>
        private static string DefaultConn = string.Empty;

        /// <summary>
        /// 加密判斷參數 AppSettings => parameterEncrypt
        /// </summary>
        private static bool encryptFlag = false;

        public MSSql(string connStr = null)
        {
            try
            {
                encryptFlag = System.Configuration.ConfigurationManager.AppSettings.Get("parameterEncrypt") == "Y";
                if (!connStr.IsNullOrWhiteSpace())
                {
                    DefaultConn = System.Configuration.ConfigurationManager.
                        ConnectionStrings[connStr].ConnectionString;
                }
                else
                {
                    DefaultConn = System.Configuration.ConfigurationManager.
                        ConnectionStrings["WanpieConnection"].ConnectionString;
                }
            }
            catch (Exception ex)
            {
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }
        }

        #endregion

        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">查詢資料Class</typeparam>
        /// <param name="sql">sql 語法</param>
        /// <param name="param">參數</param>
        /// <param name="logger"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public Tuple<string, IEnumerable<T>> Query<T>(
            string sql,
            object param = null,
            string logger = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            IEnumerable<T> result = null;
            var _msg = string.Empty;
            try
            {
                NlogSet($@"Sql:{sql}", logger);
                if (param != null)
                    NlogSet($@"param:{(JsonConvert.SerializeObject(param))}", logger);
                using (SqlConnection conn = new SqlConnection(DefaultConn))
                {
                    result = conn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType).Result;
                    NlogSet($@"查詢結果資料數:{result.Count()}", logger);
                }
            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NlogSet(_msg, logger, Nlog.Error);
                result = new List<T>();
            }
            finally
            {

            }
            return new Tuple<string, IEnumerable<T>>(_msg, result);
        }

        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T">查詢資料Class</typeparam>
        /// <param name="sql">sql 語法</param>
        /// <param name="param">參數</param>
        /// <param name="logger"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public Tuple<string, T> Query_FirstOrDefault<T>(
            string sql,
            object param = null,
            string logger = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            ) where T : class
        {
            T result = null;
            var _msg = string.Empty;
            try
            {
                NlogSet($@"Sql:{sql}", logger);
                if (param != null)
                    NlogSet($@"param:{(JsonConvert.SerializeObject(param))}", logger);
                using (SqlConnection conn = new SqlConnection(DefaultConn))
                {
                    result = conn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType).Result;
                    var _count = result == null ? 0 : 1;
                    NlogSet($@"查詢結果資料數:{_count}", logger);
                }
            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NlogSet(_msg, logger, Nlog.Error);
            }
            finally
            {

            }
            return new Tuple<string, T>(_msg, result);
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="sql">sql 語法</param>
        /// <param name="param">參數</param>
        /// <param name="logger"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public Tuple<string, int> Execute(
            string sql,
            object param = null,
            string logger = null,
            CommandType? commandType = null,
            IDbTransaction transaction = null,
            SqlConnection conn = null,
            bool dispose = true,
            int? commandTimeout = null,
            bool paramFlag = true
            )
        {
            var _msg = string.Empty;
            int result = 0;
            bool _dispose = false;
            try
            {
                NlogSet($@"Sql:{sql}", logger);
                if (paramFlag && param != null)
                    NlogSet($@"param:{(JsonConvert.SerializeObject(param))}", logger);
                if (conn == null)
                {
                    conn = new SqlConnection(DefaultConn);
                    _dispose = true;
                }
                result = conn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType).Result;
                NlogSet($@"異動結果資料數:{result}", logger);
            }
            catch (Exception ex)
            {
                _msg = ex.exceptionMessage();
                NlogSet(_msg, logger, Nlog.Error);
            }
            finally
            {
                if (dispose && _dispose)
                    conn.Dispose();
            }
            return new Tuple<string, int>(_msg, result);
        }
    }
}
