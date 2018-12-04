using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.DataDissemination.Meta.Api.Models
{
    public class Theme
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        public List<Topic> Topics { get; set; }
    }
}