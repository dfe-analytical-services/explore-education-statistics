#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetUploadViewModel
{
    public Guid Id { get; init; }

    public required string DataSetTitle { get; init; } // moved to DataSetInfoViewModel

    public required string DataFileName { get; init; } // moved to DataSetInfoViewModel

    public required string MetaFileName { get; init; } // moved to DataSetInfoViewModel

    public required DataSetUploadStatus Status { get; set; }

    public DataSetScreenerResult? ScreenerResult { get; set; }
}
