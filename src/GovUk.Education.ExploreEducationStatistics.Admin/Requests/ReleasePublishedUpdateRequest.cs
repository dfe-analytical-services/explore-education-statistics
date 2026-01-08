#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleasePublishedUpdateRequest
{
    [Range(
        typeof(DateTimeOffset),
        minimum: "2000-01-01T00:00:00Z",
        maximum: "2100-01-01T00:00:00Z",
        ErrorMessage = "Value for {0} must be between {1} and {2}"
    )]
    [Required]
    public DateTimeOffset? Published { get; init; }
}
