#nullable enable
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DeleteDataFilePlanViewModel
{
    [JsonIgnore]
    public Guid ReleaseId { get; init; }

    [JsonIgnore]
    public Guid SubjectId { get; init; }

    public DeleteDataBlockPlanViewModel DeleteDataBlockPlan { get; init; } = null!;

    public List<Guid> FootnoteIds { get; init; } = null!;

    public DeleteApiDataSetVersionPlanViewModel? ApiDataSetVersionPlan { get; init; }

    public bool Valid => ApiDataSetVersionPlan?.Valid ?? true;
}
