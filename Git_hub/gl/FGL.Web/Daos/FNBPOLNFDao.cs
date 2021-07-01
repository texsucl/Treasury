﻿
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FNBPOLN 商品檔(個險-F系統)
/// </summary>
namespace FGL.Web.Daos
{
    public class FNBPOLNFDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string qryItemName(string item, EacConnection conn)
        {
            logger.Info("proc qryItemName begin!");
            string itemName = "";

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT ITEM, ITEM_NAME, SHRT_NAME
  FROM LP_FBDB/LNBPOLN1 
    WHERE ITEM = :ITEM";

            try
            {
                cmdQ.Connection = conn;
                cmdQ.CommandText = strSQLQ;
                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ITEM", StringUtil.toString(item));

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    itemName = result["SHRT_NAME"]?.ToString();

                    break;
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("proc qryItemName end!");

                return itemName.TrimEnd();

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }
        

    }
}