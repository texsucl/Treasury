using Treasury.WebBO;
using Treasury.WebModels;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebUtils;
using Treasury.Web.ViewModels;
using System.Transactions;
using System.Data.Entity.SqlServer;

namespace Treasury.WebDaos
{
    public class CodeRoleTreaItemHisDao
    {
        /// <summary>
        /// 查詢修改前後的資料
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cRoleID"></param>
        /// <param name="apprStatus"></param>
        /// <param name="updDateB"></param>
        /// <param name="updDateE"></param>
        /// <returns></returns>
        public List<CodeRoleEquipModel> qryForRoleMgrHis(dbTreasuryEntities db, string cRoleID, string apprStatus, string updDateB, string updDateE)
        {
            bool bapprStatus = StringUtil.isEmpty(apprStatus);
            bool bDateB = StringUtil.isEmpty(updDateB);
            bool bDateE = StringUtil.isEmpty(updDateE);

            DateTime sB = DateTime.Now.AddDays(1);
            if (!bDateB)
            {
                sB = Convert.ToDateTime(updDateB);
            }
            DateTime sE = DateTime.Now.AddDays(1);
            if (!bDateE)
            {
                sE = Convert.ToDateTime(updDateE);
            }
            sE = sE.AddDays(1);


            
                var roleEquipHis = (from m in db.CODE_ROLE_TREA_ITEM_HIS

                                    join appr in db.AUTH_APPR on m.APLY_NO equals appr.APLY_NO into psAppr
                                    from xAppr in psAppr.DefaultIfEmpty()

                                    join equip in db.TREA_EQUIP on m.TREA_EQUIP_ID equals equip.TREA_EQUIP_ID into psEquip
                                    from xEquip in psEquip.DefaultIfEmpty()

                                    join cCon in db.SYS_CODE.Where(x => x.CODE_TYPE == "CONTROL_MODE") on xEquip.CONTROL_MODE equals cCon.CODE into psCon
                                    from xCon in psCon.DefaultIfEmpty()

                                    join cCust in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on m.CUSTODY_MODE equals cCust.CODE into psCust
                                    from xCust in psCust.DefaultIfEmpty()

                                    join cCustB in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on m.CUSTODY_MODE_B equals cCustB.CODE into psCustB
                                    from xCustB in psCustB.DefaultIfEmpty()


                                    where m.ROLE_ID == cRoleID
                                     & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                                     & (bDateB || xAppr.CREATE_DT >= sB)
                                     & (bDateE || xAppr.CREATE_DT <= sE)
                                    select new CodeRoleEquipModel
                                    {
                                        aplyNo = m.APLY_NO.Trim(),
                                        apprUid = xAppr.APPR_UID.Trim(),
                                        apprStatus = xAppr.APPR_STATUS.Trim(),
                                        updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                                 SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                                 SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                                        updateUid = xAppr.CREATE_UID.Trim(),
                                        execAction = m.EXEC_ACTION.Trim(),

                                        equipName = xEquip.EQUIP_NAME.Trim(),
                                        controlMode = xEquip.CONTROL_MODE,
                                        controlModeDesc = xCon == null ? "" : xCon.CODE_VALUE.Trim(),

                                       custodyMode = m.CUSTODY_MODE.Trim(),
                                       custodyModeDesc = xCust == null ? "" : xCust.CODE_VALUE.Trim(),
                                       custodyModeB = m.CUSTODY_MODE_B.Trim(),
                                       custodyModeDescB = xCustB == null ? "" : xCustB.CODE_VALUE.Trim(),

                                       custodyOrder = m.CUSTODY_ORDER == null ? "" : m.CUSTODY_ORDER.ToString(),
                                       custodyOrderB = m.CUSTODY_ORDER_B == null ? "" : m.CUSTODY_ORDER_B.ToString()


                                   }).ToList<CodeRoleEquipModel>();

                return roleEquipHis;
            
        }


