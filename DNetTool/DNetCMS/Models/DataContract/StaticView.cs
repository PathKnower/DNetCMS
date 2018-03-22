using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace DNetCMS.Models.DataContract
{
    public class StaticView
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Route { get; set; }
    }
}
