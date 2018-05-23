using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Models;
using Treasury.WebUtils;

namespace Treasury.Web.Daos
{
    public class TreaItemDao
    {

        /// <summary>
        /// 取得金庫設備下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string opType)
        {
            bool bopType = StringUtil.isEmpty(opType);

            dbTreasuryEntities context = new dbTreasuryEntities();

            //var result = context.TypeDefine.Where(x => x.CTYPE == cType).OrderBy(x => x.ISORTBY);

            var result1 = (from equip in context.TREA_ITEM
                           where equip.IS_DISABLED == "N"
                           & (bopType || (equip.ITEM_OP_TYPE == opType.Trim()))
                           orderby equip.ITEM_ID
                           select new
                           {
                               CCODE = equip.ITEM_ID.Trim(),
                               CVALUE = equip.ITEM_DESC.Trim()
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


        public string jqgridSelect(string opType)
        {
            string itemStr = "";
            var itemList = loadSelectList(opType);
            foreach (var item in itemList)
            {
                itemStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            itemStr = itemStr.Substring(0, itemStr.Length - 1) + "";

            return itemStr;
        }

    }
}