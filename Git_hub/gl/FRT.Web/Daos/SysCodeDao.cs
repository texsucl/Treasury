using FRT.Web.BO;
using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.Daos
{
    public class SysCodeDao
    {

        /// <summary>
        /// 畫面下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string sysCd, string cType, bool bPreCode)
        {
            dbFGLEntities context = new dbFGLEntities();

            //var result = context.TypeDefine.Where(x => x.CTYPE == cType).OrderBy(x => x.ISORTBY);

            var result1 = (from code in context.SYS_CODE
                           where code.SYS_CD == sysCd
                             & code.CODE_TYPE == cType
                           orderby code.ISORTBY
                           select new
                           {
                               CCODE = code.CODE.Trim(),
                               CVALUE = bPreCode ? code.CODE.Trim() + "." + code.CODE_VALUE.Trim() : code.CODE_VALUE.Trim()
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


        /// <summary>
        /// for GRID下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public string jqGridList(string sysCd, string cType, bool bPreCode)
        {
            var codeList = loadSelectList(sysCd, cType, bPreCode);
            string controlStr = "";
            foreach (var item in codeList)
            {
                controlStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }

        public SYS_CODE qryByKey(string sysCd, string cType, string code)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.SYS_CODE.Where(x => x.SYS_CD == sysCd & x.CODE_TYPE == cType & x.CODE == code)
                .OrderBy(x => x.ISORTBY).FirstOrDefault();


            return result;
        }

        /// <summary>
        /// 依type查代碼
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public List<SYS_CODE> qryByType(string sysCd, string cType)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.SYS_CODE.Where(x => x.SYS_CD == sysCd & x.CODE_TYPE == cType).OrderBy(x => x.ISORTBY).ToList();


            return result;
        }


        public Dictionary<string, string> qryByTypeDic(string sysCd, string cType)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result1 = (from code in context.SYS_CODE
                           where code.SYS_CD == sysCd
                             & code.CODE_TYPE == cType
                           orderby code.ISORTBY
                           select new
                           {
                               CCODE = code.CODE.Trim(),
                               CVALUE = code.CODE_VALUE.Trim()
                           }
                           ).ToDictionary(x => x.CCODE, x => x.CVALUE);


            return result1;
        }

        public SYS_CODE qryByReserve(string sysCd, string cType, string[] code, string reserve1, string reserve2, string reserve3, string mailGroup = null)
        {
            List<SYS_CODE> codeRows = qryByType(sysCd, cType);

            bool bReserve1 = StringUtil.isEmpty(reserve1);
            bool bReserve2 = StringUtil.isEmpty(reserve2);
            bool bReserve3 = StringUtil.isEmpty(reserve3);

            SYS_CODE rtn = new SYS_CODE();

            foreach (SYS_CODE d in codeRows)
            {
                string[] reserve1Arr = StringUtil.toString(d.RESERVE1).Split('|');
                string[] reserve2Arr = StringUtil.toString(d.RESERVE2).Split('|');
                string[] reserve3Arr = StringUtil.toString(d.RESERVE3).Split('|');

                if (string.IsNullOrWhiteSpace(mailGroup))
                {
                    if (code != null && !code.Contains(d.CODE))
                        continue;
                }
                else
                {
                    if (code != null && !(d.CODE.IndexOf(mailGroup) > -1))
                        continue;
                }

                if (!bReserve1 & !reserve1Arr.Contains(reserve1))
                    continue;

                if (!bReserve2 & !reserve2Arr.Contains(reserve2))
                    continue;

                if (!bReserve3 & !reserve3Arr.Contains(reserve3))
                    continue;

                rtn = d;

            }
            return rtn;
        }
    }
}