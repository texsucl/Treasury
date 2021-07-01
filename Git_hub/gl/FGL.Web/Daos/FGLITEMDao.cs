
using FGL.Web.AS400Models;
using FGL.Web.BO;
using System;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLITEM0險種會計科目定義檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLITEMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public void proc(string execAction, FGLITEM0Model procData, EacConnection conn, EacTransaction transaction)
        {
            if ("D".Equals(execAction))
                procInvalidate(procData, conn, transaction);
            else
                procEffective(procData, conn, transaction);
            

        }


        public void procInvalidate( FGLITEM0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procInvalidate FGLITEM0 begin!");
            //logger.Info("CORP_NO:" + procData.corpNo);
            logger.Info("ITEM:" + procData.item);
            //logger.Info("PREM_YEAR:" + procData.premYear);

            EacCommand cmd = new EacCommand();
            string strSQL = @"
DELETE FGLITEM0 
    WHERE ITEM = :ITEM";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                //cmd.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                //cmd.Parameters.Add("PREM_YEAR", StringUtil.toString(procData.premYear));

                cmd.ExecuteNonQuery();
              

                cmd.Dispose();
                cmd = null;

                logger.Info("procInvalidate FGLITEM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void procEffective(FGLITEM0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procEffective FGLITEM0 begin!");
            bool bExist = false;

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT ITEM 
  FROM FGLITEM0 
    WHERE CORP_NO = :CORP_NO 
      AND ITEM = :ITEM
      AND PREM_YEAR = :PREM_YEAR";

            try
            {
                cmdQ.Connection = conn;
                cmdQ.Transaction = transaction;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                cmdQ.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                cmdQ.Parameters.Add("PREM_YEAR", StringUtil.toString(procData.premYear));


                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    bExist = true;
                    break;
                }

                if (bExist)
                    update(procData, conn, transaction);
                else
                    insert(procData, conn, transaction);


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("procEffective FGLITEM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void update(FGLITEM0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("update FGLITEM0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI = @"
UPDATE FGLITEM0 SET
  ACCT_CODE = :ACCT_CODE
, ACCT_CODEF = :ACCT_CODEF
, ACCT_CODER = :ACCT_CODER
, ACCT_CODES = :ACCT_CODES
, ACCT_CODEG = :ACCT_CODEG
, ACCT_CODET = :ACCT_CODET
, COI_CODE = :COI_CODE
, COI_CODEF = :COI_CODEF
, COI_CODER = :COI_CODER
, ACCT_CODEI = :ACCT_CODEI
, COMU_CODEF = :COMU_CODEF
, COMU_CODER = :COMU_CODER
, ACCT_CODEO =:ACCT_CODEO
, COMU_PAYF = :COMU_PAYF
, COMU_PAYR = :COMU_PAYR
, ACCT_4570 = :ACCT_4570
, ACCT_4571 = :ACCT_4571
, ACCT_4572 = :ACCT_4572
, ACCT_4573 = :ACCT_4573
, ACCT_4574 = :ACCT_4574
, ACCT_4575 = :ACCT_4575
, UPD_ID = :UPD_ID
, UPD_YY = :UPD_YY
, UPD_MM = :UPD_MM
, UPD_DD = :UPD_DD
, FILLER_10 = :FILLER_10
, FIELD10_1 = :FIELD10_1
, FIELD10_2 = :FIELD10_2
, FIELD10_3 = :FIELD10_3
, FIELD10_4 = :FIELD10_4
, FIELD10_5 = :FIELD10_5
, FIELD10_6 = :FIELD10_6
, FIELD10_7 = :FIELD10_7
, FIELD10_8 = :FIELD10_8
  WHERE CORP_NO = :CORP_NO
    AND ITEM = :ITEM
    AND PREM_YEAR = :PREM_YEAR";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQLI;

                cmd.Parameters.Clear();

                
                cmd.Parameters.Add("ACCT_CODE", StringUtil.toString(procData.acctCode));
                cmd.Parameters.Add("ACCT_CODEF", StringUtil.toString(procData.acctCodef));
                cmd.Parameters.Add("ACCT_CODER", StringUtil.toString(procData.acctCoder));
                cmd.Parameters.Add("ACCT_CODES", StringUtil.toString(procData.acctCodes));
                cmd.Parameters.Add("ACCT_CODEG", StringUtil.toString(procData.acctCodeg));
                cmd.Parameters.Add("ACCT_CODET", StringUtil.toString(procData.acctCodet));
                cmd.Parameters.Add("COI_CODE", StringUtil.toString(procData.coiCode));
                cmd.Parameters.Add("COI_CODEF", StringUtil.toString(procData.coiCodef));
                cmd.Parameters.Add("COI_CODER", StringUtil.toString(procData.coiCoder));
                cmd.Parameters.Add("ACCT_CODEI", StringUtil.toString(procData.acctCodei));
                cmd.Parameters.Add("COMU_CODEF", StringUtil.toString(procData.comuCodef));
                cmd.Parameters.Add("COMU_CODER", StringUtil.toString(procData.comuCoder));
                cmd.Parameters.Add("ACCT_CODEO", StringUtil.toString(procData.acctCodeo));
                cmd.Parameters.Add("COMU_PAYF", StringUtil.toString(procData.comuPayf));
                cmd.Parameters.Add("COMU_PAYR", StringUtil.toString(procData.comuPayr));
                cmd.Parameters.Add("ACCT_4570", StringUtil.toString(procData.acct4570));
                cmd.Parameters.Add("ACCT_4571", StringUtil.toString(procData.acct4571));
                cmd.Parameters.Add("ACCT_4572", StringUtil.toString(procData.acct4572));
                cmd.Parameters.Add("ACCT_4573", StringUtil.toString(procData.acct4573));
                cmd.Parameters.Add("ACCT_4574", StringUtil.toString(procData.acct4574));
                cmd.Parameters.Add("ACCT_4575", StringUtil.toString(procData.acct4575));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_YY", StringUtil.toString(procData.updYy));
                cmd.Parameters.Add("UPD_MM", StringUtil.toString(procData.updMm));
                cmd.Parameters.Add("UPD_DD", StringUtil.toString(procData.updDd));
                cmd.Parameters.Add("FILLER_10", StringUtil.toString(procData.filler10));
                cmd.Parameters.Add("FIELD10_1", StringUtil.toString(procData.filler101));
                cmd.Parameters.Add("FIELD10_2", StringUtil.toString(procData.filler102));
                cmd.Parameters.Add("FIELD10_3", StringUtil.toString(procData.filler103));
                cmd.Parameters.Add("FIELD10_4", StringUtil.toString(procData.filler104));
                cmd.Parameters.Add("FIELD10_5", StringUtil.toString(procData.filler105));
                cmd.Parameters.Add("FIELD10_6", StringUtil.toString(procData.filler106));
                cmd.Parameters.Add("FIELD10_7", StringUtil.toString(procData.filler107));
                cmd.Parameters.Add("FIELD10_8", StringUtil.toString(procData.filler108));

                cmd.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                cmd.Parameters.Add("PREM_YEAR", StringUtil.toString(procData.premYear));

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("update FGLITEM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public void insert(FGLITEM0Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFGLITEM0 begin!");

            
            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI = @"insert into FGLITEM0 
 (CORP_NO, ITEM, PREM_YEAR, ACCT_CODE, ACCT_CODEF, ACCT_CODER, ACCT_CODES, ACCT_CODEG, ACCT_CODET, COI_CODE, COI_CODEF
, COI_CODER, ACCT_CODEI, COMU_CODEF, COMU_CODER, ACCT_CODEO, COMU_PAYF, COMU_PAYR, ACCT_4570, ACCT_4571, ACCT_4572
, ACCT_4573, ACCT_4574, ACCT_4575, UPD_ID, UPD_YY, UPD_MM, UPD_DD, FILLER_10, FIELD10_1, FIELD10_2, FIELD10_3, FIELD10_4
, FIELD10_5, FIELD10_6, FIELD10_7, FIELD10_8
) 
 VALUES 
 (:CORP_NO, :ITEM, :PREM_YEAR, :ACCT_CODE, :ACCT_CODEF, :ACCT_CODER, :ACCT_CODES, :ACCT_CODEG, :ACCT_CODET, :COI_CODE, :COI_CODEF
, :COI_CODER, :ACCT_CODEI, :COMU_CODEF, :COMU_CODER, :ACCT_CODEO, :COMU_PAYF, :COMU_PAYR, :ACCT_4570, :ACCT_4571, :ACCT_4572
, :ACCT_4573, :ACCT_4574, :ACCT_4575, :UPD_ID, :UPD_YY, :UPD_MM, :UPD_DD, :FILLER_10, :FIELD10_1, :FIELD10_2, :FIELD10_3, :FIELD10_4
, :FIELD10_5, :FIELD10_6, :FIELD10_7, :FIELD10_8) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                cmd.Parameters.Add("ITEM", StringUtil.toString(procData.item));
                cmd.Parameters.Add("PREM_YEAR", StringUtil.toString(procData.premYear));
                cmd.Parameters.Add("ACCT_CODE", StringUtil.toString(procData.acctCode));
                cmd.Parameters.Add("ACCT_CODEF", StringUtil.toString(procData.acctCodef));
                cmd.Parameters.Add("ACCT_CODER", StringUtil.toString(procData.acctCoder));
                cmd.Parameters.Add("ACCT_CODES", StringUtil.toString(procData.acctCodes));
                cmd.Parameters.Add("ACCT_CODEG", StringUtil.toString(procData.acctCodeg));
                cmd.Parameters.Add("ACCT_CODET", StringUtil.toString(procData.acctCodet));
                cmd.Parameters.Add("COI_CODE", StringUtil.toString(procData.coiCode));
                cmd.Parameters.Add("COI_CODEF", StringUtil.toString(procData.coiCodef));
                cmd.Parameters.Add("COI_CODER", StringUtil.toString(procData.coiCoder));
                cmd.Parameters.Add("ACCT_CODEI", StringUtil.toString(procData.acctCodei));
                cmd.Parameters.Add("COMU_CODEF", StringUtil.toString(procData.comuCodef));
                cmd.Parameters.Add("COMU_CODER", StringUtil.toString(procData.comuCoder));
                cmd.Parameters.Add("ACCT_CODEO", StringUtil.toString(procData.acctCodeo));
                cmd.Parameters.Add("COMU_PAYF", StringUtil.toString(procData.comuPayf));
                cmd.Parameters.Add("COMU_PAYR", StringUtil.toString(procData.comuPayr));
                cmd.Parameters.Add("ACCT_4570", StringUtil.toString(procData.acct4570));
                cmd.Parameters.Add("ACCT_4571", StringUtil.toString(procData.acct4571));
                cmd.Parameters.Add("ACCT_4572", StringUtil.toString(procData.acct4572));
                cmd.Parameters.Add("ACCT_4573", StringUtil.toString(procData.acct4573));
                cmd.Parameters.Add("ACCT_4574", StringUtil.toString(procData.acct4574));
                cmd.Parameters.Add("ACCT_4575", StringUtil.toString(procData.acct4575));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_YY", StringUtil.toString(procData.updYy));
                cmd.Parameters.Add("UPD_MM", StringUtil.toString(procData.updMm));
                cmd.Parameters.Add("UPD_DD", StringUtil.toString(procData.updDd));
                cmd.Parameters.Add("FILLER_10", StringUtil.toString(procData.filler10));
                cmd.Parameters.Add("FIELD10_1", StringUtil.toString(procData.filler101));
                cmd.Parameters.Add("FIELD10_2", StringUtil.toString(procData.filler102));
                cmd.Parameters.Add("FIELD10_3", StringUtil.toString(procData.filler103));
                cmd.Parameters.Add("FIELD10_4", StringUtil.toString(procData.filler104));
                cmd.Parameters.Add("FIELD10_5", StringUtil.toString(procData.filler105));
                cmd.Parameters.Add("FIELD10_6", StringUtil.toString(procData.filler106));
                cmd.Parameters.Add("FIELD10_7", StringUtil.toString(procData.filler107));
                cmd.Parameters.Add("FIELD10_8", StringUtil.toString(procData.filler108));

                cmd.CommandText = strSQLI;

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("insertFGLITEM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }





    }
}