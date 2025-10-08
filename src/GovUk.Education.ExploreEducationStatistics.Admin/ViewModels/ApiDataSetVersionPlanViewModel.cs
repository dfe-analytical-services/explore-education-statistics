#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public abstract record ApiDataSetVersionPlanViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetTitle { get; init; } = null!;

    public Guid Id { get; init; }

    public string Version { get; init; } = null!;

    public DataSetVersionStatus Status { get; init; }

    public bool Valid { get; init; } //TODO: override Valid with auto-calculated in inherrited records when EES-5779 is ready
}

public record DeleteApiDataSetVersionPlanViewModel : ApiDataSetVersionPlanViewModel;

public record ReplaceApiDataSetVersionPlanViewModel : ApiDataSetVersionPlanViewModel
{
    public MappingStatusViewModel? MappingStatus { get; init; }

    public bool ReadyToPublish => Status == DataSetVersionStatus.Draft;

    //public override bool Valid { get; set; } TODO: override Valid with auto-calculated value when EES-5779 is ready
}
