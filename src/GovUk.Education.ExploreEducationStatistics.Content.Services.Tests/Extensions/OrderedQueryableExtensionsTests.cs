using AngleSharp.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Extensions;

public class OrderedQueryableExtensionsTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public void ThenByReleaseType_ReordersPublicationsByTheirReleaseType()
    {
        // Arrange
        var publications = _dataFixture.DefaultPublication().Generate(6);

        var publicationsResult = new List<FreeTextValueResult<Publication>>();
        var publishedDate = DateTime.UtcNow;

        for (var i = 0; i < publications.Count(); i++)
        {
            var publication = publications.GetItemByIndex(i);
            publication.LatestPublishedReleaseVersion = new()
            {
                Type = (ReleaseType)i,
                Published = publishedDate,
            };

            publicationsResult.Add(new() { Rank = i, Value = publication });
        }

        var orderedQueryable = publicationsResult.AsQueryable().OrderBy(p => 1); // Initial nonsense ordering to convert to an IOrderedQueryable

        // Act
        var result = orderedQueryable.ThenByReleaseType().ToList();

        // Assert
        Assert.Equal(
            ReleaseType.AccreditedOfficialStatistics,
            result[0].Value.LatestPublishedReleaseVersion!.Type
        );
        Assert.Equal(
            ReleaseType.OfficialStatistics,
            result[1].Value.LatestPublishedReleaseVersion!.Type
        );
        Assert.Equal(
            ReleaseType.OfficialStatisticsInDevelopment,
            result[2].Value.LatestPublishedReleaseVersion!.Type
        );
        Assert.Equal(
            ReleaseType.ExperimentalStatistics,
            result[3].Value.LatestPublishedReleaseVersion!.Type
        );
        Assert.Equal(
            ReleaseType.AdHocStatistics,
            result[4].Value.LatestPublishedReleaseVersion!.Type
        );
        Assert.Equal(
            ReleaseType.ManagementInformation,
            result[5].Value.LatestPublishedReleaseVersion!.Type
        );
    }
}
