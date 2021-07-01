using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class ASLSXFKModel
    {
        public string portn { get; set; }

        public P4PARM p4parm { get; set; }
        


        public string wp0001 { get; set; }

        public string wp0002 { get; set; }

        public string wp0003 { get; set; }


        public ASLSXFKModel()
        {
            portn = "";
            p4parm = new P4PARM();
            wp0001 = "";
            wp0002 = "";
            wp0003 = "";
            

        }

        public class P4PARM
        {
            /// <summary>
            ///  hr  員工編號 
            /// </summary>
            public string p4a9wf { get; set; }

            /// <summary>
            /// hr  員工身份證代碼 
            /// </summary>
            public string p4a8wf { get; set; }

            /// <summary>
            /// hr  員工網路帳號  
            /// </summary>
            public string p4bawf { get; set; }

            /// <summary>
            /// hr  員工姓名 
            /// </summary>
            public string p4k5ig { get; set; }


            /// <summary>
            ///  hr  身份 
            /// </summary>
            public string p4bbwf { get; set; }


            /// <summary>
            ///  hr  聘任關係
            /// </summary>
            public string p4bcwf { get; set; }


            /// <summary>
            /// hr  員工出生年月日
            /// </summary>
            public string p4bdwf { get; set; }


            /// <summary>
            /// hr  員工到職日 
            /// </summary>
            public string p4bewf { get; set; }


            /// <summary>
            /// hr  員工離職日 
            /// </summary>
            public string p4bfwf { get; set; }


            /// <summary>
            ///  hr  員工職稱 
            /// </summary>
            public string p4k6ig { get; set; }


            /// <summary>
            /// hr  現職狀態  
            /// </summary>
            public string p4bgwf { get; set; }


            /// <summary>
            /// hr  員工學歷 
            /// </summary>
            public string p4k7ig { get; set; }


            /// <summary>
            ///  hr  薪資轉帳銀行 
            /// </summary>
            public string p4bhwf { get; set; }


            /// <summary>
            /// hr  薪資轉帳銀行帳號 
            /// </summary>
            public string p4biwf { get; set; }

            /// <summary>
            /// hr  員工戶籍地址 
            /// </summary>
            public string p4k8ig { get; set; }

            /// <summary>
            ///  hr  員工通訊地址 
            /// </summary>
            public string p4k9ig { get; set; }


            /// <summary>
            ///  hr  員工戶籍電話 
            /// </summary>
            public string p4bjwf { get; set; }

            /// <summary>
            /// hr  員工通訊電話 
            /// </summary>
            public string p4bkwf { get; set; }

            /// <summary>
            ///  hr  員工分機號碼 
            /// </summary>
            public string p4blwf { get; set; }

            /// <summary>
            /// hr  員工其他傳真 
            /// </summary>
            public string p4bmwf { get; set; }

            /// <summary>
            /// hr  員工專線電話 
            /// </summary>
            public string p4bnwf { get; set; }

            /// <summary>
            /// hr  員工  e-mail
            /// </summary>
            public string p4buwf { get; set; }

            /// <summary>
            /// hr  單位代號 
            /// </summary>
            public string p4a3wf { get; set; }

            /// <summary>
            /// hr  上班單位代碼 
            /// </summary>
            public string p4bpwf { get; set; }

            /// <summary>
            /// hr  員工上班地址 
            /// </summary>
            public string p4laig { get; set; }

            /// <summary>
            /// hr  人員類別 
            /// </summary>
            public string p4bqwf { get; set; }

            /// <summary>
            /// hr  員工兼任單位 
            /// </summary>
            public string p4brwf { get; set; }

            /// <summary>
            /// hr  直屬主管身分證字號
            /// </summary>
            public string p4bswf { get; set; }

            /// <summary>
            /// hr  直屬主管員工編號 
            /// </summary>
            public string p4btwf { get; set; }

            /// <summary>
            /// hr  異動日期
            /// </summary>
            public string p4a6wf { get; set; }

            /// <summary>
            /// hr  異動人員 
            /// </summary>
            public string p4a7wf { get; set; }



            public P4PARM()
            {

                p4a9wf = "";
                p4a8wf = "";
                p4bawf = "";
                p4k5ig = "";
                p4bbwf = "";
                p4bcwf = "";
                p4bdwf = "";
                p4bewf = "";
                p4bfwf = "";
                p4k6ig = "";
                p4bgwf = "";
                p4k7ig = "";
                p4bhwf = "";
                p4biwf = "";
                p4k8ig = "";
                p4k9ig = "";
                p4bjwf = "";
                p4bkwf = "";
                p4blwf = "";
                p4bmwf = "";
                p4bnwf = "";
                p4buwf = "";
                p4a3wf = "";
                p4bpwf = "";
                p4laig = "";
                p4bqwf = "";
                p4brwf = "";
                p4bswf = "";
                p4btwf = "";
                p4a6wf = "";
                p4a7wf = "";


            }
        }


    }
}