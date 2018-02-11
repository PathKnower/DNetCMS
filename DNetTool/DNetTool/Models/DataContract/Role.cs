using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetTool.Models.DataContract
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public Role ParentRole { get; set; }

        public List<User> Users { get; set; }

        public Role()
        {
            Users = new List<User>();
        }

    }
}
