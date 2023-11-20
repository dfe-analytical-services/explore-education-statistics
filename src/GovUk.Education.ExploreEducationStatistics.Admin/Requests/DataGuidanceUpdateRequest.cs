#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record DataGuidanceUpdateRequest
{
    [Required]
    public string Content { get; init; } = string.Empty;

    [MinLength(1)]
    public List<DataGuidanceDataSetUpdateRequest> DataSets { get; init; } = new();
}

public record DataGuidanceDataSetUpdateRequest
{
    [Required]
    public Guid FileId { get; init; }

    [Required]
    public string Content { get; init; } = string.Empty;
}
