#nullable enable
using System.Collections.Generic;
using System;
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

    public bool IsLinkedToApiDataSetVersion { get; init; }

    public DeleteApiDataSetVersionPlanViewModel? LinkedApiDataSetVersionDeletionPlan { get; init; }

    public bool Valid => !IsLinkedToApiDataSetVersion;
}