        public List<CodeRoleEquipModel> qryByAplyNo(String aplyNo)
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
                    List<CodeRoleEquipModel> rows = (from main in db.CODE_ROLE_TREA_ITEM_HIS
                                                   join equip in db.TREA_EQUIP on main.TREA_EQUIP_ID equals equip.TREA_EQUIP_ID

                                                     join cType in db.SYS_CODE.Where(x => x.CODE_TYPE == "EXEC_ACTION") on main.EXEC_ACTION equals cType.CODE into psCType
                                                     from xType in psCType.DefaultIfEmpty()

                                                     join cCust in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on main.CUSTODY_MODE equals cCust.CODE into psCust
                                                     from xCust in psCust.DefaultIfEmpty()

                                                     join cCustB in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on main.CUSTODY_MODE_B equals cCustB.CODE into psCustB
                                                     from xCustB in psCustB.DefaultIfEmpty()

                                                   where main.APLY_NO == aplyNo
                                                   select new CodeRoleEquipModel
                                                   {
                                                       treaEquipId = main.TREA_EQUIP_ID.Trim(),
                                                       execAction = main.EXEC_ACTION.Trim(),
                                                       execActionDesc = xType == null ? "" : xType.CODE_VALUE.Trim(),
                                                       equipName = equip.EQUIP_NAME.Trim(),
                                                       custodyMode = main.CUSTODY_MODE,
                                                       custodyModeDesc = xCust.CODE_VALUE.Trim(),
                                                       custodyModeB = main.CUSTODY_MODE_B,
                                                       custodyModeDescB = xCustB.CODE_VALUE.Trim(),
                                                       custodyOrder = main.CUSTODY_ORDER.ToString(),
                                                       custodyOrderB = main.CUSTODY_ORDER_B.ToString()
                                                   }).ToList();

