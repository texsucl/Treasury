
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;


/// <summary>
/// 功能說明：FAP_VE_TRACE_HIS 逾期未兌領清理記錄檔
/// 初版作者：20190627 Daiyu
/// 修改歷程：20190627 Daiyu
///           需求單號：
///           初版
///修改歷程：20191025 daiyu
///           需求單號：201910240295-00
///           1.【OAP0004 指定保局範圍作業】若查詢條件"保局範圍=0"時，要可以處理
///            1.1 null值
///            1.2 空白
///            1.3 "0"
/// ---------------------------------------------------------
/// 修改歷程：20210331 daiyu 
/// 需求單號：202103250638-00
/// 修改內容：執行覆核作業時，若該批結案編號中，有任一筆的清理狀態為"1.已給付"時，系統提示錯誤，且只能執行"駁回"。
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTraceHisDao
    {
        public List<OAP0011Model> qryForOAP0011(string aply_no)
        {
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE_HIS
                                join main in db.FAP_VE_TRACE on new { m.check_acct_short, m.check_no } equals new { main.check_acct_short, main.check_no }
                                where 1 == 1
                                & m.aply_no == aply_no
                                & m.srce_pgm == "OAP0011"
                                group main by new { m.check_no, m.check_acct_short, main.status } into g

                                select new OAP0011Model
                                {
                                    check_no = g.Key.check_no,
                                    check_acct_short = g.Key.check_acct_short,
                                    status = g.Key.status,
                                    check_amt = g.Sum(x => x.check_amt).ToString()
                                }).ToList<OAP0011Model>();

                    return rows;
                }
            }
        }


        /// <summary>
        /// "OAP0004 指定保局範圍作業"查詢
        /// </summary>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        public List<OAP0004Model> qryForOAP0004(string aply_no)
        {
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE_HIS 
                                    join main in db.FAP_VE_TRACE on new { m.check_acct_short, m.check_no} equals new { main.check_acct_short, main.check_no}
                                where 1 == 1
                                & m.aply_no == aply_no
                                & m.srce_pgm == "OAP0004"
                                group main by new { m.check_acct_short, m.check_no, main.check_date, main.fsc_range, fsc_range_n = m.fsc_range } into g

                                select new OAP0004Model
                                {
                                    check_no = g.Key.check_no,
                                    check_acct_short = g.Key.check_acct_short,
                                    check_date = g.Key.check_date.ToString(),
                                    //check_date = g.Key.check_date == null ? "" : (SqlFunctions.DatePart("year", g.Key.check_date) - 1911) + "/" +
                                    //              SqlFunctions.DatePart("m", g.Key.check_date) + "/" +
                                    //              SqlFunctions.DateName("day", g.Key.check_date).Trim(),
                                    fsc_range = g.Key.fsc_range,
                                    fsc_range_n = g.Key.fsc_range_n,
                                    check_amt = g.Sum(x => x.check_amt).ToString()
                                }).OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList<OAP0004Model>();


                    foreach (OAP0004Model d in rows)
                    {
                        if (!"".Equals(d.check_date))
                            d.check_date = DateUtil.ADDateToChtDate(d.check_date, 3, "/");
                    }
                    return rows;
                }
            }
        }



        /// <summary>
        /// "OAP0004 指定保局範圍作業"申請覆核
        /// </summary>
        /// <param name="check_date_b"></param>
        /// <param name="check_date_e"></param>
        /// <param name="check_no"></param>
        /// <param name="fsc_range"></param>
        /// <returns></returns>
        public int insertForOAP0004(DateTime dt, string usrId, string aply_no
            , string check_date_b, string check_date_e, string check_no, string fsc_range, string fsc_range_n
            , string[] passCheckNo, SqlConnection conn, SqlTransaction transaction)
        {
            string bCheckDateB = StringUtil.toString(check_date_b) == "" ? "Y" : "N";
            string bCheckDateE = StringUtil.toString(check_date_e) == "" ? "Y" : "N";
            string bCheckNo = StringUtil.toString(check_no) == "" ? "Y" : "N";
            string bFscRange = StringUtil.toString(fsc_range) == "" ? "Y" : "N";

            string passSql = "";
            string groupSql = " GROUP BY system, check_no, check_acct_short, paid_id";
            if (passCheckNo != null) {
                passSql = " AND check_no NOT IN ('";
                foreach (string d in passCheckNo)
                {
                    passSql += d + "','";
                }
                passSql = passSql.Substring(0, passSql.Length - 2) + ")";
            }
            


            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE_HIS
 (aply_no
 ,system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,fsc_range
 ,srce_pgm
)

SELECT @aply_no
 ,system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,@fsc_range_n
 ,'OAP0004'
  FROM FAP_VE_TRACE
WHERE 1 = 1
  AND (@bCheckDateB = 'Y' OR (@bCheckDateB = 'N' AND check_date >= @check_date_b))
  AND (@bCheckDateE = 'Y' OR (@bCheckDateE = 'N' AND check_date <= @check_date_e))
  AND (@bCheckNo = 'Y' OR (@bCheckNo = 'N' AND check_no = @check_no))
 AND (@bFscRange = 'Y' OR (@bFscRange = 'N' AND ( (@fsc_range != '0' AND fsc_range = @fsc_range)
                                                OR(@fsc_range = '0' AND(fsc_range = @fsc_range OR fsc_range = '' OR fsc_range IS NULL))
                                                    ))) 
";



                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql + passSql + groupSql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@fsc_range_n", fsc_range_n);
                //cmd.Parameters.AddWithValue("@update_id", usrId);
                //cmd.Parameters.AddWithValue("@update_datetime", dt);
                cmd.Parameters.AddWithValue("@bCheckDateB", bCheckDateB);
                cmd.Parameters.AddWithValue("@check_date_b", check_date_b);
                cmd.Parameters.AddWithValue("@bCheckDateE", bCheckDateE);
                cmd.Parameters.AddWithValue("@check_date_e", check_date_e);
                cmd.Parameters.AddWithValue("@bCheckNo", bCheckNo);
                cmd.Parameters.AddWithValue("@check_no", check_no);
                cmd.Parameters.AddWithValue("@bFscRange", bFscRange);
                cmd.Parameters.AddWithValue("@fsc_range", fsc_range);

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }




        public int insertForOAP0008(DateTime dt, string usrId, string aply_no
           , string check_no, string check_acct_short, string level_1, string level_2, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE_HIS
 (aply_no
 ,system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,level_1
 ,level_2
 ,srce_pgm
)

SELECT @aply_no
 ,system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,@level_1
 ,@level_2
 ,'OAP0008'
	FROM FAP_VE_TRACE
WHERE 1 = 1
  AND check_no = @check_no
  AND check_acct_short = @check_acct_short
 GROUP BY system, check_no, check_acct_short, paid_id
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@level_1", StringUtil.toString(level_1));
                cmd.Parameters.AddWithValue("@level_2", StringUtil.toString(level_2));
                cmd.Parameters.AddWithValue("@check_no", check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);


                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int insertForOAP0011(DateTime dt, string usrId, string aply_no, string closed_no, string level_1, string level_2, OAP0011Model d
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE_HIS
 (aply_no
 ,system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,level_1
 ,level_2
 ,srce_pgm
 ,closed_no
 ,closed_desc
) values (
  @aply_no
 ,''
 ,@check_no
 ,@check_acct_short
 ,''
 ,@level_1
 ,@level_2
 ,'OAP0011'
 ,@closed_no
 ,@closed_desc
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@closed_no", closed_no);
                cmd.Parameters.AddWithValue("@closed_desc", d.closed_desc);   //add by daiyu 20200730
                cmd.Parameters.AddWithValue("@level_1", level_1);
                cmd.Parameters.AddWithValue("@level_2", level_2);
                cmd.Parameters.AddWithValue("@check_no", d.check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", d.check_acct_short);


                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}