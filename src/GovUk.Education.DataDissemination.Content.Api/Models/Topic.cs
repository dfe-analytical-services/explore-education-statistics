using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.DataDissemination.Content.Api.Models
{
    public class Topic
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        public List<Publication> Publications { get; set; }

        public Guid ThemeId { get; set; }
        public Theme Theme { get; set; }
    }
}