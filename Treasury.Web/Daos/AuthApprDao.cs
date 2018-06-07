using Treasury.WebBO;
using Treasury.WebModels;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;

namespace Treasury.WebDaos
{
    public class AuthApprDao
    {
        public AUTH_APPR qryByKey(String aplyNo)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    AUTH_APPR authAppr = db.AUTH_APPR.Where(x => x.APLY_NO == aplyNo).FirstOrDefault<AUTH_APPR>();

                    return authAppr;
                }
            }
        }

        public AUTH_APPR qryByFreeRole(String roleId)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    AUTH_APPR authAppr = db.AUTH_APPR.Where(x => x.APPR_MAPPING_KEY == roleId & x.APPR_STATUS == "1").FirstOrDefault<AUTH_APPR>();

                    return authAppr;
                }
            }
        }

        //        public AuthReview qryNextData(string cReviewSeq, string cMappingKey)
        //        {

        //            using (DbAccountEntities db = new DbAccountEntities())
        //            {

        //                AuthReview authReview = db.AuthReview
        //                    .Where(x => x.cReviewSeq.CompareTo(cReviewSeq) > 0 && x.cReviewFlag == "2" && x.cMappingKey == cMappingKey)
        //                    .OrderByDescending(x => x.cReviewSeq)
        //                    .FirstOrDefault<AuthReview>();

        //                return authReview;
        //            }
        //        }


        //        public AuthReview qryPreData(string cReviewSeq, string cMappingKey)
        //        {

        //            using (DbAccountEntities db = new DbAccountEntities())
        //            {

        //                AuthReview authReview = db.AuthReview
        //                    .Where(x => x.cReviewSeq.CompareTo(cReviewSeq) < 0 && x.cReviewFlag == "2" && x.cMappingKey == cMappingKey)
        //                    .OrderByDescending(x => x.cReviewSeq)
        //                    .FirstOrDefault<AuthReview>();

        //                return authReview;
        //            }
        //        }


        //public List<AuthReviewModel> qryAuthReviewQry(AuthReviewQryModel authReviewQryModel, DB_INTRAEntities db)
        //{
        //    bool bReviewType = StringUtil.isEmpty(authReviewQryModel.cReviewType);
        //    bool bRoleId = StringUtil.isEmpty(authReviewQryModel.cRoleId);
        //    bool bUserId = StringUtil.isEmpty(authReviewQryModel.cUserId);
        //    bool bReviewFlag = StringUtil.isEmpty(authReviewQryModel.cReviewFlag);
        //    bool bReviewUserID = StringUtil.isEmpty(authReviewQryModel.cReviewUserID);
        //    bool bReviewDateB = StringUtil.isEmpty(authReviewQryModel.cReviewDateB);
        //    bool bReviewDateE = StringUtil.isEmpty(authReviewQryModel.cReviewDateE);
        //    bool bCrtUserID = StringUtil.isEmpty(authReviewQryModel.cCrtUserID);
        //    bool bCrtDateB = StringUtil.isEmpty(authReviewQryModel.cCrtDateB);
        //    bool bCrtDateE = StringUtil.isEmpty(authReviewQryModel.cCrtDateE);

        //    List<AuthReviewModel> authReviewList = (from m in db.AuthReview
        //                                            join cType in db.TypeDefine.Where(x => x.CTYPE == "reviewType") on m.cReviewType equals cType.CCODE into psCType
        //                                            from xType in psCType.DefaultIfEmpty()

        //                                            join cSts in db.TypeDefine.Where(x => x.CTYPE == "reviewSts") on m.cReviewFlag equals cSts.CCODE into psCSts
        //                                            from xCSts in psCSts.DefaultIfEmpty()

        //                                            join user in db.CODEUSER on m.cCrtUserID equals user.CUSERID into psUser
        //                                            from xUser in psUser.DefaultIfEmpty()

        //                                            join userR in db.CODEUSER on m.cReviewUserID equals userR.CUSERID into psUserR
        //                                            from xUserR in psUserR.DefaultIfEmpty()

        //                                            join role in db.CODEROLE on m.cMappingKey equals role.CROLEID into psRole
        //                                            from xRole in psRole.DefaultIfEmpty()

        //                                            join userM in db.CODEUSER on m.cMappingKey equals userM.CAGENTID into psUserM
        //                                            from xUserM in psUserM.DefaultIfEmpty()

        //                                            where (bReviewType || m.cReviewType.Trim() == authReviewQryModel.cReviewType)
        //                                                & (bRoleId || m.cMappingKey.Trim() == authReviewQryModel.cRoleId.Trim())
        //                                                & (bUserId || m.cMappingKey.Trim() == authReviewQryModel.cUserId.Trim())
        //                                                & (bReviewFlag || m.cReviewFlag.Trim() == authReviewQryModel.cReviewFlag)
        //                                                & (bReviewUserID || m.cReviewUserID.Trim() == authReviewQryModel.cReviewUserID)
        //                                                & (bReviewDateB || m.cReviewDate.Trim().CompareTo(authReviewQryModel.cReviewDateB.Replace("-", "")) >= 0)
        //                                                & (bReviewDateE || m.cReviewDate.Trim().CompareTo(authReviewQryModel.cReviewDateE.Replace("-", "")) <= 0)
        //                                                & (bCrtUserID || m.cCrtUserID.Trim() == authReviewQryModel.cCrtUserID)
        //                                                & (bCrtDateB || m.cCrtDate.Trim().CompareTo(authReviewQryModel.cCrtDateB.Replace("-", "")) >= 0)
        //                                                & (bCrtDateE || m.cCrtDate.Trim().CompareTo(authReviewQryModel.cCrtDateE.Replace("-", "")) <= 0)
        //                                            select new AuthReviewModel
        //                                            {

        //                                                cReviewSeq = m.cReviewSeq.Trim(),
        //                                                cReviewType = m.cReviewType.Trim(),
        //                                                cReviewTypeDesc = xType == null ? "" : xType.CVALUE.Trim(),
        //                                                cReviewFlag = m.cReviewFlag.Trim(),
        //                                                cReviewFlagDesc = xCSts == null ? "" : xCSts.CVALUE.Trim(),
        //                                                cCrtUserID = m.cCrtUserID.Trim() + (xUser == null ? String.Empty : xUser.CUSERNAME.Trim()),
        //                                                cCrtDate = m.cCrtDate + " " + m.cCrtTime,

        //                                                cReviewUserID = m.cReviewUserID.Trim() + (xUserR == null ? String.Empty : xUserR.CUSERNAME.Trim()),
        //                                                cReviewDate = m.cReviewDate + " " + m.cReviewTime,

        //                                                cMappingKey = m.cMappingKey.Trim(),
        //                                                cMappingKeyDesc = m.cReviewType == "U" ? xUserM.CUSERNAME.Trim() : xRole.CROLENAME.Trim(),

        //                                                cReviewMemo = m.cReviewMemo.Trim()
        //                                            }).OrderByDescending(x => x.cReviewSeq).ToList();


        //    return authReviewList;
        //}


        //        /// <summary>
        //        /// 查詢使用者指派單位異動資料
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <param name="db"></param>
        //        /// <returns></returns>
        //        public List<CodeUserMUnitHisModel> qryUserUnitHis(String cReviewSeq, DbAccountEntities db)
        //        {
        //            List<CodeUserMUnitHisModel> rows = (from m in db.AuthReview
        //                                           join his in db.CodeUserMaintainUnitHis on m.cReviewSeq equals his.cReviewSeq
        //                                           join cType in db.TypeDefine.Where(x => x.CTYPE == "execType") on his.cExecType equals cType.CCODE into psCType
        //                                           from xType in psCType.DefaultIfEmpty()

        //                                           where m.cReviewSeq == cReviewSeq
        //                                           select new CodeUserMUnitHisModel
        //                                           {
        //                                               cReviewSeq = m.cReviewSeq.Trim(),
        //                                               cAgentID = his.cAgentID.Trim(),
        //                                               cUnitCode = his.cUnitCode.Trim(),
        //                                               cUnitSeq = his.cUnitSeq.Trim(),
        //                                               cUnitName = his.cUnitName.Trim(),
        //                                               cEnableDate = his.cEnableDate.Trim(),
        //                                               cDisableDate = his.cDisableDate.Trim(),
        //                                               cExecType = his.cExecType,
        //                                               cExecTypeDesc = xType == null ? "" : xType.CVALUE.Trim()
        //                                           }).ToList();

        //            return rows;
        //        }


        //        /// <summary>
        //        /// 查詢使用者角色異動功能
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <param name="db"></param>
        //        /// <returns></returns>
        //        public List<UserRoleHisModel> qryUserRoleHis(String cReviewSeq, DbAccountEntities db)
        //        {
        //            List<UserRoleHisModel> rows = (from m in db.AuthReview
        //                                           join his in db.CodeUserRoleHis on m.cReviewSeq equals his.cReviewSeq
        //                                           join cType in db.TypeDefine.Where(x => x.CTYPE == "execType") on his.cExecType equals cType.CCODE into psCType
        //                                           from xType in psCType.DefaultIfEmpty()

        //                                           join cRole in db.CODEROLE on his.cRoleID equals cRole.CROLEID into psRole
        //                                           from xRole in psRole.DefaultIfEmpty()

        //                                           where m.cReviewSeq == cReviewSeq
        //                                           select new UserRoleHisModel
        //                                           {
        //                                               cReviewSeq = m.cReviewSeq.Trim(),
        //                                               cAgentID = his.cAgentID.Trim(),
        //                                               cRoleID = his.cRoleID.Trim(),
        //                                               cRoleName = xRole.CROLENAME.Trim(),
        //                                               cEnableDate = his.cEnableDate.Trim(),
        //                                               cDisableDate = his.cDisableDate.Trim(),
        //                                               cExecType = his.cExecType,
        //                                               cExecTypeDesc = xType == null ? "" : xType.CVALUE.Trim()
        //                                           }).ToList();

        //            return rows;
        //        }



        //        /// <summary>
        //        /// 查詢角色異動功能
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <param name="db"></param>
        //        /// <returns></returns>
        //        public List<RoleFuncHisModel> qryRoleFuncHis(String cReviewSeq, DbAccountEntities db)
        //        {
        //            List<RoleFuncHisModel> rows = (from m in db.AuthReview
        //                                              join his in db.CodeRoleFunctionHis on m.cReviewSeq equals his.cReviewSeq
        //                                               join cType in db.TypeDefine.Where(x => x.CTYPE == "execType") on his.cExecType equals cType.CCODE into psCType
        //                                               from xType in psCType.DefaultIfEmpty()

        //                                              join cFunc in db.CODEFUNCTION on his.cFunctionID equals cFunc.CFUNCTIONID into psFunc
        //                                              from xFunc in psFunc.DefaultIfEmpty()

        //                                               where m.cReviewSeq == cReviewSeq
        //                                              select new RoleFuncHisModel
        //                                              {

        //                                                   cReviewSeq = m.cReviewSeq.Trim(),
        //                                                  cRoleID = his.cRoleID.Trim(),
        //                                                  cFunctionID = his.cFunctionID.Trim(),
        //                                                  cFunctionName = xFunc == null ? "" : xFunc.CFUNCTIONNAME.Trim(),
        //                                                  cExecType = his.cExecType,
        //                                                  cExecTypeDesc = xType == null ? "" : xType.CVALUE.Trim()
        //                                               }).ToList();


        //            return rows;
        //        }


        /// <summary>
        /// 角色權限覆核功能(查詢待覆核資料)
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <param name="cReviewFlag"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public List<AuthReviewModel> qryAuthReview(String cReviewType, String cReviewFlag, dbTreasuryEntities db)
        {
            bool bReviewType = StringUtil.isEmpty(cReviewType);
            bool bReviewFlag = StringUtil.isEmpty(cReviewFlag);

            List<AuthReviewModel> authReviewList = new List<AuthReviewModel>();

            if ("R".Equals(cReviewType))
            {
                authReviewList = (from m in db.AUTH_APPR
                                      //join cType in db.TypeDefine.Where(x => x.CTYPE == "reviewType") on m.cReviewType equals cType.CCODE into psCType
                                      //from xType in psCType.DefaultIfEmpty()

                                  join cSts in db.SYS_CODE.Where(x => x.CODE_TYPE == "APPR_STATUS") on m.APPR_STATUS equals cSts.CODE into psCSts
                                  from xCSts in psCSts.DefaultIfEmpty()

                                      //join user in db.CODEUSER on m.cCrtUserID equals user.CUSERID into psUser
                                      //from xUser in psUser.DefaultIfEmpty()

                                  join role in db.CODE_ROLE on m.APPR_MAPPING_KEY equals role.ROLE_ID into psRole
                                  from xRole in psRole.DefaultIfEmpty()

                                  join roleM in db.CODE_ROLE_HIS on m.APLY_NO equals roleM.APLY_NO into psRoleM
                                  from xRoleM in psRoleM.DefaultIfEmpty()

                                  join cAuthType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ROLE_AUTH_TYPE") on xRoleM.ROLE_AUTH_TYPE equals cAuthType.CODE into psAuthType
                                  from xAuthType in psAuthType.DefaultIfEmpty()

                                  where (bReviewType || m.AUTH_APLY_TYPE.Trim() == cReviewType)
                                      & (bReviewFlag || m.APPR_STATUS.Trim() == cReviewFlag)
                                  select new AuthReviewModel
                                  {

                                      aplyNo = m.APLY_NO.Trim(),
                                      //cReviewType = m.cReviewType.Trim(),
                                      //cReviewTypeDesc = xType == null ? "" : xType.CVALUE.Trim(),
                                      apprStatus = m.APPR_STATUS.Trim(),
                                      apprStatusDesc = xCSts == null ? "" : xCSts.CODE_VALUE.Trim(),
                                      createUid = m.CREATE_UID,
                                      createDt = m.CREATE_DT == null ? "" : SqlFunctions.DateName("year", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DatePart("m", m.CREATE_DT) + "/" +
                                                 SqlFunctions.DateName("day", m.CREATE_DT).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.CREATE_DT).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.CREATE_DT).Trim(),
                                      cMappingKey = m.APPR_MAPPING_KEY.Trim() + (xRole == null ? "" : xRole.ROLE_NAME.Trim()),
                                      cMappingKeyDesc = xRole == null ? (xRoleM == null ? "" : xRoleM.ROLE_NAME.Trim()) : xRole.ROLE_NAME.Trim(),
                                      roleAuthType = xAuthType == null ? "" : xAuthType.CODE_VALUE.Trim()
                                  }).ToList();

            }
            else {
                authReviewList = (from m in db.AUTH_APPR

                                  join cSts in db.SYS_CODE.Where(x => x.CODE_TYPE == "APPR_STATUS") on m.APPR_STATUS equals cSts.CODE into psCSts
                                  from xCSts in psCSts.DefaultIfEmpty()


                                  where (bReviewType || m.AUTH_APLY_TYPE.Trim() == cReviewType)
                                      & (bReviewFlag || m.APPR_STATUS.Trim() == cReviewFlag)
                                  select new AuthReviewModel
                                  {

                                      aplyNo = m.APLY_NO.Trim(),
                                      //cReviewType = m.cReviewType.Trim(),
                                      //cReviewTypeDesc = xType == null ? "" : xType.CVALUE.Trim(),
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

                                  }).ToList();

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
        public string insert(AUTH_APPR authAppr, SqlConnection conn, SqlTransaction transaction)
        {

            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("G7", qPreCode).ToString();



            try
            {
                authAppr.APLY_NO = "G7"+qPreCode + cId.ToString().PadLeft(3, '0');
                authAppr.CREATE_DT = DateTime.Now;

                string sql = @"
        INSERT INTO [AUTH_APPR]
                   ([APLY_NO]
                   ,[AUTH_APLY_TYPE]
                   ,[APPR_STATUS]
                   ,[APPR_MAPPING_KEY]
                   ,[CREATE_UID]
                   ,[CREATE_DT])
             VALUES
                   (@APLY_NO
                   ,@AUTH_APLY_TYPE
                   ,@APPR_STATUS
                   ,@APPR_MAPPING_KEY
                   ,@CREATE_UID
                   ,@CREATE_DT)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(authAppr.APLY_NO));
                cmd.Parameters.AddWithValue("@AUTH_APLY_TYPE", StringUtil.toString(authAppr.AUTH_APLY_TYPE));
                cmd.Parameters.AddWithValue("@APPR_STATUS", StringUtil.toString(authAppr.APPR_STATUS));
                cmd.Parameters.AddWithValue("@APPR_MAPPING_KEY", StringUtil.toString(authAppr.APPR_MAPPING_KEY));
                cmd.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(authAppr.CREATE_UID));
                cmd.Parameters.AddWithValue("@CREATE_DT", authAppr.CREATE_DT);


                int cnt = cmd.ExecuteNonQuery();


                return authAppr.APLY_NO;
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
        public int updateStatus(AUTH_APPR authAppr, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"update [AUTH_APPR]
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
                cmd.Parameters.AddWithValue("@APLY_NO", authAppr.APLY_NO.Trim());
                cmd.Parameters.AddWithValue("@APPR_STATUS", authAppr.APPR_STATUS.Trim());
                cmd.Parameters.AddWithValue("@APPR_UID", authAppr.APPR_UID.Trim());
                cmd.Parameters.AddWithValue("@APPR_DT", authAppr.APPR_DT);
                cmd.Parameters.AddWithValue("@LAST_UPDATE_UID", authAppr.LAST_UPDATE_UID);
                cmd.Parameters.AddWithValue("@LAST_UPDATE_DT", authAppr.LAST_UPDATE_DT);

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