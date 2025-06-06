#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record TableBuilderQueryViewModel
{
    public Guid PublicationId { get; init; }
    public Guid SubjectId { get; init; }
    public TimePeriodQuery? TimePeriod { get; init; }
    public IEnumerable<Guid> Filters
    {
        [Obsolete("Use GetNonHierarchicalFilterItemIds()")]
        get;
        init;
    }
    public IEnumerable<Guid> Indicators { get; init; }
    public IEnumerable<Guid> LocationIds { get; init; }

    public IDictionary<Guid, List<List<Guid>>>? FilterHierarchyOptions { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public TableBuilderQueryViewModel() // For Newtonsoft.Json
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    public TableBuilderQueryViewModel(Guid publicationId, FullTableQuery query)
    {
        PublicationId = publicationId;
        SubjectId = query.SubjectId;
        TimePeriod = query.TimePeriod;
        Filters = query.GetNonHierarchicalFilterItemIds();
        Indicators = query.Indicators;
        LocationIds = query.LocationIds;
        FilterHierarchyOptions = query.FilterHierarchyOptions;
    }

    public IEnumerable<Guid> GetNonHierarchicalFilterItemIds()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return Filters;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
