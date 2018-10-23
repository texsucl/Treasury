using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;

namespace Treasury.Web.Service.Actual
{

    public class TreasuryKeyCheck : ITreasuryKeyCheck
    {
        #region Get Date
        public TreasuryKeyCheckViewModel GetItemId()
        {
            var result = new TreasuryKeyCheckViewModel();
            var emps = new Treasury.Web.Service.Actual.Common().GetEmps(); 
            var All = new SelectOption() { Text = "All", Value = "All" };     
            List<SelectOption> CONTROL_MODE = new List<SelectOption>(); //管控模式
            List<SelectOption> CUSTODY_MODE = new List<SelectOption>(); //保管方式
            List<SelectOption> EMP_NAME = new List<SelectOption>(); //保管人
            List<SelectOption> AGENT_NAME = new List<SelectOption>(); //代理人
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                CONTROL_MODE = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "CONTROL_MODE")
                    .OrderBy(x => x.ISORTBY)
                   .AsEnumerable().Select(x => new SelectOption()
                       {
                           Value = x.CODE,
                           Text = x.CODE_VALUE
                       }).ToList();

                CUSTODY_MODE= db.SYS_CODE.AsNoTracking()
                   .Where(x => x.CODE_TYPE == "CUSTODY_MODE")
                   .OrderBy(x => x.ISORTBY)
                   .AsEnumerable().Select(x => new SelectOption()
                       {
                           Value = x.CODE,
                           Text = x.CODE_VALUE
                       }).ToList();
  

             var  _CRTI_EMP= db.CODE_ROLE_TREA_ITEM.AsNoTracking()//找出保管順序=1的角色代碼
                    .Where(x =>x.CUSTODY_ORDER == 1)
                    .Select(x=> x.ROLE_ID)
                    .Distinct()
                    .ToList();
      
            var _CUR = db.CODE_USER_ROLE.AsNoTracking()
                    .Where(x=>_CRTI_EMP.Contains(x.ROLE_ID))
                    .Select(x=> x.USER_ID)
                    .ToList();

                  EMP_NAME =_CUR.Distinct()
                    .OrderBy(x=>x)
                    .Select(x=>new SelectOption(){
                    Value = x,
                    Text = emps.FirstOrDefault(y => y.USR_ID == x)?.EMP_NAME?.Trim()
                    }).ToList();
                EMP_NAME.Insert(0,All);
            var _CRTI_AGENT= db.CODE_ROLE_TREA_ITEM.AsNoTracking()//找出保管順序=2的角色代碼
                  .Where(x =>x.CUSTODY_ORDER == 2)
                  .Select(x=> x.ROLE_ID)
                  .ToList();

            var _CURs = db.CODE_USER_ROLE.AsNoTracking()
                    .Where(x=>_CRTI_AGENT.Contains(x.ROLE_ID))
                    .Select(x=> x.USER_ID).ToList();

                  AGENT_NAME =_CURs.Distinct()
                    .OrderBy(x=>x)
                    .Select(x=>new SelectOption(){
                    Value = x,
                    Text = emps.FirstOrDefault(y => y.USR_ID == x)?.EMP_NAME?.Trim()
                    }).ToList();

                AGENT_NAME.Insert(0,All);
            }
            result.CONTROL_MODE = CONTROL_MODE;
            result.CUSTODY_MODE = CUSTODY_MODE;
            result.EMP_NAME = EMP_NAME;
            result.AGENT_NAME = AGENT_NAME;
            return result;
        }
        #endregion

        #region Save Data

        #endregion

        #region private function
          
        #endregion
         }
    }
