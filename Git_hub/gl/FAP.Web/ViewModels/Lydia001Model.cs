using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


/// <summary>
/// 參考網址 https://km.fubonlife.com.tw/confluence/pages/viewpage.action?pageId=355045642
/// </summary>
/// 
namespace FAP.Web.ViewModels
{
    public class Lydia001Model
    {

        [Display(Name = "id")]
        public string id { get; set; }

        [Display(Name = "結果代碼")]
        public string returnCode { get; set; }

        [Display(Name = "結果訊息")]
        public string returnMessage { get; set; }

        [Display(Name = "業務人員ID")]
        public string agentId { get; set; }

        [Display(Name = "姓名")]
        public string agentName { get; set; }

        [Display(Name = "代別")]
        public string generation { get; set; }

        [Display(Name = "職稱")]
        public string agentTitle { get; set; }

        [Display(Name = "職等")]
        public string agentRank { get; set; }

        [Display(Name = "現職狀態代碼")]
        public string servingStatus { get; set; }

        [Display(Name = "在職狀態說明")]
        public string servingStatusTxt { get; set; }

        [Display(Name = "到職日")]
        public string arriveDate { get; set; }

        [Display(Name = "出生年月日")]
        public string birthDate { get; set; }

        [Display(Name = "戶籍地址")]
        public string domiciliaryAddress { get; set; }

        [Display(Name = "通訊郵遞區號")]
        public string mailingZipCode { get; set; }

        [Display(Name = "通訊地址")]
        public string mailingAddress { get; set; }

        [Display(Name = "辦公電話")]
        public string officeTel { get; set; }

        [Display(Name = "住家電話")]
        public string homeTel { get; set; }

        [Display(Name = "行動電話")]
        public string mobileNo { get; set; }

        [Display(Name = "單位代號")]
        public string unitCode { get; set; }

        [Display(Name = "單位序號")]
        public string unitSeq { get; set; }

        [Display(Name = "上班單位代碼")]
        public string workUnitCode { get; set; }

        [Display(Name = "上班單位序號")]
        public string workUnitSeq { get; set; }

        [Display(Name = "主管業務員代號")]
        public string supervisorAgentId { get; set; }

        [Display(Name = "增員者業務員代號")]
        public string recruitAgentId { get; set; }

        [Display(Name = "實際離職日")]
        public string leaveDate { get; set; }

        [Display(Name = "電子信箱")]
        public string email { get; set; }

        [Display(Name = "發展區單位序號")]
        public string devUnitSeq { get; set; }

        [Display(Name = "並列羅馬拼音姓名")]
        public string fullName { get; set; }

        [Display(Name = "中文姓名")]
        public string chineseName { get; set; }

        [Display(Name = "羅馬拼音姓名")]
        public string romanName { get; set; }


        public Lydia001Model() {
            id = "";
            returnCode = "";
            returnMessage = "";
            agentId = "";
            agentName = "";
            generation = "";
            agentTitle = "";
            agentRank = "";
            servingStatus = "";
            servingStatusTxt = "";
            arriveDate = "";
            birthDate = "";
            domiciliaryAddress = "";
            mailingZipCode = "";
            mailingAddress = "";
            officeTel = "";
            homeTel = "";
            mobileNo = "";
            unitCode = "";
            unitSeq = "";
            workUnitCode = "";
            workUnitSeq = "";
            supervisorAgentId = "";
            recruitAgentId = "";
            leaveDate = "";
            email = "";
            devUnitSeq = "";
            fullName = "";
            chineseName = "";
            romanName = "";
        }
    }
}