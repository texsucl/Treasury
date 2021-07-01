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
    public class OGL00010 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var reportModels = new List<OGL00010ReportModel>();

            string payclass = parms.Where(x => x.key == "payclass").FirstOrDefault()?.value?.Trim() ?? string.Empty; //退費項目類別

            var result = new Service.Actual.OGL00010().GetSearchData(payclass);

            if (result.RETURN_FLAG)
            {
                result.Datas.ForEach(x =>
                {
                    x.SubDatas.ForEach(y =>
                    {
                        reportModels.Add(new OGL00010ReportModel()
                        {
                            pay_class = x.pay_class, //退費項目類別
                            memo = x.memo_n, //說明
                            item_yn = x.item_yn_n, //險種否
                            year_yn = x.year_yn_n, //年次否
                            prem_yn = x.prem_yn_n, //保費類別否
                            unit_yn = x.unit_yn_n, //費用單位否
                            recp_yn = x.recp_yn_n, //送金單否
                            cont_yn = x.cont_yn_n, //合約別否
                            corp_yn = x.corp_yn_n, //帳本否
                            prem_kind = y.prem_kind_n, //保費類別
                            prem_kind_D = y.prem_kind_n_D, //保費類別_中文
                            cont_type = y.cont_type_n, //合約別
                            cont_type_D = y.cont_type_n_D, //合約別_中文
                            prod_type = y.prod_type_n, //商品別
                            prod_type_D = y.prod_type_n_D, //商品別_中文
                            corp_no = y.corp_no_n, //帳本別
                            actnum_yn = y.actnum_yn_n, //取會科否
                            acct_code = y.acct_code_n, //保費收入首年首期
                            acct_codef = y.acct_codef_n, //保費收入首年續期
                            acct_coder = y.acct_coder_n, //續年度
                        });
                    });
                });
            }


            resultsTable.Tables.Add(reportModels.ToDataTable());
            return resultsTable;
        }

        public string getCodeValue(List<SYS_CODE> SYS_CODEs,string code_type, string value)
        {
            return (SYS_CODEs.FirstOrDefault(x => x.CODE_TYPE == code_type && x.CODE == value)?.CODE_VALUE) ?? value;
        }

        public class OGL00010ReportModel {

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