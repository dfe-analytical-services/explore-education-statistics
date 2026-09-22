using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions;

public class ReleaseVersionExtensionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public void AllFilesZipPath()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        Assert.Equal(
            $"{releaseVersion.Id}/zip/{releaseVersion.Release.Publication.Slug}_{releaseVersion.Release.Slug}.zip",
            releaseVersion.AllFilesZipPath()
        );
    }

    [Fact]
    public void AllFilesZipPath_Version1UsesExistingPath()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        Assert.Equal(releaseVersion.AllFilesZipPath(), releaseVersion.AllFilesZipPath(formatVersion: 1));
    }

    [Fact]
    public void AllFilesZipPath_LaterVersionUsesNewPath()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        Assert.Equal(
            $"{releaseVersion.Id}/zip/v2/{releaseVersion.Release.Publication.Slug}_{releaseVersion.Release.Slug}.zip",
            releaseVersion.AllFilesZipPath(formatVersion: 2)
        );
    }
}
