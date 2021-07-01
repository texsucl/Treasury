using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class FastErrModel
    {
        public string fastNo { get; set; }

        public string errorCode { get; set; }

        public string errorMsg { get; set; }

        public string failCode { get; set; }

        public string execType { get; set; }

        public string textType { get; set; }

        public string emsgTxt { get; set; }

        public FastErrModel()
        {
            fastNo = "";
            errorCode = "";
            errorMsg = "";
            failCode = "";
            execType = "";
            textType = "";
            emsgTxt = "";
        }
    }
}