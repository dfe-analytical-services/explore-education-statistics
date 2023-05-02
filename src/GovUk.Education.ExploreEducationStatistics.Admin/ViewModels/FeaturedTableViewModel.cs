#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record FeaturedTableViewModel
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; set; }

        public int Order { get; set; }

        public Guid DataBlockId { get; set; }

        public DataBlock DataBlock { get; set; } = null!;
    }

    // @MarkFix move requests to own file?
    public record FeaturedTableCreateRequest
    {
        [Required] public string Name { get; init; } = string.Empty;

        public string? Description { get; set; }

        public Guid DataBlockId { get; set; }

    }

    public record FeaturedTableUpdateRequest
    {
        [Required]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; set; }
    }
}
