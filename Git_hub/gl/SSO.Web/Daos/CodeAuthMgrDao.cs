using SSO.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using SSO.Web.ViewModels;
using SSO.Web.Utils;
using SSO.Web.BO;
using System.Web.Mvc;
using SSO.Web.Utility;

namespace SSO.Web.Daos
{
    public class CodeAuthMgrDao
    {

        /// <summary>
        /// 依管理者單位查詢可授權的單位的下拉選單
        /// </summary>
        /// <param name="mgrUnit"></param>
        /// <param name="bPreCode"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string mgrUnit, bool bPreCode)
        {
            var codeList = qryByMgr(mgrUnit);
            var list = new List<listModel>();

            string controlStr = "";

            OaDeptDao oaDeptDao = new OaDeptDao();
            

            foreach (var item in codeList)
            {
                VW_OA_DEPT dept = new VW_OA_DEPT();
                dept = oaDeptDao.qryByDptCd(item.auth_unit);
                if (!"".Equals(StringUtil.toString(dept.DPT_NAME))) {
                    controlStr = "";
                    if (bPreCode)
                        controlStr += item.auth_unit.Trim() + "." + dept.DPT_NAME.Trim();
                    else
                        controlStr += dept.DPT_NAME.Trim();

                    listModel d = new listModel();
                    d.code_value = item.auth_unit;
                    d.code_text = controlStr;
                    list.Add(d);
                }
            }

            var items = new SelectList
                (
                items: list,
                dataValueField: "code_value",
                dataTextField: "code_text",
                selectedValue: (object)null
                );

            return items;
        }


        public List<CODE_AUTH_MGR> qryByMgr(string mgrUnit)
        {

            List<CODE_AUTH_MGR> rowsMgr = new List<CODE_AUTH_MGR>();

            OaDeptDao oaDeptDao = new OaDeptDao();

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    rowsMgr = db.CODE_AUTH_MGR.Where(x => x.mgr_unit == mgrUnit
                    || (x.mgr_unit.EndsWith("XX") && x.mgr_unit.Substring(0, 3) == mgrUnit.Substring(0, 3))).OrderBy(x => x.auth_unit)
                    .ToList<CODE_AUTH_MGR>();
                }
            }

