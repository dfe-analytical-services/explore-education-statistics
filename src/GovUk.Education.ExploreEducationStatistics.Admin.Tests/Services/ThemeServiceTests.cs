#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;
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

            var publishingService = new Mock<IPublishingService>(Strict);
            
            publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                .ReturnsAsync(Unit.Instance);

            var service = SetupThemeService(
                contentDbContext: context,
                publishingService: publishingService.Object);
            
            var result = await service.CreateTheme(
                new ThemeSaveViewModel
                {
                    Title = "Test theme",
                    Summary = "Test summary"
                }
            );

            VerifyAllMocks(publishingService);
            
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
                var publishingService = new Mock<IPublishingService>(Strict);
            
                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                // Arrange
                var service = SetupThemeService(
                    contentDbContext: context,
                    publishingService: publishingService.Object);
                
                var result = await service.UpdateTheme(
                    theme.Id,
                    new ThemeSaveViewModel
                    {
                        Title = "Updated theme",
                        Summary = "Updated summary"
                    }
                );

                VerifyAllMocks(publishingService);
                
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

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithId(releaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(theme)
                        .Generate())
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(releaseVersion.Publication)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            {
                contentContext.ReleaseVersions.Add(releaseVersion);
                contentContext.Methodologies.Add(methodology);
                await contentContext.SaveChangesAsync();
            }

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            {
                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.PublicationMethodologies.Count());
                Assert.Equal(1, contentContext.ReleaseVersions.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);
            var releaseService = new Mock<IReleaseService>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    methodologyService: methodologyService.Object,
                    publishingService: publishingService.Object,
                    releaseService: releaseService.Object);

                releaseService
                    .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);
                
                methodologyService
                    .Setup(s => s.DeleteMethodology(methodology.Id, true))
                    .ReturnsAsync(Unit.Instance);

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTheme(theme.Id);
                
                VerifyAllMocks(releaseDataFileService,
                    methodologyService,
                    publishingService,
                    releaseService);

                result.AssertRight();

                Assert.Empty(contentContext.Publications);
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
                ReleaseVersions = [
                    new Content.Model.ReleaseVersion
                    {
                        Id = releaseVersion2Id,
                        PreviousVersionId = releaseVersion1Id
                    },
                    new Content.Model.ReleaseVersion { Id = releaseVersion1Id },
                    new Content.Model.ReleaseVersion
                    {
                        Id = releaseVersion4Id,
                        PreviousVersionId = releaseVersion3Id
                    },
                    new Content.Model.ReleaseVersion
                    {
                        Id = releaseVersion3Id,
                        PreviousVersionId = releaseVersion2Id
                    }
                ]
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
            var releaseService = new Mock<IReleaseService>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    publishingService: publishingService.Object,
                    releaseService: releaseService.Object);

                var releaseVersionDeleteSequence = new MockSequence();

                releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseVersionId =>
                    releaseService
                        .InSequence(releaseVersionDeleteSequence)
                        .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                        .ReturnsAsync(Unit.Instance));

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTheme(themeId);
                
                VerifyAllMocks(
                    publishingService,
                    releaseService);

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

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithId(releaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(theme)
                        .Generate())
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(releaseVersion.Publication)
                .Generate();

            var otherReleaseVersionId = Guid.NewGuid();

            var otherTheme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "UI test theme to retain"
            };

            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithId(otherReleaseVersionId)
                .WithPublication(
                    _fixture
                        .DefaultPublication()
                        .WithTheme(otherTheme)
                        .Generate())
                .Generate();

            var otherMethodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(otherReleaseVersion.Publication)
                .Generate();

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
            var releaseService = new Mock<IReleaseService>(Strict);

            await using (var contentContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupThemeService(
                    contentContext,
                    methodologyService: methodologyService.Object,
                    publishingService: publishingService.Object,
                    releaseService: releaseService.Object);

                methodologyService
                    .Setup(s => s.DeleteMethodology(methodology.Id, true))
                    .ReturnsAsync(Unit.Instance);

                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                releaseService
                    .Setup(s => s.DeleteTestReleaseVersion(releaseVersionId, CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTheme(theme.Id);
                
                VerifyAllMocks(releaseDataFileService,
                    methodologyService,
                    publishingService,
                    releaseService);

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
                var publishingService = new Mock<IPublishingService>(Strict);
            
                publishingService.Setup(s => s.TaxonomyChanged(CancellationToken.None))
                    .ReturnsAsync(Unit.Instance);

                // Arrange
                var service = SetupThemeService(
                    contentDbContext: context,
                    publishingService: publishingService.Object);

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
            IMapper? mapper = null,
            IUserService? userService = null,
            IMethodologyService? methodologyService = null,
            IPublishingService? publishingService = null,
            IReleaseService? releaseService = null,
            bool enableThemeDeletion = true)
        {
            contentDbContext ??= new Mock<ContentDbContext>().Object;

            return new ThemeService(
                new AppOptions { EnableThemeDeletion = enableThemeDeletion }.ToOptionsWrapper(),
                contentDbContext,
                mapper ?? AdminMapper(),
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                releaseService ??  Mock.Of<IReleaseService>(Strict)
            );
        }
    }
}
