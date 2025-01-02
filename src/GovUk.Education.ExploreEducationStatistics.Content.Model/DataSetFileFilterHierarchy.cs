#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record DataSetFileFilterHierarchy(
    Guid RootFilterId,
    List<Guid> ChildFilterIds, // in order of the tiers
    List<Guid> RootOptionIds,
    List<Dictionary<Guid, List<Guid>>> Tiers // also in order i.e. Tier[0] is root -> childFilterIds[0], Tier[1] is childFilterIds[0] -> childFilterIds[1], etc.
);
