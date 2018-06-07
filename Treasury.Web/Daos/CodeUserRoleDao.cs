using Treasury.Web.BO;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.WebViewModels;
using System.Transactions;
using System.Data.Entity.SqlServer;
using Treasury.WebUtils;

namespace Treasury.Web.Daos
{
    public class CodeUserRoleDao
    {
        /// <summary>
        /// 以"使用者帳號"為鍵項查詢資料
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CodeUserRoleModel> qryByUserID(String userId)
        {
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {

                using (dbTreasuryEntities db = new dbTreasuryEntities())
            {

                    List<CodeUserRoleModel> rows = new List<CodeUserRoleModel>();
                    rows = (from user in db.CODE_USER_ROLE
                            join role in db.CODE_ROLE on user.ROLE_ID equals role.ROLE_ID
                            where user.USER_ID == userId.Trim()

                            join cAuthType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ROLE_AUTH_TYPE") on role.ROLE_AUTH_TYPE equals cAuthType.CODE into psAuthType
                            from xAuthType in psAuthType.DefaultIfEmpty()

                            select new CodeUserRoleModel()
                            {
                                aplyNo = "",
                                roleId = user.ROLE_ID.Trim(),
                                roleName = role.ROLE_NAME.Trim(),
                                roleAuthType = role.ROLE_AUTH_TYPE,
                                roleAuthTypeDesc = xAuthType == null ? "" : xAuthType.CODE_VALUE.Trim(),
                                execAction = "",
                                execActionDesc = "",
                                createUid = user.CREATE_UID.Trim(),
                                createDt = user.CREATE_DT == null ? "" : SqlFunctions.DateName("year", user.CREATE_DT) + "/" +
                                                                         SqlFunctions.DatePart("m", user.CREATE_DT) + "/" +
                                                                         SqlFunctions.DateName("day", user.CREATE_DT).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", user.CREATE_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", user.CREATE_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", user.CREATE_DT).Trim(),
                               
                            }).Distinct().OrderBy(d => d.roleId).ToList<CodeUserRoleModel>();



                    return rows;
            }
        }
        }


