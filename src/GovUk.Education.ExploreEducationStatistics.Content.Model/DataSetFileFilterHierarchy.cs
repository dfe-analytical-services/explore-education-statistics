#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record DataSetFileFilterHierarchy(
    List<Guid> FilterIds, // in order
    List<Dictionary<Guid, List<Guid>>> Tiers // also in order
);
