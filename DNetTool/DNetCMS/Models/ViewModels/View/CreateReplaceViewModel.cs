﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNetCMS.Models.ViewModels.View
{
    public class CreateReplaceViewModel
    {
        [Required]
        public string ChoosenView { get; set; }

        public SelectList Views { get; set; }

        public bool Enable { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
