using System;

namespace DNetCMS.Models.DataContract
{
    public class Objective
    {
        public Object MainObject { get; set; }

        public Enums.ObjectiveType ObjectiveType { get; set; }

        public Object AddObject { get; set; }
    }
}