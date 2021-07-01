using SSO.Web.BO;
using SSO.Web.Utils;
using SSO.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SSO.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;


namespace SSO.Web.Daos
{
    public class AplyRecDao
    {
        public SSO_APLY_REC qryByKey(String aplyNo)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    SSO_APLY_REC authAppr = db.SSO_APLY_REC.Where(x => x.APLY_NO == aplyNo).FirstOrDefault<SSO_APLY_REC>();

                    return authAppr;
                }
            }
        }

        public SSO_APLY_REC qryByFreeRole(String roleId)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    SSO_APLY_REC authAppr = db.SSO_APLY_REC.Where(x => x.APPR_MAPPING_KEY == roleId & x.APPR_STATUS == "1").FirstOrDefault<SSO_APLY_REC>();

                    return authAppr;
                }
            }
        }


        /// <summary>
        /// 角色權限覆核功能(查詢待覆核資料)
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <param name="cReviewFlag"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public List<AuthReviewModel> qryAuthReview(string[] unitArry, string mgrUnit, string cReviewType, string cReviewFlag, dbFGLEntities db)
        {
           
            bool bReviewType = StringUtil.isEmpty(cReviewType);
            bool bReviewFlag = StringUtil.isEmpty(cReviewFlag);

            List<AuthReviewModel> authReviewList = new List<AuthReviewModel>();

            if ("R".Equals(cReviewType))
            {
                authReviewList = (from m in db.SSO_APLY_REC
                                      //join cType in db.TypeDefine.Where(x => x.CTYPE == "reviewType") on m.cReviewType equals cType.CCODE into psCType
                                      //from xType in psCType.DefaultIfEmpty()

                                  join cSts in db.SYS_CODE.Where(x => x.CODE_TYPE == "APPR_STATUS" & x.SYS_CD == "SSO") on m.APPR_STATUS equals cSts.CODE into psCSts
                                  from xCSts in psCSts.DefaultIfEmpty()

                                      //join user in db.CODEUSER on m.cCrtUserID equals user.CUSERID into psUser
                                      //from xUser in psUser.DefaultIfEmpty()

                                  join role in db.CODE_ROLE on m.APPR_MAPPING_KEY equals role.ROLE_ID into psRole
                                  from xRole in psRole.DefaultIfEmpty()

                                  join roleM in db.CODE_ROLE_HIS on m.APLY_NO equals roleM.APLY_NO into psRoleM
                                  from xRoleM in psRoleM.DefaultIfEmpty()

                                  where (bReviewType || m.APLY_TYPE.Trim() == cReviewType)
                                      & (bReviewFlag || m.APPR_STATUS.Trim() == cReviewFlag)
                                      & (m.CREATE_UNIT == mgrUnit
                                            || unitArry.Contains(xRole.AUTH_UNIT)
                                            || unitArry.Contains(xRoleM.AUTH_UNIT)
                                            || unitArry.Contains(m.CREATE_UNIT))
                                  select new AuthReviewModel
                                  {

                                      aplyNo = m.APLY_NO.Trim(),
                                      //cReviewType = m.cReviewType.Trim(),
                                      //cReviewTypeDesc = xType == null ? "" : xType.CVALUE.Trim(),
                                      apprStatus = m.APPR_STATUS.Trim(),
                                      apprStatusDesc = xCSts == null ? "" : xCSts.CODE_VALUE.Trim(),
                                      authUnit = xRole == null ? (xRoleM == null ? "" : xRoleM.AUTH_UNIT.Trim()) : xRole.AUTH_UNIT.Trim(),
                                      createUid = m.CREATE_UID,
                                      createDt = m.CREATE_DT == null ? "" : SqlFunctions.DateName("year", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DatePart("m", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DateName("day", m.CREATE_DT).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.CREATE_DT).Trim(),
                                      cMappingKey = m.APPR_MAPPING_KEY.Trim() + (xRole == null ? "" : xRole.ROLE_NAME.Trim()),
                                      cMappingKeyDesc = xRole == null ? (xRoleM == null ? "" : xRoleM.ROLE_NAME.Trim()) : xRole.ROLE_NAME.Trim()

                                  }).ToList();

            }
            else if ("U".Equals(cReviewType))
            {
                authReviewList = (from m in db.SSO_APLY_REC

                                  join cUser in db.CODE_USER on m.APPR_MAPPING_KEY equals cUser.USER_ID into psUser
                                  from xUser in psUser.DefaultIfEmpty()

                                  join cUserRole in db.CODE_USER_ROLE_HIS on m.APLY_NO equals cUserRole.APLY_NO into psUserRole
                                  from xUserRole in psUserRole.DefaultIfEmpty()

                                  join cRole in db.CODE_ROLE on xUserRole.ROLE_ID equals cRole.ROLE_ID into psRole
                                  from xRole in psRole.DefaultIfEmpty()

                                  join cSts in db.SYS_CODE.Where(x => x.CODE_TYPE == "APPR_STATUS" & x.SYS_CD == "SSO") on m.APPR_STATUS equals cSts.CODE into psCSts
                                  from xCSts in psCSts.DefaultIfEmpty()


                                  where (bReviewType || m.APLY_TYPE.Trim() == cReviewType)
                                      & (bReviewFlag || m.APPR_STATUS.Trim() == cReviewFlag)
                                     // & (m.CREATE_UNIT == mgrUnit || unitArry.Contains(m.CREATE_UNIT))
                                  select new AuthReviewModel
                                  {

                                      aplyNo = m.APLY_NO.Trim(),
                                      //cReviewType = m.cReviewType.Trim(),
                                      //cReviewTypeDesc = xType == null ? "" : xType.CVALUE.Trim(),
                                      authUnit = xRole == null ? "" : xRole.AUTH_UNIT,
                                      userUnit = xUser.USER_UNIT,
                                      //freeAuth = xRole == null ? "N" : xRole.FREE_AUTH,
                                      apprStatus = m.APPR_STATUS.Trim(),
                                      apprStatusDesc = xCSts == null ? "" : xCSts.CODE_VALUE.Trim(),
                                      createUid = m.CREATE_UID,
                                      createDt = m.CREATE_DT == null ? "" : SqlFunctions.DateName("year", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DatePart("m", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DateName("day", m.CREATE_DT).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.CREATE_DT).Trim(),
                                      cMappingKey = m.APPR_MAPPING_KEY.Trim()

                                  }).Distinct().ToList();

            }
            


            return authReviewList;
        }



        /// <summary>
        /// 新增覆核資料
        /// </summary>
        /// <param name="authReview"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public string insert(SSO_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("SSO", "G7", qPreCode).ToString();



            try
            {
                aplyRec.APLY_NO = "G7"+qPreCode + cId.ToString().PadLeft(3, '0');
                aplyRec.CREATE_DT = DateTime.Now;

                string sql = @"
        INSERT INTO [SSO_APLY_REC]
                   ([APLY_NO]
                   ,[APLY_TYPE]
                   ,[APPR_STATUS]
                   ,[APPR_MAPPING_KEY]
                   ,[CREATE_UID]
                   ,[CREATE_DT]
                   ,[CREATE_UNIT])
             VALUES
                   (@APLY_NO
                   ,@APLY_TYPE
                   ,@APPR_STATUS
                   ,@APPR_MAPPING_KEY
                   ,@CREATE_UID
                   ,@CREATE_DT
                   ,@CREATE_UNIT)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyRec.APLY_NO));
                cmd.Parameters.AddWithValue("@APLY_TYPE", StringUtil.toString(aplyRec.APLY_TYPE));
                cmd.Parameters.AddWithValue("@APPR_STATUS", StringUtil.toString(aplyRec.APPR_STATUS));
                cmd.Parameters.AddWithValue("@APPR_MAPPING_KEY", StringUtil.toString(aplyRec.APPR_MAPPING_KEY));
                cmd.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(aplyRec.CREATE_UID));
                cmd.Parameters.AddWithValue("@CREATE_DT", aplyRec.CREATE_DT);
                cmd.Parameters.AddWithValue("@CREATE_UNIT", aplyRec.CREATE_UNIT);

                int cnt = cmd.ExecuteNonQuery();


                return aplyRec.APLY_NO;
            }
            catch (Exception e)
            {

                throw e;
            }


        }


        /// <summary>
        /// 異動覆核狀態
        /// </summary>
        /// <param name="authReview"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStatus(SSO_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"update [SSO_APLY_REC]
                    set [APPR_STATUS] = @APPR_STATUS
                   ,[APPR_UID] = @APPR_UID
                   ,[APPR_DT] = @APPR_DT
                   ,[LAST_UPDATE_UID] = @LAST_UPDATE_UID
                   ,[LAST_UPDATE_DT] = @LAST_UPDATE_DT
             where  APLY_NO = @APLY_NO
        ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aplyRec.APLY_NO.Trim());
                cmd.Parameters.AddWithValue("@APPR_STATUS", aplyRec.APPR_STATUS.Trim());
                cmd.Parameters.AddWithValue("@APPR_UID", aplyRec.APPR_UID.Trim());
                cmd.Parameters.AddWithValue("@APPR_DT", aplyRec.APPR_DT);
                cmd.Parameters.AddWithValue("@LAST_UPDATE_UID", aplyRec.LAST_UPDATE_UID);
                cmd.Parameters.AddWithValue("@LAST_UPDATE_DT", aplyRec.LAST_UPDATE_DT);

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }



        }
    }
}