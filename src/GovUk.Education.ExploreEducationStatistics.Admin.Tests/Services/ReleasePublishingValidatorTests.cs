#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Moq;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePublishingValidatorTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task IsMissingUpdatedApiDataSet_NewPublication_ReturnsFalse()
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .Generate();

        var dataSetService = new Mock<IDataSetService>();

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, []);

        Assert.False(result);
    }

    [Fact]
    public async Task IsMissingUpdatedApiDataSet_NoDataFileUploads_ReturnsFalse()
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .WithLatestPublishedReleaseVersion(releaseVersion)
            .Generate();

        var dataFileUploads = new List<File>();
        var dataSetService = new Mock<IDataSetService>();

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, dataFileUploads);

        Assert.False(result);
    }

    [Theory]
    [InlineData(DataSetVersionStatus.Draft)]
    [InlineData(DataSetVersionStatus.Mapping)]
    public async Task IsMissingUpdatedApiDataSet_AllDataSetsAssociated_ReturnsFalse(
        DataSetVersionStatus dataSetVersionStatus
    )
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .WithLatestPublishedReleaseVersion(releaseVersion)
            .Generate();

        var builder = new DataSetSummaryViewModelBuilder();
        var dataSets = new List<DataSetSummaryViewModel>
        {
            await builder.WithDraftVersion(releaseVersion.Id, dataSetVersionStatus).Build(),
        };
        var dataFileUploads = new List<File> { new() { DataSetFileId = dataSets[0].DraftVersion!.File.Id } };

        var dataSetService = new Mock<IDataSetService>();
        dataSetService.Setup(s => s.ListDataSets(releaseVersion.Release.PublicationId, default)).ReturnsAsync(dataSets);

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, dataFileUploads);

        Assert.False(result);
    }

    [Fact]
    public async Task IsMissingUpdatedApiDataSet_DataSetNotAssociated_ReturnsTrue()
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .WithLatestPublishedReleaseVersion(releaseVersion)
            .Generate();

        var builder = new DataSetSummaryViewModelBuilder();
        var dataSets = new List<DataSetSummaryViewModel> { await builder.WithLiveVersion(Guid.NewGuid()).Build() };
        var dataFileUploads = new List<File> { new() { DataSetFileId = Guid.NewGuid() } };

        var dataSetService = new Mock<IDataSetService>();
        dataSetService.Setup(s => s.ListDataSets(releaseVersion.Release.PublicationId, default)).ReturnsAsync(dataSets);

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, dataFileUploads);

        Assert.True(result);
    }

    [Fact]
    public async Task IsMissingUpdatedApiDataSet_DataSetNotAssociatedButAlreadyPublished_ReturnsFalse()
    {
        var releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithPublished(DateTimeOffset.UtcNow.AddDays(-1))
            .Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .WithLatestPublishedReleaseVersion(releaseVersion)
            .Generate();

        var builder = new DataSetSummaryViewModelBuilder();
        var dataSets = new List<DataSetSummaryViewModel> { await builder.WithDraftVersion(releaseVersion.Id).Build() };
        var dataFileUploads = new List<File> { new() { DataSetFileId = Guid.NewGuid() } };

        var dataSetService = new Mock<IDataSetService>();
        dataSetService.Setup(s => s.ListDataSets(releaseVersion.Release.PublicationId, default)).ReturnsAsync(dataSets);

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, dataFileUploads);

        Assert.False(result);
    }

    [Fact]
    public async Task IsMissingUpdatedApiDataSet_DraftVersionWrongStatus_ReturnsTrue()
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();

        _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease().WithVersions([releaseVersion])])
            .WithLatestPublishedReleaseVersion(releaseVersion)
            .Generate();

        var builder = new DataSetSummaryViewModelBuilder();
        var dataSets = new List<DataSetSummaryViewModel>
        {
            await builder.WithDraftVersion(releaseVersion.Id, DataSetVersionStatus.Failed).Build(),
        };
        var dataFileUploads = new List<File> { new() { DataSetFileId = dataSets[0].DraftVersion!.File.Id } };

        var dataSetService = new Mock<IDataSetService>();
        dataSetService.Setup(s => s.ListDataSets(releaseVersion.Release.PublicationId, default)).ReturnsAsync(dataSets);

        var validator = BuildReleasePublishingValidator(dataSetService: dataSetService.Object);

        var result = await validator.IsMissingUpdatedApiDataSet(releaseVersion, dataFileUploads);

        Assert.True(result);
    }

    private static ReleasePublishingValidator BuildReleasePublishingValidator(IDataSetService? dataSetService = null)
    {
        return new(dataSetService ?? new Mock<IDataSetService>().Object);
    }
}
