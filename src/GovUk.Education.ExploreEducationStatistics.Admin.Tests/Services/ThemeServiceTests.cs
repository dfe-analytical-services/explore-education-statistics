#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task CreateTheme()
        {
            await using var context = InMemoryApplicationDbContext();

            var service = SetupThemeService(context);
            var result = await service.CreateTheme(
                new ThemeSaveViewModel
                {
                    Title = "Test theme",
                    Summary = "Test summary"
                }
            );

            result.AssertRight();
            Assert.Equal("Test theme", result.Right.Title);
            Assert.Equal("test-theme", result.Right.Slug);
            Assert.Equal("Test summary", result.Right.Summary);

            var savedTheme = await context.Themes.FindAsync(result.Right.Id);

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
                        Summary = "Test summary"
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.CreateTheme(
                    new ThemeSaveViewModel
                    {
                        Title = "Test theme",
                        Summary = "Test summary"
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task UpdateTheme()
        {
            var theme = new Theme
            {
                Title = "Test theme",
                Slug = "test-theme",
                Summary = "Test summary"
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
                var result = await service.UpdateTheme(
                    theme.Id,
                    new ThemeSaveViewModel
                    {
                        Title = "Updated theme",
                        Summary = "Updated summary"
                    }
                );

                result.AssertRight();
                Assert.Equal("Updated theme", result.Right.Title);
                Assert.Equal("updated-theme", result.Right.Slug);
                Assert.Equal("Updated summary", result.Right.Summary);

                var savedTheme = await context.Themes.FindAsync(result.Right.Id);

                Assert.Equal("Updated theme", savedTheme.Title);
                Assert.Equal("updated-theme", savedTheme.Slug);
                Assert.Equal("Updated summary", savedTheme.Summary);
            }
        }

        [Fact]
        public async Task UpdateTheme_FailsNonUniqueSlug()
        {
            var theme = new Theme
            {
                Title = "Test theme",
                Slug = "test-theme",
                Summary = "Test summary"
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
                        Summary = "Other summary"
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(context);
                var result = await service.UpdateTheme(
                    theme.Id,
                    new ThemeSaveViewModel
                    {
                        Title = "Other theme",
                        Summary = "Updated summary"
                    }
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
                Publications =
                [
                    new()
                    {
                        Title = "Test publication 1",
                        Slug = "test-publication-1"
                    },
                    new()
                    {
                        Title = "Test publication 2",
                        Slug = "test-publication-2",
                    }
                ]
            };

            // This publication should not be included with
            // the theme as it is unrelated.
            var unrelatedTheme = new Publication
            {
                Title = "Unrelated publication",
                Slug = "unrelated-publication"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(theme);
                context.Add(unrelatedTheme);

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

                Assert.Equal(2, theme.Publications.Count);
                Assert.Equal("Test publication 1", theme.Publications[0].Title);
                Assert.Equal("test-publication-1", theme.Publications[0].Slug);

                Assert.Equal("Test publication 2", theme.Publications[1].Title);
                Assert.Equal("test-publication-2", theme.Publications[1].Slug);
            }
        }

        [Fact]
        public async Task GetThemes()
        {
            var contextId = Guid.NewGuid().ToString();

            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Test summary"
            };

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
        public async Task DeleteTheme()
        {
            var releaseVersionId = Guid.NewGuid();

            var theme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "UI test theme to delete"
            };

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(releaseVersionId)
                    .Generate())
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(releaseVersionId)
                    .Generate())
                .Generate();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithId(releaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(theme)
                        .Generate())
                .WithDataBlockVersions(ListOf(
                    dataBlockParent.LatestDraftVersion!,
                    dataBlockParent.LatestPublishedVersion!))
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(releaseVersion.Publication)
                .Generate();

            var statsReleaseVersion = new Data.Model.ReleaseVersion
            {
                Id = releaseVersionId,
                PublicationId = releaseVersion.Publication.Id
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                contentContext.ReleaseVersions.Add(releaseVersion);
                contentContext.Methodologies.Add(methodology);
                statisticsContext.ReleaseVersion.Add(statsReleaseVersion);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();
            }

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.PublicationMethodologies.Count());
                Assert.Equal(1, contentContext.ReleaseVersions.Count());
                Assert.Equal(2, contentContext.DataBlockVersions.Count());
                Assert.Equal(1, contentContext.DataBlockParents.Count());
                Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    methodologyService: methodologyService.Object,
                    publishingService: publishingService.Object,
                    cacheService: cacheService.Object,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

                releaseDataFileService
                    .Setup(s => s.DeleteAll(releaseVersionId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .Setup(s => s.DeleteAll(releaseVersionId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseSubjectRepository
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersionId, false))
                    .Returns(Task.CompletedTask);

                methodologyService
                    .Setup(s => s.DeleteMethodology(methodology.Id, true))
                    .ReturnsAsync(Unit.Instance);

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                cacheService
                    .Setup(s =>
                        s.DeleteCacheFolderAsync(
                            ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersionId))))
                    .Returns(Task.CompletedTask);

                releasePublishingStatusRepository.Setup(mock =>
                        mock.RemovePublisherReleaseStatuses(new List<Guid> { releaseVersion.Id }))
                    .Returns(Task.CompletedTask);

                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(releaseDataFileService,
                    releaseFileService,
                    releaseSubjectRepository,
                    methodologyService,
                    publishingService,
                    cacheService,
                    releasePublishingStatusRepository);

                result.AssertRight();

                Assert.Empty(contentContext.Publications);
                Assert.Empty(contentContext.ReleaseVersions);
                Assert.Empty(contentContext.DataBlockVersions);
                Assert.Empty(contentContext.DataBlockParents);
                Assert.Empty(statisticsContext.ReleaseVersion);
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

            var releaseVersionIdsInExpectedDeleteOrder =
                AsList(releaseVersion4Id, releaseVersion3Id, releaseVersion2Id, releaseVersion1Id);

            var theme = new Theme
            {
                Id = themeId,
                Title = "UI test theme"
            };

            var publication = new Publication
            {
                Id = publicationId,
                Theme = theme,
                ReleaseVersions = AsList(new ReleaseVersion
                {
                    Id = releaseVersion2Id,
                    PreviousVersionId = releaseVersion1Id
                },
                    new ReleaseVersion { Id = releaseVersion1Id },
                    new ReleaseVersion
                    {
                        Id = releaseVersion4Id,
                        PreviousVersionId = releaseVersion3Id
                    },
                    new ReleaseVersion
                    {
                        Id = releaseVersion3Id,
                        PreviousVersionId = releaseVersion2Id
                    })
            };

            var statsReleases = AsList(new Data.Model.ReleaseVersion
            {
                Id = releaseVersion1Id,
                PublicationId = publicationId
            },
                new Data.Model.ReleaseVersion
                {
                    Id = releaseVersion2Id,
                    PublicationId = publicationId
                },
                new Data.Model.ReleaseVersion
                {
                    Id = releaseVersion3Id,
                    PublicationId = publicationId
                },
                new Data.Model.ReleaseVersion
                {
                    Id = releaseVersion4Id,
                    PublicationId = publicationId
                });

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                contentContext.Publications.Add(publication);
                contentContext.Themes.Add(theme);
                statisticsContext.ReleaseVersion.AddRange(statsReleases);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Themes.Count());
                Assert.Equal(4, contentContext.ReleaseVersions.Count());
                Assert.Equal(4, statisticsContext.ReleaseVersion.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    publishingService: publishingService.Object,
                    cacheService: cacheService.Object,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

                var releaseDataFileDeleteSequence = new MockSequence();

                releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                    releaseDataFileService
                        .InSequence(releaseDataFileDeleteSequence)
                        .Setup(s => s.DeleteAll(releaseId, true))
                        .ReturnsAsync(Unit.Instance));

                var releaseFileDeleteSequence = new MockSequence();

                releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                    releaseFileService
                        .InSequence(releaseFileDeleteSequence)
                        .Setup(s => s.DeleteAll(releaseId, true))
                        .ReturnsAsync(Unit.Instance));

                var releaseSubjectDeleteSequence = new MockSequence();

                releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                    releaseSubjectRepository
                        .InSequence(releaseSubjectDeleteSequence)
                        .Setup(s => s.DeleteAllReleaseSubjects(releaseId, false))
                        .Returns(Task.CompletedTask));

                var releaseCacheInvalidationSequence = new MockSequence();

                releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                    cacheService
                        .InSequence(releaseCacheInvalidationSequence)
                        .Setup(s =>
                            s.DeleteCacheFolderAsync(
                                ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseId))))
                        .Returns(Task.CompletedTask));

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                releasePublishingStatusRepository.Setup(mock =>
                        mock.RemovePublisherReleaseStatuses(releaseVersionIdsInExpectedDeleteOrder))
                    .Returns(Task.CompletedTask);

                var result = await service.DeleteTheme(themeId);
                VerifyAllMocks(
                    releaseDataFileService,
                    releaseFileService,
                    releaseSubjectRepository,
                    publishingService,
                    cacheService,
                    releasePublishingStatusRepository);

                result.AssertRight();

                Assert.Equal(0, contentContext.Publications.Count());
                Assert.Equal(0, contentContext.Themes.Count());
                Assert.Equal(0, contentContext.ReleaseVersions.Count());
                Assert.Equal(0, statisticsContext.ReleaseVersion.Count());
            }
        }

        [Fact]
        public async Task DeleteTheme_DisallowedByNamingConvention()
        {
            var theme = new Theme
            {
                Title = "Non-conforming title",
                Publications =
                [
                    new() { Title = "UI test publication 1" },
                    new() { Title = "UI test publication 2" }
                ]
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
                Publications =
                [
                    new() { Title = "UI test publication 1" },
                    new() { Title = "UI test publication 2" }
                ]
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

            var theme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "UI test theme to delete"
            };

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(releaseVersionId)
                    .Generate())
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(releaseVersionId)
                    .Generate())
                .Generate();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithId(releaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(theme)
                        .Generate())
                .WithDataBlockVersions(ListOf(
                    dataBlockParent.LatestDraftVersion!,
                    dataBlockParent.LatestPublishedVersion!))
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(releaseVersion.Publication)
                .Generate();

            var statsReleaseVersion = new Data.Model.ReleaseVersion
            {
                Id = releaseVersionId,
                PublicationId = releaseVersion.Publication.Id
            };

            var otherReleaseVersionId = Guid.NewGuid();

            var otherTheme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "UI test theme to retain"
            };

            var otherDataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(otherReleaseVersionId)
                    .Generate())
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersionId(otherReleaseVersionId)
                    .Generate())
                .Generate();

            var otherRelease = _fixture
                .DefaultReleaseVersion()
                .WithId(otherReleaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(otherTheme)
                        .Generate())
                .WithDataBlockVersions(ListOf(
                    otherDataBlockParent.LatestDraftVersion!,
                    otherDataBlockParent.LatestPublishedVersion!))
                .Generate();

            var otherMethodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(otherRelease.Publication)
                .Generate();

            var otherStatsReleaseVersion = new Data.Model.ReleaseVersion
            {
                Id = otherReleaseVersionId,
                PublicationId = otherRelease.Publication.Id
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                contentContext.Methodologies.AddRange(methodology, otherMethodology);
                contentContext.ReleaseVersions.AddRange(releaseVersion, otherRelease);
                contentContext.Themes.AddRange(theme, otherTheme);
                statisticsContext.ReleaseVersion.AddRange(statsReleaseVersion, otherStatsReleaseVersion);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(2, contentContext.Publications.Count());
                Assert.Equal(2, contentContext.Themes.Count());
                Assert.Equal(2, contentContext.Methodologies.Count());
                Assert.Equal(2, contentContext.PublicationMethodologies.Count());
                Assert.Equal(2, contentContext.ReleaseVersions.Count());
                Assert.Equal(4, contentContext.DataBlockVersions.Count());
                Assert.Equal(2, contentContext.DataBlockParents.Count());
                Assert.Equal(2, statisticsContext.ReleaseVersion.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    methodologyService: methodologyService.Object,
                    publishingService: publishingService.Object,
                    cacheService: cacheService.Object,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

                releaseDataFileService
                    .Setup(s => s.DeleteAll(releaseVersionId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .Setup(s => s.DeleteAll(releaseVersionId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseSubjectRepository
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersionId, false))
                    .Returns(Task.CompletedTask);

                methodologyService
                    .Setup(s => s.DeleteMethodology(methodology.Id, true))
                    .ReturnsAsync(Unit.Instance);

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                cacheService
                    .Setup(s =>
                        s.DeleteCacheFolderAsync(
                            ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersionId))))
                    .Returns(Task.CompletedTask);

                releasePublishingStatusRepository.Setup(mock =>
                        mock.RemovePublisherReleaseStatuses(new List<Guid> { releaseVersionId }))
                    .Returns(Task.CompletedTask);

                var result = await service.DeleteTheme(theme.Id);
                VerifyAllMocks(releaseDataFileService,
                    releaseFileService,
                    releaseSubjectRepository,
                    methodologyService,
                    publishingService,
                    cacheService,
                    releasePublishingStatusRepository);

                result.AssertRight();

                Assert.Equal(otherRelease.Publication.Id, contentContext.Publications.Single().Id);
                Assert.Equal(otherTheme.Id, contentContext.Themes.Single().Id);
                Assert.Equal(otherRelease.Id, contentContext.ReleaseVersions.Single().Id);
                Assert.Equal(otherRelease.Id, statisticsContext.ReleaseVersion.Single().Id);
                contentContext.DataBlockVersions.ForEach(dataBlockVersion =>
                    Assert.Equal(otherReleaseVersionId, dataBlockVersion.ReleaseVersionId));
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
                Publications = [uiTestThemePublication]
            };

            var standardTitleTheme = new Theme
            {
                Id = standardTitleThemeId,
                Title = "Standard title",
                Publications = [standardTitleThemePublication]
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(uiTestTheme, uiTestThemePublication, standardTitleTheme, standardTitleThemePublication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // Arrange
                var service = SetupThemeService(context);

                // Act
                await service.DeleteUITestThemes();

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
                Publications =
                [
                    new() { Title = "UI test publication 1" },
                    new() { Title = "UI test publication 2" }
                ]
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

        private static ThemeService SetupThemeService(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null,
            IMapper? mapper = null,
            IUserService? userService = null,
            IBlobCacheService? cacheService = null,
            IMethodologyService? methodologyService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
            IPublishingService? publishingService = null,
            bool enableThemeDeletion = true)
        {
            contentDbContext ??= new Mock<ContentDbContext>().Object;

            return new ThemeService(
                new AppOptions { EnableThemeDeletion = enableThemeDeletion }.ToOptionsWrapper(),
                contentDbContext,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                mapper ?? AdminMapper(),
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(),
                publishingService ?? new Mock<IPublishingService>().Object,
                cacheService ?? Mock.Of<IBlobCacheService>(Strict)
            );
        }
    }
}
