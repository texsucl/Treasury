//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Treasury.WebViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace Treasury.WebDaos
//{


//    //public class uintItem
//    //{
//    //    public string code { get; set; }
//    //    public string value { get; set; }
//    //    public string levelCode { get; set; }

//    //}

//    public class UnitInfoDao
//    {
//        public UnitInfo qryByUnitCodeSeq(String cUnitCode, String cUnitSeq, bool bWithdrawDate)
//        {
//            sssdbEntities context = new sssdbEntities();

//            UnitInfo unit = context.UnitInfo
//                .Where(x => x.UnitCode == cUnitCode && x.UnitSeq == cUnitSeq
//                && (bWithdrawDate || (x.WithdrawDate == "000000"))).FirstOrDefault();

//            return unit;
//        }

//        public UnitInfoModel qryRegionByUnitcode(String cUnitCode)
//        {
//            //String[] systemArr = new String[] {"1", "2"};
//            int iCnt = 0;
//            UnitInfoModel unitInfoModel = new UnitInfoModel();

//            sssdbEntities context = new sssdbEntities();

//            while (!("5".Equals(StringUtil.toString(unitInfoModel.levelCode))) && iCnt <= 10) {
//                UnitInfo unitInfo = context.UnitInfo.Where(x => x.UnitCode.Trim() + x.UnitSeq.Trim() == cUnitCode
//            & x.WithdrawDate == "000000"
//           // & systemArr.Contains(x.System)
//            ).FirstOrDefault();
//                iCnt++;
//                if (unitInfo != null) {
//                    unitInfoModel.levelCode = unitInfo.Levelcode.Trim();
//                    unitInfoModel.unitCode = unitInfo.UnitCode.Trim() + unitInfo.UnitSeq.Trim();
//                    unitInfoModel.unitName = unitInfo.UnitName.Trim();
//                    cUnitCode = unitInfo.RegionCode.Trim() + unitInfo.RegionSeq.Trim();
//                }
                

//            }
            

//            return unitInfoModel;
//        }


//        public List<UnitInfoModel> qryByRegionCode(String cRegionCode)
//        {

//            //string[] strArray = new string[] { "1", "2" };

//            sssdbEntities context = new sssdbEntities();
//            List<UnitInfo> unitInfoList = context.UnitInfo.Where(x => x.RegionCode.Trim() == cRegionCode
//           & x.WithdrawDate == "000000" 
//           //& strArray.Contains(x.System.Trim())
//           ).ToList();

//            List<UnitInfoModel> unitInfoModelList = new List<UnitInfoModel>();


//            if (unitInfoList != null)
//            {
//                foreach (UnitInfo unit in unitInfoList) {
//                    UnitInfoModel unitInfoModel  = new UnitInfoModel();
//                    unitInfoModel.unitCode = unit.UnitCode.Trim() + unit.UnitSeq.Trim();
//                    unitInfoModel.unitName = unit.UnitName.Trim();

//                    unitInfoModelList.Add(unitInfoModel);
//                }
               
//            }

//            return unitInfoModelList;
//        }


//        /// <summary>
//        /// 依unitCode查詢單位
//        /// </summary>
//        /// <param name="cUnitCode"></param>
//        /// <returns></returns>
//        public UnitInfoModel qryByUnitcode(String cUnitCode) {
            

//            sssdbEntities context = new sssdbEntities();
//            UnitInfo unitInfo =  context.UnitInfo.Where(x => x.UnitCode.Trim() + x.UnitSeq.Trim() == cUnitCode 
//            & x.WithdrawDate == "000000").FirstOrDefault();

//            UnitInfoModel unitInfoModel = new UnitInfoModel();
//            if (unitInfo != null) {
//                unitInfoModel.unitCode = unitInfo.UnitCode.Trim() + unitInfo.UnitSeq.Trim();
//                unitInfoModel.unitName = unitInfo.UnitName.Trim();
//            }
            
//            return unitInfoModel;
//        }


//        /// <summary>
//        /// 依"cRegionCode+cRegionSeq"查詢單位
//        /// </summary>
//        /// <param name="cRegionCode"></param>
//        /// <param name="cRegionSeq"></param>
//        /// <returns></returns>
//        public List<UnitInfo> qryByRegionCodeSeq(String cRegionCode, String cRegionSeq)
//        {

//            //string[] strArray = new string[] { "1", "2" };

