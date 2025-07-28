#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ReplacementRequest
{
    public List<Guid> OriginalFileIds { get; set; } = [];
}
