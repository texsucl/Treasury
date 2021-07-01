using FGL.Web.AS400PGM;
using FGL.Web.Daos;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

/// -----------------------------------------------
/// 修改歷程：20191202 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：組合會計枓目中文名稱時，改用"商品簡稱"。
/// 

namespace FGL.Web.BO
{
    public class NewAcctItemUtil
    {
        /// <summary>
        /// //查詢"商品科目設定檔"
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <returns></returns>
        public OGL00001Model qryItemAcct(string productType, string fuMk, string itemCon, string discPartFeat)
        {
            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            List<OGL00001Model> itemAcctList = fGLItemAcctDao.qryHeadForItem(productType, fuMk, itemCon, discPartFeat);

            OGL00001Model itemAcct = new OGL00001Model();

            if (itemAcctList.Count > 0) {
                itemAcct = itemAcctList[0];

                switch (itemAcct.productType) {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                        itemAcct.healthMgrMk = "Y";
                        break;
                    default:
                        break;
                }
            }

            return itemAcct;

        }



        /// <summary>
        /// 取得上游的險種中文名稱
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string qryItemName(string item)
        {
            string itemName = "";

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                //商品檔(個險-F系統)
                FNBPOLNFDao fNBPOLNFDao = new FNBPOLNFDao();
                itemName = fNBPOLNFDao.qryItemName(item, conn400);

                if ("".Equals(StringUtil.toString(itemName)))
                {
                    //商品檔(個險-A系統)
                    FNBPOLNADao fNBPOLNADao = new FNBPOLNADao();
                    itemName = fNBPOLNADao.qryItemName(item, conn400);

                }

                if ("".Equals(StringUtil.toString(itemName)))
                {
                    //團險-TA
                    STAITEMUtil sTAITEMUtil = new STAITEMUtil();
                    itemName = sTAITEMUtil.callSTAITEMUtil(item, conn400);
                }

                if ("".Equals(StringUtil.toString(itemName)))
                {
                    //團險-大小團險
                    SGPFA002Util sGPFA002Util = new SGPFA002Util();
                    itemName = sGPFA002Util.callSGPFA002Util(item, conn400);
                }

                if ("".Equals(StringUtil.toString(itemName)))
                {
                    //團險-MG
                    SPMFAS002Util sPMFAS002Util = new SPMFAS002Util();
                    itemName = sPMFAS002Util.callSPMFAS002Util(item, conn400);
                }
            }

            return itemName;

        }


        public Dictionary<string, OGL00003DModel> getSmpList(string type, OGL00003Model model)
        {
            Dictionary<string, OGL00003DModel> smpMap = new Dictionary<string, OGL00003DModel>();

            //取得"商品科目設定"中的科目
            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            List<OGL00003DModel> flgItemList = fGLItemAcctDao.qryForOGL00005(
                model.productType, model.fuMk, model.itemCon, model.discPartFeat,
                model.comType, model.extSchedulType, model.lodprmType, model.pakindDmopType, model.coiType);

            foreach (OGL00003DModel d in flgItemList)
            {
                if (!"".Equals(StringUtil.toString(model.effectDate))) {
                    if ("P".Equals(type))
                        d.effectDate = (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString() + "/" + model.effectMM + "/" + model.effectDD;
                    else
                        d.effectDate = model.effectYY + "/" + model.effectMM + "/" + model.effectDD;
                }
               

                d.item = model.item;
                d.smpNumFrom = "itemAcct";

                //險種科目
                if (!"".Equals(model.itemAcct) &  "1".Equals(d.acctType.Substring(1, 1)))
                {
                    OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
                    smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.itemAcct, itemSmp);
                }



                //分離科目
                if (!"".Equals(model.separatAcct) & "2".Equals(d.acctType.Substring(1, 1)))
                {
                    OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
                    smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.separatAcct, itemSmp);
                }


                //COI科目
                if (!"".Equals(model.coiAcct) & "3".Equals(d.acctType.Substring(1, 1)))
                {
                    OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
                    smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.coiAcct, itemSmp);
                }

            }

            //取得"給付項目/費用"、"保單投資"中的科目
            List<OGL00003DModel> itemSmpList = new List<OGL00003DModel>();
            if (!"".Equals(model.aplyNo))
            {
                FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                itemSmpList = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "");
            }
            else
            {
                FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                itemSmpList = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "");
            }

            FGLSMPADao FGLSMPADao = new FGLSMPADao();

            foreach (OGL00003DModel d in itemSmpList)
            {
                if ("Y".Equals(d.flag))
                {
                    d.item = model.item;

                    if (!"".Equals(StringUtil.toString(model.effectDate)))
                    {
                        if ("P".Equals(type))
                            d.effectDate = (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString() + "/" + model.effectMM + "/" + model.effectDD;
                        else
                            d.effectDate = model.effectYY + "/" + model.effectMM + "/" + model.effectDD;
                    }


                    d.smpNumFrom = "itemSmp";

                    if ("Y".Equals(model.fuMk))
                        d.corpNo = d.acctType.Substring(1, 1) == "2" ? "7" : "3";
                    else
                        d.corpNo = d.acctType.Substring(1, 1) == "2" ? "9" : "1";

                    if ("P".Equals(type))
                        FGLSMPADao.qryForOGL00005(d);

                    if (!"".Equals(model.itemAcct) & "1".Equals(d.acctType.Substring(1, 1))) {
                        //險種科目
                        OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
                        smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.itemAcct, itemSmp);
                    }

                    if (!"".Equals(model.separatAcct) & "2".Equals(d.acctType.Substring(1, 1))) {
                        //分離科目
                        OGL00003DModel separatSmp = (OGL00003DModel)d.Clone();
                        smpMap = insertSmpChkMap(smpMap, separatSmp.smpNum + model.separatAcct, separatSmp);
                    }

                    if (!"".Equals(model.coiAcct) & "3".Equals(d.acctType.Substring(1, 1))) {
                        //COI科目
                        OGL00003DModel coiSmp = (OGL00003DModel)d.Clone();
                        smpMap = insertSmpChkMap(smpMap, coiSmp.smpNum + model.coiAcct, coiSmp);
                    }
                        
                }

            }



            //若為列印報表時，要另組報表資訊
            if ("P".Equals(type))
            {
                //回AS400取得AS400科目中文及簡稱(LGLSMPL1)
                FGLSMPLDao fGLSMPLDao = new FGLSMPLDao();
                smpMap = fGLSMPLDao.qryForOGL00005(smpMap);

                //至"FGL_SMPB 科目樣本險種類別檔"取得科目名稱
                FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
                smpMap = fGLSMPBDao.qryForOGL00005(smpMap, model.itemNameShrt); //modify by daiyu 20191202 組合會計枓目中文名稱時，改用"商品簡稱"
            }

            return smpMap;
        }


        private Dictionary<string, OGL00003DModel> insertSmpChkMap(Dictionary<string, OGL00003DModel> smpMap, string acctNum, OGL00003DModel d)
        {
            string key = acctNum + "|" + d.corpNo;
            if (!smpMap.ContainsKey(key))
            {
                d.acctNum = acctNum;
                smpMap.Add(key, d);
            }


            return smpMap;
        }
    }
}