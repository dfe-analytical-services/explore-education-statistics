using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Topic
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string Slug { get; set; }

        public Guid ThemeId { get; set; }

        public Theme Theme { get; set; }

        public List<Publication> Publications { get; set; }
    }
}