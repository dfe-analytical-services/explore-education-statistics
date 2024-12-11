#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FeaturedTableCreateRequest
{
    [Required]
    [MaxLength(120, ErrorMessage = "Featured table name must be 120 characters or less")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(200, ErrorMessage = "Featured table description must be 200 characters or less")]
    public string Description { get; set; } = string.Empty;

    public Guid DataBlockId { get; set; }

}

public record FeaturedTableUpdateRequest
{
    [Required]
    [MaxLength(120, ErrorMessage = "Featured table name must be 120 characters or less")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(200, ErrorMessage = "Featured table description must be 200 characters or less")]
    public string Description { get; set; } = string.Empty;
}
