#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DeleteApiDataSetVersionPlanViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetName { get; init; } = null!;

    public Guid DataSetVersionId { get; init; }

    public string Version { get; init; } = null!;

    public DataSetVersionStatus Status { get; init; }

    public bool Valid => Status.IsDeletableState();
}
