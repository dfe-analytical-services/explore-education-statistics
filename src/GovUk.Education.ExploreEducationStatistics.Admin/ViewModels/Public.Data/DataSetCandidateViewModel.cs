#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record DataSetCandidateViewModel
{
    public required Guid FileId { get; init; }

    public required string Title { get; init; }
}
