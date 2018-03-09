using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.DataContract
{
    public class News
    {
        [Key]
        public int Id { get; set; }

        public string Header { get; set; }

        public string Content { get; set; }

        public FileModel Picture { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
