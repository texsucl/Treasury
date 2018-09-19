using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;

namespace Treasury.Web.Service.Actual
{

    public class CDC : ICDC
    {
        #region Get Date
        public CDCViewModel GetItemId()
        {
            var result = new CDCViewModel();
            List<SelectOption> jobProject = new List <SelectOption>(); //作業項目
            List<SelectOption> treasuryIO = new List <SelectOption>(); //金庫內外
            List<SelectOption> dMargin_Take_Of_Type = new List<SelectOption>(); //存入保證金類別
            List<SelectOption> dMarging_Dep_Type = new List<SelectOption>(); //存出保證金類別
            List<SelectOption> Estate_Form_No = new List<SelectOption>(); //不動產權狀狀別
                       
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var bill = Ref.TreaItemType.D1012.ToString(); // 空白票據項目 用於條件判斷
                jobProject = db.TREA_ITEM.AsNoTracking() // 抓資料表的所有資料
                    .Where(x => x.ITEM_OP_TYPE == "3" && x.IS_DISABLED == "N" && x.ITEM_ID !=bill) //條件
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value =x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();

                var sysCode = db.SYS_CODE.AsNoTracking().ToList();

                var all = new SelectOption() { Text = "All", Value = "All" };

                treasuryIO = sysCode
                    .Where(x => x.CODE_TYPE == "YN_FLAG")
                    .OrderBy(x=>x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                    Value=x.CODE,
                    Text = x.CODE,
                    }).ToList();

                dMargin_Take_Of_Type = sysCode
                    .Where(x => x.CODE_TYPE == "MARGIN_TAKE_OF_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE,
                    }).ToList();
                dMargin_Take_Of_Type.Insert(0, all);

                dMarging_Dep_Type = sysCode
                   .Where(x => x.CODE_TYPE == "MARGING_TYPE")
                   .OrderBy(x => x.ISORTBY)
                   .AsEnumerable().Select(x => new SelectOption()
                   {
                       Value = x.CODE,
                       Text = x.CODE_VALUE,
                   }).ToList(); 
                dMarging_Dep_Type.Insert(0, all);

                Estate_Form_No = sysCode
                 .Where(x => x.CODE_TYPE == "ESTATE_TYPE")
                 .OrderBy(x => x.ISORTBY)
                 .AsEnumerable().Select(x => new SelectOption()
                 {
                     Value = x.CODE,
                     Text = x.CODE_VALUE,
                 }).ToList();
                Estate_Form_No.Insert(0, all);
            }

            result.vTreasuryIO = treasuryIO;
            result.vJobProject = jobProject;
            result.vEstate_From_No = Estate_Form_No;
            result.vMarging = dMarging_Dep_Type;
            result.vMarginp = dMargin_Take_Of_Type;
            result.vBook_No = new Estate().GetBuildName();
            result.vName = new Stock().GetStockName();
            result.vTRAD_Partners = new Deposit().GetTRAD_Partners();

            return result;
        }
        #endregion

        #region Save Data

        #endregion

        #region private function
          
        #endregion
    }
}