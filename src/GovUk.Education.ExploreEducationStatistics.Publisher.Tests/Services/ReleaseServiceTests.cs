#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task GetFiles()
        {
            var release = new Release();
            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = release,
                    Name = "Ancillary Test File",
                    File = new File
                    {
                        Filename = "ancillary.pdf",
                        ContentLength = 10240,
                        Type = Ancillary
                    }
                },
                new()
                {
                    Release = release,
                    File = new File
                    {
                        Filename = "chart.png",
                        Type = Chart
                    }
                },
                new()
                {
                    Release = release,
                    Name = "Data Test File",
                    File = new File
                    {
                        Filename = "data.csv",
                        ContentLength = 20480,
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = release,
                    File = new File
                    {
                        Filename = "data.meta.csv",
                        Type = Metadata
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release);
                await contentDbContext.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetFiles(release.Id,
                    Ancillary,
                    Chart);

                Assert.Equal(2, result.Count);
                Assert.Equal(releaseFiles[0].File.Id, result[0].Id);
                Assert.Equal(releaseFiles[1].File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetLatestRelease()
        {
            var publication = new Publication();

            var release1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2017",
                TimePeriodCoverage = AcademicYearQ4,
                Slug = "2017-18-q4",
                Published = new DateTime(2019, 4, 1),
                ApprovalStatus = Approved
            };

            var release2 = new Release
            {
                Id = new Guid("e7e1aae3-a0a1-44b7-bdf3-3df4a363ce20"),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                Slug = "2018-19-q1",
                Published = new DateTime(2019, 3, 1),
                ApprovalStatus = Approved,
            };

            var release3V0 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = new DateTime(2019, 1, 1),
                ApprovalStatus = Approved,
                Version = 0,
                PreviousVersionId = null
            };

            var release3V1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = new DateTime(2019, 2, 1),
                ApprovalStatus = Approved,
                Version = 1,
                PreviousVersionId = release3V0.Id
            };

            var release3V2Deleted = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = null,
                ApprovalStatus = Approved,
                Version = 2,
                PreviousVersionId = release3V1.Id,
                SoftDeleted = true
            };

            var release3V3NotPublished = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = null,
                ApprovalStatus = Approved,
                Version = 3,
                PreviousVersionId = release3V1.Id
            };

            var release4 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ3,
                Slug = "2018-19-q3",
                Published = null,
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication);
                await contentDbContext.AddRangeAsync(release1,
                    release2,
                    release3V0,
                    release3V1,
                    release3V2Deleted,
                    release3V3NotPublished,
                    release4);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetLatestRelease(publication.Id, Enumerable.Empty<Guid>());

                Assert.Equal(release3V1.Id, result.Id);
                Assert.Equal("Academic year Q2 2018/19", result.Title);
            }
        }

        //[Fact] // @MarkFix
        //public async Task CompletePublishing_FirstVersion()
        //{
        //    // Arrange
        //    var release = _fixture
        //        .DefaultRelease()
        //        .Generate();

        //    //var releaseSeriesItem = new ReleaseSeriesItem // @MarkFix
        //    //{
        //    //    ReleaseId = release.Id,
        //    //    Order = 1
        //    //};

        //    //var publication = new Publication
        //    //{
        //    //    Releases = new() { release },
        //    //    ReleaseSeriesView = new() { releaseSeriesItem }
        //    //};

        //    var originalDataBlockParents = _fixture
        //        .DefaultDataBlockParent()
        //        .WithLatestDraftVersion(() => _fixture
        //            .DefaultDataBlockVersion()
        //            .WithRelease(release)
        //            .Generate())
        //        .GenerateList(2);

        //    var actualPublishedDate = DateTime.UtcNow;

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //await contentDbContext.Publications.AddAsync(publication); // @MarkFix
        //        await contentDbContext.Releases.AddAsync(release);
        //        await contentDbContext.SaveChangesAsync();
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //       // var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict); // @MarkFix

        //       // publicationReleaseSeriesViewService.Setup(s => s.UpdateForPublishRelease(
        //       //     It.IsAny<Guid>(),
        //       //     It.IsAny<Guid>()))
        //       // .Returns(Task.CompletedTask);

        //       var service = BuildReleaseService(contentDbContext); //, publicationReleaseSeriesViewService.Object); // @MarkFix

        //        // Act
        //        await service.CompletePublishing(release.Id, actualPublishedDate);

        //        //// Assert // @MarkFix
        //        //VerifyAllMocks(publicationReleaseSeriesViewService);
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        var actualRelease = await contentDbContext
        //            .Releases
        //            .SingleAsync(r => r.Id == release.Id);

        //        // Expect the published date to have been updated with the actual published date
        //        Assert.Equal(actualPublishedDate, actualRelease.Published);

        //        var actualDataBlockParents = await contentDbContext
        //            .DataBlockParents
        //            .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
        //            .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
        //            .ToListAsync();

        //        Assert.Equal(2, actualDataBlockParents.Count);

        //        // Assert that the original DataBlockParents did not point to a LatestPublishedVersion.
        //        originalDataBlockParents.ForEach(parent => Assert.Null(parent.LatestPublishedVersionId));

        //        // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
        //        // reference the newly published DataBlockVersion.
        //        actualDataBlockParents.ForEach(parent =>
        //        {
        //            var originalParent = originalDataBlockParents.Single(p => p.Id == parent.Id);

        //            // The LatestPublishedVersion version is now the one that was previously the LatestDraftVersion.
        //            // Its "Published" date should have just been set as well.
        //            Assert.Equal(originalParent.LatestDraftVersionId, parent.LatestPublishedVersionId);
        //            Assert.Null(originalParent.LatestDraftVersion!.Published);
        //            parent.LatestPublishedVersion!.Published.AssertUtcNow();

        //            // The LatestDraftVersion is now set to null, until a Release amendment is created in the future.
        //            Assert.Null(parent.LatestDraftVersionId);
        //        });
        //    }
        //}

        //[Fact] // @MarkFix
        //public async Task CompletePublishing_AmendedRelease()
        //{
        //    // Arrange
        //    var previousRelease = new Release
        //    {
        //        Id = Guid.NewGuid(),
        //        Published = DateTime.UtcNow.AddDays(-1),
        //        PreviousVersionId = null,
        //        Version = 0
        //    };

        //    var release = new Release
        //    {
        //        Id = Guid.NewGuid(),
        //        PreviousVersionId = previousRelease.Id,
        //        Version = 1
        //    };

        //    //var releaseSeriesItem = new ReleaseSeriesItem // @MarkFix
        //    //{
        //    //    ReleaseId = release.Id,
        //    //    Order = 1
        //    //};

        //    //var publication = new Publication
        //    //{
        //    //    Releases = new() { previousRelease, release },
        //    //    ReleaseSeriesView = new() { releaseSeriesItem }
        //    //};

        //    // Generate Data Blocks for both the previous Release version and for the new Amendment.
        //    var originalDataBlockParents = _fixture
        //        .DefaultDataBlockParent()
        //        .WithLatestPublishedVersion(() => _fixture
        //            .DefaultDataBlockVersion()
        //            .WithRelease(previousRelease)
        //            .Generate())
        //        .WithLatestDraftVersion(() => _fixture
        //            .DefaultDataBlockVersion()
        //            .WithRelease(release)
        //            .Generate())
        //        .GenerateList(2);

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //await contentDbContext.Publications.AddAsync(publication); // @MarkFix
        //        await contentDbContext.Releases.AddRangeAsync(previousRelease, release);
        //        await contentDbContext.SaveChangesAsync();
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //       // var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict); // @MarkFix

        //       // publicationReleaseSeriesViewService.Setup(s => s.UpdateForPublishRelease(
        //       //     It.IsAny<Guid>(),
        //       //     It.IsAny<Guid>()))
        //       // .Returns(Task.CompletedTask);

        //       var service = BuildReleaseService(contentDbContext); // , publicationReleaseSeriesViewService.Object); // @MarkFix

        //        // Act
        //        await service.CompletePublishing(release.Id, DateTime.UtcNow);

        //        //// Assert // @MarkFix
        //        //VerifyAllMocks(publicationReleaseSeriesViewService);
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        var actual = await contentDbContext
        //            .Releases
        //            .SingleAsync(r => r.Id == release.Id);

        //        // Expect the published date to have been copied from the previous version
        //        Assert.Equal(previousRelease.Published, actual.Published);

        //        var actualDataBlockParents = await contentDbContext
        //            .DataBlockParents
        //            .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
        //            .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
        //            .ToListAsync();

        //        Assert.Equal(2, actualDataBlockParents.Count);

        //        // Assert that the original DataBlockParents pointed to a LatestPublishedVersion and LatestDraftVersion.
        //        originalDataBlockParents.ForEach(parent => Assert.NotNull(parent.LatestDraftVersionId));
        //        originalDataBlockParents.ForEach(parent => Assert.NotNull(parent.LatestPublishedVersionId));

        //        // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
        //        // reference the newly published DataBlockVersion.
        //        actualDataBlockParents.ForEach(parent =>
        //        {
        //            var originalParent = originalDataBlockParents.Single(p => p.Id == parent.Id);

        //            // The LatestPublishedVersion is now the one that was previously the LatestDraftVersion.
        //            // Its "Published" date should have just been set as well, and we'll double-check that it didn't
        //            // just inherit that date from the original LatestDraftVersion DataBlockVersion, as it should not
        //            // have been set until it was published.
        //            Assert.Equal(originalParent.LatestDraftVersionId, parent.LatestPublishedVersionId);
        //            Assert.Null(originalParent.LatestDraftVersion!.Published);
        //            parent.LatestPublishedVersion!.Published.AssertUtcNow();

        //            // The LatestDraftVersion is now set to null, until a Release amendment is created in the future.
        //            Assert.Null(parent.LatestDraftVersionId);
        //        });
        //    }
        //}

        //[Fact] // @MarkFix
        //public async Task CompletePublishing_AmendedRelease_DataBlockRemoved()
        //{
        //    // Arrange
        //    var previousRelease = new Release
        //    {
        //        Id = Guid.NewGuid(),
        //        Published = DateTime.UtcNow.AddDays(-1),
        //        PreviousVersionId = null,
        //        Version = 0
        //    };

        //    var release = new Release
        //    {
        //        PreviousVersionId = previousRelease.Id,
        //        Version = 1
        //    };

        //    //var releaseSeriesItem = new ReleaseSeriesItem // @MarkFix
        //    //{
        //    //    ReleaseId = release.Id,
        //    //    Order = 1
        //    //};

        //    //var publication = new Publication
        //    //{
        //    //    Releases = new() { previousRelease, release },
        //    //    ReleaseSeriesView = new() { releaseSeriesItem }
        //    //};

        //    // Generate Data Blocks for both the previous Release version and for the new Amendment.
        //    _fixture
        //        .DefaultDataBlockParent()
        //        .WithLatestPublishedVersion(() => _fixture
        //            .DefaultDataBlockVersion()
        //            .WithRelease(previousRelease)
        //            .Generate())
        //        // This time Data Blocks have been removed from the latest Release amendment, and so they now have no
        //        // "latest" version.
        //        .WithLatestDraftVersion((DataBlockVersion)null!)
        //        .GenerateList(2);

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //await contentDbContext.Publications.AddAsync(publication); // @MarkFix
        //        await contentDbContext.Releases.AddRangeAsync(previousRelease, release);
        //        await contentDbContext.SaveChangesAsync();
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict); // @MarkFix

        //        //publicationReleaseSeriesViewService.Setup(s => s.UpdateForPublishRelease(
        //        //    It.IsAny<Guid>(),
        //        //    It.IsAny<Guid>()))
        //        //.Returns(Task.CompletedTask);

        //        var service = BuildReleaseService(contentDbContext); //, publicationReleaseSeriesViewService.Object); // @MarkFix

        //        // Act
        //        await service.CompletePublishing(release.Id, DateTime.UtcNow);

        //        //// Assert // @MarkFix
        //        //VerifyAllMocks(publicationReleaseSeriesViewService);
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        var actual = await contentDbContext
        //            .Releases
        //            .SingleAsync(r => r.Id == release.Id);

        //        // Expect the published date to have been copied from the previous version
        //        Assert.Equal(previousRelease.Published, actual.Published);

        //        var actualDataBlockParents = await contentDbContext
        //            .DataBlockParents
        //            .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
        //            .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
        //            .ToListAsync();

        //        // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
        //        // be null so that this Data Block is effectively no longer publicly visible. Their LatestDraftVersions
        //        // are also null for this DataBlockParent as this Data Block no longer has a Draft version being worked
        //        // on as a part of this Release / amendment.
        //        actualDataBlockParents.ForEach(parent =>
        //        {
        //            Assert.Null(parent.LatestPublishedVersionId);
        //            Assert.Null(parent.LatestDraftVersionId);
        //        });
        //    }
        //}

        //[Fact] // @MarkFix
        //public async Task CompletePublishing_AmendedReleaseAndUpdatePublishedDateIsTrue()
        //{
        //    // Arrange
        //    var previousRelease = new Release
        //    {
        //        Id = Guid.NewGuid(),
        //        Published = DateTime.UtcNow.AddDays(-1),
        //        PreviousVersionId = null,
        //        Version = 0
        //    };

        //    var release = new Release
        //    {
        //        Published = null,
        //        PreviousVersionId = previousRelease.Id,
        //        Version = 1,
        //        UpdatePublishedDate = true
        //    };

        //    //var releaseSeriesItem = new ReleaseSeriesItem // @MarkFix
        //    //{
        //    //    ReleaseId = release.Id,
        //    //    Order = 1
        //    //};

        //    //var publication = new Publication
        //    //{
        //    //    Releases = new() { previousRelease, release },
        //    //    ReleaseSeriesView = new() { releaseSeriesItem }
        //    //};

        //    var actualPublishedDate = DateTime.UtcNow;

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //await contentDbContext.Publications.AddAsync(publication); // @MarKFix
        //        await contentDbContext.Releases.AddRangeAsync(previousRelease, release);
        //        await contentDbContext.SaveChangesAsync();
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        //var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict); // @MarkFix

        //        //publicationReleaseSeriesViewService.Setup(s => s.UpdateForPublishRelease(
        //        //    It.IsAny<Guid>(),
        //        //    It.IsAny<Guid>()))
        //        //.Returns(Task.CompletedTask);

        //        var service = BuildReleaseService(contentDbContext); //, publicationReleaseSeriesViewService.Object); // @MarkFix

        //        // Act
        //        await service.CompletePublishing(release.Id, actualPublishedDate);

        //        //// Assert // @MarkFix
        //        //VerifyAllMocks(publicationReleaseSeriesViewService);
        //    }

        //    await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        //    {
        //        var actual = await contentDbContext
        //            .Releases
        //            .SingleAsync(r => r.Id == release.Id);

        //        // Expect the published date to have been updated with the actual published date
        //        Assert.Equal(actualPublishedDate, actual.Published);
        //    }
        //}

        private static Publisher.Services.ReleaseService BuildReleaseService(
            ContentDbContext? contentDbContext = null,
            IPublicationReleaseSeriesViewService? publicationReleaseSeriesViewService = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                releaseRepository: new Content.Model.Repository.ReleaseRepository(contentDbContext),
                publicationReleaseSeriesViewService ?? Mock.Of<IPublicationReleaseSeriesViewService>(Strict)
            );
        }
    }
}
