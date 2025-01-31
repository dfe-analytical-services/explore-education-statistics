#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FootnotesUpdateRequest
{
    public List<Guid> FootnoteIds { get; set; } = new();
}
