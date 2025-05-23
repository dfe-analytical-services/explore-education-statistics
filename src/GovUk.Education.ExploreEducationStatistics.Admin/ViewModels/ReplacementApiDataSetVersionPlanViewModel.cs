#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReplacementApiDataSetVersionPlanViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetTitle { get; init; } = null!;

    public Guid Id { get; init; }

    public string Version { get; init; } = null!;

    public DataSetVersionStatus Status { get; init; }

    public MappingStatusViewModel? MappingStatus { get; set; }

    public bool Valid { get; set; } //TODO: please note, this is kept as is for backward (feature flagging) compatibility with the rest of the code.
    public bool ValidDefinition => MappingStatus is { Complete: true, FiltersComplete: true, LocationsComplete: true, HasMajorVersionUpdate: false };
}
