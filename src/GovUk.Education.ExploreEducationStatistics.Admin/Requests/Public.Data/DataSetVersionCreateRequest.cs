#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionCreateRequest
{
    public required Guid ReleaseFileId { get; init; }
}
