#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public record PermalinkCreateRequest
{
    public Guid? ReleaseId { get; init; }

    public TableBuilderConfiguration Configuration { get; init; } = new();

    public ObservationQueryContext Query { get; init; } = new();
}
