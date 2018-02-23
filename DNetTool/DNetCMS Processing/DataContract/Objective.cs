using System.Threading.Tasks;
using System;
using System.Collections;

namespace DNetCMS_Processing.DataContract
{
    public class Objective
    {
        public Object MainObject { get; set; }

        public Enums.ObjectiveType ObjectiveType { get; set; }

        public Object AddObject { get; set; }
    }
}