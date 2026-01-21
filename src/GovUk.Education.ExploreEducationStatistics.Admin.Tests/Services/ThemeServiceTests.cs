#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using Moq;
using Moq.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ThemeServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateTheme()
    {
        await using var context = InMemoryApplicationDbContext();

        var publishingService = new Mock<IPublishingService>(Strict);

        publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

        var service = SetupThemeService(contentDbContext: context, publishingService: publishingService.Object);

        var result = await service.CreateTheme(
            new ThemeSaveViewModel { Title = "Test theme", Summary = "Test summary" }
        );

        VerifyAllMocks(publishingService);

        result.AssertRight();

        Assert.Equal("Test theme", result.Right.Title);
        Assert.Equal("test-theme", result.Right.Slug);
        Assert.Equal("Test summary", result.Right.Summary);

        var savedTheme = await context.Themes.FindAsync(result.Right.Id);

        Assert.NotNull(savedTheme);
        Assert.Equal("Test theme", savedTheme.Title);
        Assert.Equal("test-theme", savedTheme.Slug);
        Assert.Equal("Test summary", savedTheme.Summary);
    }

    [Fact]
    public async Task CreateTheme_FailsNonUniqueSlug()
    {
        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(
                new Theme
                {
                    Title = "Test theme",
                    Slug = "test-theme",
                    Summary = "Test summary",
                }
            );

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context);
            var result = await service.CreateTheme(
                new ThemeSaveViewModel { Title = "Test theme", Summary = "Test summary" }
            );

            result.AssertBadRequest(SlugNotUnique);
        }
    }

    [Fact]
    public async Task UpdateTheme()
    {
        string[] expectedPublicationSlugs = ["publication-slug-1", "publication-slug-2"];
        var theme = new Theme
        {
            Title = "Test theme",
            Slug = "test-theme",
            Summary = "Test summary",
            Publications = expectedPublicationSlugs
                .Select(publicationSlug => new Publication { Slug = publicationSlug })
                .ToList(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            // Arrange
            var publishingService = new Mock<IPublishingService>(Strict);
            publishingService.Setup(s => s.TaxonomyChanged(It.IsAny<CancellationToken>())).ReturnsAsync(Unit.Instance);

            var adminEventRaiser = new AdminEventRaiserMockBuilder();
            var publicationCacheService = new PublicationCacheServiceMockBuilder();

            var service = SetupThemeService(
                contentDbContext: context,
                publishingService: publishingService.Object,
                adminEventRaiser: adminEventRaiser.Build(),
                publicationCacheService: publicationCacheService.Build()
            );

            // Act
            var result = await service.UpdateTheme(
                theme.Id,
                new ThemeSaveViewModel { Title = "Updated theme", Summary = "Updated summary" }
            );

            // Assert
            VerifyAllMocks(publishingService);

            result.AssertRight();

            Assert.Equal("Updated theme", result.Right.Title);
            Assert.Equal("updated-theme", result.Right.Slug);
            Assert.Equal("Updated summary", result.Right.Summary);

            var savedTheme = await context.Themes.FindAsync(result.Right.Id);

            Assert.NotNull(savedTheme);
            Assert.Equal("Updated theme", savedTheme.Title);
            Assert.Equal("updated-theme", savedTheme.Slug);
            Assert.Equal("Updated summary", savedTheme.Summary);

            adminEventRaiser.Assert.OnThemeUpdatedWasRaised(actual => actual == savedTheme);

            Assert.All(expectedPublicationSlugs, publicationCacheService.Assert.CacheInvalidatedForPublicationEntry);
        }
    }

    [Fact]
    public async Task UpdateTheme_FailsNonUniqueSlug()
    {
        var theme = new Theme
        {
            Title = "Test theme",
            Slug = "test-theme",
            Summary = "Test summary",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            context.Add(
                new Theme
                {
                    Title = "Other theme",
                    Slug = "other-theme",
                    Summary = "Other summary",
                }
            );

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context);
            var result = await service.UpdateTheme(
                theme.Id,
                new ThemeSaveViewModel { Title = "Other theme", Summary = "Updated summary" }
            );

            result.AssertBadRequest(SlugNotUnique);
        }
    }

    [Fact]
    public async Task GetTheme()
    {
        var theme = new Theme
        {
            Title = "Test theme",
            Slug = "test-theme",
            Summary = "Test summary",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context);
            var result = await service.GetTheme(theme.Id);

            var viewModel = result.AssertRight();
            Assert.Equal(theme.Id, viewModel.Id);
            Assert.Equal("Test theme", viewModel.Title);
            Assert.Equal("test-theme", viewModel.Slug);
            Assert.Equal("Test summary", viewModel.Summary);
        }
    }

    [Fact]
    public async Task GetThemes_CanManageAllTaxonomy()
    {
        var contextId = Guid.NewGuid().ToString();

        var theme = new Theme { Title = "Theme A", Summary = "Test summary" };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context);

            var themes = await service.GetThemes();

            Assert.Single(themes.Right);

            var themeViewModel = themes.Right[0];

            Assert.Equal(theme.Id, themeViewModel.Id);
            Assert.Equal("Theme A", themeViewModel.Title);
            Assert.Equal("Test summary", themeViewModel.Summary);
        }
    }

    [Fact]
    public async Task GetThemes_CannotManageAllTaxonomy()
    {
        User user = _fixture.DefaultUser();

        Publication publication1 = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)])
            .WithTheme(_fixture.DefaultTheme());
        Publication publication2 = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)])
            .WithTheme(_fixture.DefaultTheme());
        Publication publication3 = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)])
            .WithTheme(_fixture.DefaultTheme());

        var userReleaseRoles = _fixture
            .DefaultUserReleaseRole()
            // These roles should result in the theme being included.
            .ForIndex(
                0,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(publication1.Releases[0].Versions[0])
                        .SetRole(ReleaseRole.Approver)
            )
            .ForIndex(
                1,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(publication1.Releases[0].Versions[1])
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                2,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(publication2.Releases[0].Versions[0])
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                3,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(publication2.Releases[0].Versions[1])
                        .SetRole(ReleaseRole.Approver)
            )
            // These roles should not result in the theme being included.
            // This role is for a different user
            .ForIndex(
                4,
                s =>
                    s.SetUser(_fixture.DefaultUser())
                        .SetReleaseVersion(publication1.Releases[0].Versions[0])
                        .SetRole(ReleaseRole.Approver)
            )
            // This role is for a PrereleaseViewer and should be excluded
            .ForIndex(
                5,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(publication1.Releases[0].Versions[0])
                        .SetRole(ReleaseRole.PrereleaseViewer)
            )
            .GenerateList(6);

        var userPublicationRoles = _fixture
            .DefaultUserPublicationRole()
            // These roles should result in the theme being included.
            .ForIndex(0, s => s.SetUser(user).SetPublication(publication3).SetRole(PublicationRole.Allower))
            .ForIndex(1, s => s.SetUser(user).SetPublication(publication3).SetRole(PublicationRole.Owner))
            // The theme for these roles are already included due to the release roles.
            // They should not result in the theme doubling-up in the results
            .ForIndex(2, s => s.SetUser(user).SetPublication(publication1).SetRole(PublicationRole.Allower))
            .ForIndex(3, s => s.SetUser(user).SetPublication(publication1).SetRole(PublicationRole.Owner))
            .GenerateList(4);

        var userService = new Mock<IUserService>(Strict);
        userService.Setup(s => s.MatchesPolicy(SecurityPolicies.RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(SecurityPolicies.CanManageAllTaxonomy)).ReturnsAsync(false);
        userService.Setup(s => s.GetUserId()).Returns(user.Id);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userPublicationRoles.BuildMock());

        var service = SetupThemeService(
            userService: userService.Object,
            userReleaseRoleRepository: userReleaseRoleRepository.Object,
            userPublicationRoleRepository: userPublicationRoleRepository.Object
        );

        var result = await service.GetThemes();

        var themes = result.AssertRight();

        Assert.Equal(3, themes.Count);

        Assert.Equal(publication1.ThemeId, themes[0].Id);
        Assert.Equal(publication1.Theme.Title, themes[0].Title);
        Assert.Equal(publication1.Theme.Summary, themes[0].Summary);

        Assert.Equal(publication2.ThemeId, themes[1].Id);
        Assert.Equal(publication2.Theme.Title, themes[1].Title);
        Assert.Equal(publication2.Theme.Summary, themes[1].Summary);

        Assert.Equal(publication3.ThemeId, themes[2].Id);
        Assert.Equal(publication3.Theme.Title, themes[2].Title);
        Assert.Equal(publication3.Theme.Summary, themes[2].Summary);

        VerifyAllMocks(userService, userReleaseRoleRepository, userPublicationRoleRepository);
    }

    [Fact]
    public async Task DeleteTheme()
    {
        Theme theme = _fixture.DefaultTheme().WithTitle("UI test theme to delete");

        Publication publication1 = _fixture
            .DefaultPublication()
            .WithReleases([
                _fixture.DefaultRelease(publishedVersions: 1),
                _fixture.DefaultRelease(publishedVersions: 1),
            ])
            .WithTheme(theme);

        Publication publication2 = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 0, draftVersion: true)])
            .WithTheme(theme);

        Publication publication3 = _fixture.DefaultPublication().WithTheme(theme);

        Publication[] publications = [publication1, publication2, publication3];

        Methodology methodology = _fixture.DefaultMethodology().WithOwningPublication(publication1);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            contentContext.Publications.AddRange(publications);
            contentContext.Methodologies.Add(methodology);
            await contentContext.SaveChangesAsync();
        }

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            Assert.Equal(1, contentContext.Themes.Count());
            Assert.Equal(3, contentContext.Publications.Count());
            Assert.Equal(3, contentContext.Contacts.Count());
            Assert.Equal(1, contentContext.PublicationMethodologies.Count());
            Assert.Equal(1, contentContext.Methodologies.Count());
            Assert.Equal(3, contentContext.Releases.Count());
            Assert.Equal(3, contentContext.ReleaseVersions.Count());
        }

        var adminEventRaiser = new AdminEventRaiserMockBuilder();
        var methodologyService = new Mock<IMethodologyService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(
                contentContext,
                adminEventRaiser: adminEventRaiser.Build(),
                methodologyService: methodologyService.Object,
                publishingService: publishingService.Object,
                releaseVersionService: new TestReleaseVersionService(contentContext)
            );

            methodologyService.Setup(s => s.DeleteMethodology(methodology.Id, true)).ReturnsAsync(Unit.Instance);

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            var result = await service.DeleteTheme(theme.Id);

            VerifyAllMocks(methodologyService, publishingService);

            result.AssertRight();

            foreach (var publication in publications)
            {
                adminEventRaiser.Assert.OnPublicationDeletedWasRaised(
                    publication.Id,
                    publication.Slug,
                    publication.LatestPublishedReleaseVersion != null
                        ? new LatestPublishedReleaseInfo
                        {
                            LatestPublishedReleaseId = publication.LatestPublishedReleaseVersion.ReleaseId,
                            LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersion.Id,
                        }
                        : null
                );
            }
        }

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            Assert.Empty(contentContext.Themes);
            Assert.Empty(contentContext.Publications);
            Assert.Empty(contentContext.ReleaseVersions);
            Assert.Empty(contentContext.Contacts);
        }
    }

    [Fact]
    public async Task DeleteTheme_ReleaseVersionsDeletedByDataSetVersionOrder()
    {
        var theme = new Theme { Id = Guid.NewGuid(), Title = "UI test theme to delete" };

        Publication publication = _fixture.DefaultPublication().WithTheme(theme);

        ReleaseVersion releaseVersion1 = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

        ReleaseFile releaseVersion1ReleaseFile = _fixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion1);

        ReleaseVersion releaseVersion2 = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

        ReleaseFile releaseVersion2ReleaseFile = _fixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion2);

        ReleaseVersion releaseVersion3 = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(publication));

        ReleaseFile releaseVersion3ReleaseFile = _fixture.DefaultReleaseFile().WithReleaseVersion(releaseVersion3);

        DataSet dataSet = _fixture.DefaultDataSet();

        var dataSetVersions = _fixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .ForIndex(
                0,
                s =>
                    s.SetRelease(
                        _fixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseVersion1ReleaseFile.Id)
                    )
            )
            .ForIndex(
                1,
                s =>
                    s.SetRelease(
                        _fixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseVersion3ReleaseFile.Id)
                    )
            )
            .ForIndex(
                2,
                s =>
                    s.SetRelease(
                        _fixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseVersion2ReleaseFile.Id)
                    )
            )
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            contentContext.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2, releaseVersion3);
            contentContext.ReleaseFiles.AddRange(
                releaseVersion1ReleaseFile,
                releaseVersion2ReleaseFile,
                releaseVersion3ReleaseFile
            );
            await contentContext.SaveChangesAsync();
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        var publicDataDbContext = new Mock<PublicDataDbContext>();
        publicDataDbContext.SetupGet(c => c.DataSetVersions).ReturnsDbSet(dataSetVersions);

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(
                contentDbContext: contentContext,
                publicDataDbContext: publicDataDbContext.Object,
                publishingService: publishingService.Object,
                releaseVersionService: releaseVersionService.Object
            );

            var releaseVersionDeleteSequence = new MockSequence();

            var releaseVersionIdsInExpectedDeleteOrder = new List<Guid>
            {
                releaseVersion2.Id,
                releaseVersion3.Id,
                releaseVersion1.Id,
            };

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseVersionId =>
                releaseVersionService
                    .InSequence(releaseVersionDeleteSequence)
                    .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                    .ReturnsAsync(Unit.Instance)
            );

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            var result = await service.DeleteTheme(theme.Id);

            VerifyAllMocks(releaseDataFileService, publishingService, releaseVersionService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task DeleteTheme_ReleaseVersionsDeletedByVersionOrder()
    {
        var theme = new Theme { Id = Guid.NewGuid(), Title = "UI test theme to delete" };

        Publication publication = _fixture.DefaultPublication().WithTheme(theme);

        Release release1 = _fixture
            .DefaultRelease()
            .WithPublication(publication)
            .WithCreated(DateTime.UtcNow.AddDays(-1));

        Release release2 = _fixture
            .DefaultRelease()
            .WithPublication(publication)
            .WithCreated(DateTime.UtcNow.AddDays(-2));

        ReleaseVersion release1Version1 = _fixture.DefaultReleaseVersion().WithVersion(0).WithRelease(release1);

        ReleaseVersion release1Version2 = _fixture.DefaultReleaseVersion().WithVersion(1).WithRelease(release1);

        ReleaseVersion release1Version2Cancelled = _fixture
            .DefaultReleaseVersion()
            .WithVersion(1)
            .WithSoftDeleted()
            .WithRelease(release1);

        ReleaseVersion release1Version3 = _fixture.DefaultReleaseVersion().WithVersion(2).WithRelease(release1);

        ReleaseVersion release2Version1 = _fixture.DefaultReleaseVersion().WithVersion(0).WithRelease(release2);

        ReleaseVersion release2Version2 = _fixture.DefaultReleaseVersion().WithVersion(1).WithRelease(release2);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            contentContext.ReleaseVersions.AddRange(
                release1Version1,
                release1Version3,
                release1Version2Cancelled,
                release1Version2,
                release2Version1,
                release2Version2
            );

            await contentContext.SaveChangesAsync();
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(
                contentDbContext: contentContext,
                publishingService: publishingService.Object,
                releaseVersionService: releaseVersionService.Object
            );

            var releaseVersionDeleteSequence = new MockSequence();

            var releaseVersionIdsInExpectedDeleteOrder = new List<Guid>
            {
                // Expect the ReleaseVersions from the more recent Release to be deleted first.
                release1Version3.Id,
                release1Version2.Id,
                release1Version2Cancelled.Id,
                release1Version1.Id,
                release2Version2.Id,
                release2Version1.Id,
            };

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseVersionId =>
                releaseVersionService
                    .InSequence(releaseVersionDeleteSequence)
                    .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                    .ReturnsAsync(Unit.Instance)
            );

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            var result = await service.DeleteTheme(theme.Id);

            VerifyAllMocks(releaseDataFileService, publishingService, releaseVersionService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task DeleteTheme_ReleaseVersionsDeletedInCorrectOrder()
    {
        var themeId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var releaseVersion1Id = Guid.NewGuid();
        var releaseVersion2Id = Guid.NewGuid();
        var releaseVersion3Id = Guid.NewGuid();
        var releaseVersion4Id = Guid.NewGuid();

        var releaseVersionIdsInExpectedDeleteOrder = AsList(
            releaseVersion4Id,
            releaseVersion3Id,
            releaseVersion2Id,
            releaseVersion1Id
        );

        var theme = new Theme { Id = themeId, Title = "UI test theme" };

        var publication = new Publication
        {
            Id = publicationId,
            Theme = theme,
            ReleaseVersions =
            [
                new ReleaseVersion { Id = releaseVersion2Id, PreviousVersionId = releaseVersion1Id },
                new ReleaseVersion { Id = releaseVersion1Id },
                new ReleaseVersion { Id = releaseVersion4Id, PreviousVersionId = releaseVersion3Id },
                new ReleaseVersion { Id = releaseVersion3Id, PreviousVersionId = releaseVersion2Id },
            ],
            Contact = new Contact(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            contentContext.Publications.Add(publication);
            contentContext.Themes.Add(theme);
            await contentContext.SaveChangesAsync();

            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.Themes.Count());
            Assert.Equal(4, contentContext.ReleaseVersions.Count());
        }

        var publishingService = new Mock<IPublishingService>(Strict);
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(
                contentContext,
                publishingService: publishingService.Object,
                releaseVersionService: releaseVersionService.Object
            );

            var releaseVersionDeleteSequence = new MockSequence();

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseVersionId =>
                releaseVersionService
                    .InSequence(releaseVersionDeleteSequence)
                    .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                    .ReturnsAsync(Unit.Instance)
            );

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            var result = await service.DeleteTheme(themeId);

            VerifyAllMocks(publishingService, releaseVersionService);

            result.AssertRight();

            Assert.Equal(0, contentContext.Publications.Count());
            Assert.Equal(0, contentContext.Themes.Count());
        }
    }

    [Fact]
    public async Task DeleteTheme_DisallowedByNamingConvention()
    {
        var theme = new Theme
        {
            Title = "Non-conforming title",
            Publications = [new() { Title = "UI test publication 1" }, new() { Title = "UI test publication 2" }],
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context);
            var result = await service.DeleteTheme(theme.Id);

            result.AssertForbidden();
            Assert.Equal(1, await context.Themes.CountAsync());
        }
    }

    [Fact]
    public async Task DeleteTheme_DisallowedByConfiguration()
    {
        var theme = new Theme
        {
            Title = "UI test theme",
            Publications = [new() { Title = "UI test publication 1" }, new() { Title = "UI test publication 2" }],
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context, enableThemeDeletion: false);

            var result = await service.DeleteTheme(theme.Id);
            result.AssertForbidden();

            Assert.Equal(1, await context.Themes.CountAsync());
        }
    }

    [Fact]
    public async Task DeleteTheme_OtherThemesUnaffected()
    {
        var releaseVersionId = Guid.NewGuid();

        var theme = new Theme { Id = Guid.NewGuid(), Title = "UI test theme to delete" };

        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithId(releaseVersionId)
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication().WithTheme(theme)));

        Methodology methodology = _fixture
            .DefaultMethodology()
            .WithOwningPublication(releaseVersion.Release.Publication);

        var otherReleaseVersionId = Guid.NewGuid();

        var otherTheme = new Theme { Id = Guid.NewGuid(), Title = "UI test theme to retain" };

        ReleaseVersion otherReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithId(otherReleaseVersionId)
            .WithRelease(
                _fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication().WithTheme(otherTheme))
            );

        Methodology otherMethodology = _fixture
            .DefaultMethodology()
            .WithOwningPublication(otherReleaseVersion.Release.Publication);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            contentContext.Methodologies.AddRange(methodology, otherMethodology);
            contentContext.ReleaseVersions.AddRange(releaseVersion, otherReleaseVersion);
            contentContext.Themes.AddRange(theme, otherTheme);

            await contentContext.SaveChangesAsync();

            Assert.Equal(2, contentContext.Publications.Count());
            Assert.Equal(2, contentContext.Themes.Count());
            Assert.Equal(2, contentContext.Methodologies.Count());
            Assert.Equal(2, contentContext.PublicationMethodologies.Count());
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var methodologyService = new Mock<IMethodologyService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var releaseVersionService = new Mock<IReleaseVersionService>(Strict);

        await using (var contentContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(
                contentContext,
                methodologyService: methodologyService.Object,
                publishingService: publishingService.Object,
                releaseVersionService: releaseVersionService.Object
            );

            methodologyService.Setup(s => s.DeleteMethodology(methodology.Id, true)).ReturnsAsync(Unit.Instance);

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            releaseVersionService
                .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                .ReturnsAsync(Unit.Instance);

            var result = await service.DeleteTheme(theme.Id);

            VerifyAllMocks(releaseDataFileService, methodologyService, publishingService, releaseVersionService);

            result.AssertRight();

            Assert.Equal(otherReleaseVersion.Publication.Id, contentContext.Publications.Single().Id);
            Assert.Equal(otherTheme.Id, contentContext.Themes.Single().Id);
        }
    }

    [Fact]
    public async Task DeleteUITestThemes()
    {
        // Arrange
        var uiTestTheme1Id = Guid.NewGuid();
        var standardTitleThemeId = Guid.NewGuid();

        var uiTestThemePublication = new Publication
        {
            ThemeId = uiTestTheme1Id,
            Title = "UI test theme",
            Contact = new Contact(),
        };

        var standardTitleThemePublication = new Publication
        {
            ThemeId = standardTitleThemeId,
            Title = "Standard title theme",
        };

        var uiTestTheme = new Theme
        {
            Id = uiTestTheme1Id,
            Title = "UI test theme",
            Publications = [uiTestThemePublication],
        };

        var standardTitleTheme = new Theme
        {
            Id = standardTitleThemeId,
            Title = "Standard title",
            Publications = [standardTitleThemePublication],
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.AddRangeAsync(
                uiTestTheme,
                uiTestThemePublication,
                standardTitleTheme,
                standardTitleThemePublication
            );
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publishingService = new Mock<IPublishingService>(Strict);

            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None)).ReturnsAsync(Unit.Instance);

            // Arrange
            var service = SetupThemeService(contentDbContext: context, publishingService: publishingService.Object);

            // Act
            await service.DeleteUITestThemes();

            VerifyAllMocks(publishingService);

            // Assert
            var themesResult = await context.Themes.ToListAsync();
            var publicationsResult = await context.Publications.ToListAsync();

            Assert.Single(themesResult);
            Assert.Single(publicationsResult);
            Assert.DoesNotContain(themesResult, theme => theme.Title is "UI test theme");
        }
    }

    [Fact]
    public async Task DeleteUITestThemes_DisallowedByConfiguration()
    {
        var theme = new Theme
        {
            Title = "UI test theme",
            Publications = [new() { Title = "UI test publication 1" }, new() { Title = "UI test publication 2" }],
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupThemeService(context, enableThemeDeletion: false);

            var result = await service.DeleteUITestThemes();
            result.AssertForbidden();

            Assert.Equal(1, await context.Themes.CountAsync());
        }
    }

    public class CacheInvalidationTests
    {
        [Fact]
        public async Task GivenPublicationsForTheme_WhenInvalidatePublicationsCacheByThemeIsCalled_ThenPublicationsForThemeAreInvalidated()
        {
            // ARRANGE
            var themeId = Guid.NewGuid();
            Publication[] publications =
            [
                new() { Slug = "publication-slug-1", ThemeId = themeId },
                new()
                {
                    Slug = "publication-slug-2",
                    ThemeId = themeId,
                    SupersededById = Guid.NewGuid(),
                }, // Archived Publication
                new() { Slug = "publication-slug-3", ThemeId = themeId },
                new() { Slug = "different-theme-publication-slug-A", ThemeId = Guid.NewGuid() },
                new() { Slug = "different-theme-publication-slug-B", ThemeId = Guid.NewGuid() },
            ];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publications);
                await context.SaveChangesAsync();
            }

            var publicationCacheService = new PublicationCacheServiceMockBuilder();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = SetupThemeService(
                    contentDbContext: context,
                    publicationCacheService: publicationCacheService.Build()
                );

                // ACT
                await sut.InvalidatePublicationsCacheByTheme(themeId);
            }

            // ASSERT
            var expectedInvalidatedPublications = publications.Where(p => p.ThemeId == themeId).ToArray();
            foreach (var expected in expectedInvalidatedPublications)
            {
                publicationCacheService.Assert.CacheInvalidatedForPublicationEntry(expected.Slug);
            }
            var expectedNotInvalidatedPublications = publications.Except(expectedInvalidatedPublications);
            foreach (var notExpected in expectedNotInvalidatedPublications)
            {
                publicationCacheService.Assert.CacheNotInvalidatedForPublicationEntry(notExpected.Slug);
            }
        }

        [Fact]
        public async Task GivenPublicationsForTheme_WhenInvalidatingPublicationThrows_ThenOtherPublicationsAreStillInvalidated()
        {
            // ARRANGE
            var themeId = Guid.NewGuid();
            Publication[] publications =
            [
                new() { Slug = "publication-slug-1", ThemeId = themeId },
                new() { Slug = "publication-slug-2", ThemeId = themeId },
                new() { Slug = "publication-slug-3", ThemeId = themeId },
            ];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publications);
                await context.SaveChangesAsync();
            }

            // Setup publication-slug-2 to throw
            var publicationCacheService = new PublicationCacheServiceMockBuilder().WhereInvalidatingPublicationThrows(
                publications[1].Slug
            );

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = SetupThemeService(
                    contentDbContext: context,
                    publicationCacheService: publicationCacheService.Build()
                );

                // ACT
                await sut.InvalidatePublicationsCacheByTheme(themeId);
            }

            // ASSERT
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-1");
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-2");
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-3");
        }

        [Fact]
        public async Task GivenPublicationsForTheme_WhenInvalidatingPublicationFails_ThenOtherPublicationsAreStillInvalidated()
        {
            // ARRANGE
            var themeId = Guid.NewGuid();
            Publication[] publications =
            [
                new() { Slug = "publication-slug-1", ThemeId = themeId },
                new() { Slug = "publication-slug-2", ThemeId = themeId },
                new() { Slug = "publication-slug-3", ThemeId = themeId },
            ];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publications);
                await context.SaveChangesAsync();
            }

            // Setup publication-slug-2 to throw
            var publicationCacheService = new PublicationCacheServiceMockBuilder().WhereInvalidatingPublicationFails(
                publications[1].Slug
            );

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = SetupThemeService(
                    contentDbContext: context,
                    publicationCacheService: publicationCacheService.Build()
                );

                // ACT
                await sut.InvalidatePublicationsCacheByTheme(themeId);
            }

            // ASSERT
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-1");
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-2");
            publicationCacheService.Assert.CacheInvalidatedForPublicationEntry("publication-slug-3");
        }
    }

    private static ThemeService SetupThemeService(
        ContentDbContext? contentDbContext = null,
        PublicDataDbContext? publicDataDbContext = null,
        IMapper? mapper = null,
        IUserService? userService = null,
        IMethodologyService? methodologyService = null,
        IPublishingService? publishingService = null,
        IReleaseVersionService? releaseVersionService = null,
        IAdminEventRaiser? adminEventRaiser = null,
        IPublicationCacheService? publicationCacheService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        bool enableThemeDeletion = true
    )
    {
        contentDbContext ??= new Mock<ContentDbContext>().Object;

        if (publicDataDbContext == null)
        {
            var publicDataDbContextMock = new Mock<PublicDataDbContext>();
            publicDataDbContextMock.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet([]);
            publicDataDbContext = publicDataDbContextMock.Object;
        }

        return new ThemeService(
            appOptions: new AppOptions { EnableThemeDeletion = enableThemeDeletion }.ToOptionsWrapper(),
            contentDbContext: contentDbContext,
            dataSetVersionRepository: new DataSetVersionRepository(
                contentDbContext: contentDbContext,
                publicDataDbContext: publicDataDbContext
            ),
            mapper ?? AdminMapper(),
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService().Object,
            methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            publishingService ?? Mock.Of<IPublishingService>(Strict),
            releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            adminEventRaiser ?? new AdminEventRaiserMockBuilder().Build(),
            publicationCacheService ?? new PublicationCacheServiceMockBuilder().Build(),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            NullLogger<ThemeService>.Instance
        );
    }

    private class TestReleaseVersionService(ContentDbContext context) : IReleaseVersionService
    {
        public async Task<Either<ActionResult, Unit>> DeleteTestReleaseVersion(
            Guid releaseVersionId,
            CancellationToken cancellationToken
        )
        {
            var releaseVersion = await context.ReleaseVersions.FirstAsync(
                rv => rv.Id == releaseVersionId,
                cancellationToken
            );
            context.ReleaseVersions.Remove(releaseVersion);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Instance;
        }

        public Task<Either<ActionResult, DeleteReleasePlanViewModel>> GetDeleteReleaseVersionPlan(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, Unit>> DeleteReleaseVersion(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, ReleaseVersionViewModel>> GetRelease(Guid releaseVersionId) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseVersionId
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, ReleaseVersionViewModel>> UpdateReleaseVersion(
            Guid releaseVersionId,
            ReleaseVersionUpdateRequest request
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, Unit>> UpdatePublishedDisplayDate(
            Guid releaseVersionId,
            ReleaseVersionPublishedDisplayDateUpdateRequest request
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListReleasesWithStatuses(
            params ReleaseApprovalStatus[] releaseApprovalStatues
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListUsersReleasesForApproval() =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListScheduledReleases() =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, DeleteDataFilePlanViewModel>> GetDeleteDataFilePlan(
            Guid releaseVersionId,
            Guid fileId,
            CancellationToken cancellationToken = default
        ) => throw new NotImplementedException();

        public Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(
            Guid releaseVersionId,
            Guid fileId
        ) => throw new NotImplementedException();
    }
}