                    return rows;
                }
            }
        }



        //        /// <summary>
        //        /// 查詢修改前後的資料
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <param name="db"></param>
        //        /// <returns></returns>
        //        public AuthReviewRoleModel qryByNowHis(string cReviewSeq, DbAccountEntities db)
        //        {

        //            using (db)
        //            {
        //                //AuthReviewRoleModel roleHis = new AuthReviewRoleModel();
        //                var roleHis = (from m in db.AuthReview
        //                               join main in db.CODEROLE on m.cMappingKey equals main.CROLEID into psMain
        //                               from xMain in psMain.DefaultIfEmpty()

        //                               join his in db.CodeRoleHis on m.cReviewSeq equals his.cReviewSeq into psHis
        //                               from xHis in psHis.DefaultIfEmpty()

        //                               join user in db.CODEUSER on m.cCrtUserID equals user.CUSERID into psUser
        //                           from xUser in psUser.DefaultIfEmpty()

        //                           join codeOpr in db.TypeDefine.Where(x => x.CTYPE == "OprArea") on xMain.COPERATORAREA equals codeOpr.CCODE into psOpr
        //                           from xOpr in psOpr.DefaultIfEmpty()

        //                           join codeSrch in db.TypeDefine.Where(x => x.CTYPE == "SrchArea") on xMain.CSEARCHAREA equals codeSrch.CCODE into psSrch
        //                           from xSrch in psSrch.DefaultIfEmpty()

        //                           join codeFlag in db.TypeDefine.Where(x => x.CTYPE == "cFlag") on xMain.CFLAG equals codeFlag.CCODE into psFlag
        //                           from xFlag in psFlag.DefaultIfEmpty()

        //                           join codeOprH in db.TypeDefine.Where(x => x.CTYPE == "OprArea") on xHis.cOperatorArea equals codeOprH.CCODE into psOprH
        //                           from xOprH in psOprH.DefaultIfEmpty()

        //                           join codeSrchH in db.TypeDefine.Where(x => x.CTYPE == "SrchArea") on xHis.cSearchArea equals codeSrchH.CCODE into psSrchH
        //                           from xSrchH in psSrchH.DefaultIfEmpty()

        //                           join codeFlagH in db.TypeDefine.Where(x => x.CTYPE == "cFlag") on xHis.cFlag equals codeFlagH.CCODE into psFlagH
        //                           from xFlagH in psFlagH.DefaultIfEmpty()

        //                               join cSts in db.TypeDefine.Where(x => x.CTYPE == "reviewSts") on m.cReviewFlag equals cSts.CCODE into psCSts
        //                               from xCSts in psCSts.DefaultIfEmpty()

        //                               where m.cReviewSeq == cReviewSeq
        //                            select new AuthReviewRoleModel
        //                            {
        //                                cReviewSeq = m.cReviewSeq.Trim(),
        //                                cCrtUserID = m.cCrtUserID.Trim() + (xUser == null ? String.Empty : xUser.CUSERNAME.Trim()),
        //                                cCrtDateTime = m.cCrtDate + " " + m.cCrtTime,
        //                                cRoleID = xMain.CROLEID.Trim(),
        //                                cRoleName = xMain.CROLENAME.Trim(),
        //                                cRoleNameHis = xHis.cRoleName.Trim(),
        //                                cSearchArea = xSrch.CVALUE.Trim(),
        //                                cSearchAreaHis = xSrchH.CVALUE.Trim(),
        //                                cOperatorArea = xOpr.CVALUE.Trim(),
        //                                cOperatorAreaHis = xOprH.CVALUE.Trim(),
        //                                cFlag = xFlag.CVALUE.Trim(),
        //                                cFlagHis = xFlagH.CVALUE.Trim(),
        //                                vMemo = xMain.VMEMO.Trim(),
        //                                vMemoHis = xHis.vMemo.Trim(),
        //                                cReviewMemo = m.cReviewMemo.Trim(),
        //                                cReviewFlagDesc = xCSts.CVALUE.Trim(),
        //                                cReviewUser = m.cReviewUserID.Trim() + m.cReviewUserName.Trim(),
        //                                cReviewDate = m.cReviewDate + " " + m.cReviewTime
        //                            }).FirstOrDefault();

        //                return roleHis;
        //            }
        //        }


        //        /// <summary>
        //        /// 以"覆核單號"為鍵項查詢
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <returns></returns>
        //        public CodeRoleHis qryByKey(String cReviewSeq)
        //        {

        //            using (DbAccountEntities db = new DbAccountEntities())
        //            {
        //                CodeRoleHis codeRoleHis = db.CodeRoleHis.Where(x => x.cReviewSeq == cReviewSeq).FirstOrDefault<CodeRoleHis>();

        //                return codeRoleHis;
        //            }
        //        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="codeRoleHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aplyNo, CodeRoleEquipModel codeRoleEquipModel, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
        

                string sql = @"
        INSERT INTO [CODE_ROLE_TREA_ITEM_HIS]
                   ([APLY_NO]
                   ,[ROLE_ID]
                   ,[TREA_EQUIP_ID]
                   ,[CUSTODY_MODE]
                   ,[CUSTODY_ORDER]
                   ,[CUSTODY_MODE_B]
                   ,[CUSTODY_ORDER_B]
                   ,[EXEC_ACTION])
             VALUES
                   (@APLY_NO
                   ,@ROLE_ID
                   ,@TREA_EQUIP_ID
                   ,@CUSTODY_MODE
                   ,@CUSTODY_ORDER
                   ,@CUSTODY_MODE_B
                   ,@CUSTODY_ORDER_B
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(codeRoleEquipModel.roleId));
                cmd.Parameters.AddWithValue("@TREA_EQUIP_ID", StringUtil.toString(codeRoleEquipModel.treaEquipId));
                cmd.Parameters.AddWithValue("@CUSTODY_MODE", StringUtil.toString(codeRoleEquipModel.custodyMode));
                cmd.Parameters.AddWithValue("@CUSTODY_ORDER", StringUtil.toString(codeRoleEquipModel.custodyOrder));
                cmd.Parameters.AddWithValue("@CUSTODY_MODE_B", StringUtil.toString(codeRoleEquipModel.custodyModeB));
                cmd.Parameters.AddWithValue("@CUSTODY_ORDER_B", StringUtil.toString(codeRoleEquipModel.custodyOrderB));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(codeRoleEquipModel.execAction));

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }


        }
    }
}