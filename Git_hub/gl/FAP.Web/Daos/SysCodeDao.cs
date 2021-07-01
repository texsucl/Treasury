﻿using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FAP.Web.Daos
{
    public class SysCodeDao
    {

        /// <summary>
        /// 畫面下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string sysCd, string cType, bool bPreCode, string selectedValue = null, bool allFlag = false)
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

            if (allFlag)
            {
                var _result1 = result1.ToList();
                _result1.Insert(0, new { CCODE = "All", CVALUE = "All" });
                return new SelectList
                (
                items: _result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)selectedValue
                );
            }
            else
            {
                return new SelectList
                (
                items: result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)selectedValue
                );
            }
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


        /// <summary>
        /// 依type查代碼
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public List<SYS_CODE> qryByType(string sysCd, string cType)
        {
            using (dbFGLEntities context = new dbFGLEntities())
            {
                return context.SYS_CODE.AsNoTracking().Where(x => x.SYS_CD == sysCd & x.CODE_TYPE == cType).OrderBy(x => x.ISORTBY).ToList();
            }
            //dbFGLEntities context = new dbFGLEntities();

            //var result = context.SYS_CODE.Where(x => x.SYS_CD == sysCd & x.CODE_TYPE == cType).OrderBy(x => x.ISORTBY).ToList();
            

            //return result;
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
    }
}