﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class ITEMIMP : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>(); 
            var par = parms.Where(x => x.key == "aply_No").FirstOrDefault();
            string aply_No = string.Empty;
            if (par != null)
            {
                aply_No = par.value;
            }
            SetDetail(aply_No);
            using (var conn = new SqlConnection(defaultConnection))
            {
                string sql = string.Empty;
                sql += @"
with temp as
(
select * from OTHER_ITEM_APLY
where APLY_NO =  @APLY_NO
)
select 
ROW_NUMBER() OVER(order by ITEM_ID) AS ROW_NUMBER,
ITEM_NAME,--物品名稱
QUANTITY, --數量
AMOUNT, --金額
DESCRIPTION --說明
from
ITEM_IMPO
where ITEM_ID in (select ITEM_ID from temp)
";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    _Parameters.Add(new SqlParameter("@APLY_NO", aply_No));
                    cmd.Parameters.AddRange(_Parameters.ToArray());
                    conn.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultsTable);

                    SetExtensionParm();
                }
            }
            return resultsTable;
        }

    }
}