#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record DataSetFileFilterHierarchy(
    Guid RootFilterId,
    List<Guid> ChildFilterIds,
    List<Guid> RootOptionIds,
    List<Dictionary<Guid, List<Guid>>> Tiers
);
