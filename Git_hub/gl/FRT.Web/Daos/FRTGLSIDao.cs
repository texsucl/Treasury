using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FRT.Web.BO;

namespace FRT.Web.Daos
{
    public class FRTGLSIDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public List<SelectOption> SelectReJectedDatas()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dbGLSIEXT"].ConnectionString;
            List<SelectOption> result = new List<SelectOption>();
            using (SqlConnection connGLSI = new SqlConnection(connectionString))
            {
                string str = string.Empty;
                str = $@"
select VAR_CODE, VAR_CODE_NAME from GLSIACT.dbo.UUU85020101
where VAR_CODE_FIELD_ID = @VAR_CODE_FIELD_ID 
and CORP_NO = @CORP_NO
and DEL_YN = @DEL_YN
Order by VAR_CODE";

                var datas = connGLSI.Query<dynamic>(str, new { VAR_CODE_FIELD_ID = "REMIT_SUB_RETURN_TYPE", CORP_NO = "FUBONLIFE", DEL_YN = "N" })?.ToList();
                datas.ForEach(x => {
                    result.Add(new SelectOption() { Value = x.VAR_CODE, Text = x.VAR_CODE_NAME });
                });

                return result;
            }
        }
    }
}