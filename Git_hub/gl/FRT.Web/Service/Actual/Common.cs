using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FRT.Web.Controllers;
using FRT.Web.Models;
using FRT.Web.BO;

namespace FRT.Web.Service.Actual
{
    public class Common
    {
        public string GetUser()
        {
            return AccountController.CurrentUserId;
        }

        public List<SYS_CODE> GetSysCodes(string sysCd, List<string> codeTypes)
        {
            if (sysCd.IsNullOrWhiteSpace() || !codeTypes.Any())
                return new List<SYS_CODE>();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                return db.SYS_CODE.AsNoTracking().Where(x =>
                 x.SYS_CD == sysCd && codeTypes.Contains(x.CODE_TYPE)).ToList();
            }
        }

        /// <summary>
        /// get SysCode by CodeType
        /// </summary>
        /// /// <param name="sysCd"></param>
        /// <param name="codeType"></param>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public List<SelectOption> GetSysCode(string sysCd, string codeType, bool isAll = false , bool spaceFlag = false)
        {
            var result = new List<SelectOption>();
            if (sysCd.IsNullOrWhiteSpace() || codeType.IsNullOrWhiteSpace())
                return result;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == sysCd)
                    .Where(x => x.CODE_TYPE == codeType)
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList());

                if(isAll && result.Any())
                    result.Insert(0, new SelectOption() { Text = "All", Value = "All" });
                else if(spaceFlag && result.Any())
                    result.Insert(0, new SelectOption() { Text = " ", Value = " " });
            }
            return result;
        }

        /// <summary>
        /// tuple to selectoption
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="showValue"></param>
        /// <returns></returns>
        public List<SelectOption> tupleToSelectOption(List<Tuple<string, string>> datas, bool showValue = false)
        {
            var result = new List<SelectOption>();
            if (datas.Any())
                result.AddRange(datas.Select(x => new SelectOption()
                {
                    Value = x.Item1,
                    Text = (showValue ? (x.Item1 + " : ") : string.Empty) + x.Item2
                }));
            return result;
        }


        /// <summary>
        /// 獲取 員工資料
        /// </summary>
        /// <returns></returns>
        public List<V_EMPLY2> GetEmps()
        {
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                emps = dbIntra.V_EMPLY2.AsNoTracking().ToList();
            }

            return emps;
        }

        /// <summary>
        /// 獲取 員工名子
        /// </summary>
        /// <param name="userIds">5碼AD</param>
        /// <returns></returns>
        public Dictionary<string, string> GetEmpNames(List<string> userIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var emps = GetEmps();
            foreach (var item in emps.Where(x => userIds.Contains(x.USR_ID)))
            {
                result.Add(item.USR_ID, item.EMP_NAME);
            }
            return result;
        }

    }
}