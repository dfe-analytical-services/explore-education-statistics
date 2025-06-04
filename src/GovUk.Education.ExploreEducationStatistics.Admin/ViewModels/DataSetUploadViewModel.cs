#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetUploadViewModel
{
    public Guid Id { get; init; }

    public required string DataSetTitle { get; init; }

    public required string DataFileName { get; init; }

    public required string MetaFileName { get; init; }

    public required DataSetUploadStatus Status { get; set; }

    public DataSetScreenerResult? ScreenerResult { get; set; }
}
