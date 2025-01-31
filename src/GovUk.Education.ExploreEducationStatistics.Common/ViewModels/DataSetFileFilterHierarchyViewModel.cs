using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record DataSetFileFilterHierarchyViewModel(
    Guid RootFilterId,
    List<Guid> ChildFilterIds,
    List<Guid> RootOptionIds,
    List<DataSetFileFilterHierarchyTierViewModel> Tiers
);

public record DataSetFileFilterHierarchyTierViewModel(
    int Level,
    Dictionary<Guid, List<Guid>> Hierarchy
);
