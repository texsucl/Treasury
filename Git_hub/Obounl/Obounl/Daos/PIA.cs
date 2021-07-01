using Obounl.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obounl.Daos
{
    public class PIA
    {
        public Tuple<string,int> AddPIA(PIA_LOG_MAIN addData, string logger = null)
        {
            string sql = $@" 
INSERT INTO [dbo].[PIA_LOG_MAIN]
           ([TRACKING_TYPE]
           ,[ACCESS_ACCOUNT]
           ,[ACCOUNT_NAME]
           ,[FROM_IP]
           ,[ACCESS_DATE]
           ,[ACCESS_TIME]
           ,[PROGFUN_NAME]
           ,[ACCESSOBJ_NAME]
           ,[EXECUTION_TYPE]
           ,[EXECUTION_CONTENT]
           ,[AFFECT_ROWS]
           ,[PIA_OWNER1]
           ,[PIA_OWNER2]
           ,[PIA_TYPE])
     VALUES
           (@TRACKING_TYPE, 
            @ACCESS_ACCOUNT,
            @ACCOUNT_NAME,
            @FROM_IP,
            @ACCESS_DATE,
            @ACCESS_TIME,
            @PROGFUN_NAME,
            @ACCESSOBJ_NAME,
            @EXECUTION_TYPE,
            @EXECUTION_CONTENT,
            @AFFECT_ROWS,
            @PIA_OWNER1,
            @PIA_OWNER2,
            @PIA_TYPE)
";
            return new MSSql("dbCTILog").Execute(sql, addData, logger);
        }
    }
}