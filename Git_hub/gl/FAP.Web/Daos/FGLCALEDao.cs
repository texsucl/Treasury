

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
/// FGLCALE0   日歷檔
/// </summary>
namespace FAP.Web.Daos
{
    public class FGLCALEDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 取得指定日加設定工作日的日期(不包含指定日)
        /// </summary>
        /// <param name="TWDate">指定日(民國年月日)</param>
        /// <param name="std">工作日</param>
        /// <returns></returns>
        public string GetSTDDate(string TWDate, int std)
        {
            string result = string.Empty;
            if (TWDate.IsNullOrWhiteSpace() || std < 1)
                return result;
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                    select ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) STDDate from (
                    select YEAR,MONTH,DAY from LGLCALE1 
                    where ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) > :DATE
                    and corp_rest <> 'Y'
                    order by year,month,day
                    FETCH FIRST :STD ROWS ONLY)
                    order by year desc,month desc , day desc
                    FETCH FIRST 1 ROWS ONLY ";

                        com.Parameters.Add("DATE", TWDate);
                        com.Parameters.Add("STD", std);
                        
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            result = dbresult["STDDate"]?.ToString()?.Trim();
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 抓取開始日到結束日所有的工作日 (不包含開始日)
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">結束日</param>
        /// <returns></returns>
        public List<string> GetWorkDate(string startDate, string endDate)
        {
            var results = new List<string>();

            if (startDate.IsNullOrWhiteSpace() || endDate.IsNullOrWhiteSpace())
                return results;
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
        select ((LPAD(YEAR,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) STDDate  from LGLCALE1 
        where  ((LPAD(year,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) > :startDate
        and  ((LPAD(year,3,'0')) || (LPAD(MONTH,2,'0')) || (LPAD(DAY,2,'0'))) <= :endDate
        and corp_rest <> 'Y'
        order by YEAR,MONTH,DAY; ";
                        com.Parameters.Add("startDate", startDate);
                        com.Parameters.Add("endDate", endDate);
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            results.Add(dbresult["STDDate"]?.ToString()?.Trim()); //工作日                      
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }

            return results;
        }



    }
}