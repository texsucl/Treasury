
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;

namespace FGL.Web.Daos
{
    public class FGLItemCodeTranHisDao
    {
        public List<OGL00002Model> qryApprHis(OGL00002Model model, string apprDateB, string apprDateE, string apprStat)
        {
            bool bTranA = StringUtil.isEmpty(model.tranA);
            bool bTranB = StringUtil.isEmpty(model.tranB);
            bool bTranC = StringUtil.isEmpty(model.tranC);
            bool bTranD = StringUtil.isEmpty(model.tranD);
            bool bTranE = StringUtil.isEmpty(model.tranE);
            bool bTranF = StringUtil.isEmpty(model.tranF);
            bool bTranG = StringUtil.isEmpty(model.tranG);
            bool bTranH = StringUtil.isEmpty(model.tranH);
            bool bTranI = StringUtil.isEmpty(model.tranI);
            bool bTranJ = StringUtil.isEmpty(model.tranJ);
            bool bTranK = StringUtil.isEmpty(model.tranK);
            bool bapprStat = StringUtil.isEmpty(apprStat);
            bool bapprDateB = StringUtil.isEmpty(apprDateB);
            bool bapprDateE = StringUtil.isEmpty(apprDateE);
            DateTime sB = DateTime.Now;
            DateTime sE = DateTime.Now;

            if (!bapprDateB)
                sB = Convert.ToDateTime(apprDateB);

            if (!bapprDateE)
            {
                sE = Convert.ToDateTime(apprDateE);
                sE = sE.AddDays(1);
            }


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_ITEM_CODE_TRAN_HIS on appr.aply_no equals m.aply_no

                               join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on appr.appr_stat equals codeStatus.CODE into psStatus
                                   from xStatus in psStatus.DefaultIfEmpty()

                                   join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where (appr.appr_stat == "1" || appr.appr_stat == "2" || appr.appr_stat == "3")
                                     & (bTranA || m.tran_a == model.tranA)
                               & (bTranB || m.tran_b == model.tranB)
                               & (bTranC || m.tran_c == model.tranC)
                               & (bTranD || m.tran_d == model.tranD)
                               & (bTranE || m.tran_e == model.tranE)
                               & (bTranF || m.tran_f == model.tranF)
                               & (bTranG || m.tran_g == model.tranG)
                               & (bTranH || m.tran_h == model.tranH)
                               & (bTranI || m.tran_i == model.tranI)
                               & (bTranJ || m.tran_j == model.tranJ)
                               & (bTranK || m.tran_k == model.tranK)
                                    & (bapprStat || appr.appr_stat == apprStat)
                                     & (bapprDateB || appr.approve_datetime >= sB)
                                     & (bapprDateE || appr.approve_datetime < sE)
                                   select new OGL00002Model
                                   {
                                       aplyNo = m.aply_no,
                                       productNo = m.product_no,
                                       tranA = m.tran_a,
                                       tranB = m.tran_b,
                                       tranC = m.tran_c,
                                       tranD = m.tran_d,
                                       tranE = m.tran_e,
                                       tranF = m.tran_f,
                                       tranG = m.tran_g,
                                       tranH = m.tran_h,
                                       tranI = m.tran_i,
                                       tranJ = m.tran_j,
                                       tranK = m.tran_k,
                                       apprStat = (xStatus == null ? appr.appr_stat : xStatus.CODE_VALUE),
                                       execAction = m.exec_action.Trim(),
                                       execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                       updateId = appr.create_id,
                                       updateDatetime = appr.create_dt == null ? "" : SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim(),
                                       apprId = appr.appr_id,
                                       apprDt = appr.approve_datetime == null ? "" : SqlFunctions.DateName("year", appr.approve_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.approve_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", appr.approve_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.approve_datetime).Trim(),
                                   }).OrderByDescending(x => x.aplyNo).ToList<OGL00002Model>();



                    List<OGL00002Model> dataList = new List<OGL00002Model>();

                    foreach (OGL00002Model d in his)
                    {
                        bool bShow = true;
                        string productNo = d.productNo.PadRight(27, ' ');

                        OGL00002Model type1 = new OGL00002Model();
                        // type1.tempId = d.tempId + "|" + "1";
                        type1.tempId = d.tempId;
                        //  type1.rowType = "1";
                        type1.p01 = productNo.Substring(0, 1);
                        type1.p02 = productNo.Substring(1, 1);
                        type1.p03 = productNo.Substring(2, 1);
                        type1.p04 = productNo.Substring(3, 1);
                        type1.p05 = productNo.Substring(4, 1);
                        type1.p06 = productNo.Substring(5, 1);
                        type1.p07 = productNo.Substring(6, 1);
                        type1.p08 = productNo.Substring(7, 1);
                        type1.p09 = productNo.Substring(8, 1);
                        type1.p10 = productNo.Substring(9, 1);
                        type1.p11 = productNo.Substring(10, 1);
                        type1.p12 = productNo.Substring(11, 1);
                        type1.p13 = productNo.Substring(12, 1);
                        type1.p14 = productNo.Substring(13, 1);
                        type1.p15 = productNo.Substring(14, 1);
                        type1.p16 = productNo.Substring(15, 1);
                        type1.p17 = productNo.Substring(16, 1);
                        type1.p18 = productNo.Substring(17, 1);
                        type1.p19 = productNo.Substring(18, 1);
                        type1.p20 = productNo.Substring(19, 1);
                        type1.p21 = productNo.Substring(20, 1);
                        type1.p22 = productNo.Substring(21, 1);
                        type1.p23 = productNo.Substring(22, 1);
                        type1.p24 = productNo.Substring(23, 1);
                        type1.p25 = productNo.Substring(24, 1);
                        type1.p26 = productNo.Substring(25, 1);
                        type1.p27 = productNo.Substring(26, 1);

                        //20190708 加"查詢條件可以下保險商品編號1~27碼欄位"
                        if (!"".Equals(StringUtil.toString(model.p01)) & !StringUtil.toString(type1.p01).Equals((StringUtil.toString(model.p01))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p02)) & !StringUtil.toString(type1.p02).Equals((StringUtil.toString(model.p02))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p03)) & !StringUtil.toString(type1.p03).Equals((StringUtil.toString(model.p03))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p04)) & !StringUtil.toString(type1.p04).Equals((StringUtil.toString(model.p04))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p05)) & !StringUtil.toString(type1.p05).Equals((StringUtil.toString(model.p05))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p06)) & !StringUtil.toString(type1.p06).Equals((StringUtil.toString(model.p06))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p07)) & !StringUtil.toString(type1.p07).Equals((StringUtil.toString(model.p07))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p08)) & !StringUtil.toString(type1.p08).Equals((StringUtil.toString(model.p08))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p09)) & !StringUtil.toString(type1.p09).Equals((StringUtil.toString(model.p09))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p10)) & !StringUtil.toString(type1.p10).Equals((StringUtil.toString(model.p10))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p11)) & !StringUtil.toString(type1.p11).Equals((StringUtil.toString(model.p11))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p12)) & !StringUtil.toString(type1.p12).Equals((StringUtil.toString(model.p12))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p13)) & !StringUtil.toString(type1.p13).Equals((StringUtil.toString(model.p13))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p14)) & !StringUtil.toString(type1.p14).Equals((StringUtil.toString(model.p14))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p15)) & !StringUtil.toString(type1.p15).Equals((StringUtil.toString(model.p15))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p16)) & !StringUtil.toString(type1.p16).Equals((StringUtil.toString(model.p16))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p17)) & !StringUtil.toString(type1.p17).Equals((StringUtil.toString(model.p17))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p18)) & !StringUtil.toString(type1.p18).Equals((StringUtil.toString(model.p18))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p19)) & !StringUtil.toString(type1.p19).Equals((StringUtil.toString(model.p19))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p20)) & !StringUtil.toString(type1.p20).Equals((StringUtil.toString(model.p20))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p21)) & !StringUtil.toString(type1.p21).Equals((StringUtil.toString(model.p21))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p22)) & !StringUtil.toString(type1.p22).Equals((StringUtil.toString(model.p22))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p23)) & !StringUtil.toString(type1.p23).Equals((StringUtil.toString(model.p23))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p24)) & !StringUtil.toString(type1.p24).Equals((StringUtil.toString(model.p24))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p25)) & !StringUtil.toString(type1.p25).Equals((StringUtil.toString(model.p25))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p26)) & !StringUtil.toString(type1.p26).Equals((StringUtil.toString(model.p26))))
                            bShow = false;

                        if (!"".Equals(StringUtil.toString(model.p27)) & !StringUtil.toString(type1.p27).Equals((StringUtil.toString(model.p27))))
                            bShow = false;


                        if (bShow)
                            dataList.Add(d);


                    }


                    return dataList;
                }
            }

        }


        /// <summary>
        /// 依畫面條件刪除暫存檔資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delTmpFor00002(OGL00002Model model, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                string sql =
