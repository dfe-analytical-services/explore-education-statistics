using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class ReleaseVersionTests
{
    [Fact]
    public void Live_True()
    {
        var releaseVersion = new ReleaseVersion { Published = DateTimeOffset.UtcNow.AddDays(-1) };

        Assert.True(releaseVersion.Live);
    }

    [Fact]
    public void Live_False_PublishedIsNull()
    {
        var releaseVersion = new ReleaseVersion { Published = null };

        Assert.False(releaseVersion.Live);
    }

    [Fact]
    public void Live_False_PublishedInFuture()
    {
        // Note that this should not happen, but we test the edge case.
        var releaseVersion = new ReleaseVersion { Published = DateTimeOffset.UtcNow.AddDays(1) };

        Assert.False(releaseVersion.Live);
    }

    [Fact]
    public void NextReleaseDate_Ok()
    {
        var releaseDate = new PartialDate { Year = "2021", Month = "1" };
        var releaseVersion = new ReleaseVersion { NextReleaseDate = releaseDate };

        Assert.Equal(releaseDate, releaseVersion.NextReleaseDate);
    }

    [Fact]
    public void NextReleaseDate_InvalidDate()
    {
        Assert.Throws<FormatException>(() => new ReleaseVersion { NextReleaseDate = new PartialDate { Day = "45" } });
    }
}