            return rowsMgr;
        }


        public SelectList qryForAuthRptAuth(string authUnit)
        {

            var userRows = new List<AuthRptModel>();

            //取得所轄OWNER單位...有哪些使用者
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    userRows = (from mgr in db.CODE_AUTH_MGR
                                join codeRole in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on mgr.auth_unit equals codeRole.AUTH_UNIT
                                join userRole in db.CODE_USER_ROLE on codeRole.ROLE_ID equals userRole.ROLE_ID
                                join user in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on userRole.USER_ID equals user.USER_ID
                                where (mgr.auth_unit == authUnit)
                                select new AuthRptModel
                                {
                                    user_id = user.USER_ID,
                                    user_unit = user.USER_UNIT.TrimEnd()
                                }
                          ).Distinct().ToList<AuthRptModel>();
                }
            }

            OaEmpDao oaEmpDao = new OaEmpDao();
            OaDeptDao oaDeptDao = new OaDeptDao();
            List<V_EMPLY2> dataList = new List<V_EMPLY2>();
            Dictionary<string, string> deptDic = new Dictionary<string, string>();
            var itemList = new List<listModel>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                foreach (AuthRptModel d in userRows)
                {
                    listModel item = new listModel();
                    V_EMPLY2 emp = oaEmpDao.qryByUsrId(d.user_id, db);
                    if (emp != null)
                    {
                        if (!deptDic.ContainsKey(StringUtil.toString(emp.DPT_CD)))
                        {
                            item.code_value = StringUtil.toString(emp.DPT_CD);
                            item.code_text = StringUtil.toString(emp.DPT_CD) + " " + StringUtil.toString(emp.DPT_NAME);
                            itemList.Add(item);
                            deptDic.Add(StringUtil.toString(emp.DPT_CD), emp.DPT_NAME);
                        }
                    }
                    else
                    {
                        if (!deptDic.ContainsKey(StringUtil.toString(d.user_unit)))
                        {
                            var dept = oaDeptDao.qryByDptCd(StringUtil.toString(d.user_unit));
                            if (dept != null)
                            {
                                item.code_value = StringUtil.toString(dept.DPT_CD);
                                item.code_text = StringUtil.toString(dept.DPT_CD) + " " + StringUtil.toString(dept.DPT_NAME);
                                itemList.Add(item);
                                deptDic.Add(StringUtil.toString(dept.DPT_CD), dept.DPT_NAME);
                            }
                            else
                            {
                                item.code_value = StringUtil.toString(d.user_unit);
                                item.code_text = StringUtil.toString(d.user_unit);
                                itemList.Add(item);
                                deptDic.Add(StringUtil.toString(d.user_unit), d.user_unit);
                            }
                        }
                    }
                }
            }


            var items = new SelectList
                (
                items: itemList.OrderBy(x => x.code_value),
                dataValueField: "code_value",
                dataTextField: "code_text",
                selectedValue: (object)null
                );

            return items;

        }


        /// <summary>
        /// 依"管理單位"查詢
        /// </summary>
        /// <param name="mgrUnit"></param>
        /// <returns></returns>
        public SelectList qryForAuthRptMgr(string mgrUnit, string qryType)
        {

            var userRows = new List<AuthRptModel>();

            //取得管理者部門所轄OWNER單位...有哪些使用者
            using (new TransactionScope (
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    userRows = (from mgr in db.CODE_AUTH_MGR
                                join codeRole in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on mgr.auth_unit equals codeRole.AUTH_UNIT
                                join userRole in db.CODE_USER_ROLE on codeRole.ROLE_ID equals userRole.ROLE_ID
                                join user in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on userRole.USER_ID equals user.USER_ID
                                where (mgr.mgr_unit == mgrUnit || (mgr.mgr_unit.EndsWith("XX") && mgr.mgr_unit.Substring(0, 3) == mgrUnit.Substring(0, 3)))
                                select new AuthRptModel
                                {
                                    user_id = user.USER_ID,
                                    user_unit = user.USER_UNIT.TrimEnd()
                                }
                          ).Distinct().ToList<AuthRptModel>();
                }
            }

            OaEmpDao oaEmpDao = new OaEmpDao();
            OaDeptDao oaDeptDao = new OaDeptDao();
            List<V_EMPLY2> dataList = new List<V_EMPLY2>();
            Dictionary<string, string> deptDic = new Dictionary<string, string>();
            var itemList = new List<listModel>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                foreach (AuthRptModel d in userRows)
                {
                    listModel item = new listModel();
                    V_EMPLY2 emp = oaEmpDao.qryByUsrId(d.user_id, db);
                    if (emp != null)
                    {
                        if ("user".Equals(qryType))
                        {
                            item.code_value = StringUtil.toString(emp.USR_ID);
                            item.code_text = StringUtil.toString(emp.USR_ID) + " " + StringUtil.toString(emp.EMP_NAME);
                            itemList.Add(item);
                        }
                        else
                        {
                            if (!deptDic.ContainsKey(StringUtil.toString(emp.DPT_CD)))
                            {
                                item.code_value = StringUtil.toString(emp.DPT_CD);
                                item.code_text = StringUtil.toString(emp.DPT_CD) + " " + StringUtil.toString(emp.DPT_NAME);
                                itemList.Add(item);
                                deptDic.Add(StringUtil.toString(emp.DPT_CD), emp.DPT_NAME);
                            }
                        }
                    }
                    else {
                        if ("user".Equals(qryType))
                        {
                            item.code_value = StringUtil.toString(d.user_id);
                            item.code_text = StringUtil.toString(d.user_id);
                            itemList.Add(item);
                        }
                        else
                        {
                            if (!deptDic.ContainsKey(StringUtil.toString(d.user_unit)))
                            {
                                var dept = oaDeptDao.qryByDptCd(StringUtil.toString(d.user_unit));
                                if (dept != null)
                                {
                                    item.code_value = StringUtil.toString(dept.DPT_CD);
                                    item.code_text = StringUtil.toString(dept.DPT_CD) + " " + StringUtil.toString(dept.DPT_NAME);
                                    itemList.Add(item);
                                    deptDic.Add(StringUtil.toString(dept.DPT_CD), dept.DPT_NAME);
                                }
                                else {
                                    item.code_value = StringUtil.toString(d.user_unit);
                                    item.code_text = StringUtil.toString(d.user_unit);
                                    itemList.Add(item);
                                    deptDic.Add(StringUtil.toString(d.user_unit), d.user_unit);
                                }
                            }
                        }
                    }
                }
            }
              

            var items = new SelectList
                (
                items: itemList.OrderBy(x=> x.code_value),
                dataValueField: "code_value",
                dataTextField: "code_text",
                selectedValue: (object)null
                );

            return items;

        }


        /// <summary>
        /// 依"管理單位"查詢
        /// </summary>
        /// <param name="mgrUnit"></param>
        /// <returns></returns>
        public string[] qryAuthUnit(string mgrUnit)
        {
            List<CODE_AUTH_MGR> mgrList = qryByMgr(mgrUnit);
            string[] authUnitArr = mgrList.Select(x => x.auth_unit).ToArray();

            return authUnitArr;
        }

        /// <summary>
        /// 角色下拉選單
        /// </summary>
        /// <param name="mgrUnit"></param>
        /// <param name="rptType"></param>
        public SelectList qryUserRoleList(string mgrUnit, string rptType, List<string> owerUnit = null)
        {
            var codeList = qryByMgr(mgrUnit).Select(x => x.auth_unit);
            List<string> authList = new List<string>();
            List<listModel> rows = new List<listModel>();

            using (new TransactionScope(
       TransactionScopeOption.Required,
       new TransactionOptions
       {
           IsolationLevel = IsolationLevel.ReadUncommitted
       }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if(rptType == "rptOwnerUnit")
                    {
                        authList = db.CODE_AUTH_MGR.AsNoTracking()
                            .Where(x => x.auth_unit == mgrUnit).Select(x => x.auth_unit).ToList();
                    }
                   rows = db.CODE_ROLE.AsNoTracking()
                        .AsEnumerable()
                        .Where(x => x.IS_DISABLED == "N")
                        .Where(x => authList.Contains(x.AUTH_UNIT), rptType == "rptOwnerUnit")
                        .Where(x => codeList.Contains(x.AUTH_UNIT), rptType == "rptMgrUnit")   
                        .Where(x => owerUnit.Contains(x.AUTH_UNIT), rptType == "rptMgrUnit" && owerUnit != null)
                        .Where(x => !String.IsNullOrWhiteSpace(x.AUTH_UNIT))
                        .Select(x => new listModel() {
                            code_value = x.ROLE_ID,
                            code_text = $"{x.ROLE_NAME} ({x.AUTH_UNIT})"
                            //code_text = rptType == "rptMgrUnit" ? $"{x.ROLE_NAME} ({x.AUTH_UNIT})" : x.ROLE_NAME
                        }).ToList();
                }
            }

                var items = new SelectList
               (
               items: rows,
               dataValueField: "code_value",
               dataTextField: "code_text",
               selectedValue: (object)null
               );

            return items;
        }

        public class listModel
        {
            public string code_value { get; set; }

            public string code_text { get; set; }


        }
    }
}