@"DELETE FGL_ITEM_CODE_TRAN_HIS
 WHERE APLY_NO = ''";

                if (!string.IsNullOrWhiteSpace(model.tranA))
                {
                    sql += " AND TRAN_A = @TRAN_A";
                    cmd.Parameters.AddWithValue("@TRAN_A", model.tranA);
                }

                if (!string.IsNullOrWhiteSpace(model.tranB))
                {
                    sql += " AND TRAN_B = @TRAN_B";
                    cmd.Parameters.AddWithValue("@TRAN_B", model.tranB);
                }

                if (!string.IsNullOrWhiteSpace(model.tranC))
                {
                    sql += " AND TRAN_C = @TRAN_C";
                    cmd.Parameters.AddWithValue("@TRAN_C", model.tranC);
                }

                if (!string.IsNullOrWhiteSpace(model.tranD))
                {
                    sql += " AND TRAN_D = @TRAN_D";
                    cmd.Parameters.AddWithValue("@TRAN_D", model.tranD);
                }

                if (!string.IsNullOrWhiteSpace(model.tranE))
                {
                    sql += " AND TRAN_E = @TRAN_E";
                    cmd.Parameters.AddWithValue("@TRAN_E", model.tranE);
                }

                if (!string.IsNullOrWhiteSpace(model.tranF))
                {
                    sql += " AND TRAN_F = @TRAN_F";
                    cmd.Parameters.AddWithValue("@TRAN_F", model.tranF);
                }

                if (!string.IsNullOrWhiteSpace(model.tranG))
                {
                    sql += " AND TRAN_G = @TRAN_G";
                    cmd.Parameters.AddWithValue("@TRAN_G", model.tranG);
                }

                if (!string.IsNullOrWhiteSpace(model.tranH))
                {
                    sql += " AND TRAN_H = @TRAN_H";
                    cmd.Parameters.AddWithValue("@TRAN_H", model.tranH);
                }

                if (!string.IsNullOrWhiteSpace(model.tranI))
                {
                    sql += " AND TRAN_I = @TRAN_I";
                    cmd.Parameters.AddWithValue("@TRAN_I", model.tranI);
                }

                if (!string.IsNullOrWhiteSpace(model.tranJ))
                {
                    sql += " AND TRAN_J = @TRAN_J";
                    cmd.Parameters.AddWithValue("@TRAN_J", model.tranJ);
                }

                if (!string.IsNullOrWhiteSpace(model.tranK))
                {
                    sql += " AND TRAN_K = @TRAN_K";
                    cmd.Parameters.AddWithValue("@TRAN_K", model.tranK);
                }


                cmd.CommandText = sql;

                int cnt = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                throw e;
            }

        }


        /// <summary>
        /// 依覆核單號查歷史異動
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<OGL00002Model> qryByAplyNo(string aplyNo)
        {
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_ITEM_CODE_TRAN_HIS on appr.aply_no equals m.aply_no

                               //join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on appr.APPR_STAT equals codeStatus.CODE into psStatus
                               //from xStatus in psStatus.DefaultIfEmpty()

                               join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where appr.aply_no == aplyNo

                                   select new OGL00002Model
                                   {
                                       aplyNo = m.aply_no,
                                       productNo = m.product_no,
                                       tranA = m.tran_a,
                                       tranB = m.tran_b,
                                       tranC = m.tran_c,
                                       tranD = m.tran_d,
                                       tranE = m.tran_e,
                                       tranF = m.tran_f,
                                       tranG = m.tran_g,
                                       tranH = m.tran_h,
                                       tranI = m.tran_i,
                                       tranJ = m.tran_j,
                                       tranK = m.tran_k,
                                       execAction = m.exec_action.Trim(),
                                       execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                       updateId = appr.create_id,
                                       updateDatetime = appr.create_dt == null ? "" : SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim(),

                                   }).ToList<OGL00002Model>();

                    return his;
                }
            }

        }


        public List<OGL00002Model> qryByTranCodeTmp(OGL00002Model d)
        {
            bool bTranA = StringUtil.isEmpty(d.tranA);
            bool bTranB = StringUtil.isEmpty(d.tranB);
            bool bTranC = StringUtil.isEmpty(d.tranC);
            bool bTranD = StringUtil.isEmpty(d.tranD);
            bool bTranE = StringUtil.isEmpty(d.tranE);
            bool bTranF = StringUtil.isEmpty(d.tranF);
            bool bTranG = StringUtil.isEmpty(d.tranG);
            bool bTranH = StringUtil.isEmpty(d.tranH);
            bool bTranI = StringUtil.isEmpty(d.tranI);
            bool bTranJ = StringUtil.isEmpty(d.tranJ);
            bool bTranK = StringUtil.isEmpty(d.tranK);


            List<OGL00002Model> rows = new List<OGL00002Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows = 
                            (from m in db.FGL_ITEM_CODE_TRAN_HIS

                             join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                             from xAction in psAction.DefaultIfEmpty()

                             where 1 == 1
                                & m.aply_no == ""
                                & m.exec_action == "A"
                                  & (bTranA || m.tran_a == d.tranA)
                               & (bTranB || m.tran_b == d.tranB)
                               & (bTranC || m.tran_c == d.tranC)
                               & (bTranD || m.tran_d == d.tranD)
                               & (bTranE || m.tran_e == d.tranE)
                               & (bTranF || m.tran_f == d.tranF)
                               & (bTranG || m.tran_g == d.tranG)
                               & (bTranH || m.tran_h == d.tranH)
                               & (bTranI || m.tran_i == d.tranI)
                               & (bTranJ || m.tran_j == d.tranJ)
                               & (bTranK || m.tran_k == d.tranK)

                             select new OGL00002Model()
                             {
                                 tempId = m.product_no,
                                 productNo = m.product_no,
                                 tranA = m.tran_a,
                                 tranB = m.tran_b,
                                 tranC = m.tran_c,
                                 tranD = m.tran_d,
                                 tranE = m.tran_e,
                                 tranF = m.tran_f,
                                 tranG = m.tran_g,
                                 tranH = m.tran_h,
                                 tranI = m.tran_i,
                                 tranJ = m.tran_j,
                                 tranK = m.tran_k,
                                 execAction = m.exec_action,
                                 execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                 dataStatus = "1",
                                 dataStatusDesc = "可異動",
                                 updateId = m.update_id == null ? "" : m.update_id.Trim(),
                                 updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                          SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                          SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                          SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("s", m.update_datetime).Trim(),

                             }).Distinct().OrderBy(x => x.tempId).ToList<OGL00002Model>()

                            ;

                }
            }

            return rows;
        }

        public List<OGL00002Model> qryByTranCode(OGL00002Model d)
        {
            bool bTranA = StringUtil.isEmpty(d.tranA);
            bool bTranB = StringUtil.isEmpty(d.tranB);
            bool bTranC = StringUtil.isEmpty(d.tranC);
            bool bTranD = StringUtil.isEmpty(d.tranD);
            bool bTranE = StringUtil.isEmpty(d.tranE);
            bool bTranF = StringUtil.isEmpty(d.tranF);
            bool bTranG = StringUtil.isEmpty(d.tranG);
            bool bTranH = StringUtil.isEmpty(d.tranH);
            bool bTranI = StringUtil.isEmpty(d.tranI);
            bool bTranJ = StringUtil.isEmpty(d.tranJ);
            bool bTranK = StringUtil.isEmpty(d.tranK);


            List<OGL00002Model> rows = new List<OGL00002Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows = (from m in db.FGL_ITEM_CODE_TRAN

                            join his in db.FGL_ITEM_CODE_TRAN_HIS.Where(x => x.exec_action != "A" & (x.appr_stat == "1" || x.appr_stat == "0")) on m.product_no equals his.product_no into psHis
                            from xHis in psHis.DefaultIfEmpty()
                            

                            join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                            from xStatus in psStatus.DefaultIfEmpty()

                            join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on xHis.exec_action equals codeAction.CODE into psAction
                            from xAction in psAction.DefaultIfEmpty()

                            where 1 == 1
                            & (xHis == null || (xHis != null && (xHis.appr_stat == "1" || (xHis.appr_stat == "0"))))
                                  & (bTranA || m.tran_a == d.tranA)
                               & (bTranB || m.tran_b == d.tranB)
                               & (bTranC || m.tran_c == d.tranC)
                               & (bTranD || m.tran_d == d.tranD)
                               & (bTranE || m.tran_e == d.tranE)
                               & (bTranF || m.tran_f == d.tranF)
                               & (bTranG || m.tran_g == d.tranG)
                               & (bTranH || m.tran_h == d.tranH)
                               & (bTranI || m.tran_i == d.tranI)
                               & (bTranJ || m.tran_j == d.tranJ)
                               & (bTranK || m.tran_k == d.tranK)

                            select new OGL00002Model()
                            {
                                tempId = m.product_no,
                                productNo = m.product_no,
                                tranA = (xHis == null ? m.tran_a : xHis.tran_a),
                                tranB = (xHis == null ? m.tran_b : xHis.tran_b),
                                tranC = (xHis == null ? m.tran_c : xHis.tran_c),
                                tranD = (xHis == null ? m.tran_d : xHis.tran_d),
                                tranE = (xHis == null ? m.tran_e : xHis.tran_e),
                                tranF = (xHis == null ? m.tran_f : xHis.tran_f),
                                tranG = (xHis == null ? m.tran_g : xHis.tran_g),
                                tranH = (xHis == null ? m.tran_h : xHis.tran_h),
                                tranI = (xHis == null ? m.tran_i : xHis.tran_i),
                                tranJ = (xHis == null ? m.tran_j : xHis.tran_j),
                                tranK = (xHis == null ? m.tran_k : xHis.tran_k),

                                execAction = (xHis == null ? String.Empty : xHis.exec_action),
                                execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                dataStatus = m.data_status.Trim(),
                                dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE),
                                apprId = m.appr_id,
                                apprDt = m.approve_datetime == null ? "" :(SqlFunctions.DateName("year", m.approve_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.approve_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.approve_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.approve_datetime).Trim()),

                                updateId = (xHis == null ? m.update_id : xHis.update_id),
                                updateDatetime = (xHis == null ? SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.update_datetime).Trim()

                                                                         : SqlFunctions.DateName("year", xHis.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", xHis.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", xHis.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", xHis.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", xHis.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", xHis.update_datetime).Trim())


                            }).Distinct().OrderBy(x => x.tempId).ToList<OGL00002Model>()

                            .Union
                            (from m in db.FGL_ITEM_CODE_TRAN_HIS


                             join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                             from xAction in psAction.DefaultIfEmpty()

                             where 1 == 1
                                //   & m.APLY_NO == ""
                                & m.exec_action == "A"
                                & (m.appr_stat == "0" || m.appr_stat == "1")
                                  & (bTranA || m.tran_a == d.tranA)
                               & (bTranB || m.tran_b == d.tranB)
                               & (bTranC || m.tran_c == d.tranC)
                               & (bTranD || m.tran_d == d.tranD)
                               & (bTranE || m.tran_e == d.tranE)
                               & (bTranF || m.tran_f == d.tranF)
                               & (bTranG || m.tran_g == d.tranG)
                               & (bTranH || m.tran_h == d.tranH)
                               & (bTranI || m.tran_i == d.tranI)
                               & (bTranJ || m.tran_j == d.tranJ)
                               & (bTranK || m.tran_k == d.tranK)

                             select new OGL00002Model()
                             {
                                 tempId = m.product_no,
                                 productNo = m.product_no,
                                 tranA = m.tran_a,
                                 tranB = m.tran_b,
                                 tranC = m.tran_c,
                                 tranD = m.tran_d,
                                 tranE = m.tran_e,
                                 tranF = m.tran_f,
                                 tranG = m.tran_g,
                                 tranH = m.tran_h,
                                 tranI = m.tran_i,
                                 tranJ = m.tran_j,
                                 tranK = m.tran_k,
                                 execAction = m.exec_action,
                                 execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                 dataStatus = m.aply_no == "" ? "1" : "2",
                                 dataStatusDesc = m.aply_no == "" ? "可異動" : "凍結中",
                                 updateId = m.update_id == null ? "" : m.update_id.Trim(),
                                 updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                          SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                          SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                          SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("s", m.update_datetime).Trim(),

                             }).Distinct().OrderBy(x => x.tempId).ToList<OGL00002Model>()

                            ;

                }
            }

            return rows;
        }



        public void updateByKey(FGL_ITEM_CODE_TRAN_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql =
@"UPDATE FGL_ITEM_CODE_TRAN_HIS
   SET  EXEC_ACTION = @EXEC_ACTION
      , TRAN_A = @TRAN_A
      , TRAN_B = @TRAN_B
      , TRAN_C = @TRAN_C
      , TRAN_D = @TRAN_D
      , TRAN_E = @TRAN_E
      , TRAN_F = @TRAN_F
      , TRAN_G = @TRAN_G
      , TRAN_H = @TRAN_H
      , TRAN_I = @TRAN_I
      , TRAN_J = @TRAN_J
      , TRAN_K = @TRAN_K
      , UPDATE_ID = @UPDATE_ID
      , UPDATE_DATETIME = @UPDATE_DATETIME
 WHERE APLY_NO = @APLY_NO
   AND PRODUCT_NO = @PRODUCT_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(his.exec_action));
                cmd.Parameters.AddWithValue("@PRODUCT_NO", his.product_no.TrimEnd().ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_A", StringUtil.toString(his.tran_a).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_B", StringUtil.toString(his.tran_b).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_C", StringUtil.toString(his.tran_c).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_D", StringUtil.toString(his.tran_d).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_E", StringUtil.toString(his.tran_e).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_F", StringUtil.toString(his.tran_f).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_G", StringUtil.toString(his.tran_g).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_H", StringUtil.toString(his.tran_h).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_I", StringUtil.toString(his.tran_i).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_J", StringUtil.toString(his.tran_j).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_K", StringUtil.toString(his.tran_k).ToUpper());

                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(his.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)his.update_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void deleteByKey(FGL_ITEM_CODE_TRAN_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql =
@"DELETE FGL_ITEM_CODE_TRAN_HIS
 WHERE APLY_NO = @APLY_NO
   AND PRODUCT_NO = @PRODUCT_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@PRODUCT_NO", his.product_no.TrimEnd());

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public FGL_ITEM_CODE_TRAN_HIS qryByKey(string aplyNo, string productNo)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FGL_ITEM_CODE_TRAN_HIS d = new FGL_ITEM_CODE_TRAN_HIS();
                d = db.FGL_ITEM_CODE_TRAN_HIS
                    .Where(x => x.aply_no == aplyNo & x.product_no == productNo)
                    .FirstOrDefault<FGL_ITEM_CODE_TRAN_HIS>();

                return d;
            }

        }


        public void updateApprStat(string apprStat, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql =
@"UPDATE FGL_ITEM_CODE_TRAN_HIS
   SET  APPR_STAT = @APPR_STAT
 WHERE APLY_NO = @APLY_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(apprStat));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 承辦人執行"申請覆核"
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateAplyNo(FGL_ITEM_CODE_TRAN_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql =
@"UPDATE FGL_ITEM_CODE_TRAN_HIS
   SET  APLY_NO = @APLY_NO
      , APPR_STAT = @APPR_STAT
      , UPDATE_ID = @UPDATE_ID
      , UPDATE_DATETIME = @UPDATE_DATETIME
 WHERE APLY_NO = ''
   AND PRODUCT_NO = @PRODUCT_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(his.appr_stat));
                cmd.Parameters.AddWithValue("@PRODUCT_NO", his.product_no.TrimEnd());
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(his.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)his.update_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void insert(FGL_ITEM_CODE_TRAN_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_CODE_TRAN_HIS
                   ([APLY_NO]
                   ,[EXEC_ACTION]
                   ,[APPR_STAT]
                   ,[PRODUCT_NO]
                   ,[TRAN_A]
                   ,[TRAN_B]
                   ,[TRAN_C]
                   ,[TRAN_D]
                   ,[TRAN_E]
                   ,[TRAN_F]
                   ,[TRAN_G]
                   ,[TRAN_H]
                   ,[TRAN_I]
                   ,[TRAN_J]
                   ,[TRAN_K]
                   ,[UPDATE_ID]
                   ,[UPDATE_DATETIME])

             VALUES
                  (@APLY_NO
                   ,@EXEC_ACTION
                   ,@APPR_STAT
                   ,@PRODUCT_NO
                   ,@TRAN_A
                   ,@TRAN_B
                   ,@TRAN_C
                   ,@TRAN_D
                   ,@TRAN_E
                   ,@TRAN_F
                   ,@TRAN_G
                   ,@TRAN_H
                   ,@TRAN_I
                   ,@TRAN_J
                   ,@TRAN_K
                   ,@UPDATE_ID
                   ,@UPDATE_DATETIME)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(d.exec_action));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(d.appr_stat));
                cmd.Parameters.AddWithValue("@PRODUCT_NO", d.product_no.TrimEnd().ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_A", StringUtil.toString(d.tran_a).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_B", StringUtil.toString(d.tran_b).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_C", StringUtil.toString(d.tran_c).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_D", StringUtil.toString(d.tran_d).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_E", StringUtil.toString(d.tran_e).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_F", StringUtil.toString(d.tran_f).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_G", StringUtil.toString(d.tran_g).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_H", StringUtil.toString(d.tran_h).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_I", StringUtil.toString(d.tran_i).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_J", StringUtil.toString(d.tran_j).ToUpper());
                cmd.Parameters.AddWithValue("@TRAN_K", StringUtil.toString(d.tran_k).ToUpper());
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.update_datetime ?? System.DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }

    }
}