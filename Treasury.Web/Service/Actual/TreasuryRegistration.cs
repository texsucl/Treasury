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

    public class TreasuryRegistration : ITreasuryRegistration
    {
        #region Get Date
        public TreasuryRegistrationViewModel GetItemId()
        {
            var result = new TreasuryRegistrationViewModel();
            List<SelectOption> ActualPutTime = new List<SelectOption>(); //入庫時間
            List<SelectOption> OpenTreaType = new List <SelectOption>(); //開庫模式
                       
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //var bill = Ref.TreaItemType.D1012.ToString(); // 空白票據項目 用於條件判斷
                //OpenTreaType = db.TREA_ITEM.AsNoTracking() // 抓資料表的所有資料
                //    .Where(x => x.ITEM_OP_TYPE == "3" && x.IS_DISABLED == "N" && x.ITEM_ID !=bill) //條件
                //    .AsEnumerable().Select(x => new SelectOption()
                //    {
                //        Value =x.ITEM_ID,
                //        Text = x.ITEM_DESC
                //    }).ToList();

                var sysCode = db.SYS_CODE.AsNoTracking().ToList();

                var all = new SelectOption() { Text = "All", Value = "All" };

                ActualPutTime = sysCode
                    .Where(x => x.CODE_TYPE == "ACTUAL_PUT_TIME")
                    .OrderBy(x=>x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                    Value=x.CODE,
                    Text = x.CODE,
                    }).ToList();

                OpenTreaType = sysCode
                    .Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE,
                    }).ToList();
                //dMargin_Dep_Type.Insert(0, all);

                //tMargin_Dep_Type = sysCode
                //   .Where(x => x.CODE_TYPE == "MARGING_TYPE")
                //   .OrderBy(x => x.ISORTBY)
                //   .AsEnumerable().Select(x => new SelectOption()
                //   {
                //       Value = x.CODE,
                //       Text = x.CODE_VALUE,
                //   }).ToList(); 
                //tMargin_Dep_Type.Insert(0, all);

                //Estate_Form_No = sysCode
                // .Where(x => x.CODE_TYPE == "ESTATE_TYPE")
                // .OrderBy(x => x.ISORTBY)
                // .AsEnumerable().Select(x => new SelectOption()
                // {
                //     Value = x.CODE,
                //     Text = x.CODE_VALUE,
                // }).ToList();
                //Estate_Form_No.Insert(0, all);
            }

            result.vActualPutTime = ActualPutTime;
            result.vOpenTreaType = OpenTreaType;
            //result.vEstate_From_No = Estate_Form_No;
            //result.vMarging = tMargin_Dep_Type;
            //result.vMarginp = dMargin_Dep_Type;
            //result.vBook_No = new Estate().GetBuildName();
            //result.vName = new Stock().GetStockName();
            //result.vTRAD_Partners = new Deposit().GetTRAD_Partners();

            return result;
        }
        #endregion

        #region Save Data

        #endregion

        #region private function
          
        #endregion
    }
}