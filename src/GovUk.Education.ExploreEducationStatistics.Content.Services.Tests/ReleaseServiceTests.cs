#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";

            var methodology = new MethodologyVersionSummaryViewModel
            {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-slug",
                    Title = "Methodology"
            };

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedPublicationViewModel>(publicationPath))
                .ReturnsAsync(
                    new CachedPublicationViewModel
                    {
                        Id = publicationId,
                        Releases = new List<ReleaseTitleViewModel>
                        {
                            new() { Id = releaseId }
                        }
                    }
                );

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new CachedReleaseViewModel(releaseId)
                {
                    Type = new ReleaseTypeViewModel
                    {
                        Title = "National Statistics"
                    }
                });

            methodologyService.Setup(mock => mock.GetSummariesByPublication(publicationId))
                .ReturnsAsync(AsList(methodology));

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);

            var result = await service.Get(publicationPath, releasePath);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);

            var releaseViewModel = result.AssertRight();

            Assert.Equal(ReleaseType.NationalStatistics, releaseViewModel.Type);

            var publication = releaseViewModel.Publication;
            Assert.Equal(publicationId, publication.Id);

            Assert.Single(publication.Methodologies);
            Assert.Equal(methodology, publication.Methodologies[0]);
        }

        [Fact]
        public async Task Get_PublicationNotFound()
        {
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedPublicationViewModel>(publicationPath))
                    .ReturnsAsync(new NotFoundResult());

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new CachedReleaseViewModel(releaseId));

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);

            var result = await service.Get(publicationPath, releasePath);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Get_ReleaseNotFound()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedPublicationViewModel>(publicationPath))
                .ReturnsAsync(
                    new CachedPublicationViewModel
                    {
                        Id = publicationId,
                        Releases = new List<ReleaseTitleViewModel>
                        {
                            new() { Id = releaseId }
                        }
                    }
                );

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new NotFoundResult());

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);

            var result = await service.Get(publicationPath, releasePath);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);

            result.AssertNotFound();
        }

        [Fact]
        public async Task List()
        {
            var release1 = new Release
            {
                Slug = "release-1",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
                DataLastPublished = new DateTime(2020, 1, 1),
                NextReleaseDate = new PartialDate { Year = "2020" },
                Type = ReleaseType.NationalStatistics
            };
            var release2 = new Release
            {
                Slug = "release-2",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2021, 1, 1),
                DataLastPublished = new DateTime(2021, 1, 1),
                NextReleaseDate = new PartialDate { Year = "2021" },
                Type = ReleaseType.NationalStatistics
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(release1, release2)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                Assert.Equal(2, releases.Count);

                // Ordered from most newest to oldest
                Assert.Equal(release2.Id, releases[0].Id);
                Assert.Equal(release2.Title, releases[0].Title);
                Assert.Equal(release2.ReleaseName, releases[0].ReleaseName);
                Assert.Equal(release2.Slug, releases[0].Slug);
                Assert.Equal(release2.TimePeriodCoverage.GetEnumLabel(), releases[0].CoverageTitle);
                Assert.Equal(release2.YearTitle, releases[0].YearTitle);
                Assert.Equal(release2.Published, releases[0].Published);
                Assert.Equal(release2.NextReleaseDate, releases[0].NextReleaseDate);
                Assert.Equal(release2.Type, releases[0].Type);
                Assert.Equal(release2.DataLastPublished, releases[0].DataLastPublished);
                Assert.True(releases[0].LatestRelease);

                Assert.Equal(release1.Id, releases[1].Id);
                Assert.Equal(release1.Title, releases[1].Title);
                Assert.Equal(release1.ReleaseName, releases[1].ReleaseName);
                Assert.Equal(release1.Slug, releases[1].Slug);
                Assert.Equal(release1.TimePeriodCoverage.GetEnumLabel(), releases[1].CoverageTitle);
                Assert.Equal(release1.YearTitle, releases[1].YearTitle);
                Assert.Equal(release1.Published, releases[1].Published);
                Assert.Equal(release1.NextReleaseDate, releases[1].NextReleaseDate);
                Assert.Equal(release1.Type, releases[1].Type);
                Assert.Equal(release1.DataLastPublished, releases[1].DataLastPublished);
                Assert.False(releases[1].LatestRelease);
            }
        }

        [Fact]
        public async Task List_FiltersPreviousReleasesForAmendments()
        {
            var originalRelease = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
                Type = ReleaseType.NationalStatistics,
            };
            var amendedRelease = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2020, 2, 1),
                Type = ReleaseType.NationalStatistics,
                PreviousVersion = originalRelease
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(originalRelease, amendedRelease)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                var releaseViewModel = Assert.Single(releases);
                Assert.Equal(amendedRelease.Id, releaseViewModel.Id);
            }
        }

        [Fact]
        public async Task List_FiltersDraftReleases()
        {
            // Draft
            var release1 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Type = ReleaseType.NationalStatistics
            };
            // Published
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2021, 1, 1),
                Type = ReleaseType.NationalStatistics
            };
            // Amendment is draft
            var release2Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Type = ReleaseType.NationalStatistics,
                PreviousVersion = release2
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(release1, release2, release2Amendment)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                var releaseViewModel = Assert.Single(releases);
                Assert.Equal(release2.Id, releaseViewModel.Id);
            }
        }

        [Fact]
        public async Task List_PublicationNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List("random-slug");

            result.AssertNotFound();
        }

        private static ReleaseService SetupReleaseService(
            ContentDbContext? contentDbContext = null,
            IFileStorageService? fileStorageService = null,
            IMethodologyService? methodologyService = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext is null
                    ? Mock.Of<IPersistenceHelper<ContentDbContext>>()
                    : new PersistenceHelper<ContentDbContext>(contentDbContext),
                fileStorageService ?? Mock.Of<IFileStorageService>(),
                methodologyService ?? Mock.Of<IMethodologyService>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
