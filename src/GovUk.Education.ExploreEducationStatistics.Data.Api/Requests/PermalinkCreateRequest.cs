#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public record PermalinkCreateRequest
{
    public Guid? ReleaseId { get; init; }

    public TableBuilderConfiguration Configuration { get; init; } = new();

    public FullTableQueryRequest Query { get; init; } = new();
}
