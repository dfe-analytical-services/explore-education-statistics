#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ApiDataSetVersionPlanViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetTitle { get; init; } = null!;

    public Guid Id { get; init; }

    public string Version { get; init; } = null!;

    public DataSetVersionStatus Status { get; init; }

    public MappingStatusViewModel? MappingStatus { get; init; }
    
    public bool Valid => MappingStatus == null 
                         || (MappingStatus.FiltersComplete && MappingStatus.LocationsComplete);
}
