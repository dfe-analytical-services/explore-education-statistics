#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetSummaryViewModelBuilder
{
    private DataSetVersionSummaryViewModel? _draftVersion;
    private DataSetLiveVersionSummaryViewModel? _liveVersion;

    public async Task<DataSetSummaryViewModel> Build()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Title = "Data set title",
            Summary = "Data set summary",
            Status = DataSetStatus.Published,
            DraftVersion = _draftVersion ?? null,
            LatestLiveVersion = _liveVersion ?? null,
            SupersedingDataSetId = null,
            PreviousReleaseIds = [],
        };
    }

    public DataSetSummaryViewModelBuilder WithLiveVersion(Guid releaseVersionId)
    {
        _liveVersion = new()
        {
            Id = Guid.NewGuid(),
            File = new() { Id = Guid.NewGuid(), Title = "Data set title" },
            ReleaseVersion = new() { Id = releaseVersionId, Title = "Release version 1" },
            Status = DataSetVersionStatus.Published,
            Published = DateTimeOffset.UtcNow,
            Type = DataSetVersionType.Minor,
            Version = "1.0",
        };

        return this;
    }

    public DataSetSummaryViewModelBuilder WithDraftVersion(
        Guid releaseVersionId,
        DataSetVersionStatus status = DataSetVersionStatus.Draft
    )
    {
        _draftVersion = new()
        {
            Id = Guid.NewGuid(),
            File = new() { Id = Guid.NewGuid(), Title = "Data set title" },
            ReleaseVersion = new() { Id = releaseVersionId, Title = "Release version 1" },
            Status = status,
            Type = DataSetVersionType.Minor,
            Version = "1.1",
        };

        return this;
    }
}
