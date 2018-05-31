using Treasury.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Treasury.Web.Daos
{
    public class SysCodeDao
    {

        /// <summary>
        /// 畫面下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList(String cType)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            //var result = context.TypeDefine.Where(x => x.CTYPE == cType).OrderBy(x => x.ISORTBY);

            var result1 = (from type in context.SYS_CODE
                           where type.CODE_TYPE == cType
                           orderby type.ISORTBY
                           select new
                           {
                               CCODE = type.CODE.Trim(),
                               CVALUE = type.CODE_VALUE.Trim()
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
        public string jqGridList(String cType)
        {
            var codeList = loadSelectList(cType);
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
        public List<SYS_CODE> qryByType(String cType)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var result = context.SYS_CODE.Where(x => x.CODE_TYPE == cType).OrderBy(x => x.ISORTBY).ToList();
            

            return result;
        }



        /// <summary>
        /// 以鍵項查詢
        /// </summary>
        /// <param name="codeType"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public SYS_CODE qryByKey(string codeType, string code)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            SYS_CODE sysCode = new SYS_CODE();
            sysCode = context.SYS_CODE.Where(x => x.CODE_TYPE == codeType && x.CODE == code)
                .OrderBy(x => x.ISORTBY).FirstOrDefault();


            return sysCode;
        }


        public Dictionary<string, string> qryByTypeDic(String cType)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var result1 = (from type in context.SYS_CODE
                           where type.CODE_TYPE == cType
                           orderby type.ISORTBY
                           select new
                           {
                               CCODE = type.CODE.Trim(),
                               CVALUE = type.CODE_VALUE.Trim()
                           }
                           ).ToDictionary(x => x.CCODE, x => x.CVALUE);


            return result1;
        }
    }
}