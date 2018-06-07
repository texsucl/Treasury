using System;
using System.Collections.Generic;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
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
        /// <param name="cUserID">userId</param>
        /// <param name="custodyFlag">管理科Flag</param>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>> TreasuryAccessDetail(string cUserID, bool custodyFlag)
        {
            List<SelectOption> applicationProject = new List<SelectOption>(); //申請項目
            List<SelectOption> applicationUnit = new List<SelectOption>(); //申請單位
            List<SelectOption> applicant = new List<SelectOption>(); //申請人

            try
            {
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    var depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
                        if (custodyFlag) //是保管科人員
                        {
                            applicationProject = db.TREA_ITEM.AsNoTracking()
                                .Where(x => x.IS_DISABLED == "N" && x.ITEM_OP_TYPE == "3") //「入庫作業類型=3」且啟用中的存取項目
                                .OrderBy(x => x.ITEM_ID)
                                .AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = $@"{x.TREA_ITEM_TYPE}{x.TREA_ITEM_NAME}",
                                    Text = x.ITEM_DESC
                                }).ToList();

                            applicationUnit = db.ITEM_CHARGE_UNIT.AsNoTracking()
                                .Where(x => x.IS_DISABLED == "N") //自【保管單位設定檔】中挑出啟用中的單位
                                .Select(x => x.CHARGE_DEPT).Distinct()
                                .AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = x,
                                    Text = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x)?.DPT_NAME
                                }).ToList();
                            if (applicationUnit.Any())
                            {
                                var _first = applicationUnit.First();
                                applicant = dbINTRA.V_EMPLY2.AsNoTracking()
                                    .Where(x => x.DPT_CD == _first.Value)
                                    .AsEnumerable()
                                    .Select(x => new SelectOption()
                                    {
                                        Value = x.USR_ID,
                                        Text = $@"{x.USR_ID}({x.EMP_NAME})"
                                    }).ToList();
                            }

                        }
                        else
                        {
                            applicationProject =
                                db.CODE_USER_ROLE.AsNoTracking()
                                .Where(x => x.USER_ID == cUserID) //登入者所擁有的角色
                                .Join(db.CODE_ROLE_ITEM.AsNoTracking()
                                .Where(x => x.AUTH_TYPE == "2"), //表單申請權限=Y
                                x => x.ROLE_ID,
                                y => y.ROLE_ID,
                                (x, y) => y
                                ).Join(db.TREA_ITEM.AsNoTracking(), //金庫存取作業設定檔
                                x => x.ITEM_ID,
                                y => y.ITEM_ID,
                                (x, y) => y
                                ).AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = $@"{x.TREA_ITEM_TYPE}{x.TREA_ITEM_NAME}",
                                    Text = x.ITEM_DESC
                                }).ToList();

                            var _emply = dbINTRA.V_EMPLY2.AsNoTracking()
                                 .FirstOrDefault(x => x.USR_ID == cUserID);

                            if (_emply != null)
                            {
                                applicationUnit.Add(new SelectOption()
                                {
                                    Value = _emply.DPT_CD,
                                    Text = _emply.DPT_NAME
                                });
                                applicant.Add(new SelectOption()
                                {
                                    Value = _emply.USR_ID,
                                    Text = $@"{_emply.USR_ID}({_emply.EMP_NAME})"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.exceptionMessage();
                throw ex;
            }


            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>>(applicationProject, applicationUnit, applicant);
        }

        public List<SelectOption> ChangeUnit(string DPT_CD)
        {
            List<SelectOption> results = new List<SelectOption>();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                results = db.V_EMPLY2.AsNoTracking()
                  .Where(x => x.DPT_CD == DPT_CD)
                  .AsEnumerable()
                  .Select(x => new SelectOption()
                  {
                      Value = x.USR_ID,
                      Text = $@"{x.USR_ID}({x.EMP_NAME})"
                  }).ToList();
            }
            return results;
        }
    }
}