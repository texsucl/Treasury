using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.Daos
{
    public class SysParaDao
    {
        /// <summary>
        /// 依type查代碼(ForORTB010)
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public List<ORTB010Model> qryForORTB010(string sysCd, string grpId, string reserve2)
        {
            bool bReserve2 = StringUtil.isEmpty(reserve2);

            dbFGLEntities context = new dbFGLEntities();

            var rows = (from para in context.SYS_PARA
                        where para.SYS_CD == sysCd
                        & para.GRP_ID == grpId
                        & (bReserve2 || (para.RESERVE2 == reserve2))
                        select new ORTB010Model
                        {
                            sysCd = para.SYS_CD,
                            grpId = para.GRP_ID,
                            paraId = para.PARA_ID,
                            paraValue = para.PARA_VALUE,
                            remark = para.REMARK,
                            reserve1 = para.RESERVE1,
                            reserve2 = para.RESERVE2,
                            reserve3 = para.RESERVE3

                        }).OrderBy(x => x.reserve3).ToList<ORTB010Model>();
                        
            return rows;

        }


        /// <summary>
        /// 依"系統別+GRP_ID"查詢
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="grpId"></param>
        /// <returns></returns>
        public List<SYS_PARA> qryForGrpId(string sysCd, string grpId)
        {

            dbFGLEntities context = new dbFGLEntities();


            List<SYS_PARA> roleList = context.SYS_PARA
                        .Where(x => x.SYS_CD == sysCd && x.GRP_ID == grpId)
                        .ToList<SYS_PARA>();


            return roleList;

        }


        /// <summary>
        /// 依鍵項查詢("SYS_CD + GRP_ID + PARA_ID")
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="grpId"></param>
        /// <param name="paraId"></param>
        /// <returns></returns>
        public SYS_PARA qryByKey(string sysCd, string grpId, string paraId)
        {

            dbFGLEntities context = new dbFGLEntities();


            SYS_PARA data = context.SYS_PARA
                        .Where(x => x.SYS_CD == sysCd && x.GRP_ID == grpId && x.PARA_ID == paraId)
                        .FirstOrDefault();


            return data;

        }


        /// <summary>
        /// 依"系統別+GRP_ID"組成符合條件的下拉選單
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="grpId"></param>
        /// <param name="bPreCode"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string sysCd, string grpId, bool bPreCode, string reserve1)
        {
            bool bReserve1 = StringUtil.isEmpty(reserve1);

            dbFGLEntities context = new dbFGLEntities();

            var result1 = (from para in context.SYS_PARA
                           where para.SYS_CD == sysCd
                             & para.GRP_ID == grpId
                             & (bReserve1 || (para.RESERVE1 == reserve1))
                           orderby para.PARA_ID
                           select new
                           {
                               CCODE = para.PARA_ID.Trim(),
                               CVALUE = bPreCode ? para.PARA_ID.Trim() + "." + para.PARA_VALUE.Trim() : para.PARA_VALUE.Trim()
                           }
                           );

            var items = new SelectList
                (
                items: result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)null
                );

            return items;
        }


        public void updateParaVale(List<SYS_PARA> data, SqlConnection conn, SqlTransaction transaction)
        {
            DateTime dtNow = DateTime.Now;

            try
            {
                string sql = @"UPDATE [SYS_PARA]
SET PARA_VALUE = @PARA_VALUE
WHERE SYS_CD = @SYS_CD
  AND GRP_ID = @GRP_ID
  AND PARA_ID = @PARA_ID
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                foreach (SYS_PARA d in data)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@SYS_CD", d.SYS_CD);
                    cmd.Parameters.AddWithValue("@GRP_ID", d.GRP_ID);
                    cmd.Parameters.AddWithValue("@PARA_ID", d.PARA_ID);
                    cmd.Parameters.AddWithValue("@PARA_VALUE", d.PARA_VALUE);

                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}