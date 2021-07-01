using DFHRIS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFHRIS.model
{
    class DF_SCH_DEP : IModel
    {
        public int DEP_GUID { get; set; }

        [Required]
        [StringLength(1, ErrorMessage = "SYS_TYPE,不得大於1個字")]
        public string SYS_TYPE { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "PAR_DEP,不得大於50個字")]
        public string PAR_DEP { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "DEP_ID,不得大於50個字")]
        public string DEP_ID { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "DEP_NAME,不得大於200個字")]
        public string DEP_NAME { get; set; }

        [StringLength(50, ErrorMessage = "DEP_LEVEL,不得大於50個字")]
        public string DEP_LEVEL { get; set; }

        [StringLength(500, ErrorMessage = "DEP_MEMO1,不得大於500個字")]
        public string DEP_MEMO1 { get; set; }

        [StringLength(500, ErrorMessage = "DEP_MEMO2,不得大於500個字")]
        public string DEP_MEMO2 { get; set; }

        [StringLength(500, ErrorMessage = "DEP_MEMO3,不得大於500個字")]
        public string DEP_MEMO3 { get; set; }
    }
}
