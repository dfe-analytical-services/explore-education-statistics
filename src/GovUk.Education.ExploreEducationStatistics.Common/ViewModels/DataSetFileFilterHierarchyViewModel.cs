using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record DataSetFileFilterHierarchyTierViewModel(
    int Level,
    Guid FilterId,
    Guid? ChildFilterId,
    Dictionary<Guid, List<Guid>> Hierarchy
);
