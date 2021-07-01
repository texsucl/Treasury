using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Utilitys;
using FAP.Web.Daos;
using FAP.Web.Models;
using System.Web.Mvc;

/// <summary>
/// 功能說明：應付票據簽收資料–維護(尚未簽收)
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Service.Actual
{
    public class OAP0024 : Common, IOAP0024
    {
        /// <summary>
        /// 查詢 應付票據簽收資料–維護(尚未簽收)
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<List<OAP0024Model>> Search_OAP0024(OAP0024SearchModel searchModel)
        {
            MSGReturnModel<List<OAP0024Model>> result = new MSGReturnModel<List<OAP0024Model>>();
            List<OAP0024Model> models = new List<OAP0024Model>();
            List<OAP0021DetailSubModel> subDatas = new List<OAP0021DetailSubModel>();
            var date_s = searchModel.entry_date_s.DPformateTWdate(); //新增日期起
            var date_e = searchModel.entry_date_e.DPformateTWdate(); //新增日期迄
            if (date_s.IsNullOrWhiteSpace() || date_e.IsNullOrWhiteSpace())
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription(null, "新增日期起迄必要有,且格式需正確!");
            }
            else
            {
                var depts = GetDepts();
                var emps = GetEmps();
                var PNPR = getPNPR();
                var _srce_froms = new SysCodeDao().qryByType("AP", "SRCE_FROM");

                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    string sql = string.Empty;
                    string c = string.Empty;
                    int i = 0;
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select 
CHECK_NO,SRCE_FROM,ENTRY_DATE,ENTRY_ID,APPLY_NO,APPLY_ID,UNIT_CODE
from FAPPYSN0
where FLAG = 'N'
and ENTRY_DATE between :ENTRY_DATE_1 and :ENTRY_DATE_2
";
                        com.Parameters.Add($@"ENTRY_DATE_1", date_s);
                        com.Parameters.Add($@"ENTRY_DATE_2", date_e);
                        if (searchModel.srce_from != "All")
                        {
                            sql += $@"
and SRCE_FROM = :SRCE_FROM 
";
                            com.Parameters.Add(@"SRCE_FROM", searchModel.srce_from);
                        }
                        if (!searchModel.entry_id.IsNullOrWhiteSpace())
                        {
                            sql += $@"
and ENTRY_ID = :ENTRY_ID 
";
                            com.Parameters.Add(@"ENTRY_ID", searchModel.entry_id);
                        }
                        if (!searchModel.check_no.IsNullOrWhiteSpace())
                        {
                            sql += $@"
and CHECK_NO = :CHECK_NO
";
                            com.Parameters.Add(@"CHECK_NO", searchModel.check_no);
                        }
                        if (!searchModel.apply_no.IsNullOrWhiteSpace())
                        {
                            sql += $@"
and APPLY_NO = :APPLY_NO
";
                            com.Parameters.Add(@"APPLY_NO", searchModel.apply_no);
                        }
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var _check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                            var _srce_from = dbresult["SRCE_FROM"]?.ToString()?.Trim(); //資料來源
                            var _entry_id = dbresult["ENTRY_ID"]?.ToString()?.Trim(); //新增人員
                            var _apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(); //收件人員
                            var _unit_code = dbresult["UNIT_CODE"]?.ToString()?.Trim(); //收件單位
                            var OAP0024data = new OAP0024Model()
                            {
                                srce_from = _srce_from, //資料來源
                                srce_from_D = _srce_froms.FirstOrDefault(x=>x.CODE == _srce_from)?.CODE_VALUE, //資料來源(中文)
                                entry_date = dbresult["ENTRY_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(), //新增日期
                                entry_id = _entry_id, //新增人員
                                entry_id_D = emps.FirstOrDefault(x => x.MEM_MEMO1 == _entry_id)?.MEM_NAME, //新增人員(中文)
                                check_no = _check_no,//支票號碼
                                apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(), //單號
                                apply_id = _apply_id, //收件人員
                                apply_id_D = (emps.FirstOrDefault(x => x.MEM_MEMO1 == _apply_id)?.MEM_NAME) ??
                                              PNPR.FirstOrDefault(x=>x.apt_id == _apply_id)?.apt_name, //收件人員(中文)
                                unit_code = _unit_code, //收件單位
                                //unit_code_D = depts.FirstOrDefault(x => x.DEP_ID == _unit_code)?.DEP_NAME //收件單位(中文)
                            };                           
                            models.Add(OAP0024data);
                            var subData = new OAP0021DetailSubModel()
                            {
                                check_no = _check_no
                            };
                            subDatas.Add(subData);
                        }
                        com.Dispose();
                    }
                    new OAP0021().getSubData(subDatas);
                    var _fullDepName = getFullDepName(models.Select(x => x.unit_code).Distinct());
                    foreach (var item in models)
                    {
                        var _subData = subDatas.First(x => x.check_no == item.check_no);
                        item.amount = _subData.amount;
                        item.check_date = _subData.check_date;
                        item.receiver = _subData.receiver;
                        item.unit_code_D = _fullDepName.First(x => x.Item1 == item.unit_code).Item2;
                    }                  
                }
                if (models.Any())
                {
                    result.RETURN_FLAG = true;
                    result.Datas = models;
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
            }

            return result;
        }

        /// <summary>
        /// 修改 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MSGReturnModel updateOAP0024(OAP0024Model model)
        {
            MSGReturnModel result = new MSGReturnModel();
            string sql = string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
update LAPPYSN1
set 
APPLY_ID = :APPLY_ID,
UNIT_CODE = :UNIT_CODE
where CHECK_NO = :CHECK_NO ;
";
                    com.Parameters.Add(@"APPLY_ID", model.apply_id);
                    com.Parameters.Add(@"UNIT_CODE", model.unit_code);
                    com.Parameters.Add(@"CHECK_NO", model.check_no);
                    com.CommandText = sql;
                    try
                    {
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                        if (updateNum >= 0)
                        {
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.update_Success.GetDescription($@"支票號碼 : {model.check_no}");
                        }
                        else
                        {
                            result.DESCRIPTION = MessageType.update_Fail.GetDescription(null, "資料已異動!");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }

                }
            }
            return result;
        }

        /// <summary>
        /// 刪除 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MSGReturnModel deleteOAP0024(OAP0024Model model)
        {
            MSGReturnModel result = new MSGReturnModel();
            string sql = string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
delete LAPPYSN1
where APPLY_NO = :APPLY_NO ;
";
                    com.Parameters.Add(@"APPLY_NO", model.apply_no);
                    com.CommandText = sql;
                    try
                    {
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                        if (updateNum >= 0)
                        {
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.delete_Success.GetDescription($@"申請單號 : {model.apply_no}");
                        }
                        else
                        {
                            result.DESCRIPTION = MessageType.delete_Fail.GetDescription(null, "資料已異動!");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 依部門找尋 應付票據簽收窗口 明細檔
        /// </summary>
        /// <param name="dep_id"></param>
        /// <returns></returns>
        public List<SelectOption> getupdateDatas(string dep_id)
        {
            var PNPRs = getPNPR();
            List<SelectOption> _groups = new List<SelectOption>() {
                 new SelectOption() { Value = " ",Text = "請選擇"}
            };
            var depts = GetDepts();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                _groups.AddRange(
                db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking()
                    .Where(x => x.dep_id == dep_id).AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.division,
                        Text = depts.FirstOrDefault(y => y.DEP_ID == x.division)?.DEP_NAME
                    }));
            }
            return _groups;
            //return new SelectList(
            //    items: _groups,
            //    dataValueField: "Value",
            //    dataTextField: "Text"
            //);
        }

        public List<FAP_NOTES_PAYABLE_RECEIVED> getPNPR()
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                return db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking().ToList();
            }
        }
    }
}