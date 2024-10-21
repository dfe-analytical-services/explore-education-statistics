# nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Theme
    {
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.Empty;

        public string Slug { get; set; } = string.Empty;

        [Required] public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<Publication> Publications { get; set; } = [];

        public bool IsTestOrSeedTheme() => Title.StartsWith("UI test theme") || Title.StartsWith("Seed theme");
    }
}