        public CODE_USER_ROLE qryByKey(string userId, string roleId)
        {
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    CODE_USER_ROLE userRole = db.CODE_USER_ROLE.Where(x =>
                            x.USER_ID.Equals(userId)
                        && x.ROLE_ID.Equals(roleId)
                    ).FirstOrDefault();

                    return userRole;
                }
            }
        }


        ///// <summary>
        ///// 查詢使用者角色檔中，仍為有效的資料
        ///// </summary>
        ///// <param name="sysDate"></param>
        ///// <returns></returns>
        //public List<SrcOprUnitJobModel> qryValidUserRole(String sysDate)
        //{
        //    using (DbAccountEntities db = new DbAccountEntities())
        //    {

        //        SrcOprUnitJobModel srcOprUnitJobModel = new SrcOprUnitJobModel();
        //        var rows = (from ur in db.CODEUSERROLE.Where(x => x.CDISABLEDATE.CompareTo(sysDate) >= 0)
        //                    join u in db.CODEUSER on ur.CAGENTID equals u.CAGENTID
        //                    join r in db.CODEROLE on ur.CROLEID equals r.CROLEID
        //                    select new SrcOprUnitJobModel
        //                    {
        //                        cAgentID = u.CAGENTID.Trim(),
        //                        cWorkUnitCode = u.CWORKUNITCODE.Trim(),
        //                        cWorkUnitSeq = u.CWORKUNITSEQ.Trim(),
        //                        cRoleID = r.CROLEID.Trim(),
        //                        cOperatorArea = r.COPERATORAREA.Trim(),
        //                        cSearchArea = r.CSEARCHAREA.Trim()
        //                    }).OrderBy(x => x.cAgentID).Distinct().ToList();

        //        return rows;
        //    }
        //}





        /// <summary>
        /// 新增使用者角色資料
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public CODE_USER_ROLE insert(CODE_USER_ROLE userRole, SqlConnection conn, SqlTransaction transaction)
        {


            string sql = @"insert into CODE_USER_ROLE
([USER_ID]
,[ROLE_ID]
,[CREATE_UID]
,[CREATE_DT])
     VALUES
(@USER_ID
,@ROLE_ID
,@CREATE_UID
,@CREATE_DT)
";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@USER_ID", userRole.USER_ID.Trim());
                command.Parameters.AddWithValue("@ROLE_ID", userRole.ROLE_ID.Trim());
                command.Parameters.AddWithValue("@CREATE_UID", userRole.CREATE_UID.Trim());
                command.Parameters.Add("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)userRole.CREATE_DT ?? System.DBNull.Value;


                int cnt = command.ExecuteNonQuery();

                return userRole;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        //        /// <summary>
        //        /// 使用者管理-維護指派單位
        //        /// </summary>
        //        /// <param name="userRole"></param>
        //        /// <param name="keyAgentID"></param>
        //        /// <param name="keyRoleID"></param>
        //        /// <param name="keyEnableDate"></param>
        //        /// <param name="keyDisableDate"></param>
        //        /// <param name="conn"></param>
        //        /// <param name="transaction"></param>
        //        /// <returns></returns>
        //        public CODEUSERROLE update(CODEUSERROLE userRole, string keyAgentID, string keyRoleID, string keyEnableDate, string keyDisableDate,
        //            SqlConnection conn, SqlTransaction transaction)
        //        {
        //            string[] curDateTime = DateUtil.getCurDateTime("yyyyMMdd HHmmss").Split(' ');
        //            userRole.COPRDATE = curDateTime[0];
        //            userRole.COPRTIME = curDateTime[1];

        //            string sql = @"update CODEUSERROLE set 
        //CROLEID = @CROLEID,
        //CENABLEDATE = @CENABLEDATE,
        //CDISABLEDATE = @CDISABLEDATE,
        //COPRDATE = @COPRDATE,
        //COPRTIME = @COPRTIME
        // where CAGENTID = @KEYAGENTID 
        //   and CROLEID = @KEYROLEID
        //   and CENABLEDATE = @KEYENABLEDATE
        //   and CDISABLEDATE = @KEYDISABLEDATE
        //";
        //            SqlCommand command = conn.CreateCommand();

        //            command.Connection = conn;
        //            command.Transaction = transaction;

        //            try
        //            {
        //                command.CommandText = sql;
        //                command.Parameters.AddWithValue("@CROLEID", userRole.CROLEID.Trim());
        //                command.Parameters.AddWithValue("@CENABLEDATE", userRole.CENABLEDATE.Trim());
        //                command.Parameters.AddWithValue("@CDISABLEDATE", userRole.CDISABLEDATE.Trim());
        //                command.Parameters.AddWithValue("@COPRDATE", userRole.COPRDATE.Trim());
        //                command.Parameters.AddWithValue("@COPRTIME", userRole.COPRTIME.Trim());

        //                command.Parameters.AddWithValue("@KEYAGENTID", keyAgentID);
        //                command.Parameters.AddWithValue("@KEYROLEID", keyRoleID);
        //                command.Parameters.AddWithValue("@KEYENABLEDATE", keyEnableDate);
        //                command.Parameters.AddWithValue("@KEYDISABLEDATE", keyDisableDate);

        //                int cnt = command.ExecuteNonQuery();

        //                return userRole;
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //        }



        /// <summary>
        /// 以鍵項刪除使用者角色資料
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int delete(CODE_USER_ROLE userRole, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
        delete  CODE_USER_ROLE 
         where USER_ID = @USER_ID 
           and ROLE_ID = @ROLE_ID
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@USER_ID", StringUtil.toString(userRole.USER_ID));
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(userRole.ROLE_ID));



                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        //        /// <summary>
        //        /// 以"身份證字號"為鍵項，刪除使用者角色
        //        /// </summary>
        //        /// <param name="cAgentID"></param>
        //        /// <param name="conn"></param>
        //        /// <param name="transaction"></param>
        //        public void delByAgentID(string cAgentID, SqlConnection conn, SqlTransaction transaction)
        //        {
        //            string sql = @"
        //delete  CODEUSERROLE 
        // where CAGENTID = @CAGENTID 
        //";
        //            SqlCommand command = conn.CreateCommand();

        //            command.Connection = conn;
        //            command.Transaction = transaction;

        //            try
        //            {
        //                command.CommandText = sql;
        //                command.Parameters.AddWithValue("@CAGENTID", cAgentID.Trim());

        //                int cnt = command.ExecuteNonQuery();

        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //        }



        public String logContent(CODE_USER_ROLE userRole)
        {
            String content = "";

            content += StringUtil.toString(userRole.USER_ID) + '|';
            content += StringUtil.toString(userRole.ROLE_ID) + '|';
            content += StringUtil.toString(userRole.CREATE_UID) + '|';
            content += userRole.CREATE_DT == null ? "|" : userRole.CREATE_DT + "|";


            return content;
        }

    }

}
