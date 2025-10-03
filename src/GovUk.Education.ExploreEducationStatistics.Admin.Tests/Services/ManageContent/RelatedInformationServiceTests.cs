#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class RelatedInformationServiceTests
{
    private readonly DataFixture _fixture = new();

    private static readonly List<Link> Links =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Description = "Related page 1",
            Url = "related-page-1",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Description = "Related page 2",
            Url = "related-page-2",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Description = "Related page 3",
            Url = "related-page-3",
        },
    ];

    [Fact]
    public async Task GetRelatedInformationAsync()
    {
        // Arrange
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelatedInformation(Links).Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        // Act
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildRelatedInformationService(contentDbContext);

            var result = await service.GetRelatedInformationAsync(releaseVersion.Id);

            // Assert
            var links = result.AssertRight();

            Assert.Equal(3, links.Count);
            Assert.Equal("Related page 1", links[0].Description);
            Assert.Equal("related-page-1", links[0].Url);
            Assert.Equal("Related page 2", links[1].Description);
            Assert.Equal("related-page-2", links[1].Url);
            Assert.Equal("Related page 3", links[2].Description);
            Assert.Equal("related-page-3", links[2].Url);
        }
    }

    [Fact]
    public async Task UpdateRelatedInformation()
    {
        // Arrange
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelatedInformation(Links).Generate();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        // Act
        var linkUpdateRequest = new List<CreateUpdateLinkRequest>
        {
            new() { Description = "Related page 2", Url = "related-page-2" },
            new() { Description = "Related page 1", Url = "related-page-1" },
        };

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildRelatedInformationService(contentDbContext);

            var result = await service.UpdateRelatedInformation(releaseVersion.Id, linkUpdateRequest);

            // Assert
            var links = result.AssertRight();

            Assert.Equal(2, links.Count);
            Assert.Equal("Related page 2", links[0].Description);
            Assert.Equal("related-page-2", links[0].Url);
            Assert.Equal("Related page 1", links[1].Description);
            Assert.Equal("related-page-1", links[1].Url);
        }
    }

    private static RelatedInformationService BuildRelatedInformationService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null
    )
    {
        contentDbContext ??= InMemoryContentDbContext();

        return new(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService().Object
        );
    }
}
