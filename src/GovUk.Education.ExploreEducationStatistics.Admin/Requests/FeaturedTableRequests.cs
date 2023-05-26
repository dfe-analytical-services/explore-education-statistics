#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FeaturedTableCreateRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Guid DataBlockId { get; set; }

}

public record FeaturedTableUpdateRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;
}
