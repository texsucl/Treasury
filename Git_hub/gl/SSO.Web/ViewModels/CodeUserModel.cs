using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Web.ViewModels
{
    [Table("CodeUser")]
    public class CodeUserModel
    {
        [Key]
        public String cUserID { get; set; }

        [Required]
        [MaxLength(10)]
        public string cAgentID { get; set; }


        [MaxLength(20)]
        public string cUserName { get; set; }


        [MaxLength(1)]
        public string cUserType { get; set; }


        [MaxLength(1)]
        public string cFlag { get; set; }


        [MaxLength(250)]
        public string vMemo { get; set; }


        [MaxLength(5)]
        public string cWorkUnitCode { get; set; }


        [MaxLength(4)]
        public string cWorkUnitSeq { get; set; }


        [MaxLength(21)]
        public string cWorkUnitName { get; set; }


        [MaxLength(5)]
        public string cBelongUnitCode { get; set; }


        [MaxLength(4)]
        public string cBelongUnitSeq { get; set; }


        [MaxLength(21)]
        public string cBelongUnitName { get; set; }


        [MaxLength(10)]
        public string cCrtUserID { get; set; }


        [MaxLength(20)]
        public string cCrtUserName { get; set; }


        [MaxLength(8)]
        public string cCrtDate { get; set; }


        [MaxLength(6)]
        public string cCrtTime { get; set; }


        [MaxLength(10)]
        public string cUpdUserID { get; set; }


        [MaxLength(20)]

        public string cUpdUserName { get; set; }


        [MaxLength(8)]

        public string cUpdDate { get; set; }


        [MaxLength(6)]
        public string cUpdTime { get; set; }


        public DateTime? cLoginDateTime { get; set; }


        public DateTime? cLogOutDateTime { get; set; }

    }
}