//            sssdbEntities context = new sssdbEntities();
//            List<UnitInfo> unitInfoList = context.UnitInfo.Where(
//                x => 
//                x.RegionCode.Trim() == cRegionCode
//                & x.RegionSeq.Trim() == cRegionSeq
//                & x.UnitCode.Trim() + x.UnitSeq.Trim() != cRegionCode + cRegionSeq
//                & x.WithdrawDate.Trim() == "000000" 
//               // & strArray.Contains(x.System.Trim())
//                ).ToList();

//            return unitInfoList;

//        }


//        /// <summary>
//        /// 查詢下轄單位
//        /// </summary>
//        /// <param name="cUnitCode"></param>
//        /// <param name="cUnitSeq"></param>
//        /// <returns></returns>
//        public List<UnitInfo> qryUnderUnit(List<UnitInfo> underUnitList, String cRegionCode, String cRegionSeq)
//        {
//            bool bEnd = false;

//            //List<UnitInfo> underUnitList = new List<UnitInfo>();
//            List<UnitInfo> unitList = new List<UnitInfo>();

//            unitList = qryByRegionCodeSeq(cRegionCode, cRegionSeq);

//            if (unitList != null)
//            {
//                if (unitList.Count > 0)
//                {
//                    foreach (UnitInfo unit in unitList)
//                    {
//                        underUnitList.Add(unit);
//                        underUnitList = qryUnderUnit(underUnitList, unit.UnitCode, unit.UnitSeq);
//                    }
//                }
//                else
//                    bEnd = true;
//            }
//            else
//                bEnd = true;

            
//            return underUnitList;

//        }




//        /// <summary>
//        /// 取得"區部"下拉選單
//        /// </summary>
//        /// <param name="cType"></param>
//        /// <param name="cUnit"></param>
//        /// <returns></returns>
//        public SelectList loadSelectList(String cType, String cUnit)
//        {
//            List<SelectListItem> itemList = new List<SelectListItem>();

//            sssdbEntities context = new sssdbEntities();

//            string[] strArray = new string[] { "1", "2" };

//            List<UnitInfoModel> unitList = new List<UnitInfoModel>();


//            switch (cType) {
//                case "0":   //無
//                    break;
//                case "1":   //全公司
//                    unitList = (from unit in context.UnitInfo
//                                where unit.Levelcode == "5"
//                                 & unit.WithdrawDate == "000000"
//                                 & strArray.Contains(unit.System)

//                                select new UnitInfoModel
//                                {
//                                    unitCode = unit.UnitCode.Trim() + unit.UnitSeq.Trim(),
//                                    unitName = unit.UnitName.Trim()
//                                }
//                           ).Distinct().ToList();
//                    break;
//                default:    //所屬及下轄單位、所屬單位
//                    bool bFind = true;

//                    while (bFind) {
//                        unitList = (from unit in context.UnitInfo
//                                    where unit.WithdrawDate == "000000"
//                                     & strArray.Contains(unit.System)
//                                     & unit.UnitCode.Trim() + unit.UnitSeq.Trim() == cUnit
//                                    select new UnitInfoModel
//                                    {
//                                        unitCode = unit.UnitCode.Trim() + unit.UnitSeq.Trim(),
//                                        unitName = unit.UnitName.Trim(),
//                                        levelCode = unit.Levelcode.Trim(),
//                                        regionCode = unit.RegionCode.Trim(),
//                                        regionSeq = unit.UnitSeq.Trim()
//                                    }
//                           ).Distinct().ToList();


//                        if (unitList != null)
//                        {
//                            if (unitList.Count > 0)
//                            {
//                                foreach (UnitInfoModel item in unitList)
//                                {
//                                    if ("5".Equals(item.levelCode))
//                                        bFind = false;
//                                    else
//                                    {
//                                        cUnit = item.regionCode + item.regionSeq;
//                                    }
//                                }
//                            }
//                            else 
//                                bFind = false;
//                        }
//                        else
//                            bFind = false;
//                    }
                    
//                    break;
//            }
            

//            foreach (UnitInfoModel item in unitList) {
//                item.unitName = item.unitName.Replace("　", "");
//            }

//            if (!"1".Equals(cType))
//            {
//                var items = new SelectList
//                (
//                items: unitList,
//                dataValueField: "unitCode",
//                dataTextField: "unitName",
//                selectedValue: (object)null
//                );

//                return items;
//            }
//            else {
//                var items = new SelectList
//                (
//                items: unitList,
//                dataValueField: "unitCode",
//                dataTextField: "unitName",
//                selectedValue: unitList.First()
//                );

//                return items;
//            }

//        }
//    }

//}