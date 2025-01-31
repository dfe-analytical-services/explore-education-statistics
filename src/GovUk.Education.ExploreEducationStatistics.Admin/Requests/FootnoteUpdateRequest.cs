#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FootnoteUpdateRequest
{
    public string Content { get; init; } = string.Empty;

    public IReadOnlySet<Guid> Filters { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> FilterGroups { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> FilterItems { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> Indicators { get; init; } = new HashSet<Guid>();

    public IReadOnlySet<Guid> Subjects { get; init; } = new HashSet<Guid>();
}
