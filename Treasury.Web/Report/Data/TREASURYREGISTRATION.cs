using System;
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
    public class TREASURYREGISTRATION : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value;
            var APLY_DT_To = parms.Where(x => x.key == "APLY_DT_To").FirstOrDefault()?.value;
            var vOpenTreaType = parms.Where(x => x.key == "vOpenTreaType").FirstOrDefault()?.value;
          
            //string aply_No = string.Empty;
            //if (par != null)
            //{
            //    aply_No = par.value;
            //}
            //SetDetail(aply_No);
            using (var conn = new SqlConnection(defaultConnection))
            {
                string sql = string.Empty;
                //                //sql += @"
                //with 

                //code as
                //(
                //select CODE_TYPE,CODE,CODE_VALUE from sys_code where CODE_TYPE in ('OPEN_TREA_TYPE')
                //)
                //select 
                //ROW_NUMBER() OVER(order by OPEN_TREA_DATE,TREA_REGISTER_ID) AS ROW_NUMBER,
                //CONVERT(varchar(10), OPEN_TREA_DATE, 111) as OPEN_TREA_DATE, --入庫日期
                //TREA_REGISTER_ID, --金庫登記簿單號
                //CONVERT(varchar(8), ACTUAL_PUT_TIME, 108) as ACTUAL_PUT_TIME, --入庫時間
                //CONVERT(varchar(8), ACTUAL_GET_TIME, 108) as ACTUAL_GET_TIME, --出庫時間
                //(select top 1 CODE_VALUE from code where CODE_TYPE = 'OPEN_TREA_TYPE' and code = OPEN_TREA_TYPE) AS OPEN_TREA_TYPE, --開庫模式
                //OPEN_TREA_REASON --指定開庫原因
                //from
                //TREA_OPEN_REC
                //where ACTUAL_PUT_TIME BETWEEN @APLY_DT_From AND @APLY_DT_To
                //";

                sql += @"
DECLARE @#SomeTable TABLE (TREA_REGISTER_ID VARCHAR(10), OPEN_TREA_REASON VARCHAR(max));
DECLARE @#SomeTable2 TABLE (TREA_REGISTER_ID VARCHAR(10), OPEN_TREA_REASON VARCHAR(max) , OPEN_TREA_REASON_DES VARCHAR(max));
DECLARE @start INT, @end INT , @TREA_REGISTER_ID VARCHAR(10), @OPEN_TREA_REASON VARCHAR(max), @Result VARCHAR(MAX);

insert into @#SomeTable
(
TREA_REGISTER_ID,
OPEN_TREA_REASON
)
select TREA_REGISTER_ID,OPEN_TREA_REASON from (select * from TREA_OPEN_REC)AS TOR 
WHILE EXISTS (SELECT * FROM  @#SomeTable)
BEGIN
    SELECT TOP 1 @TREA_REGISTER_ID = TREA_REGISTER_ID,@OPEN_TREA_REASON = OPEN_TREA_REASON FROM @#SomeTable;
    SELECT @start = 1, @end = CHARINDEX(';', @OPEN_TREA_REASON) 
    WHILE @start < LEN(@OPEN_TREA_REASON) + 1 
     BEGIN 
        IF @end = 0  
            SET @end = LEN(@OPEN_TREA_REASON) + 1       
        INSERT INTO @#SomeTable2 (TREA_REGISTER_ID,OPEN_TREA_REASON)  
    --     SELECT @TREA_REGISTER_ID,
		  --TI.ITEM_DESC
    --     FROM
		  --(select * from TREA_ITEM where IS_DISABLED <> 'Y') AS TI WHERE ITEM_ID = SUBSTRING(@OPEN_TREA_REASON, @start, @end - @start)
        VALUES(@TREA_REGISTER_ID, SUBSTRING(@OPEN_TREA_REASON, @start, @end - @start))
        SET @start = @end + 1 
        SET @end = CHARINDEX(';', @OPEN_TREA_REASON, @start)
    END 
     DELETE  @#SomeTable Where TREA_REGISTER_ID = @TREA_REGISTER_ID 
END;
with tt as(
select * from TREA_ITEM where  IS_DISABLED <> 'Y'
)
update @#SomeTable2
set OPEN_TREA_REASON_DES = 
  (select tt.ITEM_DESC from tt where tt.ITEM_ID = OPEN_TREA_REASON)
--set OPEN_TREA_REASON_DES = 
--CASE WHEN 
--  EXISTS((select tt.ITEM_DESC from tt where tt.ITEM_ID = OPEN_TREA_REASON))
--  THEN
--  (select tt.ITEM_DESC from tt where tt.ITEM_ID = OPEN_TREA_REASON)
--  ELSE
--  OPEN_TREA_REASON
--  END
--select * from @#SomeTable2

--select * from @#SomeTable2
--select TREA_REGISTER_ID,
INSERT INTO @#SomeTable (TREA_REGISTER_ID,OPEN_TREA_REASON)  
select
TREA_REGISTER_ID,
(select CAST(OPEN_TREA_REASON_DES AS nvarchar(30)) + ';' from @#SomeTable2
where TREA_REGISTER_ID = st2.TREA_REGISTER_ID and OPEN_TREA_REASON_DES <> '金庫大門' and OPEN_TREA_REASON_DES <> '金庫柵門' and OPEN_TREA_REASON_DES <> '金庫密碼'
for XML PATH('')
) as OPEN_TREA_REASON
from
(
SELECT DISTINCT TREA_REGISTER_ID
from @#SomeTable2
) as st2;

with
code as
(
select CODE_TYPE,CODE,CODE_VALUE from sys_code where CODE_TYPE in ('OPEN_TREA_TYPE')
)
select 
ROW_NUMBER() OVER(order by OPEN_TREA_DATE,TOR.TREA_REGISTER_ID) AS ROW_NUMBER,
CONVERT(varchar(10), OPEN_TREA_DATE, 111) as OPEN_TREA_DATE, --入庫日期
TOR.TREA_REGISTER_ID, --金庫登記簿單號
CONVERT(varchar(8), ACTUAL_PUT_TIME, 108) as ACTUAL_PUT_TIME, --入庫時間
CONVERT(varchar(8), ACTUAL_GET_TIME, 108) as ACTUAL_GET_TIME, --出庫時間
(select top 1 CODE_VALUE from code where CODE_TYPE = 'OPEN_TREA_TYPE' and code = OPEN_TREA_TYPE) AS OPEN_TREA_TYPE, --開庫模式
st.OPEN_TREA_REASON --指定開庫原因
from
TREA_OPEN_REC TOR
join  @#SomeTable st
on TOR.TREA_REGISTER_ID = st.TREA_REGISTER_ID
where ACTUAL_PUT_TIME >= @APLY_DT_From AND ACTUAL_PUT_TIME <= DATEADD(Day, 1, @APLY_DT_To)
";
                //where ACTUAL_PUT_TIME BETWEEN @APLY_DT_From AND @APLY_DT_To
                //if(vOpenTreaType == "All")
                //    sql = sql.Remove(sql.Count() - 38, 37);
                if (vOpenTreaType != "All")
                    sql += " AND OPEN_TREA_TYPE = @vOpenTreaType";

                    using (var cmd = new SqlCommand(sql, conn))
                {
                    if (vOpenTreaType != "All")           
                        _Parameters.Add(new SqlParameter("@vOpenTreaType", vOpenTreaType));

                    _Parameters.Add(new SqlParameter("@APLY_DT_From", APLY_DT_From));
                    _Parameters.Add(new SqlParameter("@APLY_DT_To", APLY_DT_To));
                    cmd.Parameters.AddRange(_Parameters.ToArray());
                    conn.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultsTable);
                  
                }
            }
            return resultsTable;
        }
    }
}