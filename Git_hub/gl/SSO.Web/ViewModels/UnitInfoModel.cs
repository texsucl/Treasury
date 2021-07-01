using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSO.WebViewModels
{
    public class UnitInfoModel
    {
        public string unitCode { get; set; }
        public string unitName { get; set; }
        public string levelCode { get; set; }
        public string regionCode { get; set; }
        public string regionSeq { get; set; }
        

        //public UnitInfoModel(String unitCode, String cunitName) {
        //    this.unitCode = unitCode;
        //    this.unitName = unitName;
        //    this.levelCode = "";
        //}
    }
}