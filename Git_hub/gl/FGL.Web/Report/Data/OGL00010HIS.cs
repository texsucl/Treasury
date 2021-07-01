using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.Utilitys;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FGL.Web.Service.Actual;

namespace FGL.Web.Report.Data
{
    public class OGL00010HIS : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var reportModels = new List<OGL00010HISReportModel>();

            //string pk_ids = parms.Where(x => x.key == "pk_ids").FirstOrDefault()?.value?.Trim() ?? string.Empty; //pk_ids
            //var _pk_ids = pk_ids.Split(';').ToList();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _main = db.FGL_PAY_MAIN_HIS.AsNoTracking().Where(
                      x => x.apply_status == "1").ToList();
                //var _main = db.FGL_PAY_MAIN_HIS.AsNoTracking().Where(
                //    x => _pk_ids.Contains(x.pk_id) && x.apply_status == "1").ToList();
                var _main_pk_id = _main.Select(x => x.pk_id).ToList();
                var _sub = db.FGL_PAY_SUB_HIS.AsNoTracking().Where(x => _main_pk_id.Contains(x.main_pk_id)).ToList();

                _main.ForEach(main =>
                {
                    int i = 1;
                    if (main.exec_action == "U")
                    {
                        bool updateFlag = (main.memo_n != main.memo_o) ||
                                          (main.item_yn_n != main.item_yn_o) ||
                                          (main.year_yn_n != main.year_yn_o) ||
                                          (main.prem_yn_n != main.prem_yn_o) ||
                                          (main.unit_yn_n != main.unit_yn_o) ||
                                          (main.recp_yn_n != main.recp_yn_o) ||
                                          (main.cont_yn_n != main.cont_yn_o) ||
                                          (main.corp_yn_n != main.corp_yn_o);

                        foreach (var sub in _sub.Where(x => x.main_pk_id == main.pk_id))
                        {
                            if (sub.exec_action == "U" || (updateFlag && sub.exec_action.IsNullOrWhiteSpace()))
                            {
                                reportModels.Add(
                                new OGL00010HISReportModel()
                                {
                                    status = getStatus(null), //選項
                                    pay_class = $@"{main.pay_class}   {i}", //退費項目類別
                                    memo = main.memo_o, //說明
                                    item_yn = main.item_yn_o, //險種否
                                    year_yn = main.year_yn_o, //年次否
                                    recp_yn = main.recp_yn_o, //送金單否
                                    unit_yn = main.unit_yn_o, //費用單位否
                                    actnum_yn = sub.actnum_yn_o, //取會科否
                                    prem_kind = sub.prem_kind_o, //保費類別
                                    cont_type = sub.cont_type_o, //合約別
                                    prod_type = sub.prod_type_o, //商品別
                                    corp_no = sub.corp_no_o, //帳本別
                                    acct_code = sub.acct_code_o, //保費收入首年首期
                                    acct_codef = sub.acct_codef_o, //保費收入首年續期
                                    acct_coder = sub.acct_coder_o //續年度
                                });
                                reportModels.Add(
                                new OGL00010HISReportModel()
                                {
                                    status = getStatus("U"), //選項
                                    pay_class = $@"{main.pay_class}   {i}", //退費項目類別
                                    memo = main.memo_n, //說明
                                    item_yn = main.item_yn_n, //險種否
                                    year_yn = main.year_yn_n, //年次否
                                    recp_yn = main.recp_yn_n, //送金單否
                                    unit_yn = main.unit_yn_n, //費用單位否
                                    actnum_yn = sub.actnum_yn_n, //取會科否
                                    prem_kind = sub.prem_kind_n, //保費類別
                                    cont_type = sub.cont_type_n, //合約別
                                    prod_type = sub.prod_type_n, //商品別
                                    corp_no = sub.corp_no_n, //帳本別
                                    acct_code = sub.acct_code_n, //保費收入首年首期
                                    acct_codef = sub.acct_codef_n, //保費收入首年續期
                                    acct_coder = sub.acct_coder_n //續年度
                                });
                            }
                            else
                            {
                                reportModels.Add(
                                new OGL00010HISReportModel()
                                {
                                    status = getStatus(sub.exec_action), //選項
                                    pay_class = $@"{main.pay_class}   {i}", //退費項目類別
                                    memo = main.memo_n, //說明
                                    item_yn = main.item_yn_n, //險種否
                                    year_yn = main.year_yn_n, //年次否
                                    recp_yn = main.recp_yn_n, //送金單否
                                    unit_yn = main.unit_yn_n, //費用單位否
                                    actnum_yn = sub.actnum_yn_n, //取會科否
                                    prem_kind = sub.prem_kind_n, //保費類別
                                    cont_type = sub.cont_type_n, //合約別
                                    prod_type = sub.prod_type_n, //商品別
                                    corp_no = sub.corp_no_n, //帳本別
                                    acct_code = sub.acct_code_n, //保費收入首年首期
                                    acct_codef = sub.acct_codef_n, //保費收入首年續期
                                    acct_coder = sub.acct_coder_n //續年度
                                });
                            }
                            i += 1;
                        }
                    }
                    else
                    {
                        foreach (var sub in _sub.Where(x => x.main_pk_id == main.pk_id))
                        {
                            reportModels.Add(
                                new OGL00010HISReportModel()
                                {
                                    status = getStatus(sub.exec_action), //選項
                                    pay_class = $@"{main.pay_class}   {i}", //退費項目類別
                                    memo = main.memo_n, //說明
                                    item_yn = main.item_yn_n, //險種否
                                    year_yn = main.year_yn_n, //年次否
                                    recp_yn = main.recp_yn_n, //送金單否
                                    unit_yn = main.unit_yn_n, //費用單位否
                                    actnum_yn = sub.actnum_yn_n, //取會科否
                                    prem_kind = sub.prem_kind_n, //保費類別
                                    cont_type = sub.cont_type_n, //合約別
                                    prod_type = sub.prod_type_n, //商品別
                                    corp_no = sub.corp_no_n, //帳本別
                                    acct_code = sub.acct_code_n, //保費收入首年首期
                                    acct_codef = sub.acct_codef_n, //保費收入首年續期
                                    acct_coder = sub.acct_coder_n //續年度
                                });
                            i += 1;
                        }
                    }
                });
            }

            resultsTable.Tables.Add(reportModels.ToDataTable());
            return resultsTable;
        }

        public string getStatus(string status)
        {
            string result = string.Empty;
            switch (status)
            {
                case "A":
                    result = "新增";
                    break;
                case "D":
                    result = "刪除";
                    break;
                case "U":
                    result = "修改";
                    break;
                default:
                    result = "原始";
                    break;
            }
            return result;
        }

        public string getCodeValue(List<SYS_CODE> SYS_CODEs,string code_type, string value)
        {
            return (SYS_CODEs.FirstOrDefault(x => x.CODE_TYPE == code_type && x.CODE == value)?.CODE_VALUE) ?? value;
        }

        public class OGL00010HISReportModel {

            /// <summary>
            /// 選項
            /// </summary>
            public string status { get; set; }

            /// <summary>
            /// 退費項目類別
            /// </summary>
            public string pay_class { get; set; }

            /// <summary>
            /// 說明
            /// </summary>
            public string memo { get; set; }

            /// <summary>
            /// 險種否
            /// </summary>
            public string item_yn { get; set; }

            /// <summary>
            /// 年次否
            /// </summary>
            public string year_yn { get; set; }

            /// <summary>
            /// 保費類別否
            /// </summary>
            public string prem_yn { get; set; }

            /// <summary>
            /// 費用單位否
            /// </summary>
            public string unit_yn { get; set; }

            /// <summary>
            /// 送金單否
            /// </summary>
            public string recp_yn { get; set; }

            /// <summary>
            /// 合約別否
            /// </summary>
            public string cont_yn { get; set; }

            /// <summary>
            /// 帳本否
            /// </summary>
            public string corp_yn { get; set; }

            /// <summary>
            /// 保費類別
            /// </summary>
            public string prem_kind { get; set; }

            /// <summary>
            /// 保費類別_中文
            /// </summary>
            public string prem_kind_D { get; set; }

            /// <summary>
            /// 合約別
            /// </summary>
            public string cont_type { get; set; }

            /// <summary>
            /// 合約別_中文
            /// </summary>
            public string cont_type_D { get; set; }

            /// <summary>
            /// 商品別
            /// </summary>
            public string prod_type { get; set; }

            /// <summary>
            /// 商品別_中文
            /// </summary>
            public string prod_type_D { get; set; }

            /// <summary>
            /// 帳本別
            /// </summary>
            public string corp_no { get; set; }

            /// <summary>
            /// 取會科否
            /// </summary>
            public string actnum_yn { get; set; }

            /// <summary>
            /// 保費收入首年首期
            /// </summary>
            public string acct_code { get; set; }

            /// <summary>
            /// 保費收入首年續期
            /// </summary>
            public string acct_codef { get; set; }

            /// <summary>
            /// 續年度
            /// </summary>
            public string acct_coder { get; set; }

        }
    }
}