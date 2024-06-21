#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DeleteApiDataSetVersionPlanViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetTitle { get; init; } = null!;

    public Guid Id { get; init; }

    public string Version { get; init; } = null!;

    public DataSetVersionStatus Status { get; init; }

    public bool Valid { get; init; }
}
