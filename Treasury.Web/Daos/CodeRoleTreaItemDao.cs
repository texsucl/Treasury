using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using Treasury.WebUtils;

namespace Treasury.Web.Daos
{
    public class CodeRoleTreaItemDao
    {



        /// <summary>
        /// 角色金庫設備報表清單
        /// </summary>
        /// <returns></returns>
        public List<CodeRoleEquipModel> qryRoleEquip()
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
                    var rows = (from main in db.CODE_ROLE_TREA_ITEM

                                join role in db.CODE_ROLE on main.ROLE_ID equals role.ROLE_ID

                                join d in db.TREA_EQUIP on main.TREA_EQUIP_ID equals d.TREA_EQUIP_ID into psEquip
                                from xEquip in psEquip.DefaultIfEmpty()

                                join custody in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on main.CUSTODY_MODE equals custody.CODE into psCustody
                                from xCustody in psCustody.DefaultIfEmpty()

                                join control in db.SYS_CODE.Where(x => x.CODE_TYPE == "CONTROL_MODE") on xEquip.CONTROL_MODE equals control.CODE into psControl
                                from xControl in psControl.DefaultIfEmpty()


                                where 1 == 1
                                    & role.IS_DISABLED == "N"

                                select new CodeRoleEquipModel
                                {

                                    roleId = main.ROLE_ID,
                                    roleName = role.ROLE_NAME.Trim(),
                                    execAction = "",
                                    execActionDesc = "",
                                    treaEquipId = main.TREA_EQUIP_ID,
                                    equipName = xEquip.EQUIP_NAME.Trim(),
                                    controlMode = xEquip.CONTROL_MODE,
                                    controlModeDesc = xControl.CODE_VALUE.Trim(),
                                    custodyMode = main.CUSTODY_MODE,
                                    custodyModeDesc = xCustody.CODE_VALUE.Trim(),
                                    custodyOrder = main.CUSTODY_ORDER.ToString()


                                }).ToList<CodeRoleEquipModel>();
                    return rows;

                }

            }
        }


        //角色管理-查詢金庫設備權限
        public List<CodeRoleEquipModel> qryForRoleMgr(string roleId)
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
                    var rows = (from main in db.CODE_ROLE_TREA_ITEM

                                join d in db.TREA_EQUIP on main.TREA_EQUIP_ID equals d.TREA_EQUIP_ID into psEquip
                                from xEquip in psEquip.DefaultIfEmpty()

                                join custody in db.SYS_CODE.Where(x => x.CODE_TYPE == "CUSTODY_MODE") on main.CUSTODY_MODE equals custody.CODE into psCustody
                                from xCustody in psCustody.DefaultIfEmpty()

                                join control in db.SYS_CODE.Where(x => x.CODE_TYPE == "CONTROL_MODE") on xEquip.CONTROL_MODE equals control.CODE into psControl
                                from xControl in psControl.DefaultIfEmpty()


                                where 1 == 1
                                    & main.ROLE_ID == roleId

                                select new CodeRoleEquipModel
                                {

                                    roleId = main.ROLE_ID,
                                    execAction = "",
                                    execActionDesc = "",
                                    treaEquipId = main.TREA_EQUIP_ID,
                                    equipName = xEquip.EQUIP_NAME.Trim(),
                                    controlMode = xEquip.CONTROL_MODE,
                                    controlModeDesc = xControl.CODE_VALUE.Trim(),
                                    custodyMode = main.CUSTODY_MODE,
                                    custodyModeDesc = xCustody.CODE_VALUE.Trim(),
                                    custodyOrder = main.CUSTODY_ORDER.ToString()


                                }).ToList<CodeRoleEquipModel>();
                    return rows;

                }

                }

        }



        public CODE_ROLE_TREA_ITEM getRoleEquipByKey(string roleId, string treaEquipId)
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
                    CODE_ROLE_TREA_ITEM roleEquip = db.CODE_ROLE_TREA_ITEM.Where(x => x.ROLE_ID == roleId
                && x.TREA_EQUIP_ID == treaEquipId).FirstOrDefault();

                    return roleEquip;
                }
            }
        }




        /**
      新增角色金庫設備資料檔
          **/
        public int Insert(CODE_ROLE_TREA_ITEM roleEquip, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"insert into CODE_ROLE_TREA_ITEM
        (ROLE_ID, TREA_EQUIP_ID, CUSTODY_MODE, CUSTODY_ORDER, LAST_UPDATE_UID, LAST_UPDATE_DT)
        values (@ROLE_ID, @TREA_EQUIP_ID, @CUSTODY_MODE, @CUSTODY_ORDER, @LAST_UPDATE_UID, @LAST_UPDATE_DT)
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleEquip.ROLE_ID));
                command.Parameters.AddWithValue("@TREA_EQUIP_ID", StringUtil.toString(roleEquip.TREA_EQUIP_ID));
                command.Parameters.AddWithValue("@CUSTODY_MODE", StringUtil.toString(roleEquip.CUSTODY_MODE));
                command.Parameters.AddWithValue("@CUSTODY_ORDER", roleEquip.CUSTODY_ORDER);
                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(roleEquip.LAST_UPDATE_UID));
                command.Parameters.AddWithValue("@LAST_UPDATE_DT", DateTime.Now);

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /**
修改角色金庫設備資料檔
  **/
        public int Update(CODE_ROLE_TREA_ITEM roleEquip, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
update CODE_ROLE_TREA_ITEM
set  CUSTODY_MODE = @CUSTODY_MODE
   , CUSTODY_ORDER = @CUSTODY_ORDER
   , LAST_UPDATE_UID = @LAST_UPDATE_UID
   , LAST_UPDATE_DT = @LAST_UPDATE_DT
where ROLE_ID = @ROLE_ID
  and TREA_EQUIP_ID = @TREA_EQUIP_ID
";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleEquip.ROLE_ID));
                command.Parameters.AddWithValue("@TREA_EQUIP_ID", StringUtil.toString(roleEquip.TREA_EQUIP_ID));
                command.Parameters.AddWithValue("@CUSTODY_MODE", StringUtil.toString(roleEquip.CUSTODY_MODE));
                command.Parameters.AddWithValue("@CUSTODY_ORDER", roleEquip.CUSTODY_ORDER);
                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(roleEquip.LAST_UPDATE_UID));
                command.Parameters.AddWithValue("@LAST_UPDATE_DT", DateTime.Now);

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /**
刪除角色金庫設備資料檔
    **/
        public int Delete(CODE_ROLE_TREA_ITEM roleEquip, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete CODE_ROLE_TREA_ITEM
        where 1=1
        and ROLE_ID = @ROLE_ID and TREA_EQUIP_ID = @TREA_EQUIP_ID
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleEquip.ROLE_ID));
                command.Parameters.AddWithValue("@TREA_EQUIP_ID", StringUtil.toString(roleEquip.TREA_EQUIP_ID));

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /**
        將角色金庫設備資料檔的各欄位組成一字串，for Log
            **/
        public String logContent(CODE_ROLE_TREA_ITEM roleEquip)
        {
            String content = "";

            content += StringUtil.toString(roleEquip.ROLE_ID) + '|';
            content += StringUtil.toString(roleEquip.TREA_EQUIP_ID) + '|';
            content += StringUtil.toString(roleEquip.CUSTODY_MODE) + '|';
            content += roleEquip.CUSTODY_ORDER + '|';
            content += StringUtil.toString(roleEquip.LAST_UPDATE_UID) + '|';
            content += roleEquip.LAST_UPDATE_DT;

            return content;
        }

    }
}