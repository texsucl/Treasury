using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FAP.Web.ViewModels;
using FAP.Web.Models;
using FAP.Web.Utilitys;

namespace FAP.Web.Service.Actual
{
    public class OAP0019 : Common, IOAP0019
    {
        public List<OAP0019ViewModel> GetSearchData(OAP0019SearchViewModel searchModel)
        {
            List<OAP0019ViewModel> result = new List<OAP0019ViewModel>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var update_time_s = TypeTransfer.stringToADDateTimeN(searchModel.update_time_start?.Replace("/", string.Empty));
                var update_time_e = TypeTransfer.stringToADDateTimeN(searchModel.update_time_end?.Replace("/", string.Empty)).DateToLatestTime();

                var dbFun = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "FUN_ID").ToList();
                var dbAction = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP")
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var depts = GetDepts();
                var emps = GetEmps();
                string time = DateTime.Now.ToString();

                result = db.FAP_CROSS_DEPARMENT_HIS.AsNoTracking()
                    .Where(x => x.apply_datetime >= update_time_s, update_time_s != null)
                    .Where(x => x.apply_datetime <= update_time_e, update_time_e != null)
                    .Where(x => x.appr_unit == searchModel.appr_unit || x.appr_unit_before == searchModel.appr_unit, !searchModel.appr_unit.IsNullOrWhiteSpace())
                    .Where(x => x.user_unit == searchModel.user_unit || x.user_unit_before == searchModel.user_unit, !searchModel.user_unit.IsNullOrWhiteSpace())
                    .Where(x => x.apply_status != "3")
                    .AsEnumerable()
                    .Select(x => new OAP0019ViewModel() {
                        exec_action = x.exec_action,
                        exec_action_value = dbAction.FirstOrDefault(z => z.CODE == x.exec_action)?.CODE_VALUE,
                        fun_value = x.exec_action != "D" ? dbFun.Where(z => z.CODE == x.fun_id).Select(y => y.CODE_VALUE).FirstOrDefault() : dbFun.Where(z => z.CODE == x.fun_id_before).Select(y => y.CODE_VALUE).FirstOrDefault(),
                        //fun_value_before = dbFun.Where(z => z.CODE == x.fun_id_before).Select(y => y.CODE_VALUE).FirstOrDefault(),
                        //appr_unit_name = x.exec_action != "D" ? depts.Where(z => z.DEP_ID == x.appr_unit).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault() : depts.Where(z => z.DEP_ID == x.appr_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        appr_unit_name = x.exec_action != "D" ? getFullDepName(depts.Where(z => z.DEP_ID == x.appr_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2 : getFullDepName(depts.Where(z => z.DEP_ID == x.appr_unit_before)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //appr_unit_name_before = depts.Where(z => z.DEP_ID == x.appr_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        //user_unit_name = x.exec_action != "D" ? depts.Where(z => z.DEP_ID == x.user_unit).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault() : depts.Where(z => z.DEP_ID == x.user_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        user_unit_name = x.exec_action != "D" ? getFullDepName(depts.Where(z => z.DEP_ID == x.user_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2 : getFullDepName(depts.Where(z => z.DEP_ID == x.user_unit_before)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //user_unit_name_before = depts.Where(z => z.DEP_ID == x.user_unit_before).Select(z => { return $@"{z.DEP_ID}({z.DEP_NAME})"; }).FirstOrDefault(),
                        memo = x.exec_action != "D" ? x.memo : x.memo_before,
                        //memo_before = x.memo_before,
                        apply_name = x.apply_id.IsNullOrWhiteSpace() ? "" : emps.Where(z => z.MEM_MEMO1 == x.apply_id)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault(),
                        apply_time = TypeTransfer.dateTimeNToStringNT(x.apply_datetime, time),
                        appr_name = x.appr_id.IsNullOrWhiteSpace() ? "" : emps.Where(z => z.MEM_MEMO1 == x.appr_id)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault(),
                        appr_time = TypeTransfer.dateTimeNToStringNT(x.appr_datetime, time),
                    }).ToList();
            }
            return result;
        }
    }
}