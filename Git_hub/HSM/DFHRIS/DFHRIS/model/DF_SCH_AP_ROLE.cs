using DFHRIS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFHRIS.model
{
    class DF_SCH_AP_ROLE : IModel
    {
        [Required]
        public string AP_TYPE { get; set;}
        [Required]
        public string MEM_ID { get; set; }
        [Required]
        public string DEP_ID { get; set; }
        [Required]
        public string ROLE_ID { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string MODIFY_USER { get; set; }
        public DateTime? MODIFY_DATE { get; set; }
        public string ROLE_GUID { get; set; }
    }
}
