using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNetCMS.Models.ViewModels.View
{
    public class EditBaseViewOverride
    {
        [Required]
        public string ChoosenView { get; set; }
        
        public string OldView { get; set; }

        public SelectList Views { get; set; }

        public bool Enable { get; set; }

        [Required]
        public string Code { get; set; }
    }
}