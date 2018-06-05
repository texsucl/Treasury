using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class TreasuryAccess : ITreasuryAccess
    {

        public TreasuryAccess()
        {

        }

        /// <summary>
        /// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        /// </summary>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>> TreasuryAccessDetail(string cRoleID,bool custodyFlag)
        {
            List<SelectOption> applicationProject = new List<SelectOption>(); //申請項目
            List<SelectOption> applicationUnit = new List<SelectOption>(); //申請單位
            List<SelectOption> applicant = new List<SelectOption>(); //申請人

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {

            }

            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>>(applicationProject, applicationUnit, applicant);
        }

    }
}