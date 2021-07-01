using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// <summary>
/// SPMFAS002  MG讀取險種名稱 
/// </summary>
namespace FGL.Web.AS400PGM
{
    public class SPMFAS002Util
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string callSPMFAS002Util(string item, EacConnection con)
        {
            string itemName = "";

            try
            {
                EacCommand cmd = new EacCommand();
                cmd.Connection = con;

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "*PGM/SPMFAS002";
                cmd.Parameters.Clear();

                EacParameter pSysId = new EacParameter();
                pSysId.ParameterName = "LK-SYS-ID";
                pSysId.DbType = DbType.String;
                pSysId.Size = 1;
                pSysId.Direction = ParameterDirection.InputOutput;
                pSysId.Value = "";

                EacParameter pItem = new EacParameter();
                pItem.ParameterName = "LK-ITEM";
                pItem.DbType = DbType.String;
                pItem.Size = 8;
                pItem.Direction = ParameterDirection.InputOutput;
                pItem.Value = item;

                EacParameter pRtn = new EacParameter();
                pRtn.ParameterName = "LK-RTN";
                pRtn.DbType = DbType.String;
                pRtn.Size = 1;
                pRtn.Direction = ParameterDirection.InputOutput;
                pRtn.Value = "";

                EacParameter pItemText = new EacParameter();
                pItemText.ParameterName = "LK-ITEM-TE";
                pItemText.DbType = DbType.String;
                pItemText.Size = 60;
                pItemText.Direction = ParameterDirection.InputOutput;
                pItemText.Value = "";

                

                cmd.Parameters.Add(pSysId);
                cmd.Parameters.Add(pItem);
                cmd.Parameters.Add(pRtn);
                cmd.Parameters.Add(pItemText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                itemName = cmd.Parameters["LK-ITEM-TE"].Value.ToString();

                cmd.Dispose();

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return itemName;
        }


    }
}