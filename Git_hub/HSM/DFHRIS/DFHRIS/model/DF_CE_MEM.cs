using DFHRIS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFHRIS.model
{
    class DF_CE_MEM : IModel
    {

        [Required]
        [StringLength(1, ErrorMessage = "SYS_TYPE,不得大於1個字")]
        public string SYS_TYPE { get; set; }

        [Required]
        [StringLength(1, ErrorMessage = "IS_MANA,不得大於1個字")]
        public string IS_MANA { get; set; }

        [Required]
        [StringLength(1, ErrorMessage = "IS_MJOB,不得大於1個字")]
        public string IS_MJOB { get; set; }

        [StringLength(500, ErrorMessage = "ID_MEMO,不得大於500個字")]
        public string ID_MEMO { get; set; }

        [StringLength(500, ErrorMessage = "AGENT_MEMO,不得大於500個字")]
        public string AGENT_MEMO { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "DEP_ID,不得大於50個字")]
        public string DEP_ID { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "MEM_ID,不得大於50個字")]
        public string MEM_ID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "MEM_NAME,不得大於100個字")]
        public string MEM_NAME { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "MEM_EMAIL,不得大於200個字")]
        public string MEM_EMAIL { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "MEM_JOBTITLE,不得大於50個字")]
        public string MEM_JOBTITLE { get; set; }

        [StringLength(500, ErrorMessage = "MEM_MEMO1,不得大於500個字")]
        public string MEM_MEMO1 { get; set; }

        [StringLength(500, ErrorMessage = "MEM_MEMO2,不得大於500個字")]
        public string MEM_MEMO2 { get; set; }

        public Nullable<DateTime> UPDATE_DATE { get; set; }

        public string CREATE_UID { get; set; }

        public string UPDATE_UID { get; set; }
    }
}
