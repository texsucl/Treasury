using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FAP.Web.Service.Actual;

namespace FAP.Web.Report.Data
{
    public class OAP0033 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var date = parms.Where(x => x.key == "UP_DATE").FirstOrDefault()?.value?.Trim()?.DPformateTWdate() ?? string.Empty; //變更日期
            var rece_id = parms.Where(x => x.key == "RECE_ID").FirstOrDefault()?.value?.Trim(); //變更處理人員

            List<OAP0033ReportModel> datas = new List<OAP0033ReportModel>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
                    select RECE_ID , APPLY_NO , APPLY_UNIT , APPLY_ID , UPD_DATE
                    from FAPPYCH0
                    where STATUS = '4'                   
                    ";
                    if (!date.IsNullOrWhiteSpace())
                    {
                        sql += $@" and UPD_DATE = :UPD_DATE ";
                        com.Parameters.Add($@"UPD_DATE", date);
                    }
                    if (!rece_id.IsNullOrWhiteSpace())
                    {
                        sql += $@" and RECE_ID = :RECE_ID ";
                        com.Parameters.Add($@"RECE_ID", rece_id);
                    }
                    sql += " order by RECE_ID , RECE_DATE , RECE_TIME ; ";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        datas.Add(new OAP0033ReportModel()
                        {
                            RECE_ID = dbresult["RECE_ID"]?.ToString()?.Trim(), //處理人員
                            APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                            APPLY_UNIT = dbresult["APPLY_UNIT"]?.ToString()?.Trim(), //系統申請單位
                            APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(), //系統申請人員
                            UPD_DATE = dbresult["UPD_DATE"]?.ToString()?.Trim(), //變更完成日期
                        });
                    }
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }

            var common = new Service.Actual.Common();
            var userMemos = datas.Where(x => !x.APPLY_ID.IsNullOrWhiteSpace()).Select(x => x.APPLY_ID).Distinct().ToList();
            userMemos.AddRange(datas.Where(x => !x.RECE_ID.IsNullOrWhiteSpace()).Select(x => x.RECE_ID).Distinct());
            var userMemo = common.GetMemoByUserId(userMemos.Distinct());
            var _fullDepName = common.getFullDepName(datas.Where(x => !x.APPLY_UNIT.IsNullOrWhiteSpace()).Select(x => x.APPLY_UNIT).Distinct());
            foreach (var item in datas)
            {
                var _apply_id = userMemo.FirstOrDefault(x => x.Item1 == item.APPLY_ID)?.Item2;
                item.APPLY_ID = _apply_id.IsNullOrWhiteSpace() ? item.APPLY_ID : _apply_id; //系統申請人員
                var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.RECE_ID)?.Item2;
                item.RECE_ID = _rece_id.IsNullOrWhiteSpace() ? item.RECE_ID : _rece_id; //處理人員 
                item.APPLY_UNIT = _fullDepName.First(x => x.Item1 == item.APPLY_UNIT).Item2; //系統申請單位
            }
            resultsTable.Tables.Add(datas.ToDataTable());
            return resultsTable;
        }

        public class OAP0033ReportModel {

            /// <summary>
            /// 變更完成日期
            /// </summary>
            public string UPD_DATE { get; set; }

            /// <summary>
            /// 處理人員
            /// </summary>
            public string RECE_ID { get; set; }

            /// <summary>
            /// 申請單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 系統申請單位
            /// </summary>
            public string APPLY_UNIT { get; set; }

            /// <summary>
            /// 系統申請人員
            /// </summary>
            public string APPLY_ID { get; set; }
        }

    }
}