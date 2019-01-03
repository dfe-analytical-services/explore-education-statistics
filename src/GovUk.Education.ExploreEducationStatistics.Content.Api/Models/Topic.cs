using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class Topic
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }
        
        public string Description { get; set; }

        public Guid ThemeId { get; set; }
        
        public Theme Theme { get; set; }
        
        public string Summary { get; set; }

        public List<Publication> Publications { get; set; }
    }
}