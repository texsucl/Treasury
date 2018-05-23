using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Models;

namespace Treasury.Web.Daos
{
    public class TreaEquipDao
    {

        /// <summary>
        /// 取得金庫設備下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList()
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            //var result = context.TypeDefine.Where(x => x.CTYPE == cType).OrderBy(x => x.ISORTBY);

            var result1 = (from equip in context.TREA_EQUIP
                           where equip.IS_DISABLED == "N"
                           orderby equip.TREA_EQUIP_ID
                           select new
                           {
                               CCODE = equip.TREA_EQUIP_ID.Trim(),
                               CVALUE = equip.EQUIP_NAME.Trim()
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
        /// 以鍵項查詢金庫設備
        /// </summary>
        /// <param name="equip"></param>
        /// <returns></returns>
        public TREA_EQUIP qryByKey(String equipId)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var result = context.TREA_EQUIP.Where(x => x.TREA_EQUIP_ID == equipId).FirstOrDefault();


            return result;
        }



        public string jqgridSelect()
        {
            string equipStr = "";
            var equipList = loadSelectList();
            foreach (var item in equipList)
            {
                equipStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            equipStr = equipStr.Substring(0, equipStr.Length - 1) + "";
            return equipStr;

        
        }
    }
}