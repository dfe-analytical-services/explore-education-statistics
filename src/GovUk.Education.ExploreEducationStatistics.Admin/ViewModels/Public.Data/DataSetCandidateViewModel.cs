#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record DataSetCandidateViewModel
{
    public required Guid ReleaseFileId { get; init; } // @MarkFix ReleaseDataSetFileVersionId

    public required string Title { get; init; }
}
