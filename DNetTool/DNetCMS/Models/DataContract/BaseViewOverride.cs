using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.DataContract
{
    [Table("ViewOverrides")]
    public class BaseViewOverride
    {
        [Key]
        public string View { get; set; }

        public string Path { get; set; }

        public bool Enable { get; set; }
    }
}
