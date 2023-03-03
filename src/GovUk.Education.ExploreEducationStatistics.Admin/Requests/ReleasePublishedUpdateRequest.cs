#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleasePublishedUpdateRequest
{
    [Range(typeof(DateTime), minimum: "1/1/2000", maximum: "1/1/2100",
        ErrorMessage = "Value for {0} must be between {1} and {2}")]
    [Required]
    public DateTime? Published { get; init; }
}
