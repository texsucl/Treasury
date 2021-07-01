
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FGLAACT0會計科目主檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FGLAACTDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public Dictionary<string, OGL00003DModel> qryForOGL00005(Dictionary<string, OGL00003DModel> smpMap)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT CORP_NO, ACT_NUM" +
                    " FROM LGLAACT1 " +
                    " WHERE 1=1 " +
                    " AND CORP_NO = :CORP_NO" +
                    " AND ACT_NUM = :ACT_NUM";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;


                foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("CORP_NO", item.Value.corpNo);
                    cmd.Parameters.Add("ACT_NUM", item.Key);

                    DbDataReader result = cmd.ExecuteReader();

                    while (result.Read())
                    {
                        item.Value.bExist = true;
                        break;
                    }
                }

                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return smpMap;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void procInvalid(string execAction, List<FGLAACT0Model> aactList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("procInvalid LGLAACT1 begin!");
            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();
            string strSQL = @"
UPDATE LGLAACT1 
  SET INVALID_YY = :INVALID_YY
     ,INVALID_MM = :INVALID_MM
     ,INVALID_DD = :INVALID_DD
     ,UPD_ID = :UPD_ID
     ,UPD_YY = :UPD_YY
     ,UPD_MM = :UPD_MM
     ,UPD_DD = :UPD_DD
WHERE CORP_NO = :CORP_NO AND ACT_NUM = :ACT_NUM";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FGLAACT0Model procData in aactList)
                {

                    cmd.Parameters.Clear();

                    //20190806 刪除商品作業，下送到AS400的失效日期改成跟生效日期相同(莉婷)
                    if ("D".Equals(execAction))
                    {
                        cmd.Parameters.Add("INVALID_YY", procData.effYy);
                        cmd.Parameters.Add("INVALID_MM", procData.effMm);
                        cmd.Parameters.Add("INVALID_DD", procData.effDd);
                    }
                    else {
                        cmd.Parameters.Add("INVALID_YY", nowStr[0].Substring(0, 4));
                        cmd.Parameters.Add("INVALID_MM", nowStr[0].Substring(4, 2));
                        cmd.Parameters.Add("INVALID_DD", nowStr[0].Substring(6, 2));
                    }
                    
                    cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                    cmd.Parameters.Add("UPD_YY", StringUtil.toString(procData.updYy));
                    cmd.Parameters.Add("UPD_MM", StringUtil.toString(procData.updMm));
                    cmd.Parameters.Add("UPD_DD", StringUtil.toString(procData.updDd));
                    cmd.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                    cmd.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));

                    logger.Info("procData.corpNo" + procData.corpNo);
                    logger.Info("procData.actNum:" + procData.actNum);

                    cmd.ExecuteNonQuery();  
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("procInvalid LGLAACT1 end!");
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void proc(List<FGLAACT0Model>aactList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFGLAACT0 begin!");
            bool bExist = false;

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"SELECT ACT_NUM FROM LGLAACT1 WHERE CORP_NO = :CORP_NO AND ACT_NUM = :ACT_NUM";

            try
            {
                cmdQ.Connection = conn;
                cmdQ.Transaction = transaction;
                cmdQ.CommandText = strSQLQ;

                foreach (FGLAACT0Model procData in aactList) {
                    bExist = false;

                    cmdQ.Parameters.Clear();
                    cmdQ.Parameters.Add("CORP_NO", StringUtil.toString(procData.corpNo));
                    cmdQ.Parameters.Add("ACT_NUM", StringUtil.toString(procData.actNum));

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
                }



                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("insertFGLAACT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

        public void update(FGLAACT0Model d, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("update FGLAACT0 begin!");


            EacCommand cmd = new EacCommand();

            string strSQLWhere = @" WHERE CORP_NO = :CORP_NO
               AND ACT_NUM = :ACT_NUM";

            string strSQLI = "";
            strSQLI = @"
            UPDATE FGLAACT0 
              SET ACT_TYPE = :ACT_TYPE
                 ,ACT_ATRB = :ACT_ATRB
                 ,DB_SIGN = :DB_SIGN
                 ,ACT_ENGL = :ACT_ENGL
                 ,PRE_REMARK = :PRE_REMARK
                 ,RM_REMARK = :RM_REMARK
                 ,UNIT_REMRK = :UNIT_REMRK
                 ,ITEM_REMRK = :ITEM_REMRK
                 ,FAMT_REMRK = :FAMT_REMRK
                 ,FNO_REMRK = :FNO_REMRK
                 ,BONU_REMRK = :BONU_REMRK
                 ,ATR_CODE = :ATR_CODE
                 ,CORD = :CORD
                 ,CR_CODE = :CR_CODE
                 ,EFF_YY = :EFF_YY
                 ,EFF_MM = :EFF_MM
                 ,EFF_DD = :EFF_DD
                 ,INVALID_YY = :INVALID_YY
                 ,INVALID_MM = :INVALID_MM
                 ,INVALID_DD = :INVALID_DD
                 ,UPD_ID = :UPD_ID
                 ,UPD_YY = :UPD_YY
                 ,UPD_MM = :UPD_MM
                 ,UPD_DD = :UPD_DD
                 ,FILLER_1 = :FILLER_1
                 ,FILLER_8 = :FILLER_8
                 ,FILLER_10 = :FILLER_10
             ";


            //            strSQLI = @"
            //UPDATE FGLAACT0 
            //  SET ACT_TYPE = :ACT_TYPE
            //     ,ACT_ATRB = :ACT_ATRB
            //     ,DB_SIGN = :DB_SIGN
            //     ,ACT_NAME = :ACT_NAME
            //     ,ACT_SHORT = :ACT_SHORT
            //     ,ACT_ENGL = :ACT_ENGL
            //     ,PRE_REMARK = :PRE_REMARK
            //     ,RM_REMARK = :RM_REMARK
            //     ,UNIT_REMRK = :UNIT_REMRK
            //     ,ITEM_REMRK = :ITEM_REMRK
            //     ,FAMT_REMRK = :FAMT_REMRK
            //     ,FNO_REMRK = :FNO_REMRK
            //     ,BONU_REMRK = :BONU_REMRK
            //     ,ATR_CODE = :ATR_CODE
            //     ,CORD = :CORD
            //     ,CR_CODE = :CR_CODE
            //     ,EFF_YY = :EFF_YY
            //     ,EFF_MM = :EFF_MM
            //     ,EFF_DD = :EFF_DD
            //     ,INVALID_YY = :INVALID_YY
            //     ,INVALID_MM = :INVALID_MM
            //     ,INVALID_DD = :INVALID_DD
            //     ,UPD_ID = :UPD_ID
            //     ,UPD_YY = :UPD_YY
            //     ,UPD_MM = :UPD_MM
            //     ,UPD_DD = :UPD_DD
            //     ,FILLER_1 = :FILLER_1
            //     ,FILLER_8 = :FILLER_8
            //     ,FILLER_10 = :FILLER_10
            // WHERE CORP_NO = :CORP_NO
            //   AND ACT_NUM = :ACT_NUM";


            string actName = StringUtil.toString(d.actName);
            string actShort = StringUtil.toString(d.actShort);

            strSQLI += " ,ACT_NAME = '" + (actName.Length > 20 ? actName.Substring(0, 20) : actName) + "'";
            strSQLI += " ,ACT_SHORT = '" + (actShort.Length > 6 ? actShort.Substring(0, 6) : actShort) + "'";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQLI + strSQLWhere;

                cmd.Parameters.Clear();


                cmd.Parameters.Add("ACT_TYPE", StringUtil.toString(d.actType));
                cmd.Parameters.Add("ACT_ATRB", StringUtil.toString(d.actAtrb));
                cmd.Parameters.Add("DB_SIGN", StringUtil.toString(d.dbSign));
                //cmd.Parameters.Add("ACT_NAME", StringUtil.toString(d.actName));
                //cmd.Parameters.Add("ACT_SHORT", StringUtil.toString(d.actShort));
                cmd.Parameters.Add("ACT_ENGL", StringUtil.toString(d.actEngl));
                cmd.Parameters.Add("PRE_REMARK", StringUtil.toString(d.preRemark));
                cmd.Parameters.Add("RM_REMARK", StringUtil.toString(d.rmRemark));
                cmd.Parameters.Add("UNIT_REMRK", StringUtil.toString(d.unitRemrk));
                cmd.Parameters.Add("ITEM_REMRK", StringUtil.toString(d.itemRemrk));
                cmd.Parameters.Add("FAMT_REMRK", StringUtil.toString(d.famtRemrk));
                cmd.Parameters.Add("FNO_REMRK", StringUtil.toString(d.fnoRemrk));
                cmd.Parameters.Add("BONU_REMRK", StringUtil.toString(d.bonuRemrk));
                cmd.Parameters.Add("ATR_CODE", StringUtil.toString(d.atrCode));
                cmd.Parameters.Add("CORD", StringUtil.toString(d.cord));
                cmd.Parameters.Add("CR_CODE", StringUtil.toString(d.crCode));
                cmd.Parameters.Add("EFF_YY", StringUtil.toString(d.effYy));
                cmd.Parameters.Add("EFF_MM", StringUtil.toString(d.effMm));
                cmd.Parameters.Add("EFF_DD", StringUtil.toString(d.effDd));
                cmd.Parameters.Add("INVALID_YY", StringUtil.toString(d.invalidYy));
                cmd.Parameters.Add("INVALID_MM", StringUtil.toString(d.invalidMm));
                cmd.Parameters.Add("INVALID_DD", StringUtil.toString(d.invalidDd));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(d.updId));
                cmd.Parameters.Add("UPD_YY", StringUtil.toString(d.updYy));
                cmd.Parameters.Add("UPD_MM", StringUtil.toString(d.updMm));
                cmd.Parameters.Add("UPD_DD", StringUtil.toString(d.updDd));
                cmd.Parameters.Add("FILLER_1", StringUtil.toString(d.filler1));
                cmd.Parameters.Add("FILLER_8", StringUtil.toString(d.filler8));
                cmd.Parameters.Add("FILLER_10", StringUtil.toString(d.filler10));

                cmd.Parameters.Add("CORP_NO", StringUtil.toString(d.corpNo));
                cmd.Parameters.Add("ACT_NUM", StringUtil.toString(d.actNum));

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("update FGLAACT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        public void insert(FGLAACT0Model d, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFGLAACT0 begin!");


            EacCommand cmd = new EacCommand();

            string strSQLI = "";
            strSQLI = @"insert into FGLAACT0 
 (CORP_NO, ACT_NUM, ACT_TYPE, ACT_ATRB, DB_SIGN, ACT_ENGL , PRE_REMARK, RM_REMARK , UNIT_REMRK
, ITEM_REMRK, FAMT_REMRK, FNO_REMRK, BONU_REMRK, ATR_CODE, CORD, CR_CODE, EFF_YY, EFF_MM, EFF_DD 
, INVALID_YY, INVALID_MM, INVALID_DD, UPD_ID, UPD_YY, UPD_MM, UPD_DD, FILLER_1, FILLER_8, FILLER_10, ACT_NAME, ACT_SHORT) 
 VALUES 
 (:CORP_NO, :ACT_NUM, :ACT_TYPE, :ACT_ATRB, :DB_SIGN, :ACT_ENGL , :PRE_REMARK, :RM_REMARK , :UNIT_REMRK
, :ITEM_REMRK, :FAMT_REMRK, :FNO_REMRK, :BONU_REMRK, :ATR_CODE, :CORD, :CR_CODE, :EFF_YY, :EFF_MM, :EFF_DD 
, :INVALID_YY, :INVALID_MM, :INVALID_DD, :UPD_ID, :UPD_YY, :UPD_MM, :UPD_DD, :FILLER_1, :FILLER_8, :FILLER_10 ";

//            strSQLI = @"insert into FGLAACT0 
// (CORP_NO, ACT_NUM, ACT_TYPE, ACT_ATRB, DB_SIGN, ACT_NAME, ACT_SHORT, ACT_ENGL , PRE_REMARK, RM_REMARK , UNIT_REMRK
//, ITEM_REMRK, FAMT_REMRK, FNO_REMRK, BONU_REMRK, ATR_CODE, CORD, CR_CODE, EFF_YY, EFF_MM, EFF_DD 
//, INVALID_YY, INVALID_MM, INVALID_DD, UPD_ID, UPD_YY, UPD_MM, UPD_DD, FILLER_1, FILLER_8, FILLER_10) 
// VALUES 
// (:CORP_NO, :ACT_NUM, :ACT_TYPE, :ACT_ATRB, :DB_SIGN, :ACT_NAME, :ACT_SHORT, :ACT_ENGL , :PRE_REMARK, :RM_REMARK , :UNIT_REMRK
//, :ITEM_REMRK, :FAMT_REMRK, :FNO_REMRK, :BONU_REMRK, :ATR_CODE, :CORD, :CR_CODE, :EFF_YY, :EFF_MM, :EFF_DD 
//, :INVALID_YY, :INVALID_MM, :INVALID_DD, :UPD_ID, :UPD_YY, :UPD_MM, :UPD_DD, :FILLER_1, :FILLER_8, :FILLER_10) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                string actName = StringUtil.toString(d.actName);
                string actShort = StringUtil.toString(d.actShort);


                actName = (actName.Length > 20 ? actName.Substring(0, 20) : actName);
                actShort = (actShort.Length > 6 ? actShort.Substring(0, 6) : actShort);


                strSQLI += " ,'" + actName + "'";
                strSQLI += " ,'" + actShort + "'";
                strSQLI += " )";

                cmd.CommandText = strSQLI;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("CORP_NO", StringUtil.toString(d.corpNo));
                cmd.Parameters.Add("ACT_NUM", StringUtil.toString(d.actNum));
                cmd.Parameters.Add("ACT_TYPE", StringUtil.toString(d.actType));
                cmd.Parameters.Add("ACT_ATRB", StringUtil.toString(d.actAtrb));
                cmd.Parameters.Add("DB_SIGN", StringUtil.toString(d.dbSign));
                //cmd.Parameters.Add("ACT_NAME", StringUtil.toString(d.actName).TrimEnd());

                //cmd.Parameters.Add("ACT_SHORT", StringUtil.toString(d.actShort).TrimEnd());
                cmd.Parameters.Add("ACT_ENGL", StringUtil.toString(d.actEngl));
                cmd.Parameters.Add("PRE_REMARK", StringUtil.toString(d.preRemark));
                cmd.Parameters.Add("RM_REMARK", StringUtil.toString(d.rmRemark));
                cmd.Parameters.Add("UNIT_REMRK", StringUtil.toString(d.unitRemrk));
                cmd.Parameters.Add("ITEM_REMRK", StringUtil.toString(d.itemRemrk));
                cmd.Parameters.Add("FAMT_REMRK", StringUtil.toString(d.famtRemrk));
                cmd.Parameters.Add("FNO_REMRK", StringUtil.toString(d.fnoRemrk));
                cmd.Parameters.Add("BONU_REMRK", StringUtil.toString(d.bonuRemrk));
                cmd.Parameters.Add("ATR_CODE", StringUtil.toString(d.atrCode));
                cmd.Parameters.Add("CORD", StringUtil.toString(d.cord));
                cmd.Parameters.Add("CR_CODE", StringUtil.toString(d.crCode));
                cmd.Parameters.Add("EFF_YY", StringUtil.toString(d.effYy));
                cmd.Parameters.Add("EFF_MM", StringUtil.toString(d.effMm));
                cmd.Parameters.Add("EFF_DD", StringUtil.toString(d.effDd));
                cmd.Parameters.Add("INVALID_YY", StringUtil.toString(d.invalidYy));
                cmd.Parameters.Add("INVALID_MM", StringUtil.toString(d.invalidMm));
                cmd.Parameters.Add("INVALID_DD", StringUtil.toString(d.invalidDd));
                cmd.Parameters.Add("UPD_ID", StringUtil.toString(d.updId));
                cmd.Parameters.Add("UPD_YY", StringUtil.toString(d.updYy));
                cmd.Parameters.Add("UPD_MM", StringUtil.toString(d.updMm));
                cmd.Parameters.Add("UPD_DD", StringUtil.toString(d.updDd));
                cmd.Parameters.Add("FILLER_1", StringUtil.toString(d.filler1));
                cmd.Parameters.Add("FILLER_8", StringUtil.toString(d.filler8));
                cmd.Parameters.Add("FILLER_10", StringUtil.toString(d.filler10));

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("insertFGLAACT0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


    }
}