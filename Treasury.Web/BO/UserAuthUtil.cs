using Treasury.WebDaos;
using Treasury.WebModels;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Treasury.WebBO
{


    public class UserAuthUtil
    {

        //public CODEUSER qryUserUnit(CODEUSER codeUser, string cUserType, string cAgentID)
        //{
            
        //    UnitInfo unit = new UnitInfo();

        //    if (!"1".Equals(cUserType.ToLower()))
        //    {
        //        AgentInfoDao agentInfoDao = new AgentInfoDao();

        //        AgentInfo agentInfo = agentInfoDao.qryByAgentId(cAgentID);
        //        UnitInfoDao unitInfoDao = new UnitInfoDao();

        //        if (agentInfo == null)
        //            return null;

        //        codeUser.CWORKUNITCODE = agentInfo.WorkUnitCode.Trim();
        //        codeUser.CWORKUNITSEQ = agentInfo.WorkUnitSeq.Trim();
        //        unit = unitInfoDao.qryByUnitCodeSeq(codeUser.CWORKUNITCODE, codeUser.CWORKUNITSEQ, false);
        //        if (unit == null)
        //            codeUser.CWORKUNITNAME = "";
        //        else
        //            codeUser.CWORKUNITNAME = StringUtil.toString(unit.UnitName);


        //        codeUser.CBELONGUNITCODE = agentInfo.UnitCode.Trim();
        //        codeUser.CBELONGUNITSEQ = agentInfo.UnitSeq.Trim();
        //        unit = unitInfoDao.qryByUnitCodeSeq(codeUser.CBELONGUNITCODE, codeUser.CBELONGUNITSEQ, false);
        //        if (unit == null)
        //            codeUser.CBELONGUNITNAME = "";
        //        else
        //            codeUser.CBELONGUNITNAME = StringUtil.toString(unit.UnitName);


        //    }
        //    else
        //    {
        //        codeUser.CWORKUNITCODE = "";
        //        codeUser.CWORKUNITSEQ = "";
        //        codeUser.CWORKUNITNAME = "";

        //        codeUser.CBELONGUNITCODE = "";
        //        codeUser.CBELONGUNITSEQ = "";
        //        codeUser.CBELONGUNITNAME = "";
        //    }

        //    return codeUser;
        //}


        /// <summary>
        /// 查詢使用者使用特定功能的權限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="funcId"></param>
        /// <returns></returns>
        public String[] chkUserFuncAuth(String userId, String funcId) {
            UserAuthDao userAuthDao = new UserAuthDao();


            return userAuthDao.qryOpScope(userId, funcId);
        }



        ///// <summary>
        ///// 查使用者於特定角色權限時，可作業/查詢的單位
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="userUnit"></param>
        ///// <param name="opScope"></param>
        ///// <param name="type"></param>
        ///// <param name="roleId"></param>
        ///// <returns></returns>
        //public SelectList qryUserOpUnit(String userId, String userUnit, String opScope, String type, String roleId) {
        //    List<UnitInfoModel> unitList = new List<UnitInfoModel>();



        //    ////使用者所屬單位
        //    //UnitInfoDao unitInfoDao = new UnitInfoDao();
        //    //UnitInfoModel unitInfoModel = unitInfoDao.qryByUnitcode(userUnit);
        //    //unitList.Add(unitInfoModel);

        //    //使用者指派單位
        //    CodeUserMaintainUnitDao codeUserMaintainUnitDao = new CodeUserMaintainUnitDao();
        //    unitList = codeUserMaintainUnitDao.qryByUser(unitList, userId);


        //    //使用者可作業/查詢單位
        //    if ("FT001".Equals(type))
        //    {
        //        CodeUserOprUnitMappingDao unitDao = new CodeUserOprUnitMappingDao();
        //        List<CODEUSEROPRUNITMAPPING> unitOptList = unitDao.qryByIdUnit(userId, userUnit, roleId);


        //        foreach (CODEUSEROPRUNITMAPPING item in unitOptList)
        //        {
        //            item.CUNITNAME = item.CUNITNAME.Replace("　", "").Trim();

        //            UnitInfoModel unitInfoModel = new UnitInfoModel();
        //            unitInfoModel.unitCode = item.CUNITCODE.Trim() + item.CUNITSEQ.Trim();
        //            unitInfoModel.unitName = item.CUNITNAME.Trim();

        //            unitList.Add(unitInfoModel);

        //        }


                

        //    }
        //    else {
        //        CodeUserSrchUnitMappingDao unitDao = new CodeUserSrchUnitMappingDao();
        //        List<CODEUSERSRCHUNITMAPPING> unitSrchList = unitDao.qryByIdUnit(userId, userUnit, roleId);


        //        foreach (CODEUSERSRCHUNITMAPPING item in unitSrchList)
        //        {
        //            item.CUNITNAME = item.CUNITNAME.Replace("　", "").Trim();

        //            UnitInfoModel unitInfoModel = new UnitInfoModel();
        //            unitInfoModel.unitCode = item.CUNITCODE.Trim() + item.CUNITSEQ.Trim();
        //            unitInfoModel.unitName = item.CUNITNAME.Trim();

        //            unitList.Add(unitInfoModel);

        //        }
        //    }


        //    var items = new SelectList
        //            (
        //            items: unitList,
        //            dataValueField: "unitCode",
        //            dataTextField: "unitName",
        //            selectedValue: (object)null
        //            );

        //    return items;


        //}

        
    }
}