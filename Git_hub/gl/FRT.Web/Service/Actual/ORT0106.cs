using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static FRT.Web.BO.Utility;

/// <summary>
/// 功能說明：跨系統勾稽作業檔 現上執行功能
/// 初版作者：20210412 Mark
/// 修改歷程：20210412 Mark
///           需求單號：202011050211-28
///           初版
/// </summary>
///

namespace FRT.Web.Service.Actual
{
    public class ORT0106 : Common , IORT0106
    {
        /// <summary>
        /// 使用 id 查詢 跨系統勾稽作業檔
        /// </summary>
        /// <param name="check_id"></param>
        /// <returns></returns>
        public MSGReturnModel<ORT0106ViewModel> getCheck(string check_id)
        {
            MSGReturnModel<ORT0106ViewModel> result = new MSGReturnModel<ORT0106ViewModel>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _FRT_CROSS_SYSTEM_CHECK = db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking().FirstOrDefault(x => x.check_id == check_id);
                if (_FRT_CROSS_SYSTEM_CHECK != null)
                {
                    var _date = new GoujiReport().getReportDate(_FRT_CROSS_SYSTEM_CHECK);
                    result.RETURN_FLAG = true;
                    var _ORT0106ViewModel = new ORT0106ViewModel()
                    {
                        id = _FRT_CROSS_SYSTEM_CHECK.check_id,
                        runFlag = _FRT_CROSS_SYSTEM_CHECK.run_flag,
                        date_s = _date.Item2,
                        date_e = _date.Item3
                    };
                    result.Datas = _ORT0106ViewModel;
                }
                else
                {
                    result.DESCRIPTION = "參數錯誤";
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢現有 勾稽報表
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> getCross_System(string reserve1 = "")
        {
            List<SelectOption> results = new List<SelectOption>() { new SelectOption() { Text = "", Value = ""} };

            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<SYS_CODE> sys_codes = new List<SYS_CODE>();
                var gojtypes = GetSysCodes("RT", new List<string>() { "GOJ_TYPE" }).Select(x => $@"GOJ_TYPE_{x.CODE}_GROUP").ToList();
                gojtypes.AddRange(new List<string>() { "GOJ_TYPE"});
                sys_codes = GetSysCodes("RT", gojtypes);
                //foreach (var item in db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking())
                //{
                //    results.Add(new SelectOption()
                //    {
                //        Value = item.check_id,
                //        Text = $@"{sys_codes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == item.type)?.CODE_VALUE ?? item.type} : {sys_codes.FirstOrDefault(x => x.CODE_TYPE == $@"GOJ_TYPE_{item.type}_GROUP" && x.CODE == item.kind)?.CODE_VALUE ?? item.kind}"
                //    });                    
                //}        
                var _FRT_CROSS_SYSTEM_CHECK = db.FRT_CROSS_SYSTEM_CHECK.AsNoTracking().ToList();
                foreach (var item in sys_codes.Where(x => x.CODE_TYPE != "GOJ_TYPE")
                    .Where( x => x.RESERVE1 == reserve1, !reserve1.IsNullOrWhiteSpace())
                    .OrderBy(x => x.CODE_TYPE).ThenBy(x => x.CODE))
                {
                    results.Add(new SelectOption()
                    {
                        Value = $@"{item.RESERVE1}_{item.CODE}_{_FRT_CROSS_SYSTEM_CHECK.FirstOrDefault(y => y.type == item.RESERVE1 && y.kind == item.CODE)?.check_id ?? string.Empty}" ,
                        Text = $@"{sys_codes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == item.RESERVE1)?.CODE_VALUE ?? item.RESERVE1} : {item.CODE_VALUE}"
                    });
                }
            }
            return results;
        }

        public void updateRunFlag(FRT_CROSS_SYSTEM_CHECK schedulerModel, string runFlag)
        {
            try
            {
                if (schedulerModel != null)
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        var _FRT_CROSS_SYSTEM_CHECK = db.FRT_CROSS_SYSTEM_CHECK.FirstOrDefault(x => x.check_id == schedulerModel.check_id);
                        if (_FRT_CROSS_SYSTEM_CHECK != null)
                            _FRT_CROSS_SYSTEM_CHECK.run_flag = runFlag;
                        db.SaveChanges();
                    }
                }
            }
            catch
            { 
            
            }
        }
    }
}