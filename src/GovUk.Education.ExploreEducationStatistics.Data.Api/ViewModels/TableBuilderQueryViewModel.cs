#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record TableBuilderQueryViewModel
{
    public Guid PublicationId { get; init; }
    public Guid SubjectId { get; init; }
    public TimePeriodQuery TimePeriod { get; init; }
    public IEnumerable<Guid> Filters { get; init; }
    public IEnumerable<Guid> Indicators { get; init; }
    public IEnumerable<Guid> LocationIds { get; init; }

    public TableBuilderQueryViewModel()
    {
    }

    public TableBuilderQueryViewModel(Guid publicationId, ObservationQueryContext query)
    {
        PublicationId = publicationId;
        SubjectId = query.SubjectId;
        TimePeriod = query.TimePeriod;
        Filters = query.Filters;
        Indicators = query.Indicators;
        LocationIds = query.LocationIds;
    }
}
