using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTDIRLModel
    {
        //類別
        public string type { get; set; }

        //異動人員
        public string updId { get; set; }

        //登錄人員
        public string entryId { get; set; }

        //部門代號
        public string entryUnit { get; set; }

        //異動日期 
        public string updDate { get; set; }

        //異動時間 
        public string updTime { get; set; }

        //來源程式  
        public string srcepgm { get; set; }

        //來源程式1 
        public string srcepgm1 { get; set; }

        //系統  
        public string system { get; set; }

        //來源編號  
        public string resource { get; set; }

        //退費類別 
        public string rtnfeesrc { get; set; }

        //保單號碼 
        public string policyNo { get; set; }

        //保單序號 
        public string policySeq { get; set; }

        //身分證重複別 
        public string idDup { get; set; }

        //案號  
        public string changeId { get; set; }

        //人員別 
        public string memberId { get; set; }

        //受款人 ID  
        public string paidId { get; set; }

        //原中文戶名  
        public string payment { get; set; }

        //英文戶名  
        public string epayment { get; set; }

        //狀態 
        public string errsts { get; set; }

        //無英譯中文字 
        public string chword { get; set; }

        //音譯  
        public string enword { get; set; }

        //區部別 
        public string area { get; set; }

        //資料來源  
        public string srceFrom { get; set; }

        //資料類別 
        public string srceKind { get; set; }

        //給付編號 
        public string payNo { get; set; }

        //給付序號  
        public string paySeq { get; set; }

        //憑証編號 
        public string vhrNo1 { get; set; }

        //處理序號 
        public string proNo { get; set; }

        //帳本 
        public string corpNo { get; set; }

        //幣別 
        public string currency { get; set; }

        //銀行碼 
        public string bankNo { get; set; }

        //銀行帳號  
        public string bankAct { get; set; }

        //轉送碼 
        public string swiftcode { get; set; }

        //回傳碼  
        public string payfRtncd { get; set; }

        //檢核項目ＩＤ 
        public string payfCode { get; set; }

        //檢核項目錯誤碼  
        public string payfErr { get; set; }

        //是否可強制輸入  
        public string payfSwck { get; set; }

        //錯誤備註說明 
        public string payfErrtx { get; set; }

        //保留欄位１  
        public string filler1 { get; set; }

        //保留欄位2  
        public string filler2 { get; set; }

        //保留欄位3 
        public string filler3 { get; set; }

        //保留欄位4  
        public string filler4 { get; set; }

        //保留欄位5  
        public string filler5 { get; set; }

        //保留欄位6  
        public string filler6 { get; set; }



        public FRTDIRLModel()
        {
            type = "";
            updId = "";
            entryId = "";
            entryUnit = "";
            updDate = "";
            updTime = "";
            srcepgm = "";
            srcepgm1 = "";
            system = "";
            resource = "";
            rtnfeesrc = "";
            policyNo = "";
            policySeq = "";
            idDup = "";
            changeId = "";
            memberId = "";
            paidId = "";
            payment = "";
            epayment = "";
            errsts = "";
            chword = "";
            enword = "";
            area = "";
            srceFrom = "";
            srceKind = "";
            payNo = "";
            paySeq = "";
            vhrNo1 = "";
            proNo = "";
            corpNo = "";
            currency = "";
            bankNo = "";
            bankAct = "";
            swiftcode = "";
            payfRtncd = "";
            payfCode = "";
            payfErr = "";
            payfSwck = "";
            payfErrtx = "";
            filler1 = "";
            filler2 = "";
            filler3 = "";
            filler4 = "";
            filler5 = "";
            filler6 = "";
        }
    }
}