#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record TableBuilderQueryViewModel(Guid PublicationId,
    Guid SubjectId,
    TimePeriodQuery TimePeriod,
    IEnumerable<Guid> Filters,
    IEnumerable<Guid> Indicators,
    IEnumerable<Guid> LocationIds)
{
    public TableBuilderQueryViewModel(Guid publicationId, ObservationQueryContext query) :
        this(publicationId, query.SubjectId, query.TimePeriod, query.Filters, query.Indicators, query.LocationIds)
    {
    }
}
