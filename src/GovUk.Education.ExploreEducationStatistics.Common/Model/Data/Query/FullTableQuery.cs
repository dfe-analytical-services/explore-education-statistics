#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

public record FullTableQuery
{
    public Guid SubjectId { get; set; }

    public List<Guid> LocationIds { get; set; } = new();

    public TimePeriodQuery? TimePeriod { get; set; }

    public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

    public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();
}
